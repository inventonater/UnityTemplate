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
    /// Handles user inputs and normalizes them for BuggyMovement to consume and apply drive to the vehicle
    /// </summary>
    [RequireComponent(typeof(BuggyMovement))]
    public class BuggyInput : MonoBehaviour
    {

        //----------- Private Members -----------

        [SerializeField] ControllerInput _controllerInput;

        // deadzoneAngle is the amount of angular rotation that needs to be applied before turning input is applied
        [SerializeField] private float _deadzoneAngle = 5.0f;

        // maxAngle is used to define the maximum angle the user can rotate his/her controller to manipulate the buggy
        [SerializeField] private float _maxAngle;
        private BuggyMovement _buggyController;
        private float _turningValue;
        private float _thrustValue;
        private bool _isReversing;

        //----------- MonoBehaviour Methods -----------

        private void Awake()
        {
            _buggyController = GetComponent<BuggyMovement>();
        }

        private void FixedUpdate()
        {
            _thrustValue = _controllerInput.triggerValue;
            _turningValue = CalculateTurningInput();

            if (_isReversing)
            {
                _thrustValue = -_thrustValue;
            }

            // Checks if the buggy's velocity is over the threshold and resets if it's in free fall.  
            if(_buggyController.FallenThroughFloor())
            {
                _buggyController.Reset(_controllerInput.transform.position, _controllerInput.transform.rotation);
                gameObject.SetActive(false);
            }
            _buggyController.Move(_turningValue, _thrustValue);
        }
        
        private void OnEnable()
        {
            _buggyController.Reset(_controllerInput.transform.position, _controllerInput.transform.rotation);
            _controllerInput.OnTouchDown += HandleOnTouchDown;
            _controllerInput.OnTouchUp += HandleOnTouchUp;
        }

        private void OnDisable()
        {
            _controllerInput.OnTouchDown -= HandleOnTouchDown;
            _controllerInput.OnTouchUp -= HandleOnTouchUp;
        }

        //----------- Event Handlers -----------

        private void HandleOnTouchUp()
        {
            _isReversing = false;
        }

        private void HandleOnTouchDown()
        {
            _isReversing = true;
        }

        //----------- Private Methods -----------

        /// <summary>
        /// Isolates the rotation of the Z axis of the controller.
        /// Returns a normalized angle[-1, 1], taking the deadzone into account.
        /// </summary>
        private float CalculateTurningInput()
        {
            Vector3 eulerRotation = _controllerInput.transform.rotation.eulerAngles;
            Quaternion baseRotation = Quaternion.Euler(new Vector3(eulerRotation.x, eulerRotation.y, 0.0f));
            float returnedValue = Vector3.Angle(baseRotation * Vector3.up, _controllerInput.transform.up);
            if (Mathf.Abs(returnedValue) > _deadzoneAngle)
            {
                if (_controllerInput.transform.rotation.eulerAngles.z % 360 < 180)
                {
                    returnedValue = -returnedValue;
                }
                return Mathf.Clamp(returnedValue / _maxAngle, -1.0f, 1.0f);
            }
            return 0.0f;
        }
    }
}
