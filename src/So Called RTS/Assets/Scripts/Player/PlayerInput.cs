using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private float _keyboardPanSpeed = 5f;
        [SerializeField] private float _zoomSpeed = 1f;
        [SerializeField] private float _rotationSpeed = 1f;
        [SerializeField] private float _minZoomDistance = 7.5f;

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

            float rotationTime = Mathf.Clamp01((Time.time - _rotationStartTime) * _rotationSpeed);  // вычисляем время, прошедшее с начала вращения                                                                                                

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
}