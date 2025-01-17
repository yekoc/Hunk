﻿using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;

namespace HunkMod.Modules.Components
{
    internal class SyncVirus : INetMessage
    {
        private NetworkInstanceId netId;
        private GameObject target;

        public SyncVirus()
        {
        }

        public SyncVirus(NetworkInstanceId netId, GameObject target)
        {
            this.netId = netId;
            this.target = target;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.target = reader.ReadGameObject();
        }

        public void OnReceived()
        {
            if (!this.target) return;
            
            this.target.AddComponent<VirusHandler>();

            GameObject positionIndicator = GameObject.Instantiate(Modules.Assets.virusPositionIndicator);
            positionIndicator.transform.parent = this.target.transform;
            positionIndicator.transform.localPosition = Vector3.zero;

            positionIndicator.GetComponent<PositionIndicator>().targetTransform = this.target.GetComponent<CharacterBody>().mainHurtBox.transform;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.target);
        }
    }
}