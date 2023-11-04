using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoysAirplane
{
    public class WindSounds : MonoBehaviour
    {
        DroneController airplaneController;
        [Range(0f, 1f)] public float windForce;
        AudioSource audioSource;

        public AnimationCurve windVolume;
        private void Start()
        {
            airplaneController = GetComponent<DroneController>();
            audioSource = GetComponent<AudioSource>();
        }
        private void Update()
        {
            audioSource.volume = windVolume.Evaluate(airplaneController.droneSpeed.magnitude) + windForce;
            audioSource.volume = Mathf.Clamp(audioSource.volume, 0, 1f);
        }
    }
}