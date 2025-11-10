using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// にげる処理システム（バトル場とベンチの入れ替え）
    /// </summary>
    public class RetreatSystem : MonoBehaviour
    {
        public static RetreatSystem Instance { get; private set; }

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
        /// にげる処理（バトル場からベンチへ、ベンチからバトル場へ入れ替え）
        /// </summary>
        /// <param name="player">プレイヤー</param>
        /// <param name="benchIndex">入れ替え先のベンチインデックス</param>
        /// <returns>にげる成功したか</returns>
        public bool Retreat(PlayerController player, int benchIndex)
        {
            if (player.activeSlot == null)
            {
                Debug.Log("バトル場にポケモンがいません");
                return false;
            }

            if (benchIndex < 0 || benchIndex >= player.benchSlots.Count)
            {
                Debug.Log("無効なベンチ位置です");
                return false;
            }

            var active = player.activeSlot;

            // まひ・ねむりチェック
            if (active.statusCondition == StatusCondition.Paralysis)
            {
                Debug.Log($"{active.data.cardName}は まひ 状態のため、にげられません");
                return false;
            }
            if (active.statusCondition == StatusCondition.Sleep)
            {
                Debug.Log($"{active.data.cardName}は ねむり 状態のため、にげられません");
                return false;
            }

            // にげるコストを支払う
            if (!EnergySystem.Instance.PayRetreatCost(player, active))
            {
                return false;
            }

            // バトル場とベンチを入れ替え
            var newActive = player.benchSlots[benchIndex];
            player.activeSlot = newActive;
            player.benchSlots[benchIndex] = active;

            // にげることで特殊状態が回復
            active.ClearStatus();

            Debug.Log($"{player.playerName}: にげる → バトル場は《{newActive.data.cardName}》");
            return true;
        }

        /// <summary>
        /// にげる可能かチェック（UI表示用）
        /// </summary>
        public bool CanRetreat(PlayerController player)
        {
            if (player.activeSlot == null) return false;
            if (player.benchSlots.Count == 0) return false;

            var active = player.activeSlot;

            // まひ・ねむりチェック
            if (active.statusCondition == StatusCondition.Paralysis) return false;
            if (active.statusCondition == StatusCondition.Sleep) return false;

            // エネルギー数チェック
            int retreatCost = active.data.retreatCost;
            var gm = GameManager.Instance;
            if (gm.stadiumInPlay == "BeachCourt" && active.data.stage == PokemonStage.Basic)
            {
                retreatCost = Mathf.Max(0, retreatCost - 1);
            }

            if (active.attachedEnergies.Count < retreatCost) return false;

            return true;
        }

        /// <summary>
        /// にげる不可の理由を取得
        /// </summary>
        public string GetRetreatDisabledReason(PlayerController player)
        {
            if (player.activeSlot == null) return "バトル場にポケモンがいません";
            if (player.benchSlots.Count == 0) return "ベンチにポケモンがいません";

            var active = player.activeSlot;

            if (active.statusCondition == StatusCondition.Paralysis) return "まひ状態";
            if (active.statusCondition == StatusCondition.Sleep) return "ねむり状態";

            int retreatCost = active.data.retreatCost;
            var gm = GameManager.Instance;
            if (gm.stadiumInPlay == "BeachCourt" && active.data.stage == PokemonStage.Basic)
            {
                retreatCost = Mathf.Max(0, retreatCost - 1);
            }

            if (active.attachedEnergies.Count < retreatCost)
            {
                return $"エネルギー不足（{active.attachedEnergies.Count}/{retreatCost}）";
            }

            return ""; // にげる可能
        }
    }
}
