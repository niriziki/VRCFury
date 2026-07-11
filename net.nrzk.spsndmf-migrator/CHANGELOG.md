# Changelog

All notable changes to **SPSNDMF Migrator** (`net.nrzk.spsndmf-migrator`) are documented here.

**SPSNDMF Migrator** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.

公開リリースのあるバージョンのみを掲載しています。

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
- Version bump to keep the release line aligned with the other SPSNDMF packages. No functional changes.

  他の SPSNDMF パッケージとリリースラインを揃えるためのバージョン更新。機能変更なし。

## [0.2.0-beta.4] - 2026-07-08
### Added / 追加
- Build-time stub→SPSNDMF migration NDMF plugin (merged into the 0.2 line): VRCFury Stub components are converted to SPSNDMF automatically during the avatar build.

  ビルド時 スタブ→SPSNDMF 変換 NDMF プラグイン（0.2 系へ統合）。アバタービルド時に VRCFury Stub コンポーネントを SPSNDMF へ自動変換。

## [0.2.0-beta.3] - 2026-07-07
### Changed / 変更
- Baseline of the 0.2 (SPS2) release line. Carries the missing-`.meta` fixes and canonical `.meta` formatting from the 0.1 line.

  0.2 系（SPS2）リリースラインの基点。0.1 系からの `.meta` 欠落修正と `.meta` 標準形式化を引き継ぎ。

## [0.1.0-beta.4] - 2026-07-08
### Added / 追加
- Build-time stub→SPSNDMF migration NDMF plugin: VRCFury Stub components are converted to SPSNDMF automatically during the avatar build.

  ビルド時 スタブ→SPSNDMF 変換 NDMF プラグイン。アバタービルド時に VRCFury Stub コンポーネントを SPSNDMF へ自動変換。

## [0.1.0-beta.3] - 2026-07-07
### Fixed / 修正
- Add the missing `.meta` files for all package contents, and match Unity's canonical `.meta` format.

  パッケージ内全ファイルの欠落していた `.meta` を追加し、Unity 標準の `.meta` 形式に一致させた。

## [0.0.1-beta.0] - 2026-04-19
### Added / 追加
- Initial release. Convert SPS components between VRCFury, SPSNDMF, Modular Avatar, and VRChat Contacts.

  初回リリース。SPS コンポーネントを VRCFury・SPSNDMF・Modular Avatar・VRChat Contacts 間で相互変換。
