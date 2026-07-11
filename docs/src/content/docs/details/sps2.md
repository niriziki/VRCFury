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

## 使用する Contacts

SPS2 の「Plug が Socket へ曲がる」というコアな動きは[共有テクスチャ](#位置を伝えるしくみ動作原理)で行うため、**Contacts を必要としません**。Contacts は、次の付加機能のためだけに使われます（数は Plug 1個・Socket 1個あたり。Sender は発信、Receiver は受信）。

| 用途 | 種別 | Plug | Socket | 役割 | 有効になる条件 |
|---|---|---|---|---|---|
| 位置ビーコン | Sender | 4 | 2 | 「ここに Plug／Socket がある」と発信する。下の Receiver や旧方式（DPS/TPS）の相手が読む | Plug は常時／Socket はメニュー ON 時 |
| 触覚通知（OGB） | Receiver | 8 | 4〜11 | 位置ビーコンを検知し、触覚アプリに接触・挿入を伝える | OSC 触覚アプリを起動している間だけ |
| Depth Animations | Receiver | 1〜2 | 3〜6 | 挿入の深さを測り、ブレンドシェイプ等を動かす | [Depth Animations](/spsndmf/socket/) を設定したときだけ |
| Auto 選択 | Receiver | — | 1※ | 複数の Socket から最寄りを自動で選ぶ | Auto 対象の Socket が2個以上のとき |

※ Auto 選択の Receiver はアバター全体で共有の1個。

**位置ビーコン（Sender）を読むのは、上表の「触覚通知」と「Depth Animations」の Receiver です。** Plug のビーコンは Socket 側のこれらの Receiver が、Socket のビーコンは Plug 側のこれらの Receiver が読みます（読み手は自分・相手どちらのアバターにもあります）。旧方式（DPS/TPS）の相手も同様に読みます。

ただし、**基本的な使い方（触覚アプリなし・Depth Animations 未設定）では、これらの Receiver は動きません**（OGB はゲートで無効、Depth は未設定なら存在しない）。そのため位置ビーコンは旧方式の相手向けの発信として残るだけで、SPS2 のコアな動きは Contacts 抜きで完結します。これが SPS2 で実際に効く Contacts が少ない理由です。位置ビーコンは Socket の「Legacy Compatibility」設定では消えません（この設定は点光源だけを切り替えます）。

### 実際に有効な Contacts の数

干渉のしやすさは、生成された総数よりも**同時に有効な数**で決まります。通常のプレイ（OSC アプリなし）で実際に有効な Contacts の数は、**Plug の数**と、**メニューで ON にしている Socket の数**で決まります（OFF の Socket は 0）。

| 世代 | 実際に有効な数（おおよそ） |
|---|---|
| SPS1 | 18 ×（Plug 数）＋ 6〜13 ×（ON の Socket 数） |
| SPS2 | 4 ×（Plug 数）＋ 2 ×（ON の Socket 数） |

たとえば Plug 1・ON の Socket 1 なら、SPS1 は約 24〜31 個、SPS2 は約 6 個です。

SPS2 で少ないのは、触覚通知（OGB）の Receiver がゲートで無効だからです（アプリを起動すれば有効になります）。SPS1 はこのゲートが無く、さらに半径3mと大きい「SPS Plus」Receiver も常時有効なため、同時に有効な Contacts がずっと多くなります。

## NDMF ビルドでの処理の流れ

SPS for NDMF は、NDMF の非破壊ビルドの中で、Modular Avatar より前にアバター上の Plug / Socket を処理します。この処理は、アバター上に Plug または Socket が1つも無い場合は実行されません。また、SPS for NDMF は Plug / Socket 以外の VRCFury コンポーネント（[Global Collider](/migrator/global-collider/) や [Haptic Touch](/migrator/haptic-touch/) など）には影響を与えません。
