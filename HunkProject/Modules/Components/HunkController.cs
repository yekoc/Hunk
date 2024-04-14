﻿using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HunkMod.Modules.Components
{
    public class HunkController : MonoBehaviour
    {
        public ushort syncedWeapon;
        public NetworkInstanceId netId;

        public bool isAiming;
        public HunkWeaponDef weaponDef;
        private HunkWeaponDef lastWeaponDef;

        public float chargeValue;

        private CharacterBody characterBody;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;

        public int maxShellCount = 24;

        private int currentShell;
        private int currentSlug;
        private GameObject[] shellObjects;
        private GameObject[] slugObjects;

        public Action<HunkController> onWeaponUpdate;

        public int maxAmmo;
        public int ammo;

        private SkinnedMeshRenderer weaponRenderer;
        private HunkWeaponTracker _weaponTracker;

        public GameObject crosshairPrefab;
        public ParticleSystem machineGunVFX;

        public float reloadTimer;
        private WeaponNotificationQueue notificationQueue;
        private EntityStateMachine weaponStateMachine;

        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            //this.machineGunVFX = this.childLocator.FindChild("MachineGunVFX").gameObject.GetComponent<ParticleSystem>();
            this.weaponRenderer = this.childLocator.FindChild("WeaponModel").GetComponent<SkinnedMeshRenderer>();
            this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Weapon");

            this.Invoke("SetInventoryHook", 0.5f);
        }

        private void Start()
        {
            this.InitShells();

            this.EquipWeapon(0);

        }

        private void SetInventoryHook()
        {
            if (this.characterBody && this.characterBody.master && this.characterBody.master.inventory)
            {
                this.characterBody.master.inventory.onInventoryChanged += this.Inventory_onInventoryChanged;
            }

            this.CheckForNeedler();
        }

        public HunkWeaponTracker weaponTracker
        {
            get
            {
                if (this._weaponTracker) return this._weaponTracker;
                if (this.characterBody && this.characterBody.master)
                {
                    HunkWeaponTracker i = this.characterBody.master.GetComponent<HunkWeaponTracker>();
                    if (!i) i = this.characterBody.master.gameObject.AddComponent<HunkWeaponTracker>();
                    this._weaponTracker = i;
                    return i;
                }
                return null;
            }
        }

        private void CheckForNeedler()
        {
            /*if (this.characterBody && this.characterBody.master && this.characterBody.master.inventory)
            {
                HunkWeaponDef desiredWeapon = this.pistolWeaponDef;

                if (this.characterBody.master.inventory.GetItemCount(RoR2Content.Items.TitanGoldDuringTP) > 0)
                {
                    if (this.defaultWeaponDef == DriverWeaponCatalog.Pistol || this.defaultWeaponDef == DriverWeaponCatalog.PyriteGun) desiredWeapon = DriverWeaponCatalog.PyriteGun;
                }

                if (this.characterBody.master.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement) > 0)
                {
                    desiredWeapon = DriverWeaponCatalog.Needler;
                }

                if (this.maxWeaponTimer <= 0f && desiredWeapon != this.defaultWeaponDef)
                {
                    this.defaultWeaponDef = desiredWeapon;
                    this.PickUpWeapon(this.defaultWeaponDef);
                }
            }*/
        }

        private void Inventory_onInventoryChanged()
        {
            this.CheckForNeedler();
        }

        public void ConsumeAmmo()
        {
            this.reloadTimer = 2f;
            this.ammo--;

            this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].currentAmmo--;

            if (this.ammo <= 0) this.ammo = 0;
            if (this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].currentAmmo <= 0) this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].currentAmmo = 0;
        }

        private void FixedUpdate()
        {
            this.reloadTimer -= Time.fixedDeltaTime;

            if (this.reloadTimer <= 0f && this.ammo < this.maxAmmo)
            {
                this.TryReload();
            }
        }

        private void TryReload()
        {
            if (this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].totalAmmo > 0) this.weaponStateMachine.SetInterruptState(new SkillStates.Hunk.Reload(), EntityStates.InterruptPriority.Any);
        }

        public void ServerGetStoredWeapon(HunkWeaponDef newWeapon, float ammo, HunkController hunkController)
        {
            NetworkIdentity identity = hunkController.gameObject.GetComponent<NetworkIdentity>();
            if (!identity) return;

            new SyncStoredWeapon(identity.netId, newWeapon.index, ammo).Send(NetworkDestination.Clients);
        }

        public void ServerPickUpWeapon(HunkWeaponDef newWeapon, bool cutAmmo, HunkController hunkController, bool isAmmoBox = false)
        {
            NetworkIdentity identity = hunkController.gameObject.GetComponent<NetworkIdentity>();
            if (!identity) return;

            new SyncWeapon(identity.netId, newWeapon.index, cutAmmo, isAmmoBox).Send(NetworkDestination.Clients);
        }
        
        public void FinishReload()
        {
            int diff = this.maxAmmo - this.ammo;

            if (this.ammo + this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].totalAmmo >= this.weaponDef.magSize)
            {
                this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].currentAmmo = this.weaponDef.magSize;
                this.ammo = this.weaponDef.magSize;
            }
            else
            {
                this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].currentAmmo = this.ammo + this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].totalAmmo;
                this.ammo = this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].currentAmmo;
            }

            this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].totalAmmo -= diff;
            if (this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].totalAmmo <= 0) this.weaponTracker.weaponData[this.weaponTracker.equippedIndex].totalAmmo = 0;
        }

        public void PickUpWeapon(HunkWeaponDef newWeapon)
        {
            this.weaponDef = newWeapon;

            this.weaponTracker.AddWeapon(newWeapon);

            this.TryPickupNotification();

            if (this.onWeaponUpdate == null) return;
            this.onWeaponUpdate(this);
        }

        private void TryPickupNotification(bool force = false)
        {
            // attempt to add the component if it's not there
            if (!this.notificationQueue && this.characterBody.master)
            {
                this.notificationQueue = this.characterBody.master.GetComponent<WeaponNotificationQueue>();
               // if (!this.notificationQueue) this.notificationQueue = this.characterBody.master.gameObject.AddComponent<WeaponNotificationQueue>();
            }

            if (this.notificationQueue)
            {
                if (force)
                {
                    WeaponNotificationQueue.PushWeaponNotification(this.characterBody.master, this.weaponDef.index);
                    this.lastWeaponDef = this.weaponDef;
                    return;
                }

                if (this.weaponDef != this.lastWeaponDef)
                {
                    WeaponNotificationQueue.PushWeaponNotification(this.characterBody.master, this.weaponDef.index);
                }
                this.lastWeaponDef = this.weaponDef;
            }
        }

        public void EquipWeapon(int index)
        {
            this.weaponTracker.equippedIndex = index;

            this.weaponDef = this.weaponTracker.weaponData[index].weaponDef;

            // ammo
            this.maxAmmo = this.weaponDef.magSize;
            this.ammo = this.weaponTracker.weaponData[index].currentAmmo;

            // model swap
            if (this.weaponDef.mesh)
            {
                this.weaponRenderer.sharedMesh = this.weaponDef.mesh;
                this.characterModel.baseRendererInfos[this.characterModel.baseRendererInfos.Length - 1].defaultMaterial = this.weaponDef.material;
            }

            // crosshair
            this.crosshairPrefab = this.weaponDef.crosshairPrefab;
            this.characterBody._defaultCrosshairPrefab = this.crosshairPrefab;

            // animator layer
            /*switch (this.weaponDef.animationSet)
            {
                case DriverWeaponDef.AnimationSet.Default:
                    this.EnableLayer("");
                    break;
                case DriverWeaponDef.AnimationSet.TwoHanded:
                    this.EnableLayer("Body, Shotgun");
                    break;
                case DriverWeaponDef.AnimationSet.BigMelee:
                    this.EnableLayer("Body, Hammer");
                    break;
            }*/
        }

        private void EnableLayer(string layerName)
        {
            if (!this.animator) return;

            //this.animator.SetLayerWeight(this.animator.GetLayerIndex("Body, Shotgun"), 0f);
            //this.animator.SetLayerWeight(this.animator.GetLayerIndex("Body, Hammer"), 0f);

            if (layerName == "") return;

            this.animator.SetLayerWeight(this.animator.GetLayerIndex(layerName), 1f);
        }

        private void InitShells()
        {
            this.currentShell = 0;

            this.shellObjects = new GameObject[this.maxShellCount + 1];

            GameObject desiredShell = Assets.shotgunShell;

            for (int i = 0; i < this.maxShellCount; i++)
            {
                this.shellObjects[i] = GameObject.Instantiate(desiredShell, this.childLocator.FindChild("Weapon"), false);
                this.shellObjects[i].transform.localScale = Vector3.one * 1.1f;
                this.shellObjects[i].SetActive(false);
                this.shellObjects[i].GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;

                this.shellObjects[i].layer = LayerIndex.ragdoll.intVal;
                this.shellObjects[i].transform.GetChild(0).gameObject.layer = LayerIndex.ragdoll.intVal;
            }

            this.currentSlug = 0;

            this.slugObjects = new GameObject[this.maxShellCount + 1];

            desiredShell = Assets.shotgunSlug;

            for (int i = 0; i < this.maxShellCount; i++)
            {
                this.slugObjects[i] = GameObject.Instantiate(desiredShell, this.childLocator.FindChild("Weapon"), false);
                this.slugObjects[i].transform.localScale = Vector3.one * 1.2f;
                this.slugObjects[i].SetActive(false);
                this.slugObjects[i].GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;

                this.slugObjects[i].layer = LayerIndex.ragdoll.intVal;
                this.slugObjects[i].transform.GetChild(0).gameObject.layer = LayerIndex.ragdoll.intVal;
            }
        }

        public void DropShell(Vector3 force)
        {
            if (this.shellObjects == null) return;

            if (this.shellObjects[this.currentShell] == null) return;

            Transform origin = this.childLocator.FindChild("Weapon");

            this.shellObjects[this.currentShell].SetActive(false);

            this.shellObjects[this.currentShell].transform.position = origin.position;
            this.shellObjects[this.currentShell].transform.SetParent(null);

            this.shellObjects[this.currentShell].SetActive(true);

            Rigidbody rb = this.shellObjects[this.currentShell].gameObject.GetComponent<Rigidbody>();
            if (rb) rb.velocity = force;

            this.currentShell++;
            if (this.currentShell >= this.maxShellCount) this.currentShell = 0;
        }

        public void DropSlug(Vector3 force)
        {
            if (this.slugObjects == null) return;

            if (this.slugObjects[this.currentSlug] == null) return;

            Transform origin = this.childLocator.FindChild("Weapon");

            this.slugObjects[this.currentSlug].SetActive(false);

            this.slugObjects[this.currentSlug].transform.position = origin.position;
            this.slugObjects[this.currentSlug].transform.SetParent(null);

            this.slugObjects[this.currentSlug].SetActive(true);

            Rigidbody rb = this.slugObjects[this.currentSlug].gameObject.GetComponent<Rigidbody>();
            if (rb) rb.velocity = force;

            this.currentSlug++;
            if (this.currentSlug >= this.maxShellCount) this.currentSlug = 0;
        }

        private void OnDestroy()
        {
            if (this.shellObjects != null && this.shellObjects.Length > 0)
            {
                for (int i = 0; i < this.shellObjects.Length; i++)
                {
                    if (this.shellObjects[i]) Destroy(this.shellObjects[i]);
                }
            }

            if (this.slugObjects != null && this.slugObjects.Length > 0)
            {
                for (int i = 0; i < this.slugObjects.Length; i++)
                {
                    if (this.slugObjects[i]) Destroy(this.slugObjects[i]);
                }
            }

            if (this.characterBody && this.characterBody.master && this.characterBody.master.inventory)
            {
                this.characterBody.master.inventory.onInventoryChanged -= this.Inventory_onInventoryChanged;
            }
        }
    }
}