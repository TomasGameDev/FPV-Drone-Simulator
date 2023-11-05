using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
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

    public bool _isLockJoystick;
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

    public bool _isDirectionalJoystick;
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

    public bool _isLockJoystickMove;
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

    public bool _isMaximizedJoystick;
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
    public string axisXName = "Horizontal";
    public string axisYName = "Vertical";
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
    void Update()
    {
        if (controllableObjectInterface == null) LinkJoystick(controllableObject);

        if (!isJPress)
        {
            Vector2 velocity = Vector2.zero;
            velocity += Vector2.right * Input.GetAxis(axisXName);
            velocity += Vector2.up * Input.GetAxis(axisYName);
            if (velocity == Vector2.zero)
            {
                OnEndMove();
            }
            else
            {
                if (_isLockJoystickMove) joystickPos.anchoredPosition = Vector2.zero;
                Moving(new Vector2(joystickPos.position.x, joystickPos.position.y) + velocity * _joystickSize);
            }
        }
    }

    public void OnEndMove()
    {
        if (_isLockJoystick) return;
        moveX = 0;
        moveZ = 0;
        dotPos.position = joystickPos.position;
        dotImage.color = joystickImage.color = new Color(1, 1, 1, 0.05f);
        dotPos.sizeDelta = new Vector2(_dotSize, _dotSize);
        controllableObjectInterface.SetAxis(axisXName, 0);
        controllableObjectInterface.SetAxis(axisYName, 0);
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
        controllableObjectInterface.SetAxis(axisXName, moveX);
        controllableObjectInterface.SetAxis(axisYName, moveZ);
    }

    public void LinkJoystick(GameObject _controllableObject)
    {
        controllableObject = _controllableObject;
        controllableObjectInterface = controllableObject.GetComponent<IJoystickContrillable>();
        controllableObjectInterface.LinkJoyStick(GetComponent<JoystickController>());
    }

    bool isJPress;
    public void OnDrag(PointerEventData eventData)
    {
        Moving(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isJPress = true;
        if (!_isLockJoystickMove) joystickPos.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isJPress = false;
        OnEndMove();
    }
}
public interface IJoystickContrillable
{
    public void LinkJoyStick(JoystickController _scr_JoystickController);
    public void SetAxis(string axisName, float value);
}