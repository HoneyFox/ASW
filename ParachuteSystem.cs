using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AntiSubmarineWeapon
{
    public class ParachuteSystem
    {
        protected PartModule parentModule;

        protected float dragCoefficient;
        protected float deployDuration;
        protected float deployAltitude;
        protected float armingDelay;

        private Rigidbody weaponRigidBody = null;
        private double launchTime = -1;
        private float currentDragCoefficient = 0f;
        private float deployRate = 0f;
        private bool deployed = false;
        private bool cutOff = false;
        
        public ParachuteSystem(PartModule parentModule, Rigidbody weaponRigidBody, float dragCoefficient, float deployDuration, float deployAlt, float armingDelay)
        {
            this.parentModule = parentModule;
            this.weaponRigidBody = weaponRigidBody;
            this.dragCoefficient = dragCoefficient;
            this.deployDuration = deployDuration;
            this.deployAltitude = deployAlt;
            this.armingDelay = armingDelay;
        }

        public void OnActivate()
        {
            launchTime = Planetarium.GetUniversalTime();
            deployed = false;
            cutOff = false;
            deployRate = dragCoefficient / deployDuration;
        }

        public void OnFixedUpdate()
        {
            if (parentModule == null) return;
            CelestialBody mainBody = parentModule.vessel.mainBody;
            if (mainBody.atmosphere == false) return;
            if (deployed == false)
            {
                if (Planetarium.GetUniversalTime() > launchTime + armingDelay)
                {
                    if(parentModule.vessel.altitude < deployAltitude && parentModule.vessel.verticalSpeed < 0f)
                    {
                        deployed = true;
                        currentDragCoefficient = 0f;
                        // If we have a deploy-chute animation, it should be activated here.
                    }
                }
            }
            else
            {
                if (cutOff == false)
                {
                    // Parachute deployed.
                    currentDragCoefficient = Mathf.MoveTowards(currentDragCoefficient, dragCoefficient, deployRate * TimeWarp.fixedDeltaTime);

                    Vector3 dragForce = -parentModule.vessel.GetSrfVelocity().normalized *
                        0.5f * currentDragCoefficient * (float)parentModule.vessel.atmDensity * parentModule.vessel.GetSrfVelocity().sqrMagnitude;
                    weaponRigidBody.AddForce(dragForce);
                    if (parentModule.vessel.altitude < 0f)
                    {
                        // Cut the parachute.
                        cutOff = true;
                        // We should hide the parachute model here.
                    }
                }
                else
                {
                    // No longer need this system.
                }
            }
        }
    }
}
