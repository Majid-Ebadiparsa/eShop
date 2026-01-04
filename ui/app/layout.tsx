import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import { Navigation } from '@/components/Navigation'
import { BasketProvider } from '@/contexts/BasketContext'
import { BasketDrawer } from '@/components/BasketDrawer'
import { PWAInstallPrompt } from '@/components/PWAInstallPrompt'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'eShop - Modern Microservices E-Commerce',
  description: 'A modern e-commerce platform built with microservices architecture',
  manifest: '/manifest.json',
  themeColor: '#2563eb',
  viewport: 'width=device-width, initial-scale=1, maximum-scale=5',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'default',
    title: 'eShop',
  },
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <head>
        <link rel="apple-touch-icon" href="/icons/icon-192x192.png" />
      </head>
      <body className={inter.className}>
        <BasketProvider>
          <Navigation />
          <BasketDrawer />
          <PWAInstallPrompt />
          <main className="min-h-screen">
            {children}
          </main>
          <footer className="bg-gray-900 text-white py-8 mt-16">
            <div className="container mx-auto px-4 text-center">
              <p>&copy; 2024 eShop. Built with Next.js and Microservices.</p>
            </div>
          </footer>
        </BasketProvider>
      </body>
    </html>
  )
}

