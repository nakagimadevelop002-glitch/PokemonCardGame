using System.Collections.Generic;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// プレイヤー状態管理
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Player Info")]
        public string playerName;
        public int playerIndex; // 0 or 1
        public bool isAI; // AIプレイヤーフラグ

        [Header("Zones")]
        public List<CardData> deck = new List<CardData>();
        public List<CardData> hand = new List<CardData>();
        public List<CardData> discard = new List<CardData>();
        public List<CardData> lostZone = new List<CardData>();
        public List<CardData> prizes = new List<CardData>();

        [Header("Pokemon Zones")]
        public PokemonInstance activeSlot;
        public List<PokemonInstance> benchSlots = new List<PokemonInstance>(5);

        [Header("Turn State")]
        public bool energyAttachedThisTurn;
        public bool supporterUsedThisTurn;
        public bool stadiumUsedThisTurn;
        public bool attackedThisTurn;

        [Header("Stats")]
        public int mulligansGiven;

        public void Initialize(string name, int index, List<CardData> deckCards)
        {
            playerName = name;
            playerIndex = index;
            deck = new List<CardData>(deckCards);
            hand.Clear();
            discard.Clear();
            lostZone.Clear();
            prizes.Clear();
            benchSlots.Clear();

            ShuffleDeck();
            ResetTurnFlags();
        }

        public void ShuffleDeck()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
            // Debug.Log($"{playerName} shuffled deck");
        }

        public bool Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (deck.Count == 0)
                {
                    Debug.LogError($"{playerName} deck out! Loses the game.");

                    // 山札切れ判定：相手の勝利
                    var gm = GameManager.Instance;
                    if (gm != null)
                    {
                        int opponentIndex = 1 - playerIndex;
                        gm.SetWinner(opponentIndex, "相手の山札切れ");
                    }

                    return false;
                }
                hand.Add(deck[deck.Count - 1]);
                deck.RemoveAt(deck.Count - 1);
            }
            // Debug.Log($"{playerName} drew {count} cards");
            return true;
        }

        public void SetupPrizes(int prizeCount = 6)
        {
            for (int i = 0; i < prizeCount; i++)
            {
                if (deck.Count == 0) break;
                prizes.Add(deck[deck.Count - 1]);
                deck.RemoveAt(deck.Count - 1);
            }
            // Debug.Log($"{playerName} set {prizes.Count} prizes");
        }

        public bool HasBasicInHand()
        {
            foreach (var card in hand)
            {
                if (card is PokemonCardData pkm && pkm.stage == PokemonStage.Basic)
                    return true;
            }
            return false;
        }

        public void ResetTurnFlags()
        {
            energyAttachedThisTurn = false;
            supporterUsedThisTurn = false;
            stadiumUsedThisTurn = false;
            attackedThisTurn = false;
        }

        public void OnNewTurn()
        {
            ResetTurnFlags();
            if (activeSlot != null) activeSlot.OnNewTurn();
            foreach (var bench in benchSlots)
            {
                if (bench != null) bench.OnNewTurn();
            }
        }

        public List<PokemonInstance> GetAllPokemons()
        {
            var list = new List<PokemonInstance>();
            if (activeSlot != null) list.Add(activeSlot);
            list.AddRange(benchSlots);
            return list;
        }

        public PokemonInstance FindPokemonByInstanceID(string iid)
        {
            if (activeSlot != null && activeSlot.instanceID == iid) return activeSlot;
            foreach (var bench in benchSlots)
            {
                if (bench != null && bench.instanceID == iid) return bench;
            }
            return null;
        }
    }
}
