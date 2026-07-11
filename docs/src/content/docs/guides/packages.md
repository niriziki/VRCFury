---
title: パッケージ構成
sidebar:
  order: 4
---

SPS for NDMF は、役割の異なる3つのパッケージで構成されています。このページでは、それぞれの役割と依存関係、そして「自分の場合はどれを入れればいいか」を説明します。

## 3つのパッケージ

| パッケージ | 役割 |
|---|---|
| **SPS for NDMF**（`net.nrzk.spsndmf`） | 本体。VRCFury の SPS 機能を NDMF プラグインとして抽出したもの。 |
| **VRCFury Stub**（`net.nrzk.vfstub`） | VRCFury を入れずに、設定済みの VRCFury コンポーネントを変換用に保持するパッケージ。**VRCFuryと同時にインストールすることはできません。** |
| **SPSNDMF Migrator**（`net.nrzk.spsndmf-migrator`） | VRCFury / VRCFury Stub と SPS for NDMF  の間で、SPS 関連のコンポーネントを相互に変換できるエディタ拡張。（Plug/Socket以外の関連コンポーネントはVRCFury→MAの片方向変換です。） |

それぞれの詳細は [SPS for NDMF（本体）](/spsndmf/overview/)・[VRCFury Stub](/vfstub/overview/)・[SPSNDMF Migrator](/migrator/overview/) の各ページを参照してください。

## どれを入れるべきか

- **新規にアバターを組む場合**:
  - SPS for NDMF（`net.nrzk.spsndmf`）
- **VRCFury の SPS 機能を使った既存ギミックがある場合**:
  - SPS for NDMF
  - VRCFury Stub (**インストールする時はVRCFuryをアンインストールしてから！**)
  - SPSNDMF Migrator

3つを組み合わせた移行手順の詳細は [移行の流れ](/migrator/migration-flow/) を参照してください。
