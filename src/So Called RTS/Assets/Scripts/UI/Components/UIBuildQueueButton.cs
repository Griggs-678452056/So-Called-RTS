using Scripts.Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Scripts.UI.Components
{
    public class UIBuildQueueButton : MonoBehaviour, IUIElement<UnitSO, UnityAction>
    {
        [SerializeField] private Image _icon;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            Disable();
        }

        public void EnableFor(UnitSO item, UnityAction callback)
        {
            _button.onClick.RemoveAllListeners();
            _button.interactable = true;
            _button.onClick.AddListener(callback);
            _icon.gameObject.SetActive(true);
            _icon.sprite = item.Icon;
        }

        public void Disable()
        {
            _button.interactable = false;
            _button.onClick.RemoveAllListeners();
            _icon.gameObject.SetActive(false);
        }
    }
}