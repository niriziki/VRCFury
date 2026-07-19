---
title: SPS Menus
sidebar:
  order: 6
---

SPS Menus は、SPS のメニュー（Socket の切り替えや設定など）をアバターのメニューのどこに入れるかを指定するための、オプションのコンポーネントです。付けなくても SPS は動作します（メニュー直下に「SPS」フォルダが追加されます）。

Modular Avatar の MA Menu Item と同じ感覚で使えます。**メニュー内の位置はこのオブジェクトを置いた場所**、**サブメニューの名前はこのオブジェクトの名前**で決まります。

## 追加方法

Hierarchy でアバター（またはアバター内の適当なオブジェクト）を右クリックし、**SPSNDMF > Create SPS Menus** を選びます。アバター直下に「SPS」オブジェクト（SPS Menus + MA Menu Installer 付き）が作られ、そのままビルドするとメニュー直下に「SPS」フォルダが入ります。SPS Menus はアバターに1つだけ持てます。

## 既存のメニューの中に入れる

たとえば MA Menu Item で作った「身体」メニューの中に SPS メニューを入れたい場合:

1. 作成された「SPS」オブジェクトを、「身体」の MA Menu Item オブジェクトの子に移動します。
2. 「SPS」オブジェクトから **MA Menu Installer コンポーネントを削除**します（付けたままだとメニューが二重に表示されることがあります。インスペクタに警告が出ます）。

これだけで、ビルド後のメニューが「身体 > SPS」になります。

## 設定項目

### Create Parent Menu

ON（既定）のときは、このオブジェクトの名前のサブメニュー（フォルダ）を作ってその中に SPS メニューを入れます。OFF にすると、フォルダを作らず SPS メニューの中身をこの位置に直接並べます（MA Menu Group のような動作）。

### Menu Icon

サブメニューのアイコンです（Create Parent Menu が ON のときに使われます）。未設定の場合は標準の SPS アイコンになります。

### Save Sockets Between Worlds

ON にすると、Socket の ON/OFF 状態がワールドを移動しても保持されるようになります（VRChat のパラメータ保存機能を使います）。

### Legacy Mode enabled on avatar load

アバターをロードした時点で、SPS メニュー内の「Legacy Compatibility」（DPS / TPS / SPS1 互換モード）を有効にしておくかどうかです。既定は ON です。

## VRCFury からの移行

VRCFury で「SPS Options」を設定していた場合は、[SPSNDMF Migrator](/migrator/overview/) で SPS Menus に変換できます。VRCFury 側でメニューの場所（例: `身体/SPS`）を指定していた場合、場所の指定は自動では引き継げないため、変換後に上記の手順でオブジェクトを配置してください。
