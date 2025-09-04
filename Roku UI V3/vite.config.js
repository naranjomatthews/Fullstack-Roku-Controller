import { defineConfig } from 'vite'
import { viteStaticCopy } from 'vite-plugin-static-copy'
import path from 'path'

export default defineConfig({
  plugins: [
    // Remove viteSingleFile() for multi-page
    viteStaticCopy({
      targets: [
        {
          src: 'node_modules/@crestron/ch5-crcomlib/build_bundles/umd/cr-com-lib.js',
          dest: ''
        },
        {
          src: 'node_modules/@crestron/ch5-webxpanel/dist/umd/index.js',
          dest: ''
        },
        {
          src: 'node_modules/@crestron/ch5-webxpanel/dist/umd/d4412f0cafef4f213591.worker.js',
          dest: ''
        },
        {
          src: 'node_modules/@crestron/ch5-theme/output/themes/css/ch5-theme.css',
          dest: 'css'
        }
      ]
    }),
  ],
  base: './',
  build: {
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, 'index.html'),
        settings: path.resolve(__dirname, 'settings.html'),
      },
    },
  },
});
