---
title: Socket
sidebar:
  order: 3
---

Socket は、SPS の中で Plug を受け入れる側のコンポーネントです。[SPS for NDMF が実際にビルド処理の対象とする](/packages/spsndmf/)2つのコンポーネントのうちの1つです。

## これは何か

Socket（旧称 Orifice）は、挿入される側の部位に取り付けて使うコンポーネントです。Socket 自身が Plug のように変形するわけではなく、Plug が向かって曲がってくるための目印として機能します。

## 主な設定

Socket コンポーネントをアバターに追加すると、インスペクターで次のような項目を設定できます。

### 変形の設定（Enable Deformation）

- **Enable Deformation** — この Socket を SPS の対象にするかどうかのメインスイッチです。
- **Mode** — Socket の種類を選びます。
  - **Auto** — 取り付けたボーンから自動的に判定します（目安として、頭やあごなど「奥がある」部位や腰まわりでは Hole、それ以外の部位では Ring になります）。
  - **Hole** — 穴のように奥で収束するタイプです。
  - **Ring** — 輪のような形状で、SPS の Plug からは両側から挿入できます（DPS / TPS の Plug からは片側のみ挿入できます）。
  - **One-Way Ring** — Ring と似た形状ですが、片側からのみ挿入できます。通常はほとんど使いません。
- **Radius Offset** — Plug の向かう先を、Socket の上方向へ Plug の太さぶんオフセットします。手のひらのような「表面に沿わせる」用途向けの設定です。
- **Guided Path** — 中継地点（Transform）を最大3つまで追加できます。追加すると、Plug は Socket 本体を通過したあと、その地点を順にたどるように曲がります。

### 旧式互換・メニュー

- **Enable Legacy Compatibility** — 有効にすると、旧式の DPS / TPS Plug からもこの Socket が認識されるようになります。
- **Enable Menu Toggle** — この Socket をアバターのメニューから ON / OFF できるようにします。メニュー上の名前、Auto 選択（近くの Plug に応じて自動的に選ばれる Socket に含めるか）、メニューアイコンを設定できます。

### Depth Animations / Active Animation / OGB Haptics

- **Enable Depth Animations** — Plug との距離に応じて、任意のアニメーションを駆動できる機能です。
- **Enable Active Animation** — メニューでこの Socket が有効になっている間だけ再生されるアニメーションです。
- **ID sent to OGB** — 触覚（ハプティクス）対応アプリへ送信する識別名です。
- **Enable hand touch zone?** — 手で触れたときにも反応する範囲を追加するかどうかです（On / Auto / Off から選択。Auto は腰まわりに配置されている場合のみ追加されます）。

### Tags

- **Tags** — この Socket を対象にできる Plug をタグで絞り込みます。既定では「Global SPS2 タグ」が有効になっており、ほとんどの Plug から対象にされます。

これらの設定に加えて、Socket をビルドすると VRChat の Contacts（Sender / Receiver）がいくつか自動的に生成されます。これはユーザーが直接設定する項目ではありませんが、旧式互換や Depth Animations、触覚通知のために使われています。詳しくは次の「仕組み」を参照してください。

## 仕組み

Socket は、ビルド時に自分の位置・向き・種類（Hole / Ring など）を表す情報を画面の描画データに書き込む、専用のマーカーを生成します。[Plug](/sps/plug/) 側のシェーダーはこのマーカーを読み取ることで Socket の居場所を検出し、そこへ向かって曲がります。この一連のやり取りは Plug / Socket どちらもシェーダー計算だけで完結しており、CPU 側で毎フレーム座標計算を行うような処理は行われません。

これとは別に、Socket はビルド時に VRChat の Contacts もいくつか自動的に生成します。

- 旧式の DPS / TPS Plug から見えるようにするための Sender（Enable Legacy Compatibility が有効な場合）
- Socket と Plug の距離を計測し、Depth Animations に反映するための Receiver
- 触覚対応アプリ（OGB）へ、接触や距離の状態を伝えるための Receiver

これらの Contacts は、主に「近づいた／触れた」ことをアニメーションや外部アプリへ伝える役割を担っており、Plug がどちらの Socket へ向かって曲がるかという判定自体は、前述のシェーダー側の仕組みで行われます。Enable Menu Toggle の Auto 選択で、Plug に最も近い Socket だけを自動的に選んで ON にする機能も、同様に Contacts による距離計測を使って実現されています。
