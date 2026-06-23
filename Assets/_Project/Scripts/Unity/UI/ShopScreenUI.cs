using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Core.Reward;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Unity.UI
{
    public class ShopScreenUI : MonoBehaviour
    {
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private GameObject _shopItemPrefab;
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private Button _leaveButton;

        [Header("Reel Assignment")]
        [SerializeField] private GameObject _reelAssignPanel;
        [SerializeField] private Button[] _reelAssignButtons = new Button[3];

        private ShopInventory _shop;
        private RunState _runState;
        private ShopItem _pendingPurchase;

        private void Start()
        {
            _runState = ServiceLocator.Get<RunState>();
            if (_runState == null) return;

            _shop = new ShopInventory(_runState.GetRNG());
            // TODO: Pass actual skill/relic pools from game data
            // _shop.GenerateShop(skillPool, relicPool, _runState.CurrentFloor);

            UpdateGoldDisplay();

            if (_leaveButton != null)
                _leaveButton.onClick.AddListener(LeaveShop);

            if (_reelAssignPanel != null)
                _reelAssignPanel.SetActive(false);

            BuildShopUI();
        }

        private void BuildShopUI()
        {
            if (_itemContainer == null || _shopItemPrefab == null) return;

            foreach (var item in _shop.Items)
            {
                var obj = Instantiate(_shopItemPrefab, _itemContainer);
                SetupShopItemUI(obj, item);
            }
        }

        private void SetupShopItemUI(GameObject obj, ShopItem item)
        {
            var text = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = $"{item.Name}\n{item.Cost}G";

            var button = obj.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => TryPurchase(item, button));
        }

        private void TryPurchase(ShopItem item, Button button)
        {
            if (item.IsSold || _runState.Gold < item.Cost) return;

            _runState.Gold -= item.Cost;
            item.IsSold = true;
            button.interactable = false;

            if (item.IsSkill)
            {
                _pendingPurchase = item;
                ShowReelAssignment();
            }
            else if (item.IsRelic)
            {
                _runState.Relics.Add(item.Relic);
            }

            UpdateGoldDisplay();

            EventBus.Publish(new ShopPurchaseEvent
            {
                ItemId = item.IsSkill ? item.Skill.skillId : item.Relic.relicId,
                Cost = item.Cost
            });
        }

        private void ShowReelAssignment()
        {
            if (_reelAssignPanel != null)
                _reelAssignPanel.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
                int reelIdx = i;
                if (_reelAssignButtons[i] != null)
                {
                    _reelAssignButtons[i].onClick.RemoveAllListeners();
                    _reelAssignButtons[i].onClick.AddListener(() => AssignToReel(reelIdx));
                }
            }
        }

        private void AssignToReel(int reelIndex)
        {
            if (_pendingPurchase?.Skill != null)
            {
                _runState.AddSkillToReel(reelIndex, _pendingPurchase.Skill);
                _pendingPurchase = null;
            }

            if (_reelAssignPanel != null)
                _reelAssignPanel.SetActive(false);
        }

        private void UpdateGoldDisplay()
        {
            if (_goldText != null && _runState != null)
                _goldText.text = $"Gold: {_runState.Gold}";
        }

        private void LeaveShop()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
    }
}
