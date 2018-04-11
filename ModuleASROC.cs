using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BDArmory.Parts;

namespace AntiSubmarineWeapon
{
    public class ModuleASROC : PartModule
    {
        [KSPField(isPersistant = false)]
        public float boosterThrust;
        [KSPField(isPersistant = false)]
        public float boosterDuration;
        [KSPField(isPersistant = false)]
        public float normalCoefficient;
        [KSPField(isPersistant = false)]
        public float propellantMass;
        [KSPField(isPersistant = false)]
        public float loftCoefficient;
        [KSPField(isPersistant = false)]
        public float maxTorque;
        [KSPField(isPersistant = false)]
        public string exhaustPrefabPath;
        [KSPField(isPersistant = false)]
        public string exhaustTransform;
        [KSPField(isPersistant = false)]
        public Vector3 exhaustOffset;
        [KSPField(isPersistant = false)]
        public string audioClipPath;


        [KSPField(isPersistant = false)]
        public float dragCoefficient;
        [KSPField(isPersistant = false)]
        public float deployDuration;
        [KSPField(isPersistant = false)]
        public float deployAltitude;
        [KSPField(isPersistant = false)]
        public float armingDelay;

        protected Rigidbody partRigidBody;
        protected MissileLauncher missileModule;
        protected BoosterSystem booster;
        protected ParachuteSystem parachute;
        protected Vector3 targetCoords;

        private bool activated = false;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (state == StartState.Editor) return;

            missileModule = this.part.GetComponent<MissileLauncher>();
            if (missileModule == null)
                Debug.LogError("Failed when find the MissileLauncher!");
        }

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (booster == null && parachute == null)
                {
                    partRigidBody = this.part.GetComponent<Rigidbody>();
                    if (partRigidBody != null)
                    {
                        booster = new BoosterSystem(
                            this, 
                            partRigidBody, 
                            boosterThrust, 
                            boosterDuration,
                            propellantMass, 
                            exhaustPrefabPath,
                            exhaustTransform,
                            exhaustOffset,
                            audioClipPath
                        );
                        parachute = new ParachuteSystem(
                            this, 
                            partRigidBody, 
                            dragCoefficient, 
                            deployDuration,
                            deployAltitude, 
                            armingDelay
                        );
                    }
                }
                else
                {
                    if (activated == false)
                    {
                        if (!missileModule.HasFired) return;
                        activated = true;
                        booster.OnActivate();
                        parachute.OnActivate();
                        
                        // When weapon is launched, the target coordinates should be fed.
                        if (missileModule.TargetAcquired)
                            UpdateTargetCoords(missileModule.TargetPosition);
                        else if (missileModule.legacyTargetVessel != null)
                            UpdateTargetCoords(missileModule.legacyTargetVessel.GetWorldPos3D());
                    }
                    else
                    {
                        booster.OnFixedUpdate();
                        parachute.OnFixedUpdate();

                        // Guidance and aero calculation.
                        if(parachute.Deployed == false)
                            CalculateTrajectory(targetCoords);
                        Vector3 aeroForceDir = vessel.transform.forward.normalized - vessel.GetSrfVelocity().normalized;
                        Vector3 aeroForce = aeroForceDir * 0.5f * normalCoefficient * (float)vessel.atmDensity * vessel.GetSrfVelocity().sqrMagnitude;
                        partRigidBody.AddForce(aeroForce);
                    }
                }
            }
        }

        /// <summary>
        /// Feed the guidance system with target coordinates.
        /// Should do this when launching the weapon.
        /// You can also call this in-flight to provide mid-course correction.
        /// </summary>
        /// <param name="targetCoords"></param>
        public void UpdateTargetCoords(Vector3 targetCoords)
        {
            this.targetCoords = targetCoords;
        }

        protected void CalculateTrajectory(Vector3 targetCoords)
        {
            Vector3d selfPos = vessel.GetWorldPos3D();
            float distance = Vector3.Distance(selfPos, targetCoords);
            float loftDistance = loftCoefficient * distance * distance;
            Vector3 aimPos = targetCoords + vessel.upAxis.normalized * loftDistance;

            Vector3 aimTorqueAxis = Vector3.Cross(vessel.transform.forward.normalized, (aimPos - selfPos).normalized);
            partRigidBody.AddTorque(aimTorqueAxis * Mathf.Min(maxTorque, vessel.GetSrfVelocity().sqrMagnitude));
        }
    }
}
