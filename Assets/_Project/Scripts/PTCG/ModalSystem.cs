using System;
using System.Collections.Generic;
using UnityEngine;

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
                // TODO: UI実装時にここでUIを表示
                Debug.Log($"[ModalSystem] {title}: UI未実装 - 自動選択");
                callback(options[0].value);
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
