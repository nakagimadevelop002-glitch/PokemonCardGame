using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace PTCG
{
    /// <summary>
    /// 遊戯王マスターデュエル風の手札レイアウト管理
    /// 扇状配置 + ホバー時拡大表示
    /// </summary>
    public class HandCardLayoutManager : MonoBehaviour
    {
        public static HandCardLayoutManager Instance { get; private set; }

        [Header("Fan Layout Settings")]
        [Tooltip("扇の円弧半径")]
        public float fanRadius = 800f;

        [Tooltip("扇の開始角度（左端）")]
        public float startAngle = -25f;

        [Tooltip("扇の終了角度（右端）")]
        public float endAngle = 25f;

        [Tooltip("カードのY座標オフセット")]
        public float yOffset = -800f;

        [Header("Hover Settings")]
        [Tooltip("ホバー時のY移動量")]
        public float hoverYOffset = 120f;

        [Tooltip("ホバー時のスケール")]
        public float hoverScale = 1.4f;

        [Tooltip("アニメーション速度")]
        public float animationSpeed = 8f;

        private GameObject hoveredCard = null;
        private Dictionary<GameObject, CardLayoutData> cardLayoutData = new Dictionary<GameObject, CardLayoutData>();

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
        /// 手札カードを扇状に配置
        /// </summary>
        public void ArrangeHandCards(List<GameObject> handCards)
        {
            if (handCards == null || handCards.Count == 0) return;

            cardLayoutData.Clear();
            int cardCount = handCards.Count;

            // 角度の計算
            float totalAngle = endAngle - startAngle;
            float angleStep = cardCount > 1 ? totalAngle / (cardCount - 1) : 0;

            for (int i = 0; i < cardCount; i++)
            {
                GameObject card = handCards[i];
                if (card == null) continue;

                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt == null) continue;

                // 扇状の位置計算
                float angle = startAngle + (angleStep * i);
                float angleRad = angle * Mathf.Deg2Rad;

                float x = Mathf.Sin(angleRad) * fanRadius;
                float y = Mathf.Cos(angleRad) * fanRadius + yOffset;

                // レイアウトデータ保存
                CardLayoutData data = new CardLayoutData
                {
                    defaultPosition = new Vector2(x, y),
                    defaultRotation = Quaternion.Euler(0, 0, -angle),
                    defaultScale = Vector3.one,
                    sortOrder = i
                };
                cardLayoutData[card] = data;

                // 位置設定
                rt.anchoredPosition = data.defaultPosition;
                rt.localRotation = data.defaultRotation;
                rt.localScale = data.defaultScale;
                rt.SetSiblingIndex(i); // 描画順

                // イベント登録
                SetupCardEvents(card);
            }
        }

        private void SetupCardEvents(GameObject card)
        {
            EventTrigger trigger = card.GetComponent<EventTrigger>();
            if (trigger == null) return;

            // 既存のイベントをクリアせず、追加のみ行う

            // PointerEnter
            EventTrigger.Entry enterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            enterEntry.callback.AddListener((data) => { OnCardHoverEnter(card); });
            trigger.triggers.Add(enterEntry);

            // PointerExit
            EventTrigger.Entry exitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener((data) => { OnCardHoverExit(card); });
            trigger.triggers.Add(exitEntry);
        }

        private void OnCardHoverEnter(GameObject card)
        {
            if (hoveredCard == card) return;

            hoveredCard = card;
            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt == null || !cardLayoutData.ContainsKey(card)) return;

            // 最前面に移動
            rt.SetAsLastSibling();

            // カード詳細表示通知
            if (CardDetailPanel.Instance != null)
            {
                CardData cardData = GetCardDataFromUI(card);
                if (cardData != null)
                {
                    CardDetailPanel.Instance.ShowCardDetail(cardData);
                }
            }

            Debug.Log($"Card hover enter: {card.name}");
        }

        private void OnCardHoverExit(GameObject card)
        {
            if (hoveredCard == card)
            {
                hoveredCard = null;

                // カード詳細非表示
                if (CardDetailPanel.Instance != null)
                {
                    CardDetailPanel.Instance.HideCardDetail();
                }

                // 元のソート順に戻す
                if (cardLayoutData.ContainsKey(card))
                {
                    RectTransform rt = card.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.SetSiblingIndex(cardLayoutData[card].sortOrder);
                    }
                }

                Debug.Log($"Card hover exit: {card.name}");
            }
        }

        private void Update()
        {
            // ホバー中のカードをアニメーション
            foreach (var kvp in cardLayoutData)
            {
                GameObject card = kvp.Key;
                if (card == null) continue;

                CardLayoutData data = kvp.Value;
                RectTransform rt = card.GetComponent<RectTransform>();
                if (rt == null) continue;

                bool isHovered = (card == hoveredCard);

                // 目標位置・回転・スケール計算
                Vector2 targetPos = data.defaultPosition;
                Quaternion targetRot = data.defaultRotation;
                Vector3 targetScale = data.defaultScale;

                if (isHovered)
                {
                    // ホバー時: 上に移動 + 回転リセット + 拡大
                    targetPos = new Vector2(data.defaultPosition.x, data.defaultPosition.y + hoverYOffset);
                    targetRot = Quaternion.identity; // 回転なし
                    targetScale = Vector3.one * hoverScale;
                }

                // スムーズなアニメーション
                rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, Time.deltaTime * animationSpeed);
                rt.localRotation = Quaternion.Lerp(rt.localRotation, targetRot, Time.deltaTime * animationSpeed);
                rt.localScale = Vector3.Lerp(rt.localScale, targetScale, Time.deltaTime * animationSpeed);
            }
        }

        /// <summary>
        /// CardUIからCardDataを取得
        /// </summary>
        private CardData GetCardDataFromUI(GameObject cardUI)
        {
            // CardUIにはCardDataへの参照がないため、PlayerController.handから推測
            // 簡易実装: カード名からマッチング
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

            PlayerController player = GameManager.Instance.player1; // プレイヤー1の手札
            foreach (var card in player.hand)
            {
                if (card != null && card.cardName == cardName)
                {
                    return card;
                }
            }

            return null;
        }

        /// <summary>
        /// レイアウトデータクリア
        /// </summary>
        public void ClearLayout()
        {
            cardLayoutData.Clear();
            hoveredCard = null;
        }
    }

    /// <summary>
    /// カードレイアウトデータ
    /// </summary>
    public class CardLayoutData
    {
        public Vector2 defaultPosition;
        public Quaternion defaultRotation;
        public Vector3 defaultScale;
        public int sortOrder;
    }
}
