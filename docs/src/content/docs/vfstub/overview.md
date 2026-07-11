---
title: VRCFury Stub
sidebar:
  order: 1
---

VRCFury Stub（`net.nrzk.vfstub`）は、VRCFury 本体を使わずに VRCFury コンポーネント入りのシーン・プレハブを扱うための互換パッケージです。

## 役割

VRCFury 本体を入れていなくても、既存のシーンやアバターに設定済みの VRCFury のコンポーネント（設定）が壊れたり消えたりせずにそのまま開けるようにするための互換用パッケージです。VRCFury 本体と同じ扱いでコンポーネントのデータを読み込めるようにする、という仕組みで実現しています。

## 編集機能やビルド処理は持たない

VRCFury Stub はコンポーネントの設定を保持するためだけのパッケージで、**VRCFury のような編集機能やビルド処理は一切含みません**。つまり、VRCFury Stub を入れただけでは SPS のビルドは行われません。ビルド時にコンポーネントを SPSNDMF 側の形式へ自動変換する処理は、[SPSNDMF Migrator](/migrator/overview/) パッケージが担います。

## いつ使うか

すでに VRCFury のギミック（SPS を含む）が組み込まれたプロジェクトから、VRCFury 本体を撤去したいときに使います。VRCFury 本体を削除すると、シーンやプレハブに残った VRCFury コンポーネントは設定を読み込めずに壊れてしまいますが、VRCFury Stub を代わりに導入しておくことで、コンポーネントの設定を保持したまま開ける状態を維持できます。

具体的な移行手順は [移行の流れ](/migrator/migration-flow/) にまとめています。
