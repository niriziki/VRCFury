# SPS for NDMF ドキュメントサイト

[Astro](https://astro.build/) + [Starlight](https://starlight.astro.build/) 製。`https://spsndmf.nrzk.net` で公開（Cloudflare Pages）。

## ローカル開発

作業ディレクトリは `docs/`。

```
npm install
npm run dev      # http://localhost:4321
npm run build    # astro check && astro build（prebuild で CHANGELOG を取り込む）
npm run preview
```

## 更新履歴の生成

`scripts/sync-changelogs.mjs` が各パッケージの `CHANGELOG.md`（正本）を
`src/content/docs/changelog/` に生成する（`predev` / `prebuild` で自動実行、生成物は git 管理外）。
`../tools/**` などリポジトリ上位のファイルを読むため、**リポジトリ全体をチェックアウトした状態**で実行する必要がある。

## デプロイ（Cloudflare Pages / Git 連携）

Cloudflare Pages のダッシュボードでこのリポジトリを接続し、次を設定する。

| 項目 | 値 |
|---|---|
| Production branch | `ndmf` |
| Root directory | `docs` |
| Build command | `npm run build` |
| Build output directory | `dist` |
| Node version | `24`（`docs/.nvmrc` で指定済み） |

`ndmf` への push で本番デプロイ、それ以外のブランチはプレビューデプロイになる。ビルドはリポジトリ全体がチェックアウトされた状態で `docs/` を root として実行されるため、`prebuild`（CHANGELOG 取り込み）が `../tools/**` を読める。

### カスタムドメイン

Cloudflare Pages プロジェクトの **Custom domains** に `spsndmf.nrzk.net` を追加する。`nrzk.net` の DNS を Cloudflare で管理している場合は必要な CNAME レコードが自動で追加される。

（GitHub Pages は使わない。VPM リスティング `spsndmf.vpm.nrzk.net` がこのリポジトリの GitHub Pages を使用しているため、docs は Cloudflare Pages に分離している。）
