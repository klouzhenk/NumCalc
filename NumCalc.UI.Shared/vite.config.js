import { defineConfig } from 'vite';
import { ViteImageOptimizer } from 'vite-plugin-image-optimizer';
import path from 'path';

export default defineConfig({
    plugins: [
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
                main: path.resolve(__dirname, 'src/js/app.js'),
                styles: path.resolve(__dirname, 'src/css/main.css')
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
            '@': path.resolve(__dirname, './src')
        }
    }
})