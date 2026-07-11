---
title: SPS1 と SPS2
sidebar:
  order: 3
---

SPS には **SPS1** と **SPS2** という2つの世代があります。本プロジェクトは **0.1.x 系（SPS1）** と **0.2.x 系（SPS2）** の2系統を並行してリリースしており、インストールしたバージョン系統によってどちらが動くかが決まります。

| 系統 | 本パッケージのバージョン | 相当する VRCFury のバージョン | 方式 |
|---|---|---|---|
| SPS1 | 0.1.x | 〜 1.1348.0 | Unity の点光源を使ってソケットの位置を伝える方式 |
| SPS2 | 0.2.x（本ドキュメントが主に扱う版） | 1.1349.0 以降 | 共有テクスチャを介してシェーダー間でソケットの位置を伝える方式 |

## 違い

SPS の実質的前身にあたる DPS も含めて比較すると、次のようになります。DPS と SPS1 はどちらも Unity の点光源で位置を伝える方式で、SPS2 だけが Unity のライトを使わず、Socket 側と Plug 側のシェーダーが共有テクスチャを介して位置を伝える新しい方式です。

| 観点 | DPS | SPS1（0.1.x） | SPS2（0.2.x） |
|---|---|---|---|
| Socket 位置の伝え方 | Unity の Point Light | Unity の Point Light | 共有テクスチャを介したシェーダー間の受け渡し（Unity のライトを使わない） |
| 同時に有効化できる Socket の数 | 実質1個（既定で自動排他。Dual Mode で2個までだが煩雑） | 実質1個（既定で自動排他。Dual Mode で2個まで、強い警告付き） | 実質無制限 |
| タグによる絞り込み | 無し | 無し | あり（Include / Exclude Tags） |
| 挿入経路 | 単純な変形 | Socket 1つへの直線的な変形のみ | Guided Path（中継地点）による複数段の経路指定に対応 |
| 挿入時のSocket側変形 | あるが設定が複雑 (Point Light 式) | ある (Contact 式・Depth Animations) | ある (Contact 式・Depth Animations。SPS1 と同じ) |
| Radius Offset | 無し | 無し | あり（Socket の太さ分だけ Plug の向かう先をオフセット） |
| 提供元 | Raliv | VRCFury（SPS） | VRCFury（SPS） |

それぞれの詳しい動作の仕組みは [SPS1 の詳細](/details/sps1/)・[SPS2 の詳細](/details/sps2/) を参照してください。設定項目ごとの版差は [Plug](/spsndmf/plug/)・[Socket](/spsndmf/socket/) の各ページにも記載しています。

## SPS1 / DPS 特有の制限

DPS と SPS1 が使う Unity の点光源には、**1つのメッシュが同時に認識できる光源は最大4つまで**という仕様上の制約があります。Socket 1つがこのうち複数を使うため、次のような制限が生じます。

- 近くに Socket が複数あったり、現実の照明などの光源が多いと、Plug 側から認識できない Socket が出てきます。
- このため SPS1 では、メニュートグルを持つ Socket が2個以上あると既定で**排他制御**が働き、同時に有効化できるのは実質1個です（**Dual Mode** で最大2個まで。強い警告付き）。
- 正しく動作させるには、VRChat 側のライト設定（**Pixel Light Count** を High にする等）が必要です。

**SPS2** はこの点光源方式を使わないため、これらの制限はありません（同時数は実質無制限で、ライト設定にも依存しません）。

## SPS1 特有の制限: Contacts の多さによる不具合

SPS1 は、**近くの Socket を探す検出処理**などに **VRChat の Contacts** を多く使います。アバター上の Contacts が多くなると、VRChat 側の不具合により Contacts が正しく機能せず、SPS がしばしば動作しないことがあります。

SPS2 では、この Socket の検出を Contacts ではなく[共有テクスチャを介したシェーダー間のやり取り](/details/sps2/)で行うようになったため、Contacts の使用数が大幅に減りました。SPS2 で残る Contacts は、Depth Animations（挿入の深さでアニメーションを駆動）・レガシー互換・触覚通知（OGB）といった用途に限られ、この問題が起きにくくなっています。

## どちらを使うか

新規に導入する場合は、より高機能な **SPS2（0.2.x）** を推奨します。**SPS1（0.1.x）** も引き続きリリースされており、SPS1 環境を前提とした既存のアバター・アセットとの互換性が必要な場合はそちらを利用できます。

## レガシー互換

SPS2 には **Legacy Compatibility**（既定で ON）という位置づけの仕組みがあり、SPS1 が使うのと同じ点光源を追加で出力できます。これにより、SPS2 の Socket は SPS1 や DPS・TPS といった旧方式のプラグ側からも、ある程度互換的に認識されるようになっています（DPS は Raliv 社の Penetrator シェーダー機能、TPS は Poiyomi Toon Shader の TPS_Penetrator 機能を指します）。
