using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PTCG
{
    /// <summary>
    /// 手札カードの選択システム（ボタン追加、グロー効果、選択フィードバック）
    /// Yu-Gi-Oh Master Duel風のリッチな選択UI
    /// </summary>
    public class CardSelectionHandler : MonoBehaviour
    {
        public static CardSelectionHandler Instance { get; private set; }

        [Header("Selection Settings")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(1f, 1f, 0.8f, 1f); // 薄い黄色
        public Color selectedColor = new Color(1f, 0.9f, 0.3f, 1f); // 濃い黄色
        public Color glowColor = new Color(1f, 1f, 0f, 1f); // 黄色グロー

        private GameObject selectedCard = null;
        private List<GameObject> handCards = new List<GameObject>();

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
            // UIManager初期化後に実行されるよう0.5秒待機
            Invoke("InitializeCardSelection", 0.5f);
        }

        /// <summary>
        /// 手札カードにButton/Outline/EventTriggerを追加
        /// </summary>
        private void InitializeCardSelection()
        {
            GameObject playerHandZone = GameObject.Find("PlayerHand");
            if (playerHandZone == null)
            {
                Debug.LogWarning("PlayerHand not found");
                return;
            }

            UpdateHandCardButtons(playerHandZone);
        }

        /// <summary>
        /// 手札カード更新時に呼び出される（外部から呼ぶ想定）
        /// </summary>
        public void UpdateHandCardButtons(GameObject playerHandZone)
        {
            handCards.Clear();

            // PlayerHandZone内の全カードUIを取得
            for (int i = 0; i < playerHandZone.transform.childCount; i++)
            {
                GameObject cardUI = playerHandZone.transform.GetChild(i).gameObject;
                if (cardUI.name.Contains("CardUI"))
                {
                    handCards.Add(cardUI);
                    SetupCardInteraction(cardUI);
                }
            }

            Debug.Log($"CardSelectionHandler: {handCards.Count}枚の手札カードにButton追加");

            // HandCardLayoutManagerで扇状配置
            if (HandCardLayoutManager.Instance != null)
            {
                HandCardLayoutManager.Instance.ArrangeHandCards(handCards);
                Debug.Log("HandCardLayoutManager.ArrangeHandCards() 実行完了");
            }
            else
            {
                Debug.LogWarning("HandCardLayoutManager.Instance is null");
            }
        }

        /// <summary>
        /// カードUIにButton、Outline、EventTriggerを追加
        /// </summary>
        private void SetupCardInteraction(GameObject cardUI)
        {
            // カードサイズは元のまま（100x140）維持
            // RectTransform cardRT = cardUI.GetComponent<RectTransform>();
            // サイズ変更不要

            // テキストを黒色のまま維持（カード背景が紫/ピンクなので視認可能）
            foreach (Transform child in cardUI.transform)
            {
                if (child.name == "CardName" || child.name == "CardHP")
                {
                    Text txt = child.GetComponent<Text>();
                    if (txt != null)
                    {
                        // 色は変更しない（元の黒のまま）
                        txt.fontSize = child.name == "CardName" ? 14 : 12;
                    }

                    // テキストエリアをカード内に収める
                    RectTransform txtRT = child.GetComponent<RectTransform>();
                    if (txtRT != null)
                    {
                        txtRT.sizeDelta = new Vector2(140, 30);
                    }
                }
            }

            // Buttonコンポーネント追加
            Button btn = cardUI.GetComponent<Button>();
            if (btn == null)
            {
                btn = cardUI.AddComponent<Button>();
                btn.transition = Selectable.Transition.None; // ColorTintを無効化（CardImageの色を保持）

                // ButtonのtargetGraphicをnullに（カード背景色を保持）
                btn.targetGraphic = null;
            }

            // Outlineコンポーネント追加（グロー効果）
            Outline outline = cardUI.GetComponent<Outline>();
            if (outline == null)
            {
                outline = cardUI.AddComponent<Outline>();
                outline.effectColor = Color.black; // 通常時は黒縁
                outline.effectDistance = new Vector2(2, 2);
                outline.enabled = true; // デフォルトON（常に縁取り表示）
            }

            // クリックイベント設定
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnCardClicked(cardUI));

            // ホバーイベント設定（EventTrigger使用）
            UnityEngine.EventSystems.EventTrigger trigger = cardUI.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = cardUI.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // PointerEnter
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { OnCardHoverEnter(cardUI); });
            trigger.triggers.Add(pointerEnter);

            // PointerExit
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnCardHoverExit(cardUI); });
            trigger.triggers.Add(pointerExit);
        }

        /// <summary>
        /// カードホバー開始（黄色グローに変更）
        /// </summary>
        private void OnCardHoverEnter(GameObject cardUI)
        {
            // 選択中でなければ黄色グローに変更
            if (selectedCard != cardUI)
            {
                Outline outline = cardUI.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = glowColor; // 黄色
                    outline.effectDistance = new Vector2(3, 3);
                }
            }
        }

        /// <summary>
        /// カードホバー終了（選択中でなければ黒縁に戻す）
        /// </summary>
        private void OnCardHoverExit(GameObject cardUI)
        {
            if (selectedCard != cardUI)
            {
                Outline outline = cardUI.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = Color.black; // 黒縁に戻す
                    outline.effectDistance = new Vector2(2, 2);
                }
            }
        }

        /// <summary>
        /// カードクリック処理（選択状態切り替え）
        /// </summary>
        private void OnCardClicked(GameObject cardUI)
        {
            // 既存の選択を解除（黒縁に戻す）
            if (selectedCard != null && selectedCard != cardUI)
            {
                Outline prevOutline = selectedCard.GetComponent<Outline>();
                if (prevOutline != null)
                {
                    prevOutline.effectColor = Color.black;
                    prevOutline.effectDistance = new Vector2(2, 2);
                }
            }

            // 新しい選択
            if (selectedCard == cardUI)
            {
                // 同じカードをクリック → 選択解除（黒縁に戻す）
                selectedCard = null;
                Outline outline = cardUI.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = Color.black;
                    outline.effectDistance = new Vector2(2, 2);
                }
                Debug.Log("カード選択解除");
            }
            else
            {
                // 別のカードをクリック → 選択（濃い黄色グロー）
                selectedCard = cardUI;
                Outline outline = cardUI.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = selectedColor; // 濃い黄色
                    outline.effectDistance = new Vector2(4, 4); // 選択時は太く
                }
                Debug.Log($"カード選択: {cardUI.name}");
            }
        }

        /// <summary>
        /// 現在選択中のカードを取得
        /// </summary>
        public GameObject GetSelectedCard()
        {
            return selectedCard;
        }

        /// <summary>
        /// 選択をクリア（黒縁に戻す）
        /// </summary>
        public void ClearSelection()
        {
            if (selectedCard != null)
            {
                Outline outline = selectedCard.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectColor = Color.black;
                    outline.effectDistance = new Vector2(2, 2);
                }
                selectedCard = null;
            }
        }
    }
}
