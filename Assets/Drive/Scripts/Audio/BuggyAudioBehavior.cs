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
    /// Handles all audio for the buggy.
    /// </summary>
    public class BuggyAudioBehavior : VehicleAudioBehavior
    {
        //----------- Private Members -----------

        private const string EngineReverseKey = "engine_reverse";
        private const string TireScreech = "tire_screech";
        private const string Bump = "bump";
        private const string FastTurn = "fast_turn";

        //Always assume tire order of Front Left, Front Right, Back Left, Back Right
        [SerializeField] private AudioSource[] _tireAudioSource;
        [SerializeField] private float _bumpDelayTime = 1f;

        private float _bumpReadyTimer;

        //----------- Public Methods -----------

        ///// <summary>
        ///// Plays the engine start sfx.
        ///// </summary>
        public override void EngineStart()
        {
            base.EngineStart();
            StartEngineLoop();
        }

        /// <summary>
        /// Plays the engine reverse sfx.
        /// </summary>
        public void PlayEngineReverse()
        {
            PlaySound(EngineReverseKey);
        }

        /// <summary>
        /// Play the bump sfx.
        /// </summary>
        public void PlayBump()
        {
            if( Time.time < _bumpReadyTimer)
            {
                return;
            }
            PlaySound(Bump);
            _bumpReadyTimer = Time.time + _bumpDelayTime;
        }

        /// <summary>
        /// Plays/stops the tire screeching sfx on the corresponding tire that is slipping
        /// </summary>
        /// <param name="status">True if tires are screeching</param>
        /// <param name="tireNum">Tire number to play sound clip at</param>
        public void PlayTireScreech(bool status, int tireNum)
        {
            if(status && !_tireAudioSource[tireNum].isPlaying)
            {
                PlaySound(TireScreech, _tireAudioSource[tireNum]);
            }
        }

        /// <summary>
        /// Play sfx for when a fast turn has been detected.
        /// </summary>
        /// <param name="status">True if turning quickly</param>
        public void PlayFastTurn(bool status)
        {
            if(status)
            {
                foreach (AudioSource tire in _tireAudioSource)
                {
                    if (!tire.isPlaying)
                    {
                        PlaySound(FastTurn, tire);
                    }
                }
            }
        }
    }
}
