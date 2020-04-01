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
    /// Handles user inputs and normalizes them for ChoppaMovement to consume.
    /// </summary>
    public class ChoppaInput : MonoBehaviour
    {
        //----------- Private Members -----------

        [SerializeField] private ChoppaMovement _choppaMovement;
        [SerializeField] private ControllerInput _controllerInput;

        private Quaternion _headingOffset = Quaternion.identity;
        private Quaternion _goalOrientation = Quaternion.identity;
        private float _throttle = 0.0f;

        //----------- MonoBehaviour Methods -----------

        private void Update()
        {
            if (_choppaMovement.crashed)
            {
                _headingOffset = Quaternion.identity;
            }

            // Move throttle to match the trigger value
            _throttle =
                Mathf.Lerp(_throttle, _controllerInput.triggerValue, Time.smoothDeltaTime);

            if (_controllerInput.TouchActive)
            {
                // Add some extra rotation around the choppa's y-axis if the user taps the touchpad
                Vector2 touchPosition = _controllerInput.TouchPosition;
                float headingAngle = Mathf.Atan2(touchPosition.x, touchPosition.y) * Mathf.Rad2Deg;
                _headingOffset = Quaternion.AngleAxis(headingAngle, Vector3.up);
            }

            // Orient the choppa to match the controller's position
            _goalOrientation = _controllerInput.orientation * _headingOffset;
        }

        private void FixedUpdate()
        {
            _choppaMovement.Move(_goalOrientation, _throttle);
        }

        private void LateUpdate()
        {
            if (!_choppaMovement.flying)
            {
                transform.position = _controllerInput.position;
                transform.rotation = _controllerInput.orientation;
            }
        }
        private void OnDisable()
        {
            _throttle = 0.0f;
        }
    }
}