using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody _cameraTarget;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CameraConfig _cameraConfig;

        private CinemachineFollow _cinemachineFollow;
        private float _zoomStartTime;
        private float _rotationStartTime;
        private Vector3 _startingFollowOffset;
        private float _maxRotationAmount;

        private void Awake()
        {
            if (!_cinemachineCamera.TryGetComponent(out _cinemachineFollow))  // проверка на null
            {
                Debug.LogError("Cinemachine Camera не нашла Cinemachine Follow. Зум работать не будет!");
            }

            _startingFollowOffset = _cinemachineFollow.FollowOffset;          // сохраняем начальное смещение камеры для последующего использования
            _maxRotationAmount = Mathf.Abs(_cinemachineFollow.FollowOffset.z);
        }

        private void Update()
        {
            HandlePanning(); // вызываем метод для обработки перемещения камеры

            HandleZooming(); // вызываем метод для обработки зумирования камеры

            HandleRotation(); // вызываем метод для обработки вращения камеры
        }

        private void HandleRotation()
        {
            if (ShouldSetRotationStartTime())
            {
                _rotationStartTime = Time.time;  // сохраняем время начала вращения
            }

            float rotationTime = Mathf.Clamp01((Time.time - _rotationStartTime) * _cameraConfig.RotationSpeed);  // вычисляем время, прошедшее с начала вращения                                                                                                

            Vector3 targetFollowOffset;

            if (Keyboard.current.pageDownKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    _maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else if (Keyboard.current.pageUpKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    -_maxRotationAmount,
                    _cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    _startingFollowOffset.x,
                    _cinemachineFollow.FollowOffset.y,
                    _startingFollowOffset.z
                );
            }

            _cinemachineFollow.FollowOffset = Vector3.Slerp(
                _cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime);
        }

        private bool ShouldSetRotationStartTime()
        {
            return Keyboard.current.pageUpKey.wasPressedThisFrame
                || Keyboard.current.pageDownKey.wasPressedThisFrame
                || Keyboard.current.pageUpKey.wasReleasedThisFrame
                || Keyboard.current.pageDownKey.wasReleasedThisFrame;
        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            _cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0f, moveAmount.y);  // задаем линейную скорость камеры по оси X и Y, оставляя Z неизменным
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (!_cameraConfig.EnableEdgePan) // если EdgePan отключен, возвращаем нулевой вектор
            {
                return moveAmount;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();  // получаем текущую позицию мыши
            int screenWidth = Screen.width;                              // получаем ширину экрана - 1920
            int screenHeight = Screen.height;                            // 1080

            if (mousePosition.x <= _cameraConfig.EdgePanSize)  // если мышь находится в левой части экрана
            {
                moveAmount.x -= _cameraConfig.MousePanSpeed;           // перемещаем камеру влево
            }
            else if (mousePosition.x >= screenWidth - _cameraConfig.EdgePanSize)  // если мышь находится в правой части экрана
            {
                moveAmount.x += _cameraConfig.MousePanSpeed;           // перемещаем камеру вправо
            }

            if (mousePosition.y >= screenHeight - _cameraConfig.EdgePanSize)  // если мышь находится в верхней части экрана
            {
                moveAmount.y += _cameraConfig.MousePanSpeed;           // перемещаем камеру вверх
            }
            else if (mousePosition.y <= _cameraConfig.EdgePanSize)  // если мышь находится в нижней части экрана
            {
                moveAmount.y -= _cameraConfig.MousePanSpeed;           // перемещаем камеру вниз
            }

            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += _cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= _cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= _cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += _cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }

        private void HandleZooming()
        {
            if (ShouldSetZoomStartTime())
            {
                _zoomStartTime = Time.time;                     // сохраняем время начала зумирования
            }

            float zoomTime = Mathf.Clamp01((Time.time - _zoomStartTime) * _cameraConfig.ZoomSpeed);  // вычисляем время, прошедшее с начала зумирования
                                                                                        // ограничиваем значение между 0 и 1 с помощью Mathf.Clamp01, чтобы избежать выхода за пределы диапазона

            Vector3 targetFollowOffset;

            if (Keyboard.current.endKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                _cinemachineFollow.FollowOffset.x,
                _cameraConfig.MinZoomDistance,
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
}