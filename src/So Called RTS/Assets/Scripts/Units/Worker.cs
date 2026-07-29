using UnityEngine;
using UnityEngine.AI;

namespace Scripts
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_target != null)
            {
                _agent.SetDestination(_target.position);
            }
        }
    }
}