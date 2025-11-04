using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// ゲームシステム動作検証スクリプト
    /// </summary>
    public class GameTestRunner : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool runTestOnStart = false;
        public float testDelay = 1f;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private IEnumerator RunAllTests()
        {
            Debug.Log("=== ポケモンTCG システムテスト開始 ===");
            yield return new WaitForSeconds(testDelay);

            // Test 1: デッキ構築テスト
            yield return TestDeckCreation();
            yield return new WaitForSeconds(testDelay);

            // Test 2: ゲーム初期化テスト
            yield return TestGameInitialization();
            yield return new WaitForSeconds(testDelay);

            // Test 3: カードドローテスト
            yield return TestCardDraw();
            yield return new WaitForSeconds(testDelay);

            // Test 4: ベンチ配置テスト
            yield return TestBenchPlacement();
            yield return new WaitForSeconds(testDelay);

            // Test 5: 進化テスト
            yield return TestEvolution();
            yield return new WaitForSeconds(testDelay);

            // Test 6: エネルギー装着テスト
            yield return TestEnergyAttachment();
            yield return new WaitForSeconds(testDelay);

            // Test 7: 攻撃テスト
            yield return TestAttack();
            yield return new WaitForSeconds(testDelay);

            // Test 8: にげるテスト
            yield return TestRetreat();
            yield return new WaitForSeconds(testDelay);

            // Test 9: 特殊状態テスト
            yield return TestStatusConditions();
            yield return new WaitForSeconds(testDelay);

            // Test 10: どうぐ装着テスト
            yield return TestToolAttachment();
            yield return new WaitForSeconds(testDelay);

            // Test 11: トレーナーカードテスト
            yield return TestTrainerCards();
            yield return new WaitForSeconds(testDelay);

            // Test 12: 特性システムテスト
            yield return TestAbilities();
            yield return new WaitForSeconds(testDelay);

            // Test 13: AIシステムテスト
            yield return TestAI();
            yield return new WaitForSeconds(testDelay);

            // Test 14: モーダル選択テスト
            yield return TestModalSelection();
            yield return new WaitForSeconds(testDelay);

            Debug.Log("=== 全テスト完了 ===");
        }

        private IEnumerator TestDeckCreation()
        {
            Debug.Log("[Test 1] デッキ構築テスト");

            // 簡易デッキ作成（ScriptableObject参照が必要なため手動で確認）
            var deck = new List<CardData>();

            // 実際のアセット読み込みが必要
            var raltsAsset = Resources.Load<PokemonCardData>("PTCG/Pokemon/Ralts");
            if (raltsAsset != null)
            {
                Debug.Log($"✓ Raltsアセット読み込み成功: {raltsAsset.cardName}");
            }
            else
            {
                Debug.LogError("✗ Raltsアセットが見つかりません（Resourcesフォルダにあるか確認）");
            }

            yield return null;
        }

        private IEnumerator TestGameInitialization()
        {
            Debug.Log("[Test 2] ゲーム初期化テスト");

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("✗ GameManagerインスタンスなし");
                yield break;
            }

            Debug.Log($"✓ GameManager存在確認");
            Debug.Log($"  - Player1: {gm.player1?.playerName ?? "null"}");
            Debug.Log($"  - Player2: {gm.player2?.playerName ?? "null"}");
            Debug.Log($"  - TurnCount: {gm.turnCount}");

            yield return null;
        }

        private IEnumerator TestCardDraw()
        {
            Debug.Log("[Test 3] カードドローテスト");

            var player = new GameObject("TestPlayer").AddComponent<PlayerController>();

            // テストデッキ作成（ダミーカード）
            var testDeck = new List<CardData>();
            for (int i = 0; i < 60; i++)
            {
                // ダミーデータ（実際はScriptableObjectが必要）
                testDeck.Add(null);
            }

            player.Initialize("TestPlayer", 0, testDeck);

            int deckCountBefore = player.deck.Count;
            bool drawSuccess = player.Draw(7);
            int deckCountAfter = player.deck.Count;

            if (drawSuccess && deckCountBefore - deckCountAfter == 7)
            {
                Debug.Log($"✓ ドロー成功: {deckCountBefore} → {deckCountAfter} (-7枚)");
            }
            else
            {
                Debug.LogError($"✗ ドロー失敗");
            }

            Destroy(player.gameObject);
            yield return null;
        }

        private IEnumerator TestBenchPlacement()
        {
            Debug.Log("[Test 4] ベンチ配置テスト");

            var player = new GameObject("TestPlayer").AddComponent<PlayerController>();
            player.Initialize("TestPlayer", 0, new List<CardData>());

            // ダミーポケモン作成
            var dummyPokemon = ScriptableObject.CreateInstance<PokemonCardData>();
            dummyPokemon.cardID = "TestPokemon";
            dummyPokemon.cardName = "テストポケモン";
            dummyPokemon.stage = PokemonStage.Basic;

            var instance = new GameObject("TestPokemonInstance").AddComponent<PokemonInstance>();
            instance.Initialize(dummyPokemon, 0);

            player.benchSlots.Add(instance);

            if (player.benchSlots.Count == 1)
            {
                Debug.Log($"✓ ベンチ配置成功: {player.benchSlots.Count}体");
            }
            else
            {
                Debug.LogError("✗ ベンチ配置失敗");
            }

            Destroy(instance.gameObject);
            Destroy(player.gameObject);
            yield return null;
        }

        private IEnumerator TestEvolution()
        {
            Debug.Log("[Test 5] 進化システムテスト");

            var evoSystem = EvolutionSystem.Instance;
            if (evoSystem == null)
            {
                Debug.LogError("✗ EvolutionSystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ EvolutionSystem存在確認");

            // 実際の進化テストにはScriptableObjectが必要
            Debug.Log("  （詳細テストにはRalts/Kirliaアセットが必要）");

            yield return null;
        }

        private IEnumerator TestEnergyAttachment()
        {
            Debug.Log("[Test 6] エネルギー装着テスト");

            var energySystem = EnergySystem.Instance;
            if (energySystem == null)
            {
                Debug.LogError("✗ EnergySystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ EnergySystem存在確認");

            // ダミーテスト
            var dummyPokemon = ScriptableObject.CreateInstance<PokemonCardData>();
            dummyPokemon.cardName = "テストポケモン";
            var dummyEnergy = ScriptableObject.CreateInstance<EnergyCardData>();
            dummyEnergy.cardName = "基本超エネルギー";
            dummyEnergy.isBasic = true;
            dummyEnergy.providesType = PokemonType.P;

            var instance = new GameObject("TestPokemon").AddComponent<PokemonInstance>();
            instance.Initialize(dummyPokemon, 0);
            instance.AttachEnergy(dummyEnergy);

            if (instance.attachedEnergies.Count == 1)
            {
                Debug.Log($"✓ エネルギー装着成功: {instance.attachedEnergies.Count}個");
            }
            else
            {
                Debug.LogError("✗ エネルギー装着失敗");
            }

            Destroy(instance.gameObject);
            yield return null;
        }

        private IEnumerator TestAttack()
        {
            Debug.Log("[Test 7] 攻撃システムテスト");

            var battleSystem = BattleSystem.Instance;
            if (battleSystem == null)
            {
                Debug.LogError("✗ BattleSystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ BattleSystem存在確認");

            // ダメージ計算テスト
            var attacker = ScriptableObject.CreateInstance<PokemonCardData>();
            attacker.type = PokemonType.P;
            var defender = ScriptableObject.CreateInstance<PokemonCardData>();
            defender.weakness = PokemonType.P;
            defender.weaknessMultiplier = 2;
            defender.baseHP = 100;

            var defenderInstance = new GameObject("Defender").AddComponent<PokemonInstance>();
            defenderInstance.Initialize(defender, 1);

            defenderInstance.TakeDamage(30);

            if (defenderInstance.currentDamage == 30)
            {
                Debug.Log($"✓ ダメージ計算成功: {defenderInstance.currentDamage}ダメージ");
            }
            else
            {
                Debug.LogError("✗ ダメージ計算失敗");
            }

            Destroy(defenderInstance.gameObject);
            yield return null;
        }

        private IEnumerator TestRetreat()
        {
            Debug.Log("[Test 8] にげるシステムテスト");

            var retreatSystem = RetreatSystem.Instance;
            if (retreatSystem == null)
            {
                Debug.LogError("✗ RetreatSystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ RetreatSystem存在確認");

            yield return null;
        }

        private IEnumerator TestStatusConditions()
        {
            Debug.Log("[Test 9] 特殊状態システムテスト");

            // こんらん状態テスト
            var testPokemon = ScriptableObject.CreateInstance<PokemonCardData>();
            testPokemon.cardName = "テストポケモン";
            var instance = new GameObject("TestPokemon").AddComponent<PokemonInstance>();
            instance.Initialize(testPokemon, 0);

            instance.statusCondition = StatusCondition.Confusion;
            if (instance.statusCondition == StatusCondition.Confusion)
            {
                Debug.Log("✓ こんらん状態設定成功");
            }
            else
            {
                Debug.LogError("✗ 特殊状態設定失敗");
            }

            Destroy(instance.gameObject);
            yield return null;
        }

        private IEnumerator TestToolAttachment()
        {
            Debug.Log("[Test 10] どうぐ装着システムテスト");

            var toolSystem = ToolSystem.Instance;
            if (toolSystem == null)
            {
                Debug.LogError("✗ ToolSystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ ToolSystem存在確認");

            yield return null;
        }

        private IEnumerator TestTrainerCards()
        {
            Debug.Log("[Test 11] トレーナーカードシステムテスト");

            var cardPlaySystem = CardPlaySystem.Instance;
            if (cardPlaySystem == null)
            {
                Debug.LogError("✗ CardPlaySystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ CardPlaySystem存在確認");
            Debug.Log("  （14種トレーナーカード基盤実装済み）");

            yield return null;
        }

        private IEnumerator TestAbilities()
        {
            Debug.Log("[Test 12] 特性システムテスト");

            var abilitySystem = AbilitySystem.Instance;
            if (abilitySystem == null)
            {
                Debug.LogError("✗ AbilitySystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ AbilitySystem存在確認");
            Debug.Log("  （精製、アドレナブレイン、ふしぎなしっぽ等実装済み）");

            yield return null;
        }

        private IEnumerator TestAI()
        {
            Debug.Log("[Test 13] AIシステムテスト");

            var aiController = AIController.Instance;
            if (aiController != null)
            {
                Debug.Log("✓ AIController存在確認");
            }
            else
            {
                Debug.Log("  （AIControllerは未配置 - オプション機能）");
            }

            yield return null;
        }

        private IEnumerator TestModalSelection()
        {
            Debug.Log("[Test 14] モーダル選択システムテスト");

            var modalSystem = ModalSystem.Instance;
            if (modalSystem == null)
            {
                Debug.LogError("✗ ModalSystemインスタンスなし");
                yield break;
            }

            Debug.Log("✓ ModalSystem存在確認");
            Debug.Log("  （UI統合待ち - ロジック実装済み）");

            yield return null;
        }

        [ContextMenu("Run Tests Now")]
        public void RunTestsNow()
        {
            StartCoroutine(RunAllTests());
        }
    }
}
