---
title: クイックスタート
sidebar:
  order: 3
---

SPS for NDMF を使って、アバターに Plug / Socket を付けてビルドするまでの最短手順です。[インストール](/guides/install/) を先に済ませておいてください。

## このページでできること

このページでは、Modular Avatar で組んだアバターに SPS の Plug と Socket を追加し、アバターに SPS が組み込まれるまでの流れを紹介します。詳細な設定項目やシェーダー・Contacts の仕組みについては、このページでは扱いません（[Plug](/sps/plug/)・[Socket](/sps/socket/) の各ページを参照してください）。

## Plug を付ける

1. Hierarchy で、Plug を付けたいオブジェクト（アバターの対象ボーンなど）を選択します。
2. メニューの `GameObject > VRCFury > Create SPS Plug`（または `Tools > VRCFury > SPS > Create Plug`）を実行します。
3. 選択していたオブジェクトの子として `SPS Plug` という GameObject が作成され、Plug 用のコンポーネントが追加されます。
4. 作成後に表示されるダイアログの案内どおり、適切なボーンの下に配置されているか、正しい向きを向いているかを確認・調整します。

Plug の詳しい設定（形状の選択、アニメーション対応など）は [Plug の詳細](/sps/plug/) を参照してください。

## Socket を付ける

1. Hierarchy で、Socket を付けたいオブジェクトを選択します。
2. メニューの `GameObject > VRCFury > Create SPS Socket`（または `Tools > VRCFury > SPS > Create Socket`）を実行します。
3. 選択していたオブジェクトの子として `SPS Socket` という GameObject が作成され、Socket 用のコンポーネントが追加されます。
4. Plug と同様に、位置と向きを確認・調整します。

Socket の詳しい設定は [Socket の詳細](/sps/socket/) を参照してください。

## ビルドして確認

Plug / Socket を配置したら、いつもどおり Unity のプレイモードに入るか、VRChat SDK の Control Panel から Build & Test / アップロードを行います。

SPS for NDMF は Modular Avatar より前のタイミングでこの非破壊ビルドに加わり、配置した Plug / Socket の設定を MA のコンポーネントとして自動的に組み込みます。特別な操作をしなくても、この一連の流れの中で SPS が組み込まれた状態のアバターがアップロードされます。
