# Changelog

All notable changes to **SPS for NDMF** (`net.nrzk.spsndmf`) are documented here.

**SPS for NDMF** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.

公開リリースのあるバージョンのみを掲載しています。

## [0.2.0-beta.11] - 2026-07-25
### Changed / 変更
- Updated the bundled VRCFury source to upstream 1.1384.0: faster builds (controller handling and path lookups were reworked upstream), a simplified asset-saving pipeline, and shorter SPS shader compile times when the mesh has no blendshapes.

  同梱の VRCFury ソースを上流 1.1384.0 に更新: ビルドの高速化（コントローラー処理とパス検索が上流で刷新）、アセット保存処理の簡素化、ブレンドシェイプの無いメッシュでの SPS シェーダのコンパイル時間短縮が含まれます。
### Fixed / 修正
- SPS markers no longer lose their mesh during the build (upstream fix).

  ビルド中に SPS マーカーがメッシュを失う問題を修正しました（上流修正）。
- Baked normals and tangents are now scaled correctly with the object's scale (upstream fix).

  ベイクされた法線・接線がオブジェクトのスケールを正しく反映するようになりました（上流修正）。
- SPS options now appear in the root SPS menu even when some sockets are not added to the menu (upstream fix).

  メニューに追加していないソケットがあるときでも、SPS オプションがルートの SPS メニューに入るようになりました（上流修正）。

## [0.2.0-beta.10] - 2026-07-20
### Changed / 変更
- Enabling SPS in play mode no longer turns on Gizmos and Scene Lighting in the scene view. Your scene view settings are left alone. If Scene Lighting happens to be off, a warning is logged instead, because legacy (DPS / TPS / SPS1) sockets cannot deform plugs in the scene view without it. SPS2 sockets are unaffected.

  プレイモードで SPS を有効にしたとき、Scene ビューの Gizmo と Scene Lighting を勝手に ON にしないようになりました。Scene ビューの設定はそのまま維持されます。Scene Lighting が OFF の場合は代わりに警告をログに出します。OFF のままだとレガシー（DPS / TPS / SPS1）ソケットが Scene ビュー上でプラグを変形できないためです。SPS2 のソケットには影響しません。
### Fixed / 修正
- Play mode components (socket gizmos, the scene view control texture cleanup) are no longer dropped when the VRChat SDK is left in a build state. The SDK's build type can stay set even when no build is running, which made SPS behave as if an upload were in progress.

  VRChat SDK がビルド状態のまま残っているときに、プレイモード用のコンポーネント（ソケットのギズモ、Scene ビューの制御用テクスチャの後始末）が失われる問題を修正しました。SDK のビルド種別はビルドが走っていなくても設定されたままになることがあり、その場合 SPS がアップロード中と誤認していました。

## [0.2.0-beta.9] - 2026-07-20
### Fixed / 修正
- Play mode: SPS's internal control texture no longer appears as an overlay in the Scene view, and socket gizmos are shown again. The SPS components responsible for cleaning up that texture were being removed from the avatar before they had a chance to run.

  プレイモード: SPS が内部で使う制御用テクスチャが Scene ビューに重なって表示される問題を修正し、ソケットのギズモも再び表示されるようになりました。そのテクスチャを消す役目の SPS コンポーネントが、動作する前にアバターから削除されていたことが原因です。
- Avatar Optimizer no longer warns that SPS-NDMF components are unknown. Components that the build has already finished with are now removed before Avatar Optimizer runs, and the ones that have to remain are registered with its API, so optimization quality is no longer reduced.

  Avatar Optimizer が SPS-NDMF のコンポーネントを「未知のコンポーネント」として警告しなくなりました。ビルドで役目を終えたコンポーネントは Avatar Optimizer の実行前に削除し、残す必要があるものは Avatar Optimizer の API に登録するようにしたため、最適化の品質が落ちることもなくなります。

## [0.2.0-beta.8] - 2026-07-19
### Added / 追加
- **SPS Menus** component: place the SPS menu anywhere in your avatar's menu, working like a MA Menu Item — the menu position is where you put the object in the menu tree, and the submenu is named after the object. Turning off "Create Parent Menu" expands the SPS menu contents directly into the parent menu (like MA Menu Group). Create one via `GameObject > SPSNDMF > Create SPS Menus`. This solves duplicate parent folders that appeared when the legacy menu path setting (e.g. `Body/SPS`) named an existing Modular Avatar menu.

  **SPS Menus** コンポーネント: MA Menu Item と同じ感覚で、SPS メニューをアバターメニューの好きな場所に配置できます。メニュー内の位置はオブジェクトを置いた場所、サブメニュー名はオブジェクト名で決まります。「Create Parent Menu」をオフにすると、SPS メニューの中身を親メニューに直接展開します（MA Menu Group 相当）。`GameObject > SPSNDMF > Create SPS Menus` から作成できます。従来のメニューパス設定（例 `身体/SPS`）が Modular Avatar の既存メニューと同名の場合に親フォルダが重複する問題の解決策です。
### Changed / 変更
- Updated the bundled VRCFury source to upstream 1.1369.0: build performance improvements and SPS fixes (mesh array indexing, local units for injected parameters, hilt allowance limited to the first socket in a chain, marker interference with camera-trick world systems), plus a new in-editor green screen fix method.

  同梱の VRCFury ソースを上流 1.1369.0 に更新: ビルド性能改善と SPS の修正（メッシュ配列インデックス、注入パラメータのローカル単位、hilt allowance をチェーン先頭ソケットのみに制限、カメラトリック系ワールドギミックとのマーカー干渉）、およびエディタ内グリーンスクリーン修正の新方式を取り込みました。

## [0.2.0-beta.7] - 2026-07-11
### Added / 追加
- Online documentation site published at https://spsndmf.nrzk.net (`documentationUrl`).

  オンラインドキュメントサイト https://spsndmf.nrzk.net を公開（`documentationUrl`）。
### Changed / 変更
- `changelogUrl` now links to the online changelog on the documentation site.

  `changelogUrl` をドキュメントサイト上のオンライン更新履歴に変更。

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
