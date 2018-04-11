using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BDArmory.Parts;
using BDArmory.FX;

namespace AntiSubmarineWeapon
{
    public class BoosterSystem
    {
        protected PartModule parentModule;

        protected float boosterThrust;
        protected float boosterDuration;
        protected float propellantMass;

        protected string exhaustPrefabPath;
        protected string exhaustTransform;
        protected Vector3 exhaustOffset;
        protected string audioClipPath;

        private Rigidbody weaponRigidBody = null;
        private MissileLauncher missileModule = null;
        private double launchTime = -1;
        private bool exhausted = false;
        private GameObject exhaustPrefab = null;
        private AudioSource audioSource = null;
        private float startMass = -1;

        public BoosterSystem(PartModule parentModule, Rigidbody weaponRigidBody, float thrust, float duration, float propellantMass, string exhaustPrefabPath, string exhaustTransform, Vector3 exhaustOffset, string audioClipPath)
        {
            this.parentModule = parentModule;
            missileModule = parentModule.part.GetComponent<MissileLauncher>();
            if (missileModule == null)
                Debug.LogError("Failed when find the MissileLauncher!");

            this.weaponRigidBody = weaponRigidBody;
            this.boosterThrust = thrust;
            this.boosterDuration = duration;
            this.propellantMass = propellantMass;
            this.exhaustPrefabPath = exhaustPrefabPath;
            this.exhaustTransform = exhaustTransform;
            this.exhaustOffset = exhaustOffset;
            this.audioClipPath = audioClipPath;
        }

        public void OnActivate()
        {
            launchTime = Planetarium.GetUniversalTime();
            exhausted = false;
            startMass = parentModule.vessel.GetTotalMass();

            // We should activate the booster VFX here.
            if(string.IsNullOrEmpty(exhaustPrefabPath) == false)
            {
                exhaustPrefab = (GameObject)UnityEngine.Object.Instantiate(GameDatabase.Instance.GetModel(exhaustPrefabPath));
                exhaustPrefab.SetActive(true);
                exhaustPrefab.transform.parent = parentModule.part.FindModelTransform(exhaustTransform);
                exhaustPrefab.transform.localPosition = exhaustOffset;
                exhaustPrefab.transform.localRotation = Quaternion.identity;
                KSPParticleEmitter[] emitters = exhaustPrefab.GetComponentsInChildren<KSPParticleEmitter>();
                for (int i = 0; i < emitters.Length; ++i)
                {
                    if(emitters[i].useWorldSpace)
                    {
                        BDAGaplessParticleEmitter gaplessEmitter = emitters[i].gameObject.AddComponent<BDAGaplessParticleEmitter>();
                        gaplessEmitter.part = parentModule.part;
                        gaplessEmitter.emit = true;
                    }
                    else
                    {
                        emitters[i].emit = true;
                    }
                }
            }

            // And SFX for booster as well.
            audioSource = parentModule.part.gameObject.AddComponent<AudioSource>();
            audioSource.maxDistance = 1000f;
            audioSource.loop = true;
            audioSource.spatialBlend = 1;
            audioSource.clip = GameDatabase.Instance.GetAudioClip(audioClipPath);
            audioSource.Play();
        }

        public void OnFixedUpdate()
        {
            if (parentModule == null) return;
            if (exhausted == false)
            {
                if (Planetarium.GetUniversalTime() < launchTime + boosterDuration)
                {
                    weaponRigidBody.AddForce(parentModule.vessel.up.normalized * boosterThrust);
                    float deltaMass = propellantMass * TimeWarp.fixedDeltaTime / boosterDuration;
                    parentModule.part.mass -= deltaMass;
                    parentModule.part.UpdateMass();
                }
                else
                {
                    exhausted = true;
                    parentModule.part.mass = startMass - propellantMass;
                    parentModule.part.UpdateMass();

                    // We should stop the booster VFX and SFX here.
                    if (exhaustPrefab != null)
                        GameObject.Destroy(exhaustPrefab);
                    if (audioSource != null)
                        audioSource.Stop();
                }
            }
        }
    }
}
