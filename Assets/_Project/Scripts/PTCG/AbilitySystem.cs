using System.Linq;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// 特性システム（ポケモンの特性発動）
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        public static AbilitySystem Instance { get; private set; }

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
        /// 特性を使用
        /// </summary>
        public bool UseAbility(PlayerController player, PokemonInstance pokemon, string abilityID)
        {
            if (pokemon == null)
            {
                Debug.Log("対象のポケモンを選択してください");
                return false;
            }

            var ability = pokemon.data.abilities.Find(a => a.abilityID == abilityID);
            if (ability == null)
            {
                Debug.Log("このポケモンに使える特性はありません");
                return false;
            }

            // Once-per-turn チェック
            if (ability.oncePerTurn && pokemon.abilityUsedFlags.ContainsKey(abilityID) && pokemon.abilityUsedFlags[abilityID])
            {
                Debug.Log($"このターンは《{ability.abilityName}》をすでに使用しました");
                return false;
            }

            bool success = false;

            switch (abilityID)
            {
                case "adrena_brain":
                    success = UseAdrenaBrain(player, pokemon);
                    break;

                case "mysterious_tail":
                    success = UseMysteriousTail(player, pokemon);
                    break;

                case "psychic_embrace":
                    // サイコエンブレイスはEnergySystemで処理
                    Debug.Log("《サイコエンブレイス》はエネルギーシステムから使用してください");
                    return false;

                case "restart":
                    success = UseRestart(player, pokemon);
                    break;

                case "refinement":
                    success = UseRefinement(player, pokemon);
                    break;

                case "fairy_zone":
                    // 常駐特性（場にいるだけで効果）
                    Debug.Log("《フェアリーゾーン》は常駐特性です");
                    return false;

                default:
                    Debug.Log($"特性《{abilityID}》は未実装です");
                    return false;
            }

            if (success && ability.oncePerTurn)
            {
                pokemon.abilityUsedFlags[abilityID] = true;
            }

            return success;
        }

        /// <summary>
        /// アドレナブレイン（マシマシラ）
        /// </summary>
        private bool UseAdrenaBrain(PlayerController player, PokemonInstance pokemon)
        {
            // 条件：このポケモンに悪エネルギーがついている
            bool hasDarkEnergy = pokemon.attachedEnergies.Any(e =>
                (e.isBasic && e.providesType == PokemonType.D) ||
                (e.cardID == "BasicDarkness"));

            if (!hasDarkEnergy)
            {
                Debug.Log("《アドレナブレイン》：このポケモンに悪エネルギーが付いていません");
                return false;
            }

            var gm = GameManager.Instance;
            var opponent = player == gm.player1 ? gm.player2 : gm.player1;

            // 自分の場のダメカンがあるポケモン
            var sources = player.GetAllPokemons().Where(p => p.currentDamage >= 10).ToList();
            if (sources.Count == 0)
            {
                Debug.Log("→ 自分の場にダメカンがあるポケモンがいません");
                return false;
            }

            // 相手の場のポケモン
            var targets = opponent.GetAllPokemons();
            if (targets.Count == 0)
            {
                Debug.Log("→ 相手の場にポケモンがいません");
                return false;
            }

            // TODO: モーダル選択システム実装後に追加
            Debug.Log("→ アドレナブレイン（モーダル選択未実装）");
            return false;
        }

        /// <summary>
        /// ふしぎなしっぽ（ミュウ）
        /// </summary>
        private bool UseMysteriousTail(PlayerController player, PokemonInstance pokemon)
        {
            // バトル場限定
            if (player.activeSlot != pokemon)
            {
                Debug.Log("《ふしぎなしっぽ》はバトル場にいるときのみ使用できます");
                return false;
            }

            int count = Mathf.Min(6, player.deck.Count);
            if (count <= 0)
            {
                Debug.Log("山札が残っていません");
                return false;
            }

            // 上6枚からグッズを探す
            var topCards = player.deck.Skip(player.deck.Count - count).ToList();
            var items = topCards.OfType<TrainerCardData>()
                .Where(t => t.trainerType == TrainerType.Item).ToList();

            if (items.Count == 0)
            {
                Debug.Log($"《ふしぎなしっぽ》：上{count}枚にグッズなし → 山札を切り直し");
                player.ShuffleDeck();
                return true;
            }

            // TODO: モーダル選択システム実装後に追加
            Debug.Log("→ ふしぎなしっぽ（モーダル選択未実装）");
            return false;
        }

        /// <summary>
        /// リスタート（ミュウex）
        /// </summary>
        private bool UseRestart(PlayerController player, PokemonInstance pokemon)
        {
            // 手札が3枚になるまで引く
            int drawCount = Mathf.Max(0, 3 - player.hand.Count);
            if (drawCount > 0)
            {
                player.Draw(drawCount);
                Debug.Log($"《リスタート》：手札が3枚になるまで{drawCount}枚ドロー");
                return true;
            }
            else
            {
                Debug.Log("《リスタート》：手札が3枚以上あるため使用できません");
                return false;
            }
        }

        /// <summary>
        /// 精製（キルリア）
        /// </summary>
        private bool UseRefinement(PlayerController player, PokemonInstance pokemon)
        {
            if (player.hand.Count == 0)
            {
                Debug.Log("《精製》：手札がありません");
                return false;
            }

            // TODO: モーダル選択システム実装後に追加
            Debug.Log("→ 精製（モーダル選択未実装）");
            return false;
        }
    }
}
