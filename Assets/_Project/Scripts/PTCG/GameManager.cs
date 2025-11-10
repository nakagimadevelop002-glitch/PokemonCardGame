using UnityEngine;
using System.Collections.Generic;

namespace PTCG
{
    /// <summary>
    /// ゲーム全体の状態管理（Singleton）
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Players")]
        public PlayerController player1;
        public PlayerController player2;

        [Header("Game State")]
        public int currentPlayerIndex; // 0 or 1
        public int firstPlayerIndex;
        public int turnCount;
        public string currentPhase = "setup"; // setup, draw, main, end
        public string stadiumInPlay;

        [Header("Winner")]
        public int winnerIndex = -1; // -1 = no winner
        public string winReason = ""; // 勝利理由

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // カメラがない場合は作成
            if (Camera.main == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                Camera cam = cameraObject.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.transform.position = new Vector3(0, 1, -10);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                // Debug.Log("Main Camera created by GameManager");
            }
        }

        public void StartGame(List<CardData> deck1, List<CardData> deck2)
        {
            Debug.Log("=== Game Start ===");

            // Initialize players
            player1.Initialize("Player1", 0, deck1);
            player2.Initialize("Player2", 1, deck2);

            // Setup phase: draw 7 cards
            bool p1Valid = SetupPlayer(player1);
            bool p2Valid = SetupPlayer(player2);

            if (!p1Valid || !p2Valid)
            {
                Debug.LogError("Setup failed: no basic pokemon");
                return;
            }

            // Set prizes
            player1.SetupPrizes(6);
            player2.SetupPrizes(6);

            // Decide first player (random)
            firstPlayerIndex = Random.Range(0, 2);
            currentPlayerIndex = firstPlayerIndex;

            turnCount = 1;
            currentPhase = "draw";

            Debug.Log($"First player: Player{firstPlayerIndex + 1}");
            Debug.Log("Game ready! Starting turn.");

            StartTurn();
        }

        private bool SetupPlayer(PlayerController player)
        {
            player.Draw(7);

            // Mulligan check
            while (!player.HasBasicInHand())
            {
                // Debug.Log($"{player.playerName} has no basic - mulligan");
                player.mulligansGiven++;

                // Opponent draws 1 extra
                PlayerController opponent = player == player1 ? player2 : player1;
                opponent.Draw(1);

                // Shuffle hand back to deck
                player.deck.AddRange(player.hand);
                player.hand.Clear();
                player.ShuffleDeck();
                player.Draw(7);

                if (player.mulligansGiven >= 10)
                {
                    Debug.LogError($"{player.playerName} too many mulligans - auto lose");
                    return false;
                }
            }

            // Auto-select opening pokemon (HTML logic: priority order)
            AutoSelectOpening(player);  // ポケモンカードゲーム公式ルール：バトル場に1匹必須配置

            return true; // マリガン完了でセットアップ成功
        }

        private void AutoSelectOpening(PlayerController player)
        {
            // Priority order (from HTML)
            string[] priority = { "Ralts", "MewTail", "Drifloon", "Mashimashira", "MewEX", "LillieClefairyEX" };

            foreach (var cardID in priority)
            {
                CardData found = player.hand.Find(c => c is PokemonCardData pkm && pkm.cardID == cardID && pkm.stage == PokemonStage.Basic);
                if (found != null)
                {
                    SpawnPokemonToActive(player, found as PokemonCardData);
                    player.hand.Remove(found);
                    return;
                }
            }

            // Fallback: any basic
            CardData anyBasic = player.hand.Find(c => c is PokemonCardData pkm && pkm.stage == PokemonStage.Basic);
            if (anyBasic != null)
            {
                SpawnPokemonToActive(player, anyBasic as PokemonCardData);
                player.hand.Remove(anyBasic);
            }
        }

        public void SpawnPokemonToActive(PlayerController player, PokemonCardData data)
        {
            GameObject go = new GameObject(data.cardName);
            PokemonInstance instance = go.AddComponent<PokemonInstance>();
            instance.Initialize(data, player.playerIndex);
            player.activeSlot = instance;
            // Debug.Log($"{player.playerName} placed {data.cardName} as active");
        }

        /// <summary>
        /// ポケモンをベンチに配置
        /// </summary>
        public bool SpawnPokemonToBench(PlayerController player, PokemonCardData data)
        {
            if (player.benchSlots.Count >= 5)
            {
                Debug.Log($"{player.playerName}: ベンチが満員です");
                return false;
            }

            GameObject go = new GameObject(data.cardName);
            PokemonInstance instance = go.AddComponent<PokemonInstance>();
            instance.Initialize(data, player.playerIndex);
            player.benchSlots.Add(instance);
            // Debug.Log($"{player.playerName} placed {data.cardName} on bench (bench count: {player.benchSlots.Count})");
            return true;
        }

        public void StartTurn()
        {
            PlayerController current = GetCurrentPlayer();
            current.OnNewTurn();

            // Draw phase (skip first turn for first player in PTCG rules)
            if (!(currentPlayerIndex == firstPlayerIndex && turnCount == 1))
            {
                currentPhase = "draw";
                current.Draw(1);
            }

            currentPhase = "main";
            Debug.Log($"=== Turn {turnCount} - {current.playerName} ===");

            // AI自動起動
            if (current.isAI && AIController.Instance != null)
            {
                AIController.Instance.ExecuteAITurn(current);
            }

            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }
        }

        public void EndTurn()
        {
            PlayerController current = GetCurrentPlayer();

            // Between-turns effects (poison, burn, etc.)
            ProcessBetweenTurnEffects(current);

            currentPhase = "end";

            // Switch player
            currentPlayerIndex = 1 - currentPlayerIndex;
            if (currentPlayerIndex == firstPlayerIndex)
            {
                turnCount++;
            }

            StartTurn();
        }

        private void ProcessBetweenTurnEffects(PlayerController player)
        {
            if (player.activeSlot == null) return;

            var active = player.activeSlot;

            // Poison: 10 damage
            if (active.statusCondition == StatusCondition.Poison)
            {
                active.TakeDamage(10);
                Debug.Log($"{active.data.cardName} took 10 poison damage");
            }

            // Burn: flip coin, 20 damage on heads
            if (active.statusCondition == StatusCondition.Burn)
            {
                bool heads = Random.Range(0, 2) == 0;
                if (heads)
                {
                    active.TakeDamage(20);
                    Debug.Log($"{active.data.cardName} took 20 burn damage");
                }
            }

            // Sleep: flip coin to wake up
            if (active.statusCondition == StatusCondition.Sleep)
            {
                bool heads = Random.Range(0, 2) == 0;
                if (heads)
                {
                    active.statusCondition = StatusCondition.None;
                    Debug.Log($"{player.playerName} の《{active.data.cardName}》は ねむり から目覚めた");
                }
                else
                {
                    Debug.Log($"{player.playerName} の《{active.data.cardName}》は ねむり 継続");
                }
            }

            // Paralysis: recover after 1 turn
            if (active.statusCondition == StatusCondition.Paralysis)
            {
                active.paralysisTurns--;
                if (active.paralysisTurns <= 0)
                {
                    active.ClearStatus();
                    Debug.Log($"{active.data.cardName}: まひ 回復");
                }
            }

            // Check KO
            if (active.IsKnockedOut)
            {
                KnockoutPokemon(player, active);
            }
        }

        public void KnockoutPokemon(PlayerController owner, PokemonInstance pokemon)
        {
            // 詳細ログ: 誰のポケモンがきぜつ
            Debug.Log($"【きぜつ】{owner.playerName}の《{pokemon.data.cardName}》がきぜつ！");

            // Move to discard
            owner.discard.Add(pokemon.data);

            // Attached cards to discard
            foreach (var energy in pokemon.attachedEnergies)
            {
                owner.discard.Add(energy);
            }
            if (pokemon.attachedTool != null)
            {
                owner.discard.Add(pokemon.attachedTool);
            }

            // Opponent takes prize
            PlayerController opponent = owner == player1 ? player2 : player1;
            int prizeCount = pokemon.data.isEX ? 2 : 1;
            TakePrizes(opponent, prizeCount);

            // Update UI after prize taken
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            // Destroy game object
            bool wasActive = (pokemon == owner.activeSlot);
            if (wasActive)
            {
                owner.activeSlot = null;
            }
            else
            {
                owner.benchSlots.Remove(pokemon);
            }
            Destroy(pokemon.gameObject);

            // Check win condition: サイド0枚
            if (opponent.prizes.Count == 0)
            {
                SetWinner(opponent.playerIndex, "サイド0枚獲得");
                return;
            }

            // Check win condition: バトル場が空 & ベンチも空
            if (wasActive)
            {
                if (owner.benchSlots.Count == 0)
                {
                    SetWinner(opponent.playerIndex, "相手の場にポケモンがいない");
                    return;
                }

                // ベンチからバトル場へ移動（モーダル選択）
                PromptBenchSelection(owner);
            }
        }

        private void TakePrizes(PlayerController player, int count)
        {
            int prizesBefore = player.prizes.Count;
            for (int i = 0; i < count; i++)
            {
                if (player.prizes.Count > 0)
                {
                    player.hand.Add(player.prizes[player.prizes.Count - 1]);
                    player.prizes.RemoveAt(player.prizes.Count - 1);
                }
            }
            int prizesAfter = player.prizes.Count;
            int actualTaken = prizesBefore - prizesAfter;

            // 詳細ログ: 誰が何枚サイド獲得→残り何枚
            Debug.Log($"【サイド獲得】{player.playerName}が{actualTaken}枚獲得！(残りサイド: {prizesBefore}枚→{prizesAfter}枚)");
        }

        /// <summary>
        /// バトル場が空になった際、ベンチから選択してバトル場へ移動
        /// </summary>
        private void PromptBenchSelection(PlayerController owner)
        {
            // AIの場合は自動選択
            if (owner.isAI || ModalSystem.Instance == null)
            {
                owner.activeSlot = owner.benchSlots[0];
                owner.benchSlots.RemoveAt(0);
                Debug.Log($"【強制交代】{owner.playerName}: ベンチから《{owner.activeSlot.data.cardName}》をバトル場へ（自動選択）");
                if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
                return;
            }

            // プレイヤーの場合はモーダル選択
            var options = new List<SelectOption<int>>();
            for (int i = 0; i < owner.benchSlots.Count; i++)
            {
                var bench = owner.benchSlots[i];
                int currentHP = bench.data.baseHP - bench.currentDamage;
                options.Add(new SelectOption<int>($"{bench.data.cardName} (HP: {currentHP}/{bench.data.baseHP})", i));
            }

            ModalSystem.Instance.OpenSelectModal(
                $"{owner.playerName}: バトル場へ出すポケモンを選択",
                options,
                (selectedIndex) =>
                {
                    if (selectedIndex < 0 || selectedIndex >= owner.benchSlots.Count) return;

                    var selectedPokemon = owner.benchSlots[selectedIndex];
                    owner.activeSlot = selectedPokemon;
                    owner.benchSlots.RemoveAt(selectedIndex);

                    Debug.Log($"【強制交代】{owner.playerName}: ベンチから《{selectedPokemon.data.cardName}》をバトル場へ");

                    if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
                },
                defaultFirst: true
            );
        }

        public PlayerController GetCurrentPlayer()
        {
            return currentPlayerIndex == 0 ? player1 : player2;
        }

        public PlayerController GetOpponentPlayer()
        {
            return currentPlayerIndex == 0 ? player2 : player1;
        }

        /// <summary>
        /// 勝敗を設定（勝者のインデックスと勝利理由を記録）
        /// </summary>
        public void SetWinner(int winnerPlayerIndex, string reason)
        {
            winnerIndex = winnerPlayerIndex;
            winReason = reason;

            var winner = winnerPlayerIndex == 0 ? player1 : player2;
            Debug.Log($"=== {winner.playerName} WINS! ===");
            Debug.Log($"勝利理由: {reason}");

            // 勝敗確定時のモーダル表示
            ShowWinnerModal(winner, reason);
        }

        /// <summary>
        /// 勝敗結果モーダルを表示
        /// </summary>
        private void ShowWinnerModal(PlayerController winner, string reason)
        {
            if (ModalSystem.Instance == null) return;

            string message = $"{winner.playerName} の勝利！\n\n勝利理由: {reason}";
            ModalSystem.Instance.OpenConfirmModal("ゲーム終了", message, (result) =>
            {
                if (result)
                {
                    RestartGame();
                }
            });
        }

        /// <summary>
        /// ゲームを最初から再スタート
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("=== Game Restart ===");

            // 勝敗状態リセット
            winnerIndex = -1;
            winReason = "";
            turnCount = 0;
            currentPhase = "setup";
            stadiumInPlay = "";

            // プレイヤー状態リセット
            if (player1 != null)
            {
                CleanupPlayer(player1);
            }
            if (player2 != null)
            {
                CleanupPlayer(player2);
            }

            // UI完全リセット
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            // ゲーム再開
            if (GameInitializer.Instance != null)
            {
                GameInitializer.Instance.StartNewGame();
            }
        }

        /// <summary>
        /// プレイヤーの状態をクリーンアップ
        /// </summary>
        private void CleanupPlayer(PlayerController player)
        {
            // フィールドのポケモンを削除
            if (player.activeSlot != null)
            {
                Destroy(player.activeSlot.gameObject);
                player.activeSlot = null;
            }

            foreach (var bench in player.benchSlots)
            {
                if (bench != null)
                {
                    Destroy(bench.gameObject);
                }
            }
            player.benchSlots.Clear();

            // ゾーンをクリア
            player.hand.Clear();
            player.deck.Clear();
            player.discard.Clear();
            player.lostZone.Clear();
            player.prizes.Clear();

            // ターン状態リセット
            player.ResetTurnFlags();
            player.mulligansGiven = 0;
        }
    }
}
