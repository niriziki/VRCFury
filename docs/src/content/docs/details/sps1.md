---
title: SPS1 の詳細
sidebar:
  order: 2
---

これは **SPS1**（0.1.x 系）のしくみです。SPS2（0.2.x 系、SPS for NDMF が実装している方式）との違いは [SPS1 と SPS2](/spsndmf/versions/) を参照してください。実用的な設定項目だけを知りたい場合は、このページを読む必要はありません。

## Socket の位置を伝える方法: 点光源

SPS1 は、Socket の位置・向き・種類（Hole / Ring など）を、Unity の**点光源**（人の目には見えない、色を持たない光源）を使って Plug 側のシェーダーへ伝えます。Socket が有効になると、その場所に小さな点光源が生成され、Plug のシェーダーはその光を読み取ることで「近くにどの種類の Socket がどの向きにあるか」を知り、メッシュをその方向へ曲げます。

## 同時に有効化できる Socket の数に上限がある

Unity の点光源には、**1つのメッシュが同時に認識できる光源は最大4つまで**という仕様上の制約があります。SPS1 の Socket 1つはこのうち2つを使うため、近くに Socket が複数（あるいは現実の照明などの光源が多数）あると、Plug 側のシェーダーから認識できなくなる Socket が出てきます。

この制約に対応するため、SPS1 では [Socket](/spsndmf/socket/) にメニュートグルを持つものが2個以上ある場合、既定で**排他制御**が自動的に働きます。つまり、どれか1つの Socket をメニューで ON にすると、他の Socket は自動的に OFF になります。

- この排他制御を外したい場合は **Dual Mode** という設定があり、有効にすると同時に2個まで ON にできます。ただし、他の Socket との組み合わせによる誤動作のリスクが高くなるため、インスペクター上に強い警告文が表示されます。
- 自分にしか見えない **Stealth Mode** が ON の間は、この排他制御は働きません。

## 動作に必要な VRChat 側の設定

SPS1 を正しく動作させるには、アバター側の VRChat 設定が影響します。Plug / Socket のインスペクターは、次のような設定が適切でない場合に警告・エラーを表示します。

- **Avatar Self Interact** が ON になっていない場合はエラー（自分自身との組み合わせが動作しません）
- **Avatar Allowed to Interact** が Everyone になっていない場合は警告（他のプレイヤーとの組み合わせに影響する可能性があります）
- **Pixel Light Count** が High になっていない場合は警告（点光源の認識に影響します。上記の「同時に有効化できる Socket の数」とも関係します）

## 使用する Contacts

SPS1 は位置の伝達を点光源で行いますが、触覚通知や補助的な検出には VRChat Contacts も使います（数は Plug 1個・Socket 1個あたり。Sender は発信、Receiver は受信）。

| 用途 | 種別 | Plug | Socket | 役割 | 有効になる条件 |
|---|---|---|---|---|---|
| 位置ビーコン | Sender | 4 | 2 | 「ここに Plug／Socket がある」と発信する。触覚通知の Receiver や旧方式（DPS/TPS）の相手が読む | Plug は常時／Socket はメニュー ON 時 |
| 触覚通知（OGB） | Receiver | 8 | 4〜11 | 位置ビーコンを検知し、触覚アプリに接触・挿入を伝える | 常時（SPS2 のようなゲートが無い） |
| 近くの Socket を探す（SPS Plus） | Receiver | 4 | — | 近くに Socket があるか検知（SPS1 のみ。SPS2 で廃止） | 常時（半径3mと大きい） |
| スケール補正 | Sender / Receiver | 2 | — | アバターのスケールを測り挿入の計算を補正する | ほぼ常時 |
| Depth Animations | Receiver | 1〜2 | 3〜6 | 挿入の深さを測り、アニメを動かす | Depth Animations を設定したときだけ |
| Auto 選択 | Receiver | — | 1（Auto 対象ごと） | 複数 Socket から最寄りを自動選択 | Auto 対象 Socket が2個以上 |

**Sender と Receiver の関係**は SPS2 と同じで、位置を発信する Sender を、反対側のコンポーネント（Plug の Sender ↔ Socket の Receiver、およびその逆。自分・相手どちらのアバターにもある）の Receiver が受信します。詳しくは [SPS2 の詳細 — 使用する Contacts](/details/sps2/) を参照してください。

### 実際に有効になっている Contacts の数

SPS1 には SPS2 のような触覚 Receiver のゲート（OSC 触覚アプリ起動時のみ有効化）が無いため、**Plug 側の Contacts（18個）は常時有効**です。この数の多さと、SPS Plus の半径3mという大きさが、他アバターとの干渉のしやすさにつながります。

通常のプレイ（OSC アプリなし）で実際に有効な Contacts の数は、**Plug の数**と、**メニューで ON にしている Socket の数**で決まります（OFF の Socket は 0）。

| 世代 | 実際に有効な数（おおよそ） |
|---|---|
| SPS1 | 18 ×（Plug 数）＋ 6〜13 ×（ON の Socket 数） |
| SPS2 | 4 ×（Plug 数）＋ 2 ×（ON の Socket 数） |

たとえば Plug 1・ON の Socket 1 なら、SPS1 は約 24〜31 個、SPS2 は約 6 個です。SPS1 の「SPS Plus」とスケール補正（Plug 側の計6個）は、SPS2 では検出を共有テクスチャ方式に移したため不要になっています。

## SPS2 との違い

SPS2 は Socket の位置伝達に点光源を使わないため、このページで説明したような「同時に有効化できる Socket の数の実質的な上限」や「VRChat のライト関連設定への依存」がありません。詳しくは [SPS2 の詳細](/details/sps2/) を参照してください。
