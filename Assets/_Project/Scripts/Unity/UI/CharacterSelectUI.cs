using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlotRogue.Core.Events;
using SlotRogue.Core.Map;
using SlotRogue.Data;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Unity.UI
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private CharacterData[] _availableCharacters;
        [SerializeField] private Transform _characterCardContainer;
        [SerializeField] private GameObject _characterCardPrefab;

        private void Start()
        {
            if (_availableCharacters == null) return;

            foreach (var character in _availableCharacters)
            {
                if (character == null || _characterCardPrefab == null) continue;

                var card = Instantiate(_characterCardPrefab, _characterCardContainer);
                SetupCard(card, character);
            }
        }

        private void SetupCard(GameObject card, CharacterData character)
        {
            var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = $"{character.characterName}\nHP: {character.startingHP}\n{character.description}";

            var button = card.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => SelectCharacter(character));

            var portrait = card.transform.Find("Portrait")?.GetComponent<Image>();
            if (portrait != null && character.portrait != null)
                portrait.sprite = character.portrait;
        }

        private void SelectCharacter(CharacterData character)
        {
            var runState = new RunState();
            runState.InitializeFromCharacter(character);

            ServiceLocator.Register(runState);

            // Generate map
            var generator = new MapGenerator(runState.Seed);
            runState.MapNodes = generator.Generate();

            EventBus.Publish(new CharacterSelectedEvent { CharacterId = character.characterId });

            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
    }
}
