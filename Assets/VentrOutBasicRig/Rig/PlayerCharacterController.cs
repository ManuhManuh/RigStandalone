using BNG;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;


public class PlayerCharacterController : MonoBehaviour
{
    public float testSpeed = 1f;
    [SerializeField]
    private bool _disableXrSimulatorOnEditor;

    [Header("References")]
    [SerializeField]
    private Camera _PlayerRigCamera;
    [SerializeField]
    private SmoothLocomotion smoothLocomotion;

    [Header("Run Input")]
    [SerializeField]
    [Tooltip("Avg weighting value for runInput values below epsilon")]
    [Range(0f, 1f)]
    private float _BelowEpsilonWeightValue = 0f;
    [SerializeField]
    [Tooltip("Avg weighting value for runInput values above epsilon")]
    [Range(0f, 1f)]
    private float _AboveEpsilonWeightValue = 1f;
    public float RunInput => _runInput;
    private float _runInput = 0;
    private const int RunInputSize = 20;
    private float[] _runInputWindow = new float[RunInputSize];
    private int _runInputWindowIdx = 0;

    [Header("Head Input")]
    [SerializeField]
    private InputActionReference _HeadPositionAction;
    private List<float> headYPositionsBuffer = new List<float>();
    [SerializeField]
    private int _headYPositionsBufferSize = 30;
    [SerializeField]
    private float _runMinHeadPos = -0.05f;
    [SerializeField]
    private float _runMaxHeadPos = -0.005f;
    [SerializeField]
    private int _runMinHeadPeeksNumber = 2;
    [SerializeField]
    private int _runMaxHeadPeeksNumber = 7;


    [Header("Controller Input")]
    [SerializeField]
    private InputActionReference _LeftControllerPositionAction;
    [SerializeField]
    private InputActionReference _RightControllerPositionAction;
    [SerializeField]
    [Tooltip("The minimun controller velocity to run")]
    private float _minControllerVelocity = 0.1f;
    [SerializeField]
    [Tooltip("The maximum controller velovity to run")]
    private float _maxControllerVelocity = 2f;
    public float RightControllerFWVelocity { get; private set; }
    public float LeftControllerFWVelocity { get; private set; }
    private Vector3 _prevLeftControllerPosition;
    private Vector3 _prevRightControllerPosition;
    

    [Header("Settings")]
    [SerializeField]
    [Tooltip("Player speed modifiers")]
    private List<ISpeedModifier> speedCoefficients = new List<ISpeedModifier>();
    [SerializeField]
    [Tooltip("in Unity units/s")]
    private float _WalkSpeed = 2.5f;
    [SerializeField]
    [Tooltip("in Unity units/s")]
    private float _MinRunSpeed = 5f;
    [SerializeField]
    [Tooltip("in Unity units/s")]
    private float _MaxRunSpeed = 10f;

    public float currentSpeed;

    private void OnEnable()
    {
        if (_HeadPositionAction != null && !_HeadPositionAction.action.enabled)
        {
            _HeadPositionAction.action.Enable();
        }

        if (_LeftControllerPositionAction != null && !_LeftControllerPositionAction.action.enabled)
        {
            _LeftControllerPositionAction.action.Enable();
        }

        if (_RightControllerPositionAction != null && !_RightControllerPositionAction.action.enabled)
        {
            _RightControllerPositionAction.action.Enable();
        }
    }
    private void FixedUpdate()
    {
        UpdateRunValue();
        CalculateSpeed();
        smoothLocomotion.MovementSpeed = currentSpeed;
   
    }

    private void UpdateRunValue()
    {
        float newRunInput = Mathf.Max(GetHeadRunValue(), GetHandsRunValue());

        if (_runInputWindowIdx >= _runInputWindow.Length)
        {
            _runInputWindowIdx = 0;
        }

        _runInputWindow[_runInputWindowIdx++] = newRunInput;
        float sum = 0f;
        float weights = 0f;
        foreach (float val in _runInputWindow)
        {
            // Low values counts less than high ones
            if (val < Mathf.Epsilon)
            {
                weights += _BelowEpsilonWeightValue;
            }
            else
            {
                weights += _AboveEpsilonWeightValue;
            }

            sum += val;
        }

        _runInput = weights == 0? 0 : sum / weights;
        //Debug.Log($"RunInput: {_runInput}");
    }

