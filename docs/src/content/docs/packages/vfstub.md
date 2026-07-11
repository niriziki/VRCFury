---
title: net.nrzk.vfstub（VRCFury Stub）
sidebar:
  order: 3
---

`net.nrzk.vfstub`（表示名 VRCFury Stub）は、VRCFury 本体を使わずに VRCFury コンポーネント入りのシーン・プレハブを扱うための互換パッケージです。

## 役割

VRCFury Runtime の**型のみのスタブ**です。`VF.*` 名前空間・VRCFury 本体と同一の GUID でコンポーネント型を提供し、VRCFury 本体を入れなくても、既存のシーンやプレハブに埋め込まれた VRCFury コンポーネントを保持したままロードできるようにする互換レイヤです。

## エディタ・ビルド処理は持たない

VRCFury Stub は型（コンポーネントのデータ構造）だけを提供するパッケージで、**エディタロジックやビルド処理は一切含みません**。つまり、VRCFury Stub を入れただけでは SPS のビルドは行われません。ビルド時にコンポーネントを SPSNDMF 側の形式へ自動変換する処理は、[Migrator](/packages/migrator/) パッケージが担います。

## いつ使うか

すでに VRCFury のギミック（SPS を含む）が組み込まれたプロジェクトから、VRCFury 本体を撤去したいときに使います。VRCFury 本体を削除すると、シーンやプレハブに残った VRCFury コンポーネントは型が解決できずに壊れて（GUID 参照が失われて）しまいますが、VRCFury Stub を代わりに導入しておくことで、コンポーネントの型・データを保持したままロードできる状態を維持できます。

具体的な移行手順は [移行の流れ](/packages/migration-flow/) にまとめています。
