using UnityEngine;
using System.Collections.Generic;

namespace PTCG
{
    /// <summary>
    /// ゲーム初期化スクリプト（シーン起動時に自動実行）
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        public bool autoStartGame = true;

        private void Start()
        {
            Debug.Log("GameInitializer: Starting initialization");

            // GameManagerの作成
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
                Debug.Log("GameManager created");
            }

            // PlayerControllersの作成
            if (GameManager.Instance.player1 == null)
            {
                GameObject p1Obj = new GameObject("Player1");
                GameManager.Instance.player1 = p1Obj.AddComponent<PlayerController>();
                Debug.Log("Player1 created");
            }

            if (GameManager.Instance.player2 == null)
            {
                GameObject p2Obj = new GameObject("Player2");
                GameManager.Instance.player2 = p2Obj.AddComponent<PlayerController>();
                Debug.Log("Player2 created");
            }

            // UIManagerの初期化
            if (UIManager.Instance != null)
            {
                UIManager.Instance.InitializeUI();
                Debug.Log("UIManager initialized");
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
            }
        }

        private List<CardData> CreateTestDeck()
        {
            List<CardData> deck = new List<CardData>();

            // Resources/PTCG/Pokemon/からRaltsを読み込む
            PokemonCardData ralts = Resources.Load<PokemonCardData>("PTCG/Pokemon/Ralts");

            if (ralts != null)
            {
                // テストデッキ：Raltsを15枚追加（基本ポケモンが必ず引けるように）
                for (int i = 0; i < 15; i++)
                {
                    deck.Add(ralts);
                }

                // 残り45枚もRaltsで埋める（本来は他のカードを混ぜる）
                for (int i = 0; i < 45; i++)
                {
                    deck.Add(ralts);
                }

                Debug.Log($"Test deck created with {deck.Count} cards (Ralts)");
            }
            else
            {
                Debug.LogError("Failed to load test card data from Resources/PTCG/Pokemon/Ralts");
                // 空のデッキを返す
                for (int i = 0; i < 60; i++)
                {
                    deck.Add(null);
                }
            }

            return deck;
        }
    }
}
