using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private float _keyboardPanSpeed = 10f;
    [SerializeField] private float _zoomSpeed = 0.75f;
    [SerializeField] private float _minZoomDistance = 7.5f;

    private CinemachineFollow _cinemachineFollow;
    private float _zoomStartTime;
    private Vector3 _startingFollowOffset;

    private void Awake()
    {
        if (!_cinemachineCamera.TryGetComponent(out _cinemachineFollow))  // проверка на null
        {
            Debug.LogError("Cinemachine Camera не нашла Cinemachine Follow. Зум работать не будет!");
        }

        _startingFollowOffset = _cinemachineFollow.FollowOffset;          // сохраняем начальное смещение камеры для последующего использования
    }

    private void Update()
    {
        HandlePanning(); // вызываем метод для обработки перемещения камеры

        HandleZooming(); // вызываем метод для обработки зумирования камеры
    }

    private void HandlePanning()
    {
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)       // смещение камеры по оси Y при нажатии клавиш стрелок
        {
            moveAmount.y += _keyboardPanSpeed;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= _keyboardPanSpeed;
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= _keyboardPanSpeed;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += _keyboardPanSpeed;
        }

        moveAmount *= Time.deltaTime;                                           // умножаем на Time.deltaTime для плавного движения камеры
        _cameraTarget.position += new Vector3(moveAmount.x, 0f, moveAmount.y);  // перемещаем камеру в пространстве по оси X и Y, оставляя Z неизменным
    }

    private void HandleZooming()
    {
        if (ShouldSetZoomStartTime())
        {
            _zoomStartTime = Time.time;                     // сохраняем время начала зумирования
        }

        float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _zoomSpeed);  // вычисляем время, прошедшее с начала зумирования
                                                                                    // ограничиваем значение между 0 и 1 с помощью Mathf.Clamp01, чтобы избежать выхода за пределы диапазона

        Vector3 targetFollowOffset;

        if (Keyboard.current.endKey.isPressed)
        {
            targetFollowOffset = new Vector3(
            _cinemachineFollow.FollowOffset.x,
            _minZoomDistance,
            _cinemachineFollow.FollowOffset.z
            );
        }
        else
        {
            targetFollowOffset = new Vector3(
            _cinemachineFollow.FollowOffset.x,
            _startingFollowOffset.y,
            _cinemachineFollow.FollowOffset.z
            );
        }

        _cinemachineFollow.FollowOffset = Vector3.Slerp(
            _cinemachineFollow.FollowOffset,
            targetFollowOffset,
            zoomTime);  // плавное смещение камеры с помощью Vector3.Slerp, чтобы создать эффект зумирования
    }

    private bool ShouldSetZoomStartTime()
    {
        return Keyboard.current.endKey.wasPressedThisFrame || Keyboard.current.endKey.wasReleasedThisFrame;
    }
}
