using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// ポケモンの進化システム
    /// </summary>
    public class EvolutionSystem : MonoBehaviour
    {
        public static EvolutionSystem Instance { get; private set; }

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
        /// ポケモンを進化させる
        /// </summary>
        /// <param name="player">プレイヤー</param>
        /// <param name="target">進化させるポケモン</param>
        /// <param name="evolutionCard">進化先のカードデータ</param>
        /// <param name="viaRareCandy">ふしぎなアメ経由か</param>
        /// <returns>進化成功したか</returns>
        public bool Evolve(PlayerController player, PokemonInstance target, PokemonCardData evolutionCard, bool viaRareCandy = false)
        {
            // 進化先が1進化or2進化でない場合は失敗
            if (evolutionCard.stage != PokemonStage.Stage1 && evolutionCard.stage != PokemonStage.Stage2)
            {
                Debug.LogError("進化先がStage1/Stage2ではありません");
                return false;
            }

            // 通常進化チェック
            if (!viaRareCandy)
            {
                if (evolutionCard.evolvesFrom != target.data.cardName)
                {
                    Debug.Log($"→ 進化に失敗（{target.data.cardName}から{evolutionCard.cardName}には進化できません）");
                    return false;
                }

                // このターンに出したばかりは進化不可
                if (target.turnsInPlay <= 0)
                {
                    Debug.Log("→ 出したばかりは進化不可");
                    return false;
                }
            }
            // ふしぎなアメ経由チェック
            else
            {
                // たねから2進化のみ
                if (target.data.stage != PokemonStage.Basic || evolutionCard.stage != PokemonStage.Stage2)
                {
                    Debug.Log("→ ふしぎなアメは たね→2進化 のみ");
                    return false;
                }

                // このターンに出したばかりは不可
                if (target.wasPlayedThisTurn)
                {
                    Debug.Log("→ このターンに出したたねには《ふしぎなアメ》不可");
                    return false;
                }

                // 進化系統チェック（中間進化が存在するか）
                // 簡易実装: evolvesFromフィールドを信頼
                if (string.IsNullOrEmpty(evolutionCard.evolvesFrom))
                {
                    Debug.Log("→ 進化に失敗（進化系統が不明）");
                    return false;
                }
            }

            // 進化実行
            PerformEvolution(player, target, evolutionCard, viaRareCandy);
            return true;
        }

        private void PerformEvolution(PlayerController player, PokemonInstance target, PokemonCardData evolutionCard, bool viaRareCandy)
        {
            // 新しいポケモンインスタンスを作成
            GameObject newObj = new GameObject(evolutionCard.cardName);
            PokemonInstance newPokemon = newObj.AddComponent<PokemonInstance>();
            newPokemon.Initialize(evolutionCard, player.playerIndex);

            // 既存のダメージ、エネルギー、どうぐを引き継ぐ
            newPokemon.currentDamage = target.currentDamage;
            newPokemon.attachedEnergies.AddRange(target.attachedEnergies);
            newPokemon.attachedTool = target.attachedTool;
            newPokemon.turnsInPlay = target.turnsInPlay;
            newPokemon.wasPlayedThisTurn = false;

            // 状態異常はクリア（進化時にクリアされる）
            newPokemon.statusCondition = StatusCondition.None;

            // バトル場/ベンチの位置を引き継ぐ
            if (player.activeSlot == target)
            {
                player.activeSlot = newPokemon;
            }
            else
            {
                int benchIndex = player.benchSlots.IndexOf(target);
                if (benchIndex >= 0)
                {
                    player.benchSlots[benchIndex] = newPokemon;
                }
            }

            // 旧ポケモンを破棄
            Destroy(target.gameObject);

            Debug.Log($"{player.playerName}: {target.data.cardName} → {newPokemon.data.cardName} に進化{(viaRareCandy ? "（ふしぎなアメ）" : "")}");
        }

        /// <summary>
        /// 進化可能かチェック（UI表示用）
        /// </summary>
        public bool CanEvolve(PokemonInstance target, PokemonCardData evolutionCard)
        {
            if (target == null || evolutionCard == null) return false;
            if (evolutionCard.stage != PokemonStage.Stage1 && evolutionCard.stage != PokemonStage.Stage2) return false;
            if (evolutionCard.evolvesFrom != target.data.cardName) return false;
            if (target.turnsInPlay <= 0) return false;
            return true;
        }
    }
}
