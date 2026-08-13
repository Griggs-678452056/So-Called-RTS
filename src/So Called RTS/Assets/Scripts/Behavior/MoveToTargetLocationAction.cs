using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

namespace Scripts.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Move to Target Location", story: "[Agent] moves to [TargetLocation]", category: "Action/Navigation", id: "c5a64fefda457ab8d98a533054017e0a")]
    public partial class MoveToTargetLocationAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;

        private NavMeshAgent _agent;

        protected override Status OnStart()
        {
            if (!Agent.Value.TryGetComponent(out _agent))
            {
                return Status.Failure;
            }

            if (Vector3.Distance(_agent.transform.position, TargetLocation.Value) <= _agent.stoppingDistance)
            {
                return Status.Success;
            }

            _agent.SetDestination(TargetLocation.Value);

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                return Status.Success;
            }

            return Status.Running;
        }
    }
}
