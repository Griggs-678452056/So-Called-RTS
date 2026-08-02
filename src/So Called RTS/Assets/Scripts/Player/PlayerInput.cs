using Scripts.EventBus;
using Scripts.Events;
using Scripts.Units;
using System;
using System.Collections.Generic;
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
        private HashSet<AbstractUnit> _aliveUnits = new(100);
        private HashSet<AbstractUnit> _addedUnits = new(24);
        private List<ISelectable> _selectedUnits = new(12);

        private void Awake()
        {
            if (!_cinemachineCamera.TryGetComponent(out _cinemachineFollow))  // проверка на null
            {
                Debug.LogError("Cinemachine Camera не нашла Cinemachine Follow. Зум работать не будет!");
            }

            _startingFollowOffset = _cinemachineFollow.FollowOffset;          // сохраняем начальное смещение камеры для последующего использования
            _maxRotationAmount = Mathf.Abs(_cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
        }

        private void HandleUnitSpawn(UnitSpawnEvent evt)
        {
            _aliveUnits.Add(evt.Unit);
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            _selectedUnits.Add(evt.Unit);
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            _selectedUnits.Remove(evt.Unit);
        }

        private void Update()
        {
            HandlePanning(); // вызываем метод для обработки перемещения камеры
            HandleZooming(); // вызываем метод для обработки зумирования камеры
            HandleRotation(); // вызываем метод для обработки вращения камеры
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
                HandleMouseDown();
            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseUp()
        {
            if (!Keyboard.current.shiftKey.isPressed)
            {
                DeselectAllUnits();
            }

            HandleLeftClick();
            foreach (AbstractUnit unit in _addedUnits)
            {
                unit.Select();
            }
            _selectionBox.gameObject.SetActive(false); // деактивируем UI элемент для выделения
        }

        private void HandleMouseDrag()
        {
            Bounds selectionBoxBounds = ResizeSelectionBox();
            foreach (AbstractUnit unit in _aliveUnits)
            {
                Vector2 unitPosition = _camera.WorldToScreenPoint(unit.transform.position); // получаем позицию юнита на экране

                if (selectionBoxBounds.Contains(unitPosition)) // проверяем, находится ли юнит внутри выделения
                {
                    _addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseDown()
        {
            _selectionBox.sizeDelta = Vector2.zero;
            _selectionBox.gameObject.SetActive(true); // активируем UI элемент для выделения
            _startingMousePosition = Mouse.current.position.ReadValue(); // сохраняем начальную позицию мыши
            _addedUnits.Clear();
        }

        private void DeselectAllUnits()
        {
            ISelectable[] currentlySelectedUnits = _selectedUnits.ToArray(); // создаем копию списка выделенных юнитов
            foreach (ISelectable selectable in currentlySelectedUnits)
            {
                selectable.Deselect(); // снимаем выделение с каждого юнита
            }
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue(); // получаем текущую позицию мыши

            float width = mousePosition.x - _startingMousePosition.x; // вычисляем ширину выделения
            float height = mousePosition.y - _startingMousePosition.y; // вычисляем высоту выделения

            _selectionBox.anchoredPosition = _startingMousePosition + new Vector2(width / 2, height / 2); // задаем позицию UI элемента для выделения
            _selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height)); // обновляем размер UI элемента

            return new Bounds(_selectionBox.anchoredPosition, _selectionBox.sizeDelta); // возвращаем границы выделения
        }

        private void HandleRightClick()
        {
            if (_selectedUnits.Count == 0)
            {
                return;
            }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasReleasedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _floorLayers))
            {
                List<AbstractUnit> abstractUnits = new List<AbstractUnit>(_selectedUnits.Count);
                foreach (ISelectable selectable in _selectedUnits)
                {
                    if (selectable is AbstractUnit unit)
                    {
                        abstractUnits.Add(unit);
                    }
                }

                int unitsOnLayer = 0;
                int maxUnitsOnLayer = 1;
                float circleRadius = 0;
                float radiusOffset = 0;

                foreach (AbstractUnit unit in abstractUnits)
                {
                    Vector3 targetPosition = new(
                        hit.point.x + circleRadius * Mathf.Cos(radiusOffset * unitsOnLayer),
                        hit.point.y,
                        hit.point.z + circleRadius * Mathf.Sin(radiusOffset * unitsOnLayer)
                        );

                    unit.MoveTo(targetPosition);
                    unitsOnLayer++;

                    if (unitsOnLayer >= maxUnitsOnLayer)
                    {
                        unitsOnLayer = 0;
                        circleRadius += unit.AgentRadius * 3.5f;
                        maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * 2));
                        radiusOffset = 2 * Mathf.PI / maxUnitsOnLayer;
                    }
                }

                //foreach (ISelectable selectable in _selectedUnits)
                //{
                //    if (selectable is IMovable movable)
                //    {
                //        movable.MoveTo(hit.point);
                //    }
                //}
            }
        }


        private void HandleLeftClick()
        {
            if (_camera == null)
            {
                return;
            }

            Ray cameraRay = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, _selectableUnitsLayers)
                && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
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