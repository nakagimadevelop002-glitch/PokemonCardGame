@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ============================================================
echo Unity Natural MCP Environment Copy Script
echo ============================================================
echo.

REM 引数チェック
if "%~1"=="" (
    echo エラー: コピー先のプロジェクトパスを指定してください。
    echo.
    echo 使用方法:
    echo   Copy_MCP_Environment.bat "C:\Path\To\NewUnityProject"
    echo.
    pause
    exit /b 1
)

set "SOURCE_DIR=%~dp0"
set "DEST_DIR=%~1"

echo ソースディレクトリ: %SOURCE_DIR%
echo 宛先ディレクトリ: %DEST_DIR%
echo.

REM 宛先ディレクトリの存在確認
if not exist "%DEST_DIR%" (
    echo エラー: 宛先ディレクトリが見つかりません。
    echo 先に新しいUnityプロジェクトを作成してください。
    echo.
    pause
    exit /b 1
)

echo コピーを開始します...
echo.

REM =============================================================
REM 1. パッケージ設定ファイルのコピー
REM =============================================================
echo [1/13] パッケージ設定ファイルをコピー中...
xcopy "%SOURCE_DIR%Packages\manifest.json" "%DEST_DIR%\Packages\" /Y /Q
xcopy "%SOURCE_DIR%Packages\packages-lock.json" "%DEST_DIR%\Packages\" /Y /Q 2>nul

REM =============================================================
REM 2. プロジェクト設定ファイルのコピー
REM =============================================================
echo [2/13] プロジェクト設定ファイルをコピー中...
if not exist "%DEST_DIR%\ProjectSettings\" mkdir "%DEST_DIR%\ProjectSettings\"
xcopy "%SOURCE_DIR%ProjectSettings\UnityNaturalMCPSetting.asset" "%DEST_DIR%\ProjectSettings\" /Y /Q 2>nul

REM =============================================================
REM 3. NuGet設定ファイルのコピー
REM =============================================================
echo [3/13] NuGet設定ファイルをコピー中...
if not exist "%DEST_DIR%\Assets\" mkdir "%DEST_DIR%\Assets\"
xcopy "%SOURCE_DIR%Assets\NuGet.config" "%DEST_DIR%\Assets\" /Y /Q
xcopy "%SOURCE_DIR%Assets\NuGet.config.meta" "%DEST_DIR%\Assets\" /Y /Q

REM =============================================================
REM 4. アセンブリ定義ファイルのコピー
REM =============================================================
echo [4/13] アセンブリ定義ファイルをコピー中...
if not exist "%DEST_DIR%\Assets\_Project\Scripts\" mkdir "%DEST_DIR%\Assets\_Project\Scripts\"
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\ProjectScripts.Editor.asmdef*" "%DEST_DIR%\Assets\_Project\Scripts\" /Y /Q 2>nul

REM =============================================================
REM 5. MCPToolsフォルダの作成
REM =============================================================
echo [5/13] MCPToolsフォルダを作成中...
if not exist "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" mkdir "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\"

REM =============================================================
REM 6. MCPツール - アセンブリ定義のコピー
REM =============================================================
echo [6/13] MCPツールのアセンブリ定義をコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\MCPToolsEditor.asmdef*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 7. MCPツール - メインビルダーのコピー
REM =============================================================
echo [7/13] メインビルダーファイルをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\AllMCPToolsBuilder.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\AllMCPToolsBuilder.asset*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 8. MCPツール - テンプレートのコピー
REM =============================================================
echo [8/13] テンプレートファイルをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\MyCustomMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\MyCustomMCPToolBuilder.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 9. MCPツール - 基本オブジェクト操作ツール
REM =============================================================
echo [9/13] 基本オブジェクト操作ツールをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\SimpleTestMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\CubeCreatorMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\CubeController.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\CustomShapeMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\GameObjectManagerMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\PrefabManagerMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 10. MCPツール - シーン・プロジェクト管理ツール
REM =============================================================
echo [10/13] シーン・プロジェクト管理ツールをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\CreateSceneMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\PackageManagerMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\PackageInstallerMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\RefreshMCPToolsMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 11. MCPツール - UI作成ツール
REM =============================================================
echo [11/13] UI作成ツールをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\UICreatorMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\ComprehensiveUICreatorMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 12. MCPツール - コンポーネント操作ツール
REM =============================================================
echo [12/13] コンポーネント操作ツールをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\ComponentAttachMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\AttachScriptMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\InspectorFieldSetterMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\ScriptableObjectMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM 13. MCPツール - 実験的ツール (オプション)
REM =============================================================
echo [13/13] 実験的ツールをコピー中...
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\SeaweedCreatorMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\SeaweedCarpetMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q
xcopy "%SOURCE_DIR%Assets\_Project\Scripts\MCPTools\OrganicPatchMCPTool.cs*" "%DEST_DIR%\Assets\_Project\Scripts\MCPTools\" /Y /Q

REM =============================================================
REM オプション: サンプルツールビルダーのコピー
REM =============================================================
echo.
echo [オプション] サンプルツールビルダーをコピー中...
if not exist "%DEST_DIR%\Assets\MySCPTools\" mkdir "%DEST_DIR%\Assets\MySCPTools\"
xcopy "%SOURCE_DIR%Assets\MySCPTools\*.asset*" "%DEST_DIR%\Assets\MySCPTools\" /Y /Q 2>nul

REM =============================================================
REM オプション: サンプルプレハブのコピー
REM =============================================================
echo [オプション] サンプルプレハブをコピー中...
if not exist "%DEST_DIR%\Assets\_Project\Prefabs\" mkdir "%DEST_DIR%\Assets\_Project\Prefabs\"
xcopy "%SOURCE_DIR%Assets\_Project\Prefabs\Circle.prefab*" "%DEST_DIR%\Assets\_Project\Prefabs\" /Y /Q 2>nul
xcopy "%SOURCE_DIR%Assets\_Project\Prefabs\Pentagon.prefab*" "%DEST_DIR%\Assets\_Project\Prefabs\" /Y /Q 2>nul
xcopy "%SOURCE_DIR%Assets\_Project\Prefabs\Triangle*.prefab*" "%DEST_DIR%\Assets\_Project\Prefabs\" /Y /Q 2>nul
xcopy "%SOURCE_DIR%Assets\_Project\Prefabs\CorrectedTriangle.prefab*" "%DEST_DIR%\Assets\_Project\Prefabs\" /Y /Q 2>nul

echo.
echo ============================================================
echo コピー完了！
echo ============================================================
echo.
echo 次の手順:
echo 1. Unityエディタで新しいプロジェクトを開く
echo 2. パッケージのインストールを待つ（自動）
echo 3. Window ^> Unity Natural MCP ^> Settings を開いて設定確認
echo 4. AllMCPToolsBuilder.cs を開き、42-44行目のTCG関連コードを削除
echo 5. MCPサーバーを起動して動作確認
echo.
echo 詳細は MCP_Environment_Setup_Guide.md を参照してください。
echo.
pause
