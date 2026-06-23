using UnityEngine;
using SlotRogue.Core.Battle;

namespace SlotRogue.Data
{
    [CreateAssetMenu(menuName = "SlotRogue/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public string enemyId;
        public string enemyName;
        public int maxHP;
        public Sprite sprite;
        public EnemyIntentData[] intentPattern;

        public EnemyIntentPattern ToPattern()
        {
            return new EnemyIntentPattern(intentPattern);
        }
    }
}
