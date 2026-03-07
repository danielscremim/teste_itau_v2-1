import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Redireciona chamadas /api/* para a API .NET em localhost:5152
      '/api': {
        target: 'http://localhost:5152',
        changeOrigin: true,
      },
    },
  },
})
