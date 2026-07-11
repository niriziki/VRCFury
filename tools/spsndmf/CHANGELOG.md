# Changelog

All notable changes to **SPS for NDMF** (`net.nrzk.spsndmf`) are documented here.

**SPS for NDMF** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.

公開リリースのあるバージョンのみを掲載しています。

## [0.2.0-beta.6] - 2026-07-11
### Added / 追加
- Bundled changelog: a bilingual (English / Japanese) `CHANGELOG.md` is now shipped with the package and linked from `changelogUrl`.

  同梱CHANGELOG: 日英併記の `CHANGELOG.md` をパッケージに同梱し、`changelogUrl` からリンク。
### Fixed / 修正
- `licensesUrl` now points to the actual repository (was a dead link).

  `licensesUrl` を実在するリポジトリへ修正（従来はリンク切れ）。

## [0.2.0-beta.5] - 2026-07-11
### Changed / 変更
- Merged upstream VRCFury 1.1360.0 (SPS2 compile / runtime performance improvements).

  上流 VRCFury 1.1360.0 を取り込み（SPS2 のコンパイル・実行時パフォーマンス改善）。
### Added / 追加
- SPS: support for additional Shader Graph shaders, plus `Unlit/Color` and `VRChat/Mobile/Particles/Additive`.

  SPS: 一部の Shader Graph シェーダー、および `Unlit/Color`・`VRChat/Mobile/Particles/Additive` に対応。
- SPS2: depth pass is now disabled by default for plugs (shadows are kept).

  SPS2: プラグの depth パスを既定で無効化（シャドウは維持）。
### Fixed / 修正
- SPS: compatibility with simple shaders that lack a newline before the program block, and improved hilted detection.

  SPS: program ブロック前に改行のない単純なシェーダーへの互換性、および hilted 検出を改善。
- SPS: fixed the bounding box for SPS meshes that are not parented to an avatar.

  SPS: アバター直下にない SPS メッシュのバウンディングボックスを修正。
- SPS: failed shaders are no longer rendered in the editor on macOS.

  SPS: macOS のエディタで失敗したシェーダーを描画しないよう修正。
- SPS: fixed the legacy option not being synced, and a world-editor issue.

  SPS: legacy オプションが同期されない問題とワールドエディタの不具合を修正。

## [0.2.0-beta.3] - 2026-07-07
### Changed / 変更
- Merged upstream VRCFury 1.1351.0, which introduces **SPS2**.

  上流 VRCFury 1.1351.0 を取り込み。**SPS2** を導入。
### Added / 追加
- SPS: additional socket positions (chest / hands / feet), a global-tag toggle on plugs, and an SPS tag override action.

  SPS: ソケット位置の追加（胸・両手・両足）、プラグの global タグ切り替え、SPS タグ上書きアクションを追加。
### Fixed / 修正
- SPS: automatically patch the PCSS shader bug when the shader is used by SPS.

  SPS: PCSS シェーダーが SPS で使われた際のバグを自動パッチ。
- SPS: various path-stop and hole-mesh fixes.

  SPS: path stop とホールメッシュ周りの各種修正。
### Internal / 内部
- `SpsSceneViewRestoreHook` now runs only when SPS content is present on the avatar.

  `SpsSceneViewRestoreHook` を、アバターに SPS コンテンツがある場合のみ動作するよう限定。

## [0.1.0-beta.3] - 2026-07-07
### Changed / 変更
- Merged upstream VRCFury 1.1348.0 (last SPS1 release).

  上流 VRCFury 1.1348.0 を取り込み（SPS1 系の最終リリース）。
### Fixed / 修正
- Ship the missing `.meta` files for the bundled `Editor-Avatars/Plugin` sources.

  同梱する `Editor-Avatars/Plugin` ソースの欠落していた `.meta` を出荷物に含めるよう修正。
- Re-import the patched SPS shader when an outer AssetEditing scope defers it.

  外側の AssetEditing スコープにより延期された場合でも、パッチ済み SPS シェーダーを再インポートするよう修正。
### Internal / 内部
- Normalize folder `.meta` files to Unity's canonical format, and exclude the upstream `UdonApi/` directory from the package.

  フォルダ `.meta` を Unity 標準形式に正規化し、上流の `UdonApi/` ディレクトリをパッケージから除外。

## [0.0.1-beta.2] - 2026-04-26
### Changed / 変更
- Rewrite the `Tools/` and `GameObject/` menu paths during the release transform.

  リリーストランスフォーム時に `Tools/`・`GameObject/` のメニューパスを書き換え。

## [0.0.1-beta.1] - 2026-04-26
### Removed / 削除
- Strip unrelated VRCFury editor menu items (duplicate PhysBone detector, unused-override / test-copy / unused-bone-cleaner / Zawoo deleter) so only SPS ships.

  SPS のみを出荷するため、無関係な VRCFury エディタメニュー（PhysBone 重複検出・不要オーバーライド削除・テストコピー・不要ボーン削除・Zawoo 削除）を除去。
- Neutralize side-effecting init methods (`WhitelistPatch.Init`, `BadInstallDetector.Init`).

  副作用のある初期化（`WhitelistPatch.Init`・`BadInstallDetector.Init`）を無効化。

## [0.0.1-beta.0] - 2026-04-19
### Added / 追加
- Initial release. SPS (Super Plug Shader) extracted from VRCFury and repackaged as an NDMF plugin that cooperates with Modular Avatar.

  初回リリース。VRCFury から SPS（Super Plug Shader）を抽出し、Modular Avatar と協調する NDMF プラグインとして再パッケージ。
