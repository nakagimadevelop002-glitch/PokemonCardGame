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
        public bool autoSelectMode = true; // UI未実装時は自動選択

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // SelectModalPanel初期非表示
            GameObject panel = GameObject.Find("SelectModalPanel");
            if (panel != null)
            {
                panel.SetActive(false);
            }

            // OptionButtonTemplate初期非表示
            GameObject template = GameObject.Find("OptionButtonTemplate");
            if (template != null)
            {
                template.SetActive(false);
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
            GameObject panel = GameObject.Find("SelectModalPanel");
            if (panel == null)
            {
                Debug.LogError("[ModalSystem] SelectModalPanel not found");
                callback(default(T));
                return;
            }

            Text titleText = GameObject.Find("TitleText")?.GetComponent<Text>();
            Transform optionsContainer = GameObject.Find("OptionsContainer")?.transform;
            Button cancelButton = GameObject.Find("CancelButton")?.GetComponent<Button>();
            GameObject buttonTemplate = GameObject.Find("OptionButtonTemplate");

            if (titleText == null || optionsContainer == null || cancelButton == null || buttonTemplate == null)
            {
                Debug.LogError("[ModalSystem] UI elements not found");
                callback(default(T));
                return;
            }

            // タイトル設定
            titleText.text = title;

            // 既存ボタンクリア（テンプレート以外）
            foreach (Transform child in optionsContainer)
            {
                if (child.gameObject != buttonTemplate)
                {
                    Destroy(child.gameObject);
                }
            }

            // ボタン動的生成
            for (int i = 0; i < options.Count; i++)
            {
                SelectOption<T> option = options[i];
                GameObject btn = Instantiate(buttonTemplate, optionsContainer);
                btn.name = "Option_" + i;
                btn.SetActive(true);

                Text btnText = btn.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = option.label;
                }

                Button button = btn.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        callback(option.value);
                        panel.SetActive(false);
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
                panel.SetActive(false);
            });

            // テンプレート非表示
            buttonTemplate.SetActive(false);

            // パネル表示
            panel.SetActive(true);

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

        public SelectOption(string label, T value)
        {
            this.label = label;
            this.value = value;
        }
    }
}
