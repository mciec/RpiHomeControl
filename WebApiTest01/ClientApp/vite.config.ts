import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react-swc'
//import http from "https";


// https://vitejs.dev/config/
// export default defineConfig({
//   plugins: [react()],
// })

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  console.log(`[mciec]environment: ${env.VITE_API_BASE_URL}`)
  return {
    plugins:  [react()],
    server: {
      port: 5173,
      proxy: {
        
        '/api': { 
          target: env.VITE_API_BASE_URL,
          changeOrigin: true,
          secure: false,
          
          
          configure: (proxy, _options) => {
            proxy.on('error', (err, _req, _res) => {
              console.log('proxy error', err);
            });
            proxy.on('proxyReq', (proxyReq, req, _res) => {
              //req.url = `${env.VITE_API_BASE_URL}//${req.url}`
              console.log('Sending Request to the Target:', req.method, req.url);
            });
            proxy.on('proxyRes', (proxyRes, req, _res) => {
              console.log('Received Response from the Target:', proxyRes.statusCode, req.url);
            });
          },
          rewrite: (path) => path.replace(/^\/api/, '')
        }
      }
    }
  }
})