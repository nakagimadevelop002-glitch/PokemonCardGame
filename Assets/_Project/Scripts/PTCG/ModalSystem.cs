using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PTCG
{
    /// <summary>
    /// モーダル選択システム（UI実装前の基盤）
    /// </summary>
    public class ModalSystem : MonoBehaviour
    {
        public static ModalSystem Instance { get; private set; }

        [Header("Auto Select Mode")]
        public bool autoSelectMode = false; // 手動選択モード

        // UI要素キャッシュ（非アクティブでも参照可能）
        private GameObject selectModalPanel;
        private Text titleText;
        private Transform optionsContainer;
        private Button cancelButton;
        private GameObject optionButtonTemplate;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // UI要素をキャッシュ（非アクティブ化前に取得）
            selectModalPanel = GameObject.Find("SelectModalPanel");

            if (selectModalPanel != null)
            {
                // 正しく表示されているCardNameTextからフォント設定をコピー
                GameObject cardNameTextObj = GameObject.Find("CardNameText");
                Font correctFont = null;
                if (cardNameTextObj != null)
                {
                    Text cardNameText = cardNameTextObj.GetComponent<Text>();
                    if (cardNameText != null)
                    {
                        correctFont = cardNameText.font;
                        Debug.Log($"[ModalSystem] Correct font found: {(correctFont != null ? correctFont.name : "NULL")}");
                    }
                }

                // SelectModalPanelの子孫要素から名前で検索（再帰的）
                // GetComponentsInChildren<>()を使用して全子孫から検索
                Text[] allTexts = selectModalPanel.GetComponentsInChildren<Text>(true);
                foreach (var text in allTexts)
                {
                    if (text.name == "TitleText")
                    {
                        titleText = text;
                        // フォント設定をコピー
                        if (correctFont != null)
                        {
                            titleText.font = correctFont;
                            Debug.Log($"[ModalSystem] Set font for TitleText: {correctFont.name}");
                        }
                        break;
                    }
                }

                Transform[] allTransforms = selectModalPanel.GetComponentsInChildren<Transform>(true);
                foreach (var t in allTransforms)
                {
                    if (t.name == "OptionsContainer")
                    {
                        optionsContainer = t;
                    }
                    else if (t.name == "OptionButtonTemplate")
                    {
                        optionButtonTemplate = t.gameObject;
                        // OptionButtonTemplate配下のTextにもフォント設定
                        if (correctFont != null)
                        {
                            Text btnText = optionButtonTemplate.GetComponentInChildren<Text>(true);
                            if (btnText != null)
                            {
                                btnText.font = correctFont;
                                Debug.Log($"[ModalSystem] Set font for OptionButtonTemplate Text: {correctFont.name}");
                            }
                            else
                            {
                                Debug.LogWarning($"[ModalSystem] OptionButtonTemplate has no Text component");
                            }
                        }
                    }
                }

                Button[] allButtons = selectModalPanel.GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    if (btn.name == "CancelButton")
                    {
                        cancelButton = btn;
                        break;
                    }
                }

                // CancelButton配下のText要素を名前で取得してフォント設定
                if (cancelButton != null && correctFont != null)
                {
                    Text[] cancelTexts = selectModalPanel.GetComponentsInChildren<Text>(true);
                    foreach (var text in cancelTexts)
                    {
                        if (text.transform.parent == cancelButton.transform)
                        {
                            text.font = correctFont;
                            Debug.Log($"[ModalSystem] Set font for CancelButton child Text '{text.name}': {correctFont.name}");
                        }
                    }
                }

                // SelectModalPanel初期非表示
                selectModalPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("[ModalSystem] SelectModalPanel not found in scene");
            }

            // OptionButtonTemplate初期非表示
            if (optionButtonTemplate != null)
            {
                optionButtonTemplate.SetActive(false);
            }

            // デバッグ: 取得結果を確認
            Debug.Log($"[ModalSystem] Awake完了 - selectModalPanel: {(selectModalPanel != null ? "OK" : "NULL")}");
            Debug.Log($"[ModalSystem] titleText: {(titleText != null ? "OK" : "NULL")}");
            Debug.Log($"[ModalSystem] optionsContainer: {(optionsContainer != null ? "OK" : "NULL")}");
            Debug.Log($"[ModalSystem] cancelButton: {(cancelButton != null ? "OK" : "NULL")}");
            Debug.Log($"[ModalSystem] optionButtonTemplate: {(optionButtonTemplate != null ? "OK" : "NULL")}");
        }

        /// <summary>
        /// 単一選択モーダル（1つ選択）
        /// </summary>
        /// <typeparam name="T">選択肢の型</typeparam>
        /// <param name="title">タイトル</param>
        /// <param name="options">選択肢リスト</param>
        /// <param name="callback">選択結果コールバック</param>
        /// <param name="defaultFirst">最初の選択肢をデフォルトにするか</param>
        public void OpenSelectModal<T>(string title, List<SelectOption<T>> options, Action<T> callback, bool defaultFirst = false)
        {
            if (options == null || options.Count == 0)
            {
                Debug.Log($"[ModalSystem] {title}: 選択肢なし");
                callback(default(T));
                return;
            }

            if (autoSelectMode)
            {
                // UI未実装時: 最初の選択肢を自動選択
                var selected = defaultFirst || options.Count == 1 ? options[0] : options[0];
                Debug.Log($"[ModalSystem] {title}: 自動選択 → {selected.label}");
                callback(selected.value);
            }
            else
            {
                // UI表示処理
                ShowSelectModal(title, options, callback);
            }
        }

        /// <summary>
        /// 複数選択モーダル（最大N個選択）
        /// </summary>
        /// <typeparam name="T">選択肢の型</typeparam>
        /// <param name="title">タイトル</param>
        /// <param name="options">選択肢リスト</param>
        /// <param name="maxCount">最大選択数</param>
        /// <param name="callback">選択結果コールバック</param>
        public void OpenMultiSelectModal<T>(string title, List<SelectOption<T>> options, int maxCount, Action<List<T>> callback)
        {
            if (options == null || options.Count == 0)
            {
                Debug.Log($"[ModalSystem] {title}: 選択肢なし");
                callback(new List<T>());
                return;
            }

            if (autoSelectMode)
            {
                // UI未実装時: 最初からmaxCount個を自動選択
                var selected = new List<T>();
                for (int i = 0; i < Mathf.Min(maxCount, options.Count); i++)
                {
                    selected.Add(options[i].value);
                }
                Debug.Log($"[ModalSystem] {title}: 自動選択 {selected.Count}個");
                callback(selected);
            }
            else
            {
                // TODO: UI実装時にここでUIを表示
                Debug.Log($"[ModalSystem] {title}: UI未実装 - 自動選択");
                var selected = new List<T>();
                for (int i = 0; i < Mathf.Min(maxCount, options.Count); i++)
                {
                    selected.Add(options[i].value);
                }
                callback(selected);
            }
        }

        /// <summary>
        /// Yes/Noモーダル
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="callback">結果コールバック（true=Yes, false=No）</param>
        public void OpenConfirmModal(string title, string message, Action<bool> callback)
        {
            if (autoSelectMode)
            {
                // UI未実装時: デフォルトYes
                Debug.Log($"[ModalSystem] {title}: {message} → 自動Yes");
                callback(true);
            }
            else
            {
                // TODO: UI実装時にここでUIを表示
                Debug.Log($"[ModalSystem] {title}: {message} → UI未実装");
                callback(true);
            }
        }

        /// <summary>
        /// 単一選択モーダルUI表示
        /// </summary>
        private void ShowSelectModal<T>(string title, List<SelectOption<T>> options, Action<T> callback)
        {
            // キャッシュからUI要素取得
            if (selectModalPanel == null)
            {
                Debug.LogError("[ModalSystem] SelectModalPanel not found");
                callback(default(T));
                return;
            }

            if (titleText == null || optionsContainer == null || cancelButton == null || optionButtonTemplate == null)
            {
                Debug.LogError($"[ModalSystem] UI elements not found - titleText:{(titleText != null ? "OK" : "NULL")}, optionsContainer:{(optionsContainer != null ? "OK" : "NULL")}, cancelButton:{(cancelButton != null ? "OK" : "NULL")}, optionButtonTemplate:{(optionButtonTemplate != null ? "OK" : "NULL")}");
                callback(default(T));
                return;
            }

            // タイトル設定（文字化け防止: 明示的にクリアしてから設定）
            titleText.text = "";
            titleText.text = title;

            // 既存ボタンクリア（テンプレート以外）
            foreach (Transform child in optionsContainer)
            {
                if (child.gameObject != optionButtonTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            // 正しいフォントを取得（Awake()で設定したフォント）
            Font correctFont = null;
            if (titleText != null && titleText.font != null)
            {
                correctFont = titleText.font;
            }

            // ボタン動的生成
            for (int i = 0; i < options.Count; i++)
            {
                SelectOption<T> option = options[i];
                GameObject btn = Instantiate(optionButtonTemplate, optionsContainer);
                btn.name = "Option_" + i;
                btn.SetActive(true);

                Text btnText = btn.GetComponentInChildren<Text>(true);
                if (btnText != null)
                {
                    // フォント設定（Instantiate後に明示的に設定）
                    if (correctFont != null)
                    {
                        btnText.font = correctFont;
                    }
                    // 文字化け防止: 明示的にクリアしてから設定
                    btnText.text = "";
                    string displayText = option.label;
                    if (option.disabled && !string.IsNullOrEmpty(option.disabledReason))
                    {
                        displayText += $" ({option.disabledReason})";
                    }
                    btnText.text = displayText;

                    // disabled時はテキスト色を灰色に変更
                    if (option.disabled)
                    {
                        btnText.color = Color.gray;
                    }

                    Debug.Log($"[ModalSystem] Set option button text: '{displayText}' (font: {(correctFont != null ? correctFont.name : "NULL")}, disabled: {option.disabled})");
                }
                else
                {
                    Debug.LogError($"[ModalSystem] Option button '{btn.name}' has no Text component!");
                }

                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = !option.disabled; // disabled時は選択不可
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        callback(option.value);
                        selectModalPanel.SetActive(false);
                    });
                }

                // Y位置を設定（上から順に配置）
                RectTransform rt = btn.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, -i * 60);
                }
            }

            // キャンセルボタン
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                callback(default(T));
                selectModalPanel.SetActive(false);
            });

            // キャンセルボタンのTextにフォント設定
            if (correctFont != null && cancelButton != null)
            {
                foreach (Transform child in cancelButton.transform)
                {
                    Text cancelText = child.GetComponent<Text>();
                    if (cancelText != null)
                    {
                        cancelText.font = correctFont;
                        // 文字化け防止: クリア→設定
                        string originalText = cancelText.text;
                        cancelText.text = "";
                        cancelText.text = originalText;
                    }
                }
            }

            // テンプレート非表示
            optionButtonTemplate.SetActive(false);

            // パネル表示
            selectModalPanel.SetActive(true);

            Debug.Log($"[ModalSystem] {title}: UI表示 - {options.Count}個の選択肢");
        }
    }

    /// <summary>
    /// 選択肢データ構造
    /// </summary>
    [System.Serializable]
    public class SelectOption<T>
    {
        public string label; // 表示名
        public T value; // 実際の値
        public bool disabled; // 選択不可フラグ
        public string disabledReason; // 選択不可理由

        public SelectOption(string label, T value, bool disabled = false, string disabledReason = "")
        {
            this.label = label;
            this.value = value;
            this.disabled = disabled;
            this.disabledReason = disabledReason;
        }
    }
}
