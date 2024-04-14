﻿using System.Reflection;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using System.IO;
using RoR2.Audio;
using System.Collections.Generic;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using TMPro;
using RoR2.UI;
using UnityEngine.UI;
using HunkMod.Modules.Components;
using UnityEngine.Rendering.PostProcessing;
using Moonstorm.Starstorm2.Survivors;

namespace HunkMod.Modules
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle;

        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");
        internal static Material commandoMat;

        internal static List<EffectDef> effectDefs = new List<EffectDef>();
        internal static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();

        internal static NetworkSoundEventDef knifeImpactSoundDef;

        public static GameObject headshotEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Common/VFX/WeakPointProcEffect.prefab").WaitForCompletion();

        public static GameObject explosionEffect;
        public static GameObject smallExplosionEffect;

        public static GameObject pistolCrosshairPrefab;
        public static GameObject magnumCrosshairPrefab;
        public static GameObject smgCrosshairPrefab;
        public static GameObject rocketLauncherCrosshairPrefab;
        public static GameObject grenadeLauncherCrosshairPrefab;
        public static GameObject needlerCrosshairPrefab;
        public static GameObject shotgunCrosshairPrefab;
        public static GameObject circleCrosshairPrefab;

        public static GameObject weaponNotificationPrefab;
        public static GameObject headshotOverlay;
        public static GameObject headshotVisualizer;

        public static GameObject ammoPickupModel;
        public static GameObject bloodExplosionEffect;
        public static GameObject bloodSpurtEffect;

        public static GameObject shotgunShell;
        public static GameObject shotgunSlug;

        public static GameObject weaponPickup;

        public static GameObject weaponPickupEffect;

        internal static GameObject knifeImpactEffect;
        internal static GameObject knifeSwingEffect;

        public static GameObject shotgunTracer;
        public static GameObject shotgunTracerCrit;

        internal static Material woundOverlayMat;

        internal static void PopulateAssets()
        {
            if (mainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HunkMod.robhunk"))
                {
                    mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }

            /*using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("HunkMod.hunk_bank.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }*/
            woundOverlayMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/ArmorReductionOnHit/matPulverizedOverlay.mat").WaitForCompletion());
            woundOverlayMat.SetColor("_TintColor", Color.red);

            knifeImpactSoundDef = CreateNetworkSoundEventDef("sfx_driver_knife_hit");

            headshotOverlay = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerScopeLightOverlay.prefab").WaitForCompletion().InstantiateClone("DriverHeadshotOverlay", false);
            SniperTargetViewer viewer = headshotOverlay.GetComponentInChildren<SniperTargetViewer>();
            headshotOverlay.transform.Find("ScopeOverlay").gameObject.SetActive(false);

            headshotVisualizer = viewer.visualizerPrefab.InstantiateClone("DriverHeadshotVisualizer", false);
            Image headshotImage = headshotVisualizer.transform.Find("Scaler/Rectangle").GetComponent<Image>();
            headshotVisualizer.transform.Find("Scaler/Outer").gameObject.SetActive(false);
            headshotImage.color = Color.red;
            //headshotImage.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Captain/texCaptainCrosshairInner.png").WaitForCompletion();

            viewer.visualizerPrefab = headshotVisualizer;
            bool dynamicCrosshair = Modules.Config.dynamicCrosshair.Value;

            #region Pistol Crosshair
            pistolCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion().InstantiateClone("HunkPistolCrosshair", false);
            pistolCrosshairPrefab.GetComponent<RawImage>().enabled = false;
            if (dynamicCrosshair) pistolCrosshairPrefab.AddComponent<DynamicCrosshair>();
            #endregion

            #region Magnum Crosshair
            magnumCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion().InstantiateClone("HunkMagnumCrosshair", false);
            magnumCrosshairPrefab.GetComponent<RawImage>().enabled = false;
            if (dynamicCrosshair) magnumCrosshairPrefab.AddComponent<DynamicCrosshair>();
            magnumCrosshairPrefab.AddComponent<CrosshairStartRotate>();
            #endregion

            #region SMG Crosshair
            smgCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion().InstantiateClone("HunkSMGCrosshair", false);
            smgCrosshairPrefab.GetComponent<RawImage>().enabled = false;
            if (dynamicCrosshair) smgCrosshairPrefab.AddComponent<DynamicCrosshair>();
            smgCrosshairPrefab.transform.GetChild(2).gameObject.SetActive(false);
            #endregion

            #region Grenade Launcher Crosshair
            grenadeLauncherCrosshairPrefab = PrefabAPI.InstantiateClone(LoadCrosshair("ToolbotGrenadeLauncher"), "HunkGrenadeLauncherCrosshair", false);
            if (dynamicCrosshair) grenadeLauncherCrosshairPrefab.AddComponent<DynamicCrosshair>();
            CrosshairController crosshair = grenadeLauncherCrosshairPrefab.GetComponent<CrosshairController>();
            crosshair.skillStockSpriteDisplays = new CrosshairController.SkillStockSpriteDisplay[0];

            grenadeLauncherCrosshairPrefab.transform.GetChild(0).GetComponentInChildren<Image>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/Railgunner/texCrosshairRailgunSniperNib.png").WaitForCompletion();
            RectTransform rect = grenadeLauncherCrosshairPrefab.transform.GetChild(0).GetComponent<RectTransform>();
            rect.localEulerAngles = Vector3.zero;
            rect.anchoredPosition = new Vector2(-50f, -10f);

            grenadeLauncherCrosshairPrefab.transform.GetChild(1).GetComponentInChildren<Image>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/Railgunner/texCrosshairRailgunSniperNib.png").WaitForCompletion();
            rect = grenadeLauncherCrosshairPrefab.transform.GetChild(1).GetComponent<RectTransform>();
            rect.localEulerAngles = new Vector3(0f, 0f, 90f);

            grenadeLauncherCrosshairPrefab.transform.GetChild(2).GetComponentInChildren<Image>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/Railgunner/texCrosshairRailgunSniperNib.png").WaitForCompletion();
            rect = grenadeLauncherCrosshairPrefab.transform.GetChild(2).GetComponent<RectTransform>();
            rect.localEulerAngles = Vector3.zero;
            rect.anchoredPosition = new Vector2(50f, -10f);

            grenadeLauncherCrosshairPrefab.transform.Find("StockCountHolder").gameObject.SetActive(false);
            grenadeLauncherCrosshairPrefab.transform.Find("Image, Arrow (1)").gameObject.SetActive(true);

            crosshair.spriteSpreadPositions[0].zeroPosition = new Vector3(25f, 25f, 0f);
            crosshair.spriteSpreadPositions[0].onePosition = new Vector3(-25f, 25f, 0f);

            crosshair.spriteSpreadPositions[1].zeroPosition = new Vector3(75f, 0f, 0f);
            crosshair.spriteSpreadPositions[1].onePosition = new Vector3(125f, 0f, 0f);

            crosshair.spriteSpreadPositions[2].zeroPosition = new Vector3(-25f, 25f, 0f);
            crosshair.spriteSpreadPositions[2].onePosition = new Vector3(25f, 25f, 0f);
            #endregion

            #region Rocket Launcher Crosshair
            rocketLauncherCrosshairPrefab = PrefabAPI.InstantiateClone(LoadCrosshair("ToolbotGrenadeLauncher"), "HunkRocketLauncherCrosshair", false);
            if (dynamicCrosshair) rocketLauncherCrosshairPrefab.AddComponent<DynamicCrosshair>();
            crosshair = rocketLauncherCrosshairPrefab.GetComponent<CrosshairController>();
            crosshair.skillStockSpriteDisplays = new CrosshairController.SkillStockSpriteDisplay[0];
            rocketLauncherCrosshairPrefab.transform.Find("StockCountHolder").gameObject.SetActive(false);
            #endregion

            #region Needler Crosshair
            needlerCrosshairPrefab = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/LoaderCrosshair"), "HunkNeedlerCrosshair", false);
            MainPlugin.Destroy(needlerCrosshairPrefab.GetComponent<LoaderHookCrosshairController>());
            if (dynamicCrosshair) needlerCrosshairPrefab.AddComponent<DynamicCrosshair>();

            needlerCrosshairPrefab.GetComponent<RawImage>().enabled = false;

            var control = needlerCrosshairPrefab.GetComponent<CrosshairController>();

            control.maxSpreadAlpha = 0;
            control.maxSpreadAngle = 3;
            control.minSpreadAlpha = 0;
            control.spriteSpreadPositions = new CrosshairController.SpritePosition[]
            {
                new CrosshairController.SpritePosition
                {
                    target = needlerCrosshairPrefab.transform.GetChild(2).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(-20f, 0, 0),
                    onePosition = new Vector3(-48f, 0, 0)
                },
                new CrosshairController.SpritePosition
                {
                    target = needlerCrosshairPrefab.transform.GetChild(3).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(20f, 0, 0),
                    onePosition = new Vector3(48f, 0, 0)
                }
            };

            MainPlugin.Destroy(needlerCrosshairPrefab.transform.GetChild(0).gameObject);
            MainPlugin.Destroy(needlerCrosshairPrefab.transform.GetChild(1).gameObject);
            #endregion

            #region Shotgun Crosshair
            shotgunCrosshairPrefab = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/LoaderCrosshair"), "HunkShotgunCrosshair", false);
            MainPlugin.Destroy(shotgunCrosshairPrefab.GetComponent<LoaderHookCrosshairController>());
            if (dynamicCrosshair) shotgunCrosshairPrefab.AddComponent<DynamicCrosshair>();

            shotgunCrosshairPrefab.GetComponent<RawImage>().enabled = false;

            control = shotgunCrosshairPrefab.GetComponent<CrosshairController>();

            control.maxSpreadAlpha = 0;
            control.maxSpreadAngle = 3;
            control.minSpreadAlpha = 0;
            control.spriteSpreadPositions = new CrosshairController.SpritePosition[]
            {
                new CrosshairController.SpritePosition
                {
                    target = shotgunCrosshairPrefab.transform.GetChild(2).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(-32f, 0, 0),
                    onePosition = new Vector3(-75f, 0, 0)
                },
                new CrosshairController.SpritePosition
                {
                    target = shotgunCrosshairPrefab.transform.GetChild(3).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(32f, 0, 0),
                    onePosition = new Vector3(75f, 0, 0)
                }
            };

            control.transform.Find("Bracket (2)").GetComponent<RectTransform>().localScale = new Vector3(1.25f, 1.75f, 1f);
            control.transform.Find("Bracket (3)").GetComponent<RectTransform>().localScale = new Vector3(1.25f, 1.75f, 1f);

            MainPlugin.Destroy(shotgunCrosshairPrefab.transform.GetChild(0).gameObject);
            MainPlugin.Destroy(shotgunCrosshairPrefab.transform.GetChild(1).gameObject);
            #endregion

            circleCrosshairPrefab = CreateCrosshair();

            shotgunShell = mainAssetBundle.LoadAsset<GameObject>("ShotgunShell");
            shotgunShell.GetComponentInChildren<MeshRenderer>().material = CreateMaterial("matShotgunShell");
            shotgunShell.AddComponent<Modules.Components.ShellController>();

            shotgunSlug = mainAssetBundle.LoadAsset<GameObject>("ShotgunSlug");
            shotgunSlug.GetComponentInChildren<MeshRenderer>().material = CreateMaterial("matShotgunSlug");
            shotgunSlug.AddComponent<Modules.Components.ShellController>();

            weaponPickupEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandolier/AmmoPack.prefab").WaitForCompletion().GetComponentInChildren<AmmoPickup>().pickupEffect.InstantiateClone("RobHunkWeaponPickupEffect", true);
            weaponPickupEffect.AddComponent<NetworkIdentity>();
            AddNewEffectDef(weaponPickupEffect, "sfx_driver_pickup");

            weaponNotificationPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/NotificationPanel2.prefab").WaitForCompletion().InstantiateClone("HunkWeaponNotification", false);
            WeaponNotification _new = weaponNotificationPrefab.AddComponent<WeaponNotification>();
            GenericNotification _old = weaponNotificationPrefab.GetComponent<GenericNotification>();

            _new.titleText = _old.titleText;
            _new.titleTMP = _old.titleTMP;
            _new.descriptionText = _old.descriptionText;
            _new.iconImage = _old.iconImage;
            _new.previousIconImage = _old.previousIconImage;
            _new.canvasGroup = _old.canvasGroup;
            _new.fadeOutT = _old.fadeOutT;

            _old.enabled = false;


            explosionEffect = LoadEffect("BigExplosion", "sfx_driver_explosion_badass", false);
            explosionEffect.transform.Find("Shockwave").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matDistortion.mat").WaitForCompletion();
            ShakeEmitter shake = explosionEffect.AddComponent<ShakeEmitter>();
            ShakeEmitter shake2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BFG/BeamSphereExplosion.prefab").WaitForCompletion().GetComponent<ShakeEmitter>();
            shake.shakeOnStart = true;
            shake.shakeOnEnable = false;
            shake.wave = shake2.wave;
            shake.duration = 0.5f;
            shake.radius = 200f;
            shake.scaleShakeRadiusWithLocalScale = false;
            shake.amplitudeTimeDecay = true;

            smallExplosionEffect = LoadEffect("SmallExplosion", "sfx_driver_grenade_explosion_badass", false);
            smallExplosionEffect.transform.Find("Shockwave").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matDistortion.mat").WaitForCompletion();
            shake = smallExplosionEffect.AddComponent<ShakeEmitter>();
            shake.shakeOnStart = true;
            shake.shakeOnEnable = false;
            shake.wave = shake2.wave;
            shake.duration = 0.5f;
            shake.radius = 60f;
            shake.scaleShakeRadiusWithLocalScale = false;
            shake.amplitudeTimeDecay = true;

            shotgunTracer = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoShotgun").InstantiateClone("DriverShotgunTracer", true);

            if (!shotgunTracer.GetComponent<EffectComponent>()) shotgunTracer.AddComponent<EffectComponent>();
            if (!shotgunTracer.GetComponent<VFXAttributes>()) shotgunTracer.AddComponent<VFXAttributes>();
            if (!shotgunTracer.GetComponent<NetworkIdentity>()) shotgunTracer.AddComponent<NetworkIdentity>();

            Material bulletMat = null;

            foreach (LineRenderer i in shotgunTracer.GetComponentsInChildren<LineRenderer>())
            {
                if (i)
                {
                    bulletMat = UnityEngine.Object.Instantiate<Material>(i.material);
                    bulletMat.SetColor("_TintColor", new Color(0.68f, 0.58f, 0.05f));
                    i.material = bulletMat;
                    i.startColor = new Color(0.68f, 0.58f, 0.05f);
                    i.endColor = new Color(0.68f, 0.58f, 0.05f);
                }
            }

            shotgunTracerCrit = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerCommandoShotgun").InstantiateClone("DriverShotgunTracerCritical", true);

            if (!shotgunTracerCrit.GetComponent<EffectComponent>()) shotgunTracerCrit.AddComponent<EffectComponent>();
            if (!shotgunTracerCrit.GetComponent<VFXAttributes>()) shotgunTracerCrit.AddComponent<VFXAttributes>();
            if (!shotgunTracerCrit.GetComponent<NetworkIdentity>()) shotgunTracerCrit.AddComponent<NetworkIdentity>();

            foreach (LineRenderer i in shotgunTracerCrit.GetComponentsInChildren<LineRenderer>())
            {
                if (i)
                {
                    Material material = UnityEngine.Object.Instantiate<Material>(i.material);
                    material.SetColor("_TintColor", Color.yellow);
                    i.material = material;
                    i.startColor = new Color(0.8f, 0.24f, 0f);
                    i.endColor = new Color(0.8f, 0.24f, 0f);
                }
            }

            AddNewEffectDef(shotgunTracer);
            AddNewEffectDef(shotgunTracerCrit);

            Modules.Config.InitROO(Assets.mainAssetBundle.LoadAsset<Sprite>("texDriverIcon"), "My extraction point.");

            knifeSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("HunkKnifeSwing", false);
            knifeSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressSwingTrail.mat").WaitForCompletion();

            knifeImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("HunkKnifeImpact", false);
            knifeImpactEffect.GetComponent<OmniEffect>().enabled = false;

            Material hitsparkMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat").WaitForCompletion());
            hitsparkMat.SetColor("_TintColor", Color.white);

            knifeImpactEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = hitsparkMat;

            knifeImpactEffect.transform.GetChild(2).localScale = Vector3.one * 1.5f;
            knifeImpactEffect.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matOmniRing2Huntress.mat").WaitForCompletion();

            Material slashMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniRadialSlash1Generic.mat").WaitForCompletion());

            knifeImpactEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = slashMat;

            knifeImpactEffect.transform.GetChild(6).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarWisp/matOmniHitspark1LunarWisp.mat").WaitForCompletion();
            knifeImpactEffect.transform.GetChild(6).gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniHitspark2Generic.mat").WaitForCompletion();

            knifeImpactEffect.transform.GetChild(1).localScale = Vector3.one * 1.5f;

            knifeImpactEffect.transform.GetChild(1).gameObject.SetActive(true);
            knifeImpactEffect.transform.GetChild(2).gameObject.SetActive(true);
            knifeImpactEffect.transform.GetChild(3).gameObject.SetActive(true);
            knifeImpactEffect.transform.GetChild(4).gameObject.SetActive(true);
            knifeImpactEffect.transform.GetChild(5).gameObject.SetActive(true);
            knifeImpactEffect.transform.GetChild(6).gameObject.SetActive(true);
            knifeImpactEffect.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);

            knifeImpactEffect.transform.GetChild(6).transform.localScale = new Vector3(1f, 1f, 3f);

            knifeImpactEffect.transform.localScale = Vector3.one * 1.5f;

            AddNewEffectDef(knifeImpactEffect);

            bloodExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("HunkBloodExplosion", false);

            Material bloodMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion();
            Material bloodMat2 = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion();

            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            bloodExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion();
            bloodExplosionEffect.GetComponentInChildren<Light>().gameObject.SetActive(false);

            bloodExplosionEffect.GetComponentInChildren<PostProcessVolume>().sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/ppLocalGold.asset").WaitForCompletion();

            AddNewEffectDef(bloodExplosionEffect);

            bloodSpurtEffect = mainAssetBundle.LoadAsset<GameObject>("BloodSpurtEffect");

            bloodSpurtEffect.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodSpurtEffect.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial = bloodMat2;

            ammoPickupModel = mainAssetBundle.LoadAsset<GameObject>("AmmoPickup");
            ConvertAllRenderersToHopooShader(ammoPickupModel);
        }

        private static GameObject CreateTracer(string originalTracerName, string newTracerName)
        {
            GameObject newTracer = R2API.PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/" + originalTracerName), newTracerName, true);

            if (!newTracer.GetComponent<EffectComponent>()) newTracer.AddComponent<EffectComponent>();
            if (!newTracer.GetComponent<VFXAttributes>()) newTracer.AddComponent<VFXAttributes>();
            if (!newTracer.GetComponent<NetworkIdentity>()) newTracer.AddComponent<NetworkIdentity>();

            newTracer.GetComponent<Tracer>().speed = 250f;
            newTracer.GetComponent<Tracer>().length = 50f;

            AddNewEffectDef(newTracer);

            return newTracer;
        }

        internal static GameObject CreatePickupObject(HunkWeaponDef weaponDef)
        {
            // nuclear solution...... i fucking hate modding
            GameObject newPickup = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandolier/AmmoPack.prefab").WaitForCompletion().InstantiateClone("HunkWeaponPickup" + weaponDef.index, true);

            AmmoPickup ammoPickupComponent = newPickup.GetComponentInChildren<AmmoPickup>();
            Components.WeaponPickup weaponPickupComponent = ammoPickupComponent.gameObject.AddComponent<Components.WeaponPickup>();

            weaponPickupComponent.baseObject = ammoPickupComponent.baseObject;
            weaponPickupComponent.pickupEffect = weaponPickupEffect;
            weaponPickupComponent.teamFilter = ammoPickupComponent.teamFilter;
            weaponPickupComponent.weaponDef = weaponDef;

            Material uncommonPickupMat = Material.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Bandolier/matPickups.mat").WaitForCompletion());
            uncommonPickupMat.SetColor("_TintColor", new Color(0f, 80f / 255f, 0f, 1f));

            newPickup.GetComponentInChildren<MeshRenderer>().enabled = false;

            float duration = 360f;

            GameObject pickupModel = GameObject.Instantiate(mainAssetBundle.LoadAsset<GameObject>("AmmoPickup"));

            newPickup.GetComponent<BeginRapidlyActivatingAndDeactivating>().delayBeforeBeginningBlinking = duration - 5f;
            newPickup.GetComponent<DestroyOnTimer>().duration = duration;

            pickupModel.transform.parent = newPickup.transform.Find("Visuals");
            pickupModel.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            pickupModel.transform.localRotation = Quaternion.identity;

            MeshRenderer pickupMesh = pickupModel.GetComponentInChildren<MeshRenderer>();
            //pickupMesh.material = null;

            GameObject textShit = GameObject.Instantiate(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc"));
            MonoBehaviour.Destroy(textShit.GetComponent<EffectComponent>());
            textShit.transform.parent = pickupModel.transform;
            textShit.transform.localPosition = Vector3.zero;
            textShit.transform.localRotation = Quaternion.identity;

            textShit.GetComponent<DestroyOnTimer>().enabled = false;

            ObjectScaleCurve whatTheFuckIsThis = textShit.GetComponentInChildren<ObjectScaleCurve>();
            Transform helpMe = whatTheFuckIsThis.transform;
            MonoBehaviour.DestroyImmediate(whatTheFuckIsThis);
            helpMe.transform.localScale = Vector3.one * 1.25f;

            MonoBehaviour.Destroy(ammoPickupComponent);
            MonoBehaviour.Destroy(newPickup.GetComponentInChildren<RoR2.GravitatePickup>());

            newPickup.transform.Find("Visuals").Find("Particle System").Find("Particle System").gameObject.SetActive(false);
            newPickup.GetComponentInChildren<Light>().color = Modules.Survivors.Hunk.characterColor;

            // i seriously hate this but it works
            return newPickup;
        }

        private static GameObject CreateCrosshair()
        {
            GameObject crosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2CrosshairPrepRevolver.prefab").WaitForCompletion().InstantiateClone("HunkCircleCrosshair", false);
            CrosshairController crosshair = crosshairPrefab.GetComponent<CrosshairController>();
            crosshair.skillStockSpriteDisplays = new CrosshairController.SkillStockSpriteDisplay[0];

            MainPlugin.DestroyImmediate(crosshairPrefab.transform.Find("Outer").GetComponent<ObjectScaleCurve>());
            crosshairPrefab.transform.Find("Outer").GetComponent<Image>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texCrosshairTridant.png").WaitForCompletion();
            RectTransform rectR = crosshairPrefab.transform.Find("Outer").GetComponent<RectTransform>();
            rectR.localScale = Vector3.one * 0.75f;

            GameObject nibL = GameObject.Instantiate(crosshair.transform.Find("Outer").gameObject);
            nibL.transform.parent = crosshairPrefab.transform;
            //nibL.GetComponent<Image>().sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/Railgunner/texCrosshairRailgunSniperCenter.png").WaitForCompletion();
            RectTransform rectL = nibL.GetComponent<RectTransform>();
            rectL.localEulerAngles = new Vector3(0f, 0f, 180f);

            crosshair.spriteSpreadPositions = new CrosshairController.SpritePosition[]
            {
                new CrosshairController.SpritePosition
                {
                    target = rectR,
                    zeroPosition = new Vector3(0f, 0f, 0f),
                    onePosition = new Vector3(10f, 10f, 0f)
                },
                new CrosshairController.SpritePosition
                {
                    target = rectL,
                    zeroPosition = new Vector3(0f, 0f, 0f),
                    onePosition = new Vector3(-10f, -10f, 0f)
                }
            };

            crosshairPrefab.AddComponent<Modules.Components.CrosshairRotator>();

            return crosshairPrefab;
        }

        internal static GameObject CreateTextPopupEffect(string prefabName, string token, Color color)
        {
            GameObject i = CreateTextPopupEffect(prefabName, token);

            i.GetComponentInChildren<TMP_Text>().color = color;

            return i;
        }

        internal static GameObject CreateTextPopupEffect(string prefabName, string token, string soundName = "")
        {
            GameObject i = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/BearProc").InstantiateClone(prefabName, true);

            i.GetComponent<EffectComponent>().soundName = soundName;
            if (!i.GetComponent<NetworkIdentity>()) i.AddComponent<NetworkIdentity>();

            i.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>().token = token;

            Assets.AddNewEffectDef(i);

            return i;
        }

        internal static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName)
        {
            NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
            networkSoundEventDef.eventName = eventName;

            networkSoundEventDefs.Add(networkSoundEventDef);

            return networkSoundEventDef;
        }

        internal static void ConvertAllRenderersToHopooShader(GameObject objectToConvert)
        {
            foreach (Renderer i in objectToConvert.GetComponentsInChildren<Renderer>())
            {
                if (i)
                {
                    if (i.material)
                    {
                        i.material.shader = hotpoo;
                    }
                }
            }
        }

        internal static CharacterModel.RendererInfo[] SetupRendererInfos(GameObject obj)
        {
            MeshRenderer[] meshes = obj.GetComponentsInChildren<MeshRenderer>();
            CharacterModel.RendererInfo[] rendererInfos = new CharacterModel.RendererInfo[meshes.Length];

            for (int i = 0; i < meshes.Length; i++)
            {
                rendererInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = meshes[i].material,
                    renderer = meshes[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                };
            }

            return rendererInfos;
        }

        public static GameObject LoadSurvivorModel(string modelName) {
            GameObject model = mainAssetBundle.LoadAsset<GameObject>(modelName);
            if (model == null) {
                Log.Error("Trying to load a null model- check to see if the name in your code matches the name of the object in Unity");
                return null;
            }

            return PrefabAPI.InstantiateClone(model, model.name, false);
        }

        internal static Texture LoadCharacterIcon(string characterName)
        {
            return mainAssetBundle.LoadAsset<Texture>("tex" + characterName + "Icon");
        }

        internal static Mesh LoadMesh(string meshName)
        {
            return mainAssetBundle.LoadAsset<Mesh>(meshName);
        }

        internal static GameObject LoadCrosshair(string crosshairName)
        {
            return Resources.Load<GameObject>("Prefabs/Crosshair/" + crosshairName + "Crosshair");
        }

        private static GameObject LoadEffect(string resourceName)
        {
            return LoadEffect(resourceName, "", false);
        }

        private static GameObject LoadEffect(string resourceName, string soundName)
        {
            return LoadEffect(resourceName, soundName, false);
        }

        private static GameObject LoadEffect(string resourceName, bool parentToTransform)
        {
            return LoadEffect(resourceName, "", parentToTransform);
        }

        private static GameObject LoadEffect(string resourceName, string soundName, bool parentToTransform)
        {
            GameObject newEffect = mainAssetBundle.LoadAsset<GameObject>(resourceName);

            newEffect.AddComponent<DestroyOnTimer>().duration = 12;
            newEffect.AddComponent<NetworkIdentity>();
            newEffect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            var effect = newEffect.AddComponent<EffectComponent>();
            effect.applyScale = false;
            effect.effectIndex = EffectIndex.Invalid;
            effect.parentToReferencedTransform = parentToTransform;
            effect.positionAtReferencedTransform = true;
            effect.soundName = soundName;

            AddNewEffectDef(newEffect, soundName);

            return newEffect;
        }

        internal static void AddNewEffectDef(GameObject effectPrefab)
        {
            AddNewEffectDef(effectPrefab, "");
        }

        internal static void AddNewEffectDef(GameObject effectPrefab, string soundName)
        {
            EffectDef newEffectDef = new EffectDef();
            newEffectDef.prefab = effectPrefab;
            newEffectDef.prefabEffectComponent = effectPrefab.GetComponent<EffectComponent>();
            newEffectDef.prefabName = effectPrefab.name;
            newEffectDef.prefabVfxAttributes = effectPrefab.GetComponent<VFXAttributes>();
            newEffectDef.spawnSoundEventName = soundName;

            effectDefs.Add(newEffectDef);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = Assets.mainAssetBundle.LoadAsset<Material>(materialName);

            if (!tempMat) return commandoMat;

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            mat.DisableKeyword("DITHER");

            return mat;
        }

        public static Material CreateMaterial(string materialName)
        {
            return Assets.CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return Assets.CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return Assets.CreateMaterial(materialName, emission, emissionColor, 0f);
        }
    }
}