using Scripts.EventBus;
using Scripts.Units;

namespace Scripts.Events
{
    public struct UnitSelectedEvent : IEvent
    {
        public ISelectable Unit { get; private set; }

        public UnitSelectedEvent(ISelectable unit)
        {
            Unit = unit;
        }
    }
}
