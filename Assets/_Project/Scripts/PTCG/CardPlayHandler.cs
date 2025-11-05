using UnityEngine;
using UnityEngine.UI;

namespace PTCG
{
    /// <summary>
    /// カードプレイ処理のハンドラー（カードクリック時の処理）
    /// </summary>
    public class CardPlayHandler : MonoBehaviour
    {
        public static CardPlayHandler Instance { get; private set; }

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
            Debug.Log("[CardPlayHandler] CardPlayHandler initialized (direct play mode)");
        }

        /// <summary>
        /// カードを直接プレイ（外部から呼び出し可能）
        /// </summary>
        public void PlayCard(GameObject cardUI)
        {
            if (cardUI == null)
            {
                Debug.LogWarning("[CardPlayHandler] cardUI is null");
                return;
            }

            // カードデータを取得
            CardData cardData = GetCardDataFromUI(cardUI);
            if (cardData == null)
            {
                Debug.LogError("[CardPlayHandler] CardData not found");
                return;
            }

            // 現在のプレイヤーを取得
            PlayerController currentPlayer = GameManager.Instance?.GetCurrentPlayer();
            if (currentPlayer == null)
            {
                Debug.LogError("[CardPlayHandler] CurrentPlayer not found");
                return;
            }

            // カード種別判定 → 処理分岐
            if (cardData is TrainerCardData trainer)
            {
                PlayTrainerCard(currentPlayer, trainer);
            }
            else if (cardData is EnergyCardData energy)
            {
                PlayEnergyCard(currentPlayer, energy);
            }
            else if (cardData is PokemonCardData pokemon)
            {
                PlayPokemonCard(currentPlayer, pokemon);
            }
            else
            {
                Debug.LogWarning($"[CardPlayHandler] Unknown card type: {cardData.GetType()}");
            }
        }

        /// <summary>
        /// トレーナーカードをプレイ
        /// </summary>
        private void PlayTrainerCard(PlayerController player, TrainerCardData trainer)
        {
            Debug.Log($"[CardPlayHandler] Playing Trainer: {trainer.cardName}");

            // CardPlaySystem経由で実行
            if (CardPlaySystem.Instance != null)
            {
                bool success = CardPlaySystem.Instance.PlayCard(player, trainer);
                if (success)
                {
                    // UI更新
                    UIManager.Instance?.UpdateUI();
                }
            }
        }

        /// <summary>
        /// エネルギーカードをプレイ
        /// </summary>
        private void PlayEnergyCard(PlayerController player, EnergyCardData energy)
        {
            Debug.Log($"[CardPlayHandler] Playing Energy: {energy.cardName}");

            // エネルギー貼り先選択（バトル場+ベンチ）
            var options = new System.Collections.Generic.List<SelectOption<PokemonInstance>>();

            // バトル場
            if (player.activeSlot != null)
            {
                options.Add(new SelectOption<PokemonInstance>(
                    $"バトル場: {player.activeSlot.data.cardName}",
                    player.activeSlot
                ));
            }

            // ベンチ
            for (int i = 0; i < player.benchSlots.Count; i++)
            {
                if (player.benchSlots[i] != null)
                {
                    options.Add(new SelectOption<PokemonInstance>(
                        $"ベンチ{i + 1}: {player.benchSlots[i].data.cardName}",
                        player.benchSlots[i]
                    ));
                }
            }

            if (options.Count == 0)
            {
                Debug.Log("[CardPlayHandler] エネルギーを付けるポケモンがいません");
                return;
            }

            // モーダル表示
            if (ModalSystem.Instance != null)
            {
                ModalSystem.Instance.OpenSelectModal(
                    "エネルギーを付けるポケモンを選択",
                    options,
                    (selectedPokemon) =>
                    {
                        if (selectedPokemon != null && EnergySystem.Instance != null)
                        {
                            bool success = EnergySystem.Instance.AttachEnergyFromHand(player, selectedPokemon);
                            if (success)
                            {
                                // UI更新
                                UIManager.Instance?.UpdateUI();
                            }
                        }
                    },
                    defaultFirst: true
                );
            }
            else
            {
                Debug.LogError("[CardPlayHandler] ModalSystem.Instance is null");
            }
        }

        /// <summary>
        /// ポケモンカードをプレイ
        /// </summary>
        private void PlayPokemonCard(PlayerController player, PokemonCardData pokemon)
        {
            Debug.Log($"[CardPlayHandler] Playing Pokemon: {pokemon.cardName}");

            // バトル場が空なら自動的にバトル場へ
            if (player.activeSlot == null)
            {
                player.hand.Remove(pokemon);
                GameManager.Instance.SpawnPokemonToActive(player, pokemon);
                UIManager.Instance?.UpdateUI();
                return;
            }

            // バトル場が埋まっている場合、配置先選択
            var options = new System.Collections.Generic.List<SelectOption<string>>();
            options.Add(new SelectOption<string>("バトル場", "active"));

            if (player.benchSlots.Count < 5)
            {
                options.Add(new SelectOption<string>($"ベンチ（現在{player.benchSlots.Count}/5）", "bench"));
            }

            if (options.Count == 1)
            {
                // ベンチが満員の場合
                Debug.Log("[CardPlayHandler] ベンチが満員です");
                return;
            }

            // モーダル表示
            if (ModalSystem.Instance != null)
            {
                ModalSystem.Instance.OpenSelectModal(
                    $"{pokemon.cardName}の配置先を選択",
                    options,
                    (selectedSlot) =>
                    {
                        if (string.IsNullOrEmpty(selectedSlot)) return;

                        player.hand.Remove(pokemon);

                        if (selectedSlot == "active")
                        {
                            // バトル場に配置（既存ポケモンをベンチに移動）
                            if (player.activeSlot != null && player.benchSlots.Count < 5)
                            {
                                player.benchSlots.Add(player.activeSlot);
                                Debug.Log($"[CardPlayHandler] 既存のバトルポケモンをベンチへ移動");
                            }
                            GameManager.Instance.SpawnPokemonToActive(player, pokemon);
                        }
                        else if (selectedSlot == "bench")
                        {
                            GameManager.Instance.SpawnPokemonToBench(player, pokemon);
                        }

                        UIManager.Instance?.UpdateUI();
                    },
                    defaultFirst: true
                );
            }
            else
            {
                Debug.LogError("[CardPlayHandler] ModalSystem.Instance is null");
            }
        }

        /// <summary>
        /// CardUIからCardDataを取得（HandCardLayoutManager.cs:213-242と同じロジック）
        /// </summary>
        private CardData GetCardDataFromUI(GameObject cardUI)
        {
            Text nameText = null;
            foreach (Transform child in cardUI.transform)
            {
                if (child.name == "CardName")
                {
                    nameText = child.GetComponent<Text>();
                    break;
                }
            }

            if (nameText == null) return null;
            string cardName = nameText.text;

            // GameManagerから現在のプレイヤーを取得
            if (GameManager.Instance == null || GameManager.Instance.player1 == null) return null;

            PlayerController player = GameManager.Instance.GetCurrentPlayer();
            if (player == null) return null;

            foreach (var card in player.hand)
            {
                if (card != null && card.cardName == cardName)
                {
                    return card;
                }
            }

            return null;
        }
    }
}
