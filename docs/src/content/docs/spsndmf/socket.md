---
title: Socket
sidebar:
  order: 4
---

Socket は、SPS の中で Plug を受け入れる側のコンポーネントです。[SPS for NDMF が実際にビルド処理の対象とする](/spsndmf/overview/)2つのコンポーネントのうちの1つです。

## これは何か

Socket（旧称 Orifice）は、挿入される側の部位に取り付けて使うコンポーネントです。Socket 自身が Plug のように変形するわけではなく、Plug が向かって曲がってくるための目印として機能します。

## 主な設定

Socket コンポーネントをアバターに追加すると、インスペクターで次のような項目を設定できます。

### 変形の設定（Enable Deformation）

- **Enable Deformation** — この Socket を SPS の対象にするかどうかのメインスイッチです。
- **Mode** — Socket の種類を選びます。
  - **Auto** — 取り付けたボーンから自動的に判定します（目安として、頭やあごなど「奥がある」部位や腰まわりでは Hole、それ以外の部位では Ring になります）。
  - **Hole** — 穴のように奥で収束するタイプです。
  - **Ring** — 輪のような形状で、両側から挿入できます。
  - **One-Way Ring** — Ring と似た形状ですが、片側からのみ挿入できます。通常はほとんど使いません。
- **Radius Offset** — Plug の向かう先を、Socket の上方向へ Plug の太さぶんオフセットします。手のひらのような「表面に沿わせる」用途向けの設定です。
- **Guided Path** — 中継地点（Transform）を最大3つまで追加できます。追加すると、Plug は Socket 本体を通過したあと、その地点を順にたどるように曲がります。

### メニュー

- **Enable Menu Toggle** — この Socket をアバターのメニューから ON / OFF できるようにします。メニュー上の名前、Auto 選択（近くの Plug に応じて自動的に選ばれる Socket に含めるか）、メニューアイコンを設定できます。

### Depth Animations / Active Animation / OGB Haptics

- **Enable Depth Animations** — Plug との距離に応じて、任意のアニメーションを駆動できる機能です。
- **Enable Active Animation** — メニューでこの Socket が有効になっている間だけ再生されるアニメーションです。
- **ID sent to OGB** — 触覚（ハプティクス）対応アプリへ送信する識別名です。
- **Enable hand touch zone?** — 手で触れたときにも反応する範囲を追加するかどうかです（On / Auto / Off から選択。Auto は腰まわりに配置されている場合のみ追加されます）。

### Tags

- **Tags** — この Socket を対象にできる Plug をタグで絞り込みます。既定では「Global SPS2 タグ」が有効になっており、ほとんどの Plug から対象にされます。

## 仕組み

Socket は、Plug が向かって曲がってくるための目印です。近くに Socket がある場合、[Plug](/spsndmf/plug/) 側のシェーダーがその方向を検出し、GPU 上のリアルタイム計算で Plug のメッシュを曲げます。
