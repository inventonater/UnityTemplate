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
    /// Handles the visual and audio components for the biplane.
    /// </summary>
    [RequireComponent(typeof(BiplaneAudioBehavior))]
    public class BiplanePresentation : MonoBehaviour
    {
        // ------ Private Members ------

        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject _vehicleLocator;
        [SerializeField] private GameObject _explosionPrefab;
        [SerializeField] private string _pitchParamName = "Pitch";
        [SerializeField] private string _rollParamName = "Roll";
        [SerializeField] private string _speedParamName = "Speed";
        [SerializeField] private GameObject _trailRenderer;

        private BiplaneAudioBehavior _audioBehavior;
        private bool _engineIdle = true;

        private const float DestroyExplosionDelay = 10f;

        // ------ MonoBehaviour Methods ------

        private void Awake()
        {
            _audioBehavior = GetComponent<BiplaneAudioBehavior>();
        }

        //----------- Public Methods -----------

        /// <summary>
        /// Update the animator to match the current pitch.
        /// </summary>
        /// <param name="value">Pitch value</param>
        public void SetPitch(float value)
        {
            _animator.SetFloat(_pitchParamName, value);
        }

        /// <summary>
        /// Update the animator to match the current roll.
        /// </summary>
        public void SetRoll(float value)
        {
            _animator.SetFloat(_rollParamName, value);
        }

        /// <summary>
        /// Update the audio to match the current stalled state.
        /// </summary>
        /// <param name="status">True if engine is not triggered/actively used</param>
        public void SetEngineStalled(bool status)
        {
            if(status == _engineIdle)
            {
                return;
            }
            _engineIdle = status;
            if (status)
            {
                _audioBehavior.PlayEngineActive(false);
            }
            else
            {
                _audioBehavior.PlayEngineActive(true);
            }
        }

        /// <summary>
        /// Play the sfx for the engine starting.
        /// </summary>
        public void StartEngine()
        {
            if(_audioBehavior == null)
            {
                return;
            }
            _audioBehavior.EngineStart();
        }

        /// <summary>
        /// Update the animator and audio to match the current engine power.
        /// </summary>
        /// <param name="value">Engine power value</param>
        public void UpdateEnginePower(float value)
        {
            _animator.SetFloat(_speedParamName, value);
            _audioBehavior.SetEnginePower(value);
        }

        /// <summary>
        /// Spawn explosion vfx and play explosion sfx.
        /// </summary>
        public void Explode()
        {
            GameObject explosion = Instantiate(_explosionPrefab, transform.position, transform.rotation);
            Destroy(explosion, DestroyExplosionDelay);
        }

        /// <summary>
        /// Toggles the directional "off-screen" indicator for the vehicle on/off.
        /// </summary>
        /// <param name="status">True if vehicle of out of FOV</param>
        public void SetVehicleLocator(bool status)
        {
            _vehicleLocator.SetActive(status);
        }

        /// <summary>
        /// Update the vehicle's flight status.
        /// </summary>
        /// <param name="status">True if vehicle is flying/not on controller</param>
        public void SetVehicleFlying(bool status)
        {
            _audioBehavior.flying = status;
            _trailRenderer.SetActive(status);
        }
    }
}
