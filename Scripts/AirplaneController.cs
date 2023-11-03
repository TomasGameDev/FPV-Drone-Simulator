using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoysAirplane
{
    [RequireComponent(typeof(Rigidbody))]
    public class AirplaneController : MonoBehaviour, IJoystickContrillable
    {
        public AirplaneMoovingParameters airplaneMooving;
        [System.Serializable]
        public class AirplaneMoovingParameters
        {
            public float throttleIncrement = 10f;
            public float throttleDecrement = 5f;

            public Vector3 maxThrustDirection = Vector3.forward * 30;

            public float rollThrottleIncrement = 5f;
            public float rollThrottleDecrement = 0.1f;

            public float responsiveness = 80f;

            //public float forwardBal = 2.5f;
        }
        [Space(10)]
        public AirplaneDynamicParameters airplaneDynamic;
        [System.Serializable]
        public class AirplaneDynamicParameters
        {
            public float throttle;
            public float roll;
            public float rollThrottleMultiplier;
            public float pitch;
            public float yaw;
            public bool isMooving;
            public bool isMoovingFroward;
        }
        float vThrottle;
        float vRoll;
        float vPitch;
        float vYaw;
        [Space(10)]
        public AirplaneSpeed airplaneSpeed;
        [System.Serializable]
        public class AirplaneSpeed
        {
            public float magnitude;
            public float forwardSpeed;
            public float forwardSpeedAbs;
            public float upSpeed;
            public float upSpeedAbs;
        }
        Vector3 forwardPoint;
        Vector3 upPoint;

        [Space(10)]
        public AirplaneGravity airplanePhysics;
        [System.Serializable]
        public class AirplaneGravity
        {
            public bool enableGravity = true;
            public Vector3 gravity;
            public float airGliden = 100;
            [Range(0, 1)] public float airResistance = 0.5f;
            public AnimationCurve glidenCurve;
            public float glidenAttackAngle;
            public float gravityAttackAngle;
        }
        [Space(10)]
        public AirplaneUI airplaneUI;
        [System.Serializable]
        public class AirplaneUI
        {
            public float valueScale = 1;
            public Text speedText;
            public Text forwardSpeedText;
            public Text downSpeedText;
            public Text throttleText;
        }
        [Space(10)]
        [Header("Other")]
        [Space(10)]
        public Vector3 sapawnpoint;

        float rollResponseModifier
        {
            get
            {
                return (rig.mass * 0.05f) * airplaneMooving.responsiveness;
            }
        }

        float forwardResponseModifier
        {
            get
            {
                return (rig.mass * 0.02f) * airplaneMooving.responsiveness;
            }
        }

        Rigidbody rig;
        void Start()
        {
            rig = GetComponent<Rigidbody>();
            forwardPoint = upPoint = transform.position;
        }

        public void HandleInputs()
        {
            if (UnityEngine.Device.SystemInfo.deviceType == DeviceType.Desktop)
            {
                airplaneDynamic.roll = Input.GetAxis("Horizontal");
                airplaneDynamic.pitch = Input.GetAxis("Vertical");

                if (Input.GetKey(KeyCode.Space))
                {
                    airplaneDynamic.throttle += airplaneMooving.throttleIncrement;
                    airplaneDynamic.isMooving = airplaneDynamic.isMoovingFroward = true;
                }
                else airplaneDynamic.isMoovingFroward = false;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    airplaneDynamic.throttle -= airplaneMooving.throttleIncrement * (airplaneDynamic.throttle > -airplaneMooving.throttleDecrement * 10f && airplaneSpeed.forwardSpeed > 0.2f ? 1f : Time.deltaTime);
                    airplaneDynamic.isMooving = true;
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Respawn();
                }
                if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.Space))
                {
                    airplaneDynamic.isMooving = false;
                }
            }
            else
            {
                if (vRoll != 0) airplaneDynamic.roll += Time.deltaTime * 10f * vRoll;
                else airplaneDynamic.roll = 0;
                airplaneDynamic.roll = Mathf.Clamp(airplaneDynamic.roll, -1f, 1f);
                if (vPitch != 0) airplaneDynamic.pitch += Time.deltaTime * 10f * vPitch;
                else airplaneDynamic.pitch = 0;
                airplaneDynamic.pitch = Mathf.Clamp(airplaneDynamic.pitch, -1f, 1f);
            }

            if (vThrottle != 0)
            {
                airplaneDynamic.throttle += airplaneMooving.throttleDecrement * vThrottle;
            }
            airplaneDynamic.isMoovingFroward = vThrottle > 0 ? true : false;
            airplaneDynamic.isMooving = vThrottle == 0 ? false : true;
            airplaneDynamic.throttle = Mathf.Clamp(airplaneDynamic.throttle, -100f, 100f);

            if (vYaw != 0) airplaneDynamic.yaw += Time.deltaTime * 10f * vYaw;
            else airplaneDynamic.yaw = 0;
            airplaneDynamic.yaw = Mathf.Clamp(airplaneDynamic.yaw, -1f, 1f);
        }
        public void SetThrottle(float _v)
        {
            vThrottle = _v;
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
            transform.position = forwardPoint = upPoint = sapawnpoint;
            transform.rotation = Quaternion.identity;
            rig.velocity = Vector3.zero;
            airplaneSpeed.forwardSpeed = airplaneSpeed.upSpeed = airplaneSpeed.magnitude = airplaneDynamic.throttle = airplaneDynamic.rollThrottleMultiplier = 0;
        }
        private void FixedUpdate()
        {
            UpdateMoovingParameters();

            UpdatePhysics();

            UpdateUI();

        }
        private void UpdatePhysics()
        {
            rig.AddForce(transform.TransformDirection(airplaneMooving.maxThrustDirection) * airplaneDynamic.throttle * airplaneDynamic.rollThrottleMultiplier); ;
            //((  float _forwardBal =isMoovingFroward ? forwardSpeedAbs * forwardBal : forwardSpeedAbs) + 0.1f) * (roll == 0 ? 1f : forwardBal); / _forwardBal
            //print(_forwardBal);
            rig.AddTorque((transform.up * airplaneDynamic.yaw * forwardResponseModifier));
            rig.AddTorque((transform.right * -airplaneDynamic.pitch * forwardResponseModifier));
            rig.AddTorque(transform.forward * -airplaneDynamic.roll * rollResponseModifier);

            if (airplaneSpeed.magnitude < 300)
            {
                if (airplanePhysics.enableGravity)
                {
                    // airplanePhysics.gravity = Physics.gravity * airplanePhysics.airResistance * airplanePhysics.gravityAttackAngle / airplanePhysics.glidenAttackAngle * Time.deltaTime;
                    airplanePhysics.gravity = Vector3.Lerp(Physics.gravity * Time.deltaTime, Vector3.zero, airplaneSpeed.forwardSpeedAbs);
                    rig.velocity += airplanePhysics.gravity;
                    if (rig.useGravity) rig.useGravity = false;
                }
                float forwardVelocity = airplanePhysics.airGliden * -airplaneSpeed.upSpeed * airplanePhysics.glidenAttackAngle * Time.deltaTime;
                rig.velocity += transform.forward * forwardVelocity;
                //rig.velocity += transform.up * forwardVelocity * airplaneSpeed.upSpeed;
            }
        }
        private void UpdateMoovingParameters()
        {
            airplaneSpeed.magnitude = rig.velocity.magnitude;

            //FORWARD SPEED
            Vector3 _forwardPoint = transform.position + Vector3.Project(forwardPoint - transform.position, transform.forward) + Vector3.Dot(Vector3.zero, transform.forward) * transform.forward;
            Vector3 _forwardPointNormal = transform.position - _forwardPoint;
            airplaneSpeed.forwardSpeed = _forwardPointNormal.magnitude;
            airplaneSpeed.forwardSpeedAbs = airplaneSpeed.forwardSpeed /= 2f;
            bool forwardPointDirection = Vector3.Distance(transform.forward, _forwardPointNormal) < Vector3.Distance(-transform.forward, _forwardPointNormal);
            airplaneSpeed.forwardSpeed *= forwardPointDirection ? 1f : -1f;
            forwardPoint = transform.position;
            //DOWN SPEED
            Vector3 _upPoint = transform.position + Vector3.Project(upPoint - transform.position, transform.up) + Vector3.Dot(Vector3.zero, transform.up) * transform.up;
            Vector3 _upPointNormal = transform.position - _upPoint;
            airplaneSpeed.upSpeed = _upPointNormal.magnitude;
            airplaneSpeed.upSpeedAbs = airplaneSpeed.upSpeed /= 2f;
            print(transform.up);
            bool upSpeedDirection = Vector3.Distance(transform.up, _upPointNormal) < Vector3.Distance(-transform.up, _upPointNormal);
            airplaneSpeed.upSpeed *= upSpeedDirection ? 1f : -1f;
            upPoint = transform.position;

            //float wtf1 = (Vector3.Distance(Physics.gravity, transform.up * Physics.gravity.magnitude) - Physics.gravity.magnitude) / Physics.gravity.magnitude;
            //float wtf2 = (Vector3.Distance(Physics.gravity, -transform.up * Physics.gravity.magnitude) - Physics.gravity.magnitude) / Physics.gravity.magnitude;
            //float wtf12 = (wtf2 - wtf1) / 2f;

            float forwardAngle = (Vector3.Distance(Physics.gravity, transform.forward * Physics.gravity.magnitude) - Physics.gravity.magnitude) / Physics.gravity.magnitude;
            float backwardAngle = (Vector3.Distance(Physics.gravity, -transform.forward * Physics.gravity.magnitude) - Physics.gravity.magnitude) / Physics.gravity.magnitude;
            float avarageAngle = (forwardAngle - backwardAngle) / 2f;
            airplanePhysics.gravityAttackAngle = Mathf.Abs(avarageAngle);
            if (avarageAngle >= 0.5f) airplanePhysics.glidenAttackAngle = (1f - avarageAngle) * -2f;
            if (avarageAngle > -0.5f && avarageAngle < 0.5f) airplanePhysics.glidenAttackAngle = avarageAngle * -2f;
            if (avarageAngle <= -0.5f) airplanePhysics.glidenAttackAngle = (1f + avarageAngle) * 2f;

            if (airplaneDynamic.throttle + airplaneMooving.throttleDecrement < 0 && !airplaneDynamic.isMooving)
                airplaneDynamic.throttle += airplaneMooving.throttleDecrement;
            else if (airplaneDynamic.throttle - airplaneMooving.throttleDecrement > 0 && !airplaneDynamic.isMoovingFroward)
                airplaneDynamic.throttle -= airplaneMooving.throttleDecrement;
            else if (!airplaneDynamic.isMooving) airplaneDynamic.throttle = 0;

            if (airplaneDynamic.roll > 0)
            {
                if (airplaneDynamic.rollThrottleMultiplier < airplaneMooving.rollThrottleIncrement + 1) airplaneDynamic.rollThrottleMultiplier += Time.deltaTime;
            }
            else airplaneDynamic.rollThrottleMultiplier = 1;
        }
        private void UpdateUI()
        {
            airplaneUI.speedText.text = Mathf.FloorToInt(airplaneSpeed.magnitude * airplaneUI.valueScale).ToString();
            airplaneUI.forwardSpeedText.text = Mathf.FloorToInt(airplaneSpeed.forwardSpeed * 100f * airplaneUI.valueScale).ToString();
            airplaneUI.downSpeedText.text = Mathf.FloorToInt(airplaneSpeed.upSpeed * 100f * airplaneUI.valueScale).ToString();
            airplaneUI.throttleText.text = Mathf.FloorToInt(airplaneDynamic.throttle).ToString();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(forwardPoint, 0.1f);
        }
        JoystickController joystickController;
        public void LinkJoyStick(JoystickController _scr_JoystickController)
        {
            joystickController = _scr_JoystickController;
        }

        public void Move(float _moveX, float _moveZ, float _willSpeed)
        {
            SetThrottle(_moveZ);
            SetYaw(_moveX);
        }
    }
}