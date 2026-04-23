import { defineConfig, normalizePath } from 'vite';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import { createSvgIconsPlugin } from 'vite-plugin-svg-icons';
import path from 'path';

export default defineConfig({
    plugins: [
        createSvgIconsPlugin({
            iconDirs: [path.resolve(process.cwd(), 'src/icons')],
            symbolId: 'icon-[dir]-[name]',
        }),
        ViteImageOptimizer({
            png: { quality: 80 },
            jpeg: { quality: 75 },
            webp: { lossless: true }
        }),
    ],
    build: {
        outDir: 'wwwroot/dist',
        emptyOutDir: true,
        sourcemap: true,
        rollupOptions: {
            input: {
                main: normalizePath(path.resolve(process.cwd(), 'src/js/app.js')),
                styles: normalizePath(path.resolve(process.cwd(), 'src/css/main.css'))
            },
            output: {
                entryFileNames: 'js/[name].js',
                assetFileNames: (assetInfo) => {
                    if (assetInfo.name && assetInfo.name.endsWith('.css')) {
                        return 'css/[name][extname]';
                    }
                    return 'assets/[name][extname]';
                }
            }
        }
    },
    resolve: {
        alias: {
            '@': normalizePath(path.resolve(process.cwd(), './src'))
        }
    }
});