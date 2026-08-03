using Scripts.EventBus;
using Scripts.Units;

namespace Scripts.Events
{
    public struct UnitDeselectedEvent : IEvent
    {
        public ISelectable Unit { get; private set; }

        public UnitDeselectedEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}