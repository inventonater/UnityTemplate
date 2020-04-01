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
    public class ToyPlaneInput : MonoBehaviour
    {
        // ------ Private Members ------

        [SerializeField] private ControllerInput _controller;
        [SerializeField] private float _maxAngleRange;
        private BiplaneMovement _biplaneMovement;
        private bool _attachedToController;
        private float _rollInput;
        private float _pitchInput;
        private float _yawInput;
        
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
            SetInputFromController();
        }

        private void FixedUpdate()
        {
            if (_attachedToController)
            {
                return;
            }
            _biplaneMovement.Move(_rollInput, _pitchInput, _yawInput, 1.0f);
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
        /// Process the angle of roll, pitch and yaw from the contoller to get numbers between -1 and 1.
        /// </summary>
        private void SetInputFromController()
        {
            Quaternion rotationDifference = Quaternion.Inverse(transform.rotation) * _controller.orientation;
            _rollInput = NormalizeControllerAngle(rotationDifference.eulerAngles.z);
            _pitchInput = NormalizeControllerAngle(rotationDifference.eulerAngles.x);
            _yawInput = NormalizeControllerAngle(rotationDifference.eulerAngles.y);
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
            return clampedAngle;
        }
    }
}
