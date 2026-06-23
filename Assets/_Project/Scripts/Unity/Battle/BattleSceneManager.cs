using UnityEngine;
using TMPro;
using SlotRogue.Core.Battle;
using SlotRogue.Core.Events;
using SlotRogue.Core.Slot;
using SlotRogue.Data;
using SlotRogue.Unity.Bootstrap;

namespace SlotRogue.Unity.Battle
{
    public class BattleSceneManager : MonoBehaviour
    {
        [Header("Displays")]
        [SerializeField] private SlotMachineDisplay _slotMachineDisplay;
        [SerializeField] private PlayerHUD _playerHUD;
        [SerializeField] private EnemyDisplay _enemyDisplay;

        [Header("Battle Log")]
        [SerializeField] private TextMeshProUGUI _battleLog;
        [SerializeField] private int _maxLogLines = 8;

        [Header("Result")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultText;

        [Header("Test Data - Remove after ScriptableObject setup")]
        [SerializeField] private EnemyData _testEnemy;
        [SerializeField] private SkillData[] _testReel1Skills;
        [SerializeField] private SkillData[] _testReel2Skills;
        [SerializeField] private SkillData[] _testReel3Skills;
        [SerializeField] private int _playerMaxHP = 80;

        private BattleController _battleController;
        private string _logContent = "";

        private void Start()
        {
            SetupBattle();
        }

        private void SetupBattle()
        {
            _battleController = new BattleController();
            ServiceLocator.Register(_battleController);

            var player = new CombatUnit("Player", _playerMaxHP);

            // Setup reels with test data
            SetupReels(_battleController.SlotMachine);

            // Setup displays
            if (_slotMachineDisplay != null)
                _slotMachineDisplay.Initialize(_battleController);
            if (_playerHUD != null)
                _playerHUD.Initialize(_playerMaxHP);

            if (_resultPanel != null)
                _resultPanel.SetActive(false);

            // Subscribe to battle events for logging
            EventBus.Subscribe<EffectResolvedEvent>(OnEffectResolved);
            EventBus.Subscribe<EnemyActedEvent>(OnEnemyActed);
            EventBus.Subscribe<BattleWonEvent>(OnBattleWon);
            EventBus.Subscribe<BattleLostEvent>(OnBattleLost);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);

            // Setup enemy
            CombatUnit enemy;
            EnemyIntentPattern pattern;

            if (_testEnemy != null)
            {
                enemy = new CombatUnit(_testEnemy.enemyName, _testEnemy.maxHP);
                pattern = _testEnemy.ToPattern();
                if (_enemyDisplay != null)
                    _enemyDisplay.Initialize(_testEnemy);
            }
            else
            {
                enemy = new CombatUnit("Slime", 40);
                pattern = CreateDefaultPattern();
            }

            _battleController.StartBattle(player, enemy, pattern);
            AddLog("Battle Start!");
        }

        private void SetupReels(SlotMachine machine)
        {
            if (_testReel1Skills != null)
                foreach (var skill in _testReel1Skills)
                    if (skill != null) machine.Reels[0].AddSymbol(skill);

            if (_testReel2Skills != null)
                foreach (var skill in _testReel2Skills)
                    if (skill != null) machine.Reels[1].AddSymbol(skill);

            if (_testReel3Skills != null)
                foreach (var skill in _testReel3Skills)
                    if (skill != null) machine.Reels[2].AddSymbol(skill);

            // Fallback: add default skills if reels are empty
            if (machine.Reels[0].Symbols.Count == 0 ||
                machine.Reels[1].Symbols.Count == 0 ||
                machine.Reels[2].Symbols.Count == 0)
            {
                CreateDefaultSkills(machine);
            }
        }

