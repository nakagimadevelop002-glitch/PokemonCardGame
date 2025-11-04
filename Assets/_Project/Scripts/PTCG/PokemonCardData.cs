using System.Collections.Generic;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// ポケモンカードのデータ定義
    /// </summary>
    [CreateAssetMenu(fileName = "PokemonCard", menuName = "PTCG/Pokemon Card")]
    public class PokemonCardData : CardData
    {
        [Header("Pokemon Info")]
        public PokemonStage stage;
        public string evolvesFrom; // Stage1/Stage2の場合
        public PokemonType type;
        public int baseHP;
        public int retreatCost;
        public bool isEX;

        [Header("Weakness & Resistance")]
        public PokemonType weakness = PokemonType.C; // C = None
        public int weaknessMultiplier = 2;
        public PokemonType resistance = PokemonType.C; // C = None
        public int resistanceValue = 30;

        [Header("Attacks")]
        public List<AttackData> attacks = new List<AttackData>();

        [Header("Abilities")]
        public List<AbilityData> abilities = new List<AbilityData>();

        public override string GetDisplayName()
        {
            return isEX ? $"{cardName}ex" : cardName;
        }
    }

    [System.Serializable]
    public class AttackData
    {
        public string attackName;
        public int energyCost; // total colorless cost
        public List<EnergyRequirement> specificRequirements; // specific type requirements
        public int baseDamage;
        public string effectID; // for code lookup
        public string effectDescription;
        public bool clearsStatus; // ミラクルフォースの状態異常回復
    }

    [System.Serializable]
    public class EnergyRequirement
    {
        public PokemonType type;
        public int count;
    }

    [System.Serializable]
    public class AbilityData
    {
        public string abilityID;
        public string abilityName;
        public bool oncePerTurn;
        public string description;
        public string effectCode; // delegate or JSON
    }
}
