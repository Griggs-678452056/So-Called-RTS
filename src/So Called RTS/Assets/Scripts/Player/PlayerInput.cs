using Scripts.EventBus;
using Scripts.Events;
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
        [SerializeField] private Camera _camera;
        [SerializeField] private CameraConfig _cameraConfig;
        [SerializeField] private LayerMask _selectableUnitsLayers;
        [SerializeField] private LayerMask _floorLayers;
        [SerializeField] private RectTransform _selectionBox;

        private Vector2 _startingMousePosition;

        private CinemachineFollow _cinemachineFollow;
        private float _zoomStartTime;
        private float _rotationStartTime;
        private Vector3 _startingFollowOffset;
        private float _maxRotationAmount;
        private ISelectable _selectedUnit;

        private void Awake()
        {
            if (!_cinemachineCamera.TryGetComponent(out _cinemachineFollow))  // проверка на null
            {
                Debug.LogError("Cinemachine Camera не нашла Cinemachine Follow. Зум работать не будет!");
            }

            _startingFollowOffset = _cinemachineFollow.FollowOffset;          // сохраняем начальное смещение камеры для последующего использования
            _maxRotationAmount = Mathf.Abs(_cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            if (_selectedUnit != null)
            {
                _selectedUnit.Deselect();
            }

            _selectedUnit = evt.Unit;
        }

        private void Update()
        {
            HandlePanning(); // вызываем метод для обработки перемещения камеры
            HandleZooming(); // вызываем метод для обработки зумирования камеры
            HandleRotation(); // вызываем метод для обработки вращения камеры
            HandleLeftClick();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if (_selectionBox == null)
            {
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _selectionBox.gameObject.SetActive(true); // активируем UI элемент для выделения
                _startingMousePosition = Mouse.current.position.ReadValue(); // сохраняем начальную позицию мыши
            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ResizeSelectionBox();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // выбираем новые юниты
                // юниты за пределами выделения должны быть сняты с выделения
                _selectionBox.gameObject.SetActive(false); // деактивируем UI элемент для выделения
            }
        }

        private void ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue(); // получаем текущую позицию мыши

            float width = mousePosition.x - _startingMousePosition.x; // вычисляем ширину выделения
            float height = mousePosition.y - _startingMousePosition.y; // вычисляем высоту выделения

            _selectionBox.anchoredPosition = _startingMousePosition + new Vector2(width / 2, height / 2); // задаем позицию UI элемента для выделения
            _selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height)); // обновляем размер UI элемента
        }

        private void HandleRightClick()
        {
            if (_selectedUnit == null || _selectedUnit is not IMovable movable)
            {
                return;
            }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasReleasedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _floorLayers))
            {
                movable.MoveTo(hit.point);
            }
        }


        private void HandleLeftClick()
        {
            if (_camera == null)
            {
                return;
            }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (_selectedUnit != null)
                {
                    _selectedUnit.Deselect();
                    _selectedUnit = null;
                }

                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
                {
                    selectable.Select();
                }
            }
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