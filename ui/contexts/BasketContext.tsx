'use client'

import { createContext, useContext, useEffect, useState, ReactNode } from 'react'
import { BasketItem, Product } from '@/types'

interface BasketContextType {
  items: BasketItem[]
  totalItems: number
  totalAmount: number
  addItem: (product: Product, quantity?: number) => void
  removeItem: (productId: string) => void
  updateQuantity: (productId: string, quantity: number) => void
  clearBasket: () => void
  isOpen: boolean
  openBasket: () => void
  closeBasket: () => void
  toggleBasket: () => void
}

const BasketContext = createContext<BasketContextType | undefined>(undefined)

const BASKET_STORAGE_KEY = 'eshop-basket'

export function BasketProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<BasketItem[]>([])
  const [isOpen, setIsOpen] = useState(false)
  const [isInitialized, setIsInitialized] = useState(false)

  // Load basket from localStorage on mount
  useEffect(() => {
    if (typeof window !== 'undefined') {
      try {
        const stored = localStorage.getItem(BASKET_STORAGE_KEY)
        if (stored) {
          const parsed = JSON.parse(stored)
          setItems(parsed)
        }
      } catch (error) {
        console.error('Failed to load basket from localStorage:', error)
      }
      setIsInitialized(true)
    }
  }, [])

  // Save basket to localStorage whenever it changes
  useEffect(() => {
    if (isInitialized && typeof window !== 'undefined') {
      try {
        localStorage.setItem(BASKET_STORAGE_KEY, JSON.stringify(items))
      } catch (error) {
        console.error('Failed to save basket to localStorage:', error)
      }
    }
  }, [items, isInitialized])

  // Calculate totals
  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0)
  
  // For now, use a fixed price per item (since Product model doesn't have price)
  // In production, price would come from product data
  const calculatePrice = (productName: string): number => {
    // Simple price estimation based on product type
    if (productName.includes('Laptop') || productName.includes('Desktop')) return 999
    if (productName.includes('Smartphone')) return 799
    if (productName.includes('Tablet')) return 599
    if (productName.includes('Monitor')) return 399
    if (productName.includes('Smartwatch')) return 299
    if (productName.includes('Speakers')) return 199
    if (productName.includes('Chair')) return 499
    if (productName.includes('Desk')) return 699
    if (productName.includes('Headset') || productName.includes('Keyboard') || productName.includes('Mouse')) return 99
    if (productName.includes('Earbuds')) return 149
    if (productName.includes('SSD')) return 179
    if (productName.includes('Power Bank')) return 49
    return 29 // Default price for accessories
  }

  const totalAmount = items.reduce((sum, item) => {
    const price = item.unitPrice || calculatePrice(item.productName)
    return sum + (price * item.quantity)
  }, 0)

  const addItem = (product: Product, quantity: number = 1) => {
    setItems(currentItems => {
      const existingIndex = currentItems.findIndex(item => item.productId === product.productId)
      
      if (existingIndex >= 0) {
        // Update quantity if item already exists
        const newItems = [...currentItems]
        const newQuantity = newItems[existingIndex].quantity + quantity
        
        // Check stock availability
        if (newQuantity > product.availableStock) {
          alert(`Cannot add more items. Only ${product.availableStock} available in stock.`)
          return currentItems
        }
        
        newItems[existingIndex].quantity = newQuantity
        return newItems
      } else {
        // Add new item
        if (quantity > product.availableStock) {
          alert(`Cannot add ${quantity} items. Only ${product.availableStock} available in stock.`)
          return currentItems
        }
        
        const price = calculatePrice(product.productName)
        const newItem: BasketItem = {
          productId: product.productId,
          productName: product.productName,
          quantity,
          unitPrice: price,
        }
        return [...currentItems, newItem]
      }
    })
    
    // Auto-open basket when item is added
    setIsOpen(true)
  }

  const removeItem = (productId: string) => {
    setItems(currentItems => currentItems.filter(item => item.productId !== productId))
  }

  const updateQuantity = (productId: string, quantity: number) => {
    if (quantity <= 0) {
      removeItem(productId)
      return
    }

    setItems(currentItems => {
      return currentItems.map(item => {
        if (item.productId === productId) {
          return { ...item, quantity }
        }
        return item
      })
    })
  }

  const clearBasket = () => {
    setItems([])
    setIsOpen(false)
  }

  const openBasket = () => setIsOpen(true)
  const closeBasket = () => setIsOpen(false)
  const toggleBasket = () => setIsOpen(prev => !prev)

  return (
    <BasketContext.Provider
      value={{
        items,
        totalItems,
        totalAmount,
        addItem,
        removeItem,
        updateQuantity,
        clearBasket,
        isOpen,
        openBasket,
        closeBasket,
        toggleBasket,
      }}
    >
      {children}
    </BasketContext.Provider>
  )
}

export function useBasket() {
  const context = useContext(BasketContext)
  if (context === undefined) {
    throw new Error('useBasket must be used within a BasketProvider')
  }
  return context
}

