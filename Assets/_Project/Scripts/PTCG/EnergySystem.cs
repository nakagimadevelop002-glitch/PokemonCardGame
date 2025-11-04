using System.Linq;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// エネルギーシステム（手貼り、特性によるエネルギー加速など）
    /// </summary>
    public class EnergySystem : MonoBehaviour
    {
        public static EnergySystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 手札からエネルギーを手貼り（1ターン1回制限）
        /// </summary>
        public bool AttachEnergyFromHand(PlayerController player, PokemonInstance target)
        {
            if (player.energyAttachedThisTurn)
            {
                Debug.Log("このターンは手貼り済み");
                return false;
            }

            if (target == null)
            {
                Debug.Log("対象のポケモンを選択してください");
                return false;
            }

            // 手札からエネルギーカードを探す
            var energyCard = player.hand.OfType<EnergyCardData>().FirstOrDefault();
            if (energyCard == null)
            {
                Debug.Log("手札に付けられるエネルギーがありません");
                return false;
            }

            // 手札から削除してポケモンに付ける
            player.hand.Remove(energyCard);
            target.AttachEnergy(energyCard);
            player.energyAttachedThisTurn = true;

            Debug.Log($"{player.playerName}: 《{energyCard.cardName}》を《{target.data.cardName}》につけた（手貼り）");
            return true;
        }

        /// <summary>
        /// サイコエンブレイス特性（トラッシュから超エネ加速＋ダメカン2）
        /// </summary>
        public bool CanUsePsychicEmbrace(PlayerController player, PokemonInstance target)
        {
            if (target == null) return false;
            if (target.data.type != PokemonType.P) return false;

            // トラッシュに基本超エネルギーがあるか
            var hasPsychicEnergy = player.discard.OfType<EnergyCardData>()
                .Any(e => e.cardID == "BasicPsychic" && e.isBasic);
            if (!hasPsychicEnergy) return false;

            // ダメカン20を載せても気絶しないか
            if (target.currentDamage + 20 >= target.MaxHP) return false;

            return true;
        }

        public bool UsePsychicEmbrace(PlayerController player, PokemonInstance target)
        {
            if (!CanUsePsychicEmbrace(player, target))
            {
                Debug.Log("サイコエンブレイスの条件未達");
                return false;
            }

            // トラッシュから基本超エネルギーを探す（最後の1枚）
            var psychicEnergy = player.discard.OfType<EnergyCardData>()
                .LastOrDefault(e => e.cardID == "BasicPsychic" && e.isBasic);

            if (psychicEnergy == null) return false;

            // トラッシュから削除してポケモンに付ける
            player.discard.Remove(psychicEnergy);
            target.AttachEnergy(psychicEnergy);
            target.TakeDamage(20);

            Debug.Log($"{player.playerName}: サイコエンブレイス → 《{target.data.cardName}》に超エネ+1＆ダメカン20");
            return true;
        }

        /// <summary>
        /// エネルギー数をカウント（特定タイプ）
        /// </summary>
        public int CountEnergy(PokemonInstance pokemon, PokemonType type)
        {
            if (pokemon == null) return 0;

            int count = 0;
            foreach (var energy in pokemon.attachedEnergies)
            {
                if (energy.isBasic && energy.providesType == type)
                {
                    count += energy.providesAmount;
                }
                else if (energy.isSpecial)
                {
                    // リバーサルエネルギー等の特殊エネルギー処理
                    if (energy.cardID == "ReversalEnergy")
                    {
                        // リバーサルエネルギーは自分のサイドが相手より多い時のみ2個分
                        var gm = GameManager.Instance;
                        var owner = pokemon.ownerIndex == 0 ? gm.player1 : gm.player2;
                        var opponent = pokemon.ownerIndex == 0 ? gm.player2 : gm.player1;

                        if (owner.prizes.Count > opponent.prizes.Count)
                        {
                            count += 2;
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// エネルギー数をカウント（総数）
        /// </summary>
        public int CountTotalEnergy(PokemonInstance pokemon)
        {
            if (pokemon == null) return 0;
            return pokemon.attachedEnergies.Count;
        }

        /// <summary>
        /// にげるコストを計算（エネルギーをトラッシュ）
        /// </summary>
        public bool PayRetreatCost(PlayerController player, PokemonInstance pokemon)
        {
            if (pokemon == null) return false;

            int retreatCost = pokemon.data.retreatCost;

            // ビーチコートスタジアム効果: たねポケモンのにげるコスト-1
            var gm = GameManager.Instance;
            if (gm.stadiumInPlay == "BeachCourt" && pokemon.data.stage == PokemonStage.Basic)
            {
                retreatCost = Mathf.Max(0, retreatCost - 1);
            }

            if (pokemon.attachedEnergies.Count < retreatCost)
            {
                Debug.Log($"エネルギーが足りません（必要: {retreatCost}、現在: {pokemon.attachedEnergies.Count}）");
                return false;
            }

            // エネルギーをトラッシュへ
            for (int i = 0; i < retreatCost; i++)
            {
                if (pokemon.attachedEnergies.Count > 0)
                {
                    var energy = pokemon.attachedEnergies[0];
                    pokemon.attachedEnergies.RemoveAt(0);
                    player.discard.Add(energy);
                }
            }

            Debug.Log($"{pokemon.data.cardName}がにげるコスト{retreatCost}を支払いました");
            return true;
        }
    }
}
