---
title: 移行の流れ
sidebar:
  order: 5
---

このページでは、すでに VRCFury のギミック（SPS を含む）が組み込まれたアバターを、**VRCFury 本体をインストールしないまま** SPS for NDMF（SPSNDMF）へ移行するための全体フローを説明します。

## このページの目的

「VRCFury 本体を導入せずに SPS だけを使いたい」というニーズ（詳しくは [SPS for NDMF とは](/guides/about/)）は、新規制作だけでなく「すでに VRCFury で組んだアバターがある」場合にも当てはまります。この場合、単に VRCFury 本体を消すだけでは、シーンやプレハブに残った VRCFury コンポーネントの設定が読み込めなくなってしまいます。ここでは、[VRCFury Stub](/packages/vfstub/) と [SPSNDMF Migrator](/packages/migrator/) を使って、コンポーネントを壊さずに SPSNDMF へ移行する手順を示します。

## 全体像

移行は次の4段階で行います。

1. **VRCFury 本体を撤去** — プロジェクトから VRCFury 本体パッケージを削除します。
2. **VRCFury Stub で既存コンポーネントを保持** — VRCFury Stub（`net.nrzk.vfstub`）を導入し、シーン・プレハブ内の VRCFury コンポーネントの設定を保持したまま開ける状態にします。
3. **SPSNDMF Migrator で VRCFury → SPSNDMF 変換** — SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`）を使い、Plug/Socket などの VRCFury コンポーネントを SPSNDMF や Modular Avatar / VRChat Contacts のコンポーネントへ変換します。
4. **SPS for NDMF でビルド** — SPS for NDMF（`net.nrzk.spsndmf`）を導入し、通常どおり NDMF/Modular Avatar のビルドを実行します。

## 各ステップの詳細

### ① VRCFury 本体を撤去

VRCFury 本体パッケージをプロジェクトから削除します。この時点ではまだ VRCFury Stub を導入していないため、シーンやプレハブに残った VRCFury コンポーネントの設定は読み込めなくなります。次のステップで VRCFury Stub を導入するまでの一時的な状態です。

### ② VRCFury Stub で既存コンポーネントを保持

VRCFury Stub（`net.nrzk.vfstub`）を導入します。VRCFury Stub は、VRCFury 本体と同じ扱いでコンポーネントのデータを読み込めるようにするための互換パッケージです。これを入れておくことで、シーンやプレハブに保存されていた VRCFury コンポーネントの設定が壊れたり消えたりすることなく、そのままロードできる状態になります。VRCFury Stub 自体には編集機能やビルド処理は含まれていないため、この段階ではまだ SPS のビルドは行われません。

### ③ SPSNDMF Migrator で VRCFury → SPSNDMF 変換

SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`）を導入し、`Tools/SPSNDMF/Migrator` から変換対象の GameObject を選び、`VRCFury → SPSNDMF` 方向で変換します。プレビューで変換内容を確認してから実行でき、実行後も Ctrl+Z で取り消し可能です。VRCFury 本体がなくても、VRCFury Stub が提供するコンポーネントに対して変換処理が動作します。変換対象コンポーネントの詳細は [SPSNDMF Migrator の変換対象表](/packages/migrator/#変換対象) を参照してください。

### ④ SPS for NDMF でビルド

SPS for NDMF（`net.nrzk.spsndmf`）を導入し、いつもどおり NDMF/Modular Avatar の非破壊ビルドを実行します。SPS for NDMF は Modular Avatar より前のタイミングでビルドに加わり、変換済みの SPS Plug / Socket を処理します。

## 注意点

- Migrator の変換対象のうち、**VRCFury Plug/Socket は双方向**（`VRCFury ⇔ SPSNDMF`）に変換できますが、**Global Collider・Haptic Touch Receiver・Haptic Touch Sender は VRCFury → SPSNDMF 方向のみ**の変換です。SPSNDMF 側から VRCFury 側へ戻す変換はできません。
- Global Collider や Haptic Touch Receiver/Sender は、SPS for NDMF 本体のビルド処理では扱われず、SPSNDMF Migrator によって Modular Avatar の Global Collider や VRChat の Contact Receiver/Sender といったネイティブなコンポーネントへ変換されます。
- SPSNDMF Migrator には、`Tools/SPSNDMF/Migrator` からの手動変換とは別に、VRCFury Stub が入った環境でアップロード・テスト用のビルドを行うと、VRCFury → SPSNDMF 相当の変換をその場で自動的に行う機能も含まれています。これはビルド時に一時的に複製されたアバターに対して行われる処理のため、シーン上の元のオブジェクトを書き換えることはなく、Ctrl+Z による取り消しの対象にもなりません（手動変換でのプレビュー→実行→Ctrl+Z の対象は、あくまで `Tools/SPSNDMF/Migrator` からの操作です）。
