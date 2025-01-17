﻿using RoR2;
using UnityEngine;
using EntityStates;
using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;

namespace HunkMod.SkillStates.Hunk.Weapon.Slugger
{
    public class Shoot : BaseHunkSkillState
    {
        public static float damageCoefficient = 18f;
        public static float procCoefficient = 1f;
        public float baseDuration = 1.2f;
        public static float bulletRecoil = 8f;
        public static float bulletRange = 128f;
        public static float bulletThiccness = 1f;
        public float selfForce = 1200f;

        private float earlyExitTime;
        protected float duration;
        protected float fireDuration;
        protected bool hasFired;
        private bool isCrit;
        protected string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            this.muzzleString = "MuzzleSlugger";
            this.hasFired = false;
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.isCrit = base.RollCrit();
            this.earlyExitTime = 1f * this.duration;

            if (this.isCrit) Util.PlaySound("sfx_hunk_slugger_shoot_critical", base.gameObject);
            else Util.PlaySound("sfx_hunk_slugger_shoot", base.gameObject);

            this.PlayAnimation("Gesture, Override", "ShootSlugger", "Shoot.playbackRate", this.duration * 1.2f);

            this.fireDuration = 0;

            this.hunk.ConsumeAmmo();
        }

        public virtual void FireBullet()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                float recoilAmplitude = Shoot.bulletRecoil / this.attackSpeedStat;

                base.AddRecoil2(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
                this.characterBody.AddSpreadBloom(4f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FireBarrage.effectPrefab, this.gameObject, this.muzzleString, false);

                GameObject tracer = Modules.Assets.shotgunTracer;
                if (this.isCrit) tracer = Modules.Assets.shotgunTracerCrit;

                if (base.isAuthority)
                {
                    float damage = Shoot.damageCoefficient * this.damageStat;

                    Ray aimRay = base.GetAimRay2();

                    float force = 1500;

                    BulletAttack bulletAttack = new BulletAttack
                    {
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damage,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = bulletRange,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        isCrit = this.isCrit,
                        owner = this.gameObject,
                        muzzleName = this.muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = procCoefficient,
                        radius = Shoot.bulletThiccness,
                        sniper = false,
                        stopperMask = LayerIndex.world.collisionMask,
                        weapon = null,
                        tracerEffectPrefab = tracer,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FireBarrage.hitEffectPrefab,
                        HitEffectNormal = false,
                    };

                    bulletAttack.minSpread = 0;
                    bulletAttack.maxSpread = 0;
                    bulletAttack.bulletCount = 1;

                    bulletAttack.modifyOutgoingDamageCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
                    {
                        if (BulletAttack.IsSniperTargetHit(hitInfo))
                        {
                            damageInfo.damage *= 1.5f;
                            damageInfo.damageColorIndex = DamageColorIndex.Sniper;
                            EffectData effectData = new EffectData
                            {
                                origin = hitInfo.point,
                                rotation = Quaternion.LookRotation(-hitInfo.direction)
                            };

                            effectData.SetHurtBoxReference(hitInfo.hitHurtBox);
                            //EffectManager.SpawnEffect(Modules.Assets.headshotEffect, effectData, true);
                            Util.PlaySound("sfx_hunk_headshot", hitInfo.hitHurtBox.gameObject);

                            NetworkIdentity identity = this.GetComponent<NetworkIdentity>();
                            if (identity) new Modules.Components.SyncHeadshot(identity.netId, hitInfo.hitHurtBox.healthComponent.gameObject).Send(NetworkDestination.Server);

                            hitInfo.hitHurtBox.healthComponent.gameObject.AddComponent<Modules.Components.HunkHeadshotTracker>();
                        }
                    };

                    bulletAttack.Fire();

                    float _selfForce = this.selfForce;
                    if (!this.isGrounded) _selfForce *= 1.5f;
                    this.characterMotor.ApplyForce(aimRay.direction * -_selfForce);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireDuration)
            {
                this.FireBullet();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge >= this.earlyExitTime) return InterruptPriority.Any;
            return InterruptPriority.Skill;
        }
    }
}