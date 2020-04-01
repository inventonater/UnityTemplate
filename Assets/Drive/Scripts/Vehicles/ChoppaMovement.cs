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
    ///<summary>
    /// Vehicle controller for da choppa.
    ///</summary>
    public class ChoppaMovement : MonoBehaviour 
    {
        //----------- Public Members -----------
        
        [HideInInspector] public bool flying;
        [HideInInspector] public bool crashed;

        //----------- Private Members -----------
        
        private const float GravityScale = 1.05f;

        [SerializeField] private ChoppaPresentation _choppaPresentation;
        [SerializeField] private Rigidbody _rigidBody;
        [SerializeField] private AnimationCurve _throttleToLift = AnimationCurve.EaseInOut(0f, 0.85f, 1f, 1.15f);
        [SerializeField] private AnimationCurve _horizontalVelocityByAngle = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _horizontalVelocityScale = 0.8f;
        [SerializeField] private float _verticalVelocityScale = 0.7f;

        private Quaternion _previousControllerOrientation;

        //----------- MonoBehaviour Methods -----------
        
        private void Start()
        {
            _choppaPresentation.SetEngineActive(true);
        }

        private void OnEnable()
        {
            ResetVehicle();
            _choppaPresentation.SetEngineActive(true);
        }

        private void OnDisable()
        {
            ResetVehicle();
            _choppaPresentation.SetEngineActive(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Ignore collisions while we're attached to the controller
            if (!flying)
            {
                return;
            }

            // Explode!
            _choppaPresentation.Explode();
            crashed = true;
        }

        private void LateUpdate()
        {
            if (crashed)
            {
                crashed = false;
                gameObject.SetActive(false);
            }
        }

        //----------- Public Methods -----------

        public void Move(Quaternion goalOrientation, float thrust)
        {
            // Detect liftoff
            if (!flying && LiftForce(thrust) > -Physics.gravity.y)
            {
                flying = true;
                _rigidBody.useGravity = true;
                _choppaPresentation.SetVehicleLocator(true);
                _choppaPresentation.SetVehicleFlying(true);
            }

            if (flying)
            {
                // Calculate lift force
                Vector3 impulse = _rigidBody.rotation * new Vector3(0f, LiftForce(thrust), 0f);

                // Calculate torque force
                Vector3 torque = GetTorqueNeededToMatchOrientation(goalOrientation);

                // Apply lift and torque
                _rigidBody.AddForce(impulse, ForceMode.Force);
                _rigidBody.AddTorque(torque, ForceMode.Force);

                // Clamp XZ velocity
                // xzAngle will be close to 0 when the choppa's up vector is pointing close to vertical
                // and close to 1 when the choppa's up is pointing close to horizontal
                float xzAngle = 1 - Mathf.Abs(Vector3.Dot(_rigidBody.transform.up, Vector3.up));
                Vector3 choppaVelocityDir = _rigidBody.velocity.normalized;
                Vector3 xzVelocity = Vector3.ProjectOnPlane(choppaVelocityDir, Vector3.up);
                float maxHorizontalSpeed = _horizontalVelocityByAngle.Evaluate(xzAngle) * _horizontalVelocityScale;
                float clampedHorizontalSpeed = Mathf.Clamp(xzVelocity.magnitude, 0f, maxHorizontalSpeed);
                Vector3 clampedHorizontalVelocity = xzVelocity.normalized * clampedHorizontalSpeed;

                // Clamp Y velocity
                // We allow the vehicle to fall just a little bit faster than it can rise
                float maxVerticalVelocity = _verticalVelocityScale;
                float clampedVerticalVelocity = Mathf.Clamp(_rigidBody.velocity.y, -(maxVerticalVelocity * GravityScale), maxVerticalVelocity);

                _rigidBody.velocity = new Vector3(clampedHorizontalVelocity.x, clampedVerticalVelocity, clampedHorizontalVelocity.z);

            }
            _choppaPresentation.UpdateEnginePower(thrust);

        }

        private float LiftForce(float throttle)
        {
            return _throttleToLift.Evaluate(throttle) * _rigidBody.mass * -Physics.gravity.y;
        }

        /// <summary>
        /// Reset the vehicle to its default, unlaunched state.
        /// </summary>
        public void ResetVehicle()
        {
            flying = false;
            _rigidBody.useGravity = false;
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
            _choppaPresentation.SetVehicleLocator(false);
            _choppaPresentation.SetVehicleFlying(false);
        }

        //----------- Private Methods -----------

        private Vector3 GetTorqueNeededToMatchOrientation(Quaternion orientation)
        {
            Vector3 torque = Vector3.zero;
            // Ignore sudden jumps in controller orientation, controller data can be noisy..
            if (Quaternion.Dot(orientation, _previousControllerOrientation) > 0.75f)
            {
                Vector3 neededTorque = GetRelativeRotationalAxis(orientation);
                torque = RotateAroundInertialCenter(neededTorque);
            }
            _previousControllerOrientation = orientation;
            return torque;
        }

        private Vector3 GetRelativeRotationalAxis(Quaternion orientation)
        {
            // Get axis of rotation between choppa and controller orientation
            Vector3 v = Vector3.Cross(transform.up, orientation * Vector3.up);
            v += Vector3.Cross(transform.right, orientation * Vector3.right);
            v += Vector3.Cross(transform.forward, orientation * Vector3.forward);
            v /= 3;
            return v;
        }

        private Vector3 RotateAroundInertialCenter(Vector3 v)
        {
            float t = Mathf.Asin(v.magnitude);
            Vector3 a = v.normalized * (t / Time.deltaTime);
            Quaternion q = _rigidBody.rotation * _rigidBody.inertiaTensorRotation;
            return q * Vector3.Scale(_rigidBody.inertiaTensor, Quaternion.Inverse(q) * a);
        }
    }
}