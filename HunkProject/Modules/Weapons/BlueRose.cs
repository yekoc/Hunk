﻿using RoR2.Skills;
using UnityEngine;

namespace HunkMod.Modules.Weapons
{
    public class BlueRose : BaseWeapon<BlueRose>
    {
        public override string weaponNameToken => "BLUEROSE";
        public override string weaponName => "Blue Rose";
        public override string weaponDesc => "A six-chamber revolver that fires two shots at a time? What kinda whackjob thinks up something like that?";
        public override string iconName => "texBlueRoseIcon";
        public override GameObject crosshairPrefab => Modules.Assets.magnumCrosshairPrefab;
        public override int magSize => 6;
        public override float magPickupMultiplier => 0.5f;
        public override float reloadDuration => 0.9f;
        public override string ammoName => "Blue Rose Ammo";
        public override GameObject modelPrefab => Modules.Assets.mainAssetBundle.LoadAsset<GameObject>("mdlBlueRose");
        public override HunkWeaponDef.AnimationSet animationSet => HunkWeaponDef.AnimationSet.Pistol;
        public override bool storedOnBack => false;
        public override float damageFillValue => 1f;
        public override float rangefillValue => 0.8f;
        public override float fireRateFillValue => 0.85f;
        public override float reloadFillValue => 0.1f;
        public override float accuracyFillValue => 0.8f;

        public override SkillDef primarySkillDef => Modules.Skills.CreatePrimarySkillDef(
new EntityStates.SerializableEntityStateType(typeof(SkillStates.Hunk.Weapon.Revolver.Shoot)),
"Weapon",
"ROB_HUNK_BODY_SHOOT_REVOLVER_NAME",
"ROB_HUNK_BODY_SHOOT_REVOLVER_DESCRIPTION",
Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("texShootIcon"),
false);

        public override void Init()
        {
            base.Init();
            this.weaponDef.roundReload = true;
        }
    }
}