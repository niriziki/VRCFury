# Changelog

All notable changes to **VRCFury Stub** (`net.nrzk.vfstub`) are documented here.

**VRCFury Stub** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.

公開リリースのあるバージョンのみを掲載しています。

## [1.1429.1] - 2026-09-26
### Changed / 変更
- Stable release of the 1.1429.1 line. It includes everything from 1.1429.1-beta.1 and 1.1429.1-beta.2 below: VRCFury's inspectors for **SPS Plug**, **SPS Socket**, **Global Collider** and **Haptic Touch**, the **Tools/VRCFury Stub** menu, and showing the gizmos only for the selection by default.

  1.1429.1 系の安定版リリースです。下記の 1.1429.1-beta.1 と 1.1429.1-beta.2 の変更（**SPS Plug**・**SPS Socket**・**Global Collider**・**Haptic Touch** の VRCFury と同じインスペクタ、**Tools/VRCFury Stub** メニュー、既定で選択中のものだけギズモを表示する設定）をすべて含みます。

### Added / 追加
- When SPS for NDMF and SPSNDMF Migrator are installed, **SPS Plug** and **SPS Socket** components of the stub are counted by tools that show how many synced parameter bits an avatar uses before upload, such as Modular Avatar's parameter usage view, the same way SPS for NDMF 1.1429.3 counts its own components. They appear under SPS for NDMF there, since it builds them.

  SPS for NDMF と SPSNDMF Migrator が入っている環境では、Modular Avatar のパラメータ使用状況表示など、アップロード前に同期パラメータの使用ビット数を表示するツールで、スタブの **SPS Plug** と **SPS Socket** も数えられるようになりました。数え方は SPS for NDMF 1.1429.3 の自前のコンポーネントと同じです。実際にビルドするのは SPS for NDMF なので、表示上は SPS for NDMF の分として扱われます。

## [1.1429.1-beta.2] - 2026-09-25
### Changed / 変更
- The scene view now shows the gizmos of **SPS Plug** and **SPS Socket** only for the selected object and its children, instead of for every plug and socket in the scene. To show them all the time as in VRCFury, uncheck **Tools/VRCFury Stub/Settings/Show SPS Gizmos Only When Selected**. This setting is shared with SPS for NDMF.

  Scene ビューで **SPS Plug** と **SPS Socket** のギズモを、シーン内のすべてではなく、選択しているオブジェクトとその子にあるものだけ表示するようにしました。VRCFury と同じく常に表示したい場合は、**Tools/VRCFury Stub/Settings/Show SPS Gizmos Only When Selected** のチェックを外してください。この設定は SPS for NDMF と共通です。

## [1.1429.1-beta.1] - 2026-09-24
### Added / 追加
- **SPS Plug** and **SPS Socket** components now open in the same inspector as in VRCFury, and can be edited in place: every option, the depth / active / post-bake actions, the scene gizmos and the prefab-instance handling behave as they do in VRCFury. The header reads "VRCFuryStub" instead of "VRCFury". This needs NDMF and Modular Avatar to be installed (the same packages SPS for NDMF needs); without them the stub keeps Unity's default inspector as before.

  **SPS Plug** と **SPS Socket** のコンポーネントを、VRCFury と同じインスペクターで表示し、そのまま編集できるようになりました。各オプション、Depth / Active / Post-Bake のアクション、シーンのギズモ、プレハブインスタンスの扱いはすべて VRCFury と同じです。ヘッダーの表記は「VRCFury」ではなく「VRCFuryStub」になります。NDMF と Modular Avatar のインストールが必要です（SPS for NDMF が必要とするものと同じ）。入っていない場合は、これまでどおり Unity の標準インスペクターのままです。
- Other VRCFury components show their settings as plain fields instead of the "not available in this type of project" message. **Global Collider** and **Haptic Touch** use their VRCFury inspectors.

  その他の VRCFury コンポーネントは、「not available in this type of project」の表示ではなく、設定を素のフィールドとして表示するようになりました。**Global Collider** と **Haptic Touch** は VRCFury のインスペクターで表示されます。
- A **Tools/VRCFury Stub** menu with **Create Socket**, **Create Plug**, **Allow Editing on Prefab Instances**, **Migrate Project Data** and **Enable SPS Haptics**. The prefab-instance editing setting is stored separately from SPS for NDMF's.

  **Tools/VRCFury Stub** メニューを追加しました（**Create Socket**・**Create Plug**・**Allow Editing on Prefab Instances**・**Migrate Project Data**・**Enable SPS Haptics**）。プレハブインスタンスの編集設定は SPS for NDMF とは別に保存されます。

### Changed / 変更
- Opening a Plug or Socket in the inspector upgrades data saved by an older VRCFury version, as VRCFury itself does. The migrator produces the same result, so a component edited in the stub and one converted to SPS for NDMF stay identical.

  古い VRCFury で保存された Plug / Socket をインスペクターで開くと、VRCFury 本体と同じようにデータが最新の形式に更新されます。SPSNDMF Migrator も同じ結果を生成するので、スタブで編集したコンポーネントと SPS for NDMF へ変換したコンポーネントは一致します。

## [1.1429.0] - 2026-09-21
### Added / 追加
- The stub now ships the SPS part of VRCFury's public API (`com.vrcfury.api`): creating a VRCFury Socket from code and adding depth actions to it. Tools that set up SPS through this API (such as SPS2 Setup Assistant's depth action generation) now recognize the stub as capable. The API assembly is only present when SPS for NDMF 1.1429.0 or newer and SPSNDMF Migrator 1.0.0 or newer are installed, because those packages are what actually build the data the API writes. The non-SPS parts of the API (Armature Link, Toggle, Full Controller) are not included.

  VRCFury の公開 API（`com.vrcfury.api`）のうち SPS に関わる部分（コードから VRCFury Socket を作成し、深度アクションを追加する機能）を同梱するようになりました。この API を通じて SPS を設定するツール（SPS2 Setup Assistant の深度アクション生成など）が、スタブ環境を対応済みとして認識します。API が書き込んだデータを実際にビルドするのは SPS for NDMF と SPSNDMF Migrator なので、API のアセンブリは SPS for NDMF 1.1429.0 以上と SPSNDMF Migrator 1.0.0 以上の両方が入っているときだけ有効になります。SPS 以外の API（Armature Link・Toggle・Full Controller）は含みません。

