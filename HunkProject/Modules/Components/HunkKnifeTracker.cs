﻿using UnityEngine;

namespace HunkMod.Modules.Components
{
    public class HunkKnifeTracker : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(this, 1f);
        }
    }
}