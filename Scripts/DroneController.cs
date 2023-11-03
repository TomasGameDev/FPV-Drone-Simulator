using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoysAirplane
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneController : MonoBehaviour, IJoystickContrillable
    {
        public bool stabilization;
        public DronePartsParameters droneParts;
        [System.Serializable]
        public class DronePartsParameters
        {
            public Transform propellerFR;
            public Transform propellerFL;
            public Transform propellerBR;
            public Transform propellerBL;

            public float FRRot;
            public float FLRot;
            public float BRRot;
            public float BLRot;
        }
        [Space(10)]
        public DroneParameters droneParameters;
        [System.Serializable]
        public class DroneParameters
        {
            public float throttleIncrement = 10;
            public float maxThrottle = 500;
            public float roll;
            public float pitch;
            public float yaw;

            public float stabilizationForce;
        }
        [Space(10)]
        public DroneSpeed droneSpeed;
        [System.Serializable]
        public class DroneSpeed
        {
            public float magnitude;
            public float forwardSpeed;
            public float forwardSpeedAbs;
        }

        [Space(10)]
        public DroneUI droneUI;
        [System.Serializable]
        public class DroneUI
        {
            public float speedValueScale = 1;
            public Text speedText;
            public float throttleValueScale = 1;
            public Text throttleText;
        }
        [Space(10)]

        public DroneGyroscope droneGyroscope;
        [System.Serializable]
        public class DroneGyroscope
        {
            public float Roll; 
            public float Pitch; 
        }

        [Space(10)]
        [Header("Other")]
        [Space(10)]
        public Vector3 sapawnpoint;

        float vThrottle;
        float vRoll;
        float vPitch;
        float vYaw;

        Rigidbody rig;

        void Start()
        {
            rig = GetComponent<Rigidbody>();
        }

        public void HandleInputs()
        {
            if (Input.GetKey(KeyCode.R))
            {
                Respawn();
            }
        }
        public void SetThrottle(float _v)
        {
            vThrottle += _v * droneParameters.throttleIncrement;
            vThrottle = Mathf.Clamp(vThrottle, 0, droneParameters.maxThrottle);
        }
        public void SetRoll(float _v)
        {
            vRoll = _v;
        }
        public void SetPitch(float _v)
        {
            vPitch = _v;
        }
        public void SetYaw(float _v)
        {
            vYaw = _v;
        }

        void Update()
        {
            HandleInputs();
            //if (transform.position.y < 9.5f) Respawn();
        }
        public void Respawn()
        {
            transform.position = sapawnpoint;
            transform.rotation = Quaternion.identity;
            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
            droneSpeed.forwardSpeed = droneSpeed.magnitude = 0;
        }
        private void FixedUpdate()
        {
            UpdateMoovingParameters();

            UpdatePhysics();

            UpdateUI();

        }
        private void UpdatePhysics()
        {
            droneParts.FRRot = vThrottle + vPitch * droneParameters.pitch - vRoll * droneParameters.roll;
            droneParts.FLRot = vThrottle + vPitch * droneParameters.pitch + vRoll * droneParameters.roll;
            droneParts.BRRot = vThrottle - vPitch * droneParameters.pitch - vRoll * droneParameters.roll;
            droneParts.BLRot = vThrottle - vPitch * droneParameters.pitch + vRoll * droneParameters.roll;

            if (stabilization)
            {
                droneParts.FRRot += droneGyroscope.Roll * droneParameters.stabilizationForce;
                droneParts.BRRot += droneGyroscope.Roll * droneParameters.stabilizationForce; 

                droneParts.FRRot += droneGyroscope.Pitch * droneParameters.stabilizationForce;
                droneParts.FLRot += droneGyroscope.Pitch * droneParameters.stabilizationForce; 
            }

            rig.AddForceAtPosition(transform.up * droneParts.FRRot, droneParts.propellerFR.position);
            rig.AddForceAtPosition(transform.up * droneParts.FLRot, droneParts.propellerFL.position);


            rig.AddForceAtPosition(transform.up * droneParts.BRRot, droneParts.propellerBR.position);
            rig.AddForceAtPosition(transform.up * droneParts.BLRot, droneParts.propellerBL.position);

            rig.AddTorque(transform.up * vYaw * droneParameters.yaw);
        }
        private void UpdateMoovingParameters()
        {
            droneSpeed.magnitude = rig.velocity.magnitude;
             
            droneGyroscope.Roll = transform.eulerAngles.z; 


            droneGyroscope.Pitch = transform.eulerAngles.x; 
        }
        private void UpdateUI()
        {
            droneUI.speedText.text = Mathf.FloorToInt(droneSpeed.magnitude * droneUI.speedValueScale).ToString();
            droneUI.throttleText.text = Mathf.FloorToInt(vThrottle * droneUI.throttleValueScale).ToString();
        }
        JoystickController joystickController;
        public void LinkJoyStick(JoystickController _scr_JoystickController)
        {
            joystickController = _scr_JoystickController;
        }

        public void SetAxis(string axisName, float value)
        {
            switch (axisName)
            {
                case "Roll":
                    SetRoll(value);
                    break;
                case "Pitch":
                    SetPitch(value);
                    break;
                case "Yaw":
                    SetYaw(value);
                    break;
                case "Throttle":
                    SetThrottle(value);
                    break;
            }
        }
    }
}