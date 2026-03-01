using System;
using UnityEngine;
using Cinemachine;

public class CinemachineZoomController : MonoBehaviour
{
    [Range(1, 8), SerializeField, Header("기본 거리")]
    private float defaultDistance;

    [Range(1, 8), SerializeField, Header("최소 거리")]
    private float minDistance;

    [Range(1, 8), SerializeField, Header("최대 거리")]
    private float maxDistance;

    public float zoomSensitivity = 1;
    public float targetDistance;

    private CinemachineFramingTransposer cinemachineFramingTransposer;
    private AxisState.IInputAxisProvider _inputProvider;

    private void Awake()
    {
        cinemachineFramingTransposer = GetComponent<CinemachineVirtualCamera>()
            .GetCinemachineComponent<CinemachineFramingTransposer>();
        var gameProvider = GetComponent<JM.GameInputCameraProvider>();
        _inputProvider = gameProvider != null
            ? (AxisState.IInputAxisProvider)gameProvider
            : GetComponent<CinemachineInputProvider>();
        targetDistance = defaultDistance;
    }

    private void Update()
    {
        if (_inputProvider == null) return;
        float delta = -Math.Clamp(_inputProvider.GetAxisValue(2), -1, 1) * zoomSensitivity;
        targetDistance = Math.Clamp(targetDistance + delta, minDistance, maxDistance);
        cinemachineFramingTransposer.m_CameraDistance = targetDistance;
    }
}
