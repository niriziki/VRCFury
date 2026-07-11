---
title: Plug
sidebar:
  order: 3
---

Plug は、SPS の中で Socket に向かって曲がっていく側のコンポーネントです。[SPS for NDMF が実際にビルド処理の対象とする](/spsndmf/overview/)2つのコンポーネントのうちの1つです。

## これは何か

Plug（旧称 Penetrator）は、挿入する側の部位（ペニスなど）のメッシュに取り付けて使うコンポーネントです。取り付けたメッシュは、近くに Socket があるとその方向へ滑らかに変形し、Socket が無い状態や範囲外にいる間は通常のモデリングされた形状のまま表示されます。

## 主な設定

Plug コンポーネントをアバターに追加すると、インスペクターで次のような項目を設定できます。

### サイズとマスク

- **Automatically find mesh** — 対象のメッシュ（レンダラー）を自動検出するかどうかです。オフにすると対象レンダラーを手動で指定できます。
- **Detect length from mesh** / **Length** — Plug の長さを自動計測するかどうかです。オフの場合は数値で直接指定します。
- **Detect radius from mesh** / **Radius** — Plug の太さを自動計測するかどうかです。オフの場合は数値で直接指定します。
- **Automatically mask using bone weights** — スキンメッシュのボーンウェイトをもとに、変形させる範囲を自動的に絞り込みます。
- **Optional additional texture mask** — テクスチャを使って、変形させない・長さ計算に使わない範囲を追加で指定できます。

### 変形の設定（Enable Deformation）

- **Enable Deformation** — SPS による変形を有効にするかどうかのメインスイッチです。
- **Auto-Rig** — ボーンの無い静止したメッシュに対して、自動的にボーンと PhysBone を追加し、揺れを付けます。
- **Post-Bake Actions** — 変形計算の基準になる「まっすぐな姿勢」を確定させたあとに、見た目のポーズを調整するためのアクションを追加できます。
- **Animated Toggle** — 変形の ON / OFF をアニメーションクリップで切り替えられるようにします。特定の状況で変形を止めたい場合に使います。
- **Animated blendshapes to keep while deforming** — 変形中も反映させたいブレンドシェイプを、最大16個まで指定できます。指定しないブレンドシェイプは、変形中はエディタ上の見た目に固定されます。
- **Allow Hole Overrun** — Hole（[Socket](/spsndmf/socket/)を参照）に最後まで挿入されたとき、わずかにめり込ませることで見た目の破綻を防ぎます。

### Depth Animations / OGB Haptics / Tags

- **Enable Depth Animations** — Socket との距離に応じて、任意のアニメーションを駆動できる機能です。
- **ID sent to OGB** — 触覚（ハプティクス）対応アプリへ送信する識別名です。省略すると自動的に割り当てられます。
- **Tags（Include / Exclude）** — この Plug が対象にできる／できない Socket をタグで絞り込みます。既定では「Global SPS2 タグ」が含まれており、ほとんどの Socket を対象にできます。Plug が Hips（腰）に配置されている場合は、自分自身の Hips 上の Socket を除外するオプションも表示されます。

## 仕組み

Plug のメッシュは、ビルド時に基準となるまっすぐな姿勢のデータとしてあらかじめ計算（ベイク）され、Plug のマテリアルに使われているシェーダーへ、変形用の処理が自動的に組み込まれます。実行中は、このシェーダーが近くの Socket の方向へ向けて Plug の各頂点をリアルタイムに曲げます。この一連の処理は GPU 上のシェーダー計算だけで完結しており、追加のスクリプト処理を必要としません。

近くに複数の Socket がある場合は、タグ設定や距離をもとに適切なものが自動的に選ばれ、そこへ向かって曲がります。Socket が複数の中継地点（Guided Path）を持っている場合は、それらの地点を順にたどるように曲がります。
