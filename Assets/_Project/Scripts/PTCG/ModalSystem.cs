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

        // SelectModal UI要素キャッシュ（非アクティブでも参照可能）
        private GameObject selectModalPanel;
        private Text titleText;
        private Transform optionsContainer;
        private Button cancelButton;
        private GameObject optionButtonTemplate;
        private Button confirmButton; // 複数選択時の確認ボタン
        private Text selectionCountText; // 選択数表示

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
                // Canvas/GraphicRaycaster確認と自動追加
                Canvas canvas = selectModalPanel.GetComponentInParent<Canvas>();
                GraphicRaycaster raycaster = selectModalPanel.GetComponentInParent<GraphicRaycaster>();
                Debug.Log("[ModalSystem] SelectModalPanel Canvas: " + (canvas != null ? "OK" : "NULL"));
                Debug.Log("[ModalSystem] SelectModalPanel GraphicRaycaster: " + (raycaster != null ? "OK" : "NULL"));

                if (canvas != null && raycaster == null)
                {
                    Debug.LogWarning("[ModalSystem] GraphicRaycasterが見つかりません。自動的に追加します。");
                    raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                    Debug.Log("[ModalSystem] GraphicRaycaster追加完了");
                }
                else if (canvas == null)
                {
                    Debug.LogError("[ModalSystem] SelectModalPanelの親Canvasが見つかりません。UIクリックが動作しません。");
                }
                // 正しく表示されているCardNameTextからフォント設定をコピー
                GameObject cardNameTextObj = GameObject.Find("CardNameText");
                Font correctFont = null;
                if (cardNameTextObj != null)
                {
                    Text cardNameText = cardNameTextObj.GetComponent<Text>();
                    if (cardNameText != null)
                    {
                        correctFont = cardNameText.font;
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

            // SelectModalPanel の ConfirmButton と SelectionCountText をキャッシュ
            if (selectModalPanel != null)
            {
                Font correctFont = null;
                if (titleText != null && titleText.font != null)
                {
                    correctFont = titleText.font;
                }

                Button[] allButtons = selectModalPanel.GetComponentsInChildren<Button>(true);
                foreach (var btn in allButtons)
                {
                    if (btn.name == "ConfirmButton")
                    {
                        confirmButton = btn;
                        confirmButton.gameObject.SetActive(false); // 初期非表示（複数選択時のみ表示）
                        break;
                    }
                }

                Text[] allTexts = selectModalPanel.GetComponentsInChildren<Text>(true);
                foreach (var text in allTexts)
                {
                    if (text.name == "SelectionCountText")
                    {
                        selectionCountText = text;
                        if (correctFont != null) selectionCountText.font = correctFont;
                        selectionCountText.gameObject.SetActive(false); // 初期非表示（複数選択時のみ表示）
                        break;
                    }
                }
            }

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
                callback(default(T));
                return;
            }

            if (autoSelectMode)
            {
                // UI未実装時: 最初の選択肢を自動選択
                var selected = defaultFirst || options.Count == 1 ? options[0] : options[0];
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
                callback(selected);
            }
            else
            {
                // SelectModalPanel を使用した複数選択UI
                ShowSelectModalMulti(title, options, maxCount, callback);
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
            // 常にデフォルトYes（UI未実装）
            callback(true);
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
                Debug.LogError("[ModalSystem] UI elements not found - titleText:" + (titleText != null ? "OK" : "NULL") + ", optionsContainer:" + (optionsContainer != null ? "OK" : "NULL") + ", cancelButton:" + (cancelButton != null ? "OK" : "NULL") + ", optionButtonTemplate:" + (optionButtonTemplate != null ? "OK" : "NULL"));
                callback(default(T));
                return;
            }

            // タイトル設定
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
                    string displayText = option.label;
                    if (option.disabled && !string.IsNullOrEmpty(option.disabledReason))
                    {
                        displayText += " (" + option.disabledReason + ")";
                    }
                    btnText.text = displayText;

                    // disabled時はテキスト色を灰色に変更
                    if (option.disabled)
                    {
                        btnText.color = Color.gray;
                    }
                }

                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = !option.disabled; // disabled時は選択不可
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        selectModalPanel.SetActive(false); // callback前に非アクティブ化（callback内で別モーダルを開く場合に対応）
                        callback(option.value);
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
                selectModalPanel.SetActive(false); // callback前に非アクティブ化
                callback(default(T));
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
                    }
                }
            }

            // テンプレート非表示
            optionButtonTemplate.SetActive(false);

            // パネル表示
            selectModalPanel.SetActive(true);
        }

        /// <summary>
        /// 複数選択モーダルUI表示（SelectModalPanelを使用）
        /// </summary>
        private void ShowSelectModalMulti<T>(string title, List<SelectOption<T>> options, int maxCount, Action<List<T>> callback)
        {
            Debug.Log("[ShowSelectModalMulti] 開始");
            Debug.Log("[ShowSelectModalMulti] selectModalPanel: " + (selectModalPanel != null ? "OK" : "NULL"));
            Debug.Log("[ShowSelectModalMulti] titleText: " + (titleText != null ? "OK" : "NULL"));
            Debug.Log("[ShowSelectModalMulti] optionsContainer: " + (optionsContainer != null ? "OK" : "NULL"));
            Debug.Log("[ShowSelectModalMulti] optionButtonTemplate: " + (optionButtonTemplate != null ? "OK" : "NULL"));
            Debug.Log("[ShowSelectModalMulti] confirmButton: " + (confirmButton != null ? "OK" : "NULL"));
            Debug.Log("[ShowSelectModalMulti] selectionCountText: " + (selectionCountText != null ? "OK" : "NULL"));

            if (selectModalPanel == null || titleText == null || optionsContainer == null ||
                optionButtonTemplate == null || confirmButton == null || selectionCountText == null)
            {
                Debug.LogError("[ModalSystem] SelectModalPanel UI elements not found for multi-select");
                Debug.LogError("[ModalSystem] Missing elements: " +
                    (selectModalPanel == null ? "selectModalPanel " : "") +
                    (titleText == null ? "titleText " : "") +
                    (optionsContainer == null ? "optionsContainer " : "") +
                    (optionButtonTemplate == null ? "optionButtonTemplate " : "") +
                    (confirmButton == null ? "confirmButton " : "") +
                    (selectionCountText == null ? "selectionCountText " : ""));
                callback(new List<T>());
                return;
            }

            // タイトル設定
            titleText.text = title;

            // 既存ボタンクリア（テンプレート以外）
            foreach (Transform child in optionsContainer)
            {
                if (child.gameObject != optionButtonTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            Font correctFont = titleText.font;
            List<T> selectedValues = new List<T>();

            // トグルボタン動的生成
            for (int i = 0; i < options.Count; i++)
            {
                SelectOption<T> option = options[i];
                GameObject toggleObj = Instantiate(optionButtonTemplate, optionsContainer);
                toggleObj.name = "Option_" + i;
                toggleObj.SetActive(true);

                // Button コンポーネントを削除（Toggle と共存不可）
                Button existingButton = toggleObj.GetComponent<Button>();
                if (existingButton != null)
                {
                    DestroyImmediate(existingButton);
                }

                // Toggleコンポーネント追加
                UnityEngine.UI.Toggle toggle = toggleObj.GetComponent<UnityEngine.UI.Toggle>();
                if (toggle == null)
                {
                    toggle = toggleObj.AddComponent<UnityEngine.UI.Toggle>();
                    toggle.isOn = false;
                }

                // targetGraphicを明示的に設定（Toggleに必須）
                Image toggleImage = toggleObj.GetComponent<Image>();
                if (toggleImage != null)
                {
                    toggle.targetGraphic = toggleImage;
                    toggleImage.raycastTarget = true; // クリックイベント検知に必須
                    Debug.Log("[ShowSelectModalMulti] toggleImage.raycastTarget設定: " + option.label);
                }
                else
                {
                    Debug.LogWarning("[ShowSelectModalMulti] toggleImage not found for " + option.label);
                }

                // Checkmark用のImageを動的に作成（選択状態の視覚的フィードバック）
                GameObject checkmarkObj = new GameObject("Checkmark");
                checkmarkObj.transform.SetParent(toggleObj.transform, false);
                Image checkmarkImage = checkmarkObj.AddComponent<Image>();
                checkmarkImage.color = new Color(0.2f, 1f, 0.2f, 1f); // 緑色のチェックマーク

                RectTransform checkmarkRT = checkmarkObj.GetComponent<RectTransform>();
                checkmarkRT.anchorMin = new Vector2(0.9f, 0.1f);
                checkmarkRT.anchorMax = new Vector2(0.95f, 0.9f);
                checkmarkRT.offsetMin = Vector2.zero;
                checkmarkRT.offsetMax = Vector2.zero;

                checkmarkImage.gameObject.SetActive(false); // 初期は非表示
                toggle.graphic = checkmarkImage; // ToggleのgraphicプロパティにCheckmarkを設定

                // Toggle transitions設定（視覚的フィードバック）
                toggle.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                ColorBlock colors = toggle.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 1f);
                colors.highlightedColor = new Color(0.8f, 0.8f, 1f, 1f);
                colors.pressedColor = new Color(0.6f, 0.6f, 1f, 1f);
                colors.selectedColor = new Color(0.8f, 1f, 0.8f, 1f);
                toggle.colors = colors;

                Debug.Log("[ShowSelectModalMulti] Toggle設定完了: " + option.label + " targetGraphic=" + (toggle.targetGraphic != null ? "OK" : "NULL") + " graphic=" + (toggle.graphic != null ? "OK" : "NULL"));

                Text toggleText = toggleObj.GetComponentInChildren<Text>(true);
                if (toggleText != null)
                {
                    if (correctFont != null) toggleText.font = correctFont;
                    toggleText.text = option.label;

                    if (option.disabled) toggleText.color = Color.gray;
                }

                toggle.interactable = !option.disabled;

                // トグル変更時の選択数更新
                toggle.onValueChanged.AddListener((isOn) => {
                    Debug.Log("[ShowSelectModalMulti] Toggle変更: " + option.label + " isOn=" + isOn);

                    if (isOn && !selectedValues.Contains(option.value))
                    {
                        if (selectedValues.Count < maxCount)
                        {
                            selectedValues.Add(option.value);
                            Debug.Log("[ShowSelectModalMulti] 選択追加: " + option.label + " (現在" + selectedValues.Count + "個)");
                        }
                        else
                        {
                            toggle.isOn = false;
                            Debug.Log("[ShowSelectModalMulti] 選択上限到達: " + maxCount + "個");
                        }
                    }
                    else if (!isOn && selectedValues.Contains(option.value))
                    {
                        selectedValues.Remove(option.value);
                        Debug.Log("[ShowSelectModalMulti] 選択解除: " + option.label + " (現在" + selectedValues.Count + "個)");
                    }

                    // 選択数表示更新
                    selectionCountText.text = selectedValues.Count + " / " + maxCount + "個選択";

                    // 決定ボタンの有効/無効
                    confirmButton.interactable = selectedValues.Count > 0 && selectedValues.Count <= maxCount;
                    Debug.Log("[ShowSelectModalMulti] confirmButton.interactable=" + confirmButton.interactable);
                });

                // Y位置設定
                RectTransform rt = toggleObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, -i * 60);
                }
            }

            Debug.Log("[ShowSelectModalMulti] トグルボタン生成完了: " + options.Count + "個");

            // 初期状態: 選択数0
            selectionCountText.text = "0 / " + maxCount + "個選択";
            selectionCountText.gameObject.SetActive(true); // 複数選択時は表示
            confirmButton.interactable = false;
            confirmButton.gameObject.SetActive(true); // 複数選択時は表示

            Debug.Log("[ShowSelectModalMulti] 初期状態設定完了: 選択数0、confirmButton無効");

            // 決定ボタン
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
                Debug.Log("[ShowSelectModalMulti] confirmButtonクリック: 選択数=" + selectedValues.Count);
                callback(new List<T>(selectedValues));
                confirmButton.gameObject.SetActive(false);
                selectionCountText.gameObject.SetActive(false);
                selectModalPanel.SetActive(false);
            });

            // キャンセルボタン
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                callback(new List<T>());
                confirmButton.gameObject.SetActive(false);
                selectionCountText.gameObject.SetActive(false);
                selectModalPanel.SetActive(false);
            });

            // テンプレート非表示
            optionButtonTemplate.SetActive(false);

            // パネル表示
            selectModalPanel.SetActive(true);
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
