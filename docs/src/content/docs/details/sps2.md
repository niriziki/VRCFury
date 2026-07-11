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

この共有テクスチャは、描画中の画面を一時的に取得する Unity 標準の仕組み（GrabPass）を使っています。アバターが持つテクスチャ資産ではないため、パフォーマンスランクの Texture VRAM を増やすものではありません。

## Socket 側の反応（挿入時の変形）

Socket 自体は SPS のシェーダーで変形するわけではありません（シェーダーで曲がるのは Plug のメッシュです）。Socket が挿入に応じて見た目を変える（穴が開く等）場合は、Socket の **Depth Animations** を使います。これは VRChat の Contacts で挿入の深さを測り、あらかじめ用意したブレンドシェイプやアニメーションを駆動する仕組みです。

この Depth Animations の仕組みは **SPS1 でも SPS2 でも同じ**です。SPS1 から SPS2 で変わったのは Plug が Socket へ向かうときの「位置の伝え方」（点光源 → 共有テクスチャ）だけで、Socket 側の反応のしくみは変わっていません。設定方法は [Socket](/spsndmf/socket/) の Depth Animations を参照してください。

## 使用する Contacts の内訳

SPS2 は位置の伝達を共有テクスチャで行いますが、触覚通知や一部の検出には VRChat Contacts も使います。既定設定でのおおよその内訳は次のとおりです。

**Plug 1個あたり（生成 約12個）**

| 用途 | 種類 | 数 | 有効になる条件 |
|---|---|---|---|
| 位置・レガシー検出 | Sender | 4 | 常時 |
| 触覚通知（OGB） | Receiver | 8 | 自分のクライアントで OSC 触覚アプリを起動している間だけ |

**Socket 1個あたり（生成 約6〜13個）**

| 用途 | 種類 | 数 | 有効になる条件 |
|---|---|---|---|
| 位置・レガシー検出 | Sender | 2 | メニューでこの Socket を ON にしている間 |
| 触覚通知（OGB） | Receiver | 4〜11 | メニュー ON かつ OSC 触覚アプリを起動している間だけ |

このほか、[Depth Animations](/spsndmf/socket/) を使う Plug / Socket では、挿入の深さを測る Receiver が数個ずつ追加されます。

「有効になる条件」に注目すると、**通常のプレイ（OSC 触覚アプリを起動していない）では OGB の Receiver は有効になりません**。SPS2 が実際に同時に有効化する Contacts は、Plug 側の検出 Sender（約4個）に、メニューで ON にした Socket があればその検出 Sender（2個）が加わる程度の少数に収まります。生成される総数よりも、この「実際に同時に有効な数」が他アバターとの干渉のしやすさに効きます。

検出用の Contacts が **Sender**（発信）なのは、Plug / Socket が「ここに Plug（Socket）があります」と発信するビーコンだからです。それを読む **Receiver は、1つの Plug / Socket の中に対で用意されているのではなく、別の場所**にあります。

- **相手のアバター側** — レガシー（DPS/TPS/SPS1）の相手や、触覚のやり取りの相手が、この Sender を自分の Receiver で検出します。
- **同じアバター内の別の Receiver** — Depth Animations や Auto Mode、OGB の Receiver が、この検出 Sender のタグを読み取ります（表で別に数えている Receiver 系がこれにあたります）。

なお、**SPS2 自身の「Plug が Socket へ曲がる」検出は共有テクスチャで行う**ため、これらの検出 Sender は SPS2 のコア検出には使われていません（用途はレガシー／相手アバター向けの発信と、上記 Receiver に読ませるためのタグ源です）。

[SPS1](/details/sps1/) では、これらに加えて Plug 側に「近くの Socket を探すための Receiver（SPS Plus）」やスケール補正用の Contacts が常時付いていました。SPS2 ではこの検出を共有テクスチャ方式へ移したためそれらが不要になりましたが、**触覚通知（OGB）と検出用 Sender は SPS2 でも同じように使う**ため、Contacts が大きく減るわけではありません。

## NDMF ビルドでの処理の流れ

SPS for NDMF は、NDMF の非破壊ビルドの中で、Modular Avatar より前にアバター上の Plug / Socket を処理します。この処理は、アバター上に Plug または Socket が1つも無い場合は実行されません。また、SPS for NDMF は Plug / Socket 以外の VRCFury コンポーネント（[Global Collider](/migrator/global-collider/) や [Haptic Touch](/migrator/haptic-touch/) など）には影響を与えません。
