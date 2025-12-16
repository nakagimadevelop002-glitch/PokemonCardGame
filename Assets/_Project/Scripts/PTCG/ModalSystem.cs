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

        // スクロールUI閾値（選択肢がこの数以上でスクロールUI使用）
        private const int SCROLL_THRESHOLD = 3;

        // SelectModal UI要素キャッシュ（非アクティブでも参照可能）
        private GameObject selectModalPanel;
        private Text titleText;
        private Transform optionsContainer;
        private Button cancelButton;
        private GameObject optionButtonTemplate;
        private Button confirmButton; // 複数選択時の確認ボタン
        private Text selectionCountText; // 選択数表示

        // ScrollModal UI要素キャッシュ（スクロール版モーダル）
        private GameObject selectModalScrollPanel;
        private Text scrollTitleText;
        private Transform scrollContent; // ScrollContent（VerticalLayoutGroup付き）
        private Button scrollCancelButton;
        private GameObject scrollOptionTemplate;
        private Button scrollConfirmButton;
        private Text scrollSelectionCountText;

        // ConfirmModal UI要素キャッシュ
        private GameObject confirmModalPanel;
        private Text confirmTitleText;
        private Text confirmMessageText;
        private Button yesButton;
        private Button noButton;

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

                if (canvas != null && raycaster == null)
                {
                    raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                }
                else if (canvas == null)
                {
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

                // CancelButton配下のText要素を名前で取得してフォント設定と日本語テキスト設定
                if (cancelButton != null && correctFont != null)
                {
                    Text[] cancelTexts = selectModalPanel.GetComponentsInChildren<Text>(true);
                    foreach (var text in cancelTexts)
                    {
                        if (text.transform.parent == cancelButton.transform)
                        {
                            text.font = correctFont;
                            // 文字化け対策: C#スクリプト内で日本語テキスト設定
                            text.text = "";
                            text.text = "キャンセル";
                        }
                    }
                }

                // SelectModalPanel初期非表示
                selectModalPanel.SetActive(false);
            }
            else
            {
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

                        // ConfirmButton配下のText要素に日本語テキスト設定（文字化け対策）
                        if (correctFont != null)
                        {
                            Text confirmText = confirmButton.GetComponentInChildren<Text>(true);
                            if (confirmText != null)
                            {
                                confirmText.font = correctFont;
                                confirmText.text = "";
                                confirmText.text = "決定";
                            }
                        }
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

            // ConfirmModalPanel UI要素をキャッシュ
            confirmModalPanel = GameObject.Find("ConfirmModalPanel");
            if (confirmModalPanel != null)
            {
                Transform modalWindow = confirmModalPanel.transform.Find("ModalWindow");
                if (modalWindow != null)
                {
                    confirmTitleText = modalWindow.Find("TitleText")?.GetComponent<Text>();
                    confirmMessageText = modalWindow.Find("MessageText")?.GetComponent<Text>();
                    yesButton = modalWindow.Find("YesButton")?.GetComponent<Button>();
                    noButton = modalWindow.Find("NoButton")?.GetComponent<Button>();
                }

                confirmModalPanel.SetActive(false);
            }
            else
            {
            }

            // ScrollModalPanel UI要素をキャッシュ
            selectModalScrollPanel = GameObject.Find("SelectModalScrollPanel");
            if (selectModalScrollPanel != null)
            {
                // フォント取得（SelectModalPanelと同じフォントを使用）
                Font correctFont = null;
                if (titleText != null && titleText.font != null)
                {
                    correctFont = titleText.font;
                }

                Transform scrollModalWindow = selectModalScrollPanel.transform.Find("ScrollModalWindow");
                if (scrollModalWindow != null)
                {
                    // ScrollTitleText
                    scrollTitleText = scrollModalWindow.Find("ScrollTitleText")?.GetComponent<Text>();
                    if (scrollTitleText != null && correctFont != null)
                    {
                        scrollTitleText.font = correctFont;
                    }

                    // ScrollView > Viewport > ScrollContent
                    Transform scrollView = scrollModalWindow.Find("ScrollView");
                    if (scrollView != null)
                    {
                        Transform viewport = scrollView.Find("Viewport");
                        if (viewport != null)
                        {
                            scrollContent = viewport.Find("ScrollContent");
                            if (scrollContent != null)
                            {
                                // ScrollOptionTemplate
                                scrollOptionTemplate = scrollContent.Find("ScrollOptionTemplate")?.gameObject;
                                if (scrollOptionTemplate != null)
                                {
                                    scrollOptionTemplate.SetActive(false); // 初期非表示
                                    Text btnText = scrollOptionTemplate.GetComponentInChildren<Text>(true);
                                    if (btnText != null && correctFont != null)
                                    {
                                        btnText.font = correctFont;
                                    }
                                }
                            }
                        }
                    }

                    // ScrollCancelButton
                    scrollCancelButton = scrollModalWindow.Find("ScrollCancelButton")?.GetComponent<Button>();
                    if (scrollCancelButton != null && correctFont != null)
                    {
                        Text cancelText = scrollCancelButton.GetComponentInChildren<Text>(true);
                        if (cancelText != null)
                        {
                            cancelText.font = correctFont;
                            cancelText.text = "";
                            cancelText.text = "キャンセル";
                        }
                    }

                    // ScrollConfirmButton
                    scrollConfirmButton = scrollModalWindow.Find("ScrollConfirmButton")?.GetComponent<Button>();
                    if (scrollConfirmButton != null)
                    {
                        scrollConfirmButton.gameObject.SetActive(false); // 初期非表示（複数選択時のみ表示）
                        if (correctFont != null)
                        {
                            Text confirmText = scrollConfirmButton.GetComponentInChildren<Text>(true);
                            if (confirmText != null)
                            {
                                confirmText.font = correctFont;
                                confirmText.text = "";
                                confirmText.text = "決定";
                            }
                        }
                    }

                    // ScrollSelectionCountText
                    scrollSelectionCountText = scrollModalWindow.Find("ScrollSelectionCountText")?.GetComponent<Text>();
                    if (scrollSelectionCountText != null)
                    {
                        if (correctFont != null) scrollSelectionCountText.font = correctFont;
                        scrollSelectionCountText.gameObject.SetActive(false); // 初期非表示（複数選択時のみ表示）
                    }
                }

                selectModalScrollPanel.SetActive(false); // 初期非表示
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
            if (confirmModalPanel == null || confirmTitleText == null || confirmMessageText == null ||
                yesButton == null || noButton == null)
            {
                callback(true);
                return;
            }

            // UI要素設定
            confirmTitleText.text = title;
            confirmMessageText.text = message;

            // ボタンクリックリスナー設定
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                confirmModalPanel.SetActive(false);
                callback(true);
            });

            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() =>
            {
                confirmModalPanel.SetActive(false);
                callback(false);
            });

            // モーダル表示
            confirmModalPanel.SetActive(true);
        }


        /// <summary>
        /// 単一選択モーダルUI表示
        /// </summary>
        private void ShowSelectModal<T>(string title, List<SelectOption<T>> options, Action<T> callback)
        {
            if (options.Count >= SCROLL_THRESHOLD)
            {
                ShowSelectModalScroll(title, options, callback);
                return;
            }

            // ★既存のコードはそのまま（一切変更しない）★
            // キャッシュからUI要素取得
            if (selectModalPanel == null)
            {
                callback(default(T));
                return;
            }

            if (titleText == null || optionsContainer == null || cancelButton == null || optionButtonTemplate == null)
            {
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
            Debug.Log("[DEBUG] ShowSelectModalMulti called - options.Count: " + options.Count + ", SCROLL_THRESHOLD: " + SCROLL_THRESHOLD);

            if (options.Count >= SCROLL_THRESHOLD)
            {
                Debug.Log("[DEBUG] Using SCROLL UI (options.Count >= SCROLL_THRESHOLD)");
                ShowSelectModalMultiScroll(title, options, maxCount, callback);
                return;
            }

            Debug.Log("[DEBUG] Using FIXED UI (options.Count < SCROLL_THRESHOLD)");

            // ★既存のコードはそのまま（一切変更しない）★
            if (selectModalPanel == null || titleText == null || optionsContainer == null ||
                optionButtonTemplate == null || confirmButton == null || selectionCountText == null)
            {
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
                }
                else
                {
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

                    if (isOn && !selectedValues.Contains(option.value))
                    {
                        if (selectedValues.Count < maxCount)
                        {
                            selectedValues.Add(option.value);
                        }
                        else
                        {
                            toggle.isOn = false;
                        }
                    }
                    else if (!isOn && selectedValues.Contains(option.value))
                    {
                        selectedValues.Remove(option.value);
                    }

                    // 選択数表示更新
                    selectionCountText.text = selectedValues.Count + " / " + maxCount + "個選択";

                    // 決定ボタンの有効/無効
                    confirmButton.interactable = selectedValues.Count > 0 && selectedValues.Count <= maxCount;
                });

                // Y位置設定
                RectTransform rt = toggleObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, -i * 60);
                }
            }


            // 初期状態: 選択数0
            selectionCountText.text = "0 / " + maxCount + "個選択";
            selectionCountText.gameObject.SetActive(true); // 複数選択時は表示
            confirmButton.interactable = false;
            confirmButton.gameObject.SetActive(true); // 複数選択時は表示


            // 決定ボタン
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => {
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

        /// <summary>
        /// スクロール版単一選択モーダルUI表示
        /// </summary>
        private void ShowSelectModalScroll<T>(string title, List<SelectOption<T>> options, Action<T> callback)
        {
            if (selectModalScrollPanel == null || scrollTitleText == null || scrollContent == null ||
                scrollCancelButton == null || scrollOptionTemplate == null)
            {
                callback(default(T));
                return;
            }

            // タイトル設定
            scrollTitleText.text = title;

            // 既存ボタンクリア（テンプレート以外）
            foreach (Transform child in scrollContent)
            {
                if (child.gameObject != scrollOptionTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            // 正しいフォントを取得
            Font correctFont = scrollTitleText.font;

            // ボタン動的生成（VerticalLayoutGroupが自動配置）
            for (int i = 0; i < options.Count; i++)
            {
                SelectOption<T> option = options[i];
                GameObject btn = Instantiate(scrollOptionTemplate, scrollContent);
                btn.name = "ScrollOption_" + i;
                btn.SetActive(true);

                Text btnText = btn.GetComponentInChildren<Text>(true);
                if (btnText != null)
                {
                    if (correctFont != null) btnText.font = correctFont;
                    string displayText = option.label;
                    if (option.disabled && !string.IsNullOrEmpty(option.disabledReason))
                    {
                        displayText += " (" + option.disabledReason + ")";
                    }
                    btnText.text = displayText;

                    if (option.disabled) btnText.color = Color.gray;
                }

                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = !option.disabled;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        selectModalScrollPanel.SetActive(false);
                        callback(option.value);
                    });
                }
            }

            // キャンセルボタン
            scrollCancelButton.onClick.RemoveAllListeners();
            scrollCancelButton.onClick.AddListener(() => {
                selectModalScrollPanel.SetActive(false);
                callback(default(T));
            });

            // テンプレート非表示
            scrollOptionTemplate.SetActive(false);

            // パネル表示
            selectModalScrollPanel.SetActive(true);
        }

        /// <summary>
        /// スクロール版複数選択モーダルUI表示
        /// </summary>
        private void ShowSelectModalMultiScroll<T>(string title, List<SelectOption<T>> options, int maxCount, Action<List<T>> callback)
        {
            Debug.Log("[DEBUG] ShowSelectModalMultiScroll called - options.Count: " + options.Count);
            Debug.Log("[DEBUG] Null check - selectModalScrollPanel: " + (selectModalScrollPanel == null) +
                      ", scrollTitleText: " + (scrollTitleText == null) +
                      ", scrollContent: " + (scrollContent == null) +
                      ", scrollOptionTemplate: " + (scrollOptionTemplate == null) +
                      ", scrollConfirmButton: " + (scrollConfirmButton == null) +
                      ", scrollSelectionCountText: " + (scrollSelectionCountText == null));

            if (selectModalScrollPanel == null || scrollTitleText == null || scrollContent == null ||
                scrollOptionTemplate == null || scrollConfirmButton == null || scrollSelectionCountText == null)
            {
                Debug.LogError("[DEBUG] Null check FAILED - returning empty list");
                callback(new List<T>());
                return;
            }

            Debug.Log("[DEBUG] Null check PASSED - proceeding with UI generation");

            // タイトル設定
            scrollTitleText.text = title;

            // 既存ボタンクリア（テンプレート以外）
            foreach (Transform child in scrollContent)
            {
                if (child.gameObject != scrollOptionTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            Font correctFont = scrollTitleText.font;
            List<T> selectedValues = new List<T>();

            Debug.Log("[DEBUG] Starting option generation loop - options.Count: " + options.Count);

            // トグルボタン動的生成（VerticalLayoutGroupが自動配置）
            for (int i = 0; i < options.Count; i++)
            {
                SelectOption<T> option = options[i];
                Debug.Log("[DEBUG] Generating option " + i + ": " + option.label);
                GameObject toggleObj = Instantiate(scrollOptionTemplate, scrollContent);
                toggleObj.name = "ScrollOption_" + i;
                toggleObj.SetActive(true);
                Debug.Log("[DEBUG] Option " + i + " instantiated and activated");

                // Button削除
                Button existingButton = toggleObj.GetComponent<Button>();
                if (existingButton != null) DestroyImmediate(existingButton);

                // Toggle追加
                UnityEngine.UI.Toggle toggle = toggleObj.GetComponent<UnityEngine.UI.Toggle>();
                if (toggle == null)
                {
                    toggle = toggleObj.AddComponent<UnityEngine.UI.Toggle>();
                    toggle.isOn = false;
                }

                // targetGraphic設定
                Image toggleImage = toggleObj.GetComponent<Image>();
                if (toggleImage != null)
                {
                    toggle.targetGraphic = toggleImage;
                    toggleImage.raycastTarget = true;
                }

                // Checkmark作成
                GameObject checkmarkObj = new GameObject("Checkmark");
                checkmarkObj.transform.SetParent(toggleObj.transform, false);
                Image checkmarkImage = checkmarkObj.AddComponent<Image>();
                checkmarkImage.color = new Color(0.2f, 1f, 0.2f, 1f);

                RectTransform checkmarkRT = checkmarkObj.GetComponent<RectTransform>();
                checkmarkRT.anchorMin = new Vector2(0.9f, 0.1f);
                checkmarkRT.anchorMax = new Vector2(0.95f, 0.9f);
                checkmarkRT.offsetMin = Vector2.zero;
                checkmarkRT.offsetMax = Vector2.zero;

                checkmarkImage.gameObject.SetActive(false);
                toggle.graphic = checkmarkImage;

                // Toggle transitions設定
                toggle.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                ColorBlock colors = toggle.colors;
                colors.normalColor = new Color(1f, 1f, 1f, 1f);
                colors.highlightedColor = new Color(0.8f, 0.8f, 1f, 1f);
                colors.pressedColor = new Color(0.6f, 0.6f, 1f, 1f);
                colors.selectedColor = new Color(0.8f, 1f, 0.8f, 1f);
                toggle.colors = colors;

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
                    if (isOn && !selectedValues.Contains(option.value))
                    {
                        if (selectedValues.Count < maxCount)
                        {
                            selectedValues.Add(option.value);
                        }
                        else
                        {
                            toggle.isOn = false;
                        }
                    }
                    else if (!isOn && selectedValues.Contains(option.value))
                    {
                        selectedValues.Remove(option.value);
                    }

                    scrollSelectionCountText.text = selectedValues.Count + " / " + maxCount + "個選択";
                    scrollConfirmButton.interactable = selectedValues.Count > 0 && selectedValues.Count <= maxCount;
                });
            }

            // 初期状態: 選択数0
            scrollSelectionCountText.text = "0 / " + maxCount + "個選択";
            scrollSelectionCountText.gameObject.SetActive(true);
            scrollConfirmButton.interactable = false;
            scrollConfirmButton.gameObject.SetActive(true);

            // 決定ボタン
            scrollConfirmButton.onClick.RemoveAllListeners();
            scrollConfirmButton.onClick.AddListener(() => {
                callback(new List<T>(selectedValues));
                scrollConfirmButton.gameObject.SetActive(false);
                scrollSelectionCountText.gameObject.SetActive(false);
                selectModalScrollPanel.SetActive(false);
            });

            // キャンセルボタン
            scrollCancelButton.onClick.RemoveAllListeners();
            scrollCancelButton.onClick.AddListener(() => {
                callback(new List<T>());
                scrollConfirmButton.gameObject.SetActive(false);
                scrollSelectionCountText.gameObject.SetActive(false);
                selectModalScrollPanel.SetActive(false);
            });

            // テンプレート非表示
            scrollOptionTemplate.SetActive(false);

            // パネル表示
            selectModalScrollPanel.SetActive(true);
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
