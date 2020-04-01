// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using MagicLeap.Utilities;

namespace MagicKit
{
    /// <summary>
    /// Handles all audio for the biplane.
    /// </summary>
    public class BiplaneAudioBehavior : FlyingVehicleAudioBehavior
    {
        //----------- Private Members -----------

        private const string EngineStalledKey = "engine_stalled";

        [SerializeField] private AudioSource _enginStalledAudioSource;

        //----------- Public Methods -----------

        /// <summary>
        /// Plays/stops the looping engine active sfx on the engine loop channel.
        /// </summary>
        /// <param name="status">True if Engine is active/on</param>
        public void PlayEngineActive(bool status)
        {
            if (status)
            {
                _enginStalledAudioSource.Stop();
                StartEngineLoop();
            }
            else
            {
                PlaySound(EngineStalledKey, _enginStalledAudioSource);
                EngineStop();
            }
        }
    }
}
