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

SPS2 の「Plug が Socket へ向けて曲がる」というコアな動作は[共有テクスチャ](#位置を伝えるしくみ動作原理)で行うため、**それ自体には VRChat Contacts を必要としません**（同じ画面に相手の Socket も描かれるので、Contacts 無しで相手の Socket も検出できます）。Contacts は、次の付加的な機能のためだけに使われます。

Contacts には、位置を**発信する Sender** と、それを**検知する Receiver** の2種類があります。SPS2 が使う Contacts を役割ごとに並べると次のようになります（数は Plug 1個・Socket 1個あたりの目安）。

| Contact | 種別 | 数 | 役割 | 有効になる条件 |
|---|---|---|---|---|
| 位置ビーコン | Sender | Plug 4 ／ Socket 2 | 「ここに Plug（Socket）があります」と発信する。下の Receiver や、旧方式（DPS/TPS）の相手がこれを読み取る | Plug は常時／Socket はメニューで ON にしている間 |
| 触覚通知（OGB） | Receiver | Plug 8 ／ Socket 4〜11 | 相手（または自分）の位置ビーコンを検知し、触覚アプリに「触れた・挿入された」ことを伝える | 自分のクライアントで OSC 触覚アプリを起動している間だけ |
| Depth Animations | Receiver | 数個（設定時のみ） | Plug と Socket の距離＝挿入の深さを測り、その値でブレンドシェイプ等を動かす | [Depth Animations](/spsndmf/socket/) を設定したときだけ |
| Auto 選択 | Receiver | 1（アバターで共有） | 複数の Socket から最寄りのものを自動で選ぶ | Auto 対象の Socket が2個以上あるとき |

**Sender と Receiver の関係**: 位置を発信する側が Sender、それを検知して機能を動かす側が Receiver です。Plug が出す位置ビーコン（Sender）は、**Socket 側の Receiver**（触覚通知・Depth Animations）が読み取り、Socket が出す位置ビーコンは **Plug 側の Receiver** が読み取ります。この Receiver は自分のアバターにも相手のアバターにもあり、自分自身との組み合わせ（Self）と他人との組み合わせ（Others）の両方に対応しています。旧方式（DPS/TPS）を使っている相手も、この位置ビーコンを自分の Receiver で検知します。

位置ビーコン（Sender）は、SPS2 自身の触覚通知・Depth Animations・Auto 選択が読むため常に生成されます（VRCFury の Socket の「Legacy Compatibility」設定は点光源の出力だけを切り替えるもので、この Sender の生成は止めません）。

### 実際に有効になっている Contacts の数

生成される Contacts の総数よりも、**実行時に同時に有効になっている数**が、他アバターとの干渉のしやすさに効きます。通常のプレイ（OSC 触覚アプリを起動していない）で、Socket を1つメニューで ON にした状態（Plug 1・Socket 1）を比べると、次のようになります。

| 世代 | 実際に有効な合計 | 内訳 |
|---|---|---|
| SPS1 | 約 24個 | Sender 約7 ＋ Receiver 約17 |
| SPS2 | 約 6個 | Sender 約6 ＋ Receiver 0 |

SPS2 で Receiver が 0 なのは、触覚通知（OGB）の Receiver が「OSC 触覚アプリ起動時のみ有効」というゲートで**無効化されている**ためです（Receiver が無いわけではなく、アプリを起動すれば有効になります）。SPS1 にはこのゲートが無く OGB の Receiver が常に有効なため、同時に有効な Contacts がずっと多くなります。加えて SPS1 は、SPS2 に無い「近くの Socket を探す Receiver（SPS Plus、半径3mと大きい）」も常時有効です。

## NDMF ビルドでの処理の流れ

SPS for NDMF は、NDMF の非破壊ビルドの中で、Modular Avatar より前にアバター上の Plug / Socket を処理します。この処理は、アバター上に Plug または Socket が1つも無い場合は実行されません。また、SPS for NDMF は Plug / Socket 以外の VRCFury コンポーネント（[Global Collider](/migrator/global-collider/) や [Haptic Touch](/migrator/haptic-touch/) など）には影響を与えません。
