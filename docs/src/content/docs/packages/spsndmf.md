---
title: net.nrzk.spsndmf（本体）
sidebar:
  order: 2
---

`net.nrzk.spsndmf`（表示名 SPS for NDMF）は、3パッケージの中核となる本体パッケージです。

## 役割

VRCFury の SPS (Super Plug Shader) 機能を抽出し、NDMF プラグインとして実行するパッケージです。VRCFury 本体を導入しなくても、Modular Avatar と同じ NDMF ビルドパイプライン上で SPS の Plug / Socket 処理だけを単体で使えるようにします。

## 動作するタイミング

SPS for NDMF は、NDMF の `BuildPhase.Transforming` で **Modular Avatar より前**に実行されるプラグインとして登録されています。ビルドは次の2パスで行われます。

1. VRCFury の SPS ビルドロジックをアバターに対して実行するパス
2. その結果を Modular Avatar のコンポーネントとして出力するパス

この順序により、SPS の処理結果が MA コンポーネントとして出力された状態で、その後の MA のビルドが実行されます。内部の詳しい仕組みは [しくみ](/sps/how-it-works/) を参照してください。

## 提供コンポーネント

SPS for NDMF が実際にビルド処理の対象とするのは、**SPS Plug** と **SPS Socket** の2コンポーネントです。アバター上にこのいずれかが存在する場合にのみ SPS のビルド処理が実行されます。それぞれの詳しい使い方は [Plug の詳細](/sps/plug/)・[Socket の詳細](/sps/socket/) を参照してください。

VRCFury には、このほかに Global Collider や Haptic Touch Receiver / Sender といったコンポーネントもありますが、これらは SPS for NDMF のビルド処理の対象には含まれていません。既存の VRCFury アバターでこれらを使っている場合は、[Migrator](/packages/migrator/) で Modular Avatar / VRChat Contacts のネイティブなコンポーネントに変換して使うことを想定しています。詳しくは [移行の流れ](/packages/migration-flow/) を参照してください。
