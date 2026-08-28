using Scripts.Units;
using UnityEngine;

namespace Scripts.Commands
{
    [CreateAssetMenu(fileName = "Stop Action", menuName = "AI/Commands/Stop", order = 101)]
    public class StopCommand : ActionBase
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = (AbstractUnit)context.Commandable;
            unit.Stop();
        }
    }
}