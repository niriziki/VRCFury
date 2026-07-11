---
title: SPS1 と SPS2
sidebar:
  order: 3
---

SPS には **SPS1** と **SPS2** という2つの世代があります。本プロジェクトは `release/0.1.x` 系（SPS1 相当）と `ndmf`/0.2.x 系（SPS2 相当）の2系統を並行してリリースしており、インストールしたバージョン系統によってどちらが動くかが決まります。

| 系統 | バージョン | 方式 |
|---|---|---|
| SPS1 | 0.1.x（`release/0.1.x`） | Unity の点光源を使ってソケットの位置を伝える方式 |
| SPS2 | 0.2.x（`ndmf`、本ドキュメントが主に扱う版） | シェーダー内でソケットの位置を伝える方式 |

## 違い

| 観点 | SPS1（0.1.x） | SPS2（0.2.x） |
|---|---|---|
| Socket 位置の伝え方 | Unity の点光源（頂点ライト） | シェーダー内で完結（GPU 上でのやり取り） |
| 同時に有効化できる Socket の数 | 実質1個（既定で自動排他。Dual Mode で2個まで、強い警告付き） | 実質無制限 |
| タグによる絞り込み | 無し | あり（Include / Exclude Tags） |
| 挿入経路 | Socket 1つへの直線的な変形のみ | Guided Path（中継地点）による複数段の経路指定に対応 |
| Radius Offset | 無し | あり（Socket の太さ分だけ Plug の向かう先をオフセット） |

それぞれの詳しい変形のしくみは [SPS1 のしくみ](/spsndmf/how-it-works-sps1/)・[SPS2 のしくみ](/spsndmf/how-it-works-sps2/) を参照してください。設定項目ごとの版差は [Plug](/spsndmf/plug/)・[Socket](/spsndmf/socket/) の各ページにも記載しています。

## どちらを使うか

新規に導入する場合は、より高機能な **SPS2（0.2.x）** を推奨します。**SPS1（0.1.x）** も引き続きリリースされており、SPS1 環境を前提とした既存のアバター・アセットとの互換性が必要な場合はそちらを利用できます。

## レガシー互換

SPS2 には **Legacy Compatibility**（既定で ON）という位置づけの仕組みがあり、SPS1 が使うのと同じ点光源を追加で出力できます。これにより、SPS2 の Socket は SPS1 や DPS・TPS といった旧方式のプラグ側からも、ある程度互換的に認識されるようになっています（DPS は Raliv 社の Penetrator シェーダー機能、TPS は Poiyomi Toon Shader の TPS_Penetrator 機能を指します）。
