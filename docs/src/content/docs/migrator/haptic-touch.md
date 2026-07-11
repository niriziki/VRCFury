---
title: Haptic Touch
sidebar:
  order: 3
---

Haptic Touch は、SPS 本体とは別に VRCFury が提供している、関連するハプティクス機能です。

## これは何か

Haptic Touch は、触覚（ハプティクス）対応アプリへ「触れた」ことを通知するための、シンプルな送受信コンポーネントです。Plug / Socket とは独立した汎用の接触検知機能で、次の2種類があります。

- **Haptic Touch Receiver** — 自分のアバターに取り付け、他人（または Plug など）に触れられたことを検知する側です。
- **Haptic Touch Sender** — 指などに取り付け、他人の Receiver に触れたことを伝える側です。

これらは実質的にVRC Contactsの設定を簡略化するものにすぎず、VRC Contactsで完全に置換できます。

## 主な設定

- **Haptic Touch Receiver**
  - **Radius** — 検知する範囲の半径です。
  - **ID sent to OGB** — 触覚対応アプリへ送信する識別名です。省略すると自動的に割り当てられます。
- **Haptic Touch Sender**
  - **Radius** — 検知範囲の半径です。

## 仕組みと対応

NDMF 環境では VRCFury の Haptic Touch コンポーネントをそのまま使うことはできません。

NDMF 環境で同等の機能を使いたい場合は、代わりに **VRChat 標準の Contacts（Contact Receiver / Contact Sender）** を使います。すでに VRCFury の Haptic Touch が設定されたアバターがある場合は、[SPSNDMF Migrator](/migrator/overview/) を使って次のように変換できます（いずれも VRCFury → NDMF 方向のみ）。

- **Haptic Touch Receiver** → 同じ半径の **VRC Contact Receiver ×2**（「自分が触れたとき用」「他人が触れたとき用」）が、子オブジェクトとして生成されます。
- **Haptic Touch Sender** → 同じ半径を持つ **VRC Contact Sender**（コリジョンタグ "Finger"）が追加されます。

詳しくは [移行の流れ](/migrator/migration-flow/) を参照してください。
