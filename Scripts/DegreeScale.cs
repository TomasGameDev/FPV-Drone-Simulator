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
        public float Y = 0;
        private void Start()
        {

        }
        void Update()
        {
            Y = target.transform.eulerAngles.y;

            degreeScale[0].anchoredPosition = new Vector2(1f / 360f * -Y * Screen.width, 0);
        }
    }
}