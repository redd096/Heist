using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Main/Character")]
    public class Character : Redd096Main
    {
        public enum ECharacterType { Player, AI };

        public ECharacterType CharacterType = ECharacterType.AI;
        [ShowIf("CharacterType", ECharacterType.AI)] public string EnemyType = "Base Enemy";
    }
}