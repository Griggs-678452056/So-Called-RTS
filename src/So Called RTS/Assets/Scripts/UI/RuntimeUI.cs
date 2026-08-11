using Scripts.EventBus;
using Scripts.Events;
using Scripts.UI.Containers;
using Scripts.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private ActionsUI _actionsUI;
        [SerializeField] private BuildingBuildingUI _buildingBuildingUI;
        private HashSet<AbstractCommandable> _selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
        }

        private void Start()
        {
            _actionsUI.Disable();
            _buildingBuildingUI.Disable();
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                _selectedUnits.Add(commandable);
                _actionsUI.EnableFor(_selectedUnits);
            }

            if (_selectedUnits.Count == 1 && evt.Unit is BaseBuilding building)
            {
                _buildingBuildingUI.EnableFor(building);
            }
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                _selectedUnits.Remove(commandable);

                if (_selectedUnits.Count > 0)
                {
                    _actionsUI.EnableFor(_selectedUnits);

                    if (_selectedUnits.Count == 1 && _selectedUnits.First() is BaseBuilding building)
                    {
                        _buildingBuildingUI.EnableFor(building);
                    }
                    else
                    {
                        _buildingBuildingUI.Disable();
                    }
                }
                else
                {
                    _actionsUI.Disable();
                    _buildingBuildingUI.Disable();
                }
            }
        }
    }
}