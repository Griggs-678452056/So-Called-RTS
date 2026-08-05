using Scripts.Units;
using UnityEngine;

namespace Scripts.Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "AI/Actions/Move", order = 100)]
    public class MoveCommand : ActionBase
    {
        [SerializeField] private float _radiusMultiplier = 3.5f;

        private int _unitsOnLayer = 0;
        private int _maxUnitsOnLayer = 1;
        private float _circleRadius = 0;
        private float _radiusOffset = 0;

        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = (AbstractUnit)context.Commandable;

            if (context.UnitIndex == 0)
            {
                _unitsOnLayer = 0;
                _circleRadius = 0;
                _maxUnitsOnLayer = 1;
                _radiusOffset = 0;
            }
            
            Vector3 targetPosition = new(
                context.Hit.point.x + _circleRadius * Mathf.Cos(_radiusOffset * _unitsOnLayer),
                context.Hit.point.y,
                context.Hit.point.z + _circleRadius * Mathf.Sin(_radiusOffset * _unitsOnLayer)
                );

            unit.MoveTo(targetPosition);
            _unitsOnLayer++;

            if (_unitsOnLayer >= _maxUnitsOnLayer)
            {
                _unitsOnLayer = 0;
                _circleRadius += unit.AgentRadius * _radiusMultiplier;
                _maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * _circleRadius / (unit.AgentRadius * 2));
                _radiusOffset = 2 * Mathf.PI / _maxUnitsOnLayer;
            }
        }
    }
}
