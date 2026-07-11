import { defineRouteMiddleware } from '@astrojs/starlight/route-data';

export const onRequest = defineRouteMiddleware((context) => {
  const { entry, head } = context.locals.starlightRoute;
  if (!context.site) return;

  // トップ（splash）ページは entry.id が空文字になるため index にフォールバック
  const slug = entry.id || 'index';
  const ogImageUrl = new URL(`/og/${slug}.png`, context.site);
  head.push({ tag: 'meta', attrs: { property: 'og:image', content: ogImageUrl.href } });
  head.push({ tag: 'meta', attrs: { property: 'og:image:width', content: '1200' } });
  head.push({ tag: 'meta', attrs: { property: 'og:image:height', content: '630' } });
  head.push({ tag: 'meta', attrs: { name: 'twitter:image', content: ogImageUrl.href } });
  head.push({ tag: 'meta', attrs: { name: 'twitter:card', content: 'summary_large_image' } });
});
