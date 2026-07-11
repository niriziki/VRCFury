---
title: インストール
sidebar:
  order: 2
---

SPS for NDMF のインストール方法を説明します。

## 前提

SPS for NDMF は **NDMF 環境専用**のパッケージです。Modular Avatarが依存関係として必要です。

## VPM で入れる

[SPS for NDMF の VPM リポジトリページ](https://spsndmf.vpm.nrzk.net/) を開き、「**Add to VCC**」ボタンからお使いの VPM 対応マネージャー（VCC / ALCOM など）にリポジトリを追加します。

リポジトリを登録すると、対象の Unity プロジェクトに SPS for NDMF パッケージを追加できるようになります。

## インストーラで入れる

VPM リポジトリを登録する代わりに、[Releases](https://github.com/niriziki/VRCFury/releases) から `net.nrzk.vfstub-installer.zip
` をダウンロード＆解凍して unitypackage 形式のインストーラーをインポートする方法もあります。Unity プロジェクトを開いた状態でこの unitypackage をインポートすると、VPM リポジトリの登録とパッケージの導入がまとめて行われます。

## どのパッケージを入れるか

新規にアバターを組む場合は、通常は本体の SPS for NDMF（`net.nrzk.spsndmf`）だけを入れれば十分です。

すでに VRCFury（またはその SPS 機能）を使ったシーン・プレハブがある場合は、VRCFury Stub（`net.nrzk.vfstub`）や SPSNDMF Migrator（`net.nrzk.spsndmf-migrator`）が必要になることがあります。これらの役割と使い分けについては [パッケージ構成](/guides/packages/) を参照してください。

インストールが終わったら、[クイックスタート](/guides/quickstart/) に進んでください。
