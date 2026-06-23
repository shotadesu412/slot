using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Data;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Unity.UI
{
    public class RewardScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _rewardPanel;
        [SerializeField] private Transform _skillCardContainer;
        [SerializeField] private GameObject _skillCardPrefab;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TextMeshProUGUI _goldRewardText;

        [Header("Reel Assignment")]
        [SerializeField] private GameObject _reelAssignPanel;
        [SerializeField] private Button[] _reelAssignButtons = new Button[3];

        private SkillData _selectedSkill;
        private RunState _runState;

        public void ShowRewards(List<SkillData> skills, int goldReward)
        {
            _runState = ServiceLocator.Get<RunState>();

            if (_rewardPanel != null)
                _rewardPanel.SetActive(true);
            if (_reelAssignPanel != null)
                _reelAssignPanel.SetActive(false);

            if (_goldRewardText != null)
                _goldRewardText.text = $"+{goldReward}G";

            if (_runState != null)
                _runState.Gold += goldReward;

            // Clear existing cards
            if (_skillCardContainer != null)
                foreach (Transform child in _skillCardContainer)
                    Destroy(child.gameObject);

            // Create skill cards
            foreach (var skill in skills)
            {
                if (skill == null || _skillCardPrefab == null) continue;

                var card = Instantiate(_skillCardPrefab, _skillCardContainer);
                SetupSkillCard(card, skill);
            }

            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipPressed);
        }

        private void SetupSkillCard(GameObject card, SkillData skill)
        {
            var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = $"{skill.skillName}\n{skill.description}\n[{skill.rarity}]";

            var icon = card.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && skill.icon != null)
                icon.sprite = skill.icon;

            var button = card.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnSkillSelected(skill));
        }

        private void OnSkillSelected(SkillData skill)
        {
            _selectedSkill = skill;

            // Show reel assignment panel
            if (_reelAssignPanel != null)
                _reelAssignPanel.SetActive(true);

            for (int i = 0; i < 3; i++)
            {
                int reelIdx = i;
                if (_reelAssignButtons[i] != null)
                {
                    _reelAssignButtons[i].onClick.RemoveAllListeners();
                    _reelAssignButtons[i].onClick.AddListener(() => AssignToReel(reelIdx));

                    var text = _reelAssignButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null && _runState != null)
                        text.text = $"Reel {reelIdx + 1}\n({_runState.ReelContents[reelIdx].Count} skills)";
                }
            }
        }

        private void AssignToReel(int reelIndex)
        {
            if (_selectedSkill == null || _runState == null) return;

            _runState.AddSkillToReel(reelIndex, _selectedSkill);

            EventBus.Publish(new SkillRewardChosenEvent
            {
                SkillId = _selectedSkill.skillId,
                ReelIndex = reelIndex
            });

            CloseAndReturn();
        }

        private void OnSkipPressed()
        {
            EventBus.Publish(new SkipRewardEvent());
            CloseAndReturn();
        }

        private void CloseAndReturn()
        {
            if (_rewardPanel != null)
                _rewardPanel.SetActive(false);

            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
    }
}
