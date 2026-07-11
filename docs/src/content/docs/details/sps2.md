---
title: SPS2 の詳細
sidebar:
  order: 1
---

これは **SPS2**（0.2.x 系、SPS for NDMF が実装している方式）のしくみです。SPS1（0.1.x 系）との違いは [SPS1 と SPS2](/spsndmf/versions/) を参照してください。実用的な設定項目だけを知りたい場合は、このページを読む必要はありません。

## SPS の変形

[Plug](/spsndmf/plug/) は、近くにある [Socket](/spsndmf/socket/) の方向へ向けて、GPU 上のシェーダー計算によってリアルタイムに曲がります。この処理はビルド時に Plug のシェーダーへ組み込まれ、実行中は追加のスクリプトやアニメーションクリップを必要としません。

近くに複数の Socket がある場合は、タグ設定などをもとに対象が自動的に選ばれます。

## 位置を伝えるしくみ（動作原理）

SPS2 は、Socket の位置や種類を **共有テクスチャ**を介して Plug へ伝えます。Socket 側のシェーダーが「どこに、どの種類の Socket があるか」を1枚のテクスチャに書き込み、Plug 側のシェーダーがそのテクスチャを読み取って、メッシュを対象の Socket へ向けて曲げます。GPU 上のシェーダー同士が共有のテクスチャをやり取りの場として使うイメージです。

Unity の点光源を使わないため、[SPS1](/details/sps1/) のような「同時に認識できる Socket 数の実質的な上限」や「VRChat のライト設定への依存」がありません。多数の Plug / Socket を同時に扱えるほか、この方式によってタグでの絞り込みや複数段の経路（Guided Path）といった高度な指定も可能になっています。

## Socket 側の反応（挿入時の変形）

Socket 自体は SPS のシェーダーで変形するわけではありません（シェーダーで曲がるのは Plug のメッシュです）。Socket が挿入に応じて見た目を変える（穴が開く等）場合は、Socket の **Depth Animations** を使います。これは VRChat の Contacts で挿入の深さを測り、あらかじめ用意したブレンドシェイプやアニメーションを駆動する仕組みです。

この Depth Animations の仕組みは **SPS1 でも SPS2 でも同じ**です。SPS1 から SPS2 で変わったのは Plug が Socket へ向かうときの「位置の伝え方」（点光源 → 共有テクスチャ）だけで、Socket 側の反応のしくみは変わっていません。設定方法は [Socket](/spsndmf/socket/) の Depth Animations を参照してください。

## NDMF ビルドでの処理の流れ

SPS for NDMF は、NDMF の非破壊ビルドの中で、Modular Avatar より前にアバター上の Plug / Socket を処理します。この処理は、アバター上に Plug または Socket が1つも無い場合は実行されません。また、SPS for NDMF は Plug / Socket 以外の VRCFury コンポーネント（[Global Collider](/migrator/global-collider/) や [Haptic Touch](/migrator/haptic-touch/) など）には影響を与えません。
