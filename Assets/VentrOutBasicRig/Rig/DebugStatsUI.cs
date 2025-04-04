using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Profiling;

public class DebugStatsUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float _HudRefreshRate = 1f;

    [Header("References")]
    [SerializeField]
    private PlayerCharacterController playerCharacterController;
    [SerializeField]
    private TMP_Text _DebugText;

    [Header("Input")]
    [SerializeField]
    private bool _DebugInput;

    [SerializeField]
    private InputActionReference _MovementAction;
    [SerializeField]
    private InputActionReference _RunAction;

    private float _timer;
    private int _fps = 0;
    private int _maxFPS = Int32.MinValue;
    private ProfilerRecorder _setPassCallsRecorder;
    private ProfilerRecorder _drawCallsRecorder;

    private Vector3 prevLeft = Vector3.zero;
    private Vector3 prevRight = Vector3.zero;

    private void Awake()
    {
        _setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        _drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
    }

    void Update()
    {
        _DebugText.text = string.Empty;
        UpdateStats();

        if (_DebugInput)
        {
            UpdateInput();
        }

        if (Time.unscaledTime > _timer)
        {
            _timer = Time.unscaledTime + _HudRefreshRate;
        }

    }

    void UpdateStats()
    {
        if (Time.unscaledTime > _timer)
        {
            _fps = (int)(1f / Time.unscaledDeltaTime);
            if (_fps > _maxFPS) _maxFPS = _fps;
        }
        _DebugText.text = string.Empty;
        _DebugText.text = $"FPS: {_fps} MAX: {_maxFPS}\n" +
                          $"MALLOC: {Profiler.GetTotalAllocatedMemoryLong() >> 20} MB / {Profiler.GetTotalReservedMemoryLong() >> 20} MB (SYSMEM: {SystemInfo.systemMemorySize} MB)\n" +
                          $"SET_PASS: {_setPassCallsRecorder.LastValue}\n" +
                          $"DRAW_CALLS: {_drawCallsRecorder.LastValue}\n" +
                          $"GRAPHICS_DEVICE_TYPE: {SystemInfo.graphicsDeviceType}\n";
    }

    void UpdateInput()
    {
        _DebugText.text += $"INPUT:\n";
        _DebugText.text += $"RUN: {playerCharacterController.RunInput:0.00}\n";
        _DebugText.text += $"VELOCITY: {playerCharacterController.CurrentVelocity:0.00} Units/s\n";
    }
}
