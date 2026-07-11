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

SPS for NDMF は NDMF の `BuildPhase.Transforming` で、**Modular Avatar より前**に実行されるプラグインとして登録されています。SPS の処理結果が MA コンポーネントとして出力された状態で、その後の MA のビルドが実行される流れです。NDMF 環境専用の設計になっており、VRC SDK 単体でのビルドや、上流 VRCFury の旧来のビルド経路は使用しません。

内部でどのようにビルドが行われるかは [しくみ](/sps/how-it-works/) で解説します。

## 3つのパッケージ

本プロジェクトは以下の3パッケージを配布しています。

| パッケージ | 役割 |
|---|---|
| `net.nrzk.spsndmf`（SPS for NDMF） | 本体。VRCFury の SPS 機能を NDMF プラグインとして抽出したもの。 |
| `net.nrzk.vfstub`（VRCFury Stub） | VRCFury Runtime の型のみのスタブ。VRCFury 本体を入れずに、既存のシーン／プレハブに埋め込まれた VRCFury コンポーネントを保持したままロードするための互換レイヤ。 |
| `net.nrzk.spsndmf-migrator`（SPSNDMF Migrator） | VRCFury / VRCFury Stub / SPSNDMF / Modular Avatar / VRChat Contacts の間で SPS 関連コンポーネントを相互変換するエディタ拡張。 |

通常は本体（`net.nrzk.spsndmf`）だけで利用できます。他の2パッケージが必要になるケースについては [パッケージと連携](/packages/overview/) を参照してください。

次は [インストール](/guides/install/) に進んでください。
