using System.Collections;
using System.Linq;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// AI制御システム（簡易判断木ベース）
    /// </summary>
    public class AIController : MonoBehaviour
    {
        public static AIController Instance { get; private set; }

        [Header("AI Settings")]
        public float actionDelay = 0.35f; // 行動間隔（秒）

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
        /// AIターンを実行
        /// </summary>
        public void ExecuteAITurn(PlayerController aiPlayer)
        {
            StartCoroutine(AITurnSequence(aiPlayer));
        }

        private IEnumerator AITurnSequence(PlayerController ai)
        {
            var gm = GameManager.Instance;
            var opponent = ai == gm.player1 ? gm.player2 : gm.player1;

            // 0. バトル場が空ならベンチから出す
            if (ai.activeSlot == null && ai.benchSlots.Count > 0)
            {
                ai.activeSlot = ai.benchSlots[0];
                ai.benchSlots.RemoveAt(0);

                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                }

                yield return new WaitForSeconds(actionDelay);
            }

            // 1. MewEX「リスタート」特性（手札補充）
            yield return UseRestartAbility(ai);

            // 2. MewTail「ふしぎなしっぽ」特性（グッズサーチ）
            yield return UseMysteriousTailAbility(ai);

            // 3. 手札のたねポケモンを全てベンチに出す
            yield return PlaceBasicPokemon(ai);

            // 4. 進化処理（ラルトス→キルリア→サーナイトex）
            yield return PerformEvolutions(ai);

            // 5. Kirlia「精製」特性（手札トラッシュ→ドロー）
            yield return UseRefinementAbility(ai);

            // 6. エネルギーを手貼り
            yield return AttachEnergyFromHand(ai);

            // 7. サイコエンブレイス特性でエネルギー加速
            yield return UsePsychicEmbrace(ai);

            // 8. Mashimashira「アドレナブレイン」特性（ダメカン移動）
            yield return UseAdrenaBrainAbility(ai);

            // 9. 攻撃可能なら攻撃
            bool canAttack = BattleSystem.Instance.CanAttack(ai);
            if (canAttack)
            {
                BattleSystem.Instance.PerformAttack(ai, opponent);

                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                }

                yield return new WaitForSeconds(actionDelay);
            }

            // 10. ターン終了
            yield return new WaitForSeconds(actionDelay);
            gm.EndTurn();
        }

        /// <summary>
        /// MewEX「リスタート」特性使用
        /// </summary>
        private IEnumerator UseRestartAbility(PlayerController ai)
        {
            // MewEXが場にいるか確認
            var mewEX = ai.GetAllPokemons().FirstOrDefault(p => p.data.cardID == "MewEX");
            if (mewEX == null) yield break;

            // 手札が3枚未満なら使用
            if (ai.hand.Count >= 3) yield break;

            // 既に使用済みか確認
            if (mewEX.abilityUsedFlags.ContainsKey("restart") && mewEX.abilityUsedFlags["restart"])
            {
                yield break;
            }

            // リスタート使用
            int drawCount = 3 - ai.hand.Count;
            ai.Draw(drawCount);
            mewEX.abilityUsedFlags["restart"] = true;

            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            yield return new WaitForSeconds(actionDelay);
        }

        /// <summary>
        /// MewTail「ふしぎなしっぽ」特性使用
        /// </summary>
        private IEnumerator UseMysteriousTailAbility(PlayerController ai)
        {
            // MewTailがバトル場にいるか確認
            if (ai.activeSlot == null || ai.activeSlot.data.cardID != "MewTail") yield break;

            // 既に使用済みか確認
            if (ai.activeSlot.abilityUsedFlags.ContainsKey("mysterious_tail") && ai.activeSlot.abilityUsedFlags["mysterious_tail"])
            {
                yield break;
            }

            int count = Mathf.Min(6, ai.deck.Count);
            if (count <= 0) yield break;

            // 上6枚からグッズを探す
            var topCards = ai.deck.Skip(ai.deck.Count - count).ToList();
            var items = topCards.OfType<TrainerCardData>()
                .Where(t => t.trainerType == TrainerType.Item).ToList();

            if (items.Count > 0)
            {
                // 最初のグッズを自動選択
                var selectedCard = items[0];
                ai.deck.Remove(selectedCard);
                ai.hand.Add(selectedCard);
            }

            // 山札をシャッフル
            ai.ShuffleDeck();
            ai.activeSlot.abilityUsedFlags["mysterious_tail"] = true;

            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            yield return new WaitForSeconds(actionDelay);
        }

        /// <summary>
        /// Kirlia「精製」特性使用
        /// </summary>
        private IEnumerator UseRefinementAbility(PlayerController ai)
        {
            // Kirliaが場にいるか確認
            var kirlia = ai.GetAllPokemons().FirstOrDefault(p => p.data.cardID == "Kirlia");
            if (kirlia == null) yield break;

            // 手札がないなら使用不可
            if (ai.hand.Count == 0) yield break;

            // 既に使用済みか確認
            if (kirlia.abilityUsedFlags.ContainsKey("refinement") && kirlia.abilityUsedFlags["refinement"])
            {
                yield break;
            }

            // 手札からランダムに1枚トラッシュ
            var cardToTrash = ai.hand[Random.Range(0, ai.hand.Count)];
            ai.hand.Remove(cardToTrash);
            ai.discard.Add(cardToTrash);

            // 2枚ドロー
            ai.Draw(2);
            kirlia.abilityUsedFlags["refinement"] = true;

            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            yield return new WaitForSeconds(actionDelay);
        }

        /// <summary>
        /// Mashimashira「アドレナブレイン」特性使用
        /// </summary>
        private IEnumerator UseAdrenaBrainAbility(PlayerController ai)
        {
            var gm = GameManager.Instance;
            var opponent = ai == gm.player1 ? gm.player2 : gm.player1;

            // Mashimashiraが場にいるか確認
            var mashimashira = ai.GetAllPokemons().FirstOrDefault(p => p.data.cardID == "Mashimashira");
            if (mashimashira == null) yield break;

            // 悪エネルギーが付いているか確認
            bool hasDarkEnergy = mashimashira.attachedEnergies.Any(e =>
                (e.isBasic && e.providesType == PokemonType.D) ||
                (e.cardID == "BasicDarkness"));
            if (!hasDarkEnergy) yield break;

            // 既に使用済みか確認
            if (mashimashira.abilityUsedFlags.ContainsKey("adrena_brain") && mashimashira.abilityUsedFlags["adrena_brain"])
            {
                yield break;
            }

            // 自分の場のダメカンを持つポケモンを確認
            var damagedPokemons = ai.GetAllPokemons().Where(p => p.currentDamage > 0).ToList();
            if (damagedPokemons.Count == 0) yield break;

            // 最大3個のダメカンを移動（自動選択：最もダメージを受けているポケモンから）
            int totalDamageToMove = 0;
            foreach (var p in damagedPokemons.OrderByDescending(p => p.currentDamage).Take(3))
            {
                int damageToMove = Mathf.Min(10, p.currentDamage);
                p.currentDamage -= damageToMove;
                totalDamageToMove += damageToMove;
            }

            if (totalDamageToMove == 0) yield break;

            // 相手のポケモンに移動（自動選択：ランダム）
            var targets = opponent.GetAllPokemons();
            if (targets.Count > 0)
            {
                var target = targets[Random.Range(0, targets.Count)];
                target.currentDamage += totalDamageToMove;
                mashimashira.abilityUsedFlags["adrena_brain"] = true;

                // きぜつチェック
                if (target.IsKnockedOut)
                {
                    gm.KnockoutPokemon(opponent, target);
                }

                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                }

                yield return new WaitForSeconds(actionDelay);
            }
        }

        /// <summary>
        /// たねポケモンをベンチに出す
        /// </summary>
        private IEnumerator PlaceBasicPokemon(PlayerController ai)
        {
            while (ai.benchSlots.Count < 5)
            {
                var basicCard = ai.hand.OfType<PokemonCardData>()
                    .FirstOrDefault(c => c.stage == PokemonStage.Basic);

                if (basicCard == null) break;

                // ベンチに出す
                GameObject go = new GameObject(basicCard.cardName);
                PokemonInstance instance = go.AddComponent<PokemonInstance>();
                instance.Initialize(basicCard, ai.playerIndex);
                ai.benchSlots.Add(instance);
                ai.hand.Remove(basicCard);

                yield return new WaitForSeconds(actionDelay * 0.5f);
            }
        }

        /// <summary>
        /// 進化処理
        /// </summary>
        private IEnumerator PerformEvolutions(PlayerController ai)
        {
            // ラルトス → キルリア
            foreach (var pokemon in ai.GetAllPokemons())
            {
                if (pokemon.data.cardName == "Ralts" && pokemon.turnsInPlay > 0)
                {
                    var kirliaCard = ai.hand.OfType<PokemonCardData>()
                        .FirstOrDefault(c => c.cardID == "Kirlia");
                    if (kirliaCard != null)
                    {
                        if (EvolutionSystem.Instance.Evolve(ai, pokemon, kirliaCard))
                        {
                            ai.hand.Remove(kirliaCard);
                            yield return new WaitForSeconds(actionDelay);
                        }
                    }
                }
            }

            // キルリア → サーナイトex
            foreach (var pokemon in ai.GetAllPokemons())
            {
                if (pokemon.data.cardName == "Kirlia" && pokemon.turnsInPlay > 0)
                {
                    var gardevoirCard = ai.hand.OfType<PokemonCardData>()
                        .FirstOrDefault(c => c.cardID == "GardevoirEX");
                    if (gardevoirCard != null)
                    {
                        if (EvolutionSystem.Instance.Evolve(ai, pokemon, gardevoirCard))
                        {
                            ai.hand.Remove(gardevoirCard);
                            yield return new WaitForSeconds(actionDelay);
                            break; // 1体進化したら終了
                        }
                    }
                }
            }
        }

        /// <summary>
        /// エネルギーを手貼り
        /// </summary>
        private IEnumerator AttachEnergyFromHand(PlayerController ai)
        {
            if (ai.energyAttachedThisTurn)
            {
                yield break;
            }

            var energyCard = ai.hand.OfType<EnergyCardData>().FirstOrDefault();
            if (energyCard == null)
            {
                yield break;
            }

            if (ai.activeSlot == null)
            {
                yield break;
            }

            if (EnergySystem.Instance.AttachEnergyFromHand(ai, ai.activeSlot))
            {
                yield return new WaitForSeconds(actionDelay);
            }
        }

        /// <summary>
        /// サイコエンブレイス特性を使用
        /// </summary>
        private IEnumerator UsePsychicEmbrace(PlayerController ai)
        {
            if (ai.activeSlot == null) yield break;

            // 必要エネルギー数を計算
            int needUnits = 0;
            if (ai.activeSlot.data.cardID == "GardevoirEX")
            {
                int currentUnits = EnergySystem.Instance.CountEnergy(ai.activeSlot, PokemonType.P);
                needUnits = Mathf.Max(0, 3 - currentUnits);
            }
            else if (ai.activeSlot.data.cardID == "Drifloon")
            {
                int currentUnits = EnergySystem.Instance.CountTotalEnergy(ai.activeSlot);
                needUnits = Mathf.Max(0, 1 - currentUnits);
            }

            // サイコエンブレイスを必要回数実行
            for (int i = 0; i < needUnits; i++)
            {
                if (EnergySystem.Instance.UsePsychicEmbrace(ai, ai.activeSlot))
                {
                    yield return new WaitForSeconds(actionDelay);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
