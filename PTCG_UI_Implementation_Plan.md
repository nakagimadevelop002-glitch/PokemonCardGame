# ポケモンTCG Live風 UI実装計画

**目標**: `ptcglbattle.jpg` の完全再現
**実装方式**: 段階的構築 + 各段階でスクリーンショット確認

---

## 事前準備

### 新規シーン作成
- **シーン名**: `BattleScene`
- **保存先**: `Assets/_Project/Scenes/BattleScene.unity`
- **理由**: 既存TestSceneと分離、UI専用環境

### 必要アセット確認
- [ ] カード背面画像（デッキ/トラッシュ用）
- [ ] 背景グラデーション画像 or UI Shader
- [ ] ポケモンカード画像（テスト用）
- [ ] ボタン用Sprite（END TURNボタン等）

---

## Phase 1: Canvas基礎構造作成 ✓ 確認ポイント1

### 作業内容
1. Canvas作成 (Screen Space - Overlay)
2. CanvasScaler設定 (1920x1080, Scale With Screen Size)
3. EventSystem作成

### 作成GameObject
```
Canvas (PTCG_BattleUI)
├── BackgroundPanel (Image: 黒単色)
└── EventSystem
```

### MCPコマンド
```bash
# 1. シーン作成
CreateScene("BattleScene")

# 2. Canvas作成
CreateGameObject("Canvas", parent=null)
AddComponent("Canvas", "Canvas")
AddComponent("Canvas", "CanvasScaler")
AddComponent("Canvas", "GraphicRaycaster")

# 3. CanvasScaler設定
SetInspectorField("Canvas/CanvasScaler", "uiScaleMode", "ScaleWithScreenSize")
SetInspectorField("Canvas/CanvasScaler", "referenceResolution", "1920x1080")

# 4. EventSystem作成
CreateGameObject("EventSystem", parent=null)
AddComponent("EventSystem", "EventSystem")
AddComponent("EventSystem", "StandaloneInputModule")
```

### 確認事項
- [ ] Canvasが表示されているか
- [ ] Game Viewで1920x1080表示確認
- [ ] Hierarchy整理されているか

**→ スクリーンショット撮影 & 報告**

---

## Phase 2: 3大エリア作成 ✓ 確認ポイント2

### 作業内容
1. OpponentArea (上部1/3・赤背景)
2. BattleFieldArea (中央1/3・紫背景)
3. PlayerArea (下部1/3・青背景)

### 作成GameObject
```
Canvas
├── OpponentArea (Panel)
│   └── Background (Image: 赤グラデーション)
├── BattleFieldArea (Panel)
│   └── Background (Image: 紫グラデーション)
└── PlayerArea (Panel)
    └── Background (Image: 青グラデーション)
```

### RectTransform設定
```csharp
// OpponentArea
Anchor: Min(0, 0.66), Max(1, 1)
Offset: Left=0, Top=0, Right=0, Bottom=0
Color: #B43232 (RGB 180, 50, 50, 230)

// BattleFieldArea
Anchor: Min(0, 0.33), Max(1, 0.66)
Offset: すべて0
Color: #7832C8 (RGB 120, 50, 200, 242)

// PlayerArea
Anchor: Min(0, 0), Max(1, 0.33)
Offset: すべて0
Color: #3264B4 (RGB 50, 100, 180, 230)
```

### MCPコマンド
```bash
# OpponentArea作成
CreatePanel("OpponentArea", parent="Canvas",
            anchorMin=[0,0.66], anchorMax=[1,1],
            color="#B43232E6")

# BattleFieldArea作成
CreatePanel("BattleFieldArea", parent="Canvas",
            anchorMin=[0,0.33], anchorMax=[1,0.66],
            color="#7832C8F2")

# PlayerArea作成
CreatePanel("PlayerArea", parent="Canvas",
            anchorMin=[0,0], anchorMax=[1,0.33],
            color="#3264B4E6")
```

### 確認事項
- [ ] 3つのエリアが画面を3分割しているか
- [ ] 背景色が正しく表示されているか（赤/紫/青）
- [ ] 境界線がきれいに整列しているか