    private float GetHeadRunValue()
    {
        if (_HeadPositionAction == null)
        {
            return 0;
        }

        int consecutiveAlternatePeeks = 0;
        Vector3 headPosition = _HeadPositionAction.action.ReadValue<Vector3>();
        headYPositionsBuffer.Add(headPosition.y);

        if (headYPositionsBuffer.Count > _headYPositionsBufferSize)
        {
            headYPositionsBuffer.RemoveAt(0);

            bool lastPeekNegative = false;
            foreach (float headYPosition in headYPositionsBuffer)
            {
                if (consecutiveAlternatePeeks == 0 && (headYPosition <= _runMinHeadPos || headYPosition >= _runMaxHeadPos))
                {
                    ++consecutiveAlternatePeeks;
                    lastPeekNegative = headYPosition <= _runMinHeadPos;
                }
                else if (consecutiveAlternatePeeks > 0 && ((headYPosition <= _runMinHeadPos && !lastPeekNegative) || (headYPosition >= _runMaxHeadPos && lastPeekNegative)))
                {
                    ++consecutiveAlternatePeeks;
                    lastPeekNegative = !lastPeekNegative;
                }
            }
        }

        float headRunInput = Mathf.Clamp01(Mathf.InverseLerp(_runMinHeadPeeksNumber, _runMaxHeadPeeksNumber, consecutiveAlternatePeeks));
        
        return headRunInput;
    }

    private float GetHandsRunValue()
    {
        if (_LeftControllerPositionAction == null || _RightControllerPositionAction == null)
        {
            Debug.Log("One controller position action is null");
            return 0;
        }

        Vector3 leftControllerPosition = _LeftControllerPositionAction.action.ReadValue<Vector3>();
        Vector3 rightControllerPosition = _RightControllerPositionAction.action.ReadValue<Vector3>();

#if UNITY_EDITOR
        if (!_disableXrSimulatorOnEditor)
        {
            //to be able to test in editor with the XR simulator where you have to move the controllers in the same direction if they are moved together
            rightControllerPosition = -rightControllerPosition;
        }
#endif

        float handsRunInput = 0;

        Vector3 lDir = leftControllerPosition - _prevLeftControllerPosition;
        Vector3 rDir = rightControllerPosition - _prevRightControllerPosition;

        Vector3 lDirWorld = transform.TransformDirection(lDir);
        Vector3 rDirWorld = transform.TransformDirection(rDir);

        Vector3 forward = _PlayerRigCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 lDirOnForward = Vector3.Project(lDirWorld, forward);
        Vector3 rDirOnForward = Vector3.Project(rDirWorld, forward);

        if (Vector3.Dot(lDirOnForward, rDirOnForward) < -Mathf.Epsilon)
        {
            Debug.Log($"Time divisor: {Time.fixedDeltaTime}");
            LeftControllerFWVelocity = lDirOnForward.magnitude / Time.fixedDeltaTime;
            RightControllerFWVelocity = rDirOnForward.magnitude / Time.fixedDeltaTime;
            float velocity = Mathf.Min(LeftControllerFWVelocity, RightControllerFWVelocity) * 10;
            //Debug.Log($"Controller velocity: left {LeftControllerFWVelocity}; right {RightControllerFWVelocity}");
            handsRunInput = Mathf.Clamp01(Mathf.InverseLerp(_minControllerVelocity, _maxControllerVelocity, velocity));
        }

        _prevLeftControllerPosition = leftControllerPosition;
        _prevRightControllerPosition = rightControllerPosition;
        //Debug.Log($"handsRunInput: {handsRunInput}");
        return handsRunInput;
    }


    private float CalculateSpeed()
    {
        float speedCoefficient = 1f;
        float previousCurrentSpeed = currentSpeed;
        float newCurrentSpeed = 0f;

        foreach (ISpeedModifier speedModifier in speedCoefficients)
        {
            speedCoefficient *= speedModifier.SpeedModifier;
        }

        speedCoefficient *= testSpeed;

        if (_runInput == 0)
        {
            // if there is no run input, the player is walking - adjust for modifiers
            newCurrentSpeed = _WalkSpeed * speedCoefficient;
        }
        else
        {
            // if there is run input, the player is running - calculate speed based on running input plus modifiers
            newCurrentSpeed = Mathf.Lerp(_MinRunSpeed, _MaxRunSpeed, _runInput) * speedCoefficient;

        }

        currentSpeed = Mathf.Lerp(previousCurrentSpeed, newCurrentSpeed, Time.deltaTime);
       
        return currentSpeed;
    }

    private void OnDisable()
    {
        if (_HeadPositionAction != null && _HeadPositionAction.action.enabled)
        {
            _HeadPositionAction.action.Disable();
        }

        if (_LeftControllerPositionAction != null && _LeftControllerPositionAction.action.enabled)
        {
            _LeftControllerPositionAction.action.Disable();
        }

        if (_RightControllerPositionAction != null && _RightControllerPositionAction.action.enabled)
        {
            _RightControllerPositionAction.action.Disable();
        }
    }

}
