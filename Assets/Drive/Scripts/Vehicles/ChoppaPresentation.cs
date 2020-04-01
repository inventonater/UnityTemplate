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
    /// Handles the visual and audio components for da choppa.
    /// </summary>
    [RequireComponent(typeof(ChoppaAudioBehavior))]
    public class ChoppaPresentation : MonoBehaviour
    {
        // ------ Private Members ------

        [SerializeField] private GameObject _vehicleLocator;
        [SerializeField] private GameObject _explosionPrefab;
        [SerializeField] private string _speedParamName = "Speed";
        [SerializeField] private Animator _animator;
        [SerializeField] private ChoppaAudioBehavior _audioBehavior;
        [SerializeField] private GameObject _trailRenderer;

        private bool _engineActive;

        private const float DestroyExplosionDelay = 10f;

        //----------- Public Methods -----------

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
        /// Update the audio to match the current engine state.
        /// </summary>
        /// <param name="status">True if engine is active/on</param>
        public void SetEngineActive(bool status)
        {
            if(_engineActive == status)
            {
                return;
            }
            _engineActive = status;
            if (_engineActive)
            {
                _audioBehavior.EngineStart();
                _audioBehavior.flying = true;
            }
            else
            {
                _audioBehavior.EngineStop();
                _audioBehavior.flying = false;
            }
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
        /// <param name="status">True if vehicle is out of FOV</param>
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