**→ スクリーンショット撮影 & 報告**

---

## Phase 3: OpponentArea子要素配置 ✓ 確認ポイント3

### 作業内容
OpponentAreaに以下を配置:
1. DeckZone (左上)
2. PrizeArea (中央上)
3. TrashZone (右上)
4. ActiveZone (中央)
5. BenchArea (左側)

### 作成GameObject
```
OpponentArea
├── DeckZone (Panel)
│   ├── DeckPileImage (Image: カード裏面)
│   └── DeckCountText (Text: "6")
├── PrizeArea (Panel - Horizontal Layout)
│   ├── PrizeCard1-6 (Image x6)
├── TrashZone (Panel)
│   ├── TrashPileImage (Image: カード裏面)
│   └── TrashCountText (Text: "47")
├── ActiveZone (Panel)
│   └── ActiveCardSlot (Image: 空スロット)
└── BenchArea (Panel - Horizontal Layout)
    └── BenchSlot1-5 (Image x5)
```

### 座標設定
```csharp
// DeckZone
Parent: OpponentArea
Anchor: Min(0.02, 0.3), Max(0.15, 0.7)
Size: 自動計算（Stretchベース）

// PrizeArea
Anchor: Min(0.35, 0.85), Max(0.65, 0.95)
Horizontal Layout Group: Spacing=10, Child Force Expand=False

// TrashZone
Anchor: Min(0.85, 0.3), Max(0.98, 0.7)

// ActiveZone
Anchor: Min(0.4, 0.1), Max(0.6, 0.5)

// BenchArea
Anchor: Min(0.15, 0.1), Max(0.38, 0.4)
Horizontal Layout Group: Spacing=5
```

### 確認事項
- [ ] DeckZoneが左上に配置されているか
- [ ] PrizeAreaが6枚横並びか
- [ ] TrashZoneが右上に配置されているか
- [ ] ActiveZoneが中央に配置されているか
- [ ] BenchAreaが5スロット横並びか

**→ スクリーンショット撮影 & 報告**

---

## Phase 4: PlayerArea子要素配置 ✓ 確認ポイント4

### 作業内容
PlayerAreaに以下を配置（OpponentAreaと対称配置）:
1. DeckZone (左下)
2. TrashZone (右下)
3. ActiveZone (中央)
4. BenchArea (左側)

### 座標設定（PlayerArea基準）
```csharp
// DeckZone
Anchor: Min(0.02, 0.3), Max(0.15, 0.7)

// TrashZone
Anchor: Min(0.85, 0.3), Max(0.98, 0.7)

// ActiveZone
Anchor: Min(0.4, 0.5), Max(0.6, 0.9)

// BenchArea
Anchor: Min(0.15, 0.6), Max(0.38, 0.9)
Horizontal Layout Group: Spacing=5
```

### 確認事項
- [ ] OpponentAreaと対称配置になっているか
- [ ] ベンチエリアが5スロット確保されているか
- [ ] デッキ/トラッシュが同じサイズか

**→ スクリーンショット撮影 & 報告**

---

## Phase 5: BattleFieldArea配置 ✓ 確認ポイント5

### 作業内容
中央バトルフィールドに対戦カード配置:
1. BattleEffectImage (光るエフェクト背景)
2. OpponentActiveBattle (上側カードスロット)
3. PlayerActiveBattle (下側カードスロット)

### 作成GameObject
```
BattleFieldArea
├── BattleEffectImage (Image: 紫光エフェクト)
├── OpponentActiveBattle (Panel)
│   └── CardSlot (Image: Ralts 70HP)
└── PlayerActiveBattle (Panel)
    └── CardSlot (Image: Duraludon 130HP)
```

### 座標設定
```csharp
// BattleEffectImage
Anchor: Min(0.3, 0.2), Max(0.7, 0.8)
Color: 半透明白 (255,255,255,100)

// OpponentActiveBattle
Anchor: Min(0.4, 0.55), Max(0.6, 0.85)
Size: 150x210px想定

// PlayerActiveBattle
Anchor: Min(0.4, 0.15), Max(0.6, 0.45)
Size: 150x210px想定
```

