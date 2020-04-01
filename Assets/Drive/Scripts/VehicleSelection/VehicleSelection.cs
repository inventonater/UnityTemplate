// ---------------------------------------------------------------------
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MagicKit
{
    ///<summary>
    /// Control the UI to allow the selection of the three vehicles.
    ///</summary>
    public class VehicleSelection : MonoBehaviour 
    {

        //----------- Private Members -----------
        
        private const string SelectedTrigger = "SetActive";
        private const string NotSelectedTrigger = "SetInactive";
        private const string DefaultTrigger = "SetDefault";
        
        // Anchors are the target points that the UI objects will tween towards
        [FormerlySerializedAs("_planeAnchor")]
        [SerializeField] private Transform _biplaneAnchor;
        [SerializeField] private Transform _choppaAnchor;
        [SerializeField] private Transform _buggyAnchor;
        // The selectables are the UI representations of our vehicles
        [FormerlySerializedAs("_planeSelectable")]
        [SerializeField] private GameObject _biplaneSelectable;
        [SerializeField] private GameObject _choppaSelectable;
        [SerializeField] private GameObject _buggySelectable;
        // These are the actual vehicles which are controlled in the world
        [FormerlySerializedAs("_plane")]
        [SerializeField] private GameObject _biplane;
        [SerializeField] private GameObject _choppa;
        [SerializeField] private GameObject _buggy;
        // Tutorial animations to help show off the control scheme
        [FormerlySerializedAs("_planeTutorial")]
        [SerializeField] private GameObject _biplaneTutorial;
        [SerializeField] private GameObject _choppaTutorial;
        [SerializeField] private GameObject _buggyTutorial;
        // Animators to give some extra feedback when selecting menu items
        [FormerlySerializedAs("_planeAnimator")]
        [SerializeField] private Animator _biplaneAnimator;
        [SerializeField] private Animator _choppaAnimator;
        [SerializeField] private Animator _buggyAnimator;
        // Tutorial to remind players to open the UI
        [SerializeField] private GameObject _openUiTutorial;
        [SerializeField] private GameObject _chooseVehiclePrompt;
        [SerializeField] private Transform _content;
        [SerializeField] private ControllerInput _controllerInput;
        [SerializeField] private float _selectionDistance = 0.4f;
        [SerializeField] private float _animationTweenDuration = 1f;
        [SerializeField] private VehicleSelectionAudioBehavior _audioHandler;
        [SerializeField] private MLSpatialMapperController _mlSpatialMapperController;
        [SerializeField] private Vector3 _vehicleSpatialBounds;
        [SerializeField] private GameObject _menuAudioSourceLocation;
        private float _animateLerpValue;
        private Coroutine _animateInOutCoroutine;
        private bool _doSelection;
        private GameObject _selectedVehicle;
        private GameObject _currentActiveVehicle = null;

        //----------- MonoBehaviour Methods -----------
        
        private void Start()
        {
            // Subscribe to controller events
            _controllerInput.OnBumperDown += HandleBumperDown;
            
            // Disable the UI content
            _content.gameObject.SetActive(false);
            _chooseVehiclePrompt.SetActive(false);
        
            // Disable the in-world vehicles
            _biplane.SetActive(false);
            _choppa.SetActive(false);
            _buggy.SetActive(false);
        
            // Enable the tutorial explaining how to open the UI
            _openUiTutorial.SetActive(true);
        }

        private void OnDestroy()
        {
            // Unsubscribe from controller events
            _controllerInput.OnBumperDown -= HandleBumperDown;
        }

        private void Update()
        {
            if (_doSelection)
            {
                // Figure out if the user is selecting or deselecting one of the menu options
                GameObject prevSelected = _selectedVehicle;
                _selectedVehicle = SelectClosestVehicle();

                if (prevSelected != _selectedVehicle)
                {
                    if (_selectedVehicle != null)
                    {
                        // Disable the choose vehicle prompt as soon as a vehicle is selected.
                        _chooseVehiclePrompt.SetActive(false);
                    }
                    
                    if (prevSelected != null)
                    {
                        // We need at least 1 frame to reset our selectables back to their default anim state..
                        _selectedVehicle = null;
                    }

                    // Turn off tutorials of the deselected vehicle
                    SetVehicleDeselected(prevSelected);
                    // Turn on tutorials of the selected vehicle
                    SetVehicleSelected(_selectedVehicle);
                }
            }
            else
            {
                // Try to display the open-ui tutorial if it is disabled, and the UI is not transitioning
                if (!_openUiTutorial.activeSelf && _animateInOutCoroutine == null)
                {
                    // if all the vehicles are disabled then we show the user the tutorial to open the ui
                    if (_biplane.activeInHierarchy == _choppa.activeInHierarchy == _buggy.activeInHierarchy == false)
                    {
                        _openUiTutorial.SetActive(true);
                    }
                }
            }
        }

        //----------- Event Handlers -----------
        
        private void HandleBumperDown()
        {
            // Disable the playable vehicles
            _biplane.SetActive(false);
            _choppa.SetActive(false);
            _buggy.SetActive(false);

            // Either show or hide the menu
            if (_doSelection)
            {
                PlayOutro();
            }
            else
            {
                PlayIntro();
            }
        }

        //----------- Private Methods -----------
        
        private GameObject SelectClosestVehicle()
        {
            // Detect selected object, if any
            float planeDistance = Vector3.Distance(_controllerInput.position, _biplaneAnchor.position);
            float choppaDistance = Vector3.Distance(_controllerInput.position, _choppaAnchor.position);
            float buggyDistance = Vector3.Distance(_controllerInput.position, _buggyAnchor.position);

            if (planeDistance <= choppaDistance && planeDistance <= buggyDistance)
            {
                if (planeDistance < _selectionDistance)
                {
                    return _biplaneSelectable;
                }
            }
            else if (choppaDistance <= planeDistance && choppaDistance <= buggyDistance)
            {
                if (choppaDistance < _selectionDistance)
                {
                    return _choppaSelectable;
                }
            }
            else if (buggyDistance <= planeDistance && buggyDistance <= choppaDistance)
            {
                if (buggyDistance < _selectionDistance)
                {
                    return _buggySelectable;
                }
            }
        
            return null;
        }

        private void SetVehicleSelected(GameObject selectable)
        {
            if (selectable == _biplaneSelectable)
            {
                _biplaneAnimator.SetTrigger(SelectedTrigger);
                _choppaAnimator.SetTrigger(NotSelectedTrigger);
                _buggyAnimator.SetTrigger(NotSelectedTrigger);
                _biplaneTutorial.SetActive(true);
                _audioHandler.PlayVehicleSelected(true, selectable);
            }

            if (selectable == _choppaSelectable)
            {
                _biplaneAnimator.SetTrigger(NotSelectedTrigger);
                _choppaAnimator.SetTrigger(SelectedTrigger);
                _buggyAnimator.SetTrigger(NotSelectedTrigger);
                _choppaTutorial.SetActive(true);
                _audioHandler.PlayVehicleSelected(true, selectable);
            }

            if (selectable == _buggySelectable)
            {
                _biplaneAnimator.SetTrigger(NotSelectedTrigger);
                _choppaAnimator.SetTrigger(NotSelectedTrigger);
                _buggyAnimator.SetTrigger(SelectedTrigger);
                _buggyTutorial.SetActive(true);
                _audioHandler.PlayVehicleSelected(true, selectable);
            }

            if (selectable == null)
            {
                _biplaneAnimator.SetTrigger(DefaultTrigger);
                _choppaAnimator.SetTrigger(DefaultTrigger);
                _buggyAnimator.SetTrigger(DefaultTrigger);
            }
            else
            {
                _biplaneAnimator.ResetTrigger(DefaultTrigger);
                _choppaAnimator.ResetTrigger(DefaultTrigger);
                _buggyAnimator.ResetTrigger(DefaultTrigger);
            }
        }

        private void SetVehicleDeselected(GameObject selectable)
        {
            if (selectable == _biplaneSelectable)
            {
                _biplaneTutorial.SetActive(false);
                _audioHandler.PlayVehicleSelected(false, selectable);
            }

            if (selectable == _choppaSelectable)
            {
                _choppaTutorial.SetActive(false);
                _audioHandler.PlayVehicleSelected(false, selectable);
            }

            if (selectable == _buggySelectable)
            {
                _buggyTutorial.SetActive(false);
                _audioHandler.PlayVehicleSelected(false, selectable);
            }
        }

        private void PlayIntro()
        {
            // Move content to the controller's position
            _content.transform.position = _controllerInput.position;
        
            // Orient the content to face the headpose, while keeping it vertical
            Transform headpose = Camera.main.transform;
            Vector3 contentPosition = _content.transform.position;
            Vector3 contentToHeadpose = headpose.position - contentPosition;
            contentToHeadpose.y = 0f;
            Quaternion lookRot = Quaternion.LookRotation(contentToHeadpose, Vector3.up);
            _content.transform.rotation = lookRot;
        
            // Start Intro animation
            if (_animateInOutCoroutine != null)
            {
                StopCoroutine(_animateInOutCoroutine);
            }
            _animateInOutCoroutine = StartCoroutine(AnimateIn());
        }

        private void PlayOutro()
        {
            // Start Outro animation
            if (_animateInOutCoroutine != null)
            {
                StopCoroutine(_animateInOutCoroutine);
            }
            _animateInOutCoroutine = StartCoroutine(AnimateOut());
        }

        //----------- Coroutines -----------
        
        private IEnumerator AnimateIn()
        {
            // Enable the visual content
            _content.gameObject.SetActive(true);
            _audioHandler.PlayMenuOpen(_menuAudioSourceLocation);
        
            // Play the tweening animation
            do
            {
                _animateLerpValue += Time.deltaTime / _animationTweenDuration;
                TweenMeshes();
                yield return null;
            } 
            while (_animateLerpValue < 1f);
            yield return null;
        
            // Disable the open ui tutorial
            _openUiTutorial.SetActive(false);
            
            // Enable initial choose vehicle prompt
            _chooseVehiclePrompt.SetActive(true);
        
            // Start selecting
            _doSelection = true;
        
            // Clean up the state fields
            _animateInOutCoroutine = null;
        }

        private IEnumerator AnimateOut()
        {
            // Stop selecting
            _doSelection = false;
        
            // Play the tweening animation
            do
            {
                _animateLerpValue -= Time.deltaTime / _animationTweenDuration;
                TweenMeshes();
                // Make the selected vehicle do a little wobble before entering play
                if (_selectedVehicle != null)
                {
                    TweenMeshToScale(_selectedVehicle);
                }
                yield return null;
            } 
            while (_animateLerpValue > 0);
            yield return null;

            _audioHandler.PlayMenuClose(_menuAudioSourceLocation);

            // Disable the visual content
            _content.gameObject.SetActive(false);
            _chooseVehiclePrompt.SetActive(false);
        
            if (_selectedVehicle != null)
            {
                // Disable any animators or highlights of the selected ui vehicle
                SetVehicleDeselected(_selectedVehicle);
                // Enable selected vehicle and disable others, if any selection was made
                EnableSelectedVehicle();
            }

            // Clean up the state fields
            _selectedVehicle = null;
            _animateInOutCoroutine = null;
        }

        private void EnableSelectedVehicle()
        {
            // Disable previous active vehicle
            if (_currentActiveVehicle != null)
            {
                _mlSpatialMapperController.Remove(_currentActiveVehicle.transform);
                _currentActiveVehicle.SetActive(false);
            }

            // Move selected vehicle into the world
            if (_biplaneSelectable == _selectedVehicle)
            {
                _biplane.transform.position = _biplaneSelectable.transform.position;
                _biplane.transform.rotation = _biplaneSelectable.transform.rotation;
                _currentActiveVehicle = _biplane;
            }

            if (_choppaSelectable == _selectedVehicle)
            {
                _choppa.transform.position = _choppaSelectable.transform.position;
                _choppa.transform.rotation = _choppaSelectable.transform.rotation;
                _currentActiveVehicle = _choppa;
            }
        
            if (_buggySelectable == _selectedVehicle)
            {
                _buggy.transform.position = _buggySelectable.transform.position;
                _buggy.transform.rotation = _buggySelectable.transform.rotation;
                _currentActiveVehicle = _buggy;
            }

            _currentActiveVehicle.SetActive(true);
            _mlSpatialMapperController.Add(_currentActiveVehicle.transform, _vehicleSpatialBounds);
        }

        private void TweenMeshes()
        {
            if (_selectedVehicle != _biplaneSelectable)
            {
                TweenMeshToAnchor(_biplaneSelectable.gameObject, _biplaneAnchor);
            }
            if (_selectedVehicle != _choppaSelectable)
            {
                TweenMeshToAnchor(_choppaSelectable.gameObject, _choppaAnchor);
            }
            if (_selectedVehicle != _buggySelectable)
            {
                TweenMeshToAnchor(_buggySelectable.gameObject, _buggyAnchor);
            }
        }

        private void TweenMeshToAnchor(GameObject mesh, Transform anchor)
        {
            mesh.transform.localScale = Vector3.Lerp(Vector3.zero, anchor.localScale, _animateLerpValue);
            mesh.transform.localPosition = Vector3.Lerp(Vector3.zero, anchor.localPosition, _animateLerpValue);
            mesh.transform.localRotation = Quaternion.Slerp(Quaternion.identity, anchor.localRotation, _animateLerpValue);
        }

        private void TweenMeshToScale(GameObject mesh)
        {
            // convert _animateLerpValue from [0,1] to a smooth curve from 1 to 0.5 back to 1
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Abs(_animateLerpValue - 0.5f) + 0.5f);
            mesh.transform.localScale = Vector3.Lerp(Vector3.one/2f, Vector3.one, t);
        }
    }
}