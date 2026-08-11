using Scripts.UI.Components;
using Scripts.Units;
using System.Collections;
using UnityEngine;

namespace Scripts.UI.Containers
{
    public class BuildingBuildingUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private UIBuildQueueButton[] _unitButtons;
        [SerializeField] private ProgressBar _progressBar;

        private Coroutine _buildCoroutine;
        private BaseBuilding _building;

        public void EnableFor(BaseBuilding item)
        {
            _progressBar.SetProgress(0f);
            gameObject.SetActive(true);
            _building = item;
            _building.OnQueueUpdated += HandleQueueUpdated;
            SetupUnitButtons();

            _buildCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        private void SetupUnitButtons()
        {
            int i = 0;
            for (; i < _building.QueueSize; i++) // int i = 0 взято со строчки выше, можно и так
            {
                int index = i;
                _unitButtons[i].EnableFor(_building.Queue[i], () => _building.CancelBuildingUnit(index));
            }

            for (; i < _unitButtons.Length; i++)
            {
                _unitButtons[i].Disable();
            }
        }

        private void HandleQueueUpdated(UnitSO[] unitsInQueue)
        {
            if (unitsInQueue.Length == 1 && _buildCoroutine == null)
            {
                _buildCoroutine = StartCoroutine(UpdateUnitProgress());
            }

            SetupUnitButtons();
        }

        public void Disable()
        {
            if (_building != null)
            {
                _building.OnQueueUpdated -= HandleQueueUpdated;
            }
            gameObject.SetActive(false);
            _building = null;
            _buildCoroutine = null;
        }

        private IEnumerator UpdateUnitProgress()
        {
            while (_building != null && _building.QueueSize > 0)
            {
                float startTime = _building.CurrentQueueStartTime;
                float endTime = startTime + _building.BuildingUnit.BuildTime;

                float progress = Mathf.Clamp01((Time.time - startTime) / (endTime - startTime));

                _progressBar.SetProgress(progress);
                yield return null;
            }

            _buildCoroutine = null;
        }
    }
}