using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// トレーナーズカード（サポート、グッズ、スタジアム）の実行システム
    /// </summary>
    public class CardPlaySystem : MonoBehaviour
    {
        public static CardPlaySystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool PlayCard(PlayerController player, CardData card)
        {
            if (card is TrainerCardData trainer)
            {
                return PlayTrainerCard(player, trainer);
            }
            return false;
        }

        private bool PlayTrainerCard(PlayerController player, TrainerCardData trainer)
        {
            // サポートチェック
            if (trainer.trainerType == TrainerType.Supporter)
            {
                if (player.supporterUsedThisTurn)
                {
                    Debug.Log("このターンはサポート済み");
                    return false;
                }
                // 先攻1Tはサポート不可
                var gm = GameManager.Instance;
                if (gm.currentPlayerIndex == gm.firstPlayerIndex && gm.turnCount == 1)
                {
                    Debug.Log("先攻1Tはサポート不可");
                    return false;
                }
            }

            // 手札から削除してトラッシュへ
            if (!player.hand.Remove(trainer))
            {
                Debug.LogError($"{trainer.cardName} が手札にありません");
                return false;
            }
            player.discard.Add(trainer);

            Debug.Log($"{player.playerName}: {trainer.trainerType} 《{trainer.cardName}》");

            // カード効果を実行
            bool success = ExecuteTrainerEffect(player, trainer);

            if (success && trainer.trainerType == TrainerType.Supporter)
            {
                player.supporterUsedThisTurn = true;
            }

            return success;
        }

        private bool ExecuteTrainerEffect(PlayerController player, TrainerCardData trainer)
        {
            var gm = GameManager.Instance;
            var opponent = player == gm.player1 ? gm.player2 : gm.player1;

            switch (trainer.cardID)
            {
                // ========= サポート =========
                case "Research": // 博士の研究
                    {
                        int handCount = player.hand.Count;
                        player.discard.AddRange(player.hand);
                        player.hand.Clear();
                        player.Draw(7);
                        Debug.Log($"→ 博士：手札{handCount} → 全トラッシュ／7枚ドロー");
                        return true;
                    }

                case "Iono": // ナンジャモ
                    {
                        int p1Count = gm.player1.prizes.Count;
                        int p2Count = gm.player2.prizes.Count;

                        // 手札を山札に戻す
                        gm.player1.deck.AddRange(gm.player1.hand);
                        gm.player1.hand.Clear();
                        gm.player2.deck.AddRange(gm.player2.hand);
                        gm.player2.hand.Clear();

                        gm.player1.ShuffleDeck();
                        gm.player2.ShuffleDeck();

                        gm.player1.Draw(p1Count);
                        gm.player2.Draw(p2Count);

                        Debug.Log($"→ ナンジャモ：Player1={p1Count} / Player2={p2Count} 枚ドロー");
                        return true;
                    }

                case "Boss": // ボスの指令
                    {
                        if (opponent.benchSlots.Count == 0)
                        {
                            Debug.Log("→ 相手のベンチなし");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ ボスの指令（モーダル選択未実装）");
                        return true;
                    }

                case "Pepper": // ペパー
                    {
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ ペパー（モーダル選択未実装）");
                        return true;
                    }

                // ========= グッズ =========
                case "NestBall": // ネストボール
                    {
                        if (player.benchSlots.Count >= 5)
                        {
                            Debug.Log("→ ベンチがいっぱい");
                            return true;
                        }
                        var basics = player.deck.OfType<PokemonCardData>()
                            .Where(p => p.stage == PokemonStage.Basic).ToList();
                        if (basics.Count == 0)
                        {
                            Debug.Log("→ 対象なし");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ ネストボール（モーダル選択未実装）");
                        return true;
                    }

                case "LevelBall": // レベルボール
                    {
                        var candidates = player.deck.OfType<PokemonCardData>()
                            .Where(p => p.baseHP <= 90).ToList();
                        if (candidates.Count == 0)
                        {
                            Debug.Log("→ 対象なし");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ レベルボール（モーダル選択未実装）");
                        return true;
                    }

                case "HyperBall": // ハイパーボール
                    {
                        if (player.hand.Count < 2)
                        {
                            Debug.Log("→ 手札が2枚未満");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ ハイパーボール（モーダル選択未実装）");
                        return true;
                    }

                case "RareCandy": // ふしぎなアメ
                    {
                        // TODO: 進化システム実装後に追加
                        Debug.Log("→ ふしぎなアメ（進化システム未実装）");
                        return true;
                    }

                case "EarthenVessel": // 大地の器
                    {
                        if (player.hand.Count == 0)
                        {
                            Debug.Log("→ 手札がありません");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ 大地の器（モーダル選択未実装）");
                        return true;
                    }

                case "EscapeRope": // あなぬけのヒモ
                    {
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ あなぬけのヒモ（モーダル選択未実装）");
                        return true;
                    }

                case "SuperRod": // すごいつりざお
                    {
                        var pool = player.discard.Where(c =>
                            c is PokemonCardData ||
                            (c is EnergyCardData e && e.isBasic)
                        ).ToList();
                        if (pool.Count == 0)
                        {
                            Debug.Log("→ トラッシュに対象なし");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ すごいつりざお（モーダル選択未実装）");
                        return true;
                    }

                case "CounterCatcher": // カウンターキャッチャー
                    {
                        if (player.prizes.Count <= opponent.prizes.Count)
                        {
                            Debug.Log("→ サイドが相手より多くないため使えません");
                            return true;
                        }
                        if (opponent.benchSlots.Count == 0)
                        {
                            Debug.Log("→ 相手ベンチなし");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ カウンターキャッチャー（モーダル選択未実装）");
                        return true;
                    }

                case "LostSweeper": // ロストスイーパー
                    {
                        if (player.hand.Count == 0)
                        {
                            Debug.Log("→ コスト支払い用の手札がありません");
                            return true;
                        }
                        // TODO: モーダル選択システム実装後に追加
                        Debug.Log("→ ロストスイーパー（モーダル選択未実装）");
                        return true;
                    }

                // ========= スタジアム =========
                case "Artazon": // ボウルタウン
                case "BeachCourt": // ビーチコート
                    {
                        return PlayStadium(player, trainer);
                    }

                default:
                    Debug.LogWarning($"未実装のトレーナーカード: {trainer.cardID}");
                    return false;
            }
        }

        private bool PlayStadium(PlayerController player, TrainerCardData stadium)
        {
            var gm = GameManager.Instance;

            // 既存のスタジアムを上書き
            if (!string.IsNullOrEmpty(gm.stadiumInPlay))
            {
                Debug.Log($"→ 既存のスタジアム《{gm.stadiumInPlay}》を破棄");
            }

            gm.stadiumInPlay = stadium.cardID;
            Debug.Log($"→ スタジアム《{stadium.cardName}》を場に出しました");

            // スタジアム効果は常駐効果のため、ここでは設置のみ
            return true;
        }
    }
}
