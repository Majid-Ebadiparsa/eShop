'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { Package, ShoppingCart, ArrowLeft, AlertCircle, CheckCircle, AlertTriangle } from 'lucide-react'
import api from '@/lib/api-client'
import { Product } from '@/types'
import { useBasket } from '@/contexts/BasketContext'

export default function ProductDetailPage({ params }: { params: { id: string } }) {
  const router = useRouter()
  const { addItem } = useBasket()
  const [product, setProduct] = useState<Product | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [quantity, setQuantity] = useState(1)

  useEffect(() => {
    fetchProduct()
  }, [params.id])

  const fetchProduct = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const response = await api.inventory.getByProductId(params.id)
      setProduct(response.data as Product)
    } catch (err: any) {
      console.error('Failed to fetch product:', err)
      setError(err.response?.data?.message || err.message || 'Failed to load product')
    } finally {
      setIsLoading(false)
    }
  }

  const handleAddToCart = () => {
    if (!product) return
    addItem(product, quantity)
  }

  const handleQuantityChange = (newQuantity: number) => {
    if (!product) return
    const max = product.availableStock
    const validQuantity = Math.max(1, Math.min(newQuantity, max))
    setQuantity(validQuantity)
  }

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-primary-600 mx-auto mb-4"></div>
            <p className="text-gray-600 dark:text-gray-400">Loading product...</p>
          </div>
        </div>
      </div>
    )
  }

  if (error || !product) {
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
                Product Not Found
              </h3>
              <p className="text-red-700 dark:text-red-300 mb-4">
                {error || 'The product you are looking for does not exist.'}
              </p>
              <Link
                href="/products"
                className="inline-block bg-red-600 hover:bg-red-700 text-white font-medium py-2 px-4 rounded-lg transition-colors"
              >
                Browse Products
              </Link>
            </div>
          </div>
        </div>
      </div>
    )
  }

  const isOutOfStock = product.availableStock === 0
  const isLowStock = product.availableStock <= product.reorderLevel && !isOutOfStock

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Back Button */}
      <Link
        href="/products"
        className="inline-flex items-center text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 mb-6 transition-colors"
      >
        <ArrowLeft className="w-4 h-4 mr-2" />
        Back to Products
      </Link>

      {/* Product Details */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Product Image */}
        <div className="bg-gradient-to-br from-primary-100 to-primary-200 dark:from-gray-700 dark:to-gray-600 rounded-lg p-12 flex items-center justify-center">
          <Package className="w-48 h-48 text-primary-600 dark:text-primary-400" />
        </div>

        {/* Product Info */}
        <div className="space-y-6">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              {product.productName}
            </h1>

            {/* Stock Status Badge */}
            <div className="mb-4">
              {isOutOfStock ? (
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                  <AlertCircle className="w-4 h-4 mr-1" />
                  Out of Stock
                </span>
              ) : isLowStock ? (
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
                  <AlertTriangle className="w-4 h-4 mr-1" />
                  Low Stock
                </span>
              ) : (
                <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                  <CheckCircle className="w-4 h-4 mr-1" />
                  In Stock
                </span>
              )}
            </div>
          </div>

          {/* Stock Information */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 space-y-4">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
              Availability
            </h2>
            <div className="space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Available Stock:</span>
                <span className="text-lg font-bold text-gray-900 dark:text-white">
                  {product.availableStock}
                </span>
              </div>
              {product.reservedStock > 0 && (
                <div className="flex justify-between items-center">
                  <span className="text-gray-600 dark:text-gray-400">Reserved Stock:</span>
                  <span className="text-lg font-semibold text-gray-900 dark:text-white">
                    {product.reservedStock}
                  </span>
                </div>
              )}
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Reorder Level:</span>
                <span className="text-gray-900 dark:text-white">
                  {product.reorderLevel}
                </span>
              </div>
              {product.lastRestocked && (
                <div className="flex justify-between items-center">
                  <span className="text-gray-600 dark:text-gray-400">Last Restocked:</span>
                  <span className="text-gray-900 dark:text-white">
                    {new Date(product.lastRestocked).toLocaleDateString()}
                  </span>
                </div>
              )}
            </div>
          </div>

          {/* Quantity Selector & Add to Cart */}
          {!isOutOfStock && (
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-6 space-y-4">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                Quantity
              </h2>
              <div className="flex items-center gap-4">
                <button
                  onClick={() => handleQuantityChange(quantity - 1)}
                  disabled={quantity <= 1}
                  className="w-10 h-10 rounded-lg border border-gray-300 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
                >
                  -
                </button>
                <input
                  type="number"
                  value={quantity}
                  onChange={(e) => handleQuantityChange(parseInt(e.target.value) || 1)}
                  min="1"
                  max={product.availableStock}
                  className="w-20 text-center border border-gray-300 dark:border-gray-600 rounded-lg py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                />
                <button
                  onClick={() => handleQuantityChange(quantity + 1)}
                  disabled={quantity >= product.availableStock}
                  className="w-10 h-10 rounded-lg border border-gray-300 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed font-semibold"
                >
                  +
                </button>
                <span className="text-sm text-gray-600 dark:text-gray-400 ml-2">
                  Max: {product.availableStock}
                </span>
              </div>

              <button
                onClick={handleAddToCart}
                className="w-full bg-primary-600 hover:bg-primary-700 text-white font-semibold py-3 px-6 rounded-lg transition-colors flex items-center justify-center gap-2"
              >
                <ShoppingCart className="w-5 h-5" />
                Add to Cart
              </button>
            </div>
          )}

          {/* Product ID */}
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Product ID: {product.productId}
          </div>
        </div>
      </div>

      {/* Additional Information */}
      <div className="mt-12 bg-white dark:bg-gray-800 rounded-lg shadow-md p-6">
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
          Product Information
        </h2>
        <div className="prose dark:prose-invert max-w-none">
          <p className="text-gray-600 dark:text-gray-400">
            This is a sample product in the eShop catalog. In a production environment,
            this section would contain detailed product descriptions, specifications,
            features, reviews, and other relevant information.
          </p>
        </div>
      </div>
    </div>
  )
}

