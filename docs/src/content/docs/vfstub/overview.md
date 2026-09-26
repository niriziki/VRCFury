---
title: VRCFury Stub
sidebar:
  order: 1
---

VRCFury Stub（`net.nrzk.vfstub`）は、VRCFury 本体を入れずに、VRCFury のコンポーネントを設定ごと保持するためのパッケージです。VRCFury から [SPS for NDMF](/spsndmf/overview/) へ移行するときに、VRCFury の代わりに入れます。手順は [移行の流れ](/migrator/migration-flow/) を参照してください。

:::caution
VRCFury と同時にはインストールできません。VRCFury をアンインストールしてから導入してください。
:::

## 編集

**SPS Plug**・**SPS Socket**・**Global Collider**・**Haptic Touch** は、VRCFury と同じインスペクタで編集できます。[ギズモの表示](/spsndmf/gizmos/)と[プレハブの中の Socket / Plug の編集](/spsndmf/prefab-instances/)（既定では編集不可）は SPS for NDMF と同じです。メニューは **Tools > VRCFury Stub** にあります。

インスペクタとメニューを使うには、NDMF と Modular Avatar が必要です（SPS for NDMF を使っていれば入っています）。

## ビルド

VRCFury Stub 自体はビルドを行いません。上の4つのコンポーネントは、SPS for NDMF と [SPSNDMF Migrator](/migrator/overview/) の両方を入れると、ビルド時に変換されてアバターに反映されます。それ以外の VRCFury の機能はビルドされません。

## 公開 API

VRCFury には、他のツールがコードから SPS Socket を作成・設定するための公開 API（`com.vrcfury.api`）があります。VRCFury Stub はこの API の SPS に関わる部分を同梱しているので、この API で SPS を設定するツールを VRCFury Stub の環境でも使えます。API が有効になるのは、SPS for NDMF と SPSNDMF Migrator の両方が入っているときだけです。
