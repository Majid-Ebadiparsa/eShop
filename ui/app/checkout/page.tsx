'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { ShoppingBag, CreditCard, MapPin, User, Mail, Phone, ArrowLeft, Loader2 } from 'lucide-react'
import { useBasket } from '@/contexts/BasketContext'
import api from '@/lib/api-client'
import { CheckoutFormData, PlaceOrderRequest } from '@/types'

export default function CheckoutPage() {
  const router = useRouter()
  const { items, totalAmount, clearBasket } = useBasket()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [formData, setFormData] = useState<CheckoutFormData>({
    customerName: '',
    email: '',
    phone: '',
    street: '',
    city: '',
    postalCode: '',
    country: 'USA',
  })

  // Redirect if cart is empty
  if (items.length === 0 && !isSubmitting) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="max-w-2xl mx-auto text-center">
          <ShoppingBag className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            Your cart is empty
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mb-6">
            Add some products before proceeding to checkout.
          </p>
          <Link
            href="/products"
            className="inline-block bg-primary-600 hover:bg-primary-700 text-white font-semibold py-3 px-8 rounded-lg transition-colors"
          >
            Browse Products
          </Link>
        </div>
      </div>
    )
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({ ...prev, [name]: value }))
  }

  const validateForm = (): boolean => {
    if (!formData.customerName.trim()) {
      setError('Please enter your name')
      return false
    }
    if (!formData.email.trim() || !formData.email.includes('@')) {
      setError('Please enter a valid email address')
      return false
    }
    if (!formData.street.trim()) {
      setError('Please enter your street address')
      return false
    }
    if (!formData.city.trim()) {
      setError('Please enter your city')
      return false
    }
    if (!formData.postalCode.trim()) {
      setError('Please enter your postal code')
      return false
    }
    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!validateForm()) {
      return
    }

    setIsSubmitting(true)

    try {
      // Generate a customer ID (in production, this would come from authentication)
      const customerId = crypto.randomUUID()

      // Prepare order request
      const orderRequest: PlaceOrderRequest = {
        customerId,
        street: formData.street,
        city: formData.city,
        postalCode: formData.postalCode,
        items: items.map(item => ({
          productId: item.productId,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
        })),
      }

      console.log('Placing order:', orderRequest)

      // Call order creation API
      const response = await api.order.create(orderRequest)
      
      console.log('Order created:', response.data)

      // Store order info for confirmation page
      sessionStorage.setItem('lastOrder', JSON.stringify({
        orderId: response.data,
        customerName: formData.customerName,
        email: formData.email,
        totalAmount,
        items: items.length,
      }))

      // Store order in history for orders page
      sessionStorage.setItem(`order_${response.data}`, JSON.stringify({
        orderId: response.data,
        orderDate: new Date().toISOString(),
        status: 'Pending',
        totalAmount,
        itemCount: items.length,
      }))

      // Clear basket after successful order
      clearBasket()

      // Redirect to confirmation page
      router.push(`/order-confirmation?orderId=${response.data}`)
    } catch (err: any) {
      console.error('Order creation failed:', err)
      
      // Handle specific error scenarios
      if (err.response?.status === 400) {
        setError('Invalid order data. Please check your information and try again.')
      } else if (err.response?.data?.message) {
        setError(err.response.data.message)
      } else if (err.message?.includes('Inventory')) {
        setError('Some items are out of stock. Please review your cart.')
      } else if (err.message?.includes('Payment')) {
        setError('Payment processing failed. Please try again.')
      } else {
        setError('Failed to create order. Please try again later.')
      }
      
      setIsSubmitting(false)
    }
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <Link
        href="/products"
        className="inline-flex items-center text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 mb-6 transition-colors"
      >
        <ArrowLeft className="w-4 h-4 mr-2" />
        Continue Shopping
      </Link>

      <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-8">
        Checkout
      </h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Checkout Form */}
        <div className="lg:col-span-2">
          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Customer Information */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
              <div className="flex items-center mb-4">
                <User className="w-5 h-5 text-primary-600 mr-2" />
                <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                  Customer Information
                </h2>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Full Name *
                  </label>
                  <input
                    type="text"
                    name="customerName"
                    value={formData.customerName}
                    onChange={handleInputChange}
                    required
                    className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    placeholder="John Doe"
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Email *
                    </label>
                    <div className="relative">
                      <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                      <input
                        type="email"
                        name="email"
                        value={formData.email}
                        onChange={handleInputChange}
                        required
                        className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                        placeholder="john@example.com"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Phone
                    </label>
                    <div className="relative">
                      <Phone className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                      <input
                        type="tel"
                        name="phone"
                        value={formData.phone}
                        onChange={handleInputChange}
                        className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                        placeholder="+1 (555) 123-4567"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Shipping Address */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
              <div className="flex items-center mb-4">
                <MapPin className="w-5 h-5 text-primary-600 mr-2" />
                <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                  Shipping Address
                </h2>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Street Address *
                  </label>
                  <input
                    type="text"
                    name="street"
                    value={formData.street}
                    onChange={handleInputChange}
                    required
                    className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    placeholder="123 Main Street"
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div className="md:col-span-1">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      City *
                    </label>
                    <input
                      type="text"
                      name="city"
                      value={formData.city}
                      onChange={handleInputChange}
                      required
                      className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      placeholder="New York"
                    />
                  </div>

                  <div className="md:col-span-1">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Postal Code *
                    </label>
                    <input
                      type="text"
                      name="postalCode"
                      value={formData.postalCode}
                      onChange={handleInputChange}
                      required
                      className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      placeholder="10001"
                    />
                  </div>

                  <div className="md:col-span-1">
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Country *
                    </label>
                    <select
                      name="country"
                      value={formData.country}
                      onChange={handleInputChange}
                      required
                      className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    >
                      <option value="USA">United States</option>
                      <option value="CAN">Canada</option>
                      <option value="GBR">United Kingdom</option>
                      <option value="AUS">Australia</option>
                    </select>
                  </div>
                </div>
              </div>
            </div>

            {/* Error Message */}
            {error && (
              <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
                <p className="text-red-800 dark:text-red-200">{error}</p>
              </div>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white font-semibold py-3 px-6 rounded-lg transition-colors flex items-center justify-center gap-2"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="w-5 h-5 animate-spin" />
                  Processing Order...
                </>
              ) : (
                <>
                  <CreditCard className="w-5 h-5" />
                  Place Order
                </>
              )}
            </button>
          </form>
        </div>

        {/* Order Summary */}
        <div className="lg:col-span-1">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 sticky top-20">
            <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
              Order Summary
            </h2>

            {/* Items List */}
            <div className="space-y-3 mb-4 max-h-64 overflow-y-auto">
              {items.map((item) => (
                <div key={item.productId} className="flex justify-between text-sm">
                  <div className="flex-1">
                    <p className="font-medium text-gray-900 dark:text-white line-clamp-1">
                      {item.productName}
                    </p>
                    <p className="text-gray-600 dark:text-gray-400">
                      Qty: {item.quantity} × ${item.unitPrice.toFixed(2)}
                    </p>
                  </div>
                  <p className="font-medium text-gray-900 dark:text-white">
                    ${(item.quantity * item.unitPrice).toFixed(2)}
                  </p>
                </div>
              ))}
            </div>

            {/* Totals */}
            <div className="border-t border-gray-200 dark:border-gray-700 pt-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-600 dark:text-gray-400">Subtotal</span>
                <span className="text-gray-900 dark:text-white">${totalAmount.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-600 dark:text-gray-400">Shipping</span>
                <span className="text-gray-900 dark:text-white">Free</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-600 dark:text-gray-400">Tax</span>
                <span className="text-gray-900 dark:text-white">$0.00</span>
              </div>
              <div className="border-t border-gray-200 dark:border-gray-700 pt-2 flex justify-between">
                <span className="text-lg font-bold text-gray-900 dark:text-white">Total</span>
                <span className="text-lg font-bold text-primary-600 dark:text-primary-400">
                  ${totalAmount.toFixed(2)}
                </span>
              </div>
            </div>

            {/* Payment Info */}
            <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
              <p className="text-sm text-blue-800 dark:text-blue-200">
                <CreditCard className="w-4 h-4 inline mr-1" />
                Payment will be processed by our secure payment service
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

