'use client'

import { useEffect, useState } from 'react'
import { Package, AlertCircle } from 'lucide-react'
import api from '@/lib/api-client'
import { Product } from '@/types'
import { ProductCard } from '@/components/ProductCard'
import { ProductFilters } from '@/components/ProductFilters'
import { useBasket } from '@/contexts/BasketContext'

// Extract category from product name
function extractCategory(productName: string): string {
  if (productName.includes('Laptop') || productName.includes('Desktop') || 
      productName.includes('Monitor') || productName.includes('Keyboard') || 
      productName.includes('Mouse') || productName.includes('Headset') || 
      productName.includes('Webcam')) {
    return 'Electronics'
  }
  if (productName.includes('Smartphone') || productName.includes('Tablet') || 
      productName.includes('Smartwatch')) {
    return 'Mobile Devices'
  }
  if (productName.includes('Speakers') || productName.includes('Earbuds') || 
      productName.includes('Microphone')) {
    return 'Audio Equipment'
  }
  if (productName.includes('USB') || productName.includes('SSD') || 
      productName.includes('Power Bank') || productName.includes('Bag') || 
      productName.includes('Case') || productName.includes('Protector') || 
      productName.includes('Cable')) {
    return 'Accessories'
  }
  if (productName.includes('Desk') || productName.includes('Chair') || 
      productName.includes('Lamp') || productName.includes('Monitor Arm') || 
      productName.includes('Cable Management')) {
    return 'Home & Office'
  }
  return 'Other'
}

export default function ProductsPage() {
  const { addItem } = useBasket()
  const [products, setProducts] = useState<Product[]>([])
  const [filteredProducts, setFilteredProducts] = useState<Product[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedCategory, setSelectedCategory] = useState('')
  const [categories, setCategories] = useState<string[]>([])

  useEffect(() => {
    fetchProducts()
  }, [])

  useEffect(() => {
    filterProducts()
  }, [products, searchQuery, selectedCategory])

  const fetchProducts = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const response = await api.inventory.getAll()
      const productsData = response.data as Product[]
      
      setProducts(productsData)

      // Extract unique categories
      const uniqueCategories = Array.from(
        new Set(productsData.map(p => extractCategory(p.productName)))
      ).sort()
      setCategories(uniqueCategories)
    } catch (err: any) {
      console.error('Failed to fetch products:', err)
      setError(err.response?.data?.message || err.message || 'Failed to load products')
    } finally {
      setIsLoading(false)
    }
  }

  const filterProducts = () => {
    let filtered = [...products]

    // Apply search filter
    if (searchQuery) {
      filtered = filtered.filter(product =>
        product.productName.toLowerCase().includes(searchQuery.toLowerCase())
      )
    }

    // Apply category filter
    if (selectedCategory) {
      filtered = filtered.filter(product =>
        extractCategory(product.productName) === selectedCategory
      )
    }

    setFilteredProducts(filtered)
  }

  const handleAddToCart = (product: Product) => {
    addItem(product, 1)
  }

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center">
            <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-primary-600 mx-auto mb-4"></div>
            <p className="text-gray-600 dark:text-gray-400">Loading products...</p>
          </div>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-6">
          <div className="flex items-start">
            <AlertCircle className="w-6 h-6 text-red-600 dark:text-red-400 mr-3 flex-shrink-0 mt-0.5" />
            <div>
              <h3 className="text-lg font-semibold text-red-900 dark:text-red-200 mb-2">
                Failed to Load Products
              </h3>
              <p className="text-red-700 dark:text-red-300 mb-4">{error}</p>
              <button
                onClick={fetchProducts}
                className="bg-red-600 hover:bg-red-700 text-white font-medium py-2 px-4 rounded-lg transition-colors"
              >
                Try Again
              </button>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center">
          <Package className="w-8 h-8 text-primary-600 mr-3" />
          <div>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
              Product Catalog
            </h1>
            <p className="text-gray-600 dark:text-gray-400 mt-1">
              {filteredProducts.length} product{filteredProducts.length !== 1 ? 's' : ''} available
            </p>
          </div>
        </div>
      </div>

      {/* Filters */}
      <ProductFilters
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        selectedCategory={selectedCategory}
        onCategoryChange={setSelectedCategory}
        categories={categories}
      />

      {/* Products Grid */}
      {filteredProducts.length === 0 ? (
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-12 text-center">
          <Package className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
            No Products Found
          </h3>
          <p className="text-gray-600 dark:text-gray-400 mb-4">
            {searchQuery || selectedCategory
              ? 'Try adjusting your search or filters'
              : 'No products available at the moment'}
          </p>
          {(searchQuery || selectedCategory) && (
            <button
              onClick={() => {
                setSearchQuery('')
                setSelectedCategory('')
              }}
              className="bg-primary-600 hover:bg-primary-700 text-white font-medium py-2 px-6 rounded-lg transition-colors"
            >
              Clear Filters
            </button>
          )}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {filteredProducts.map((product) => (
            <ProductCard
              key={product.productId}
              product={product}
              onAddToCart={handleAddToCart}
            />
          ))}
        </div>
      )}

      {/* Summary Stats */}
      {products.length > 0 && (
        <div className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
            <p className="text-sm text-gray-600 dark:text-gray-400">Total Products</p>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {products.length}
            </p>
          </div>
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
            <p className="text-sm text-gray-600 dark:text-gray-400">Total Stock</p>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {products.reduce((sum, p) => sum + p.availableStock, 0).toLocaleString()}
            </p>
          </div>
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-4">
            <p className="text-sm text-gray-600 dark:text-gray-400">Categories</p>
            <p className="text-2xl font-bold text-gray-900 dark:text-white">
              {categories.length}
            </p>
          </div>
        </div>
      )}
    </div>
  )
}
