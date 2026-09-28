// Verifies the production bundle against the ceilings in bundle-budget.json.
// Run after `npm run build`; CI runs it as a step in the browser-smoke job.
// The authoritative ceiling lives in bundle-budget.json so that the Vite
// advisory, this check, and performance-budget.md cannot disagree.

import { readFileSync, readdirSync, statSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { gzipSync } from 'node:zlib';

const root = fileURLToPath(new URL('.', import.meta.url));
const budget = JSON.parse(readFileSync(join(root, 'bundle-budget.json'), 'utf8'));
const assets = join(root, 'dist', 'assets');

let emitted;
try {
  emitted = readdirSync(assets);
} catch {
  console.error('dist/assets was not found. Run "npm run build" first.');
  process.exit(1);
}

const kbytes = (bytes) => Math.round((bytes / 1000) * 100) / 100;
const sizeOf = (name) => statSync(join(assets, name)).size;
const gzipOf = (name) => gzipSync(readFileSync(join(assets, name))).length;
const total = (names, measure) => names.reduce((sum, name) => sum + measure(name), 0);

const scripts = emitted.filter((name) => name.endsWith('.js'));
const styles = emitted.filter((name) => name.endsWith('.css'));
const other = emitted.filter((name) => !name.endsWith('.js') && !name.endsWith('.css'));

const checks = [
  { label: 'JavaScript, minified', actual: total(scripts, sizeOf), ceiling: budget.maxJsKbytes },
  { label: 'JavaScript, gzipped', actual: total(scripts, gzipOf), ceiling: budget.maxJsGzipKbytes },
  { label: 'CSS, minified', actual: total(styles, sizeOf), ceiling: budget.maxCssKbytes },
];

let overBudget = false;
for (const { label, actual, ceiling } of checks) {
  const exceeded = actual > ceiling * 1000;
  overBudget ||= exceeded;
  const status = exceeded ? 'FAIL' : ' ok ';
  console.log(`${status} ${label}: ${kbytes(actual)} kB (ceiling ${ceiling} kB)`);
}

if (other.length > 0) {
  const summary = other.map((name) => `${name} ${kbytes(sizeOf(name))} kB`).join(', ');
  console.log(` -- static assets (not gated): ${summary}`);
}

if (overBudget) {
  console.error(
    'Admin SPA bundle exceeds a ceiling in bundle-budget.json. A performance '
      + 'regression is a release-blocking defect; either reduce the bundle or raise the '
      + 'ceiling deliberately and update docs/04-engineering-standards/performance-budget.md.',
  );
  process.exit(1);
}
