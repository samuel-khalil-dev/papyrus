import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath, URL } from 'node:url';

const apiTarget = process.env['services__admin-api__http__0']
  ?? process.env.VITE_API_BASE_URL
  ?? 'http://localhost:5205';

export default defineConfig({
  base: '/',
  plugins: [react({ babel: { plugins: [['babel-plugin-react-compiler', { target: '19' }] ] } }), tailwindcss()],
  resolve: { alias: { '@': fileURLToPath(new URL('./src', import.meta.url)) } },
  server: {
    port: 5173,
    proxy: {
      '/api/v1': { target: apiTarget, changeOrigin: true },
    },
  },
});
