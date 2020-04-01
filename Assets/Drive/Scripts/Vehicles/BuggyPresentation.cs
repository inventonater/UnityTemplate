// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;

namespace MagicKit
{
    /// <summary>
    /// Handles the visual and audio components for the Buggy.
    /// </summary>
    [RequireComponent(typeof(BuggyMovement))]
    [RequireComponent(typeof(BuggyAudioBehavior))]
    public class BuggyPresentation : MonoBehaviour
    {
        // ------ Private Members ------

        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _vehicleLocator;
        [SerializeField] private ParticleSystem[] _wheelParticles = new ParticleSystem[4];
        [SerializeField] private GameObject[] _wheelMeshes = new GameObject[4];

        private BuggyAudioBehavior _audioBehavior;

        // ------ MonoBehaviour Methods ------

        private void Awake()
        {
            _audioBehavior = GetComponent<BuggyAudioBehavior>();
        }

        private void OnEnable()
        {
            _audioBehavior.EngineStart();
        }

        private void OnDisable()
        {
            _audioBehavior.EngineStop();
        }

        //----------- Public Methods -----------

        /// <summary>
        /// Handles particle effects as well as audio for tires spinning out
        /// </summary>
        public void TireSlipping(bool status, int tireNum)
        {
            _audioBehavior.PlayTireScreech(status, tireNum);
            if(status)
            {
                _wheelParticles[tireNum].Play();
            }
            else
            {
                _wheelParticles[tireNum].Stop();
            }
        }

        /// <summary>
        /// Uses steerValue and speed to handle various animations on the buggy
        /// </summary>
        public void SetAnimationState(float steerValue, float speed)
        {
            _animator.SetFloat("steer", steerValue);
            _animator.SetFloat("speed", speed);
        }

        /// <summary>
        /// Sets a tire's position and rotation
        /// </summary>
        public void SetTireTransform(Vector3 position, Quaternion rotation, int tireNum)
        {
            _wheelMeshes[tireNum].transform.position = position;
            _wheelMeshes[tireNum].transform.rotation = rotation;
        }

        /// <summary>
        /// Updates the pitch for the EngineRev sfx
        /// </summary>
        public void UpdateAudioPitch(float engineRev)
        {
            _audioBehavior.SetEnginePower(engineRev);
        }

        /// <summary>
        /// Plays bump SFX
        /// </summary>
        public void PlayBump()
        {
            _audioBehavior.PlayBump();
        }

        /// <summary>
        /// Plays engine reverse SFX
        /// </summary>
        public void PlayReverse()
        {
            _audioBehavior.PlayEngineReverse();
        }

        /// <summary>
        /// Plays Fast Turn SFX
        /// </summary>
        public void PlayFastTurn()
        {
            _audioBehavior.PlayFastTurn(true);
        }

        /// <summary>
        /// Toggles the directional "off-screen" indicator for the vehicle on/off.
        /// </summary>
        public void SetVehicleLocator(bool status)
        {
            _vehicleLocator.SetActive(status);
        }
    }
}