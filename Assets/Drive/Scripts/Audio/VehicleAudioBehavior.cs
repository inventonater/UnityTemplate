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
    /// Handles all audio for all vehicles (Base Class).
    /// </summary>
    public class VehicleAudioBehavior : AudioBehavior
    {
        //----------- Private Members -----------

        private const string EngineStartKey = "engine_start";
        private const string EngineLoopKey = "engine_main_loop";
        private const string EnginePowerParameter = "engine_power_parameter";

        [SerializeField] protected AudioSource _engineAudioSource;

        protected AudioEvent _engineAudioEvent;

        //----------- Public Methods -----------

        /// <summary>
        /// Plays the engine start sfx.
        /// </summary>
        public virtual void EngineStart()
        {
            PlaySound(EngineStartKey);
        }

        /// <summary>
        /// Plays the sfx for engine stop.
        /// </summary>
        public virtual void EngineStop()
        {
            _engineAudioSource.Stop();
            _engineAudioEvent = null;
        }

        /// <summary>
        /// Sets parameter according to engine speed.
        /// </summary>
        /// <param name="value">Engine power value</param>
        public virtual void SetEnginePower(float value)
        {
            // Change the range of the input we get from the engine power (0 to 1) to the range we want for the pitch.
            if (_engineAudioEvent != null)
            {
                SetParameter(_engineAudioEvent, EnginePowerParameter, value);
            }
        }

        //----------- Protected Methods -----------

        /// <summary>
        /// Plays the sfx for engine loop.
        /// </summary>
        protected void StartEngineLoop()
        {
            _engineAudioEvent = PlaySound(EngineLoopKey, _engineAudioSource);
        }
    }
}