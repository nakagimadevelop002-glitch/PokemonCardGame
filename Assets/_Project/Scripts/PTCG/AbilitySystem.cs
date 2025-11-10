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
                    success = UsePsychicEmbrace(player, pokemon);
                    break;

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

            // 第1段階: 自分のポケモンからダメカンを最大2個選択
            var sourceOptions = sources.Select(p => new SelectOption<PokemonInstance>(
                $"{p.data.cardName}（ダメカン{p.currentDamage / 10}個）",
                p
            )).ToList();

            ModalSystem.Instance.OpenMultiSelectModal(
                "ダメカンを取るポケモンを3個まで選択",
                sourceOptions,
                3,
                (selectedPokemons) =>
                {
                    if (selectedPokemons == null || selectedPokemons.Count == 0)
                    {
                        Debug.Log("→ アドレナブレイン: ダメカンを選択しませんでした");
                        return;
                    }

                    // 各ポケモンから10ダメージ（1個）ずつ減らす
                    int totalDamageToMove = selectedPokemons.Count * 10;
                    foreach (var p in selectedPokemons)
                    {
                        p.currentDamage = Mathf.Max(0, p.currentDamage - 10);
                    }

                    // 第2段階: 相手のポケモンを1匹選択
                    var targetOptions = targets.Select(p => new SelectOption<PokemonInstance>(
                        $"{p.data.cardName}（HP {p.data.baseHP - p.currentDamage}/{p.data.baseHP}）",
                        p
                    )).ToList();

                    ModalSystem.Instance.OpenSelectModal(
                        "ダメカンを乗せる相手のポケモンを選択",
                        targetOptions,
                        (selectedTarget) =>
                        {
                            if (selectedTarget == null)
                            {
                                Debug.Log("→ アドレナブレイン: 相手のポケモンを選択しませんでした");
                                return;
                            }

                            // ダメージを乗せる
                            selectedTarget.currentDamage += totalDamageToMove;
                            Debug.Log($"→ アドレナブレイン: ダメカン{selectedPokemons.Count}個を《{selectedTarget.data.cardName}》へ移動");

                            // きぜつチェック
                            if (selectedTarget.IsKnockedOut)
                            {
                                gm.KnockoutPokemon(opponent, selectedTarget);
                            }

                            UIManager.Instance?.UpdateUI();
                        },
                        defaultFirst: true
                    );
                }
            );

            return true;
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

            // グッズから1枚選択
            var itemOptions = items.Select(card => new SelectOption<TrainerCardData>(
                card.cardName,
                card
            )).ToList();

            ModalSystem.Instance.OpenSelectModal(
                $"山札上{count}枚から グッズ を1枚選択",
                itemOptions,
                (selectedCard) =>
                {
                    if (selectedCard == null)
                    {
                        Debug.Log("→ ふしぎなしっぽ: グッズを選択しませんでした");
                        player.ShuffleDeck();
                        return;
                    }

                    // 選択したカードを山札から削除し手札に追加
                    player.deck.Remove(selectedCard);
                    player.hand.Add(selectedCard);
                    Debug.Log($"→ ふしぎなしっぽ: 《{selectedCard.cardName}》を手札に");

                    // 山札をシャッフル
                    player.ShuffleDeck();
                    UIManager.Instance?.UpdateUI();
                },
                defaultFirst: true
            );

            return true;
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

            // 手札から1枚選択してトラッシュ
            var handOptions = player.hand.Select(card => new SelectOption<CardData>(
                card.cardName,
                card
            )).ToList();

            ModalSystem.Instance.OpenSelectModal(
                "手札から1枚トラッシュ",
                handOptions,
                (selectedCard) =>
                {
                    if (selectedCard == null)
                    {
                        Debug.Log("→ 精製: カードを選択しませんでした");
                        return;
                    }

                    // 手札から削除してトラッシュへ
                    player.hand.Remove(selectedCard);
                    player.discard.Add(selectedCard);
                    Debug.Log($"→ 精製: 《{selectedCard.cardName}》をトラッシュ");

                    // 2枚ドロー
                    player.Draw(2);
                    Debug.Log("→ 精製: 2枚ドロー");

                    UIManager.Instance?.UpdateUI();
                },
                defaultFirst: true
            );

            return true;
        }

        /// <summary>
        /// サイコエンブレイス（サーナイトex）
        /// </summary>
        private bool UsePsychicEmbrace(PlayerController player, PokemonInstance pokemon)
        {
            // トラッシュに基本超エネルギーがあるか
            var hasPsychicEnergy = player.discard.OfType<EnergyCardData>()
                .Any(e => e.cardID == "BasicPsychic" && e.isBasic);

            if (!hasPsychicEnergy)
            {
                Debug.Log("《サイコエンブレイス》：トラッシュに基本超エネルギーがありません");
                return false;
            }

            // 自分の場の超ポケモン（ダメカン20を乗せても気絶しないもの）
            var psychicPokemons = player.GetAllPokemons()
                .Where(p => p.data.type == PokemonType.P && (p.currentDamage + 20 < p.MaxHP))
                .ToList();

            if (psychicPokemons.Count == 0)
            {
                Debug.Log("→ 対象の超ポケモンがいません（または気絶してしまいます）");
                return false;
            }

            // 対象を選択
            var targetOptions = psychicPokemons.Select(p => new SelectOption<PokemonInstance>(
                $"{p.data.cardName}（HP {p.MaxHP - p.currentDamage}/{p.MaxHP}）",
                p
            )).ToList();

            ModalSystem.Instance.OpenSelectModal(
                "《サイコエンブレイス》エネルギーを付けるポケモンを選択",
                targetOptions,
                (selectedPokemon) =>
                {
                    if (selectedPokemon == null)
                    {
                        Debug.Log("→ サイコエンブレイス: ポケモンを選択しませんでした");
                        return;
                    }

                    // EnergySystemで処理
                    if (EnergySystem.Instance.UsePsychicEmbrace(player, selectedPokemon))
                    {
                        UIManager.Instance?.UpdateUI();
                    }
                },
                defaultFirst: true
            );

            return true;
        }
    }
}
