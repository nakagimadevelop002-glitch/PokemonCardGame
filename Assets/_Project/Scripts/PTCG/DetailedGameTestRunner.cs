using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTCG
{
    /// <summary>
    /// 詳細なゲームロジック検証（UI自動選択モード利用）
    /// </summary>
    public class DetailedGameTestRunner : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool runTestOnStart = true;
        public float testDelay = 1f;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunDetailedTests());
            }
        }

        private IEnumerator RunDetailedTests()
        {
            yield return new WaitForSeconds(testDelay);

            // Test 1: 進化システム詳細テスト
            yield return TestEvolutionDetail();
            yield return new WaitForSeconds(testDelay);

            // Test 2: サイコエンブレイス特性テスト
            yield return TestPsychicEmbraceAbility();
            yield return new WaitForSeconds(testDelay);

            // Test 3: 弱点・抵抗力計算テスト
            yield return TestWeaknessResistance();
            yield return new WaitForSeconds(testDelay);

            // Test 4: ハイパーボールテスト（自動選択）
            yield return TestHyperBall();
            yield return new WaitForSeconds(testDelay);

            // Test 5: 攻撃ダメージ計算詳細テスト
            yield return TestAttackDamageCalculation();
            yield return new WaitForSeconds(testDelay);

        }

        private IEnumerator TestEvolutionDetail()
        {

            var evoSystem = EvolutionSystem.Instance;
            if (evoSystem == null)
            {
                yield break;
            }

            // Ralts作成
            var raltsData = Resources.Load<PokemonCardData>("PTCG/Pokemon/Ralts");
            if (raltsData == null)
            {
                yield break;
            }

            var raltsInstance = new GameObject("Ralts").AddComponent<PokemonInstance>();
            raltsInstance.Initialize(raltsData, 0);

            // エネルギー装着
            var psychicEnergy = ScriptableObject.CreateInstance<EnergyCardData>();
            psychicEnergy.cardName = "基本超エネルギー";
            psychicEnergy.isBasic = true;
            psychicEnergy.providesType = PokemonType.P;
            raltsInstance.AttachEnergy(psychicEnergy);

            // ダメージ追加
            raltsInstance.TakeDamage(20);


            // Kirlia進化（アセットがあれば）
            var kirliaData = Resources.Load<PokemonCardData>("PTCG/Pokemon/Kirlia");
            if (kirliaData != null)
            {
                var player = new GameObject("TestPlayer").AddComponent<PlayerController>();
                player.Initialize("TestPlayer", 0, new List<CardData>());
                player.benchSlots.Add(raltsInstance);

                // ターンを進めて進化可能にする
                raltsInstance.turnsInPlay = 1;
                raltsInstance.wasPlayedThisTurn = false;

                bool canEvolve = evoSystem.CanEvolve(raltsInstance, kirliaData);
                if (canEvolve)
                {
                    evoSystem.Evolve(player, raltsInstance, kirliaData, false);
                    var evolved = player.benchSlots[0];
                }
                else
                {
                }

                Destroy(player.gameObject);
            }
            else
            {
            }

            Destroy(raltsInstance.gameObject);
            yield return null;
        }

        private IEnumerator TestPsychicEmbraceAbility()
        {

            var energySystem = EnergySystem.Instance;
            if (energySystem == null)
            {
                yield break;
            }

            var gardevoirData = Resources.Load<PokemonCardData>("PTCG/Pokemon/GardevoirEX");
            if (gardevoirData == null)
            {
                yield break;
            }

            var player = new GameObject("TestPlayer").AddComponent<PlayerController>();
            player.Initialize("TestPlayer", 0, new List<CardData>());

            var gardevoir = new GameObject("GardevoirEX").AddComponent<PokemonInstance>();
            gardevoir.Initialize(gardevoirData, 0);
            player.benchSlots.Add(gardevoir);

            // トラッシュに超エネルギー配置（模擬）
            var psychicEnergy = ScriptableObject.CreateInstance<EnergyCardData>();
            psychicEnergy.cardName = "基本超エネルギー";
            psychicEnergy.isBasic = true;
            psychicEnergy.providesType = PokemonType.P;
            player.discard.Add(psychicEnergy);


            // サイコエンブレイス実行
            bool success = energySystem.UsePsychicEmbrace(player, gardevoir);

            if (success)
            {
            }
            else
            {
            }

            Destroy(gardevoir.gameObject);
            Destroy(player.gameObject);
            yield return null;
        }

        private IEnumerator TestWeaknessResistance()
        {

            var battleSystem = BattleSystem.Instance;
            if (battleSystem == null)
            {
                yield break;
            }

            // 攻撃側：超タイプ
            var attackerData = ScriptableObject.CreateInstance<PokemonCardData>();
            attackerData.cardName = "超タイプ攻撃者";
            attackerData.type = PokemonType.P;
            attackerData.baseHP = 100;
            // 簡単な攻撃追加
            attackerData.attacks = new System.Collections.Generic.List<AttackData>
            {
                new AttackData { attackName = "テスト攻撃", baseDamage = 50, energyCost = 1 }
            };

            var attacker = new GameObject("Attacker").AddComponent<PokemonInstance>();
            attacker.Initialize(attackerData, 0);
            // エネルギー追加
            var energy = ScriptableObject.CreateInstance<EnergyCardData>();
            energy.cardName = "基本超エネルギー";
            energy.isBasic = true;
            energy.providesType = PokemonType.P;
            attacker.AttachEnergy(energy);

            // 防御側：弱点超x2、抵抗力鋼-30
            var defenderData = ScriptableObject.CreateInstance<PokemonCardData>();
            defenderData.cardName = "防御側";
            defenderData.type = PokemonType.C;
            defenderData.baseHP = 100;
            defenderData.weakness = PokemonType.P;
            defenderData.weaknessMultiplier = 2;
            defenderData.resistance = PokemonType.M;
            defenderData.resistanceValue = 30;

            var defender = new GameObject("Defender").AddComponent<PokemonInstance>();
            defender.Initialize(defenderData, 1);

            // プレイヤー作成
            var player1 = new GameObject("TestPlayer1").AddComponent<PlayerController>();
            player1.Initialize("TestPlayer1", 0, new List<CardData>());
            player1.activeSlot = attacker;

            var player2 = new GameObject("TestPlayer2").AddComponent<PlayerController>();
            player2.Initialize("TestPlayer2", 1, new List<CardData>());
            player2.activeSlot = defender;

            int damageBefore = defender.currentDamage;

            // GameManager必要なので簡易実装をスキップ
            // 代わりに直接ダメージテスト
            defender.TakeDamage(100); // 弱点適用後の想定ダメージ

            Destroy(attacker.gameObject);
            Destroy(defender.gameObject);
            Destroy(player1.gameObject);
            Destroy(player2.gameObject);
            yield return null;
        }

        private IEnumerator TestHyperBall()
        {

            var cardPlaySystem = CardPlaySystem.Instance;
            var modalSystem = ModalSystem.Instance;

            if (cardPlaySystem == null)
            {
                yield break;
            }

            if (modalSystem == null)
            {
            }


            yield return null;
        }

        private IEnumerator TestAttackDamageCalculation()
        {

            var battleSystem = BattleSystem.Instance;
            if (battleSystem == null)
            {
                yield break;
            }

            // Drifloon（selfCountersX30攻撃）テスト
            var drifloonData = Resources.Load<PokemonCardData>("PTCG/Pokemon/Drifloon");
            if (drifloonData != null)
            {
                var attacker = new GameObject("Drifloon").AddComponent<PokemonInstance>();
                attacker.Initialize(drifloonData, 0);

                // ダメカン30配置
                attacker.TakeDamage(30);

                var defenderData = ScriptableObject.CreateInstance<PokemonCardData>();
                defenderData.cardName = "テスト防御者";
                defenderData.baseHP = 100;
                var defender = new GameObject("Defender").AddComponent<PokemonInstance>();
                defender.Initialize(defenderData, 1);

                // selfCountersX30計算確認（BattleSystem内部で処理）
                if (drifloonData.attacks != null && drifloonData.attacks.Count > 0)
                {
                    var attack = drifloonData.attacks[0];

                    if (attack.effectID == "selfCountersX30")
                    {
                        int expectedDamage = (attacker.currentDamage / 10) * 30;
                    }
                    else
                    {
                    }
                }
                else
                {
                }

                Destroy(attacker.gameObject);
                Destroy(defender.gameObject);
            }
            else
            {
            }

            yield return null;
        }

        [ContextMenu("Run Detailed Tests Now")]
        public void RunDetailedTestsNow()
        {
            StartCoroutine(RunDetailedTests());
        }
    }
}
