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
    /// Handles user inputs and normalizes them for BiplaneMovement to consume.
    /// </summary>
    [RequireComponent(typeof(BiplaneMovement))]
    public class BiplaneInput : MonoBehaviour
    {
        // ------ Private Members ------

        [SerializeField] private ControllerInput _controller;
        // Input values below this are disregarded.
        [SerializeField] [Range(0f, 1f)] private float _inputDeadzone = 0f;
        [SerializeField] private float _maxAngleRange = 90f;

        private BiplaneMovement _biplaneMovement;
        private bool _attachedToController;
        private float _thrustInput;
        private float _rollInput;
        private float _pitchInput;
  
        private const float ThrustInputSmooth = 2f;

        // ------ MonoBehaviour Methods ------

        private void Awake()
        {
            _biplaneMovement = GetComponent<BiplaneMovement>();
        }

        private void OnEnable()
        {
            // Attach plane to controller at the beginning.
            SetAttachmentToController(true);
        }

        private void Start()
        {
            _controller.OnTriggerDown += HandleOnTriggerDown;
        }

        private void Update()
        {
            SetThrustFromController();
            SetRollFromController();
            SetPitchFromController();
        }

        private void FixedUpdate()
        {
            if (_attachedToController)
            {
                return;
            }
            _biplaneMovement.Move(_rollInput, _pitchInput, 0.0f, _thrustInput);
        }

        private void OnDestroy()
        {
            _controller.OnTriggerDown -= HandleOnTriggerDown;
        }

        // ------ Event Handlers ------

        private void HandleOnTriggerDown()
        {
            // Detach and begin flight.
            SetAttachmentToController(false);
        }
         
        // ------ Private Methods ------

        /// <summary>
        /// Either reset the biplane by parenting it to the controller or release it so it can begin flying.
        /// </summary>
        private void SetAttachmentToController(bool status)
        {
            _attachedToController = status;
            if (_attachedToController)
            {
                _biplaneMovement.Reset();
                transform.SetParent(_controller.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(null);
                _biplaneMovement.BeginFlight();
            }
        }

        /// <summary>
        /// Process the angle of roll from the contoller to get a number between -1 and 1.
        /// </summary>
        private void SetRollFromController()
        {
            float angle = _controller.orientation.eulerAngles.z;
            _rollInput = NormalizeControllerAngle(angle);
        }

        /// <summary>
        /// Process the angle of pitch from the contoller to get a number between -1 and 1.
        /// </summary>
        private void SetPitchFromController()
        {
            float angle = _controller.orientation.eulerAngles.x;
            _pitchInput = NormalizeControllerAngle(angle);
        }

        /// <summary>
        /// Normalizes an angle (in degrees) to a number between -1 and 1.
        /// </summary>
        private float NormalizeControllerAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }
            float normalizedAngle = Mathf.Clamp(angle / _maxAngleRange, -1f, 1f);
            float clampedAngle = normalizedAngle;
            // Apply dead-zone.
            if (Mathf.Abs(clampedAngle) < _inputDeadzone)
            {
                clampedAngle = 0f;
            }
            clampedAngle = Mathf.Clamp(clampedAngle, -1f, 1f);
            return clampedAngle;
        }

        /// <summary>
        /// Process the trigger pull amount from the contoller to get a number between -1 and 1.
        /// </summary>
        private void SetThrustFromController()
        {
            // Change the range of the input we get from the trigger (0 to 1) to the range we want for the thrust (-1 to 1).
            float triggerMin = 0f;
            float triggerMax = 1f;
            float thrustMin = -1f;
            float thrustMax = 1f;
            float triggerRange = triggerMax - triggerMin;
            float thrustRange = thrustMax - thrustMin;
            float rawThrust = (((_controller.triggerValue - triggerMin) * thrustRange) / triggerRange) + thrustMin;
            // Smooth our thrust value to prevent sharp jumps in velocity.
            _thrustInput = Mathf.Lerp(_thrustInput, rawThrust, Time.deltaTime * ThrustInputSmooth);
            _thrustInput = Mathf.Clamp(_thrustInput, -1f, 1f);
        }
    }
}
