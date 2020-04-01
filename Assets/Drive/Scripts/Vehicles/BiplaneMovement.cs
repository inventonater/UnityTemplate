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
    /// Moves the biplane based upon input from the controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BiplanePresentation))]
    public class BiplaneMovement : MonoBehaviour
    {
        // ------ Public Members ------

        [HideInInspector] public bool crashed;

        // ------ Private Members ------

        [SerializeField] private AnimationCurve _gravityCurve;
        // The min percentage of gravity that the plane is affected by.
        [SerializeField] [Range(0f, 1f)] private float _minGravityEffect = 0f;
        // The max percentage of gravity that the plane is affected by.
        [SerializeField] [Range(0f, 1f)] private float _maxGravityEffect = 0.5f;
        // Initial boost to thrust when launching the plane from the controller.
        [SerializeField] [Range(0f, 1f)] private float _launchBoost = 1f;
        [SerializeField] private float _maxPower = 5f;
        [SerializeField] private float _minPower = 0.2f;
        [SerializeField] private float _rotationSpeed = 0.6f;
        [SerializeField] private float _lift = 0.001f;
        [SerializeField] private float _rollFactor = 1f;
        [SerializeField] private float _pitchFactor = 1f;
        [SerializeField] private float _yawFactor = 1f;
        // Factor by which thrust input is increased - controls how fast max engine power is reached when holding the trigger down.
        [SerializeField] private float _thrustFactor = 0.5f;
        // Influences how quickly the plane will orient toward the direction it is moving - such as having it face down when the plane is stalled and falling.
        [SerializeField] private float _alignToVelocityFactor = 1.5f;
        // Influences how much drag is increased as the plane picks up speed.
        [SerializeField] private float _airResistanceFactor = 1f;

        private BiplanePresentation _biplanePresentation;
        private Rigidbody _rbody;
        private float _power;
        private float _thrust;
        private float _forwardSpeed;
        private float _defaultDrag;
        private float _defaultAngularDrag;
        private bool _flying;

        // ------ MonoBehaviour Methods ------

        private void Awake()
        {
            _biplanePresentation = GetComponent<BiplanePresentation>();
            _rbody = GetComponent<Rigidbody>();
            _defaultDrag = _rbody.drag;
            _defaultAngularDrag = _rbody.angularDrag;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_flying)
            {
                return;
            }
            // Mayday!
            _biplanePresentation.Explode();
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

        private void OnEnable()
        {
            _biplanePresentation.StartEngine();
        }

        // ------ Public Methods ------

        /// <summary>
        /// Disable/reset physics.
        /// </summary>
        public void Reset()
        {
            _flying = false;
            _rbody.isKinematic = true;
            _biplanePresentation.SetEngineStalled(true);
            _biplanePresentation.SetVehicleLocator(false);
            _biplanePresentation.SetVehicleFlying(false);
            _power = _minPower;
        }

        /// <summary>
        /// Re-enable physics to begin flight.
        /// </summary>
        public void BeginFlight()
        {
            _flying = true;
            _rbody.isKinematic = false;
            // Give it an initial boost.
            _thrust = _launchBoost;
            _biplanePresentation.SetVehicleFlying(true);
            _biplanePresentation.SetVehicleLocator(true);
        }

        /// <summary>
        /// Apply physics operations to the biplane based upon supplied input and update the biplane display appropriatley.
        /// </summary>
        public void Move(float roll, float pitch, float yaw, float thrust)
        {
            // Calculate air resistance.
            float airResistance = _rbody.velocity.magnitude * _airResistanceFactor;

            // Calculate current forward speed.
            Vector3 localVelocity = transform.InverseTransformDirection(_rbody.velocity);
            _forwardSpeed = Mathf.Max(0, localVelocity.z);

            // Calculate forward power.
            _thrust = Mathf.Clamp01(_thrust + thrust * Time.deltaTime * _thrustFactor);
            _power = Mathf.Clamp(_thrust * _maxPower, _minPower, _maxPower);
            Vector3 forwardForce = _power * transform.forward;

            // Calculate lift.
            Vector3 liftDirection = Vector3.Cross(_rbody.velocity, transform.right).normalized;
            float liftPower = _forwardSpeed * _forwardSpeed * _lift;
            Vector3 liftForce = liftPower * liftDirection;

            // Calculate a custom "arcade-physics" gravity based upon the current percentage of power the engine has and an animation curve.
            float normalizedPower = (_power - _minPower) / (_maxPower - _minPower);
            float gravCurveVal = _gravityCurve.Evaluate(normalizedPower);
            float gravAmount = Mathf.Lerp(_maxGravityEffect, _minGravityEffect, gravCurveVal);
            Vector3 gravityForce = Physics.gravity * gravAmount;

            // Calculate the total force to apply this tick.
            Vector3 force = forwardForce + liftForce + gravityForce;

            // Calculate torque.
            Vector3 pitchForce = pitch * _pitchFactor * transform.right;
            Vector3 rollForce = roll * _rollFactor * transform.forward;
            Vector3 yawForce = yaw * _yawFactor * transform.up;
            float alignFwdDot = Vector3.Dot(transform.forward, _rbody.velocity.normalized);
            float tPower = _rotationSpeed * alignFwdDot;
            Vector3 torque = (pitchForce + rollForce + yawForce) * tPower;

            // Update the rigidbody.
            _rbody.drag = _defaultDrag + airResistance;
            _rbody.angularDrag = _defaultAngularDrag * _forwardSpeed;
            _rbody.angularVelocity = Vector3.zero;
            _rbody.AddForce(force);
            _rbody.AddTorque(torque);
            if(_rbody.velocity.magnitude > 0f)
            {
                float alignment = Mathf.Lerp(_alignToVelocityFactor, 0f, gravCurveVal);
                Quaternion lookRot = Quaternion.LookRotation(_rbody.velocity, transform.up);
                _rbody.rotation = Quaternion.Slerp(_rbody.rotation, lookRot, alignment * Time.deltaTime);
            }

            // Update the display.
            float powerPercentage = Mathf.Clamp01(_power / _maxPower);
            UpdateBiplanePresentation(roll, pitch, powerPercentage);
        }

        // ------ Private Methods ------

        private void UpdateBiplanePresentation(float roll, float pitch, float power)
        {
            if (!_flying)
            {
                _biplanePresentation.SetEngineStalled(false);
                _biplanePresentation.UpdateEnginePower(_minPower);
                return;
            }
            _biplanePresentation.SetPitch(pitch);
            _biplanePresentation.SetRoll(roll);
            bool stalled = (_power == _minPower);
            _biplanePresentation.SetEngineStalled(stalled);
            if (!stalled)
            {
                _biplanePresentation.UpdateEnginePower(power);
            }
        }
    }
}