---
title: 公開 API
sidebar:
  order: 1
---

VRCFury の公開 API（`com.vrcfury.api`）を使うと、エディタ拡張のコードから SPS Socket を作成・設定できます。このページは、SPS に対応したツールを作る開発者向けに、この API を [VRCFury Stub](/vfstub/overview/) の環境でも使う方法を説明します。

## 使える環境

SPS Socket を作成するコードは、次の2つの環境で共通です。

| 利用者の環境 | アバターに保存されるもの | ビルドの流れ |
|---|---|---|
| VRCFury | VRCFury の SPS Socket | VRCFury がビルド |
| VRCFury Stub、SPS for NDMF、SPSNDMF Migrator のすべて | VRCFury Stub の SPS Socket | SPSNDMF Migrator が変換し、SPS for NDMF がビルド |

これ以外の環境では API を使えません。たとえば SPS for NDMF だけの環境で使うには、利用者に VRCFury Stub と [SPSNDMF Migrator](/migrator/overview/) も入れてもらう必要があります。

## コードの例

```csharp
using com.vrcfury.api;
using com.vrcfury.api.Components;
using UnityEngine;

namespace MyTool {
    internal static class SocketCreator {
        public static void Create(GameObject obj, GameObject target) {
            var socket = FuryComponents.CreateSocket(obj);
            socket.SetName("Example");
            socket.SetMode(FurySocket.Mode.Hole);

            var depth = socket.AddDepthActions(new Vector2(-0.25f, 0), 0.1f);
            depth.AddTurnOn(target);
        }
    }
}
```

## VRCFury Stub の環境での動作

VRCFury Stub の環境では、次の順に処理されます。

1. API を呼ぶと、アバターに VRCFury Stub の SPS Socket が追加されます。この Socket は、VRCFury と同じインスペクタで編集できます。
2. ビルド時に、SPSNDMF Migrator が VRCFury Stub の SPS Socket を SPS for NDMF の SPS Socket に変換します。変換するのはビルド用の複製だけなので、元のアバターには VRCFury Stub の SPS Socket が残ります。
3. SPS for NDMF が、変換後の SPS Socket をビルドします。

VRCFury Stub 自体はビルドを行いません。SPS for NDMF と SPSNDMF Migrator のどちらかが欠けると、書き込んだ設定はアバターに反映されません。そのため、VRCFury Stub に同梱された `com.vrcfury.api` のアセンブリは、この2つがそろっているときだけコンパイルされます。VRCFury Stub が入っていても、API が使えるとは限りません。

## 使える API

VRCFury の公開 API のうち、VRCFury Stub に含まれるのは次の API だけです。Plug を作成する API は、VRCFury の公開 API にもありません。

| 型 | メソッド | 内容 |
|---|---|---|
| `FuryComponents` | `CreateSocket(obj)` | SPS Socket を追加し、`FurySocket` を返します。 |
| `FurySocket` | `SetName(name)` | Name in menu を設定します。 |
| | `SetMode(mode)` | Mode を設定します。値は `Auto`・`Hole`・`Ring`・`RingOneWay`・`None` で、`None` は Enable Deformation をオフにします。 |
| | `SetAutoOff()` | Include in Auto selection? をオフにします。 |
| | `UseRadiusOffset()` | Radius Offset をオンにします。 |
| | `AddDepthActions(range, smoothingSeconds, enableSelf)` | Depth Animations に1件追加し、その Action Set を `FuryActionSet` で返します。`range` は Activation distance（単位はメートル）、`smoothingSeconds` は Smoothing Seconds、`enableSelf` は Allow avatar to trigger its own animation? で、`enableSelf` を省略するとオフです。 |
| | `GetActiveActions()` | Active Animation の Action Set を `FuryActionSet` で返します。 |
| `FuryActionSet` | `AddTurnOn(obj)` | オブジェクトをオンにするアクションを追加します。 |
| | `AddAnimationClip(clip)` | アニメーションクリップを追加します。 |
| | `AddBlendshape(name, value, renderer)` | ブレンドシェイプのアクションを追加します。`renderer` を省略すると、すべてのレンダラーが対象です。 |
| | `AddAap(name, value)` | Animator のパラメータを指定の値にするアクションを追加します。そのパラメータは、VRChat のメニューや OSC からは操作も参照もできなくなります。 |
| | `AddFlipbookBuilder()` | Flipbook のアクションを追加し、`FuryFlipbookBuilder` を返します。 |
| `FuryFlipbookBuilder` | `AddPage()` | ページを追加します。返り値の `GetActions()` で、そのページの `FuryActionSet` を取得できます。 |

各設定の意味は [Socket](/spsndmf/socket/) を参照してください。

VRCFury の公開 API にある `CreateToggle`・`CreateArmatureLink`・`CreateFullController` と `FuryUtils` クラスは、VRCFury Stub には含まれません。これらを呼ぶコードは、VRCFury の環境でだけコンパイルされるように、`#if` や別のアセンブリで分けてください。

## 環境の切り替え

`com.vrcfury.api` のアセンブリが無い環境もあります。API を呼ぶコードは、次のパッケージの有無とバージョンで判定し、`#if` や別のアセンブリで切り替えてください。

| 環境 | 判定するパッケージ |
|---|---|
| VRCFury | `com.vrcfury.vrcfury` |
| VRCFury Stub | `net.nrzk.vfstub`・`net.nrzk.spsndmf`・`net.nrzk.spsndmf-migrator` のすべて |

VRCFury Stub の環境で必要なバージョンは、[VRCFury Stub の更新履歴](/changelog/vfstub/) の、公開 API を同梱したバージョンの項目を参照してください。

`com.vrcfury.api` は自動では参照されないので、ツールのアセンブリ定義の `references` に書きます。API が無い環境でも `references` の記述はエラーになりません。エラーになるのは API を呼ぶコードなので、そのコードを上の条件で切り替えます。

:::caution
VRCFury Stub のパッケージ名は `net.nrzk.vfstub` です。`com.vrcfury.vrcfury` だけで判定すると、API を使える VRCFury Stub の環境でも、API を呼ぶコードが無効になります。
:::

## 呼び出すタイミング

エディタ上の操作で呼ぶ場合、タイミングの指定は必要ありません。

ビルド中に NDMF プラグインから呼ぶ場合は、SPSNDMF Migrator と SPS for NDMF より前に実行してください。後から追加した SPS Socket は変換されず、ビルドに反映されません。Transforming フェーズで実行するときは、次のように順序を指定します。Transforming より前のフェーズなら、この指定は必要ありません。

```csharp
InPhase(BuildPhase.Transforming)
    .BeforePlugin("net.nrzk.spsndmf-migrator")
    .BeforePlugin("net.nrzk.spsndmf")
    .Run("Create SPS Socket", ctx => {
        // ここで API を呼ぶ
    });
```

指定したプラグインが入っていない環境では、その指定は無視されます。

## VPM の依存関係

VRCFury と VRCFury Stub は同時にインストールできません。ツールの `vpmDependencies` に `com.vrcfury.vrcfury` を書くと、VRCFury Stub の環境にも VRCFury を入れようとしてしまいます。依存関係には書かず、「環境の切り替え」のパッケージで判定してください。
