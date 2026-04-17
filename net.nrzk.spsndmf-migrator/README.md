# SPSNDMF Migrator

VRCFury / VRCFury スタブ / SPSNDMF / Modular Avatar / VRChat Contacts の間で SPS 関連コンポーネントを相互変換するエディタ拡張。

## 使い方

1. Unity メニューから `Tools/SPSNDMF/Migrator` を開く
2. Hierarchy で変換対象の GameObject（通常はアバタールート）を選択
3. 変換方向を選ぶ：`VRCFury → SPSNDMF` または `SPSNDMF → VRCFury`
4. `VRCFury → SPSNDMF` のときは追加変換チェックボックスを必要に応じて選択
5. `プレビュー` で変換予定を確認、`実行` で適用（Ctrl+Z で取り消し可能）

## 変換対象

| 変換元 | 変換先 | 方向 |
|---|---|---|
| VRCFury Plug/Socket | SPSNDMF Plug/Socket | 双方向 |
| VRCFury Global Collider | MA Global Collider | VRCFury → SPSNDMF のみ |
| VRCFury Haptic Touch Receiver | VRC Contact Receiver × 2（子オブジェクト） | VRCFury → SPSNDMF のみ |
| VRCFury Haptic Touch Sender | VRC Contact Sender | VRCFury → SPSNDMF のみ |

## 依存

- Modular Avatar
- VRCSDK3 Avatars

VRCFury 本体は必須ではなく、VRCFury 互換のスタブパッケージ（`VF.Component.*` 型を提供）でも動作する。
