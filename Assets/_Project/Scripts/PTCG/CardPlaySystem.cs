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

                        // モーダル選択：相手のベンチポケモンを選択
                        var benchOptions = opponent.benchSlots.Select(p => new SelectOption<PokemonInstance>(
                            p.data.cardName,
                            p
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "相手ベンチを前へ（公開）",
                            benchOptions,
                            (selectedPokemon) =>
                            {
                                if (selectedPokemon == null)
                                {
                                    Debug.Log("選択がありません");
                                    return;
                                }

                                // ベンチから選択されたポケモンを削除
                                int benchIndex = opponent.benchSlots.IndexOf(selectedPokemon);
                                if (benchIndex < 0)
                                {
                                    Debug.LogError("選択されたポケモンがベンチに見つかりません");
                                    return;
                                }

                                opponent.benchSlots.RemoveAt(benchIndex);

                                // 現在のバトル場ポケモンをベンチの先頭に移動
                                if (opponent.activeSlot != null)
                                {
                                    opponent.benchSlots.Insert(0, opponent.activeSlot);
                                }

                                // 選択されたポケモンをバトル場へ
                                opponent.activeSlot = selectedPokemon;

                                Debug.Log("→ ボス：呼び出し完了");
                                UIManager.Instance?.UpdateUI();
                            },
                            defaultFirst: true
                        );
                        return true;
                    }

                case "Pepper": // ペパー
                    {
                        // 山札からグッズとどうぐを抽出
                        var items = player.deck.OfType<TrainerCardData>()
                            .Where(t => t.trainerType == TrainerType.Item).ToList();
                        var tools = player.deck.OfType<TrainerCardData>()
                            .Where(t => t.trainerType == TrainerType.Tool).ToList();

                        if (items.Count == 0)
                        {
                            Debug.Log("→ グッズが山札にありません");
                            return true;
                        }

                        // 第1段階：グッズを1枚選択（必須）
                        var itemOptions = items.Select(card => new SelectOption<TrainerCardData>(
                            card.cardName,
                            card
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "ペパー：グッズを1枚選択（必須）",
                            itemOptions,
                            (selectedItem) =>
                            {
                                if (selectedItem == null)
                                {
                                    Debug.Log("→ グッズが選ばれていません");
                                    return;
                                }

                                // グッズを手札へ
                                player.deck.Remove(selectedItem);
                                player.hand.Add(selectedItem);

                                // 第2段階：どうぐを1枚選択（山札にあればのみ）
                                if (tools.Count > 0)
                                {
                                    var toolOptions = tools.Select(card => new SelectOption<TrainerCardData>(
                                        card.cardName,
                                        card
                                    )).ToList();

                                    ModalSystem.Instance.OpenSelectModal(
                                        "ペパー：どうぐを1枚選択（スキップ可）",
                                        toolOptions,
                                        (selectedTool) =>
                                        {
                                            TrainerCardData pickedTool = null;
                                            if (selectedTool != null)
                                            {
                                                player.deck.Remove(selectedTool);
                                                player.hand.Add(selectedTool);
                                                pickedTool = selectedTool;
                                            }

                                            player.ShuffleDeck();
                                            var toolText = pickedTool != null ? $" ＋ どうぐ《{pickedTool.cardName}》" : "（どうぐスキップ）";
                                            Debug.Log($"→ 公開：グッズ《{selectedItem.cardName}》{toolText} を手札に");
                                            UIManager.Instance?.UpdateUI();
                                        },
                                        defaultFirst: true
                                    );
                                }
                                else
                                {
                                    // どうぐが山札にない場合
                                    player.ShuffleDeck();
                                    Debug.Log($"→ 公開：グッズ《{selectedItem.cardName}》（どうぐ該当なし） を手札に");
                                    UIManager.Instance?.UpdateUI();
                                }
                            },
                            defaultFirst: true
                        );
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

                        // モーダル選択
                        var options = basics.Select(card => new SelectOption<PokemonCardData>(
                            card.cardName,
                            card
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "ベンチに出す たね を選択（公開）",
                            options,
                            (selectedCard) =>
                            {
                                if (selectedCard == null)
                                {
                                    Debug.Log("選択がありません");
                                    return;
                                }

                                player.deck.Remove(selectedCard);
                                gm.SpawnPokemonToBench(player, selectedCard);
                                Debug.Log($"→ 公開：《{selectedCard.cardName}》をベンチへ");
                                UIManager.Instance?.UpdateUI();
                            },
                            defaultFirst: true
                        );
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

                        // モーダル選択（HP表示）
                        var options = candidates.Select(card => new SelectOption<PokemonCardData>(
                            $"{card.cardName}（HP{card.baseHP}）",
                            card
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "HP90以下のポケモンを手札に（公開）",
                            options,
                            (selectedCard) =>
                            {
                                if (selectedCard == null)
                                {
                                    Debug.Log("→ 何も選ばれていません（OKの前に1つ選んでください）");
                                    return;
                                }

                                player.deck.Remove(selectedCard);
                                player.hand.Add(selectedCard);
                                player.ShuffleDeck();
                                Debug.Log($"→ 公開：《{selectedCard.cardName}》を手札に");
                                UIManager.Instance?.UpdateUI();
                            },
                            defaultFirst: true
                        );
                        return true;
                    }

                case "HyperBall": // ハイパーボール
                    {
                        if (player.hand.Count < 2)
                        {
                            Debug.Log("→ 手札が2枚未満");
                            return true;
                        }

                        // 第1段階：手札から2枚トラッシュ（コスト）
                        var handOptions = player.hand.Select(card => new SelectOption<CardData>(
                            GetCardLabel(card),
                            card
                        )).ToList();

                        ModalSystem.Instance.OpenMultiSelectModal(
                            "手札から2枚トラッシュ（コスト）",
                            handOptions,
                            2,
                            (trashCards) =>
                            {
                                if (trashCards == null || trashCards.Count != 2)
                                {
                                    Debug.Log("→ 2枚選択してください");
                                    return;
                                }

                                // トラッシュへ移動
                                foreach (var card in trashCards)
                                {
                                    player.hand.Remove(card);
                                    player.discard.Add(card);
                                }

                                // 第2段階：山札からポケモン1枚
                                var pokemons = player.deck.OfType<PokemonCardData>().ToList();
                                if (pokemons.Count == 0)
                                {
                                    Debug.Log("→ 山札にポケモンなし");
                                    return;
                                }

                                var pokemonOptions = pokemons.Select(card => new SelectOption<PokemonCardData>(
                                    GetCardLabel(card),
                                    card
                                )).ToList();

                                ModalSystem.Instance.OpenSelectModal(
                                    "山札からポケモン1枚（公開）",
                                    pokemonOptions,
                                    (selectedPokemon) =>
                                    {
                                        if (selectedPokemon == null)
                                        {
                                            Debug.Log("選択がありません");
                                            return;
                                        }

                                        player.deck.Remove(selectedPokemon);
                                        player.hand.Add(selectedPokemon);
                                        player.ShuffleDeck();
                                        Debug.Log($"→ 公開：《{selectedPokemon.cardName}》を手札に");
                                        UIManager.Instance?.UpdateUI();
                                    },
                                    defaultFirst: true
                                );
                            }
                        );
                        return true;
                    }

                case "RareCandy": // ふしぎなアメ
                    {
                        // 場のたねポケモンを取得
                        var allBasics = player.GetAllPokemons()
                            .Where(p => p.data.stage == PokemonStage.Basic).ToList();

                        if (allBasics.Count == 0)
                        {
                            Debug.Log("→ 場にたねポケモンがいません");
                            return true;
                        }

                        // 手札の2進化カードを取得
                        var stage2Cards = player.hand.OfType<PokemonCardData>()
                            .Where(p => p.stage == PokemonStage.Stage2).ToList();

                        if (stage2Cards.Count == 0)
                        {
                            Debug.Log("→ 手札に2進化カードがありません");
                            return true;
                        }

                        // 進化可能なたねポケモンをフィルタ
                        var validBasics = new List<PokemonInstance>();
                        foreach (var basic in allBasics)
                        {
                            // このたねから進化できる2進化カードが手札にあるか
                            bool hasValidStage2 = stage2Cards.Any(s2 =>
                            {
                                // 進化系統チェック（中間進化の存在を確認）
                                // evolvesFromが中間進化の名前になっているはず
                                // 簡易実装: evolvesFromフィールドを信頼
                                return !string.IsNullOrEmpty(s2.evolvesFrom);
                            });

                            if (hasValidStage2)
                            {
                                validBasics.Add(basic);
                            }
                        }

                        if (validBasics.Count == 0)
                        {
                            Debug.Log("→ 場に有効な たね がいない、または手札に対応する2進化カードがありません");
                            return true;
                        }

                        // モーダル選択：進化させるたねを選択
                        var basicOptions = validBasics.Select(p => new SelectOption<PokemonInstance>(
                            p.data.cardName,
                            p
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "進化させる たね を選択",
                            basicOptions,
                            (selectedBasic) =>
                            {
                                if (selectedBasic == null)
                                {
                                    Debug.Log("選択がありません");
                                    return;
                                }

                                // 手札から対応する2進化カードを選択
                                var compatibleStage2 = stage2Cards.Where(s2 =>
                                    !string.IsNullOrEmpty(s2.evolvesFrom)
                                ).ToList();

                                if (compatibleStage2.Count == 0)
                                {
                                    Debug.Log("→ 対応する2進化カードが見つかりません");
                                    return;
                                }

                                var stage2Options = compatibleStage2.Select(s2 => new SelectOption<PokemonCardData>(
                                    s2.cardName,
                                    s2
                                )).ToList();

                                ModalSystem.Instance.OpenSelectModal(
                                    "進化先の2進化カードを選択",
                                    stage2Options,
                                    (selectedStage2) =>
                                    {
                                        if (selectedStage2 == null)
                                        {
                                            Debug.Log("選択がありません");
                                            return;
                                        }

                                        // EvolutionSystem経由で進化
                                        bool success = EvolutionSystem.Instance.Evolve(
                                            player,
                                            selectedBasic,
                                            selectedStage2,
                                            viaRareCandy: true
                                        );

                                        if (success)
                                        {
                                            // 手札から2進化カードを削除
                                            player.hand.Remove(selectedStage2);
                                            UIManager.Instance?.UpdateUI();
                                        }
                                        else
                                        {
                                            Debug.Log("→ 進化に失敗（対応する系統ではありません）");
                                        }
                                    },
                                    defaultFirst: true
                                );
                            },
                            defaultFirst: true
                        );
                        return true;
                    }

                case "EarthenVessel": // 大地の器
                    {
                        if (player.hand.Count == 0)
                        {
                            Debug.Log("→ 手札がありません");
                            return true;
                        }

                        // モーダル選択：手札から1枚トラッシュ（コスト）
                        var handOptions = player.hand.Select(card => new SelectOption<CardData>(
                            GetCardLabel(card),
                            card
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "手札から1枚トラッシュ（コスト）",
                            handOptions,
                            (selectedCard) =>
                            {
                                if (selectedCard == null)
                                {
                                    Debug.Log("選択がありません");
                                    return;
                                }

                                // 手札からトラッシュへ
                                player.hand.Remove(selectedCard);
                                player.discard.Add(selectedCard);

                                // 山札から基本超エネルギーを最大2枚探す
                                var basicPsychicEnergies = new List<EnergyCardData>();
                                foreach (var card in player.deck.OfType<EnergyCardData>())
                                {
                                    if (card.cardID == "BasicPsychic" && basicPsychicEnergies.Count < 2)
                                    {
                                        basicPsychicEnergies.Add(card);
                                    }
                                }

                                // 見つかった分だけ手札に追加
                                foreach (var energy in basicPsychicEnergies)
                                {
                                    player.deck.Remove(energy);
                                    player.hand.Add(energy);
                                }

                                player.ShuffleDeck();
                                Debug.Log($"→ 基本超エネルギー {basicPsychicEnergies.Count}枚 を手札に");
                                UIManager.Instance?.UpdateUI();
                            },
                            defaultFirst: true
                        );
                        return true;
                    }

                case "EscapeRope": // あなぬけのヒモ
                    {
                        // 第1段階：相手のベンチがあれば交代させる
                        if (opponent.benchSlots.Count > 0)
                        {
                            var oppBenchOptions = opponent.benchSlots.Select(p => new SelectOption<PokemonInstance>(
                                p.data.cardName,
                                p
                            )).ToList();

                            ModalSystem.Instance.OpenSelectModal(
                                "相手の交代先を選択（先に選びます）",
                                oppBenchOptions,
                                (selectedOppPokemon) =>
                                {
                                    if (selectedOppPokemon == null)
                                    {
                                        Debug.Log("選択がありません");
                                        return;
                                    }

                                    // 相手を交代させる
                                    int oppBenchIndex = opponent.benchSlots.IndexOf(selectedOppPokemon);
                                    if (oppBenchIndex >= 0)
                                    {
                                        opponent.benchSlots.RemoveAt(oppBenchIndex);
                                        var prevOppActive = opponent.activeSlot;
                                        opponent.activeSlot = selectedOppPokemon;
                                        if (prevOppActive != null)
                                        {
                                            opponent.benchSlots.Insert(0, prevOppActive);
                                        }
                                        Debug.Log("→ あなぬけのヒモ：相手が交代");
                                    }

                                    // 第2段階：自分のベンチがあれば交代させる
                                    if (player.benchSlots.Count > 0)
                                    {
                                        var playerBenchOptions = player.benchSlots.Select(p => new SelectOption<PokemonInstance>(
                                            p.data.cardName,
                                            p
                                        )).ToList();

                                        ModalSystem.Instance.OpenSelectModal(
                                            "自分の交代先を選択",
                                            playerBenchOptions,
                                            (selectedPlayerPokemon) =>
                                            {
                                                if (selectedPlayerPokemon == null)
                                                {
                                                    Debug.Log("選択がありません");
                                                    return;
                                                }

                                                // 自分を交代させる
                                                int playerBenchIndex = player.benchSlots.IndexOf(selectedPlayerPokemon);
                                                if (playerBenchIndex >= 0)
                                                {
                                                    player.benchSlots.RemoveAt(playerBenchIndex);
                                                    var prevPlayerActive = player.activeSlot;
                                                    player.activeSlot = selectedPlayerPokemon;
                                                    if (prevPlayerActive != null)
                                                    {
                                                        player.benchSlots.Insert(0, prevPlayerActive);
                                                    }
                                                }

                                                UIManager.Instance?.UpdateUI();
                                            },
                                            defaultFirst: true
                                        );
                                    }
                                    else
                                    {
                                        UIManager.Instance?.UpdateUI();
                                    }
                                },
                                defaultFirst: true
                            );
                        }
                        else if (player.benchSlots.Count > 0)
                        {
                            // 相手のベンチがない場合、自分のみ交代
                            var playerBenchOptions = player.benchSlots.Select(p => new SelectOption<PokemonInstance>(
                                p.data.cardName,
                                p
                            )).ToList();

                            ModalSystem.Instance.OpenSelectModal(
                                "自分の交代先を選択",
                                playerBenchOptions,
                                (selectedPlayerPokemon) =>
                                {
                                    if (selectedPlayerPokemon == null)
                                    {
                                        Debug.Log("選択がありません");
                                        return;
                                    }

                                    int playerBenchIndex = player.benchSlots.IndexOf(selectedPlayerPokemon);
                                    if (playerBenchIndex >= 0)
                                    {
                                        player.benchSlots.RemoveAt(playerBenchIndex);
                                        var prevPlayerActive = player.activeSlot;
                                        player.activeSlot = selectedPlayerPokemon;
                                        if (prevPlayerActive != null)
                                        {
                                            player.benchSlots.Insert(0, prevPlayerActive);
                                        }
                                    }

                                    UIManager.Instance?.UpdateUI();
                                },
                                defaultFirst: true
                            );
                        }
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

                        // モーダル選択：トラッシュから最大3枚選択
                        var poolOptions = pool.Select(c => new SelectOption<CardData>(
                            GetCardLabel(c),
                            c
                        )).ToList();

                        ModalSystem.Instance.OpenMultiSelectModal(
                            "トラッシュから（最大3枚）山札にもどす",
                            poolOptions,
                            3,
                            (selectedCards) =>
                            {
                                if (selectedCards == null || selectedCards.Count == 0)
                                {
                                    return;
                                }

                                // トラッシュから削除して山札へ
                                foreach (var card in selectedCards)
                                {
                                    player.discard.Remove(card);
                                    player.deck.Add(card);
                                }

                                player.ShuffleDeck();
                                Debug.Log($"→ {selectedCards.Count}枚を山札にもどした");
                                UIManager.Instance?.UpdateUI();
                            }
                        );
                        return true;
                    }

                case "CounterCatcher": // カウンターキャッチャー
                    {
                        if (player.prizes.Count >= opponent.prizes.Count)
                        {
                            Debug.Log("→ サイドが相手より少なくないため使えません");
                            return true;
                        }
                        if (opponent.benchSlots.Count == 0)
                        {
                            Debug.Log("→ 相手ベンチなし");
                            return true;
                        }

                        // モーダル選択：相手のベンチポケモンを選択
                        var benchOptions = opponent.benchSlots.Select(p => new SelectOption<PokemonInstance>(
                            p.data.cardName,
                            p
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "相手ベンチを前へ（公開）",
                            benchOptions,
                            (selectedPokemon) =>
                            {
                                if (selectedPokemon == null)
                                {
                                    Debug.Log("選択がありません");
                                    return;
                                }

                                // ベンチから選択されたポケモンを削除
                                int benchIndex = opponent.benchSlots.IndexOf(selectedPokemon);
                                if (benchIndex < 0)
                                {
                                    Debug.LogError("選択されたポケモンがベンチに見つかりません");
                                    return;
                                }

                                opponent.benchSlots.RemoveAt(benchIndex);

                                // 現在のバトル場ポケモンをベンチの先頭に移動
                                if (opponent.activeSlot != null)
                                {
                                    opponent.benchSlots.Insert(0, opponent.activeSlot);
                                }

                                // 選択されたポケモンをバトル場へ
                                opponent.activeSlot = selectedPokemon;

                                Debug.Log("→ 呼び出し完了");
                                UIManager.Instance?.UpdateUI();
                            },
                            defaultFirst: true
                        );
                        return true;
                    }

                case "LostSweeper": // ロストスイーパー
                    {
                        if (player.hand.Count == 0)
                        {
                            Debug.Log("→ コスト支払い用の手札がありません");
                            return true;
                        }

                        // 第1段階：手札から1枚トラッシュ（コスト）
                        var handOptions = player.hand.Select(card => new SelectOption<CardData>(
                            GetCardLabel(card),
                            card
                        )).ToList();

                        ModalSystem.Instance.OpenSelectModal(
                            "手札から1枚トラッシュ（コスト）",
                            handOptions,
                            (selectedCard) =>
                            {
                                if (selectedCard == null)
                                {
                                    Debug.Log("選択がありません");
                                    return;
                                }

                                // 手札からトラッシュへ
                                player.hand.Remove(selectedCard);
                                player.discard.Add(selectedCard);

                                // 第2段階：除去対象をリストアップ
                                var targets = new List<SelectOption<string>>();

                                // スタジアムがあれば追加
                                if (!string.IsNullOrEmpty(gm.stadiumInPlay))
                                {
                                    targets.Add(new SelectOption<string>(
                                        $"場のスタジアム《{gm.stadiumInPlay}》",
                                        "stadium"
                                    ));
                                }

                                // 全プレイヤーの全ポケモンのどうぐをチェック
                                for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                                {
                                    var targetPlayer = playerIndex == 0 ? gm.player1 : gm.player2;
                                    var allPokemons = targetPlayer.GetAllPokemons();

                                    foreach (var pokemon in allPokemons)
                                    {
                                        if (pokemon.attachedTool != null)
                                        {
                                            targets.Add(new SelectOption<string>(
                                                $"{targetPlayer.playerName}の《{pokemon.data.cardName}》のどうぐ《{pokemon.attachedTool.cardName}》",
                                                $"tool:{playerIndex}:{pokemon.GetHashCode()}"
                                            ));
                                        }
                                    }
                                }

                                if (targets.Count == 0)
                                {
                                    Debug.Log("→ 除去できる対象がありません");
                                    return;
                                }

                                // モーダル選択：除去対象を選択
                                ModalSystem.Instance.OpenSelectModal(
                                    "ロストスイーパー：除去対象を選択（ロスト送り）",
                                    targets,
                                    (selectedTarget) =>
                                    {
                                        if (selectedTarget == null)
                                        {
                                            Debug.Log("選択がありません");
                                            return;
                                        }

                                        // スタジアム除去
                                        if (selectedTarget == "stadium")
                                        {
                                            var stadiumName = gm.stadiumInPlay;
                                            gm.stadiumInPlay = null;
                                            Debug.Log($"→ スタジアム《{stadiumName}》をロストゾーンへ");
                                        }
                                        // どうぐ除去
                                        else if (selectedTarget.StartsWith("tool:"))
                                        {
                                            var parts = selectedTarget.Split(':');
                                            int targetPlayerIndex = int.Parse(parts[1]);
                                            int pokemonHash = int.Parse(parts[2]);

                                            var targetPlayer = targetPlayerIndex == 0 ? gm.player1 : gm.player2;
                                            var allPokemons = targetPlayer.GetAllPokemons();

                                            foreach (var pokemon in allPokemons)
                                            {
                                                if (pokemon.GetHashCode() == pokemonHash && pokemon.attachedTool != null)
                                                {
                                                    var tool = pokemon.attachedTool;
                                                    pokemon.attachedTool = null;
                                                    targetPlayer.lostZone.Add(tool);
                                                    Debug.Log($"→ どうぐ《{tool.cardName}》をロストゾーンへ");
                                                    break;
                                                }
                                            }
                                        }

                                        UIManager.Instance?.UpdateUI();
                                    },
                                    defaultFirst: true
                                );
                            },
                            defaultFirst: true
                        );
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

        /// <summary>
        /// カード表示用ラベル生成（HTML版cardLabel関数に相当）
        /// </summary>
        private string GetCardLabel(CardData card)
        {
            if (card == null) return "不明なカード";

            if (card is EnergyCardData)
            {
                return card.cardName;
            }
            else if (card is TrainerCardData trainer)
            {
                return $"{card.cardName}（{trainer.trainerType}）";
            }
            else if (card is PokemonCardData pokemon)
            {
                return $"{card.cardName}（{pokemon.stage}・HP{pokemon.baseHP}）";
            }

            return card.cardName;
        }
    }
}
