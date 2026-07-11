---
title: SPS for NDMF とは
sidebar:
  order: 1
---

**SPS for NDMF** は、[VRCFury](https://vrcfury.com) の中から **SPS (Super Plug Shader)** 機能だけを抽出し、[NDMF](https://github.com/bdunderscore/ndmf) プラグインとして再パッケージしたものです。[Modular Avatar](https://modular-avatar.nadena.dev/)（以下 MA）を使ってアバターを組んでいる人が、VRCFuryを使わずにSPSを使えるようにすることを目指しています。

## SPS for NDMF とは

VRCFury は非破壊ビルドのための多機能ツールですが、SPS for NDMF はそのうち SPS（Plug / Socket による接触判定・アニメーション機能）だけを取り出し、独立した NDMF プラグインにしたものです。VRCFury 本体をインストールしなくても、SPS 機能だけを単体で使うことができます。

## 何を解決するか

- VRCFury 全体は導入したくないが、SPS の Plug / Socket 機能だけは使いたい。
- すでに Modular Avatar で非破壊ビルドの構成を組んでいて、そこに別のビルドシステムを持ち込みたくない。

VRCFuryの様々な暗黙的な副作用やNDMF系ツールとの相性を懸念するユースケースのためのものです。
SPS に必要な部分だけを抽出しているため、MA を中心とした構成に SPS だけを過不足なく追加できます。

## 3つのパッケージ

本プロジェクトは以下の3パッケージを配布しています。

| パッケージ | 役割 |
|---|---|
| SPS for NDMF（`net.nrzk.spsndmf`） | 本体。VRCFury の SPS 機能を NDMF プラグインとして抽出したもの。 |
| VRCFury Stub（`net.nrzk.vfstub`） | VRCFury を入れずに、設定済みの VRCFury コンポーネントを変換用に保持するパッケージ。**VRCFuryと同時にインストールすることはできません。** |
| SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`） | VRCFury / VRCFury Stub と SPS for NDMF  の間で、SPS 関連のコンポーネントを相互に変換できるエディタ拡張。（Plug/Socket以外の関連コンポーネントはVRCFury→MAの片方向変換です。） |

自分で新規導入する際は本体の SPS for NDMF（`net.nrzk.spsndmf`）だけで利用できます。既存ギミック活用等他の2パッケージが必要になるケースについては [パッケージ構成](/guides/packages/) を参照してください。

次は [インストール](/guides/install/) に進んでください。
