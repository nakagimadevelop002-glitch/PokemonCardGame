using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PTCG
{
    /// <summary>
    /// UI全体の管理とGameManagerとの連携
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI References")]
        public Button endTurnButton;
        public Transform stadiumZone;
        public Text stadiumText;

        [Header("Opponent UI")]
        public Transform opponentDeckZone;
        public Transform opponentHandZone;
        public Transform opponentBenchZone;
        public Transform opponentPrizesZone;
        public Transform opponentActiveZone;
        public Text opponentDeckCount;
        public Text opponentPrizesCount;
        public Text opponentHandCount;
        public Text opponentDiscardCount;

        [Header("Player UI")]
        public Transform playerDeckZone;
        public Transform playerHandZone;
        public Transform playerBenchZone;
        public Transform playerPrizesZone;
        public Transform playerActiveZone;
        public Text playerDeckCount;
        public Text playerPrizesCount;
        public Text playerDiscardCount;

        [Header("Card Prefab")]
        public GameObject cardUIPrefab;

        [Header("Debug UI")]
        public Text turnText;
        public Text mulliganLogText;

        // Active card UI instances
        private List<GameObject> playerHandUICards = new List<GameObject>();
        private List<GameObject> opponentHandUICards = new List<GameObject>();
        private GameObject playerActiveUICard;
        private GameObject opponentActiveUICard;
        private List<GameObject> playerBenchUICards = new List<GameObject>();
        private List<GameObject> opponentBenchUICards = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load card prefab if not assigned
            if (cardUIPrefab == null)
            {
                cardUIPrefab = Resources.Load<GameObject>("Prefabs/CardUI");
                if (cardUIPrefab == null)
                {
                }
            }
        }

        private void Start()
        {
            // Setup button listeners
            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            // Auto-find UI references if not assigned
            AutoFindUIReferences();
        }

        private void AutoFindUIReferences()
        {
            if (endTurnButton == null)
            {
                GameObject btn = GameObject.Find("EndTurnButton");
                if (btn != null) endTurnButton = btn.GetComponent<Button>();
            }

            if (opponentDeckZone == null)
            {
                GameObject obj = GameObject.Find("OpponentDeck");
                if (obj != null) opponentDeckZone = obj.transform;
            }

            if (opponentHandZone == null)
            {
                GameObject obj = GameObject.Find("OpponentHand");
                if (obj != null) opponentHandZone = obj.transform;
            }

            if (opponentBenchZone == null)
            {
                GameObject obj = GameObject.Find("OpponentBench");
                if (obj != null) opponentBenchZone = obj.transform;
            }

            if (opponentPrizesZone == null)
            {
                GameObject obj = GameObject.Find("OpponentPrizes");
                if (obj != null) opponentPrizesZone = obj.transform;
            }

            if (opponentActiveZone == null)
            {
                GameObject obj = GameObject.Find("OpponentActive");
                if (obj != null) opponentActiveZone = obj.transform;
            }

            if (playerDeckZone == null)
            {
                GameObject obj = GameObject.Find("PlayerDeck");
                if (obj != null) playerDeckZone = obj.transform;
            }

            if (playerHandZone == null)
            {
                GameObject obj = GameObject.Find("PlayerHand");
                if (obj != null) playerHandZone = obj.transform;
            }

            if (playerBenchZone == null)
            {
                GameObject obj = GameObject.Find("PlayerBench");
                if (obj != null) playerBenchZone = obj.transform;
            }

            if (playerPrizesZone == null)
            {
                GameObject obj = GameObject.Find("PlayerPrizes");
                if (obj != null) playerPrizesZone = obj.transform;
            }

            if (playerActiveZone == null)
            {
                GameObject obj = GameObject.Find("PlayerActive");
                if (obj != null) playerActiveZone = obj.transform;
            }

            if (stadiumZone == null)
            {
                GameObject obj = GameObject.Find("Stadium");
                if (obj != null) stadiumZone = obj.transform;
            }

            // Find text labels
            if (opponentDeckCount == null)
            {
                GameObject obj = GameObject.Find("OpponentDeck");
                if (obj != null)
                {
                    Text[] texts = obj.GetComponentsInChildren<Text>();
                    if (texts.Length > 0) opponentDeckCount = texts[0];
                }
            }

            if (opponentPrizesCount == null)
            {
                GameObject obj = GameObject.Find("OpponentPrizes");
                if (obj != null)
                {
                    Text[] texts = obj.GetComponentsInChildren<Text>();
                    if (texts.Length > 0) opponentPrizesCount = texts[0];
                }
            }

            if (playerDeckCount == null)
            {
                GameObject obj = GameObject.Find("PlayerDeck");
                if (obj != null)
                {
                    Text[] texts = obj.GetComponentsInChildren<Text>();
                    if (texts.Length > 0) playerDeckCount = texts[0];
                }
            }

            if (playerPrizesCount == null)
            {
                GameObject obj = GameObject.Find("PlayerPrizes");
                if (obj != null)
                {
                    Text[] texts = obj.GetComponentsInChildren<Text>();
                    if (texts.Length > 0) playerPrizesCount = texts[0];
                }
            }

            if (stadiumText == null)
            {
                GameObject obj = GameObject.Find("Stadium");
                if (obj != null)
                {
                    Text[] texts = obj.GetComponentsInChildren<Text>();
                    if (texts.Length > 0) stadiumText = texts[0];
                }
            }

            // Debug UI
            if (turnText == null)
            {
                GameObject obj = GameObject.Find("TurnText");
                if (obj != null) turnText = obj.GetComponent<Text>();
            }

            if (mulliganLogText == null)
            {
                GameObject obj = GameObject.Find("MulliganLogText");
                if (obj != null) mulliganLogText = obj.GetComponent<Text>();
            }
        }

        private void OnEndTurnClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndTurn();
                UpdateUI();
            }
        }

        /// <summary>
        /// Update all UI based on current game state
        /// </summary>
        public void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            var player1 = GameManager.Instance.player1;
            var player2 = GameManager.Instance.player2;
            var currentPlayer = GameManager.Instance.GetCurrentPlayer();

            // Fixed layout (like HTML version): Player1(User) at bottom, Player2(AI) at top
            UpdatePlayerUI(player1, isBottom: true);  // User at bottom
            UpdatePlayerUI(player2, isBottom: false); // AI at top

            // Update stadium display
            UpdateStadiumDisplay();

            // フィールドカードのボタンを再セットアップ（UIが再生成されるため）
            GameInitializer.Instance?.SetupFieldCardButtons();

            // EndTurnButtonの有効/無効制御（AIターン中は無効化）
            if (endTurnButton != null)
            {
                endTurnButton.interactable = !currentPlayer.isAI;
            }

            // Update debug UI
            if (turnText != null)
            {
                turnText.text = "Turn: " + GameManager.Instance.turnCount;
            }

            if (mulliganLogText != null)
            {
                mulliganLogText.text = "P1: Mulligan " + player1.mulligansGiven + ", Hand " + player1.hand.Count + " | P2: Mulligan " + player2.mulligansGiven + ", Hand " + player2.hand.Count;
            }
        }

        private void UpdatePlayerUI(PlayerController player, bool isBottom)
        {
            if (player == null) return;

            if (isBottom)
            {
                // Update player (bottom) UI
                if (playerDeckCount != null)
                    playerDeckCount.text = player.deck.Count.ToString();

                if (playerPrizesCount != null)
                    playerPrizesCount.text = player.prizes.Count.ToString();

                if (playerDiscardCount != null)
                    playerDiscardCount.text = player.discard.Count.ToString();

                // Update hand display
                UpdateHandDisplay(player);

                // Update active pokemon
                UpdateActivePokemonDisplay(player, true);

                // Update bench display
                UpdateBenchDisplay(player, true);
            }
            else
            {
                // Update opponent (top) UI
                if (opponentDeckCount != null)
                    opponentDeckCount.text = player.deck.Count.ToString();

                if (opponentPrizesCount != null)
                    opponentPrizesCount.text = player.prizes.Count.ToString();

                if (opponentDiscardCount != null)
                    opponentDiscardCount.text = player.discard.Count.ToString();

                // Update opponent hand display (face-down cards)
                UpdateOpponentHandDisplay(player);

                // Update active pokemon
                UpdateActivePokemonDisplay(player, false);

                // Update bench display
                UpdateBenchDisplay(player, false);
            }
        }

        private void UpdateHandDisplay(PlayerController player)
        {
            if (playerHandZone == null || cardUIPrefab == null) return;

            // Clear existing hand UI
            foreach (var card in playerHandUICards)
            {
                if (card != null) DestroyImmediate(card);
            }
            playerHandUICards.Clear();

            // Create UI cards for each card in hand
            int cardCount = player.hand.Count;
            float spacing = 110f; // Card width + gap
            float startX = -(cardCount - 1) * spacing / 2f;

            for (int i = 0; i < cardCount; i++)
            {
                CardData cardData = player.hand[i];
                GameObject cardUI = Instantiate(cardUIPrefab, playerHandZone);

                RectTransform rt = cardUI.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);
                }

                // Set card visual data
                UpdateCardVisual(cardUI, cardData);

                playerHandUICards.Add(cardUI);
            }

            // Notify CardSelectionHandler to update button interactions
            if (CardSelectionHandler.Instance != null)
            {
                CardSelectionHandler.Instance.UpdateHandCardButtons(playerHandZone.gameObject);
            }
        }

        /// <summary>
        /// 相手手札を裏面表示で更新（上開き扇状配置）
        /// </summary>
        private void UpdateOpponentHandDisplay(PlayerController player)
        {
            if (opponentHandZone == null || cardUIPrefab == null) return;

            // Clear existing hand UI
            foreach (var card in opponentHandUICards)
            {
                if (card != null) DestroyImmediate(card);
            }
            opponentHandUICards.Clear();

            int cardCount = player.hand.Count;

            // 扇状配置パラメータ（上開き∩字型）
            float fanRadius = 800f;
            float startAngle = -25f;  // プレイヤーと同じ
            float endAngle = 25f;     // プレイヤーと同じ
            float yOffset = 200f;     // Y=-600になるように調整（-800 + 200 = -600）

            // 角度の計算
            float totalAngle = endAngle - startAngle;
            float angleStep = cardCount > 1 ? totalAngle / (cardCount - 1) : 0;

            for (int i = 0; i < cardCount; i++)
            {
                GameObject cardUI = Instantiate(cardUIPrefab, opponentHandZone);

                RectTransform rt = cardUI.GetComponent<RectTransform>();
                if (rt != null)
                {
                    // 扇状の位置計算（Cosの符号を反転して上開き扇形を実現）
                    float angle = startAngle + (angleStep * i);
                    float angleRad = angle * Mathf.Deg2Rad;

                    float x = Mathf.Sin(angleRad) * fanRadius;
                    float y = -Mathf.Cos(angleRad) * fanRadius + yOffset;  // 符号反転で上開き

                    rt.anchoredPosition = new Vector2(x, y);
                    rt.localRotation = Quaternion.Euler(0, 0, 180 + angle);  // 180度回転+扇形角度（プレイヤーから見て逆さま）
                    rt.localScale = Vector3.one;
                    // sizeDeltaはそのまま維持（CardUI親要素のImageを表示するため）
                }

                // 裏面表示: カード詳細を隠す
                SetCardBackDisplay(cardUI);

                opponentHandUICards.Add(cardUI);
            }
        }

        /// <summary>
        /// カードUIを裏面表示に設定（不要な子要素を削除）
        /// </summary>
        private void SetCardBackDisplay(GameObject cardUI)
        {
            if (cardUI == null) return;

            // 不要な子要素（CardName, CardHP, CardImage）を全て削除
            List<Transform> childrenToDestroy = new List<Transform>();
            foreach (Transform child in cardUI.transform)
            {
                if (child.name == "CardName" || child.name == "CardHP" || child.name == "CardImage")
                {
                    childrenToDestroy.Add(child);
                }
            }
            foreach (Transform child in childrenToDestroy)
            {
                DestroyImmediate(child.gameObject);
            }

            // CardUI自体（親要素）のImageコンポーネントを白にする
            Image cardUIImage = cardUI.GetComponent<Image>();
            if (cardUIImage != null)
            {
                cardUIImage.enabled = true;
                cardUIImage.color = Color.white; // 白い背景
            }
            else
            {
                // Imageコンポーネントがない場合は追加
                cardUIImage = cardUI.AddComponent<Image>();
                cardUIImage.color = Color.white;
            }

            // カードの縁（Outline）を追加（濃い青黒縁で裏面を表現）
            Outline outline = cardUI.GetComponent<Outline>();
            if (outline == null)
            {
                outline = cardUI.AddComponent<Outline>();
            }
            outline.effectColor = new Color(0.15f, 0.2f, 0.35f); // 濃い青黒の縁
            outline.effectDistance = new Vector2(4, 4);
            outline.enabled = true;
        }

        private void UpdateActivePokemonDisplay(PlayerController player, bool isPlayer)
        {
            Transform activeZone = isPlayer ? playerActiveZone : opponentActiveZone;
            if (activeZone == null || cardUIPrefab == null) return;

            // Clear existing active UI
            if (isPlayer)
            {
                if (playerActiveUICard != null) Destroy(playerActiveUICard);
                playerActiveUICard = null;
            }
            else
            {
                if (opponentActiveUICard != null) Destroy(opponentActiveUICard);
                opponentActiveUICard = null;
            }

            // Create UI card if active pokemon exists
            if (player.activeSlot != null)
            {
                GameObject cardUI = Instantiate(cardUIPrefab, activeZone);
                UpdateCardVisual(cardUI, player.activeSlot);

                if (isPlayer)
                    playerActiveUICard = cardUI;
                else
                    opponentActiveUICard = cardUI;
            }
        }

        private void UpdateBenchDisplay(PlayerController player, bool isPlayer)
        {
            Transform benchZone = isPlayer ? playerBenchZone : opponentBenchZone;
            List<GameObject> benchUICards = isPlayer ? playerBenchUICards : opponentBenchUICards;

            if (benchZone == null || cardUIPrefab == null) return;

            // Clear existing bench UI
            foreach (var card in benchUICards)
            {
                if (card != null) Destroy(card);
            }
            benchUICards.Clear();

            // Create UI cards for each bench pokemon
            int benchCount = player.benchSlots.Count;
            float spacing = 110f; // Card width + gap
            float startX = -(benchCount - 1) * spacing / 2f;

            for (int i = 0; i < benchCount; i++)
            {
                PokemonInstance benchPokemon = player.benchSlots[i];
                GameObject cardUI = Instantiate(cardUIPrefab, benchZone);

                RectTransform rt = cardUI.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(startX + i * spacing, 0f);
                }

                // Set card visual data
                UpdateCardVisual(cardUI, benchPokemon);

                benchUICards.Add(cardUI);
            }
        }


        private void UpdateCardVisual(GameObject cardUI, CardData cardData)
        {
            if (cardUI == null || cardData == null) return;

            // Find child components
            Text nameText = null;
            Text hpText = null;
            Image cardImage = null;

            foreach (Transform child in cardUI.transform)
            {
                if (child.name == "CardName")
                    nameText = child.GetComponent<Text>();
                else if (child.name == "CardHP")
                    hpText = child.GetComponent<Text>();
                else if (child.name == "CardImage")
                    cardImage = child.GetComponent<Image>();
            }

            // Update text
            if (nameText != null)
            {
                nameText.text = cardData.cardName;
            }

            if (cardData is PokemonCardData pkm)
            {
                if (hpText != null)
                {
                    hpText.text = "HP: " + pkm.baseHP;
                }
            }
            else
            {
                if (hpText != null)
                    hpText.text = "";
            }

            // Set card image sprite (if cardArt is assigned)
            if (cardImage != null)
            {
                if (cardData.cardArt != null)
                {
                    cardImage.sprite = cardData.cardArt;
                    cardImage.color = Color.white; // Reset color to white to display sprite correctly
                }
                else if (cardData is PokemonCardData pkmData)
                {
                    // Fallback: Set card image color based on type if no sprite is assigned
                    switch (pkmData.type)
                    {
                        case PokemonType.P:
                            cardImage.color = new Color(0.7f, 0.3f, 0.7f); // Psychic - Purple
                            break;
                        case PokemonType.D:
                            cardImage.color = new Color(0.3f, 0.3f, 0.3f); // Dark - Dark Gray
                            break;
                        case PokemonType.Y:
                            cardImage.color = new Color(1.0f, 0.7f, 0.8f); // Fairy - Pink
                            break;
                        case PokemonType.G:
                            cardImage.color = new Color(0.3f, 0.8f, 0.3f); // Grass - Green
                            break;
                        case PokemonType.M:
                            cardImage.color = new Color(0.7f, 0.7f, 0.8f); // Metal - Steel Gray
                            break;
                        case PokemonType.C:
                            cardImage.color = new Color(0.8f, 0.8f, 0.8f); // Colorless - Light Gray
                            break;
                        default:
                            cardImage.color = Color.white;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// PokemonInstance版のカード表示更新（現在HP表示対応）
        /// </summary>
        private void UpdateCardVisual(GameObject cardUI, PokemonInstance pokemon)
        {
            if (cardUI == null || pokemon == null || pokemon.data == null) return;

            // Find child components
            Text nameText = null;
            Text hpText = null;
            Image cardImage = null;

            foreach (Transform child in cardUI.transform)
            {
                if (child.name == "CardName")
                    nameText = child.GetComponent<Text>();
                else if (child.name == "CardHP")
                    hpText = child.GetComponent<Text>();
                else if (child.name == "CardImage")
                    cardImage = child.GetComponent<Image>();
            }

            // Update text
            if (nameText != null)
            {
                nameText.text = pokemon.data.cardName;
            }

            if (pokemon.data is PokemonCardData pkm)
            {
                if (hpText != null)
                {
                    // 現在HP/最大HP形式で表示
                    hpText.text = "HP: " + pokemon.RemainingHP + "/" + pokemon.MaxHP;
                }
            }
            else
            {
                if (hpText != null)
                    hpText.text = "";
            }

            // Set card image sprite (if cardArt is assigned)
            if (cardImage != null)
            {
                if (pokemon.data.cardArt != null)
                {
                    cardImage.sprite = pokemon.data.cardArt;
                    cardImage.color = Color.white; // Reset color to white to display sprite correctly
                }
                else if (pokemon.data is PokemonCardData pkmData)
                {
                    // Fallback: Set card image color based on type if no sprite is assigned
                    switch (pkmData.type)
                    {
                        case PokemonType.P:
                            cardImage.color = new Color(0.7f, 0.3f, 0.7f); // Psychic - Purple
                            break;
                        case PokemonType.D:
                            cardImage.color = new Color(0.3f, 0.3f, 0.3f); // Dark - Dark Gray
                            break;
                        case PokemonType.Y:
                            cardImage.color = new Color(1.0f, 0.7f, 0.8f); // Fairy - Pink
                            break;
                        case PokemonType.G:
                            cardImage.color = new Color(0.3f, 0.8f, 0.3f); // Grass - Green
                            break;
                        case PokemonType.M:
                            cardImage.color = new Color(0.7f, 0.7f, 0.8f); // Metal - Steel Gray
                            break;
                        case PokemonType.C:
                            cardImage.color = new Color(0.8f, 0.8f, 0.8f); // Colorless - Light Gray
                            break;
                        default:
                            cardImage.color = Color.white;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// スタジアム表示を更新
        /// </summary>
        private void UpdateStadiumDisplay()
        {
            if (stadiumText == null) return;

            var gm = GameManager.Instance;
            if (gm == null) return;

            if (string.IsNullOrEmpty(gm.stadiumInPlay))
            {
                stadiumText.text = "";
            }
            else
            {
                // stadiumInPlayにはcardID（例: "BeachCourt"）が入っているので、
                // 対応する日本語名を取得
                string displayName = GetStadiumDisplayName(gm.stadiumInPlay);
                stadiumText.text = displayName;
            }
        }

        /// <summary>
        /// スタジアムIDから表示名を取得
        /// </summary>
        private string GetStadiumDisplayName(string stadiumID)
        {
            switch (stadiumID)
            {
                case "Artazon": return "ボウルタウン";
                case "BeachCourt": return "ビーチコート";
                default: return stadiumID;
            }
        }

        /// <summary>
        /// Called when game starts to initialize UI
        /// </summary>
        public void InitializeUI()
        {
            UpdateUI();
        }
    }
}
