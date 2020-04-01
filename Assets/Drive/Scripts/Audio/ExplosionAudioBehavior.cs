// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using MagicLeap.Utilities;

namespace MagicKit
{
    /// <summary>
    /// Handles audio for an explosion effect.
    /// </summary>
    public class ExplosionAudioBehavior : AudioBehavior
    {
        //----------- Private Members -----------

        private const string ExplosionKey = "explosion";

        //----------- MonoBehaviour Methods -----------

        private void Start()
        {
            PlaySound(ExplosionKey);
        }
    }
}
