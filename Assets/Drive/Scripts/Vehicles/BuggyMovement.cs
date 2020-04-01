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
    /// Handles all physics calculations on the buggy and sends updates to the BuggyPresentation.  
    /// </summary>
    [RequireComponent(typeof(BuggyPresentation))]
    public class BuggyMovement : MonoBehaviour
    {
        // ------ Private Members ------

        [SerializeField] private WheelCollider[] _wheelColliders = new WheelCollider[4];
        [SerializeField] private float _maxSteerAngle;
        [SerializeField] private float _maxMotorTorque;
        [SerializeField] private float _fallingGravityMultiplier;
        [SerializeField] private float _resetSpeed;
        [SerializeField] private float _tireSlipValue;

        private Rigidbody _rigidBody;
        private BuggyPresentation _buggyPresentation;
        private float _steerAngle;
        private float _gearRatio;
        private float _originalGravityMagnitude;
        private float _fastTurnThreshold = 0.95f;

        private const int NumOfWheels = 4;

        private bool _isReversing = false;
        private bool _playReverseSound = true;

        // ------ MonoBehaviour Methods ------

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _buggyPresentation = GetComponent<BuggyPresentation>();
            _rigidBody.centerOfMass = Vector3.zero;
            _gearRatio = 0.0f;
        }

        private void Update()
        {
            SetGravity();
        }

        private void OnEnable()
        {
            _originalGravityMagnitude = Physics.gravity.magnitude;
            _buggyPresentation.SetVehicleLocator(true);
        }

        private void OnDisable()
        {
            Physics.gravity = Vector3.down * _originalGravityMagnitude;
            _buggyPresentation.SetVehicleLocator(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            _buggyPresentation.PlayBump();
        }

        //----------- Public Methods -----------

        /// <summary>
        /// Apply physics operations to the Buggy based upon steering/acceleration inputs (used by BuggyInput)
        /// </summary>
        public void Move(float steering, float acceleration)
        {
            steering = Mathf.Clamp(steering, -1, 1);
            acceleration = Mathf.Clamp(acceleration, -1, 1);

            if (acceleration < 0)
            {
                _isReversing = true;
            }
            else
            {
                _isReversing = false;
                _playReverseSound = true;
            }

            if (_isReversing && _playReverseSound)
            {
                _buggyPresentation.PlayReverse();
                _playReverseSound = false;
            }

            _gearRatio = Mathf.Lerp(_gearRatio, Mathf.Abs(acceleration), Time.deltaTime);

            ApplySteering(steering);
            if(Mathf.Abs(steering) > _fastTurnThreshold && acceleration > _fastTurnThreshold)
            {
                _buggyPresentation.PlayFastTurn();
            }
            ApplyThrust(acceleration);

            for (int i = 0; i < NumOfWheels; i++)
            {
                Quaternion rotation;
                Vector3 position;
                WheelHit hit;
                bool isSlipping = false;
                _wheelColliders[i].GetWorldPose(out position, out rotation);
                _wheelColliders[i].GetGroundHit(out hit);
                if (Mathf.Abs(hit.forwardSlip) > _tireSlipValue)
                {
                    isSlipping = true;
                }
                _buggyPresentation.TireSlipping(isSlipping, i);
                _buggyPresentation.SetTireTransform(position, rotation, i);
            }
            
            _buggyPresentation.UpdateAudioPitch(_gearRatio * _rigidBody.velocity.magnitude);
        }

        /// <summary>
        /// Resets the vehicle to the position and rotation given in the parameters
        /// </summary>
        public void Reset(Vector3 position, Quaternion rotation)
        {
            if(_rigidBody == null)
            {
                _rigidBody = GetComponent<Rigidbody>();
            }
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
            _rigidBody.position = position;
            _rigidBody.rotation = rotation;
        }

        /// <summary>
        /// If the vehicle is travelling faster than it possibly could on ground,
        /// then we assume it has fallen through the world and needs to be reset
        /// </summary>
        public bool FallenThroughFloor()
        {
            if(_rigidBody.velocity.magnitude > _resetSpeed)
            {
                return true;
            }
            return false;
        }

        //----------- Private Methods -----------

        private void SetGravity()
        {
            Vector3 normal = GetSurfaceNormal();
            if (normal != Vector3.zero)
            {
                Physics.gravity = -normal * _originalGravityMagnitude;
            }
            else
            {
                Physics.gravity = Vector3.down * _fallingGravityMultiplier;
            }
        }

        private void ApplySteering(float steerValue)
        {
            _steerAngle = steerValue * _maxSteerAngle;
            _wheelColliders[0].steerAngle = _steerAngle;
            _wheelColliders[1].steerAngle = _steerAngle;
        }

        private void ApplyThrust(float accel)
        {
            float thrustTorque = accel * _maxMotorTorque;
            for (int i = 0; i < NumOfWheels; i++)
            {
                _wheelColliders[i].motorTorque = thrustTorque;
            }
        }

        private Vector3 GetSurfaceNormal()
        {
            WheelHit hit;
            Vector3 aggregateNormal = Vector3.zero;
            foreach (WheelCollider wheel in _wheelColliders)
            {
                if (wheel.GetGroundHit(out hit))
                {
                    aggregateNormal += hit.normal;
                }
            }
            aggregateNormal.Normalize();
            return aggregateNormal;
        }
    }
}