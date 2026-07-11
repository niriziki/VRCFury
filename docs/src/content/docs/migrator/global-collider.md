---
title: Global Collider
sidebar:
  order: 2
---

Global Collider は、SPS 本体とは別に VRCFury が提供している、関連するハプティクス機能です。[SPS for NDMF はこのコンポーネントをビルド処理の対象にしていません](/spsndmf/overview/)。

## これは何か

Global Collider は、アバターの指以外の任意の位置を、他のプレイヤーの PhysBone（揺れもの）や触覚通知に反応する「指」のように振る舞わせるための機能です。VRChat のアバターは指ごとに専用のコライダーを持っていますが、Global Collider はそのうちの1本を、指定した任意の Transform の位置・大きさに割り当て直すことでこれを実現します。

たとえば SPS の Plug の先端など、指以外の場所で他のアバターの PhysBone に触れたい場合や、触覚対応アプリへ通知を送りたい場合に使われます。

## 主な設定

VRCFury の Global Collider には次の設定があります。

- **Root Transform Override** — コライダーを配置する基準の Transform です。未指定の場合は、コンポーネントを付けたオブジェクト自身の位置が使われます。
- **Radius** — コライダーの半径です。
- **Height** — コライダーの高さ（カプセル形状の長さ）です。

## 仕組みと対応

SPS for NDMF は、Plug と Socket 以外の VRCFury コンポーネントをビルド処理の対象にしていません。Global Collider もこの対象外のコンポーネントの一つで、SPSNDMF 環境では VRCFury の Global Collider コンポーネントをそのまま使うことはできません。

SPSNDMF 環境で同等の機能を使いたい場合は、代わりに **Modular Avatar の Global Collider** コンポーネントを使います。すでに VRCFury の Global Collider が設定されたアバターがある場合は、[SPSNDMF Migrator](/migrator/overview/) を使うと、半径・高さ・基準 Transform の設定を引き継いだ状態で Modular Avatar の Global Collider へ変換できます（VRCFury → SPSNDMF 方向のみ）。詳しくは [移行の流れ](/migrator/migration-flow/) を参照してください。
