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

        // Active card UI instances
        private List<GameObject> playerHandUICards = new List<GameObject>();
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
                    Debug.LogWarning("CardUI prefab not found in Resources/Prefabs/");
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

            // Current player is displayed at bottom, opponent at top
            if (currentPlayer == player1)
            {
                UpdatePlayerUI(player1, isBottom: true);
                UpdatePlayerUI(player2, isBottom: false);
            }
            else
            {
                UpdatePlayerUI(player2, isBottom: true);
                UpdatePlayerUI(player1, isBottom: false);
            }

            // Update stadium display
            UpdateStadiumDisplay();

            // フィールドカードのボタンを再セットアップ（UIが再生成されるため）
            GameInitializer.Instance?.SetupFieldCardButtons();

            // EndTurnButtonの有効/無効制御（AIターン中は無効化）
            if (endTurnButton != null)
            {
                endTurnButton.interactable = !currentPlayer.isAI;
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

                if (opponentHandCount != null)
                    opponentHandCount.text = player.hand.Count.ToString();

                if (opponentDiscardCount != null)
                    opponentDiscardCount.text = player.discard.Count.ToString();

                // Update active pokemon
                UpdateActivePokemonDisplay(player, false);

                // Update bench display
                UpdateBenchDisplay(player, false);
            }
        }

        private void UpdateHandDisplay(PlayerController player)
        {
            // Debug.Log($"UpdateHandDisplay called: player={player?.playerName}, handCount={player?.hand.Count}, playerHandZone={playerHandZone != null}, cardUIPrefab={cardUIPrefab != null}");
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

            // Update text（文字化け防止: クリア後に設定）
            if (nameText != null)
            {
                nameText.text = "";
                nameText.text = cardData.cardName;
            }

            if (cardData is PokemonCardData pkm)
            {
                if (hpText != null)
                {
                    hpText.text = "";
                    hpText.text = $"HP: {pkm.baseHP}";
                }
            }
            else
            {
                if (hpText != null)
                    hpText.text = "";
            }

            // Set card image color based on type
            if (cardImage != null && cardData is PokemonCardData pkmData)
            {
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

            // Update text（文字化け防止: クリア後に設定）
            if (nameText != null)
            {
                nameText.text = "";
                nameText.text = pokemon.data.cardName;
            }

            if (pokemon.data is PokemonCardData pkm)
            {
                if (hpText != null)
                {
                    hpText.text = "";
                    // 現在HP/最大HP形式で表示
                    hpText.text = $"HP: {pokemon.RemainingHP}/{pokemon.MaxHP}";
                }
            }
            else
            {
                if (hpText != null)
                    hpText.text = "";
            }

            // Set card image color based on type
            if (cardImage != null && pokemon.data is PokemonCardData pkmData)
            {
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
                // 対応する日本語名を取得（文字化け防止: クリア後に設定）
                string displayName = GetStadiumDisplayName(gm.stadiumInPlay);
                stadiumText.text = "";
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
            // Debug.Log("UI initialized");
        }
    }
}
