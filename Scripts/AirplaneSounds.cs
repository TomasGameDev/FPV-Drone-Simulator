using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoysAirplane
{
    public class AirplaneSounds : MonoBehaviour
    {
        DroneController airplaneController;
        AudioSource audioSource;

        public AnimationCurve windVolume;
        private void Start()
        {
            airplaneController = GetComponent<DroneController>();
            audioSource = GetComponent<AudioSource>();
        }
        private void Update()
        {
            audioSource.volume = windVolume.Evaluate(airplaneController.droneSpeed.magnitude);
        }
    }
}