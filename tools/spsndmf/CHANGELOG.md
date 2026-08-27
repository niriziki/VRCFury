# Changelog

All notable changes to **SPS for NDMF** (`net.nrzk.spsndmf`) are documented here.

**SPS for NDMF** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.

公開リリースのあるバージョンのみを掲載しています。

## [1.1419.1] - 2026-08-27
### Fixed / 修正
- Animations you author yourself (in the avatar's own FX layer or brought in with MA Merge Animator) can now control SPS plug features. Previously these animations were silently ignored, so for example toggling **Animated Toggle** from the expressions menu did not stop the deformation. Now working:
  - **Animated Toggle**: animating the checkbox pauses and resumes SPS deformation.
  - Animating SPS material properties (such as `material._SPS_Enabled`) on the plug mesh.
  - **Animated blendshapes to keep while deforming**: registered blendshapes stay animatable during deformation.
  - Material-swap animations on the plug mesh: the swapped-in material is now SPS-enabled too, so deformation survives the swap.
  - Animating the legacy TPS toggle (`material._TPS_AnimatedToggle`) on the plug mesh.

  自作のアニメーション（アバター自身の FX レイヤーや MA Merge Animator で持ち込んだもの）から SPS Plug の機能を操作できるようになりました。これまでこれらのアニメーションは黙って無視されており、たとえば Ex メニューから **Animated Toggle** をオフにしても変形が止まりませんでした。以下が動作するようになります：
  - **Animated Toggle**：チェックボックスのアニメーションで SPS の変形を一時停止・再開できます。
  - Plug メッシュの SPS マテリアルプロパティ（`material._SPS_Enabled` など）の直接アニメーション。
  - **Animated blendshapes to keep while deforming**：登録したブレンドシェイプが変形中もアニメーションで動かせます。
  - Plug メッシュのマテリアル差し替えアニメーション：差し替え先のマテリアルにも SPS が適用され、差し替え後も変形が維持されます。
  - Plug メッシュの旧 TPS トグル（`material._TPS_AnimatedToggle`）のアニメーション。

### Known limitations / 既知の制限
- If the plug's GameObject starts hidden (inactive) and is only shown by an animation, the automatic scale detection may not run and the deformation depth can be wrong. Keep the plug object active and use **Animated Toggle** to pause deformation instead.

  Plug の GameObject を最初から非表示（非アクティブ）にしておき、アニメーションで表示に切り替える構成では、スケール自動検出が働かず変形の深さが正しくならないことがあります。Plug オブジェクトはアクティブのままにし、変形を止めたい場合は **Animated Toggle** を使ってください。
## [1.1419.0] - 2026-08-21
### Changed / 変更
- Updated the bundled VRCFury source to upstream 1.1419.0.

  同梱の VRCFury ソースを上流 1.1419.0 に更新しました。
- When an SPS component sits under an object with non-uniform scale, the build error now lists every offending object and its scale at once, instead of only the first one (upstream change).

  SPS コンポーネントが不均一なスケール（X・Y・Z が一致しない）のオブジェクトの下にある場合、ビルドエラーに該当するオブジェクトとスケールがまとめて一覧表示されるようになりました。これまでは最初の1件だけでした（上流の変更）。

## [1.1416.1] - 2026-08-11
### Added / 追加
- SPS Sockets and Plugs can now be edited directly on a prefab instance, instead of only inside the original prefab. This is off by default and is turned on from **Tools > SPSNDMF > SPS > Allow Editing on Prefab Instances**. See the documentation for what it costs.

  SPS Socket / Plug を、元のプレハブを開かずにプレハブインスタンス上で直接編集できるようになりました。既定ではオフで、**Tools > SPSNDMF > SPS > Allow Editing on Prefab Instances** から有効にします。引き換えになるものはドキュメントを参照してください。
- **Tools > SPSNDMF > SPS > Migrate Project Data** migrates every prefab and every instance in the project to the current data format in one pass, keeping the settings each instance overrides. It is only needed while the above setting is on.

  **Tools > SPSNDMF > SPS > Migrate Project Data** は、プロジェクト内のすべてのプレハブとインスタンスを現在のデータ形式へ一度に移行します。各インスタンスが上書きしている設定は保持されます。上記の設定がオンのときにのみ必要です。

### Changed / 変更
- Nothing changes unless you turn the new setting on. With it off, SPS behaves exactly as before; only the two menu items above are added.

  新しい設定をオンにしない限り、動作は一切変わりません。オフのままなら SPS はこれまでどおりで、増えるのは上記のメニュー項目2つだけです。

## [1.1416.0] - 2026-08-10
### Changed / 変更
- Updated the bundled VRCFury source to upstream 1.1416.0.

  同梱の VRCFury ソースを上流 1.1416.0 に更新しました。
- Removed the "Enable SPS contacts in play mode" setting added in 1.1408.2. It was based on a wrong assumption: VRChat contacts do interact in play mode without it, so the setting never did anything. Nothing changes for you, and SPS keeps reacting in play mode.

  1.1408.2 で追加した「Enable SPS contacts in play mode」の設定を削除しました。VRChat のコンタクトはこの設定が無くてもプレイモードで反応するため、この設定は何もしていませんでした（追加時の前提が誤っていました）。動作に変化はなく、プレイモードでも SPS はこれまでどおり反応します。

- A build now stops with an error if an SPS component points at an object outside the avatar, including a reference into a prefab asset (upstream change). Such references used to be ignored silently, so a build that used to succeed may now fail. This usually means the component was copied between avatars and needs its reference fixed, or the component removed.

  SPS コンポーネントがアバターの外のオブジェクトを参照している場合、ビルドがエラーで止まるようになりました（上流の変更）。プレハブアセットへの参照も対象です。これまで黙って無視されていたため、通っていたビルドが失敗するようになる場合があります。多くはアバター間でコンポーネントをコピーしたときに起きるもので、参照を直すかコンポーネントを削除してください。

### Fixed / 修正
- SPS Socket markers no longer inherit the object's scale, so sockets on models built at unusual scales (100x and the like) no longer produce enormous bounding boxes (upstream fix).

  SPS Socket のマーカーがオブジェクトのスケールを引き継がなくなり、100倍などの特殊なスケールで作られたモデルでも巨大なバウンディングボックスにならなくなりました（上流修正）。
- Legacy socket light offsets and guided path tangents are now stored in local units instead of world units, so they no longer shift when the object is scaled (upstream fix). Existing sockets are converted automatically the first time they are loaded.

  ソケットの旧形式のライト位置とガイドパスの制御点が、ワールド単位ではなくローカル単位で保存されるようになり、オブジェクトのスケールでずれなくなりました（上流修正）。既存のソケットは最初に読み込まれたときに自動で変換されます。

## [1.1408.2] - 2026-08-10
### Added / 追加
- SPS Plugs and Sockets placed outside an avatar (directly in the scene) now build in play mode too, so a single prop or gadget can be tested on its own. Everything is discarded when you leave play mode.

  アバターの外（シーンに直接置いたオブジェクトなど）に付けた SPS Plug / Socket も、プレイモードで組み立てられるようになりました。小物やギミック単体で動作を確認できます。プレイモードを抜けると元の状態に戻ります。
- VRChat contacts do not interact with each other in play mode, which kept SPS from reacting. All contacts are now made to interact while in play mode. This affects every contact in the editor, not just the ones SPS creates, so it can be turned off from **Tools → SPSNDMF → Settings → Enable SPS contacts in play mode**, and a build in play mode reports it to the NDMF Console.

  VRChat のコンタクトはプレイモードでは互いに反応せず、SPS も動きませんでした。プレイモード中はすべてのコンタクトが互いに反応するようにしました。エディタ全体のコンタクトに影響するため、**Tools → SPSNDMF → Settings → Enable SPS contacts in play mode** でオフにでき、プレイモード中のビルドでは NDMF Console に通知が出ます。

## [1.1408.1] - 2026-08-09
### Fixed / 修正
- SPS Socket's automatic type detection (hole or ring, and where the hand touch zone goes) now looks at where the socket ends up after Modular Avatar moves it. Sockets placed with MA Bone Proxy, Merge Armature or Replace Object were previously judged from their original position in the scene and were often treated as a ring by mistake.

  SPS Socket の自動判定（穴かリングか、手の接触ゾーンの位置）が、Modular Avatar による移動後の位置を見るようになりました。MA Bone Proxy・Merge Armature・Replace Object で配置したソケットは、これまでシーン上の元の位置で判定されてしまい、リングと誤判定されることがありました。

### Added / 追加
- If the same GameObject has more than one SPS Plug or more than one SPS Socket, the build now stops and points at that object in the NDMF Console. Until now duplicated sockets silently produced doubled contacts and duplicated menu entries.

  1つの GameObject に SPS Plug や SPS Socket が2つ以上付いている場合、ビルドを止めて NDMF コンソールから該当オブジェクトを指し示すようにしました。これまで、ソケットの重複はコンタクトやメニュー項目が二重に作られたまま気付けませんでした。

## [1.1408.0] - 2026-08-06
### Changed / 変更
- First stable release. The beta suffix is gone, and the version number now matches the bundled upstream VRCFury release (1.1408.0). Fixes made only in this package raise the patch number (1.1408.1, 1.1408.2, …); picking up a newer upstream moves to that upstream's number. Nothing changed since 0.2.0-beta.13.

  最初の安定版リリースです。beta が外れ、バージョン番号は同梱している上流 VRCFury のバージョン（1.1408.0）と一致するようになりました。本パッケージ独自の修正ではパッチ番号が上がり（1.1408.1、1.1408.2 …）、新しい上流を取り込んだときはその上流のバージョン番号になります。0.2.0-beta.13 からの変更はありません。

## [0.2.0-beta.13] - 2026-08-04
### Changed / 変更
- The published package no longer ships VRCFury's test assemblies or its public API assembly. Neither was ever compiled in this package, so nothing changes at runtime; the download is simply smaller.

  配布パッケージに VRCFury のテスト用アセンブリと公開 API アセンブリを含めないようにしました。どちらも本パッケージではコンパイルされていなかったため動作に変化はなく、ダウンロードサイズが小さくなります。

## [0.2.0-beta.12] - 2026-08-04
### Changed / 変更
- Updated the bundled VRCFury source to upstream 1.1408.0. SPS baking was reorganised upstream, VRChat's new global PhysBone colliders are used where available, and Unity 6 compatibility was improved.

  同梱の VRCFury ソースを上流 1.1408.0 に更新しました。SPS のベイク処理が上流で整理され、VRChat の新しいグローバル PhysBone コライダーが利用可能な場合は使われるようになり、Unity 6 との互換性が改善されています。
### Fixed / 修正
- Patched SPS shaders are no longer rebuilt on every build. They are now kept in `Assets/ZZZ_GeneratedAssets/spsndmf-shaders` and reused whenever the same shader is patched again, which made a repeat manual bake go from about 33 seconds to about 8 seconds in our test avatar. You can delete that folder at any time; it is recreated on the next build.

  パッチ済みの SPS シェーダーが毎回のビルドで作り直されていた問題を修正しました。`Assets/ZZZ_GeneratedAssets/spsndmf-shaders` に保存され、同じシェーダーであれば再利用されます。検証アバターでは2回目以降の手動ビルドが約33秒から約8秒に短縮されました。このフォルダはいつ削除しても構いません（次のビルドで作り直されます）。
- Fixed the weight of the animator layers that SPS adds to the Action playable layer being changed during the merge with Modular Avatar.

  SPS が Action プレイアブルレイヤーに追加するアニメーターレイヤーの重みが、Modular Avatar との統合時に変わってしまう問題を修正しました。
- lilToon materials no longer produce a shader error when SPS patches them (upstream fix).

  lilToon のマテリアルに SPS のパッチを当てたときにシェーダーエラーが出る問題を修正しました（上流修正）。
- SPS deformation is no longer injected into shadow caster and meta passes, so shadows and lightmap baking are unaffected by plug deformation (upstream fix).

  影を落とすパスとメタパスに SPS の変形処理が入らなくなり、影やライトマップのベイクがプラグの変形の影響を受けなくなりました（上流修正）。
- Material swaps now work correctly on plugs whose mesh was auto-rigged by SPS (upstream fix).

  SPS による自動リグが行われたメッシュのプラグで、マテリアルの切り替えが正しく動作するようになりました（上流修正）。
- Entering play mode no longer risks crashing Unity while SPS samples plug colors (upstream fix). Note that plug colors are not sampled in play mode as a result; they are still sampled for manual builds and uploads.

  プレイモードに入るときに SPS がプラグの色を読み取ることで Unity がクラッシュしうる問題を修正しました（上流修正）。その代わりプレイモードではプラグの色が読み取られません（手動ビルドとアップロードでは従来どおり読み取られます）。

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
