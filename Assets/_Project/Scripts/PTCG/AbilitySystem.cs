using System.Collections.Generic;
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
                return false;
            }

            var ability = pokemon.data.abilities.Find(a => a.abilityID == abilityID);
            if (ability == null)
            {
                return false;
            }

            // Once-per-turn チェック
            if (ability.oncePerTurn && pokemon.abilityUsedFlags.ContainsKey(abilityID) && pokemon.abilityUsedFlags[abilityID])
            {
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
                    return false;

                default:
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
                return false;
            }


            var gm = GameManager.Instance;
            var opponent = player == gm.player1 ? gm.player2 : gm.player1;

            // 自分の場のダメカンがあるポケモン
            var sources = player.GetAllPokemons().Where(p => p.currentDamage >= 10).ToList();
            if (sources.Count == 0)
            {
                return false;
            }

            foreach (var p in sources)
            {
            }

            // 相手の場のポケモン
            var targets = opponent.GetAllPokemons();
            if (targets.Count == 0)
            {
                return false;
            }

            foreach (var t in targets)
            {
            }

            // 第1段階: 自分のポケモン1匹を選択（移動元）
            var sourceOptions = sources.Select(p => new SelectOption<PokemonInstance>(
                p.data.cardName + "（ダメカン" + (p.currentDamage / 10) + "個）",
                p
            )).ToList();


            ModalSystem.Instance.OpenSelectModal(
                "アドレナブレイン：移動元（自分の場）を選択",
                sourceOptions,
                (selectedSource) =>
                {
                    if (selectedSource == null)
                    {
                        return;
                    }


                    // 第2段階: ダメカンの個数を選択（1～3個、ポケモンのダメカン数まで）
                    int maxDamageCounters = Mathf.Min(3, selectedSource.currentDamage / 10);
                    var countOptions = new List<SelectOption<int>>();
                    for (int i = 1; i <= maxDamageCounters; i++)
                    {
                        countOptions.Add(new SelectOption<int>(i + "個", i * 10));
                    }


                    ModalSystem.Instance.OpenSelectModal(
                        "移動するダメカンの数",
                        countOptions,
                        (damageAmount) =>
                        {
                            if (damageAmount == 0)
                            {
                                return;
                            }

                            int damageCounters = damageAmount / 10;

                            // 第3段階: 相手のポケモンを1匹選択（移動先）
                            var targetOptions = targets.Select(p => new SelectOption<PokemonInstance>(
                                p.data.cardName + "（ダメカン" + (p.currentDamage / 10) + "個）",
                                p
                            )).ToList();


                            ModalSystem.Instance.OpenSelectModal(
                                "移動先（相手の場）を選択",
                                targetOptions,
                                (selectedTarget) =>
                                {
                                    if (selectedTarget == null)
                                    {
                                        return;
                                    }


                                    // ダメージを移動
                                    selectedSource.currentDamage = Mathf.Max(0, selectedSource.currentDamage - damageAmount);
                                    selectedTarget.currentDamage += damageAmount;


                                    // きぜつチェック
                                    if (selectedTarget.IsKnockedOut)
                                    {
                                        gm.KnockoutPokemon(opponent, selectedTarget);
                                    }

                                    UIManager.Instance?.UpdateUI();
                                },
                                defaultFirst: true
                            );
                        },
                        defaultFirst: true
                    );
                },
                defaultFirst: true
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
                return false;
            }

            int count = Mathf.Min(6, player.deck.Count);
            if (count <= 0)
            {
                return false;
            }

            // 上6枚からグッズを探す
            var topCards = player.deck.Skip(player.deck.Count - count).ToList();
            var items = topCards.OfType<TrainerCardData>()
                .Where(t => t.trainerType == TrainerType.Item).ToList();

            if (items.Count == 0)
            {
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
                        player.ShuffleDeck();
                        return;
                    }

                    // 選択したカードを山札から削除し手札に追加
                    player.deck.Remove(selectedCard);
                    player.hand.Add(selectedCard);

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
                return true;
            }
            else
            {
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
                        return;
                    }

                    // 手札から削除してトラッシュへ
                    player.hand.Remove(selectedCard);
                    player.discard.Add(selectedCard);

                    // 2枚ドロー
                    player.Draw(2);

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
                return false;
            }

            // 自分の場の超ポケモン（ダメカン20を乗せても気絶しないもの）
            var psychicPokemons = player.GetAllPokemons()
                .Where(p => p.data.type == PokemonType.P && (p.currentDamage + 20 < p.MaxHP))
                .ToList();

            if (psychicPokemons.Count == 0)
            {
                return false;
            }

            // 対象を選択
            var targetOptions = psychicPokemons.Select(p => new SelectOption<PokemonInstance>(
                p.data.cardName + "（HP " + (p.MaxHP - p.currentDamage) + "/" + p.MaxHP + "）",
                p
            )).ToList();

            ModalSystem.Instance.OpenSelectModal(
                "《サイコエンブレイス》エネルギーを付けるポケモンを選択",
                targetOptions,
                (selectedPokemon) =>
                {
                    if (selectedPokemon == null)
                    {
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
