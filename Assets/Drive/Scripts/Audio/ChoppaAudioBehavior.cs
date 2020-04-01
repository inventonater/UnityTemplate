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
    /// Handles all audio for da choppa.
    /// </summary>
    public class ChoppaAudioBehavior : FlyingVehicleAudioBehavior
    {
        //----------- Public Methods -----------

        /// <summary>
        /// Plays the engine power to modify engine pitch.
        /// </summary>
        public override void SetEnginePower(float value)
        {
            base.SetEnginePower(value);
            if(value > 0.2f) // Minimum chopper engine power required for flyby
            {
                base.flying = true;
            }
            else
            {
                base.flying = false;
            }
        }

        
    }
}

