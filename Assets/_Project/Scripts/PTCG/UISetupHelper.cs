using UnityEngine;
using UnityEngine.UI;

namespace PTCG
{
    /// <summary>
    /// UI要素のサイズと色を設定するヘルパー
    /// </summary>
    public class UISetupHelper : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("UISetupHelper: Awake called");
            SetupUIElements();
        }

        private void Start()
        {
            Debug.Log("UISetupHelper: Start called");
            SetupUIElements();
        }

        private void SetupUIElements()
        {
            // OpponentDeck
            SetPanelProperties("OpponentDeck", new Vector2(150, 200), new Color(0.3f, 0.3f, 0.3f, 0.8f));

            // OpponentBench
            SetPanelProperties("OpponentBench", new Vector2(800, 180), new Color(0.2f, 0.2f, 0.2f, 0.5f));

            // OpponentPrizes
            SetPanelProperties("OpponentPrizes", new Vector2(150, 200), new Color(0.3f, 0.3f, 0.3f, 0.8f));

            // OpponentActive
            SetPanelProperties("OpponentActive", new Vector2(200, 280), new Color(0.4f, 0.2f, 0.2f, 0.7f));

            // PlayerActive
            SetPanelProperties("PlayerActive", new Vector2(200, 280), new Color(0.2f, 0.4f, 0.2f, 0.7f));

            // PlayerPrizes
            SetPanelProperties("PlayerPrizes", new Vector2(150, 200), new Color(0.3f, 0.3f, 0.3f, 0.8f));

            // PlayerHand
            SetPanelProperties("PlayerHand", new Vector2(1000, 180), new Color(0.2f, 0.2f, 0.2f, 0.5f));

            // PlayerDeck
            SetPanelProperties("PlayerDeck", new Vector2(150, 200), new Color(0.3f, 0.3f, 0.3f, 0.8f));

            // BattleField - 透明に
            SetPanelProperties("BattleField", new Vector2(1200, 400), new Color(0, 0, 0, 0));

            // OpponentArea - 透明に
            SetPanelProperties("OpponentArea", new Vector2(1600, 220), new Color(0, 0, 0, 0));

            // PlayerArea - 透明に
            SetPanelProperties("PlayerArea", new Vector2(1600, 220), new Color(0, 0, 0, 0));

            // EndTurnButtonの色を設定
            GameObject endTurnBtn = GameObject.Find("EndTurnButton");
            if (endTurnBtn != null)
            {
                Image btnImage = endTurnBtn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = new Color(0.2f, 0.6f, 0.9f, 1f); // 青色
                }

                // ボタンテキストの色を白に
                Text btnText = endTurnBtn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.color = Color.white;
                    btnText.fontSize = 18;
                }
            }

            // テキストの色とサイズを調整
            SetupTextElements();

            Debug.Log("UI setup completed by UISetupHelper");
        }

        private void SetPanelProperties(string panelName, Vector2 size, Color color)
        {
            GameObject panel = GameObject.Find(panelName);
            if (panel != null)
            {
                RectTransform rt = panel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = size;
                }

                Image image = panel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = color;
                }
            }
        }

        private void SetupTextElements()
        {
            // All text elements - make them white and larger
            Text[] allTexts = FindObjectsByType<Text>(FindObjectsSortMode.None);
            foreach (Text txt in allTexts)
            {
                if (txt.gameObject.name.Contains("Count"))
                {
                    txt.color = Color.white;
                    txt.fontSize = 36;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                }
            }
        }
    }
}
