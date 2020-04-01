// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using MagicLeap.Utilities;
using System.Collections;

namespace MagicKit
{
    /// <summary>
    /// Handles all audio for all flying vehicles (Base Class).
    /// </summary>
    public class FlyingVehicleAudioBehavior : VehicleAudioBehavior
    {
        //----------- Public Members -----------

        public bool flying = false;

        //----------- Private Members -----------

        private const string FlybyKey = "flyby";

        //Time in seconds from the start of the audio clip to the peak whoosh sound.
        [SerializeField] private float _flybyTimeToPeak = 0.562f;
        //How long should we wait before triggering the next passby on this object.
        [SerializeField] private float _flybyCooldown = 5f;

        private Vector3 _lastPosition;
        private float _lastDistanceToListener;
        private float _flybyTimer;
        private AudioListener _audioListener;

        private void Start()
        {
            _lastPosition = transform.position;
            _audioListener = (AudioListener)FindObjectOfType(typeof(AudioListener));
        }

        private void Update()
        {
            // Decrement cooldown timer as needed.
            if (_flybyTimer > 0f)
            {
                _flybyTimer -= Time.deltaTime;
            }

            // Am I moving towards the listener?
            float distanceToListener = Vector3.Distance(transform.position, _audioListener.transform.position);
            bool movingTowardsListener = (distanceToListener < _lastDistanceToListener);
            _lastDistanceToListener = distanceToListener;

            // What is my velocity?
            float velocity = (transform.position - _lastPosition).magnitude / Time.deltaTime;
            _lastPosition = transform.position;

            // Find the distance at which we need to trigger the passby sound.
            float passbyDistance = _flybyTimeToPeak * velocity;

            // Trigger passby if:
            //  - flying
            //  - moving towards listener
            //  - at the right distance to "whoosh" at the right time
            //  - cooldown is finished
            if (flying && movingTowardsListener && (_lastDistanceToListener <= passbyDistance) && _flybyTimer <= 0.0f)
            {
                PlaySound(FlybyKey);
                _flybyTimer = _flybyCooldown;
            }
        }

        /// <summary>
        /// Plays the engine start sfx for flying vehicles.
        /// </summary>
        public override void EngineStart()
        {
            base.EngineStart();
            StartEngineLoop();
            flying = false;
        }
    }
}