using System.Collections.Generic;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// 場に出ているポケモンの実体（Runtime Instance）
    /// </summary>
    public class PokemonInstance : MonoBehaviour
    {
        [Header("Card Reference")]
        public PokemonCardData data;
        public string instanceID; // uid

        [Header("Owner")]
        public int ownerIndex; // 0 or 1

        [Header("Status")]
        public int currentDamage;
        public int turnsInPlay;
        public bool wasPlayedThisTurn;

        [Header("Attached")]
        public List<EnergyCardData> attachedEnergies = new List<EnergyCardData>();
        public TrainerCardData attachedTool;

        [Header("Condition")]
        public StatusCondition statusCondition = StatusCondition.None;
        public int paralysisOwner = -1; // who caused paralysis
        public int paralysisTurns;

        [Header("Ability Flags")]
        public Dictionary<string, bool> abilityUsedFlags = new Dictionary<string, bool>();

        public int MaxHP => data.baseHP + (attachedTool != null && attachedTool.effectID == "BraveryCharm" && data.stage == PokemonStage.Basic ? 50 : 0);
        public int RemainingHP => MaxHP - currentDamage;
        public bool IsKnockedOut => currentDamage >= MaxHP;

        private void Awake()
        {
            instanceID = System.Guid.NewGuid().ToString();
        }

        public void Initialize(PokemonCardData cardData, int owner)
        {
            data = cardData;
            ownerIndex = owner;
            currentDamage = 0;
            turnsInPlay = 0;
            wasPlayedThisTurn = true;
            attachedEnergies.Clear();
            attachedTool = null;
            statusCondition = StatusCondition.None;
            abilityUsedFlags.Clear();
        }

        public void OnNewTurn()
        {
            turnsInPlay++;
            wasPlayedThisTurn = false;
            abilityUsedFlags.Clear(); // Reset once-per-turn abilities
        }

        public void TakeDamage(int damage)
        {
            currentDamage += damage;
            Debug.Log($"{data.cardName} takes {damage} damage. Total: {currentDamage}/{MaxHP}");
        }

        public void Heal(int amount)
        {
            currentDamage = Mathf.Max(0, currentDamage - amount);
        }

        public void AttachEnergy(EnergyCardData energy)
        {
            attachedEnergies.Add(energy);
            Debug.Log($"Attached {energy.cardName} to {data.cardName}");
        }

        public void ClearStatus()
        {
            statusCondition = StatusCondition.None;
            paralysisOwner = -1;
            paralysisTurns = 0;
        }
    }
}
