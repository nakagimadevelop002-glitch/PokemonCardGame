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
            Debug.Log("AI: 行動開始");

            var gm = GameManager.Instance;
            var opponent = ai == gm.player1 ? gm.player2 : gm.player1;

            // 0. バトル場が空ならベンチから出す
            if (ai.activeSlot == null && ai.benchSlots.Count > 0)
            {
                ai.activeSlot = ai.benchSlots[0];
                ai.benchSlots.RemoveAt(0);
                Debug.Log($"AI: 《{ai.activeSlot.data.cardName}》をバトル場に出した");

                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                }

                yield return new WaitForSeconds(actionDelay);
            }

            // 1. 手札のたねポケモンを全てベンチに出す
            yield return PlaceBasicPokemon(ai);

            // 2. 進化処理（ラルトス→キルリア→サーナイトex）
            yield return PerformEvolutions(ai);

            // 3. エネルギーを手貼り
            yield return AttachEnergyFromHand(ai);

            // 4. サイコエンブレイス特性でエネルギー加速
            yield return UsePsychicEmbrace(ai);

            // 5. 攻撃可能なら攻撃
            bool canAttack = BattleSystem.Instance.CanAttack(ai);
            Debug.Log($"AI: 攻撃可能? {canAttack}");
            if (canAttack)
            {
                Debug.Log("AI: 攻撃実行");
                BattleSystem.Instance.PerformAttack(ai, opponent);

                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                }

                yield return new WaitForSeconds(actionDelay);
            }
            else
            {
                string reason = BattleSystem.Instance.GetAttackDisabledReason(ai);
                Debug.Log($"AI: 攻撃不可 - {reason}");
            }

            // 6. ターン終了
            Debug.Log("AI: ターン終了");
            yield return new WaitForSeconds(actionDelay);
            gm.EndTurn();
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

                Debug.Log($"AI: 《{basicCard.cardName}》をベンチに出した");
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
                Debug.Log("AI: エネルギー手貼り済み");
                yield break;
            }

            var energyCard = ai.hand.OfType<EnergyCardData>().FirstOrDefault();
            if (energyCard == null)
            {
                Debug.Log("AI: 手札にエネルギーなし");
                yield break;
            }

            if (ai.activeSlot == null)
            {
                Debug.Log("AI: バトル場が空");
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
