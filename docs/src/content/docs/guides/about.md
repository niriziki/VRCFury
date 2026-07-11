---
title: SPS for NDMF とは
sidebar:
  order: 1
---

**SPS for NDMF** は、[VRCFury](https://vrcfury.com) の中から **SPS (Super Plug Shader)** 機能だけを抽出し、[NDMF](https://github.com/bdunderscore/ndmf) プラグインとして再パッケージしたものです。[Modular Avatar](https://modular-avatar.nadena.dev/)（以下 MA）を使ってアバターを組んでいる人が、そのまま同じビルドの流れで SPS を使えるようにすることを目指しています。

## SPS for NDMF とは

VRCFury は非破壊ビルドのための多機能ツールですが、SPS for NDMF はそのうち SPS（Plug / Socket による接触判定・アニメーション機能）だけを取り出し、独立した NDMF プラグインとして配布しています。VRCFury 本体をインストールしなくても、SPS 機能だけを単体で使うことができます。

## 何を解決するか

- VRCFury 全体は導入したくないが、SPS の Plug / Socket 機能だけは使いたい。
- すでに Modular Avatar で非破壊ビルドの構成を組んでいて、そこに別のビルドシステムを持ち込みたくない。

SPS for NDMF は、こうした「VRCFury 全体は導入せず SPS だけを使いたい」「既存の MA 中心の非破壊ビルドに、そのまま SPS だけを足したい」というニーズに応えるために作られています。SPS に必要な部分だけを抽出しているため、MA を中心とした構成に SPS だけを過不足なく追加できます。

## NDMF・Modular Avatar との関係

SPS for NDMF は NDMF のビルド処理の中で、**Modular Avatar より前**に実行されるように登録されています。SPS の処理結果が MA コンポーネントとして出力された状態で、その後の MA のビルドが実行される流れです。

内部でどのようにビルドが行われるかは [しくみ](/spsndmf/how-it-works/) で解説します。

## 3つのパッケージ

本プロジェクトは以下の3パッケージを配布しています。

| パッケージ | 役割 |
|---|---|
| SPS for NDMF（`net.nrzk.spsndmf`） | 本体。VRCFury の SPS 機能を NDMF プラグインとして抽出したもの。 |
| VRCFury Stub（`net.nrzk.vfstub`） | VRCFury を入れていなくても、既存アバターに設定済みの VRCFury の設定が壊れたり消えたりせずに開けるようにするための互換用パッケージ。 |
| SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`） | VRCFury / VRCFury Stub / SPS for NDMF / Modular Avatar / VRChat Contacts の間で、SPS 関連のコンポーネントを相互に変換できるエディタ拡張。 |

通常は本体の SPS for NDMF（`net.nrzk.spsndmf`）だけで利用できます。他の2パッケージが必要になるケースについては [パッケージ構成](/guides/packages/) を参照してください。

次は [インストール](/guides/install/) に進んでください。
