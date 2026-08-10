using Scripts.EventBus;
using Scripts.Events;
using Scripts.UI.Containers;
using Scripts.Units;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private ActionsUI _actionsUI;
        private HashSet<AbstractCommandable> _selectedUnits = new(12);

        private void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
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
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
            {
                _selectedUnits.Remove(commandable);

                if (_selectedUnits.Count > 0)
                {
                    _actionsUI.EnableFor(_selectedUnits);
                }
                else
                {
                    _actionsUI.Disable();
                }
            }
        }
    }
}