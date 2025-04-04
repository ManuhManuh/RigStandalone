using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacterController : MonoBehaviour
{
    public float testSpeed = 1f;
    [Header("References")]
    [SerializeField]
    private Camera _PlayerRigCamera;
    [SerializeField]
    private CapsuleCollider _PlayerCollider;
    [SerializeField]
    private GameObject _XrSimulator;
    [SerializeField]
    private JumpHeadTrigger _JumpHeadStandTrigger;
    [SerializeField]
    private JumpHeadTrigger _JumpHeadDuckTrigger;


    [Header("Input")]
    [SerializeField]
    private bool useHeadForDirection;
    [SerializeField]
    private InputActionReference _MovementAction;
    [SerializeField]
    private InputActionReference _RunAction;
    [SerializeField]
    private InputActionReference _RunEnabledAction;
    [SerializeField]
    private InputActionReference _JumpAction;
    [SerializeField]
    private InputActionReference _HeadPositionAction;
    [SerializeField]
    private InputActionReference _LeftControllerPositionAction;
    [SerializeField]
    private InputActionReference _RightControllerPositionAction;
    [SerializeField]
    private InputActionReference _MapAction;
    [SerializeField]
    private InputActionReference _primaryEquip;
    [SerializeField]
    private InputActionReference _secondaryEquip;
    [SerializeField]
    private InputActionReference _inventoryOpen;


    [Header("Settings")]
    [SerializeField]
    [Tooltip("in Unity units/s")]
    private float _WalkSpeed = 2.5f;
    [SerializeField]
    [Tooltip("in Unity units/s")]
    private float _MinRunSpeed = 5f;
    [SerializeField]
    [Tooltip("in Unity units/s")]
    private float _MaxRunSpeed = 10f;
    [SerializeField]
    [Tooltip("in Unity units/s, speed required to activate the heart beat sound")]
    private float _HeartBeatSpeed = 2.5f;
    [SerializeField]
    [Tooltip("Angle at which running becomes more difficult")]
    private float stressAngle = 45f;
    [SerializeField]
    [Tooltip("Amount of influence slope has on uphill speed")]
    private float uphillWeight = 1f;
    [SerializeField]
    [Tooltip("Amount of influence slope has on downhill speed")]
    private float downhillWeight = 1f;
    [SerializeField]
    private float _JumpForce = 5;
    [SerializeField]
    private float _MaxDuckTimeToJump = 0.5f;
    [SerializeField]
    private LayerMask _GroundLayer;
    [SerializeField]
    [Range(0, 1)]
    private float _HeartBeatSoundVolume = 0.3f;
    [SerializeField]
    [Range(0, 0.5f)]
    private float _SfxsStopDelay = 0.1f;

    [SerializeField]
    [Tooltip("The minimun controller velocity to run")]
    private float _minControllerVelocity = 0.1f;
    [SerializeField]
    [Tooltip("The maximum controller velovity to run")]
    private float _maxControllerVelocity = 2f;

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

    [SerializeField]
    [Tooltip("Avg weighting value for runInput values below epsilon")]
    [Range(0f, 1f)]
    private float _BelowEpsilonWeightValue = 0f;
    [SerializeField]
    [Tooltip("Avg weighting value for runInput values above epsilon")]
    [Range(0f, 1f)]
    private float _AboveEpsilonWeightValue = 1f;

    [SerializeField]
    [Tooltip("Player speed modifiers")]
    private List<ISpeedModifier> speedCoefficients = new List<ISpeedModifier>();

    [SerializeField]
    private bool _disableXrSimulatorOnEditor;

    private Rigidbody _rb;
    private float xRotation = 0f;
    private Vector3 _currVelocity;

    private float _runInput = 0;
    public float RunInput => _runInput;

    public float CurrentVelocity => _rb.velocity.magnitude;

    private bool _isGrounded;
    private bool _isDucked;
    private float _duckTimer = 0;

    private Vector3 _prevLeftControllerPosition;
    private Vector3 _prevRightControllerPosition;

    private AudioSource _runningAudioSource = null;
    private AudioSource _heartBeatAudioSource = null;

    private Coroutine _delayedStopRunSfxCoroutine = null;
    private Coroutine _delayedStopHeartBeatSfxCoroutine = null;

    private const int RunInputSize = 20;
    private float[] _runInputWindow = new float[RunInputSize];
    private int _runInputWindowIdx = 0;

    public float LeftControllerFWVelocity { get; private set; }
    public float RightControllerFWVelocity { get; private set; }

    private List<float> headYPositionsBuffer = new List<float>();

    public Toggle immersiveMode;
    public ActionBasedSnapTurnProvider snapTurn;
    public LocomotionSystem locomotionSystem;
    private float currentRunSpeed;
    private Vector3 savedForward = new Vector3();
    private Vector3 savedRight = new Vector3();

    public float currentSpeed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_XrSimulator != null)
        {
#if UNITY_EDITOR
            if (_disableXrSimulatorOnEditor)
            {
                Destroy(_XrSimulator);
            }
            else
            {
                _XrSimulator.SetActive(true);
            }
#else
            Destroy(_XrSimulator);
#endif
        }
    }

    private void OnEnable()
    {
        _JumpHeadStandTrigger.Triggered += OnStand;
        _JumpHeadDuckTrigger.Triggered += OnDuck;
        _secondaryEquip.action.performed += ctx => ToggleDirectionFromHead();

        if (_MovementAction != null && !_MovementAction.action.enabled)
        {
            _MovementAction.action.Enable();
        }

        if (_RunAction != null && !_RunAction.action.enabled)
        {
            _RunAction.action.Enable();
        }

        if (_RunEnabledAction != null && !_RunEnabledAction.action.enabled)
        {
            _RunEnabledAction.action.Enable();
        }

        if (_JumpAction != null && !_JumpAction.action.enabled)
        {
            _JumpAction.action.Enable();
        }

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

        if (_MapAction != null && !_MapAction.action.enabled)
        {
            _MapAction.action.Enable();
        }

        if (_primaryEquip != null && !_primaryEquip.action.enabled)
        {
            _primaryEquip.action.Enable();
        }

    }

    private void Update()
    {
        //make sure the jump colliders follow the head
        _JumpHeadDuckTrigger.transform.localPosition = new Vector3(_PlayerRigCamera.transform.localPosition.x, _JumpHeadDuckTrigger.transform.localPosition.y, _PlayerRigCamera.transform.localPosition.z);
        _JumpHeadStandTrigger.transform.localPosition = new Vector3(_PlayerRigCamera.transform.localPosition.x, _JumpHeadStandTrigger.transform.localPosition.y, _PlayerRigCamera.transform.localPosition.z);

        //duck timer
        if (_isDucked)
        {
            _duckTimer += Time.deltaTime;
            if (_duckTimer >= _MaxDuckTimeToJump)
            {
                _duckTimer = 0;
                _isDucked = false;
            }
        }
        else
        {
            _duckTimer = 0;
        }

        UpdateJump();
    }

    private void FixedUpdate()
    {
        UpdateRunValue();

        UpdateMovement();
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

        _runInput = sum / weights;
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
        //Debug.Log($"headRunInput: {headRunInput}");
        return headRunInput;
    }

    private float GetHandsRunValue()
    {
        if (_LeftControllerPositionAction == null || _RightControllerPositionAction == null)
        {
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
            LeftControllerFWVelocity = lDirOnForward.magnitude / Time.fixedDeltaTime;
            RightControllerFWVelocity = rDirOnForward.magnitude / Time.fixedDeltaTime;
            float velocity = Mathf.Min(LeftControllerFWVelocity, RightControllerFWVelocity);
            handsRunInput = Mathf.Clamp01(Mathf.InverseLerp(_minControllerVelocity, _maxControllerVelocity, velocity));
        }

        _prevLeftControllerPosition = leftControllerPosition;
        _prevRightControllerPosition = rightControllerPosition;
        //Debug.Log($"handsRunInput: {handsRunInput}");
        return handsRunInput;
    }

    private void UpdateMovement()
    {
        var movementInput = _MovementAction != null ? _MovementAction.action.ReadValue<Vector2>() : Vector2.zero;
#if UNITY_EDITOR
        if (_runInput == 0)
        {
            _runInput = _RunAction != null ? _RunAction.action.ReadValue<float>() : 0;
        }
#endif

        // Get direction
        Vector3 forward = GetDirection(true, movementInput);
        Vector3 right = GetDirection(false, movementInput);
        // Get speed
        float currentMovementSpeed = CalculateSpeed();

        _currVelocity = _rb.velocity;
        var yVel = _currVelocity.y;

        if (useHeadForDirection)
        {
            //_currVelocity = forward * ((_runInput > 0f ? currentRunSpeed : _WalkSpeed) * movementInput.y) + right * ((_runInput > 0f ? 0f : _WalkSpeed) * movementInput.x);
            _currVelocity = forward * (currentMovementSpeed * movementInput.y) + right * ((_runInput > 0f ? 0f : currentMovementSpeed) * movementInput.x);
        }
        else
        {
            //_currVelocity = forward * ((_runInput > 0f ? currentRunSpeed : _WalkSpeed) * movementInput.y) + (right * movementInput.x);
            _currVelocity = forward * (currentMovementSpeed * movementInput.y) + (right * movementInput.x);

        }
        
        _currVelocity.y = yVel;
        _rb.velocity = _currVelocity;

    }

    private Vector3 GetDirection(bool getForward, Vector2 movementInput)
    {
        Vector3 movementDirection;

        if (useHeadForDirection)    //normal, original movement based on camera direction
        {
            if (getForward)
            {
                movementDirection = _PlayerRigCamera.transform.forward;
            }
            else
            {
                movementDirection = _PlayerRigCamera.transform.right;
            }
        }
        else   //movement based on controller direction, reset to camera direction when released
        {
            if (_runInput == 0 && movementInput == Vector2.zero)  // not moving, reset to camera direction
            {
                if (getForward)
                {
                    savedForward = Vector3.zero;
                    movementDirection = _PlayerRigCamera.transform.forward;
                }
                else
                {
                    savedRight = Vector3.zero;
                    movementDirection = _PlayerRigCamera.transform.right;
                }

            }

            else
            {
                if (getForward)
                {
                    if (savedForward == Vector3.zero)  //first frame of moving, save camera direction
                    {
                        savedForward = _PlayerRigCamera.transform.forward;
                    }
                    movementDirection = savedForward;  // use previously saved forward direction
                }
                else
                {
                    if (savedRight == Vector3.zero)  //first frame of moving, save camera direction
                    {
                        savedRight = _PlayerRigCamera.transform.right;
                    }
                    movementDirection = savedRight;  // use previously saved right direction
                }

            }

        }

        movementDirection.y = 0f;
        movementDirection.Normalize();
        return movementDirection;

    }

    private float CalculateSpeed()
    {
        float speedCoefficient = 1f;

        foreach (ISpeedModifier speedModifier in speedCoefficients)
        {
            speedCoefficient *= speedModifier.SpeedModifier;
        }

        if (_runInput == 0)
        {
            // if there is no run input, the player is walking - adjust for modifiers
            currentSpeed = _WalkSpeed * speedCoefficient;
        }
        else
        {
            // if there is run input, the player is running - calculate speed based on running input plus modifiers
            currentSpeed = Mathf.Lerp(_MinRunSpeed, _MaxRunSpeed, _runInput) * speedCoefficient;

        }
        //Debug.Log($"Returning speed: {currentSpeed}");
        return currentSpeed;
    }
    private void CheckGround()
    {
        Vector3 colliderBottomPosition = transform.position + _PlayerCollider.center;
        colliderBottomPosition.y -= ((_PlayerCollider.height / 2) - 0.05f);
        _isGrounded = Physics.Raycast(colliderBottomPosition, Vector3.down, 0.25f, _GroundLayer);
    }

    private void UpdateJump()
    {
        CheckGround();

        if (_isGrounded && _JumpAction != null && _JumpAction.action.triggered)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if (!_isGrounded)
        {
            return;
        }

        _rb.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
    }

    private void OnStand()
    {
        if (_isDucked)
        {
            Jump();
        }

        _isDucked = false;
        _duckTimer = 0;
    }

    private void OnDuck()
    {
        _isDucked = true;
    }

    private void ToggleDirectionFromHead()
    {
        useHeadForDirection = !useHeadForDirection;
    }
    private void OnDisable()
    {
        _JumpHeadStandTrigger.Triggered -= OnStand;
        _JumpHeadDuckTrigger.Triggered -= OnDuck;

        if (_MovementAction != null && _MovementAction.action.enabled)
        {
            _MovementAction.action.Disable();
        }

        if (_RunAction != null && _RunAction.action.enabled)
        {
            _RunAction.action.Disable();
        }

        if (_RunEnabledAction != null && _RunEnabledAction.action.enabled)
        {
            _RunEnabledAction.action.Disable();
        }

        if (_JumpAction != null && _JumpAction.action.enabled)
        {
            _JumpAction.action.Disable();
        }

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

        if (_primaryEquip != null && _primaryEquip.action.enabled)
        {
            _primaryEquip.action.Disable();
        }

    }
}
