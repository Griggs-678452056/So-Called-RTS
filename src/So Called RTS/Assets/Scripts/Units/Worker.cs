using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Scripts
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Worker : MonoBehaviour, ISelectable, IMovable
    {
        [SerializeField] private DecalProjector _decalProjector;
        private NavMeshAgent _agent;

        public void Deselect()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(false);
            }
        }

        public void MoveTo(Vector3 position)
        {
            _agent.SetDestination(position);
        }

        public void Select()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(true);
            }
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }
    }
}