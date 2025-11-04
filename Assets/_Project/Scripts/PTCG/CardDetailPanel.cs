using UnityEngine;
using UnityEngine.UI;

namespace PTCG
{
    /// <summary>
    /// カード詳細表示パネル（ホバー時）
    /// カード名、HP、タイプ、攻撃、特性、効果テキストを表示
    /// </summary>
    public class CardDetailPanel : MonoBehaviour
    {
        public static CardDetailPanel Instance { get; private set; }

        [Header("UI References")]
        public GameObject panelRoot;
        public Text cardNameText;
        public Text cardTypeText;
        public Text cardHPText;
        public Text cardDescriptionText;
        public Image cardTypeIcon;

        [Header("Panel Settings")]
        public Vector2 panelOffset = new Vector2(300f, 0f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 初期状態は非表示
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        /// <summary>
        /// カード詳細を表示
        /// </summary>
        public void ShowCardDetail(CardData cardData)
        {
            if (cardData == null || panelRoot == null) return;

            // パネル表示
            panelRoot.SetActive(true);

            // カード名
            if (cardNameText != null)
            {
                cardNameText.text = cardData.cardName;
            }

            // ポケモンカードの場合
            if (cardData is PokemonCardData pkm)
            {
                // タイプ
                if (cardTypeText != null)
                {
                    cardTypeText.text = $"タイプ: {GetTypeName(pkm.type)}";
                }

                // HP
                if (cardHPText != null)
                {
                    cardHPText.text = $"HP: {pkm.baseHP}";
                }

                // 説明（攻撃・特性）
                if (cardDescriptionText != null)
                {
                    string description = "";

                    // 特性
                    if (pkm.abilities != null && pkm.abilities.Count > 0)
                    {
                        description += "【特性】\n";
                        foreach (var ability in pkm.abilities)
                        {
                            description += $"{ability.abilityName}\n";
                        }
                        description += "\n";
                    }

                    // 攻撃
                    if (pkm.attacks != null && pkm.attacks.Count > 0)
                    {
                        description += "【ワザ】\n";
                        foreach (var attack in pkm.attacks)
                        {
                            string energyCost = "";
                            if (attack.specificRequirements != null && attack.specificRequirements.Count > 0)
                            {
                                foreach (var req in attack.specificRequirements)
                                {
                                    for (int i = 0; i < req.count; i++)
                                    {
                                        energyCost += GetTypeSymbol(req.type) + " ";
                                    }
                                }
                            }
                            description += $"{energyCost}{attack.attackName} {attack.baseDamage}\n";
                        }
                    }

                    cardDescriptionText.text = description;
                }

                // タイプアイコン色
                if (cardTypeIcon != null)
                {
                    cardTypeIcon.color = GetTypeColor(pkm.type);
                }
            }
            // トレーナーカードの場合
            else if (cardData is TrainerCardData trainer)
            {
                if (cardTypeText != null)
                {
                    cardTypeText.text = "トレーナー";
                }

                if (cardHPText != null)
                {
                    cardHPText.text = "";
                }

                if (cardDescriptionText != null)
                {
                    cardDescriptionText.text = "トレーナーカードの効果";
                }

                if (cardTypeIcon != null)
                {
                    cardTypeIcon.color = Color.gray;
                }
            }
            // エネルギーカードの場合
            else if (cardData is EnergyCardData energy)
            {
                if (cardTypeText != null)
                {
                    cardTypeText.text = "エネルギー";
                }

                if (cardHPText != null)
                {
                    cardHPText.text = "";
                }

                if (cardDescriptionText != null)
                {
                    cardDescriptionText.text = $"{energy.cardName}を提供します。";
                }

                if (cardTypeIcon != null)
                {
                    cardTypeIcon.color = Color.yellow;
                }
            }

            Debug.Log($"Showing card detail: {cardData.cardName}");
        }

        /// <summary>
        /// カード詳細を非表示
        /// </summary>
        public void HideCardDetail()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Debug.Log("Hiding card detail");
        }

        private string GetTypeName(PokemonType type)
        {
            switch (type)
            {
                case PokemonType.P: return "超";
                case PokemonType.D: return "悪";
                case PokemonType.Y: return "フェアリー";
                case PokemonType.G: return "草";
                case PokemonType.M: return "鋼";
                case PokemonType.C: return "無色";
                default: return "不明";
            }
        }

        private string GetTypeSymbol(PokemonType type)
        {
            switch (type)
            {
                case PokemonType.P: return "[超]";
                case PokemonType.D: return "[悪]";
                case PokemonType.Y: return "[妖]";
                case PokemonType.G: return "[草]";
                case PokemonType.M: return "[鋼]";
                case PokemonType.C: return "[無]";
                default: return "[?]";
            }
        }

        private Color GetTypeColor(PokemonType type)
        {
            switch (type)
            {
                case PokemonType.P: return new Color(0.7f, 0.3f, 0.7f); // 紫
                case PokemonType.D: return new Color(0.3f, 0.3f, 0.3f); // 黒
                case PokemonType.Y: return new Color(1.0f, 0.7f, 0.8f); // ピンク
                case PokemonType.G: return new Color(0.3f, 0.8f, 0.3f); // 緑
                case PokemonType.M: return new Color(0.7f, 0.7f, 0.8f); // 灰色
                case PokemonType.C: return new Color(0.8f, 0.8f, 0.8f); // 薄灰色
                default: return Color.white;
            }
        }
    }
}
