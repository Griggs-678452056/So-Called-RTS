using Scripts.Commands;
using Scripts.EventBus;
using Scripts.Events;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Scripts.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public ActionBase[] AvailableCommands { get; private set; }
        [SerializeField] private DecalProjector _decalProjector;
        [SerializeField] private UnitSO UnitSO;

        protected virtual void Start()
        {
            CurrentHealth = UnitSO.Health;
            MaxHealth = UnitSO.Health;
        }

        public void Deselect()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(false);
            }

            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void Select()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(true);
            }

            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}