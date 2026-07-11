import { getCollection } from 'astro:content';
import { OGImageRoute } from 'astro-og-canvas';

const entries = await getCollection('docs');

export const { getStaticPaths, GET } = await OGImageRoute({
  pages: Object.fromEntries(entries.map((entry) => [entry.id, entry])),
  getImageOptions: (_id, entry) => ({
    title: entry.data.title,
    description: 'SPS for NDMF ドキュメント',
    bgGradient: [
      [30, 27, 46],
      [15, 14, 25],
    ],
    border: { color: [124, 58, 237], width: 24, side: 'inline-start' },
    padding: 70,
    fonts: [
      'https://cdn.jsdelivr.net/fontsource/fonts/noto-sans-jp@latest/japanese-400-normal.ttf',
    ],
    font: {
      title: { color: [255, 255, 255], size: 66 },
      description: { color: [167, 139, 250], size: 40 },
    },
  }),
});
