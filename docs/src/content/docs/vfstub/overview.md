---
title: VRCFury Stub
sidebar:
  order: 1
---

VRCFury Stub（`net.nrzk.vfstub`）は、VRCFury 本体を使わずに VRCFury コンポーネント入りのシーン・プレハブを扱うための互換パッケージです。

:::caution
VRCFury Stub は **VRCFuryと同時にインストールすることはできません。** インストールする時はVRCFuryをアンインストールしてから！
:::

## 役割

VRCFury 本体を入れていなくても、既存のシーンやアバターに設定済みの VRCFury のコンポーネント（設定）が壊れたり消えたりせずにそのまま開けるようにするための互換用パッケージです。VRCFury 本体と同じ扱いでコンポーネントのデータを読み込めるようにする、という仕組みで実現しています。

## 編集機能やビルド処理は持たない

VRCFury Stub はコンポーネントの設定を保持するためだけのパッケージで、**VRCFury のような編集機能やビルド処理は一切含みません**。つまり、VRCFury Stub を入れただけでは SPS のビルドは行われません。ビルド時にコンポーネントを SPSNDMF 側の形式へ自動変換する処理は、[SPSNDMF Migrator](/migrator/overview/) パッケージが担います。

## 他のツールから SPS を設定する API

VRCFury には、他のツールがコードから VRCFury Socket を作って設定するための公開 API（`com.vrcfury.api`）があります。VRCFury Stub はこの API のうち **SPS に関わる部分** を同梱しているので、この API を使って SPS を組み込むツールは、VRCFury Stub の環境でも利用できます。

この API は、書き込まれた設定を実際にビルドできる環境、つまり [SPS for NDMF](/spsndmf/overview/) と [SPSNDMF Migrator](/migrator/overview/) の両方がインストールされているときだけ有効になります。どちらかが欠けている場合、API は存在しない扱いになります。

VRCFury Stub は SPS 専用のパッケージなので、SPS 以外の機能を作る API は含みません。

## いつ使うか

すでに VRCFury のギミック（SPS を含む）が組み込まれたプロジェクトから、VRCFury 本体を撤去したいときに使います。VRCFury 本体を削除すると、シーンやプレハブに残った VRCFury コンポーネントは設定を読み込めずに壊れてしまいますが、VRCFury Stub を代わりに導入しておくことで、コンポーネントの設定を保持したまま開ける状態を維持できます。

具体的な移行手順は [移行の流れ](/migrator/migration-flow/) にまとめています。
