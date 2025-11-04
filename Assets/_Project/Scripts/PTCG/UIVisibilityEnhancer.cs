using UnityEngine;
using UnityEngine.UI;

namespace PTCG
{
    /// <summary>
    /// UI要素の視認性を向上させる - Awakeで即座に実行
    /// </summary>
    [ExecuteAlways]
    public class UIVisibilityEnhancer : MonoBehaviour
    {
        [Header("Auto Setup on Awake")]
        public bool autoSetup = true;

        private void Awake()
        {
            if (autoSetup)
            {
                Debug.Log("[UIVisibilityEnhancer] Starting UI enhancement...");
                EnhanceUI();
            }
        }

        private void OnEnable()
        {
            if (autoSetup)
            {
                EnhanceUI();
            }
        }

        [ContextMenu("Enhance UI Now")]
        public void EnhanceUI()
        {
            int enhanced = 0;

            // 全てのテキストを白く、大きく、太字にする
            Text[] allTexts = FindObjectsOfType<Text>(true);
            foreach (Text txt in allTexts)
            {
                if (txt.name.Contains("Count") || txt.name.Contains("Text"))
                {
                    txt.color = Color.white;
                    txt.fontSize = Mathf.Max(txt.fontSize, 32);
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;

                    // テキストに影を追加
                    Shadow shadow = txt.GetComponent<Shadow>();
                    if (shadow == null)
                    {
                        shadow = txt.gameObject.AddComponent<Shadow>();
                    }
                    shadow.effectColor = Color.black;
                    shadow.effectDistance = new Vector2(2, -2);

                    enhanced++;
                }
            }

            // 全てのパネルに背景色を設定
            Image[] allImages = FindObjectsOfType<Image>(true);
            foreach (Image img in allImages)
            {
                string name = img.gameObject.name;

                if (name.Contains("Deck") && !name.Contains("Area"))
                {
                    img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
                    SetRectSize(img.rectTransform, 120, 160);
                    enhanced++;
                }
                else if (name.Contains("Prizes"))
                {
                    img.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
                    SetRectSize(img.rectTransform, 120, 160);
                    enhanced++;
                }
                else if (name.Contains("Active"))
                {
                    if (name.Contains("Opponent"))
                    {
                        img.color = new Color(0.4f, 0.1f, 0.1f, 0.8f); // 赤系
                    }
                    else
                    {
                        img.color = new Color(0.1f, 0.4f, 0.1f, 0.8f); // 緑系
                    }
                    SetRectSize(img.rectTransform, 180, 250);
                    enhanced++;
                }
                else if (name.Contains("Bench") || name.Contains("Hand"))
                {
                    img.color = new Color(0.15f, 0.15f, 0.15f, 0.7f);
                    enhanced++;
                }
                else if (name == "EndTurnButton")
                {
                    img.color = new Color(0.1f, 0.5f, 0.9f, 1f); // 青色
                    enhanced++;
                }
            }

            // ボタンのテキストも調整
            Button[] buttons = FindObjectsOfType<Button>(true);
            foreach (Button btn in buttons)
            {
                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.color = Color.white;
                    btnText.fontSize = 20;
                    btnText.fontStyle = FontStyle.Bold;
                    enhanced++;
                }
            }

            Debug.Log($"[UIVisibilityEnhancer] Enhanced {enhanced} UI elements");
        }

        private void SetRectSize(RectTransform rt, float width, float height)
        {
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(width, height);
            }
        }
    }
}
