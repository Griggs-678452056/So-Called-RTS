using Scripts.EventBus;
using Scripts.Events;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMovable
    {
        public float AgentRadius => _agent.radius;
        private NavMeshAgent _agent;
        private BehaviorGraphAgent _graphAgent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _graphAgent = GetComponent<BehaviorGraphAgent>();
            MoveTo(transform.position);
        }

        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
            MoveTo(transform.position);
        }

        public void MoveTo(Vector3 position)
        {
            _graphAgent.SetVariableValue("TargetLocation", position);
        }
    }
}