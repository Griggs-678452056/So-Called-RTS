using Scripts.EventBus;
using Scripts.Events;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Scripts.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class AbstractUnit : MonoBehaviour, ISelectable, IMovable
    {
        [SerializeField] private DecalProjector _decalProjector;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        public void Deselect()
        {
            if (_decalProjector != null)
            {
                _decalProjector.gameObject.SetActive(false);
            }

            Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
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

            Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}