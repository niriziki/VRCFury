# Changelog

All notable changes to **VRCFury Stub** (`net.nrzk.vfstub`) are documented here.

**VRCFury Stub** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.

公開リリースのあるバージョンのみを掲載しています。

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
