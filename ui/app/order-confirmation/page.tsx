'use client'

import { useEffect, useState, Suspense } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import Link from 'next/link'
import { CheckCircle, Package, Truck, CreditCard, Home, ArrowRight } from 'lucide-react'

function OrderConfirmationContent() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const orderId = searchParams.get('orderId')
  
  const [orderInfo, setOrderInfo] = useState<any>(null)

  useEffect(() => {
    // Load order info from sessionStorage
    const stored = sessionStorage.getItem('lastOrder')
    if (stored) {
      setOrderInfo(JSON.parse(stored))
      // Clear from storage after loading
      sessionStorage.removeItem('lastOrder')
    } else if (!orderId) {
      // No order info available, redirect to products
      router.push('/products')
    }
  }, [orderId, router])

  if (!orderId) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <p className="text-gray-600 dark:text-gray-400">Loading...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="max-w-3xl mx-auto">
        {/* Success Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-green-100 dark:bg-green-900/20 rounded-full mb-4">
            <CheckCircle className="w-12 h-12 text-green-600 dark:text-green-400" />
          </div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
            Order Placed Successfully!
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Thank you for your order. We've received your order and will begin processing it shortly.
          </p>
        </div>

        {/* Order Details Card */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
          <div className="border-b border-gray-200 dark:border-gray-700 pb-4 mb-4">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
              Order Details
            </h2>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="text-gray-600 dark:text-gray-400">Order Number</p>
                <p className="font-mono font-semibold text-gray-900 dark:text-white break-all">
                  {orderId}
                </p>
              </div>
              {orderInfo && (
                <>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400">Order Date</p>
                    <p className="font-semibold text-gray-900 dark:text-white">
                      {new Date().toLocaleDateString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400">Customer</p>
                    <p className="font-semibold text-gray-900 dark:text-white">
                      {orderInfo.customerName}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400">Email</p>
                    <p className="font-semibold text-gray-900 dark:text-white">
                      {orderInfo.email}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400">Total Items</p>
                    <p className="font-semibold text-gray-900 dark:text-white">
                      {orderInfo.items}
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400">Total Amount</p>
                    <p className="font-semibold text-primary-600 dark:text-primary-400">
                      ${orderInfo.totalAmount.toFixed(2)}
                    </p>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Confirmation Email Notice */}
          <div className="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4">
            <p className="text-sm text-blue-800 dark:text-blue-200">
              📧 A confirmation email has been sent to {orderInfo?.email || 'your email address'}
            </p>
          </div>
        </div>

        {/* What Happens Next */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 mb-6">
          <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
            What Happens Next?
          </h2>
          
          <div className="space-y-4">
            {/* Step 1 */}
            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-primary-100 dark:bg-primary-900/20 rounded-full flex items-center justify-center">
                  <Package className="w-5 h-5 text-primary-600 dark:text-primary-400" />
                </div>
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white mb-1">
                  Order Processing
                </h3>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  Our system is processing your order and reserving inventory.
                </p>
              </div>
            </div>

            {/* Step 2 */}
            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-primary-100 dark:bg-primary-900/20 rounded-full flex items-center justify-center">
                  <CreditCard className="w-5 h-5 text-primary-600 dark:text-primary-400" />
                </div>
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white mb-1">
                  Payment Authorization
                </h3>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  Your payment will be authorized and processed securely.
                </p>
              </div>
            </div>

            {/* Step 3 */}
            <div className="flex gap-4">
              <div className="flex-shrink-0">
                <div className="w-10 h-10 bg-primary-100 dark:bg-primary-900/20 rounded-full flex items-center justify-center">
                  <Truck className="w-5 h-5 text-primary-600 dark:text-primary-400" />
                </div>
              </div>
              <div>
                <h3 className="font-semibold text-gray-900 dark:text-white mb-1">
                  Shipment Creation
                </h3>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  Once payment is confirmed, we'll prepare your order for shipping.
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-4">
          <Link
            href={`/orders/${orderId}`}
            className="flex-1 bg-primary-600 hover:bg-primary-700 text-white font-semibold py-3 px-6 rounded-lg transition-colors text-center flex items-center justify-center gap-2"
          >
            View Order Status
            <ArrowRight className="w-5 h-5" />
          </Link>
          <Link
            href="/products"
            className="flex-1 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white font-semibold py-3 px-6 rounded-lg transition-colors text-center flex items-center justify-center gap-2"
          >
            <Home className="w-5 h-5" />
            Continue Shopping
          </Link>
        </div>

        {/* Event Flow Notice (for testing) */}
        <div className="mt-8 bg-gray-50 dark:bg-gray-900 rounded-lg p-6">
          <h3 className="font-semibold text-gray-900 dark:text-white mb-2">
            🔄 Background Processing
          </h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
            Your order is now flowing through our microservices architecture:
          </p>
          <ul className="text-sm text-gray-600 dark:text-gray-400 space-y-1 list-disc list-inside">
            <li>OrderCreatedEvent → InventoryService (reserving stock)</li>
            <li>InventoryReserved → PaymentService (authorizing payment)</li>
            <li>PaymentCaptured → DeliveryService (creating shipment)</li>
            <li>ShipmentCreated → OrderService (updating status)</li>
          </ul>
          <p className="text-xs text-gray-500 dark:text-gray-500 mt-3">
            💡 Check docker logs to see the event flow in real-time!
          </p>
        </div>
      </div>
    </div>
  )
}

export default function OrderConfirmationPage() {
  return (
    <Suspense fallback={
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <p className="text-gray-600 dark:text-gray-400">Loading order confirmation...</p>
        </div>
      </div>
    }>
      <OrderConfirmationContent />
    </Suspense>
  )
}

