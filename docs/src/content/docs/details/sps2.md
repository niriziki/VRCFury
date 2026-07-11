---
title: SPS2 の詳細
sidebar:
  order: 1
---

これは **SPS2**（0.2.x 系、SPS for NDMF が実装している方式）のしくみです。SPS1（0.1.x 系）との違いは [SPS1 と SPS2](/spsndmf/versions/) を参照してください。実用的な設定項目だけを知りたい場合は、このページを読む必要はありません。

## SPS の変形

[Plug](/spsndmf/plug/) は、近くにある [Socket](/spsndmf/socket/) の方向へ向けて、GPU 上のシェーダー計算によってリアルタイムに曲がります。この処理はビルド時に Plug のシェーダーへ組み込まれ、実行中は追加のスクリプトやアニメーションクリップを必要としません。

近くに複数の Socket がある場合は、タグ設定などをもとに対象が自動的に選ばれます。SPS2 は位置の伝達に Unity の点光源を使わないため、SPS1 のような「同時に認識できる Socket 数の実質的な上限」が無く、多数の Socket を同時に有効化できます。

## NDMF ビルドでの処理の流れ

SPS for NDMF は、NDMF の非破壊ビルドの中で、Modular Avatar より前にアバター上の Plug / Socket を処理します。この処理は、アバター上に Plug または Socket が1つも無い場合は実行されません。また、SPS for NDMF は Plug / Socket 以外の VRCFury コンポーネント（[Global Collider](/migrator/global-collider/) や [Haptic Touch](/migrator/haptic-touch/) など）には影響を与えません。
