---
title: しくみ
sidebar:
  order: 6
---

[Plug](/sps/plug/) と [Socket](/sps/socket/) の各ページの「仕組み」節で紹介した内容を、SPS 全体の視点からまとめて説明します。実用的な設定項目だけを知りたい場合は、このページを読む必要はありません。

## SPS シェーダーの原理

SPS の変形処理は、Plug 側のメッシュに組み込まれた専用のシェーダーによって行われます。ビルド時に、Plug に設定されたメッシュの「基準となるまっすぐな姿勢」がデータとして焼き込まれ（ベイク）、通常使っているシェーダー（Standard やトゥーンシェーダーなど）に対して、この変形処理が自動的に追加されます。

実行中、このシェーダーは同じフレームの中で Socket 側が書き込んだ位置・向き・種類などの情報を、画面の描画データを介して読み取ります。これによって、CPU 側の処理やアニメーションクリップを介さずに、GPU 上の頂点計算だけで Plug をリアルタイムに曲げることができます。近くに複数の Socket がある場合は、その中から距離やタグ設定に基づいて適切なものが自動的に選ばれます。

Plug の長さ・太さは、Automatically find mesh 等の設定に応じて自動計測されるほか（[Plug のページ](/sps/plug/)を参照）、実行中の曲がり方の太さの変化にもこの計測結果が使われます。テクスチャマスクやボーンウェイトによるマスク設定は、変形させる範囲・長さや太さの計算に使う範囲を絞り込むために使われます。

なお、SPS は旧式の DPS / TPS との互換性のため、Unity のライト（点光源）を使った位置伝達も補助的にサポートしています（[SPS とは](/sps/what-is-sps/)の「DPS / TPS との違い」も参照）。この互換用の仕組みは、Socket の「Enable Legacy Compatibility」設定で個別に ON / OFF できます。

## VRChat Contacts との関係

SPS で Socket の検出そのものに使われているのは、前述のシェーダーによる画面データのやり取りです。VRChat の Contacts（Sender / Receiver）は、この検出処理そのものには使われておらず、主に次のような補助的な役割のために、Plug / Socket のビルド時に自動生成されます。

- **旧式互換** — 旧式の DPS / TPS Plug からも見えるようにするための、Socket 側の Sender。
- **Depth Animations** — Socket と Plug の距離や挿入の度合いをアニメーションに反映するための、距離計測用の Receiver。
- **触覚通知（OGB Haptics）** — 触覚対応アプリへ、接触や距離の状態を伝えるための Receiver。既定では無効化されており、対応アプリを起動している場合にのみ有効になります。
- **メニューの Auto 選択** — Socket の Enable Menu Toggle の Auto 選択で、Plug に一番近い Socket だけを自動的に選ぶための距離計測。

このように、SPS は Plug / Socket の検出処理自体をシェーダー側に持たせることで、Contacts の使用数を必要最小限に抑える設計になっています。DPS / TPS など Contacts と光源だけで実現していた旧方式に比べて、同時に扱える Plug / Socket の組み合わせ数に無理のない設計になっているのはこのためです。

## NDMF ビルドでの処理の流れ

SPS for NDMF は、NDMF の非破壊ビルドの中で Modular Avatar より前に実行される、次の2段階の処理として組み込まれています。

1. **SPS のビルド処理を実行する** — アバター上の Plug / Socket を検出し、これまでに説明した変形用シェーダーの組み込みや、Contacts の生成、メニュー・パラメータの構築などを行います。
2. **結果を Modular Avatar のコンポーネントとして出力する** — 1 で生成されたアニメーター・メニュー・パラメータを、Modular Avatar の Merge Animator・Menu Installer・Parameters といったコンポーネントとして出力します。

この2段階の処理は、アバター上に Plug または Socket が1つも無い場合は実行されません。また、SPS for NDMF は SPS に関係する範囲だけを処理するように設計されており、Plug / Socket 以外の VRCFury コンポーネント（[Global Collider](/sps/global-collider/) や [Haptic Touch](/sps/haptic-touch/) など）には影響を与えません。この2段階の処理が完了したあと、続けて Modular Avatar 自体のビルドが実行され、1つのアバターとして統合されます。
