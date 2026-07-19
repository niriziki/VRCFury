---
title: SPSNDMF Migrator
sidebar:
  order: 1
---

SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`）は、SPS 関連コンポーネントの相互変換を行うエディタ拡張です。

## 役割

VRCFury / VRCFury Stub / SPSNDMF / Modular Avatar / VRChat Contacts の間で、SPS 関連コンポーネントを相互変換するエディタ拡張です。VRCFury 本体は必須ではなく、[VRCFury Stub](/vfstub/overview/) が入っている環境でも動作します。

:::caution
VRCFury Stub は VRCFury 本体と同時にインストールすることはできません。VRCFuryをアンインストールしてから導入してください。
:::

## 使い方

1. Unity メニューから `Tools/SPSNDMF/Migrator` を開きます。
2. Hierarchy で変換対象の GameObject（通常はアバタールート）を選択します。
3. 変換方向を選びます（`VRCFury → SPSNDMF` または `SPSNDMF → VRCFury`）。※対応する方向はコンポーネントの種類によって異なります（下の「変換対象」表を参照）。
4. `VRCFury → SPSNDMF` のときは、必要に応じて追加の変換チェックボックスを選択します（既定はすべてON）。
   - **VRCFury Global Collider を MA Global Collider に変換**
   - **VRCFury Haptic Touch Receiver を VRC Contact Receiver に変換**
   - **VRCFury Haptic Touch Sender を VRC Contact Sender に変換**
   
   いずれも Plug/Socket 以外の関連コンポーネントを、対応する変換先へ含めるかどうかのチェックボックスです（詳しくは下の「変換対象」表を参照）。
5. `プレビュー` で変換予定の内容を確認し、`実行` で適用します。適用後は Ctrl+Z で取り消し可能です。

## 変換対象

| 変換元 | 変換先 | 方向 |
|---|---|---|
| VRCFury Plug/Socket | SPSNDMF Plug/Socket | 双方向 |
| VRCFury SPS Options | SPSNDMF SPS Menus | 双方向（メニュー位置の指定は手動で再設定） |
| VRCFury Global Collider | MA Global Collider | VRCFury → NDMF のみ |
| VRCFury Haptic Touch Receiver | VRC Contact Receiver × 2（子オブジェクト） | VRCFury → VRChat ネイティブコンポーネント のみ |
| VRCFury Haptic Touch Sender | VRC Contact Sender | VRCFury → VRChat ネイティブコンポーネント のみ |

Plug/Socket と SPS Options 以外の3種類は VRCFury → NDMF 方向のみに対応しており、逆方向（NDMF → VRCFury）の変換はできません。既存の VRCFury アバターをこれらのコンポーネントごと移行する場合の全体的な流れは [移行の流れ](/migrator/migration-flow/) を参照してください。
