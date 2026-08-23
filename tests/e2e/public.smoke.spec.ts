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