        private void CreateDefaultSkills(SlotMachine machine)
        {
            // Create runtime SkillData for testing without ScriptableObject assets
            var attack = ScriptableObject.CreateInstance<SkillData>();
            attack.skillId = "attack";
            attack.skillName = "Strike";
            attack.effectId = "deal_damage";
            attack.effectValue = 6;
            attack.description = "6ダメージを与える";

            var heavyAttack = ScriptableObject.CreateInstance<SkillData>();
            heavyAttack.skillId = "heavy_attack";
            heavyAttack.skillName = "Heavy Strike";
            heavyAttack.effectId = "deal_damage";
            heavyAttack.effectValue = 10;
            heavyAttack.description = "10ダメージを与える";

            var block = ScriptableObject.CreateInstance<SkillData>();
            block.skillId = "block";
            block.skillName = "Guard";
            block.effectId = "gain_block";
            block.effectValue = 5;
            block.description = "ブロック5を得る";

            var heavyBlock = ScriptableObject.CreateInstance<SkillData>();
            heavyBlock.skillId = "heavy_block";
            heavyBlock.skillName = "Iron Wall";
            heavyBlock.effectId = "gain_block";
            heavyBlock.effectValue = 8;
            heavyBlock.description = "ブロック8を得る";

            // Reel 1: Attack focused
            if (machine.Reels[0].Symbols.Count == 0)
            {
                machine.Reels[0].AddSymbol(attack);
                machine.Reels[0].AddSymbol(attack);
                machine.Reels[0].AddSymbol(heavyAttack);
                machine.Reels[0].AddSymbol(block);
            }

            // Reel 2: Balanced
            if (machine.Reels[1].Symbols.Count == 0)
            {
                machine.Reels[1].AddSymbol(attack);
                machine.Reels[1].AddSymbol(block);
                machine.Reels[1].AddSymbol(attack);
                machine.Reels[1].AddSymbol(block);
            }

            // Reel 3: Defense focused
            if (machine.Reels[2].Symbols.Count == 0)
            {
                machine.Reels[2].AddSymbol(block);
                machine.Reels[2].AddSymbol(block);
                machine.Reels[2].AddSymbol(heavyBlock);
                machine.Reels[2].AddSymbol(attack);
            }
        }

        private EnemyIntentPattern CreateDefaultPattern()
        {
            return new EnemyIntentPattern(new[]
            {
                new EnemyIntentData { Type = IntentType.Attack, Value = 8 },
                new EnemyIntentData { Type = IntentType.Defend, Value = 5 },
                new EnemyIntentData { Type = IntentType.Attack, Value = 12 },
                new EnemyIntentData { Type = IntentType.Buff, Value = 2 },
            });
        }

        private void Update()
        {
            _battleController?.Update(Time.deltaTime);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            AddLog($"--- Turn {evt.TurnNumber} (Spins: {evt.SpinsAvailable}) ---");
        }

        private void OnEffectResolved(EffectResolvedEvent evt)
        {
            AddLog($"  {evt.SkillName}: {evt.Description}");
        }

        private void OnEnemyActed(EnemyActedEvent evt)
        {
            AddLog(evt.ActionDescription);
        }

        private void OnBattleWon(BattleWonEvent evt)
        {
            AddLog("Victory!");
            ShowResult("VICTORY!");
        }

        private void OnBattleLost(BattleLostEvent evt)
        {
            AddLog("Defeat...");
            ShowResult("DEFEAT...");
        }

        private void ShowResult(string text)
        {
            if (_resultPanel != null)
            {
                _resultPanel.SetActive(true);
                if (_resultText != null)
                    _resultText.text = text;
            }

            if (_slotMachineDisplay != null)
                _slotMachineDisplay.SetInteractable(false);
        }

        private void AddLog(string message)
        {
            _logContent += message + "\n";
            var lines = _logContent.Split('\n');
            if (lines.Length > _maxLogLines)
            {
                int skip = lines.Length - _maxLogLines;
                _logContent = string.Join("\n", lines, skip, _maxLogLines);
            }

            if (_battleLog != null)
                _battleLog.text = _logContent;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EffectResolvedEvent>(OnEffectResolved);
            EventBus.Unsubscribe<EnemyActedEvent>(OnEnemyActed);
            EventBus.Unsubscribe<BattleWonEvent>(OnBattleWon);
            EventBus.Unsubscribe<BattleLostEvent>(OnBattleLost);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            ServiceLocator.Clear();
        }
    }
}