### Changed / 変更
- The version number now follows the SPS for NDMF release (1.1429.0). The mirrored runtime types are unchanged from 1.1426.2; this keeps tools that read the installed VRCFury version from seeing an older release than the one SPS for NDMF is based on.

  バージョン番号を SPS for NDMF のリリース（1.1429.0）に合わせました。写し取っているランタイム型は 1.1426.2 から変わっていません。導入済みの VRCFury のバージョンを読むツールが、SPS for NDMF の元になっているリリースより古い版だと判断しないようにするためです。

## [1.1426.2] - 2026-08-28
### Changed / 変更
- Mirrored the runtime types of VRCFury 1.1426.0. The VRCFury Socket legacy type list gained the One-Way Ring option, so scenes and prefabs saved with that VRCFury keep the value while the stub is installed.

  VRCFury 1.1426.0 のランタイム型に追随しました。VRCFury Socket のレガシータイプに One-Way Ring が追加されたため、そのバージョンで保存したシーンやプレハブの値が、スタブを入れた状態でも保持されます。

## [1.1416.0] - 2026-08-10
### Changed / 変更
- Mirrored the runtime types of VRCFury 1.1416.0. VRCFury Socket gained fields for the new local-unit light offsets and guided path tangents, so scenes and prefabs saved with that VRCFury keep those values while the stub is installed.

  VRCFury 1.1416.0 のランタイム型に追随しました。VRCFury Socket にローカル単位のライト位置とガイドパス制御点のフィールドが追加されたため、そのバージョンで保存したシーンやプレハブの値が、スタブを入れた状態でも保持されます。

## [1.1408.0] - 2026-08-06
### Changed / 変更
- First stable release. The beta suffix is gone, and the version number now matches the VRCFury release whose runtime types this stub mirrors (1.1408.0). Nothing changed since 0.2.0-beta.13.

  最初の安定版リリースです。beta が外れ、バージョン番号はこのスタブが写し取っている VRCFury のバージョン（1.1408.0）と一致するようになりました。0.2.0-beta.13 からの変更はありません。

## [0.2.0-beta.13] - 2026-08-04
### Changed / 変更
- Version bump to keep the release line aligned with the other SPSNDMF packages. No functional changes.

  他の SPSNDMF パッケージとバージョンを揃えるためのリリースです。機能変更はありません。

## [0.2.0-beta.12] - 2026-08-04
### Changed / 変更
- Version bump to keep the release line aligned with the other SPSNDMF packages (updated to the VRCFury 1.1408.0 based SPS for NDMF). Upstream 1.1408.0 changed nothing in the runtime types, so the stub is unchanged.

  他の SPSNDMF パッケージ（VRCFury 1.1408.0 ベースの SPS for NDMF）とバージョンを揃えるためのリリースです。上流 1.1408.0 ではランタイム型に変更が無かったため、スタブの内容は変わっていません。

## [0.2.0-beta.11] - 2026-07-25
### Changed / 変更
- Updated stub types to match upstream VRCFury 1.1384.0 (updates the internals of `VRCFurySpsGreenScreenFix`). Version bump to keep the release line aligned with the other SPSNDMF packages.

  スタブ型を上流 VRCFury 1.1384.0 に追従しました（`VRCFurySpsGreenScreenFix` の内部を更新）。他の SPSNDMF パッケージとバージョンを揃えるためのリリースです。

## [0.2.0-beta.8] - 2026-07-19
### Changed / 変更
- Updated stub types to match upstream VRCFury 1.1369.0 (adds `VRCFurySpsGreenScreenFix`).

  スタブ型を上流 VRCFury 1.1369.0 に追従しました（`VRCFurySpsGreenScreenFix` を追加）。

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
- Version bump to keep the release line aligned with the other SPSNDMF packages. No functional changes to the stub.

  他の SPSNDMF パッケージとリリースラインを揃えるためのバージョン更新。スタブ自体の機能変更なし。

## [0.2.0-beta.3] - 2026-07-07
### Changed / 変更
- Version bump alongside the SPSNDMF 0.2 (SPS2) line. No functional changes to the stub.

  SPSNDMF 0.2 系（SPS2）に合わせたバージョン更新。スタブ自体の機能変更なし。

## [0.1.0-beta.3] - 2026-07-07
### Fixed / 修正
- Ship `Runtime.meta` and `package.json.meta` in the stub package.

  スタブパッケージに `Runtime.meta` と `package.json.meta` を同梱するよう修正。
- Normalize folder `.meta` files to Unity's canonical format.

  フォルダ `.meta` を Unity 標準形式に正規化。

## [0.0.1-beta.0] - 2026-04-19
### Added / 追加
- Initial release. Type-only stub of VRCFury's Runtime assembly: preserves the VRCFury component types (`VF.*` namespace, identical GUIDs) so existing scenes deserialize without the full VRCFury package. No editor logic or build processing is included.

  初回リリース。VRCFury の Runtime アセンブリの型のみのスタブ。VRCFury コンポーネント型（`VF.*` 名前空間・同一 GUID）を保持し、VRCFury 本体なしで既存シーンをデシリアライズ可能にする。エディタロジックやビルド処理は含まない。
