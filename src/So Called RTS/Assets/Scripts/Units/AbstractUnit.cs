using Scripts.EventBus;
using Scripts.Events;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : AbstractCommandable, IMovable
    {
        public float AgentRadius => _agent.radius;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        protected override void Start()
        {
            base.Start();
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position);
        }
    }
}