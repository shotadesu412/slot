using SlotRogue.Core.Events;
using SlotRogue.Core.Skill;
using SlotRogue.Core.Slot;
using SlotRogue.Data;

namespace SlotRogue.Core.Battle
{
    public class BattleController
    {
        public CombatUnit Player { get; private set; }
        public CombatUnit Enemy { get; private set; }
        public SlotMachine SlotMachine { get; private set; }
        public TurnManager TurnManager { get; private set; }
        public BattleState State { get; private set; }
        public EnemyIntent CurrentEnemyIntent { get; private set; }

        private EnemyIntentPattern _enemyPattern;
        private int _intentIndex;

        public BattleController()
        {
            SlotMachine = new SlotMachine();
            TurnManager = new TurnManager(2);
            State = BattleState.NotStarted;
        }

        public void StartBattle(CombatUnit player, CombatUnit enemy, EnemyIntentPattern pattern)
        {
            Player = player;
            Enemy = enemy;
            _enemyPattern = pattern;
            _intentIndex = 0;

            EventBus.Publish(new BattleStartedEvent
            {
                EnemyName = enemy.Name,
                EnemyHP = enemy.MaxHP,
                PlayerHP = player.MaxHP
            });

            StartPlayerTurn();
        }

        private void StartPlayerTurn()
        {
            if (!Player.IsAlive)
            {
                SetState(BattleState.Defeat);
                EventBus.Publish(new BattleLostEvent());
                return;
            }

            SetState(BattleState.PlayerTurnStart);
            Player.ResetBlock();
            Player.ProcessPoison();

            if (!Player.IsAlive)
            {
                SetState(BattleState.Defeat);
                EventBus.Publish(new BattleLostEvent());
                return;
            }

            Player.ProcessRegeneration();
            TurnManager.StartNewTurn();

            ChooseEnemyIntent();
            SetState(BattleState.WaitingForSpin);
        }

        private void ChooseEnemyIntent()
        {
            if (_enemyPattern == null || _enemyPattern.Intents.Length == 0)
            {
                CurrentEnemyIntent = new EnemyIntent(IntentType.Attack, 6);
            }
            else
            {
                var data = _enemyPattern.Intents[_intentIndex % _enemyPattern.Intents.Length];
                CurrentEnemyIntent = new EnemyIntent(data.Type, data.Value);
            }

            EventBus.Publish(new EnemyIntentRevealedEvent
            {
                IntentType = CurrentEnemyIntent.Type.ToString(),
                Value = CurrentEnemyIntent.Value
            });
        }

        public bool TryStartSpin()
        {
            if (State != BattleState.WaitingForSpin) return false;
            if (!TurnManager.ConsumeSpin()) return false;

            SlotMachine.StartSpin();
            SetState(BattleState.WaitingForReelStop);
            return true;
        }

        public void Update(float deltaTime)
        {
            if (State == BattleState.WaitingForReelStop)
                SlotMachine.Update(deltaTime);
        }

        public bool TryStopReel(int index)
        {
            if (State != BattleState.WaitingForReelStop) return false;
            if (!SlotMachine.IsReelSpinning(index)) return false;

            SlotMachine.StopReel(index);

            if (!SlotMachine.IsSpinning)
                ResolveSpinEffects();

            return true;
        }

        private void ResolveSpinEffects()
        {
            SetState(BattleState.ResolvingEffects);

            var result = SlotMachine.GetSpinResult();

            for (int i = 0; i < 3; i++)
            {
                var skill = result[i];
                if (skill == null) continue;

                var context = new BattleContext
                {
                    Player = Player,
                    Enemy = Enemy,
                    Battle = this,
                    EffectValue = DamageCalculator.CalculateAttackDamage(Player, skill.CurrentValue),
                    ReelIndex = i
                };

                if (skill.effectId == "deal_damage")
                    context.EffectValue = DamageCalculator.CalculateAttackDamage(Player, skill.CurrentValue);
                else
                    context.EffectValue = skill.CurrentValue;

                var effect = SkillEffectFactory.Create(skill.effectId);
                effect.Execute(context);
            }

            if (!Enemy.IsAlive)
            {
                SetState(BattleState.Victory);
                EventBus.Publish(new BattleWonEvent());
                return;
            }

            if (TurnManager.HasSpins)
                SetState(BattleState.WaitingForSpin);
            else
                ExecuteEnemyTurn();
        }

        private void ExecuteEnemyTurn()
        {
            SetState(BattleState.EnemyTurn);
            TurnManager.EndTurn();

            Enemy.ResetBlock();
            Enemy.ProcessPoison();

            if (!Enemy.IsAlive)
            {
                SetState(BattleState.Victory);
                EventBus.Publish(new BattleWonEvent());
                return;
            }

            switch (CurrentEnemyIntent.Type)
            {
                case IntentType.Attack:
                    int damage = DamageCalculator.CalculateAttackDamage(Enemy, CurrentEnemyIntent.Value);
                    int actual = Player.TakeDamage(damage);
                    EventBus.Publish(new DamageDealtEvent
                    {
                        IsPlayer = true,
                        Amount = actual,
                        RemainingHP = Player.CurrentHP
                    });
                    EventBus.Publish(new EnemyActedEvent { ActionDescription = $"{Enemy.Name}の攻撃！{actual}ダメージ！" });
                    break;

                case IntentType.Defend:
                    Enemy.GainBlock(CurrentEnemyIntent.Value);
                    EventBus.Publish(new BlockGainedEvent
                    {
                        IsPlayer = false,
                        Amount = CurrentEnemyIntent.Value,
                        TotalBlock = Enemy.Block
                    });
                    EventBus.Publish(new EnemyActedEvent { ActionDescription = $"{Enemy.Name}はブロック{CurrentEnemyIntent.Value}を得た" });
                    break;

                case IntentType.Buff:
                    Enemy.ApplyStatus(StatusType.Strength, CurrentEnemyIntent.Value);
                    EventBus.Publish(new StatusAppliedEvent
                    {
                        IsPlayer = false,
                        StatusName = "Strength",
                        Stacks = CurrentEnemyIntent.Value
                    });
                    EventBus.Publish(new EnemyActedEvent { ActionDescription = $"{Enemy.Name}は筋力を{CurrentEnemyIntent.Value}上げた" });
                    break;

                case IntentType.Debuff:
                    Player.ApplyStatus(StatusType.Weakness, CurrentEnemyIntent.Value);
                    EventBus.Publish(new StatusAppliedEvent
                    {
                        IsPlayer = true,
                        StatusName = "Weakness",
                        Stacks = CurrentEnemyIntent.Value
                    });
                    EventBus.Publish(new EnemyActedEvent { ActionDescription = $"{Enemy.Name}は脱力を付与した" });
                    break;
            }

            _intentIndex++;
            Enemy.TickStatuses();

            if (!Player.IsAlive)
            {
                SetState(BattleState.Defeat);
                EventBus.Publish(new BattleLostEvent());
                return;
            }

            StartPlayerTurn();
        }

        public void EndTurnManually()
        {
            if (State != BattleState.WaitingForSpin) return;
            ExecuteEnemyTurn();
        }

        private void SetState(BattleState newState)
        {
            State = newState;
        }
    }

    [System.Serializable]
    public class EnemyIntentData
    {
        public IntentType Type;
        public int Value;
    }

    public class EnemyIntentPattern
    {
        public EnemyIntentData[] Intents { get; }

        public EnemyIntentPattern(EnemyIntentData[] intents)
        {
            Intents = intents;
        }
    }
}
