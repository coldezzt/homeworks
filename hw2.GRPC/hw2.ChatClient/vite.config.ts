import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/grpc': {
        target: 'http://localhost:1400',
        changeOrigin: true,
        ws: true
      }
    }
  }
})
