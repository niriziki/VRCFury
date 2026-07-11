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

## パッケージ構成

SPS for NDMF は、役割の異なる3つのパッケージで構成されています。

| パッケージ | 役割 |
|---|---|
| **SPS for NDMF**（`net.nrzk.spsndmf`） | 本体。VRCFury の SPS 機能を NDMF プラグインとして抽出したもの。 |
| **VRCFury Stub**（`net.nrzk.vfstub`） | VRCFury を入れずに、設定済みの VRCFury コンポーネントを変換用に保持するパッケージ。 |
| **SPSNDMF Migrator**（`net.nrzk.spsndmf-migrator`） | VRCFury / VRCFury Stub と SPS for NDMF の間で、SPS 関連のコンポーネントを相互に変換できるエディタ拡張。（Plug/Socket以外の関連コンポーネントはVRCFury→MAの片方向変換です。） |

:::caution
VRCFury Stub は **VRCFuryと同時にインストールすることはできません。** インストールする時はVRCFuryをアンインストールしてから！
:::

それぞれの詳細は [SPS for NDMF（本体）](/spsndmf/overview/)・[VRCFury Stub](/vfstub/overview/)・[SPSNDMF Migrator](/migrator/overview/) の各ページを参照してください。

## どれを入れるか

- **新規にアバターを組む場合**:
  - SPS for NDMF（`net.nrzk.spsndmf`）
- **VRCFury の SPS 機能を使った既存ギミックがある場合**:
  - SPS for NDMF
  - VRCFury Stub（**インストールする時はVRCFuryをアンインストールしてから！**）
  - SPSNDMF Migrator

3つを組み合わせた移行手順の詳細は [移行の流れ](/migrator/migration-flow/) を参照してください。

インストールが終わったら、[クイックスタート](/guides/quickstart/) に進んでください。
