---
title: 移行の流れ
sidebar:
  order: 5
---

このページでは、すでに VRCFury のギミック（SPS を含む）が組み込まれたアバターを、**VRCFury 本体をインストールしないまま** SPS for NDMF（SPSNDMF）へ移行するための全体フローを説明します。

## このページの目的

VRCFury 本体を導入せずに SPS だけを使いたい、という [SPS for NDMF とは](/guides/about/) のニーズは、新規制作だけでなく「すでに VRCFury で組んだアバターがある」場合にも当てはまります。この場合、単に VRCFury 本体を消すだけでは、シーンやプレハブに残った VRCFury コンポーネントの型が解決できなくなってしまいます。ここでは、[VRCFury Stub](/packages/vfstub/) と [Migrator](/packages/migrator/) を使って、コンポーネントを壊さずに SPSNDMF へ移行する手順を示します。

## 全体像

移行は次の4段階で行います。

1. **VRCFury 本体を撤去** — プロジェクトから VRCFury 本体パッケージを削除します。
2. **VRCFury Stub で既存コンポーネントをロード保持** — `net.nrzk.vfstub` を導入し、シーン・プレハブ内の VRCFury コンポーネントを型ごと保持したままロードできる状態にします。
3. **Migrator で VRCFury → SPSNDMF 変換** — `net.nrzk.spsndmf-migrator` を使い、Plug/Socket などの VRCFury コンポーネントを SPSNDMF や Modular Avatar / VRChat Contacts のコンポーネントへ変換します。
4. **SPSNDMF でビルド** — `net.nrzk.spsndmf` を導入し、通常どおり NDMF/Modular Avatar のビルドを実行します。

## 各ステップの詳細

### ① VRCFury 本体を撤去

VRCFury 本体パッケージをプロジェクトから削除します。この時点ではまだ VRCFury Stub を導入していないため、シーンやプレハブに残った VRCFury コンポーネントは型を解決できなくなります。次のステップで VRCFury Stub を導入するまでの一時的な状態です。

### ② vfstub で既存コンポーネントをロード保持

`net.nrzk.vfstub` を導入します。VRCFury Stub は VRCFury Runtime の型のみのスタブで、`VF.*` 名前空間・VRCFury 本体と**同一の GUID** でコンポーネント型を提供します。GUID が同一であるため、シーンやプレハブに保存されていた VRCFury コンポーネントの参照が壊れることなく、データを保持したままロードできる状態になります。VRCFury Stub 自体はエディタロジックやビルド処理を持たないため、この段階ではまだ SPS のビルドは行われません。

### ③ Migrator で VRCFury → SPSNDMF 変換

`net.nrzk.spsndmf-migrator` を導入し、`Tools/SPSNDMF/Migrator` から変換対象の GameObject を選び、`VRCFury → SPSNDMF` 方向で変換します。プレビューで変換内容を確認してから実行でき、実行後も Ctrl+Z で取り消し可能です。VRCFury 本体がなくても、VRCFury Stub が提供する型に対して変換処理が動作します。変換対象コンポーネントの詳細は [Migrator の変換対象表](/packages/migrator/#変換対象) を参照してください。

### ④ SPSNDMF でビルド

`net.nrzk.spsndmf` を導入し、いつもどおり NDMF/Modular Avatar の非破壊ビルドを実行します。SPSNDMF は Modular Avatar より前のタイミングでビルドに加わり、変換済みの SPS Plug / Socket を処理します。

## 注意点

- Migrator の変換対象のうち、**VRCFury Plug/Socket は双方向**（`VRCFury ⇔ SPSNDMF`）に変換できますが、**Global Collider・Haptic Touch Receiver・Haptic Touch Sender は VRCFury → SPSNDMF 方向のみ**の変換です。SPSNDMF 側から VRCFury 側へ戻す変換はできません。
- Global Collider や Haptic Touch Receiver/Sender は、SPSNDMF 本体のビルド処理では扱われず、Migrator によって Modular Avatar の Global Collider や VRChat の Contact Receiver/Sender といったネイティブなコンポーネントへ変換されます。
- Migrator には、`Tools/SPSNDMF/Migrator` からの手動変換とは別に、VRCFury Stub が入った環境でビルドを実行した際に VRCFury → SPSNDMF 相当の変換をビルド時に自動的に行う NDMF ビルドパスが含まれています。これはビルド用に複製されたアバターに対して行われる処理で、Undo の対象にはなりません（手動変換で使う「プレビュー→実行→Ctrl+Z」の対象は、あくまで `Tools/SPSNDMF/Migrator` からの操作です）。
