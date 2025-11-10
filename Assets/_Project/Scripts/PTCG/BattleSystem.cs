using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace PTCG
{
    /// <summary>
    /// バトルシステム（攻撃処理、ダメージ計算、弱点・抵抗力）
    /// </summary>
    public class BattleSystem : MonoBehaviour
    {
        public static BattleSystem Instance { get; private set; }

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
        /// 攻撃可能かチェック
        /// </summary>
        public bool CanAttack(PlayerController player)
        {
            if (player.activeSlot == null) return false;
            if (player.attackedThisTurn)
            {
                Debug.Log("このターンはすでに攻撃しました");
                return false;
            }

            var gm = GameManager.Instance;
            if (gm.currentPlayerIndex == gm.firstPlayerIndex && gm.turnCount == 1)
            {
                return false;
            }

            var active = player.activeSlot;

            if (active.statusCondition == StatusCondition.Paralysis)
            {
                Debug.Log("まひ：攻撃できない");
                return false;
            }
            if (active.statusCondition == StatusCondition.Sleep)
            {
                Debug.Log("ねむり：攻撃できない");
                return false;
            }

            if (active.data.attacks == null || active.data.attacks.Count == 0)
            {
                return false;
            }

            var attack = active.data.attacks[0];
            if (active.attachedEnergies.Count < attack.energyCost)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 攻撃不可の理由を取得
        /// </summary>
        public string GetAttackDisabledReason(PlayerController player)
        {
            if (player.activeSlot == null) return "バトル場にポケモンがいません";
            if (player.attackedThisTurn) return "このターンはすでに攻撃しました";

            var gm = GameManager.Instance;
            if (gm.currentPlayerIndex == gm.firstPlayerIndex && gm.turnCount == 1)
            {
                return "先攻1ターン目は攻撃不可";
            }

            var active = player.activeSlot;

            if (active.statusCondition == StatusCondition.Paralysis) return "まひ状態";
            if (active.statusCondition == StatusCondition.Sleep) return "ねむり状態";

            if (active.data.attacks == null || active.data.attacks.Count == 0) return "ワザがありません";

            var attack = active.data.attacks[0];
            if (active.attachedEnergies.Count < attack.energyCost)
            {
                return $"エネルギー不足（{active.attachedEnergies.Count}/{attack.energyCost}）";
            }

            return ""; // 攻撃可能
        }

        /// <summary>
        /// 攻撃実行
        /// </summary>
        public void PerformAttack(PlayerController attacker, PlayerController defender, int attackIndex = 0)
        {
            attacker.attackedThisTurn = true;

            var atkPokemon = attacker.activeSlot;
            var defPokemon = defender.activeSlot;

            if (atkPokemon == null || defPokemon == null) return;

            var attackData = atkPokemon.data.attacks[attackIndex];

            Debug.Log($"{attacker.playerName}: 《{attackData.attackName}》");

            if (atkPokemon.statusCondition == StatusCondition.Confusion)
            {
                bool heads = Random.Range(0, 2) == 0;
                if (!heads)
                {
                    atkPokemon.TakeDamage(30);
                    Debug.Log($"こんらんで自傷30");
                    CheckKnockout(attacker, atkPokemon);
                    return;
                }
                // else { Debug.Log("こんらん：コイン オモテ → 攻撃成功"); }
            }

            if (!string.IsNullOrEmpty(attackData.effectID) &&
                (attackData.effectID == "copy_attack" || attackData.effectID == "copy_random_attack"))
            {
                HandleCopyAttack(attacker, defender, attackData);
                return;
            }

            if (attackData.clearsStatus)
            {
                atkPokemon.ClearStatus();
                // Debug.Log($"特殊状態回復");
            }

            int baseDamage = ComputeAttackDamage(attackData, atkPokemon, defPokemon);
            int finalDamage = ApplyWeaknessAndResistance(atkPokemon, defPokemon, baseDamage);

            int hpBefore = defPokemon.data.baseHP - defPokemon.currentDamage;
            defPokemon.TakeDamage(finalDamage);
            int hpAfter = defPokemon.data.baseHP - defPokemon.currentDamage;

            // 詳細ログ: 誰が誰にどれだけダメージ
            Debug.Log($"【攻撃】{attacker.playerName}の《{atkPokemon.data.cardName}》→ {defender.playerName}の《{defPokemon.data.cardName}》に {finalDamage}ダメージ (HP: {hpBefore}→{hpAfter})");

            if (attackData.effectID == "inflict_confusion")
            {
                defPokemon.statusCondition = StatusCondition.Confusion;
                Debug.Log("→ 追加効果：こんらん");
            }

            CheckKnockout(defender, defPokemon);
        }

        private int ComputeAttackDamage(AttackData attackData, PokemonInstance attacker, PokemonInstance defender)
        {
            if (!string.IsNullOrEmpty(attackData.effectID))
            {
                switch (attackData.effectID)
                {
                    case "selfCountersX30":
                        int counters = attacker.currentDamage / 10;
                        return counters * 30;

                    case "bench20Plus":
                        var gm = GameManager.Instance;
                        int totalBench = gm.player1.benchSlots.Count + gm.player2.benchSlots.Count;
                        return 20 + totalBench * 20;
                }
            }

            return attackData.baseDamage;
        }

        private int ApplyWeaknessAndResistance(PokemonInstance attacker, PokemonInstance defender, int baseDamage)
        {
            int damage = baseDamage;
            PokemonType atkType = attacker.data.type;
            PokemonType defWeakness = defender.data.weakness;

            if (HasFairyZone(attacker.ownerIndex))
            {
                defWeakness = PokemonType.Y;
            }

            if (defWeakness == atkType)
            {
                damage *= defender.data.weaknessMultiplier;
            }

            if (defender.data.resistance == atkType)
            {
                damage = Mathf.Max(0, damage - defender.data.resistanceValue);
            }

            return damage;
        }

        private bool HasFairyZone(int attackerOwner)
        {
            var gm = GameManager.Instance;
            var player = attackerOwner == 0 ? gm.player1 : gm.player2;

            bool CheckLillie(PokemonInstance p)
            {
                return p != null && p.data.cardID == "LillieClefairyEX";
            }

            if (CheckLillie(player.activeSlot)) return true;

            foreach (var bench in player.benchSlots)
            {
                if (CheckLillie(bench)) return true;
            }

            return false;
        }

        private void HandleCopyAttack(PlayerController attacker, PlayerController defender, AttackData attackData)
        {
            var defenderAttacks = defender.activeSlot.data.attacks;
            if (defenderAttacks == null || defenderAttacks.Count == 0)
            {
                Debug.Log("コピー元のワザがありません");
                return;
            }

            if (attackData.effectID == "copy_random_attack")
            {
                int randomIndex = Random.Range(0, defenderAttacks.Count);
                var copiedAttack = defenderAttacks[randomIndex];

                int baseDamage = ComputeAttackDamage(copiedAttack, attacker.activeSlot, defender.activeSlot);
                int finalDamage = ApplyWeaknessAndResistance(attacker.activeSlot, defender.activeSlot, baseDamage);

                if (copiedAttack.clearsStatus)
                {
                    attacker.activeSlot.ClearStatus();
                }

                defender.activeSlot.TakeDamage(finalDamage);
                CheckKnockout(defender, defender.activeSlot);
            }
            else  // "copy_attack" - Mew's ゲノムハック
            {
                // ワザ選択肢作成
                var options = new List<SelectOption<int>>();
                for (int i = 0; i < defenderAttacks.Count; i++)
                {
                    var attack = defenderAttacks[i];
                    string label = $"{attack.attackName}（ダメージ: {attack.baseDamage}）";
                    options.Add(new SelectOption<int>(label, i));
                }

                // モーダル選択表示
                ModalSystem.Instance.OpenSelectModal(
                    "コピーするワザを選択",
                    options,
                    (selectedIndex) =>
                    {
                        if (selectedIndex < 0 || selectedIndex >= defenderAttacks.Count) return;

                        var copiedAttack = defenderAttacks[selectedIndex];
                        Debug.Log($"《{attacker.activeSlot.data.cardName}》が《{copiedAttack.attackName}》をコピー");

                        int baseDamage = ComputeAttackDamage(copiedAttack, attacker.activeSlot, defender.activeSlot);
                        int finalDamage = ApplyWeaknessAndResistance(attacker.activeSlot, defender.activeSlot, baseDamage);

                        if (copiedAttack.clearsStatus)
                        {
                            attacker.activeSlot.ClearStatus();
                        }

                        defender.activeSlot.TakeDamage(finalDamage);
                        CheckKnockout(defender, defender.activeSlot);

                        // UI更新
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.UpdateUI();
                        }
                    },
                    defaultFirst: false
                );
            }
        }

        private void CheckKnockout(PlayerController owner, PokemonInstance pokemon)
        {
            if (pokemon.IsKnockedOut)
            {
                GameManager.Instance.KnockoutPokemon(owner, pokemon);
            }
        }
    }
}
