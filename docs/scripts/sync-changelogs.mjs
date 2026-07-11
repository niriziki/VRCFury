import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const scriptDir = dirname(fileURLToPath(import.meta.url));
const docsRoot = resolve(scriptDir, '..');       // docs/
const repoRoot = resolve(docsRoot, '..');        // リポジトリルート
const outDir = resolve(docsRoot, 'src/content/docs/changelog');

const sources = [
  { slug: 'spsndmf', title: 'SPS for NDMF', order: 1, path: 'tools/spsndmf/CHANGELOG.md' },
  { slug: 'vfstub', title: 'VRCFury Stub', order: 2, path: 'tools/vfstub/CHANGELOG.md' },
  { slug: 'migrator', title: 'SPSNDMF Migrator', order: 3, path: 'net.nrzk.spsndmf-migrator/CHANGELOG.md' },
];

await mkdir(outDir, { recursive: true });

for (const s of sources) {
  const raw = await readFile(resolve(repoRoot, s.path), 'utf8');
  // 先頭の H1 は frontmatter の title と重複するため除去
  const body = raw.replace(/^#[^\n]*\r?\n+/, '');
  const frontmatter = `---\ntitle: ${s.title} 更新履歴\nsidebar:\n  order: ${s.order}\n---\n\n`;
  await writeFile(resolve(outDir, `${s.slug}.md`), frontmatter + body, 'utf8');
  console.log(`generated changelog/${s.slug}.md`);
}
