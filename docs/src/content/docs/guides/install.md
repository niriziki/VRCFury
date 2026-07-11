---
title: インストール
sidebar:
  order: 2
---

SPS for NDMF のインストール方法を説明します。

## 前提

SPS for NDMF は **NDMF 環境専用**のパッケージです。次のパッケージが依存関係として必要です。

- `com.vrchat.avatars` `>=3.7.6`
- `nadena.dev.modular-avatar` `^1.15.0`

VPM 経由でインストールする場合、これらの依存関係は VPM 対応マネージャー（VCC / ALCOM など）が解決します。手動でインポートする場合は、あらかじめ VRChat Avatars SDK と Modular Avatar を導入したプロジェクトを用意してください。

## VPM で入れる

VCC (VRChat Creator Companion) や ALCOM などの VPM 対応マネージャーの「Add Repository」（リポジトリを追加）から、以下の URL を登録します。

```
https://spsndmf.vpm.nrzk.net/index.json
```

リポジトリを登録すると、対象の Unity プロジェクトに `SPS for NDMF` パッケージを追加できるようになります。

## VPAI インストーラで入れる

VPM リポジトリを手動で登録したくない場合は、[Releases](https://github.com/niriziki/VRCFury/releases) から VPAI (unitypackage 形式のインストーラ) をダウンロードして使うこともできます。Unity プロジェクトを開いた状態で unitypackage を開くと、VPM リポジトリの登録とパッケージの導入が行えます。

## どのパッケージを入れるか

新規にアバターを組む場合は、通常は本体の `net.nrzk.spsndmf` だけを入れれば十分です。

すでに VRCFury（またはその SPS 機能）を使ったシーン・プレハブがある場合は、`net.nrzk.vfstub` や `net.nrzk.spsndmf-migrator` が必要になることがあります。これらの役割と使い分けについては [パッケージと連携](/packages/overview/) を参照してください。

インストールが終わったら、[クイックスタート](/guides/quickstart/) に進んでください。
