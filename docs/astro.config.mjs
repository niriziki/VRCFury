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
        { label: 'ガイド', items: [{ autogenerate: { directory: 'guides' } }] },
        { label: 'パッケージと連携', items: [{ autogenerate: { directory: 'packages' } }] },
        { label: 'SPS を理解する', items: [{ autogenerate: { directory: 'sps' } }] },
        { label: 'リファレンス', items: [{ autogenerate: { directory: 'reference' } }] },
        { label: '更新履歴', items: [{ autogenerate: { directory: 'changelog' } }] },
      ],
    }),
  ],
});
