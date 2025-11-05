using System.ComponentModel;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using ModelContextProtocol.Server;
using UnityEngine;
using System.Collections.Generic;
using PTCG;

#if UNITY_EDITOR
using UnityEditor;
#endif

[McpServerToolType, Description("Fix card names for ScriptableObjects")]
public class CardNameFixerMCPTool
{
    [McpServerTool, Description("Set correct Japanese card names for all cards")]
    public async ValueTask<string> FixAllCardNames()
    {
        await UniTask.SwitchToMainThread();

#if UNITY_EDITOR
        var fixedCards = new List<string>();

        // Trainers
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/HyperBall.asset", "ハイパーボール", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/Research.asset", "博士の研究", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/Boss.asset", "ボスの指令", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/Iono.asset", "ナンジャモ", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/Pepper.asset", "ペパー", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/NestBall.asset", "ネストボール", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/RareCandy.asset", "ふしぎなアメ", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/EarthenVessel.asset", "大地の器", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/SuperRod.asset", "スーパーロッド", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/CounterCatcher.asset", "カウンターキャッチャー", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/LevelBall.asset", "レベルボール", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/LostSweeper.asset", "ロストスイーパー", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/EscapeRope.asset", "あなぬけのヒモ", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/Artazon.asset", "ボウルタウン", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/BeachCourt.asset", "ビーチコート", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Trainers/BraveryCharm.asset", "いさぎのふんどし", fixedCards);

        // Energies
        SetCardName("Assets/_Project/Resources/PTCG/Energies/BasicPsychic.asset", "基本超エネルギー", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Energies/ReversalEnergy.asset", "リバーサルエネルギー", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Energies/BasicDarkness.asset", "基本悪エネルギー", fixedCards);

        // Pokemon (check if they need fixing)
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/Ralts.asset", "ラルトス", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/Kirlia.asset", "キルリア", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/GardevoirEX.asset", "サーナイトex", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/Drifloon.asset", "フワンテ", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/MewEX.asset", "ミュウex", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/MewTail.asset", "ミュウテール", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/Mashimashira.asset", "マシマシラ", fixedCards);
        SetCardName("Assets/_Project/Resources/PTCG/Pokemon/LillieClefairyEX.asset", "リリィのピクシーex", fixedCards);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return $"SUCCESS: Fixed {fixedCards.Count} card names:\n" + string.Join("\n", fixedCards);
#else
        return "ERROR: This tool only works in Unity Editor";
#endif
    }

#if UNITY_EDITOR
    private void SetCardName(string assetPath, string cardName, List<string> fixedCards)
    {
        var asset = AssetDatabase.LoadAssetAtPath<PTCG.CardData>(assetPath);
        if (asset != null)
        {
            SerializedObject so = new SerializedObject(asset);
            SerializedProperty cardNameProp = so.FindProperty("cardName");
            if (cardNameProp != null)
            {
                cardNameProp.stringValue = cardName;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                fixedCards.Add($"{asset.name}: {cardName}");
            }
        }
        else
        {
            Debug.LogWarning($"Asset not found: {assetPath}");
        }
    }
#endif
}
