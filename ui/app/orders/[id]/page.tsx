'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { ArrowLeft, Package, Loader2, AlertCircle, CheckCircle, Clock, Truck } from 'lucide-react'
import api from '@/lib/api-client'
import { Order } from '@/types'

export default function OrderStatusPage({ params }: { params: { id: string } }) {
  const [order, setOrder] = useState<Order | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchOrder()
    // Poll every 5 seconds to check for status updates
    const interval = setInterval(fetchOrder, 5000)
    return () => clearInterval(interval)
  }, [params.id])

  const fetchOrder = async () => {
    try {
      setError(null)
      const response = await api.order.getById(params.id)
      setOrder(response.data as Order)
    } catch (err: any) {
      console.error('Failed to fetch order:', err)
      setError(err.response?.data?.message || err.message || 'Failed to load order')
    } finally {
      setIsLoading(false)
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'completed':
      case 'delivered':
        return <CheckCircle className="w-6 h-6 text-green-600" />
      case 'shipped':
      case 'dispatched':
        return <Truck className="w-6 h-6 text-blue-600" />
      case 'processing':
      case 'pending':
        return <Clock className="w-6 h-6 text-yellow-600" />
      case 'failed':
      case 'cancelled':
        return <AlertCircle className="w-6 h-6 text-red-600" />
      default:
        return <Package className="w-6 h-6 text-gray-600" />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'completed':
      case 'delivered':
        return 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-200'
      case 'shipped':
      case 'dispatched':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-200'
      case 'processing':
      case 'pending':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-200'
      case 'failed':
      case 'cancelled':
        return 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-200'
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-200'
    }
  }

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <Loader2 className="w-12 h-12 text-primary-600 animate-spin mx-auto mb-4" />
            <p className="text-gray-600 dark:text-gray-400">Loading order details...</p>
          </div>
        </div>
      </div>
    )
  }

  if (error || !order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Link
          href="/products"
          className="inline-flex items-center text-primary-600 hover:text-primary-700 mb-6"
        >
          <ArrowLeft className="w-4 h-4 mr-2" />
          Back to Products
        </Link>

        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6">
          <div className="flex items-start">
            <AlertCircle className="w-6 h-6 text-red-600 dark:text-red-400 mr-3 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="text-lg font-semibold text-red-900 dark:text-red-200 mb-2">
                Order Not Found
              </h3>
              <p className="text-red-700 dark:text-red-300">{error || 'The order you are looking for does not exist.'}</p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <Link
        href="/products"
        className="inline-flex items-center text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 mb-6 transition-colors"
      >
        <ArrowLeft className="w-4 h-4 mr-2" />
        Back to Products
      </Link>

      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
          <div className="flex items-start justify-between mb-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                Order Details
              </h1>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Order ID: <span className="font-mono">{order.orderId}</span>
              </p>
            </div>
            <div className="flex items-center gap-2">
              {getStatusIcon(order.status)}
              <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(order.status)}`}>
                {order.status}
              </span>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
            <div>
              <p className="text-gray-600 dark:text-gray-400">Order Date</p>
              <p className="font-semibold text-gray-900 dark:text-white">
                {new Date(order.orderDate).toLocaleDateString()}
              </p>
            </div>
            <div>
              <p className="text-gray-600 dark:text-gray-400">Customer ID</p>
              <p className="font-mono text-sm text-gray-900 dark:text-white break-all">
                {order.customerId}
              </p>
            </div>
            <div>
              <p className="text-gray-600 dark:text-gray-400">Total Amount</p>
              <p className="font-bold text-primary-600 dark:text-primary-400">
                ${order.totalAmount.toFixed(2)}
              </p>
            </div>
          </div>
        </div>

        {/* Order Items */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            Order Items
          </h2>
          <div className="space-y-4">
            {order.items.map((item, index) => (
              <div
                key={index}
                className="flex justify-between items-center py-3 border-b border-gray-200 dark:border-gray-700 last:border-0"
              >
                <div className="flex-1">
                  <p className="font-medium text-gray-900 dark:text-white">
                    {item.productName || `Product ${item.productId.substring(0, 8)}...`}
                  </p>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Quantity: {item.quantity} × ${item.unitPrice.toFixed(2)}
                  </p>
                </div>
                <p className="font-semibold text-gray-900 dark:text-white">
                  ${(item.quantity * item.unitPrice).toFixed(2)}
                </p>
              </div>
            ))}
          </div>
        </div>

        {/* Auto-refresh Notice */}
        <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4">
          <p className="text-sm text-blue-800 dark:text-blue-200">
            🔄 This page auto-refreshes every 5 seconds to show the latest order status
          </p>
        </div>
      </div>
    </div>
  )
}

