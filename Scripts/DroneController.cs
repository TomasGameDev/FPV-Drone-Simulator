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

        public RectTransform horizontalLine;

        public RectTransform[] lines;
        public int linesPosPixelStep = 15;
        public RectTransform[] linesPoses;


        public Transform cameraPos;
    }
    [Space(10)]
    public DroneParameters droneParameters;
    [System.Serializable]
    public class DroneParameters
    {
        public bool _isWork;
        public bool isWork
        {
            get { return _isWork; }
            set
            {
                _isWork = value;
                instance.droneUI.droneWorkText.SetActive(!value);
                instance.rig.angularDrag = _isWork ? 14f : 2.5f;
            }
        }
        public bool _stabilization;
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

        public float rollStabilization = 30f;
        public float pitchStabilization = 30f;
        public float yawStabilization = 30f;

        public float gravityResistMultiplier = 30f;

        public float vThrottle;
        public float vRoll;
        public float vPitch;
        public float vYaw;

        public float hoverHeight = 20f;
        public float hoverForce = 5;

        public float cameraAngle;
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
        public GameObject droneWorkText;
    }
    [Space(10)]

    public DroneGyroscope droneGyroscope;
    [System.Serializable]
    public class DroneGyroscope
    {
        public float Roll;
        public float RollDelta;

        public float Pitch;
        public float PitchDelta;

        public float YawDelta;
    }

    [Space(10)]
    [Header("Other")]
    [Space(10)]
    public Transform sapawnpoint;

    Rigidbody rig;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        droneParts.propellerBRSound = droneParts.propellerBR.GetComponent<AudioSource>();
        droneParts.propellerBRSound.time = Random.Range(0, droneParts.propellerBRSound.clip.length);

        droneParts.propellerBLSound = droneParts.propellerBL.GetComponent<AudioSource>();
        droneParts.propellerBLSound.time = Random.Range(0, droneParts.propellerBLSound.clip.length);

        droneParts.propellerFRSound = droneParts.propellerFR.GetComponent<AudioSource>();
        droneParts.propellerFRSound.time = Random.Range(0, droneParts.propellerFRSound.clip.length);

        droneParts.propellerFLSound = droneParts.propellerFL.GetComponent<AudioSource>();
        droneParts.propellerFLSound.time = Random.Range(0, droneParts.propellerFLSound.clip.length);

        instance = this;

        droneParameters.stabilization = droneParameters.stabilization;

        droneParameters.isWork = droneParameters.isWork;

        Respawn();
    }

    public void ToggleStabilization()
    {
        droneParameters.stabilization = !droneParameters.stabilization;
    }

    public void HandleInputs()
    {
        if (Input.GetKey(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick1Button9))
        {
            Respawn();
        }
        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            ToggleStabilization();
        }
        if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.Joystick1Button8))
        {
            droneParameters.isWork = !droneParameters.isWork;
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            droneParameters.vThrottle = Physics.gravity.magnitude / 4f * rig.mass;
        }
    }
    bool rollPressed;
    bool pitchPressed;
    public void SetThrottle(float _v)
    {
        droneParameters.vThrottle = _v * droneParameters.maxThrottle;
        //droneParameters.vThrottle += _v * droneParameters.throttleIncrement;
        //droneParameters.vThrottle = Mathf.Clamp(droneParameters.vThrottle, -droneParameters.maxThrottle, droneParameters.maxThrottle);
        droneUI.throttleText.text = Mathf.FloorToInt(droneParameters.vThrottle * droneUI.throttleValueScale).ToString();
    }
    public void SetRoll(float _v)
    {
        droneParameters.vRoll = _v;
        rollPressed = _v != 0;
    }
    public void SetPitch(float _v)
    {
        droneParameters.vPitch = _v;
        pitchPressed = _v != 0;
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
        transform.position = sapawnpoint.position;
        transform.rotation = sapawnpoint.rotation;
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
        droneSpeed.forwardSpeed = droneSpeed.magnitude = 0;
        droneParameters.vThrottle = 0;
        droneParts.droneTimer.ReloadClock();

        droneGyroscope.Pitch = 0;
        droneGyroscope.Roll = 0;
        droneGyroscope.PitchDelta = 0;
        droneGyroscope.RollDelta = 0;
        droneGyroscope.YawDelta = 0;
    }
    private void FixedUpdate()
    {
        UpdateMoovingParameters();

        UpdatePhysics();

        UpdateUI();

        if (droneParameters.cameraAngle != 0)
        {
            droneParts.cameraPos.localRotation = Quaternion.Euler(droneParameters.cameraAngle, 0, 0);
        }
    }
    private void UpdatePhysics()
    {
        if (!droneParameters.isWork)
        {
            droneParameters.vPitch = 0;
            droneParameters.vRoll = 0;
            droneParameters.vYaw = 0;
            if (droneParameters.vThrottle > droneParameters.throttleIncrement || droneParameters.vThrottle < -droneParameters.throttleIncrement)
                droneParameters.vThrottle += droneParameters.throttleIncrement * (droneParameters.vThrottle > 0 ? -1f : 1f);
            else droneParameters.vThrottle = 0;
        }

        if (droneParameters.vPitch == 0) droneParameters.vPitch = droneGyroscope.PitchDelta * droneParameters.pitchStabilization;
        if (droneParameters.vRoll == 0) droneParameters.vRoll = droneGyroscope.RollDelta * droneParameters.rollStabilization;
        if (droneParameters.vYaw == 0) droneParameters.vYaw = droneGyroscope.YawDelta * droneParameters.yawStabilization;

        if (droneParameters.vThrottle != 0 && (
            (droneParameters.vThrottle > 0 && rig.velocity.y < 0) ||
            (droneParameters.vThrottle < 0 && rig.velocity.y > 0)))
        {
            rig.velocity += Vector3.up * (droneParameters.vThrottle + rig.velocity.y * droneParameters.gravityResistMultiplier) * Time.deltaTime; // Calculate up speed for this
        }

        droneParts.FRRot = droneParameters.vThrottle + droneParameters.vPitch * droneParameters.pitch - droneParameters.vRoll * droneParameters.roll;
        droneParts.FLRot = droneParameters.vThrottle + droneParameters.vPitch * droneParameters.pitch + droneParameters.vRoll * droneParameters.roll;
        droneParts.BRRot = droneParameters.vThrottle - droneParameters.vPitch * droneParameters.pitch - droneParameters.vRoll * droneParameters.roll;
        droneParts.BLRot = droneParameters.vThrottle - droneParameters.vPitch * droneParameters.pitch + droneParameters.vRoll * droneParameters.roll;


        if (droneParameters.stabilization && droneParameters.isWork)
        {
            if (!rollPressed)
            {
                droneParts.FRRot -= droneGyroscope.Roll * droneParameters.stabilizationForce;
                droneParts.BRRot -= droneGyroscope.Roll * droneParameters.stabilizationForce;

                droneParts.FLRot += droneGyroscope.Roll * droneParameters.stabilizationForce;
                droneParts.BLRot += droneGyroscope.Roll * droneParameters.stabilizationForce;
            }
            if (!pitchPressed)
            {
                droneParts.FRRot -= droneGyroscope.Pitch * droneParameters.stabilizationForce;
                droneParts.FLRot -= droneGyroscope.Pitch * droneParameters.stabilizationForce;

                droneParts.BRRot += droneGyroscope.Pitch * droneParameters.stabilizationForce;
                droneParts.BLRot += droneGyroscope.Pitch * droneParameters.stabilizationForce;
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

        float propellerFRHover = 0;
        float propellerFLHover = 0;
        float propellerBRHover = 0;
        float propellerBLHover = 0;
        for (int p = 0; p < 4; p++)
        {
            Ray ray = new Ray();

            switch (p)
            {
                case 0:
                    ray.origin = droneParts.propellerFR.position;
                    break;
                case 1:
                    ray.origin = droneParts.propellerFL.position;
                    break;
                case 2:
                    ray.origin = droneParts.propellerBR.position;
                    break;
                case 3:
                    ray.origin = droneParts.propellerBL.position;
                    break;
            }
            ray.direction = -transform.up;
            Color rayColor = Color.white;
            if (Physics.Raycast(ray, out RaycastHit hit, droneParameters.hoverHeight))
            {
                float dist = 1f * droneParameters.hoverHeight / Vector3.Distance(ray.origin, hit.point);
                switch (p)
                {
                    case 0:
                        propellerFRHover = dist;
                        break;
                    case 1:
                        propellerFLHover = dist;
                        break;
                    case 2:
                        propellerBRHover = dist;
                        break;
                    case 3:
                        propellerBLHover = dist;
                        break;
                }
                rayColor = Color.yellow;
            }
            Debug.DrawRay(ray.origin, ray.direction * droneParameters.hoverHeight, rayColor);
        }

        rig.AddForceAtPosition(transform.up * (droneParts.FRRot + propellerFRHover * (droneParts.FRRot > 0 ? droneParameters.hoverForce : 0)), droneParts.propellerFR.position);
        rig.AddForceAtPosition(transform.up * (droneParts.FLRot + propellerFLHover * (droneParts.FLRot > 0 ? droneParameters.hoverForce : 0)), droneParts.propellerFL.position);


        rig.AddForceAtPosition(transform.up * (droneParts.BRRot + propellerBRHover * (droneParts.BRRot > 0 ? droneParameters.hoverForce : 0)), droneParts.propellerBR.position);
        rig.AddForceAtPosition(transform.up * (droneParts.BLRot + propellerBLHover * (droneParts.BLRot > 0 ? droneParameters.hoverForce : 0)), droneParts.propellerBL.position);

        rig.AddTorque(transform.up * droneParameters.vYaw * droneParameters.yaw);
    }
    private void UpdateMoovingParameters()
    {
        droneSpeed.magnitude = rig.velocity.magnitude;

        // droneGyroscope.YawDelta = 0;//?

        droneParts.gyroscope.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        float distRollR = Vector3.Distance(transform.up, droneParts.gyroscope.right);
        float distRollL = Vector3.Distance(transform.up, -droneParts.gyroscope.right);

        droneGyroscope.RollDelta = droneGyroscope.Roll;
        droneGyroscope.Roll = Vector3.Distance(transform.right, droneParts.gyroscope.right) / 2f * (distRollR > distRollL ? 1f : -1f);
        droneGyroscope.RollDelta = droneGyroscope.Roll - droneGyroscope.RollDelta;

        float distPitchR = Vector3.Distance(transform.up, droneParts.gyroscope.forward);
        float distPitchL = Vector3.Distance(transform.up, -droneParts.gyroscope.forward);

        droneGyroscope.PitchDelta = droneGyroscope.Pitch;
        droneGyroscope.Pitch = Vector3.Distance(transform.forward, droneParts.gyroscope.forward) / 2f * (distPitchR > distPitchL ? 1f : -1f);
        droneGyroscope.PitchDelta = droneGyroscope.PitchDelta - droneGyroscope.Pitch;


        droneParts.horizontalLine.anchoredPosition = new Vector2(0, -droneGyroscope.Pitch * Screen.height / 3f);
        droneParts.horizontalLine.rotation = Quaternion.Euler(0, 0, -transform.eulerAngles.z);

        for (int l = 0; l < droneParts.linesPoses.Length; l++)
        {
            droneParts.lines[l].position = new Vector2(
                Mathf.RoundToInt(droneParts.linesPoses[l].position.x / droneParts.linesPosPixelStep) * droneParts.linesPosPixelStep,
                Mathf.RoundToInt(droneParts.linesPoses[l].position.y / droneParts.linesPosPixelStep) * droneParts.linesPosPixelStep);
            //droneParts.lines[l].rotation = Quaternion.Euler(0, 0, -transform.eulerAngles.z);
        }
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
