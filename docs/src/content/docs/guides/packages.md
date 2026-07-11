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
| **VRCFury Stub**（`net.nrzk.vfstub`） | VRCFury 本体を入れていなくても、既存のシーンやアバターに設定済みの VRCFury のコンポーネント（設定）が壊れたり消えたりせずにそのまま開けるようにするための互換パッケージ。VRCFury のような編集機能やビルド処理は持たない。 |
| **SPSNDMF Migrator**（`net.nrzk.spsndmf-migrator`） | VRCFury / VRCFury Stub / SPSNDMF / Modular Avatar / VRChat Contacts の間で SPS 関連コンポーネントを相互変換するエディタ拡張。 |

それぞれの詳細は [SPS for NDMF（本体）](/spsndmf/overview/)・[VRCFury Stub](/vfstub/overview/)・[SPSNDMF Migrator](/migrator/overview/) の各ページを参照してください。

## 依存関係

各パッケージが必要とする依存パッケージは次のとおりです。

| パッケージ | 依存パッケージ |
|---|---|
| SPS for NDMF（`net.nrzk.spsndmf`） | `com.vrchat.avatars` `>=3.7.6`、`nadena.dev.modular-avatar` `^1.15.0` |
| VRCFury Stub（`net.nrzk.vfstub`） | `com.vrchat.avatars` `>=3.7.6` |
| SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`） | `com.vrchat.avatars` `>=3.7.6`、`nadena.dev.modular-avatar` `^1.15.0`、`nadena.dev.ndmf` `>=1.11.0 <2.0.0-a` |

VRCFury Stub は VRCFury のコンポーネントを保持するだけの軽量なパッケージなので、Modular Avatar や NDMF には依存しません。SPS for NDMF は NDMF の仕組みの上で動作しますが、Modular Avatar 経由で導入されます（Modular Avatar 自体が NDMF を基盤にしたパッケージのため）。SPSNDMF Migrator はビルド時に自動で変換を行う機能を持つため、NDMF を直接の依存として指定しています。

いずれのパッケージも VRCFury 本体（`com.vrcfury.vrcfury`）を依存関係に含みません。SPSNDMF Migrator の変換処理は、VRCFury 本体または VRCFury Stub のどちらが入っていても動作します。

## どれを入れるべきか

- **新規にアバターを組む場合**：通常は本体の SPS for NDMF（`net.nrzk.spsndmf`）だけで十分です。
- **すでに VRCFury（またはその SPS 機能）を使ったシーン・プレハブがあり、VRCFury 本体を使い続けたくない場合**：SPS for NDMF に加えて VRCFury Stub と SPSNDMF Migrator の3つすべてが必要になります。

3つを組み合わせた移行手順の詳細は [移行の流れ](/migrator/migration-flow/) を参照してください。
