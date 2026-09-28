import { expect, test } from '@playwright/test';

const blogUrl = process.env.PAPYRUS_E2E_BLOG_URL ?? 'http://127.0.0.1:5075';
const portfolioUrl = process.env.PAPYRUS_E2E_PORTFOLIO_URL ?? 'http://127.0.0.1:5024';

test('public blog feed renders', async ({ page }) => {
  await page.goto(blogUrl);
  await expect(page).toHaveTitle(/Blog/i);
  await expect(page.getByRole('heading', { name: 'Blog', exact: true })).toBeVisible();
  await expect(page.getByRole('form', { name: 'Filter posts' })).toBeVisible();
});

test('public portfolio page renders', async ({ page }) => {
  await page.goto(portfolioUrl);
  await expect(page).toHaveTitle(/Portfolio/i);
  await expect(page.getByRole('heading', { name: 'Portfolio', exact: true })).toBeVisible();
  await expect(page.getByRole('form', { name: 'Filter projects' })).toBeVisible();
});

test('public post detail renders the seeded post', async ({ page }) => {
  await page.goto(`${blogUrl}/post/building-papyrus-platform-foundation`);

  await expect(page.getByRole('heading', { name: 'Building the Papyrus platform foundation' })).toBeVisible();
  // The detail page emits the post body with Html.Raw after sanitising, so a
  // regression in that sanitiser would surface as a visible script node.
  await expect(page.locator('.article-content__prose')).toContainText('Papyrus is being shaped as a personal site');
});

test('public project detail renders the seeded project', async ({ page }) => {
  await page.goto(`${portfolioUrl}/project/platform-foundation`);

  await expect(page.getByRole('heading', { name: 'Papyrus Platform Foundation' })).toBeVisible();
  await expect(page.locator('.project-content__prose')).toContainText('Aspire-based foundation');
});

test('an unknown public slug returns 404', async ({ request }) => {
  const response = await request.get(`${blogUrl}/post/no-such-post`, { failOnStatusCode: false });

  expect(response.status()).toBe(404);
});

test('the blog serves a valid RSS feed', async ({ request }) => {
  const response = await request.get(`${blogUrl}/feed.xml`);

  expect(response.ok()).toBeTruthy();
  // RSS 2.0 has its own registered media type; feed readers expect it rather
  // than the generic application/xml the sitemap uses.
  expect(response.headers()['content-type']).toContain('application/rss+xml');
  const body = await response.text();
  expect(body.startsWith('<?xml')).toBeTruthy();
  expect(body).toContain('<rss');
  expect(body).toContain('building-papyrus-platform-foundation');
});

test('the blog and portfolio serve valid sitemaps', async ({ request }) => {
  for (const base of [blogUrl, portfolioUrl]) {
    const response = await request.get(`${base}/sitemap.xml`);

    expect(response.ok()).toBeTruthy();
    const body = await response.text();
    expect(body).toContain('urlset');
    expect(body).toContain('http://www.sitemaps.org/schemas/sitemap/0.9');
  }
});
