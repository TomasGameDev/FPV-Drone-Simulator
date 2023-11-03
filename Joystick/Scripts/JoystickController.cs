using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour
{
    public GameObject controllableObject;
     IJoystickContrillable controllableObjectInterface;
    public Color enabledButtonColor;
    public Color disabledButtonColor;

    public float _joystickSize = 150;
    public float joystickSize
    {
        get
        {
            return _joystickSize;
        }
        set
        {
            _joystickSize = value;

            joystickPos.sizeDelta = Vector2.one * _joystickSize * 2f;

            jArrowsTransform[0].anchoredPosition = Vector2.up * _joystickSize;
            jArrowsTransform[1].anchoredPosition = Vector2.down * _joystickSize;
            jArrowsTransform[2].anchoredPosition = Vector2.right * _joystickSize;
            jArrowsTransform[3].anchoredPosition = Vector2.left * _joystickSize;
            for (int jA = 0; jA < jArrowsTransform.Length; jA++) jArrowsTransform[jA].sizeDelta = Vector2.one * (1f / jProportionConst * _joystickSize) * 140f;
        }
    }
    public bool dotAutoSize;
    public float _dotSize = 80;
    public float dotSize
    {
        get
        {
            return _dotSize;
        }
        set
        {
            if (!dotAutoSize)
                _dotSize = value;
            else
                _dotSize = (1f / jProportionConst * _joystickSize) * 80f;
            dotPos.sizeDelta = Vector2.one * _dotSize;
        }
    }
    const float jProportionConst = 150f;

    public float _maximizedZoneRange = 0.3f;
    public float maximizedZoneRange
    {
        get
        {
            return _maximizedZoneRange;
        }
        set
        {
            _maximizedZoneRange = value;
            Vector2 _anchorMin = Vector2.one * (1f - value) * 0.5f;
            maximizedZoneImage.GetComponent<RectTransform>().anchorMin = _anchorMin;
            maximizedZoneImage.GetComponent<RectTransform>().anchorMax = new Vector2(1f - _anchorMin.x, 1f - _anchorMin.y);
        }
    }

    public float willSpeed;
    public float moveX;
    public float moveZ;

    private bool _isLockJoystick;
    public Image lockJoystickButton;
    public bool isLockJoystick
    {
        get
        {
            return _isLockJoystick;
        }
        set
        {
            _isLockJoystick = value;
            lockJoystickButton.color = value ? enabledButtonColor : disabledButtonColor;
        }
    }
    public void LockJoystick()
    {
        isLockJoystick = !_isLockJoystick;
    }

    private bool _isDirectionalJoystick;
    public Image directionalJoystickImage;
    public Image directionalJoystickButton;
    public bool isDirectionalJoystick
    {
        get
        {
            return _isDirectionalJoystick;
        }
        set
        {
            directionalJoystickImage.enabled = _isDirectionalJoystick = value;
            directionalJoystickButton.color = value ? enabledButtonColor : disabledButtonColor;
            CheckForJArrows();
        }
    }
    public void SetDirectionalJoystick()
    {
        isDirectionalJoystick = !_isDirectionalJoystick;
    }

    private bool _isLockJoystickMove;
    public Image isLockJoystickMoveButton;
    public bool isLockJoystickMove
    {
        get
        {
            return _isLockJoystickMove;
        }
        set
        {
            _isLockJoystickMove = value;
            isLockJoystickMoveButton.color = value ? enabledButtonColor : disabledButtonColor;
            CheckForJArrows();
            if (!value) OnEndMove();
        }
    }
    public void LockJoystickMove()
    {
        isLockJoystickMove = !isLockJoystickMove;
    }

    private bool _isMaximizedJoystick;
    public Image maximizedZoneImage;
    public Image maximizedJoystickButton;
    public bool isMaximizedJoystic
    {
        get
        {
            return _isMaximizedJoystick;
        }
        set
        {
            _isMaximizedJoystick = maximizedZoneImage.enabled = value;
            maximizedJoystickButton.color = value ? enabledButtonColor : disabledButtonColor;
            CheckForJArrows();
        }
    }
    public void MaximizeJoystick()
    {
        isMaximizedJoystic = !_isMaximizedJoystick;
    }

    private bool _joystickSettings;
    public GameObject joystickSettingsPanel;
    public void JoystickSettings()
    {
        _joystickSettings = !_joystickSettings;
        joystickSettingsPanel.SetActive(_joystickSettings);
    }

    public GameObject jArrowsObject;
    [Tooltip("\nElement 0 - Up\nElement 1 - Down\nElement 2 - Right\nElement 3 - Left")] public RectTransform[] jArrowsTransform;

    void CheckForJArrows()
    {
        jArrowsObject.SetActive(_isMaximizedJoystick && _isDirectionalJoystick && _isLockJoystickMove);
    }

    public GameObject joystick;
    public GameObject dot;
    RectTransform joystickField;
    RectTransform joystickPos;
    RectTransform dotPos;
    Image dotImage;
    Image joystickImage;
    Vector2 locPos;
    public Vector2 joystickPosition = Vector2.one * 200;

    void Start()
    {
        LinkJoystick(controllableObject);
        joystickField = GetComponent<RectTransform>();
        joystickPos = joystick.GetComponent<RectTransform>();
        dotPos = dot.GetComponent<RectTransform>();
        joystickImage = joystick.GetComponent<Image>();
        dotImage = dot.GetComponent<Image>();
        joystickSize = _joystickSize;
        dotSize = _dotSize;
        maximizedZoneRange = _maximizedZoneRange;

    }
    bool isJPress;
    public void OnJPress(bool _isJPress)
    {
        isJPress = _isJPress;
        Vector2 touchPos = GetJTouchPos();
        if (_isJPress)
        {
            if (touchPos == Vector2.zero)
            {
                isJPress = false;
                return;
            }
            if (!_isLockJoystickMove)
                dotPos.position = joystickPos.position = touchPos;
            Moving(touchPos);
        }
        else
        {
            OnEndMove();
        }
    }
    Vector2 touch = Vector2.zero;
    public Vector3 GetJTouchPos()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.touches[i].position.x > Screen.width * joystickField.anchorMin.x && Input.touches[i].position.x < Screen.width * joystickField.anchorMax.x &&
                Input.touches[i].position.y > Screen.height * joystickField.anchorMin.y && Input.touches[i].position.y < Screen.height * joystickField.anchorMax.y)
                return touch = Input.touches[i].position;
        }
        return touch;
    }
    void Update()
    {
        if(controllableObjectInterface==null) LinkJoystick(controllableObject);
        Vector2 touch = Vector3.zero;
        if (isJPress)
        {
            touch = GetJTouchPos();
            Moving(touch);
        }

        //#if UNITY_EDITOR
        // if (UnityEngine.Device.SystemInfo.deviceType != DeviceType.Desktop) return;
        Vector2 velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            velocity += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += Vector2.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            velocity += Vector2.up;
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity += Vector2.down;
        }
        if (velocity == Vector2.zero)
        {
            if(touch == Vector2.zero)
                OnEndMove();
        }
        else
        {
            if (!_isLockJoystickMove) joystickPos.position = joystickPosition;
            Moving(new Vector2(joystickPos.position.x, joystickPos.position.y) + velocity * _joystickSize);
        }
        //#endif
    }

    public void OnEndMove()
    {
        if (_isLockJoystick) return;
        moveX = 0;
        moveZ = 0;
        dotPos.position = joystickPos.position;
        dotImage.color = joystickImage.color = new Color(1, 1, 1, 0.05f);
        dotPos.sizeDelta = new Vector2(_dotSize, _dotSize);
        controllableObjectInterface.Move(0, 0, 0);
    }
    public float maxJoystickAlpha = 0.2f;
    public float maxJoystickDotAlpha = 1f;
    public void Moving(Vector2 _pos)
    {
        locPos = new Vector2(_pos.x - joystickPos.position.x, _pos.y - joystickPos.position.y); // LOCAL POSITION
        if (_isDirectionalJoystick)
        {
            if (Mathf.Abs(locPos.x) > Mathf.Abs(locPos.y)) locPos.y = 0;
            else locPos.x = 0;
        }

        moveX = locPos.x * (1f / _joystickSize);

        moveZ = locPos.y * (1f / _joystickSize);

        willSpeed = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(moveX), 2) + Mathf.Pow(Mathf.Abs(moveZ), 2)); // joystick hypotenuse

        if ((willSpeed >= 1 || _isMaximizedJoystick) && willSpeed > 0)
        {
            if (willSpeed > maximizedZoneRange)
            {
                moveX /= willSpeed;
                moveZ /= willSpeed;
                willSpeed = 1;
            }
            else
            {
                moveX = 0;
                moveZ = 0;
                willSpeed = 0;
            }
        }

        if (willSpeed < 1)
        {

            Color _joystickColor = new Color(1, 1, 1, willSpeed * maxJoystickAlpha);
            joystickImage.color = _joystickColor;
            if (_isDirectionalJoystick) directionalJoystickImage.color = _joystickColor;
            dotImage.color = new Color(1, 1, 1, willSpeed * maxJoystickDotAlpha);
            dotPos.sizeDelta = new Vector2(_dotSize + ((_dotSize / 5f) * willSpeed), _dotSize + ((_dotSize / 5f) * willSpeed));
        }
        else
        {
            Color _joystickColor = new Color(1, 1, 1, maxJoystickAlpha);
            joystickImage.color = _joystickColor;
            if (_isDirectionalJoystick) directionalJoystickImage.color = _joystickColor;
            dotImage.color = new Color(1, 1, 1, maxJoystickDotAlpha);
        }
        dotPos.position = new Vector2(moveX * _joystickSize + joystickPos.position.x, moveZ * _joystickSize + joystickPos.position.y);
        controllableObjectInterface.Move(moveX, moveZ, willSpeed);
    }

    public void LinkJoystick(GameObject _controllableObject)
    {
        controllableObject = _controllableObject;
        controllableObjectInterface = controllableObject.GetComponent<IJoystickContrillable>();
        controllableObjectInterface.LinkJoyStick(GetComponent<JoystickController>());
    }
}
public interface IJoystickContrillable
{
    public void LinkJoyStick(JoystickController _scr_JoystickController);
    public void Move(float _moveX, float _moveZ, float _willSpeed); //axis x, z; distance from the center of the joystick
}