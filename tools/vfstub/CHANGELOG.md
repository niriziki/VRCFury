# Changelog

All notable changes to **VRCFury Stub** (`net.nrzk.vfstub`) are documented here.
**VRCFury Stub** の主な変更点を記録します。各項目は英語・日本語を併記しています。

Only versions that have a published release are listed.
公開リリースのあるバージョンのみを掲載しています。

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
