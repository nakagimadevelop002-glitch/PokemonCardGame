using UnityEngine;
using System.Collections.Generic;

namespace PTCG
{
    /// <summary>
    /// ゲーム初期化スクリプト（シーン起動時に自動実行）
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        public static GameInitializer Instance { get; private set; }

        public bool autoStartGame = true;
        [Header("Test Mode (0=通常, 1=山札切れ, 2=サイド0, 3=場が空, 4=ベンチ選択, 5=UI表示, 6=MewEXリスタート, 7=アドレナブレイン)")]
        public int testMode = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // GameManagerの作成
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }

            // PlayerControllersの作成
            if (GameManager.Instance.player1 == null)
            {
                GameObject p1Obj = new GameObject("Player1");
                GameManager.Instance.player1 = p1Obj.AddComponent<PlayerController>();
            }

            if (GameManager.Instance.player2 == null)
            {
                GameObject p2Obj = new GameObject("Player2");
                GameManager.Instance.player2 = p2Obj.AddComponent<PlayerController>();
                GameManager.Instance.player2.isAI = true; // Player2はAI
            }

            // 各種システムの作成
            if (BattleSystem.Instance == null)
            {
                GameObject battleObj = new GameObject("BattleSystem");
                battleObj.AddComponent<BattleSystem>();
            }

            if (EnergySystem.Instance == null)
            {
                GameObject energyObj = new GameObject("EnergySystem");
                energyObj.AddComponent<EnergySystem>();
            }

            if (EvolutionSystem.Instance == null)
            {
                GameObject evolutionObj = new GameObject("EvolutionSystem");
                evolutionObj.AddComponent<EvolutionSystem>();
            }

            if (AbilitySystem.Instance == null)
            {
                GameObject abilityObj = new GameObject("AbilitySystem");
                abilityObj.AddComponent<AbilitySystem>();
            }

            if (RetreatSystem.Instance == null)
            {
                GameObject retreatObj = new GameObject("RetreatSystem");
                retreatObj.AddComponent<RetreatSystem>();
            }

            if (ModalSystem.Instance == null)
            {
                GameObject modalObj = new GameObject("ModalSystem");
                modalObj.AddComponent<ModalSystem>();
            }

            if (CardPlaySystem.Instance == null)
            {
                GameObject cardPlayObj = new GameObject("CardPlaySystem");
                cardPlayObj.AddComponent<CardPlaySystem>();
            }

            if (CardPlayHandler.Instance == null)
            {
                GameObject cardPlayHandlerObj = new GameObject("CardPlayHandler");
                cardPlayHandlerObj.AddComponent<CardPlayHandler>();
            }

            if (CardSelectionHandler.Instance == null)
            {
                GameObject cardSelectionObj = new GameObject("CardSelectionHandler");
                cardSelectionObj.AddComponent<CardSelectionHandler>();
            }

            if (HandCardLayoutManager.Instance == null)
            {
                GameObject handLayoutObj = new GameObject("HandCardLayoutManager");
                handLayoutObj.AddComponent<HandCardLayoutManager>();
            }

            if (CardDetailPanel.Instance == null)
            {
                GameObject cardDetailObj = new GameObject("CardDetailPanel");
                cardDetailObj.AddComponent<CardDetailPanel>();
            }

            if (AIController.Instance == null)
            {
                GameObject aiObj = new GameObject("AIController");
                aiObj.AddComponent<AIController>();
            }

            // UIManagerの初期化
            if (UIManager.Instance != null)
            {
                UIManager.Instance.InitializeUI();
            }

            if (autoStartGame)
            {
                // テストデッキの作成
                List<CardData> deck1 = CreateTestDeck();
                List<CardData> deck2 = CreateTestDeck();

                // ゲーム開始
                GameManager.Instance.StartGame(deck1, deck2);

                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateUI();
                }

                // テスト環境構築
                SetupTestMode();
            }
        }

        private List<CardData> CreateTestDeck()
        {
            List<CardData> deck = new List<CardData>();

            // Pokemon
            PokemonCardData ralts = Resources.Load<PokemonCardData>("PTCG/Pokemon/Ralts");
            PokemonCardData kirlia = Resources.Load<PokemonCardData>("PTCG/Pokemon/Kirlia");
            PokemonCardData gardevoirEX = Resources.Load<PokemonCardData>("PTCG/Pokemon/GardevoirEX");
            PokemonCardData drifloon = Resources.Load<PokemonCardData>("PTCG/Pokemon/Drifloon");
            PokemonCardData mewEX = Resources.Load<PokemonCardData>("PTCG/Pokemon/MewEX");

            // Trainers
            TrainerCardData hyperBall = Resources.Load<TrainerCardData>("PTCG/Trainers/HyperBall");
            TrainerCardData research = Resources.Load<TrainerCardData>("PTCG/Trainers/Research");
            TrainerCardData boss = Resources.Load<TrainerCardData>("PTCG/Trainers/Boss");
            TrainerCardData iono = Resources.Load<TrainerCardData>("PTCG/Trainers/Iono");
            TrainerCardData pepper = Resources.Load<TrainerCardData>("PTCG/Trainers/Pepper");
            TrainerCardData nestBall = Resources.Load<TrainerCardData>("PTCG/Trainers/NestBall");
            TrainerCardData levelBall = Resources.Load<TrainerCardData>("PTCG/Trainers/LevelBall");
            TrainerCardData rareCandy = Resources.Load<TrainerCardData>("PTCG/Trainers/RareCandy");
            TrainerCardData earthenVessel = Resources.Load<TrainerCardData>("PTCG/Trainers/EarthenVessel");
            TrainerCardData superRod = Resources.Load<TrainerCardData>("PTCG/Trainers/SuperRod");
            TrainerCardData artazon = Resources.Load<TrainerCardData>("PTCG/Trainers/Artazon");
            TrainerCardData escapeRope = Resources.Load<TrainerCardData>("PTCG/Trainers/EscapeRope");
            TrainerCardData counterCatcher = Resources.Load<TrainerCardData>("PTCG/Trainers/CounterCatcher");
            TrainerCardData lostSweeper = Resources.Load<TrainerCardData>("PTCG/Trainers/LostSweeper");
            TrainerCardData beachCourt = Resources.Load<TrainerCardData>("PTCG/Trainers/BeachCourt");

            // Energies
            EnergyCardData basicPsychic = Resources.Load<EnergyCardData>("PTCG/Energies/BasicPsychic");
            EnergyCardData reversalEnergy = Resources.Load<EnergyCardData>("PTCG/Energies/ReversalEnergy");

            // Gardevoir EX Deck構成（60枚）
            AddCards(deck, ralts, 4);
            AddCards(deck, kirlia, 2);
            AddCards(deck, gardevoirEX, 3);
            AddCards(deck, drifloon, 2);
            AddCards(deck, mewEX, 1);

            AddCards(deck, hyperBall, 4);
            AddCards(deck, research, 4);
            AddCards(deck, boss, 2);
            AddCards(deck, iono, 2);
            AddCards(deck, pepper, 2);
            AddCards(deck, nestBall, 1);
            AddCards(deck, levelBall, 2);
            AddCards(deck, rareCandy, 3);
            AddCards(deck, earthenVessel, 2);
            AddCards(deck, superRod, 1);
            // テスト用トレーナー追加（後で削除予定）
            AddCards(deck, artazon, 1);
            AddCards(deck, beachCourt, 1);
            AddCards(deck, escapeRope, 1);
            AddCards(deck, counterCatcher, 1);
            AddCards(deck, lostSweeper, 1);

            AddCards(deck, basicPsychic, 18); // テスト用調整: 24 → 18（テスト用トレーナー5枚追加により）
            AddCards(deck, reversalEnergy, 2);

            // Debug.Log($"Gardevoir EX deck created: {deck.Count} cards");

            // デバッグ: 各カードの読み込み状態を確認
            // Debug.Log($"Ralts loaded: {ralts != null}, name: {ralts?.cardName}");
            // Debug.Log($"HyperBall loaded: {hyperBall != null}, name: {hyperBall?.cardName}");
            // Debug.Log($"Research loaded: {research != null}, name: {research?.cardName}");
            // Debug.Log($"BasicPsychic loaded: {basicPsychic != null}, name: {basicPsychic?.cardName}");

            return deck;
        }

        private void AddCards(List<CardData> deck, CardData card, int count)
        {
            if (card != null)
            {
                for (int i = 0; i < count; i++)
                {
                    deck.Add(card);
                }
            }
            else
            {
                Debug.LogWarning($"Card is null, skipping {count} cards");
            }
        }

        /// <summary>
        /// テストモードに応じた初期状態を設定
        /// </summary>
        private void SetupTestMode()
        {
            var gm = GameManager.Instance;
            var player1 = gm.player1;
            var player2 = gm.player2;

            switch (testMode)
            {
                case 0:
                    break;

                case 1:
                    while (player1.deck.Count > 2)
                    {
                        player1.deck.RemoveAt(player1.deck.Count - 1);
                    }
                    break;

                case 2:
                    while (player2.prizes.Count > 1)
                    {
                        player2.deck.Add(player2.prizes[player2.prizes.Count - 1]);
                        player2.prizes.RemoveAt(player2.prizes.Count - 1);
                    }
                    if (player2.activeSlot != null)
                    {
                        player2.activeSlot.currentDamage = player2.activeSlot.data.baseHP - 10;
                    }
                    // player1にエネルギーを付けて攻撃可能に
                    if (player1.activeSlot != null)
                    {
                        EnergyCardData basicPsychic = Resources.Load<EnergyCardData>("PTCG/Energies/BasicPsychic");
                        if (basicPsychic != null)
                        {
                            player1.activeSlot.attachedEnergies.Add(basicPsychic);
                            player1.activeSlot.attachedEnergies.Add(basicPsychic);
                        }
                    }
                    break;

                case 3:
                    player2.benchSlots.Clear();
                    if (player2.activeSlot != null)
                    {
                        player2.activeSlot.currentDamage = player2.activeSlot.data.baseHP - 10;
                    }
                    // player1にエネルギーを付けて攻撃可能に
                    if (player1.activeSlot != null)
                    {
                        EnergyCardData basicPsychic = Resources.Load<EnergyCardData>("PTCG/Energies/BasicPsychic");
                        if (basicPsychic != null)
                        {
                            player1.activeSlot.attachedEnergies.Add(basicPsychic);
                            player1.activeSlot.attachedEnergies.Add(basicPsychic);
                        }
                    }
                    break;

                case 4:
                    if (player1.activeSlot != null)
                    {
                        player1.activeSlot.currentDamage = player1.activeSlot.data.baseHP - 10;
                    }
                    // player2にエネルギーを付けて攻撃可能に
                    if (player2.activeSlot != null)
                    {
                        EnergyCardData basicPsychic = Resources.Load<EnergyCardData>("PTCG/Energies/BasicPsychic");
                        if (basicPsychic != null)
                        {
                            player2.activeSlot.attachedEnergies.Add(basicPsychic);
                            player2.activeSlot.attachedEnergies.Add(basicPsychic);
                        }
                    }
                    break;

                case 5:
                    break;

                case 6:
                    // player1のバトル場にMewEXを配置
                    PokemonCardData mewEX = Resources.Load<PokemonCardData>("PTCG/Pokemon/MewEX");
                    if (mewEX != null)
                    {
                        // 既存のバトル場ポケモンを削除
                        if (player1.activeSlot != null)
                        {
                            Destroy(player1.activeSlot.gameObject);
                        }

                        // MewEXを配置
                        GameObject mewObj = new GameObject("MewEX");
                        PokemonInstance mewInstance = mewObj.AddComponent<PokemonInstance>();
                        mewInstance.Initialize(mewEX, player1.playerIndex);
                        player1.activeSlot = mewInstance;
                    }

                    // player1の手札を2枚に減らす（リスタート発動条件）
                    while (player1.hand.Count > 2)
                    {
                        player1.hand.RemoveAt(player1.hand.Count - 1);
                    }
                    break;

                case 7:
                    Debug.Log("[TEST MODE 7] ====== アドレナブレインテスト環境構築開始 ======");

                    // player1のベンチにMashimashira配置
                    PokemonCardData mashimashira = Resources.Load<PokemonCardData>("PTCG/Pokemon/Mashimashira");
                    if (mashimashira != null)
                    {
                        GameObject mashiObj = new GameObject("Mashimashira");
                        PokemonInstance mashiInstance = mashiObj.AddComponent<PokemonInstance>();
                        mashiInstance.Initialize(mashimashira, player1.playerIndex);
                        player1.benchSlots.Add(mashiInstance);

                        Debug.Log("[TEST MODE 7] ✅ Player1ベンチにMashimashira配置");

                        // 悪エネルギー付与（発動条件）
                        EnergyCardData darkEnergy = Resources.Load<EnergyCardData>("PTCG/Energies/BasicDarkness");
                        if (darkEnergy != null)
                        {
                            mashiInstance.attachedEnergies.Add(darkEnergy);
                            Debug.Log("[TEST MODE 7] ✅ Mashimashiraに悪エネルギー付与");
                        }
                    }

                    // player1のバトル場とベンチにダメージ付与（移動元）
                    if (player1.activeSlot != null)
                    {
                        player1.activeSlot.currentDamage = 30; // ダメカン3個
                        Debug.Log($"[TEST MODE 7] ✅ Player1バトル場({player1.activeSlot.data.cardName})にダメカン3個");
                    }

                    // ベンチの最初の2体にもダメージ
                    for (int i = 0; i < Mathf.Min(2, player1.benchSlots.Count); i++)
                    {
                        player1.benchSlots[i].currentDamage = 20; // ダメカン2個
                        Debug.Log($"[TEST MODE 7] ✅ Player1ベンチ{i + 1}({player1.benchSlots[i].data.cardName})にダメカン2個");
                    }

                    Debug.Log("[TEST MODE 7] ====== テスト環境構築完了 ======");
                    Debug.Log("[TEST MODE 7] 使用方法: Player1のMashimashiraをクリック → 「特性: アドレナブレイン」を選択");
                    break;

                default:
                    break;
            }

            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            // バトル場とベンチのカードUIにButtonを追加（1秒待機）
            Invoke("SetupFieldCardButtons", 1.0f);
        }

        /// <summary>
        /// バトル場とベンチのカードUIにButton/EventTriggerを追加（テスト用）
        /// UIManager.UpdateUI()後に再呼び出し可能
        /// </summary>
        public void SetupFieldCardButtons()
        {
            int buttonCount = 0;

            Debug.Log("[SetupFieldCardButtons] 開始");

            // PlayerActiveZoneのCardUIを取得
            GameObject playerActiveZone = GameObject.Find("PlayerActive");
            if (playerActiveZone != null)
            {
                Debug.Log("[SetupFieldCardButtons] PlayerActive found");
                foreach (Transform child in playerActiveZone.transform)
                {
                    if (child.name.Contains("CardUI"))
                    {
                        Debug.Log("[SetupFieldCardButtons] PlayerActive CardUI: " + child.name);
                        AddFieldCardButton(child.gameObject);
                        buttonCount++;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[SetupFieldCardButtons] PlayerActive not found");
            }

            // PlayerBenchのCardUIを取得
            GameObject playerBenchZone = GameObject.Find("PlayerBench");
            if (playerBenchZone != null)
            {
                Debug.Log("[SetupFieldCardButtons] PlayerBench found, children count: " + playerBenchZone.transform.childCount);
                foreach (Transform child in playerBenchZone.transform)
                {
                    Debug.Log("[SetupFieldCardButtons] PlayerBench child: " + child.name);
                    if (child.name.Contains("CardUI"))
                    {
                        Debug.Log("[SetupFieldCardButtons] PlayerBench CardUI: " + child.name);
                        AddFieldCardButton(child.gameObject);
                        buttonCount++;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[SetupFieldCardButtons] PlayerBench not found");
            }

            Debug.Log("[SetupFieldCardButtons] 完了: " + buttonCount + "枚のカードにButton追加");
        }

        /// <summary>
        /// カードUIにButton/EventTriggerを追加
        /// </summary>
        private void AddFieldCardButton(GameObject cardUI)
        {
            // Buttonコンポーネント追加
            UnityEngine.UI.Button btn = cardUI.GetComponent<UnityEngine.UI.Button>();
            if (btn == null)
            {
                btn = cardUI.AddComponent<UnityEngine.UI.Button>();
                btn.transition = UnityEngine.UI.Selectable.Transition.None;
                btn.targetGraphic = null;

                // クリックイベント
                btn.onClick.AddListener(() => OnFieldCardClicked(cardUI));
            }

            // Outlineコンポーネント追加
            UnityEngine.UI.Outline outline = cardUI.GetComponent<UnityEngine.UI.Outline>();
            if (outline == null)
            {
                outline = cardUI.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = UnityEngine.Color.black;
                outline.effectDistance = new UnityEngine.Vector2(2, 2);
                outline.enabled = true;
            }

            // EventTrigger追加（ホバー強調表示）
            UnityEngine.EventSystems.EventTrigger trigger = cardUI.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = cardUI.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                // PointerEnter（マウスホバー開始）
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((data) => {
                    var outlineComp = cardUI.GetComponent<UnityEngine.UI.Outline>();
                    if (outlineComp != null)
                    {
                        outlineComp.effectColor = UnityEngine.Color.yellow; // 黄色で強調
                    }
                });
                trigger.triggers.Add(pointerEnter);

                // PointerExit（マウスホバー終了）
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => {
                    var outlineComp = cardUI.GetComponent<UnityEngine.UI.Outline>();
                    if (outlineComp != null)
                    {
                        outlineComp.effectColor = UnityEngine.Color.black; // 元の黒に戻す
                    }
                });
                trigger.triggers.Add(pointerExit);
            }
        }

        /// <summary>
        /// バトル場/ベンチカードクリック処理（攻撃・にげる・特性統合）
        /// </summary>
        private void OnFieldCardClicked(GameObject cardUI)
        {
            // カード名からPokemonInstanceを検索
            string cardName = null;
            UnityEngine.UI.Text[] texts = cardUI.GetComponentsInChildren<UnityEngine.UI.Text>();
            foreach (var txt in texts)
            {
                if (txt.name == "CardName")
                {
                    cardName = txt.text;
                    break;
                }
            }

            if (string.IsNullOrEmpty(cardName))
            {
                Debug.LogWarning("Card name not found");
                return;
            }

            // PokemonInstanceを検索
            PokemonInstance[] allPokemons = FindObjectsByType<PokemonInstance>(FindObjectsSortMode.None);
            PokemonInstance targetPokemon = null;
            foreach (var pokemon in allPokemons)
            {
                if (pokemon.data.cardName == cardName)
                {
                    targetPokemon = pokemon;
                    break;
                }
            }

            if (targetPokemon == null)
            {
                Debug.LogWarning($"Pokemon not found: {cardName}");
                return;
            }

            var gm = GameManager.Instance;
            var currentPlayer = gm.GetCurrentPlayer();

            // 選択肢リスト作成
            var options = new List<SelectOption<string>>();

            // バトル場のポケモンの場合
            if (currentPlayer.activeSlot == targetPokemon)
            {
                // 攻撃（常に表示、不可能な場合はdisabled）
                bool canAttack = BattleSystem.Instance.CanAttack(currentPlayer);
                string attackDisabledReason = canAttack ? "" : BattleSystem.Instance.GetAttackDisabledReason(currentPlayer);
                options.Add(new SelectOption<string>("攻撃する", "attack", !canAttack, attackDisabledReason));

                // にげる（常に表示、不可能な場合はdisabled）
                bool canRetreat = RetreatSystem.Instance.CanRetreat(currentPlayer);
                string retreatDisabledReason = canRetreat ? "" : RetreatSystem.Instance.GetRetreatDisabledReason(currentPlayer);
                options.Add(new SelectOption<string>("にげる", "retreat", !canRetreat, retreatDisabledReason));
            }

            // 特性があるかチェック（バトル場・ベンチ共通）
            if (targetPokemon.data.abilities != null && targetPokemon.data.abilities.Count > 0)
            {
                foreach (var ability in targetPokemon.data.abilities)
                {
                    options.Add(new SelectOption<string>($"特性: {ability.abilityName}", $"ability:{ability.abilityID}"));
                }
            }

            // 選択肢がない場合は何もしない
            if (options.Count == 0)
            {
                return;
            }

            // モーダル表示
            ModalSystem.Instance.OpenSelectModal(
                $"《{cardName}》の操作を選択",
                options,
                (selectedAction) =>
                {
                    if (selectedAction == null) return;

                    if (selectedAction == "attack")
                    {
                        var opponent = gm.GetOpponentPlayer();

                        // 相手のバトル場チェック
                        if (opponent.activeSlot == null)
                        {
                            Debug.LogWarning("攻撃失敗: 相手のバトル場にポケモンがいません");
                            return;
                        }

                        BattleSystem.Instance.PerformAttack(currentPlayer, opponent);
                        UIManager.Instance?.UpdateUI();
                    }
                    else if (selectedAction == "retreat")
                    {
                        // にげる処理
                        var benchOptions = new System.Collections.Generic.List<SelectOption<int>>();
                        for (int i = 0; i < currentPlayer.benchSlots.Count; i++)
                        {
                            var bench = currentPlayer.benchSlots[i];
                            benchOptions.Add(new SelectOption<int>(
                                $"ベンチ{i + 1}: {bench.data.cardName}",
                                i
                            ));
                        }

                        if (benchOptions.Count == 0)
                        {
                            return;
                        }

                        ModalSystem.Instance.OpenSelectModal(
                            "入れ替え先のポケモンを選択",
                            benchOptions,
                            (selectedIndex) =>
                            {
                                if (selectedIndex < 0) return;

                                bool success = RetreatSystem.Instance.Retreat(currentPlayer, selectedIndex);
                                if (success)
                                {
                                    UIManager.Instance?.UpdateUI();
                                }
                            },
                            defaultFirst: false
                        );
                    }
                    else if (selectedAction.StartsWith("ability:"))
                    {
                        string abilityID = selectedAction.Substring(8);
                        AbilitySystem.Instance.UseAbility(currentPlayer, targetPokemon, abilityID);
                        UIManager.Instance?.UpdateUI();
                    }
                },
                defaultFirst: false
            );
        }

        /// <summary>
        /// 新しいゲームを開始（リスタート用）
        /// </summary>
        public void StartNewGame()
        {
            // テストデッキの作成
            List<CardData> deck1 = CreateTestDeck();
            List<CardData> deck2 = CreateTestDeck();

            // ゲーム開始
            GameManager.Instance.StartGame(deck1, deck2);

            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateUI();
            }

            // テスト環境構築
            SetupTestMode();
        }

    }
}
