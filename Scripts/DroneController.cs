using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour, IJoystickContrillable
{
    public static DroneController instance;
    public DronePartsParameters droneParts;
    [System.Serializable]
    public class DronePartsParameters
    {
        public Transform propellerFR;
        public Transform propellerFL;
        public Transform propellerBR;
        public Transform propellerBL;

        [HideInInspector] public AudioSource propellerFRSound;
        [HideInInspector] public AudioSource propellerFLSound;
        [HideInInspector] public AudioSource propellerBRSound;
        [HideInInspector] public AudioSource propellerBLSound;

        public float FRRot;
        public float FLRot;
        public float BRRot;
        public float BLRot;

        public Transform gyroscope;

        public DroneTimer droneTimer;

    }
    [Space(10)]
    public DroneParameters droneParameters;
    [System.Serializable]
    public class DroneParameters
    {
        bool _stabilization;
        public bool stabilization
        {
            get { return _stabilization; }
            set
            {
                _stabilization = value;
                instance.droneUI.modeText.text = value ? "STAB" : "ACRO";
            }
        }
        public float stabilizationForce;

        public float throttleIncrement = 10;
        public float maxThrottle = 500;
        public float roll;
        public float pitch;
        public float yaw; 

        public float vThrottle;
        public float vRoll;
        public float vPitch;
        public float vYaw;

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
        public Text modeText;
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

    Rigidbody rig;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        droneParts.propellerBRSound = droneParts.propellerBR.GetComponent<AudioSource>();
        droneParts.propellerBLSound = droneParts.propellerBL.GetComponent<AudioSource>();
        droneParts.propellerFRSound = droneParts.propellerFR.GetComponent<AudioSource>();
        droneParts.propellerFLSound = droneParts.propellerFL.GetComponent<AudioSource>();
        instance = this;
    }

    public void HandleInputs()
    {
        if (Input.GetKey(KeyCode.R))
        {
            Respawn();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            droneParameters.stabilization = !droneParameters.stabilization;
        }
    }
    bool rollPressed;
    bool pitchPressed;
    public void SetThrottle(float _v)
    {
        droneParameters.vThrottle += _v * droneParameters.throttleIncrement;
        droneParameters.vThrottle = Mathf.Clamp(droneParameters.vThrottle, -droneParameters.maxThrottle, droneParameters.maxThrottle);
        droneUI.throttleText.text = Mathf.FloorToInt(droneParameters.vThrottle * droneUI.throttleValueScale).ToString();
    }
    public void SetRoll(float _v)
    {
        droneParameters.vRoll = _v;
        rollPressed = _v > 0;
    }
    public void SetPitch(float _v)
    {
        droneParameters.vPitch = _v;
        pitchPressed = _v > 0;
    }
    public void SetYaw(float _v)
    {
        droneParameters.vYaw = _v;
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
        droneParameters.vThrottle = 0;
        droneParts.droneTimer.ReloadClock();
    }
    private void FixedUpdate()
    {
        UpdateMoovingParameters();

        UpdatePhysics();

        UpdateUI();

    }
    private void UpdatePhysics()
    { 
        droneParts.FRRot = droneParameters.vThrottle + droneParameters.vPitch * droneParameters.pitch - droneParameters.vRoll * droneParameters.roll;
        droneParts.FLRot = droneParameters.vThrottle + droneParameters.vPitch * droneParameters.pitch + droneParameters.vRoll * droneParameters.roll;
        droneParts.BRRot = droneParameters.vThrottle - droneParameters.vPitch * droneParameters.pitch - droneParameters.vRoll * droneParameters.roll;
        droneParts.BLRot = droneParameters.vThrottle - droneParameters.vPitch * droneParameters.pitch + droneParameters.vRoll * droneParameters.roll;


        if (droneParameters.stabilization)
        {
            if (!rollPressed)
            {
                droneParts.FRRot -= droneGyroscope.Roll * droneParameters.stabilizationForce;
                droneParts.BRRot -= droneGyroscope.Roll * droneParameters.stabilizationForce;
            }
            if (!pitchPressed)
            {
                droneParts.FRRot -= droneGyroscope.Pitch * droneParameters.stabilizationForce;
                droneParts.FLRot -= droneGyroscope.Pitch * droneParameters.stabilizationForce;
            }
        }

        float maxValue = droneParameters.maxThrottle + droneParameters.pitch + droneParameters.roll;
        float yawAbs = Mathf.Abs(droneParameters.vYaw) * 0.2f;
        droneParts.propellerBRSound.volume = Mathf.Clamp(Mathf.Abs(droneParts.BRRot) / maxValue + yawAbs, 0f, 1f);
        droneParts.propellerBLSound.volume = Mathf.Clamp(Mathf.Abs(droneParts.BLRot) / maxValue + yawAbs, 0f, 1f);
        droneParts.propellerFRSound.volume = Mathf.Clamp(Mathf.Abs(droneParts.FRRot) / maxValue + yawAbs, 0f, 1f);
        droneParts.propellerFLSound.volume = Mathf.Clamp(Mathf.Abs(droneParts.FLRot) / maxValue + yawAbs, 0f, 1f);

        droneParts.propellerBRSound.pitch = 0.7f + droneParts.propellerBRSound.volume * 0.4f;
        droneParts.propellerBLSound.pitch = 0.7f + droneParts.propellerBLSound.volume * 0.4f;
        droneParts.propellerFRSound.pitch = 0.7f + droneParts.propellerFRSound.volume * 0.4f;
        droneParts.propellerFLSound.pitch = 0.7f + droneParts.propellerFLSound.volume * 0.4f;


        rig.AddForceAtPosition(transform.up * droneParts.FRRot, droneParts.propellerFR.position);
        rig.AddForceAtPosition(transform.up * droneParts.FLRot, droneParts.propellerFL.position);


        rig.AddForceAtPosition(transform.up * droneParts.BRRot, droneParts.propellerBR.position);
        rig.AddForceAtPosition(transform.up * droneParts.BLRot, droneParts.propellerBL.position);

        rig.AddTorque(transform.up * droneParameters.vYaw * droneParameters.yaw);
    }
    private void UpdateMoovingParameters()
    {
        droneSpeed.magnitude = rig.velocity.magnitude;

        droneParts.gyroscope.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        float distRollR = Vector3.Distance(transform.up, droneParts.gyroscope.right);
        float distRollL = Vector3.Distance(transform.up, -droneParts.gyroscope.right);

        droneGyroscope.Roll = Vector3.Distance(transform.right, droneParts.gyroscope.right) / 2f * (distRollR > distRollL ? 1f : -1f);

        float distPitchR = Vector3.Distance(transform.up, droneParts.gyroscope.forward);
        float distPitchL = Vector3.Distance(transform.up, -droneParts.gyroscope.forward);

        droneGyroscope.Pitch = Vector3.Distance(transform.forward, droneParts.gyroscope.forward) / 2f * (distPitchR > distPitchL ? 1f : -1f);
    }
    private void UpdateUI()
    {
        droneUI.speedText.text = Mathf.FloorToInt(droneSpeed.magnitude * droneUI.speedValueScale).ToString();
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