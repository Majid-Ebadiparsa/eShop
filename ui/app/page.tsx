'use client'

import { useEffect, useState } from 'react'
import { apiClient } from '@/lib/api-client'
import { ShoppingBag, Package, CreditCard, Truck, CheckCircle } from 'lucide-react'

interface HealthStatus {
  status: string
  checks?: Record<string, any>
}

export default function Home() {
  const [healthStatus, setHealthStatus] = useState<HealthStatus | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const checkHealth = async () => {
      try {
        setIsLoading(true)
        setError(null)
        // Test API connectivity by calling a health endpoint
        const response = await apiClient.get('/health/order/ready')
        setHealthStatus(response.data)
      } catch (err: any) {
        setError(err.message || 'Failed to connect to API Gateway')
        console.error('Health check failed:', err)
      } finally {
        setIsLoading(false)
      }
    }

    checkHealth()
  }, [])

  const features = [
    {
      icon: ShoppingBag,
      title: 'Browse Products',
      description: 'Explore our catalog of products across multiple categories'
    },
    {
      icon: Package,
      title: 'Real-time Inventory',
      description: 'Check product availability with live inventory updates'
    },
    {
      icon: CreditCard,
      title: 'Secure Payments',
      description: 'Process payments securely through our payment service'
    },
    {
      icon: Truck,
      title: 'Track Deliveries',
      description: 'Monitor your order status and shipment in real-time'
    }
  ]

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Hero Section */}
      <div className="text-center py-16">
        <h1 className="text-5xl font-bold text-gray-900 dark:text-white mb-4">
          Welcome to eShop
        </h1>
        <p className="text-xl text-gray-600 dark:text-gray-300 mb-8 max-w-2xl mx-auto">
          A modern e-commerce platform powered by microservices architecture
        </p>

        {/* API Connection Status */}
        <div className="max-w-md mx-auto mb-8">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
            <h3 className="text-lg font-semibold mb-4">API Gateway Status</h3>
            {isLoading ? (
              <div className="flex items-center justify-center">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
              </div>
            ) : error ? (
              <div className="text-red-600 dark:text-red-400">
                <p className="font-medium">⚠️ Connection Error</p>
                <p className="text-sm mt-2">{error}</p>
              </div>
            ) : (
              <div className="text-green-600 dark:text-green-400">
                <CheckCircle className="w-12 h-12 mx-auto mb-2" />
                <p className="font-medium">✓ Connected Successfully</p>
                <p className="text-sm mt-2 text-gray-600 dark:text-gray-400">
                  Status: {healthStatus?.status || 'Healthy'}
                </p>
              </div>
            )}
          </div>
        </div>

        <div className="flex gap-4 justify-center">
          <a
            href="/products"
            className="bg-primary-600 hover:bg-primary-700 text-white font-semibold py-3 px-8 rounded-lg transition-colors"
          >
            Browse Products
          </a>
          <a
            href="/orders"
            className="bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white font-semibold py-3 px-8 rounded-lg transition-colors"
          >
            View Orders
          </a>
        </div>
      </div>

      {/* Features Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mt-16">
        {features.map((feature, index) => {
          const Icon = feature.icon
          return (
            <div
              key={index}
              className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
            >
              <Icon className="w-12 h-12 text-primary-600 mb-4" />
              <h3 className="text-xl font-semibold mb-2 text-gray-900 dark:text-white">
                {feature.title}
              </h3>
              <p className="text-gray-600 dark:text-gray-300">
                {feature.description}
              </p>
            </div>
          )
        })}
      </div>

      {/* Architecture Info */}
      <div className="mt-16 bg-gradient-to-r from-primary-50 to-blue-50 dark:from-gray-800 dark:to-gray-900 rounded-lg p-8">
        <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
          Built with Modern Architecture
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
          <div>
            <h3 className="font-semibold text-lg mb-2 text-gray-900 dark:text-white">
              Microservices
            </h3>
            <p className="text-gray-600 dark:text-gray-300">
              Independent services for Order, Inventory, Payment, Delivery, and Invoice processing
            </p>
          </div>
          <div>
            <h3 className="font-semibold text-lg mb-2 text-gray-900 dark:text-white">
              CQRS + Event Sourcing
            </h3>
            <p className="text-gray-600 dark:text-gray-300">
              Separated read and write models with MongoDB projections and event-driven architecture
            </p>
          </div>
          <div>
            <h3 className="font-semibold text-lg mb-2 text-gray-900 dark:text-white">
              API Gateway
            </h3>
            <p className="text-gray-600 dark:text-gray-300">
              Ocelot gateway with Consul service discovery and health monitoring
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}