### 確認事項
- [ ] 2枚のカードが対峙しているか
- [ ] 中央配置が画像と一致しているか
- [ ] カードサイズが適切か

**→ スクリーンショット撮影 & 報告**

---

## Phase 6: HandArea作成 ✓ 確認ポイント6

### 作業内容
最下部に手札エリア作成:
1. HandAreaPanel (最下部15%)
2. HandCardsContainer (Horizontal Layout)
3. HandCard1-6 (テストカード6枚配置)

### 作成GameObject
```
Canvas
└── HandArea (Panel)
    └── HandCardsContainer (Horizontal Layout Group)
        ├── HandCard1 (Image: 120x170px)
        ├── HandCard2 (Image)
        ├── HandCard3 (Image)
        ├── HandCard4 (Image)
        ├── HandCard5 (Image)
        └── HandCard6 (Image)
```

### 座標設定
```csharp
// HandArea
Anchor: Min(0, 0), Max(1, 0.15)
Color: 半透明黒 (25,25,25,180)

// HandCardsContainer
Anchor: Min(0.15, 0.1), Max(0.85, 0.95)
Horizontal Layout Group:
  - Spacing: 10
  - Child Alignment: Middle Center
  - Child Force Expand: Width=False, Height=True
```

### 確認事項
- [ ] 手札が6枚横並びで表示されているか
- [ ] カードサイズが統一されているか (120x170px)
- [ ] 中央寄せになっているか

**→ スクリーンショット撮影 & 報告**

---

## Phase 7: UIコントロール配置 ✓ 確認ポイント7

### 作業内容
画面右側・左側にUIボタン配置:

**右側UI**:
1. SettingsButton (歯車・右上)
2. TimerText (20:00)
3. EndTurnButton (白ボタン "END TURN")
4. PrizeCountOpponent (6)
5. PrizeCountPlayer (6)
6. PlayerTimerText (19:52)

**左側UI**:
1. PlayerNameText (kami198)
2. ChatButton
3. SettingsButton
4. LogButton

### 作成GameObject
```
Canvas
├── UIControlsRight (Panel - 透明)
│   ├── SettingsButton (Button)
│   ├── TimerText (Text)
│   ├── EndTurnButton (Button)
│   ├── PrizeCountOpp (Text)
│   ├── PrizeCountPlayer (Text)
│   └── PlayerTimer (Text)
└── UIControlsLeft (Panel - 透明)
    ├── PlayerName (Text)
    ├── ChatButton (Button)
    ├── SettingsButton (Button)
    └── LogButton (Button)
```

### 座標設定
```csharp
// UIControlsRight
Anchor: Min(0.9, 0), Max(1, 1)

// EndTurnButton (重要)
Anchor: Min(0.92, 0.50), Max(0.99, 0.56)
Color: 白背景 (255,255,255,255)
Text: "END\nTURN" (Bold, 24px)

// TimerText
Anchor: Min(0.92, 0.85), Max(0.98, 0.88)
Text: "20:00" (White, 20px)

// UIControlsLeft
Anchor: Min(0, 0), Max(0.1, 1)

// PlayerName
Anchor: Min(0.01, 0.95), Max(0.10, 0.99)
Text: "kami198" (White, 18px)
```

### 確認事項
- [ ] END TURNボタンが白背景で目立っているか
- [ ] タイマーが正しい位置か
- [ ] プレイヤー名が左上に表示されているか
- [ ] ボタンがクリック可能か

**→ スクリーンショット撮影 & 報告**

---

## Phase 8: カードプレハブ作成 ✓ 確認ポイント8

### 作業内容
再利用可能なカードプレハブ作成:
1. PokemonCardPrefab
2. TrainerCardPrefab
3. EnergyCardPrefab

### プレハブ構造
```
PokemonCardPrefab
├── CardBackground (Image: 白枠)
├── CardImage (Image: カードイラスト)
├── CardName (Text: "Ralts")
├── CardHP (Text: "70")
├── CardType (Image: 超タイプアイコン)
├── AttackArea (Panel)
└── EnergyIcons (Horizontal Layout)
```

