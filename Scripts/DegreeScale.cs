using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoysAirplane
{
    public class DegreeScale : MonoBehaviour
    {
        public Transform target;
        public RectTransform[] degreeScale;  
        void Update()
        {
            degreeScale[0].anchoredPosition = new Vector2(1f / 180f * -target.transform.eulerAngles.y * GetComponent<RectTransform>().sizeDelta.x + GetComponent<RectTransform>().sizeDelta.x, 0);
        }
    }
}