﻿using EntityStates;
using HunkMod.Modules.Components;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HunkMod.SkillStates.Hunk.Counter
{
    public class Lunge : BaseHunkSkillState
    {
        protected Vector3 slipVector = Vector3.zero;
        public float duration = 0.8f;
		public float checkRadius = 3f;

        private bool peepee;
        private float coeff = 24f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.slipVector = this.inputBank.aimDirection;
            this.slipVector.y = 0f;
            this.slipVector = this.slipVector.normalized;

            base.PlayCrossfade("FullBody, Override", "CounterLunge", "Dodge.playbackRate", this.duration, 0.05f);

            Util.PlaySound("sfx_hunk_step_foley", this.gameObject);

            EntityStateMachine.FindByCustomName(this.gameObject, "Aim").SetNextStateToMain();
            this.skillLocator.secondary.stock = 0;
            this.skillLocator.secondary.rechargeStopwatch = 0f;

            this.hunk.immobilized = true;

            this.ApplyBuff();
            this.CreateDashEffect();
        }

        public virtual void ApplyBuff()
        {
            if (NetworkServer.active) this.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f);
        }

        public virtual void CreateDashEffect()
        {
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.characterBody.aimTimer = -1f;
            this.hunk.reloadTimer = 1f;
            this.inputBank.moveVector = Vector3.zero;
            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.rootMotion = this.slipVector * (this.coeff * Time.fixedDeltaTime) * Mathf.Cos(base.fixedAge / this.duration * 1.57079637f);

            if (base.isAuthority)
            {
                if (base.characterDirection)
                {
                    base.characterDirection.forward = this.slipVector;
                }
            }

            this.CheckForCounterattack();

            if (!this.peepee && base.fixedAge >= (0.52f * this.duration))
            {
                this.peepee = true;
                this.coeff = 4f;
            }

            if (base.isAuthority && this.peepee)
            {
                if (this.inputBank.moveVector != Vector3.zero)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            if (base.isAuthority)
            {
                if (base.fixedAge >= this.duration)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }

                if (base.fixedAge >= (0.5f * this.duration))
                {
                    if (this.checkRadius < 5f)
                    {
                        this.checkRadius = 5f;
                        if (this.CheckForCounterattack()) return;
                    }

                    if (this.inputBank.skill1.down)
                    {
                        this.outer.SetNextState(new KnifeCounter());
                    }
                }
            }
        }

        private bool CheckForCounterattack()
        {
			Ray aimRay = base.GetAimRay();

			BullseyeSearch2 bullseyeSearch = new BullseyeSearch2
			{
				teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
				filterByLoS = false,
				searchOrigin = aimRay.origin,
				searchDirection = UnityEngine.Random.onUnitSphere,
				sortMode = BullseyeSearch2.SortMode.Distance,
				maxDistanceFilter = this.checkRadius,
				maxAngleFilter = 360f,
                onlyBullseyes = false
			};
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(base.gameObject);

			List<HurtBox> list = bullseyeSearch.GetResults().ToList<HurtBox>();
			foreach (HurtBox hurtBox in list)
			{
				if (hurtBox)
				{
					if (hurtBox.healthComponent && hurtBox.healthComponent.body)
					{
                        if (hurtBox.healthComponent.gameObject.name == "LemurianBody(Clone)")
                        {
                            if (base.isAuthority)
                            {
                                this.outer.SetNextState(new NeckSnap
                                {
                                    targetObject = hurtBox.healthComponent.gameObject
                                });
                            }

                            return true;
                        }

                        if (hurtBox.healthComponent.body.hullClassification == HullClassification.BeetleQueen || hurtBox.healthComponent.body.hullClassification == HullClassification.Golem)
                        {
                            if (base.isAuthority) this.outer.SetNextState(new Punch());
                            return true;
                        }

                        if (base.isAuthority) this.outer.SetNextState(new Kick());
                        return true;
					}
				}
			}

            return false;
		}

        public override void OnExit()
        {
            this.hunk.immobilized = false;

            base.OnExit();

            if (base.isAuthority && this.inputBank.moveVector != Vector3.zero) this.characterBody.isSprinting = true;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (this.peepee) return InterruptPriority.Any;
            return InterruptPriority.Frozen;
        }
    }
}