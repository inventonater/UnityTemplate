// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using MagicKit;

namespace MagicLeap.Utilities
{
    ///<summary>
    /// Provides a visual pointer to an object in the world and helps with limited FOV usage.
    ///</summary>
    public class Locator : MonoBehaviour
    {
        //----------- Public Members -----------

        [Tooltip("The object we will be pointing to.")]
        public Transform target;

        [Tooltip("Turns on and off the locator's operations.")]
        public bool locate;

        [Tooltip("The pointing visual that will be used as our pointer.")]
        public Transform visual;

        [Tooltip("The scale of our pointing visual.")]
        public float scale;

        [Tooltip("The distance from the camera to place the pointing visual.")]
        public float distance;

        [Tooltip("The radius around which the pointing visual will pivot.")]
        public float radius;

        [Tooltip("The smoothing amount to apply to the pointing visual as it moves.")]
        public float speed;

        [Tooltip("To avoid confusing pointing directions when a target is behind us this will flatten the direction when behind.")]
        public bool flattenIfBehind;

        [Tooltip("Should we fade the pointing visual as we get closer to our target?")]
        public bool fadeByDistance;

        //----------- Private Members -----------

        private Camera _camera;
        private IndicatorProperties _targetIndicatorProperties;

        //----------- MonoBehaviour Methods -----------

        private void Reset()
        {
            target = transform;
            locate = true;
            distance = 1;
            scale = 1;
            speed = 20;
            fadeByDistance = true;
            flattenIfBehind = true;
        }

        private void Awake()
        {
            _camera = Camera.main;
            _targetIndicatorProperties = GetComponentInChildren<IndicatorProperties>();
        }

        private void LateUpdate()
        {
            if (visual == null)
            {
                return;
            }

            if (!locate)
            {
                visual.gameObject.SetActive(false);
                return;
            }
            else
            {
                if (!visual.gameObject.activeSelf)
                {
                    visual.gameObject.SetActive(true);
                }

                //apply position:
                visual.transform.position = _camera.transform.forward * distance + _camera.transform.position;

                //scale:
                scale = Mathf.Max(0, scale);
                float frustumHeight = 2.0f * distance * Mathf.Tan(_camera.fieldOfView * .5f * Mathf.Deg2Rad);
                visual.transform.localScale = new Vector3(frustumHeight * scale, frustumHeight * scale, 1);

                //rotation calcs:
                Vector3 flattenedVector = Vector3.ProjectOnPlane((target.position - visual.transform.position).normalized, _camera.transform.forward);
                Quaternion lookRotation = Quaternion.LookRotation(-_camera.transform.forward, flattenedVector.normalized);

                //percentage to:
                float dotTo = Vector3.Dot(_camera.transform.forward, (target.position - _camera.transform.position).normalized);
                bool inFront = dotTo >= 0;

                //angle to:
                float sideTo = Vector3.Dot(_camera.transform.right, (target.position - _camera.transform.position).normalized);

                //fade application:
                if (fadeByDistance && _targetIndicatorProperties != null)
                {
                    float alpha = Mathf.Clamp01(dotTo);
                    alpha = 1 - alpha;
                    _targetIndicatorProperties.SetAlpha(alpha);
                }

                //stylize rotation:
                if (flattenIfBehind && !inFront)
                {
                    //lock to a left and right direction if the target is behind:
                    if (sideTo > 0)
                    {
                        lookRotation = Quaternion.LookRotation(-_camera.transform.forward, _camera.transform.right);
                    }
                    else
                    {
                        lookRotation = Quaternion.LookRotation(-_camera.transform.forward, -_camera.transform.right);
                    }
                }

                //apply rotation:
                //snap if the angle difference is large:
                if (Quaternion.Angle(visual.transform.rotation, lookRotation) > 90)
                {
                    visual.transform.rotation = lookRotation;
                }
                else
                {
                    visual.transform.rotation = Quaternion.Slerp(visual.transform.rotation, lookRotation, speed * Time.deltaTime);
                }

                //apply radius:
                visual.transform.Translate(Vector3.up * radius);
            }
            
        }
    }
}