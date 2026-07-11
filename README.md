# SPS for NDMF

[VRCFury](https://vrcfury.com) の **SPS (Super Plug Shader) 機能のみ** を抽出し、[NDMF](https://github.com/bdunderscore/ndmf) プラグインとして再パッケージしたもの。

[Modular Avatar](https://modular-avatar.nadena.dev/) と同じ NDMF ビルドパイプライン上で動作し、非破壊ビルド時に SPS の処理だけをアバターへ適用する。

## 位置付け

- VRCFury 全体を導入せず、SPS だけを使いたい環境向け。
- NDMF 環境専用。VRC SDK 単体ビルドや上流 VRCFury の旧ビルド経路は使用しない。
- `BuildPhase.Transforming` で Modular Avatar の前に実行される。

## 提供パッケージ

本リポジトリでは以下3つのパッケージを配布している。

| パッケージ | 役割 |
|---|---|
| `net.nrzk.spsndmf` (SPS for NDMF) | 本体。VRCFury の SPS 機能を NDMF プラグインとして抽出したもの。 |
| `net.nrzk.vfstub` (VRCFury Stub) | VRCFury Runtime の型のみのスタブ（`VF.*` 名前空間・同一 GUID）。VRCFury 本体を入れずに既存のシーン／プレハブに埋め込まれた VRCFury コンポーネントを保持したままロードするための互換レイヤ。エディタロジック・ビルド処理は持たない。 |
| `net.nrzk.spsndmf-migrator` (SPSNDMF Migrator) | VRCFury / VRCFury Stub / SPSNDMF / Modular Avatar / VRChat Contacts の間で SPS 関連コンポーネントを相互変換するエディタ拡張。 |

## インストール

VPAI インストーラまたは VPM リポジトリ経由:

- VPM: `https://spsndmf.vpm.nrzk.net/index.json`
- VPAI installer (unitypackage) は [Releases](../../releases) から入手可能。

## ライセンス

本プロジェクトは VRCFury のフォークであり、VRCFury のライセンスに従う。

ライセンス条文は [`LICENSE.md`](./LICENSE.md) を参照。本フォークは VRCFury の **Personal License** の下で配布される派生物である。

- Copyright (c) 2026 Senky (VRCFury)
- 商用利用については VRCFury 本体のライセンス（`vcc.vrcfury.com` からの入手が必要）に従う必要があるため、本フォーク経由での商用利用はできない点に注意。
- 本パッケージは SPS シェーダーを含むため、VRChat にアップロードされたアバター／ワールドのアセットバンドル内での実行時利用には **VRCA / VRCW License** が併せて適用される（VRChat 外での SPS シェーダーの商用実行時利用は不可）。

## リポジトリ構成

- `com.vrcfury.vrcfury/` — 上流 VRCFury のソース。SPSNDMF 本体の修正もここに直接加える。
- `net.nrzk.spsndmf/` — `spsndmf` のリリース時に生成される出荷用パッケージ（手編集しない）。
- `net.nrzk.vfstub/` — `vfstub` のリリース時に生成される出荷用パッケージ（手編集しない）。
- `net.nrzk.spsndmf-migrator/` — migrator パッケージ本体（生成ではなく直接管理）。
- `tools/spsndmf/ReleaseTransform.cs` — `com.vrcfury.vrcfury/` → `net.nrzk.spsndmf/` 変換スクリプト。
- `tools/vfstub/ReleaseTransform.cs` — VRCFury Runtime → `net.nrzk.vfstub/` 変換スクリプト。
- `.agent-docs/vrcfury-ndmf-complete-reference.md` — VRCFury 処理の網羅リファレンス（内部・開発者向け）。

## 関連

- [VRCFury](https://vrcfury.com)
- [NDMF](https://github.com/bdunderscore/ndmf)
- [Modular Avatar](https://modular-avatar.nadena.dev/)
