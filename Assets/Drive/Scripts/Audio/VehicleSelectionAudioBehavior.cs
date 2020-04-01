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
    /// Handles audio for the vehicle selection menu.
    /// </summary>
    public class VehicleSelectionAudioBehavior : AudioBehavior
    {
        //----------- Private Members -----------

        private const string SelectedKey = "selected";
        private const string DeselectedKey = "deselected";
        private const string MenuOpenKey = "menu_open";
        private const string MenuCloseKey = "menu_close";

        [SerializeField] private AudioSource _buggyAudioSource;
        [SerializeField] private AudioSource _choppaAudioSource;
        [SerializeField] private AudioSource _biplaneAudioSource;

        //----------- Public Methods -----------

        /// <summary>
        /// Plays the sfx for a vehicle being selected/deselected
        /// </summary>
        public void PlayVehicleSelected(bool status, GameObject location)
        {
            if (status)
            {
                PlaySoundAt(SelectedKey, location);
            }
            else
            {
                PlaySoundAt(DeselectedKey, location);
            }
        }

        /// <summary>
        /// Plays the sfx for the menu opening.
        /// </summary>
        public void PlayMenuOpen(GameObject location)
        {
            PlaySoundAt(MenuOpenKey, location);
        }

        /// <summary>
        /// Plays the sfx for the menu closing.
        /// </summary>
        public void PlayMenuClose(GameObject location)
        {
            PlaySoundAt(MenuCloseKey, location);
        }
    }
}
