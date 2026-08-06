using Scripts.Commands;
using Scripts.EventBus;

namespace Scripts.Events
{
    public struct ActionSelectedEvent : IEvent
    {
        public ActionBase Action {  get; }

        public ActionSelectedEvent(ActionBase action)
        {
            Action = action;
        }
    }
}