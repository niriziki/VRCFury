---
title: SPS Options
sidebar:
  order: 6
---

SPS Options は、アバター全体の SPS の動作をまとめて調整するためのオプションのコンポーネントです。付けなくても SPS は動作します（すべて既定値になります）。

## 追加方法

Hierarchy でアバター（またはアバター内の適当なオブジェクト）を右クリックし、**SPSNDMF > Create SPS Options** を選びます。アバターのルートに SPS Options が追加されます。SPS Options はアバターに1つだけ持てます（すでにある場合は既存のものが選択されます）。

## 設定項目

### SPS Menu Icon Override

アクションメニューに追加される「SPS」メニューのアイコンを、好きな画像に差し替えます。未設定の場合は標準の SPS アイコンになります。

### SPS Menu Path Override (Default: SPS)

「SPS」メニューを追加する場所を変更します。**Select** ボタンからアバターの既存メニューを選んで指定できます。`装備/SPS` のようにスラッシュ区切りで指定すると、サブメニューの中に SPS メニューを入れられます。空欄の場合はメニュー直下の「SPS」になります。

### Save Sockets Between Worlds

ON にすると、Socket の ON/OFF 状態がワールドを移動しても保持されるようになります（VRChat のパラメータ保存機能を使います）。

### Legacy Mode enabled on avatar load

アバターをロードした時点で、SPS メニュー内の「Legacy Compatibility」（DPS / TPS / SPS1 互換モード）を有効にしておくかどうかです。既定は ON です。

## VRCFury からの移行

VRCFury で設定済みの SPS Options は、[SPSNDMF Migrator](/migrator/overview/) で SPSNDMF 用に変換できます（双方向）。VRCFury Stub 環境では、ビルド時の自動変換にも含まれます。
