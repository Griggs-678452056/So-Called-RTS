using UnityEngine;

namespace Scripts.UI.Components
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _mask;
        private RectTransform _maskParentRectTransform;
        [SerializeField] private Vector2 _padding = new Vector2(9, 8);

        private void Awake()
        {
            if (_mask == null)
            {
                Debug.LogError($"Progress bar {name} is missing a mask.");
                return;
            }

            _maskParentRectTransform = _mask.parent.GetComponent<RectTransform>();
        }

        public void SetProgress(float progress)
        {
            Vector2 parentSize = _maskParentRectTransform.sizeDelta;
            Vector2 targetSize = parentSize - _padding * 2;

            targetSize.x *= Mathf.Clamp01(progress);

            _mask.offsetMin = _padding;
            _mask.offsetMax = new Vector2(_padding.x + targetSize.x - parentSize.x, -_padding.y);
        }
    }
}