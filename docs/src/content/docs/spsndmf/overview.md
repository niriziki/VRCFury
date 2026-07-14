---
title: SPS for NDMF（本体）
sidebar:
  order: 1
---

SPS for NDMF（`net.nrzk.spsndmf`）は、3パッケージの中核となる本体パッケージです。

## 役割

VRCFury の SPS (Super Plug Shader) 機能を抽出し、NDMF プラグインとして実行するパッケージです。VRCFury 本体を導入しなくても、Modular Avatar と同じ NDMF ビルドパイプライン上で SPS の Plug / Socket 処理だけを単体で使えるようにします。

## 動作するタイミング

SPS for NDMF は、Modular Avatar と同じ非破壊ビルドの仕組みの中で、**Modular Avatar より前**に SPS の処理を行うプラグインとして組み込まれています。ビルドは次の2段階で行われます。

1. VRCFury の SPS ビルドロジックをアバターに対して実行する
2. その結果を Modular Avatar（以下 MA）のコンポーネントとして出力する

この順序により、SPS の処理結果が MA コンポーネントとして出力された状態で、その後の MA のビルドが実行されます。

## 提供コンポーネント

SPS for NDMF が実際にビルド処理の対象とするのは、**SPS Plug** と **SPS Socket** の2コンポーネントです。アバター上にこのいずれかが存在する場合にのみ SPS のビルド処理が実行されます。それぞれの詳しい使い方は [Plug の詳細](/spsndmf/plug/)・[Socket の詳細](/spsndmf/socket/) を参照してください。このほかに、SPS メニューの場所などを調整する補助コンポーネントとして [SPS Options](/spsndmf/options/) があります（Plug / Socket があるときに効果を持ちます）。

VRCFury には、このほかに Global Collider や Haptic Touch Receiver / Sender といったコンポーネントもありますが、これらは SPS for NDMF のビルド処理の対象には含まれていません。既存の VRCFury アバターでこれらを使っている場合は、[SPSNDMF Migrator](/migrator/overview/) で Modular Avatar / VRChat Contacts のネイティブなコンポーネントに変換して使うことを想定しています。詳しくは [移行の流れ](/migrator/migration-flow/) を参照してください。