### スクリプト連携
```csharp
// CardUIController.cs (新規作成)
public class CardUIController : MonoBehaviour
{
    public Image cardImage;
    public Text cardName;
    public Text cardHP;
    public void SetCardData(PokemonCardData data);
}
```

### 確認事項
- [ ] プレハブが正しく生成されるか
- [ ] カードデータを渡すと表示が更新されるか
- [ ] サイズが統一されているか

**→ 動作テスト & 報告**

---

## Phase 9: UIManager統合 ✓ 確認ポイント9

### 作業内容
GameManagerとUI接続:
1. BattleUIManager.cs作成
2. 各UIコンポーネント参照設定
3. GameManagerからの更新メソッド実装

### スクリプト作成
```csharp
// BattleUIManager.cs
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    // UI References
    public Transform opponentActiveZone;
    public Transform playerActiveZone;
    public Transform[] opponentBenchSlots;
    public Transform[] playerBenchSlots;
    public Transform handCardsContainer;
    public Text deckCountText;
    public Text trashCountText;
    public Button endTurnButton;

    // Update Methods
    public void UpdateActiveZone(PlayerController player, PokemonInstance pokemon);
    public void UpdateBench(PlayerController player);
    public void UpdateHand(PlayerController player);
    public void UpdateDeckCount(int count);
}
```

### GameManager統合
```csharp
// GameManager.csに追加
private void UpdateUI()
{
    BattleUIManager.Instance.UpdateActiveZone(player1, player1.activeSlot);
    BattleUIManager.Instance.UpdateBench(player1);
    BattleUIManager.Instance.UpdateHand(player1);
}
```

### 確認事項
- [ ] GameManagerが起動するとUIが更新されるか
- [ ] カードをドローすると手札UIが更新されるか
- [ ] ポケモンを場に出すとActiveZoneに表示されるか
- [ ] END TURNボタンが機能するか

**→ 動作テスト & 報告**

---

## Phase 10: 最終調整 ✓ 確認ポイント10

### 作業内容
1. 色調整（画像との完全一致）
2. サイズ微調整
3. アニメーション追加（オプション）
4. パフォーマンステスト

### 最終チェックリスト
- [ ] `ptcglbattle.jpg` との目視比較で95%以上一致
- [ ] 全エリアが正しく配置されている
- [ ] ボタンがすべて機能する
- [ ] GameManager連携が動作する
- [ ] 1920x1080以外の解像度でも正しく表示される
- [ ] パフォーマンス: 60fps維持

**→ 最終スクリーンショット撮影 & 完成報告**

---

## 実装順序まとめ

```
Phase 1: Canvas基礎 (5分)
Phase 2: 3大エリア作成 (10分) ← まず形を作る
Phase 3: OpponentArea配置 (15分)
Phase 4: PlayerArea配置 (15分)
Phase 5: BattleField配置 (10分)
Phase 6: HandArea作成 (10分) ← ここまでで骨格完成
Phase 7: UIコントロール (15分)
Phase 8: カードプレハブ (20分) ← 再利用部品
Phase 9: UIManager統合 (30分) ← ロジック連携
Phase 10: 最終調整 (20分)

合計推定時間: 2.5〜3時間
```

---

## 段階的確認の進め方

### 各Phaseで実施:
1. **実装**: MCPコマンドでUI作成
2. **PlayMode起動**: 実際の表示確認
3. **スクリーンショット**: Game View撮影
4. **報告**: "Phase X完了 - スクリーンショット確認お願いします"
5. **ユーザー承認**: OKなら次Phase / NGなら修正

### MCPコマンド実行パターン
```bash
# 1. GameObject作成
curl -X POST http://localhost:56780/mcp/ -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"CreateGameObject","arguments":{...}},"id":1}'

# 2. プロパティ設定
curl -X POST http://localhost:56780/mcp/ -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"InspectorFieldSetter","arguments":{...}},"id":1}'

# 3. ログ確認
curl -X POST http://localhost:56780/mcp/ -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"GetCurrentConsoleLogs"},"id":1}'
```

---

この計画でよろしいですか？
承認いただければPhase 1から開始します。
