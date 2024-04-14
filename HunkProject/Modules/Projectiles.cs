﻿using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

namespace HunkMod.Modules
{
    internal static class Projectiles
    {
        public static GameObject rocketProjectilePrefab;
        public static GameObject missileProjectilePrefab;

        internal static void RegisterProjectiles()
        {
            //rocketProjectilePrefab = CreateRocket(false, "DriverRocketProjectile", "DriverRocketGhost", "DriverBigRocketGhost");
            //missileProjectilePrefab = CreateRocket(false, "DriverMissileProjectile", "DriverMissileGhost", "DriverMissileGhost");
        }

        private static GameObject CreateRocket(bool gravity, string projectileName, string ghostName = "", string ghostToLoad = "")
        {
            GameObject projectilePrefab = CloneProjectilePrefab("CommandoGrenadeProjectile", projectileName);
            projectilePrefab.AddComponent<Modules.Components.RocketRotation>();
            projectilePrefab.transform.localScale *= 2f;

            ProjectileImpactExplosion impactExplosion = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
            InitializeImpactExplosion(impactExplosion);

            GameObject fuckMyLife = Modules.Assets.explosionEffect;
            fuckMyLife.AddComponent<NetworkIdentity>();

            impactExplosion.blastRadius = 10f;
            impactExplosion.destroyOnEnemy = true;
            impactExplosion.lifetime = 12f;
            impactExplosion.impactEffect = fuckMyLife;
            //impactExplosion.lifetimeExpiredSound = Modules.Assets.CreateNetworkSoundEventDef("sfx_driver_explosion");
            impactExplosion.timerAfterImpact = true;
            impactExplosion.lifetimeAfterImpact = 0f;

            ProjectileController rocketController = projectilePrefab.GetComponent<ProjectileController>();

            if (ghostName != "")
            {
                GameObject ghost = CreateGhostPrefab(ghostToLoad);
                ghost.name = ghostName;
                ghost.transform.GetChild(0).Find("Smoke").gameObject.AddComponent<Modules.Components.DetachOnDestroy>();
                ghost.transform.GetChild(0).Find("Smoke").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matDustDirectional.mat").WaitForCompletion();
                ghost.transform.GetChild(0).Find("Flame").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Golem/matVFXFlame1.mat").WaitForCompletion();

                rocketController.ghostPrefab = ghost;
                rocketController.startSound = "";
            }

            projectilePrefab.GetComponent<Rigidbody>().useGravity = gravity;

            Prefabs.projectilePrefabs.Add(projectilePrefab);

            return projectilePrefab;
        }

        private static void InitializeImpactExplosion(ProjectileImpactExplosion projectileImpactExplosion)
        {
            projectileImpactExplosion.blastDamageCoefficient = 1f;
            projectileImpactExplosion.blastProcCoefficient = 1f;
            projectileImpactExplosion.blastRadius = 1f;
            projectileImpactExplosion.bonusBlastForce = Vector3.zero;
            projectileImpactExplosion.childrenCount = 0;
            projectileImpactExplosion.childrenDamageCoefficient = 0f;
            projectileImpactExplosion.childrenProjectilePrefab = null;
            projectileImpactExplosion.destroyOnEnemy = false;
            projectileImpactExplosion.destroyOnWorld = false;
            projectileImpactExplosion.explosionSoundString = "";
            projectileImpactExplosion.falloffModel = RoR2.BlastAttack.FalloffModel.None;
            projectileImpactExplosion.fireChildren = false;
            projectileImpactExplosion.impactEffect = null;
            projectileImpactExplosion.lifetime = 0f;
            projectileImpactExplosion.lifetimeAfterImpact = 0f;
            projectileImpactExplosion.lifetimeExpiredSoundString = "";
            projectileImpactExplosion.lifetimeRandomOffset = 0f;
            projectileImpactExplosion.offsetForLifetimeExpiredSound = 0f;
            projectileImpactExplosion.timerAfterImpact = false;

            projectileImpactExplosion.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
        }

        private static GameObject CreateGhostPrefab(string ghostName)
        {
            GameObject ghostPrefab = Modules.Assets.mainAssetBundle.LoadAsset<GameObject>(ghostName).InstantiateClone(ghostName);
            ghostPrefab.AddComponent<NetworkIdentity>();
            ghostPrefab.AddComponent<ProjectileGhostController>();

            Modules.Assets.ConvertAllRenderersToHopooShader(ghostPrefab);

            return ghostPrefab;
        }

        private static GameObject CloneProjectilePrefab(string prefabName, string newPrefabName)
        {
            GameObject newPrefab = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/" + prefabName), newPrefabName);
            return newPrefab;
        }
    }
}