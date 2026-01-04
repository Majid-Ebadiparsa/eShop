'use client'

import Link from 'next/link'
import { Package, ShoppingCart } from 'lucide-react'
import { Product } from '@/types'

interface ProductCardProps {
  product: Product
  onAddToCart?: (product: Product) => void
}

export function ProductCard({ product, onAddToCart }: ProductCardProps) {
  const isLowStock = product.availableStock <= product.reorderLevel
  const isOutOfStock = product.availableStock === 0

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault()
    if (onAddToCart && !isOutOfStock) {
      onAddToCart(product)
    }
  }

  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md overflow-hidden hover:shadow-xl transition-shadow duration-300">
      {/* Product Image Placeholder */}
      <Link href={`/products/${product.productId}`}>
        <div className="bg-gradient-to-br from-primary-100 to-primary-200 dark:from-gray-700 dark:to-gray-600 h-48 flex items-center justify-center cursor-pointer">
          <Package className="w-16 h-16 text-primary-600 dark:text-primary-400" />
        </div>
      </Link>

      {/* Product Info */}
      <div className="p-4">
        <Link href={`/products/${product.productId}`}>
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2 hover:text-primary-600 dark:hover:text-primary-400 cursor-pointer line-clamp-2">
            {product.productName}
          </h3>
        </Link>

        {/* Stock Status */}
        <div className="mb-3">
          {isOutOfStock ? (
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
              Out of Stock
            </span>
          ) : isLowStock ? (
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
              Low Stock ({product.availableStock} left)
            </span>
          ) : (
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
              In Stock ({product.availableStock})
            </span>
          )}
        </div>

        {/* Product Details */}
        <div className="text-sm text-gray-600 dark:text-gray-400 space-y-1 mb-4">
          <div className="flex justify-between">
            <span>Available:</span>
            <span className="font-medium">{product.availableStock}</span>
          </div>
          {product.reservedStock > 0 && (
            <div className="flex justify-between">
              <span>Reserved:</span>
              <span className="font-medium">{product.reservedStock}</span>
            </div>
          )}
        </div>

        {/* Actions */}
        <div className="flex gap-2">
          <Link
            href={`/products/${product.productId}`}
            className="flex-1 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-900 dark:text-white font-medium py-2 px-4 rounded-lg transition-colors text-center"
          >
            View Details
          </Link>
          <button
            onClick={handleAddToCart}
            disabled={isOutOfStock}
            className={`flex items-center justify-center px-4 py-2 rounded-lg font-medium transition-colors ${
              isOutOfStock
                ? 'bg-gray-300 text-gray-500 cursor-not-allowed dark:bg-gray-700 dark:text-gray-500'
                : 'bg-primary-600 hover:bg-primary-700 text-white'
            }`}
            aria-label="Add to cart"
          >
            <ShoppingCart className="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>
  )
}

