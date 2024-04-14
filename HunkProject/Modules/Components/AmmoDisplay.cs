﻿using UnityEngine;
using RoR2;
using RoR2.UI;
using UnityEngine.Networking;

namespace HunkMod.Modules.Components
{
    public class AmmoDisplay : MonoBehaviour
    {
        public HUD targetHUD;
        public LanguageTextMeshController targetText;

        private HunkController hunk;

        private int totalAmmo
        {
            get
            {
                return this.hunk.weaponTracker.weaponData[this.hunk.weaponTracker.equippedIndex].totalAmmo;
            }
        }

        private void FixedUpdate()
        {
            if (this.targetHUD)
            {
                if (this.targetHUD.targetBodyObject)
                {
                    if (!this.hunk) this.hunk = this.targetHUD.targetBodyObject.GetComponent<HunkController>();
                }
            }

            if (this.targetText)
            {
                if (this.hunk)
                {
                    if (this.hunk.maxAmmo <= 0f)
                    {
                        this.targetText.token = "";
                        return;
                    }

                    if (this.hunk.ammo <= 0f)
                    {
                        if (this.totalAmmo > 0)
                        {
                            this.targetText.token = "<color=#C80000>0 / " + this.totalAmmo + Helpers.colorSuffix;
                        }
                        else
                        {
                            this.targetText.token = "<color=#C80000>0 / 0" + Helpers.colorSuffix;
                        }
                    }
                    else
                    {
                        this.targetText.token = Mathf.CeilToInt(this.hunk.ammo).ToString() + " / " + Mathf.CeilToInt(this.totalAmmo).ToString();
                    }
                }
                else
                {
                    this.targetText.token = "";
                }
            }
        }
    }
}