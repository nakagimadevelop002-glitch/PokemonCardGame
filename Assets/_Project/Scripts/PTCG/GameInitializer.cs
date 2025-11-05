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

                // テスト用: 確認したいカードを手札に追加
                AddTestCardsToHand();

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
            TrainerCardData rareCandy = Resources.Load<TrainerCardData>("PTCG/Trainers/RareCandy");
            TrainerCardData earthenVessel = Resources.Load<TrainerCardData>("PTCG/Trainers/EarthenVessel");
            TrainerCardData superRod = Resources.Load<TrainerCardData>("PTCG/Trainers/SuperRod");

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
            AddCards(deck, nestBall, 2);
            AddCards(deck, rareCandy, 3);
            AddCards(deck, earthenVessel, 2);
            AddCards(deck, superRod, 1);

            AddCards(deck, basicPsychic, 24);
            AddCards(deck, reversalEnergy, 2);

            Debug.Log($"Gardevoir EX deck created: {deck.Count} cards");

            // デバッグ: 各カードの読み込み状態を確認
            Debug.Log($"Ralts loaded: {ralts != null}, name: {ralts?.cardName}");
            Debug.Log($"HyperBall loaded: {hyperBall != null}, name: {hyperBall?.cardName}");
            Debug.Log($"Research loaded: {research != null}, name: {research?.cardName}");
            Debug.Log($"BasicPsychic loaded: {basicPsychic != null}, name: {basicPsychic?.cardName}");

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
        /// テスト用: 確認したいカードを手札に追加
        /// </summary>
        private void AddTestCardsToHand()
        {
            PlayerController currentPlayer = GameManager.Instance.GetCurrentPlayer();
            if (currentPlayer == null)
            {
                Debug.LogWarning("AddTestCardsToHand: CurrentPlayer is null");
                return;
            }

            // 博士の研究
            TrainerCardData research = Resources.Load<TrainerCardData>("PTCG/Trainers/Research");
            if (research != null)
            {
                currentPlayer.hand.Add(research);
                Debug.Log($"Added to hand: {research.cardName}");
            }

            // ハイパーボール
            TrainerCardData hyperBall = Resources.Load<TrainerCardData>("PTCG/Trainers/HyperBall");
            if (hyperBall != null)
            {
                currentPlayer.hand.Add(hyperBall);
                Debug.Log($"Added to hand: {hyperBall.cardName}");
            }

            // ネストボール
            TrainerCardData nestBall = Resources.Load<TrainerCardData>("PTCG/Trainers/NestBall");
            if (nestBall != null)
            {
                currentPlayer.hand.Add(nestBall);
                Debug.Log($"Added to hand: {nestBall.cardName}");
            }

            Debug.Log($"Test cards added. Total hand: {currentPlayer.hand.Count} cards");
        }
    }
}
