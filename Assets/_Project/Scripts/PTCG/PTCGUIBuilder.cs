using UnityEngine;
using UnityEngine.UI;

namespace PTCG
{
    /// <summary>
    /// ポケモンカードゲームのUIを構築
    /// </summary>
    public class PTCGUIBuilder : MonoBehaviour
    {
        private void Start()
        {
            BuildUI();
            // 自分自身を削除
            Destroy(gameObject);
        }

        [ContextMenu("Build PTCG UI")]
        public void BuildUI()
        {
            // Canvas作成
            GameObject canvasObj = new GameObject("PTCGCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 背景（黒）
            CreatePanel(canvasObj.transform, "Background", 0, 0, 1920, 1080, new Color(0.05f, 0.05f, 0.05f, 1f));

            // === 上部エリア（相手側・赤系） ===
            CreateOpponentArea(canvasObj.transform);

            // === 中央エリア（バトルフィールド・紫系） ===
            CreateBattleFieldArea(canvasObj.transform);

            // === 下部エリア（プレイヤー側・青系） ===
            CreatePlayerArea(canvasObj.transform);

            // === 右側UI ===
            CreateRightSideUI(canvasObj.transform);

            Debug.Log("PTCG UI Built Successfully!");
        }

        void CreateOpponentArea(Transform parent)
        {
            float yPos = 370;

            // 相手プライズ（左上）
            GameObject opponentPrizes = CreatePanel(parent, "OpponentPrizes", -700, yPos, 180, 220,
                new Color(0.3f, 0.2f, 0.2f, 0.95f));
            AddBorder(opponentPrizes.transform, new Color(0.8f, 0.2f, 0.2f, 1f), 4);
            CreateText(opponentPrizes.transform, "PrizesCount", 0, -90, "6", 48, Color.white, FontStyle.Bold);

            // 相手ベンチ（中央上）
            GameObject opponentBench = CreatePanel(parent, "OpponentBench", 0, yPos, 1000, 200,
                new Color(0.25f, 0.15f, 0.15f, 0.9f));
            AddBorder(opponentBench.transform, new Color(0.6f, 0.3f, 0.3f, 1f), 3);

            // 相手デッキ（右上）
            GameObject opponentDeck = CreatePanel(parent, "OpponentDeck", 700, yPos, 180, 220,
                new Color(0.3f, 0.2f, 0.2f, 0.95f));
            AddBorder(opponentDeck.transform, new Color(0.8f, 0.2f, 0.2f, 1f), 4);
            CreateText(opponentDeck.transform, "DeckCount", 0, -90, "47", 48, Color.white, FontStyle.Bold);
        }

        void CreateBattleFieldArea(Transform parent)
        {
            // バトルフィールド背景
            GameObject battleField = CreatePanel(parent, "BattleField", 0, 0, 1200, 480,
                new Color(0.15f, 0.05f, 0.25f, 0.8f));

            // 相手アクティブ（上側）
            GameObject opponentActive = CreatePanel(battleField.transform, "OpponentActive", 0, 120, 200, 260,
                new Color(0.5f, 0.15f, 0.15f, 0.95f));
            AddBorder(opponentActive.transform, new Color(1f, 0.3f, 0.3f, 1f), 5);
            CreateText(opponentActive.transform, "Label", 0, -110, "相手", 24, Color.white, FontStyle.Bold);

            // プレイヤーアクティブ（下側）
            GameObject playerActive = CreatePanel(battleField.transform, "PlayerActive", 0, -120, 200, 260,
                new Color(0.15f, 0.3f, 0.5f, 0.95f));
            AddBorder(playerActive.transform, new Color(0.3f, 0.6f, 1f, 1f), 5);
            CreateText(playerActive.transform, "Label", 0, -110, "自分", 24, Color.white, FontStyle.Bold);
        }

        void CreatePlayerArea(Transform parent)
        {
            float yPos = -370;

            // プレイヤープライズ（左下）
            GameObject playerPrizes = CreatePanel(parent, "PlayerPrizes", -700, yPos, 180, 220,
                new Color(0.2f, 0.25f, 0.35f, 0.95f));
            AddBorder(playerPrizes.transform, new Color(0.3f, 0.5f, 0.9f, 1f), 4);
            CreateText(playerPrizes.transform, "PrizesCount", 0, -90, "6", 48, Color.white, FontStyle.Bold);

            // プレイヤー手札（中央下）
            GameObject playerHand = CreatePanel(parent, "PlayerHand", 0, yPos, 1000, 200,
                new Color(0.15f, 0.2f, 0.3f, 0.9f));
            AddBorder(playerHand.transform, new Color(0.4f, 0.5f, 0.7f, 1f), 3);

            // プレイヤーデッキ（右下）
            GameObject playerDeck = CreatePanel(parent, "PlayerDeck", 700, yPos, 180, 220,
                new Color(0.2f, 0.25f, 0.35f, 0.95f));
            AddBorder(playerDeck.transform, new Color(0.3f, 0.5f, 0.9f, 1f), 4);
            CreateText(playerDeck.transform, "DeckCount", 0, -90, "46", 48, Color.white, FontStyle.Bold);
        }

        void CreateRightSideUI(Transform parent)
        {
            // END TURNボタン
            GameObject endTurnBtn = CreateButton(parent, "EndTurnButton", 820, -300, 160, 80,
                "END\nTURN", new Color(0.9f, 0.6f, 0.1f, 1f));

            // プライズカウンター表示（右側）
            CreateText(parent, "OpponentPrizesDisplay", 820, 300, "6", 60,
                new Color(1f, 0.3f, 0.3f, 1f), FontStyle.Bold);
            CreateText(parent, "PlayerPrizesDisplay", 820, -400, "6", 60,
                new Color(0.3f, 0.6f, 1f, 1f), FontStyle.Bold);
        }

        GameObject CreatePanel(Transform parent, string name, float x, float y, float width, float height, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);

            Image image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        void AddBorder(Transform parent, Color borderColor, float thickness)
        {
            GameObject border = new GameObject("Border");
            border.transform.SetParent(parent, false);

            RectTransform rt = border.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(thickness, -thickness);

            Image image = border.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
        }

        GameObject CreateText(Transform parent, string name, float x, float y, string text, int fontSize, Color color, FontStyle style)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rt = textObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(200, 100);

            Text txt = textObj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.fontStyle = style;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 影追加
            Shadow shadow = textObj.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(2, -2);

            return textObj;
        }

        GameObject CreateButton(Transform parent, string name, float x, float y, float width, float height, string text, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);

            Image image = btnObj.AddComponent<Image>();
            image.color = color;

            Button button = btnObj.AddComponent<Button>();

            // ボタンテキスト
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            Text txt = textObj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return btnObj;
        }
    }
}
