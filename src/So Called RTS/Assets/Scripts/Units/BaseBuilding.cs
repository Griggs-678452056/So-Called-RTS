using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        private Queue<UnitSO> _buildingQueue = new(MAX_QUEUE_SIZE);

        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if (_buildingQueue.Count == MAX_QUEUE_SIZE)
            {
                Debug.LogError("Создание юнита вызвано, когда очередь заполнена!");
                return;
            }

            _buildingQueue.Enqueue(unit);
            if (_buildingQueue.Count == 1)
            {
                StartCoroutine(DoBuildUnits());
            }
        }

        private IEnumerator DoBuildUnits()
        {
            while (_buildingQueue.Count > 0)
            {
                UnitSO unit = _buildingQueue.Peek();
                yield return new WaitForSeconds(unit.BuildTime);
                Instantiate(unit.Prefab, transform.position, Quaternion.identity);
                _buildingQueue.Dequeue();
            }
        }
    }
}