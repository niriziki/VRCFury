import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

export default defineConfig({
  site: 'https://spsndmf.nrzk.net',
  integrations: [
    starlight({
      title: 'SPS for NDMF',
      defaultLocale: 'root',
      locales: {
        root: { label: '日本語', lang: 'ja' },
      },
      social: [
        { icon: 'github', label: 'GitHub', href: 'https://github.com/niriziki/VRCFury' },
      ],
      editLink: {
        baseUrl: 'https://github.com/niriziki/VRCFury/edit/ndmf/docs/',
      },
      customCss: ['./src/styles/custom.css'],
      sidebar: [
        { label: 'はじめに', items: [{ autogenerate: { directory: 'guides' } }] },
        { label: 'SPSNDMF の使い方', items: [{ autogenerate: { directory: 'spsndmf' } }] },
        { label: 'VRCFury Stub の使い方', items: [{ autogenerate: { directory: 'vfstub' } }] },
        { label: 'SPSNDMF Migrator の使い方', items: [{ autogenerate: { directory: 'migrator' } }] },
        { label: '詳細', items: [{ autogenerate: { directory: 'details' } }] },
        { label: '更新履歴', items: [{ autogenerate: { directory: 'changelog' } }] },
      ],
    }),
  ],
});
