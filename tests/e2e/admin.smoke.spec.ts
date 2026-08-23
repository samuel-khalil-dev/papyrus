import { expect, test } from '@playwright/test';

const apiKey = process.env.PAPYRUS_E2E_ADMIN_KEY ?? 'dev-admin-key';

test('versioned admin API rejects missing credentials', async ({ request }) => {
  const response = await request.get('/api/v1/blog-posts');
  expect(response.status()).toBe(401);
});

test('admin SPA renders and completes the post write path', async ({ page }) => {
  await page.goto('/admin/');
  await page.evaluate((key: string) => localStorage.setItem('papyrus.admin.api-key', key), apiKey);
  await page.reload();
  await page.goto('/admin/#posts');
  await expect(page.getByText('papyrus.', { exact: false })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Blog posts', exact: true })).toBeVisible();

  await page.getByRole('button', { name: 'New post', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Create post', exact: true })).toBeVisible();
  await page.locator('input[name="title"]').fill(`Browser smoke ${Date.now()}`);
  await page.locator('input[name="slug"]').fill(`browser-smoke-${Date.now()}`);
  await page.locator('input[name="summary"]').fill('A browser smoke test post summary.');
  await page.locator('textarea[name="contentMarkdown"]').fill('# Browser smoke\n\nCreated by Playwright.');
  await page.getByRole('button', { name: 'Save post', exact: true }).click();
  await expect(page.getByText('Post saved.', { exact: true })).toBeVisible();
});

test('authorized API access is available to the browser session', async ({ page }) => {
  await page.goto('/admin/');
  await page.evaluate((key: string) => localStorage.setItem('papyrus.admin.api-key', key), apiKey);
  const response = await page.request.get('/api/v1/blog-posts', {
    headers: { 'X-Admin-Api-Key': apiKey, 'X-Tenant-Id': 'main' }
  });
  expect(response.ok()).toBeTruthy();
});
