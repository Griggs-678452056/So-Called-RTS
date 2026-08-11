using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public int QueueSize => _buildingQueue.Count;
        public UnitSO[] Queue => _buildingQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO BuildingUnit { get; private set; }

        private const int MAX_QUEUE_SIZE = 5;
        private List<UnitSO> _buildingQueue = new(MAX_QUEUE_SIZE);

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;



        public void BuildUnit(UnitSO unit)
        {
            if (_buildingQueue.Count == MAX_QUEUE_SIZE)
            {
                Debug.LogError("Создание юнита вызвано, когда очередь заполнена!");
                return;
            }

            _buildingQueue.Add(unit);
            if (_buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(_buildingQueue.ToArray());
            }
        }

        public void CancelBuildingUnit(int index)
        {
            if (index < 0 || index >= _buildingQueue.Count)
            {
                Debug.LogError("Попытка отменить создание юнита за пределами очереди!");
                return;
            }

            _buildingQueue.RemoveAt(index);
            if (index == 0)
            {
                StopAllCoroutines();

                if (_buildingQueue.Count > 0)
                {
                    StartCoroutine(DoBuildUnits());
                }
                else
                {
                    OnQueueUpdated?.Invoke(_buildingQueue.ToArray());
                }
            }
            else
            {
                OnQueueUpdated?.Invoke(_buildingQueue.ToArray());
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (_buildingQueue.Count > 0)
            {
                BuildingUnit = _buildingQueue[0]; // возврат объекта в начало без удаления, как при методе Peek класса Queue
                CurrentQueueStartTime = Time.time;
                OnQueueUpdated?.Invoke(_buildingQueue.ToArray());

                yield return new WaitForSeconds(BuildingUnit.BuildTime);

                Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                _buildingQueue.RemoveAt(0);
            }

            OnQueueUpdated?.Invoke(_buildingQueue.ToArray());
        }
    }
}