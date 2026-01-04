// Common types for the eShop application

export interface Product {
  productId: string
  productName: string
  availableStock: number
  reservedStock: number
  reorderLevel: number
  lastRestocked?: string
}

export interface Order {
  orderId: string
  customerId: string
  items: OrderItem[]
  orderDate: string
  status: string
  totalAmount: number
}

export interface OrderItem {
  productId: string
  productName?: string
  quantity: number
  unitPrice: number
}

export interface Payment {
  paymentId: string
  orderId: string
  amount: number
  status: string
  paymentMethod: string
  createdAt: string
}

export interface Shipment {
  shipmentId: string
  orderId: string
  status: string
  trackingNumber?: string
  carrier?: string
  estimatedDeliveryDate?: string
  actualDeliveryDate?: string
}

export interface Invoice {
  id: string
  invoiceNumber: string
  customerId: string
  customerName: string
  amount: number
  issueDate: string
  dueDate: string
  status: string
}

export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
  errors?: string[]
}

export interface HealthStatus {
  status: string
  checks?: Record<string, any>
}

export interface BasketItem {
  productId: string
  productName: string
  quantity: number
  unitPrice: number
}

export interface Basket {
  items: BasketItem[]
  totalItems: number
  totalAmount: number
}

// Checkout types
export interface CheckoutFormData {
  customerName: string
  email: string
  phone: string
  street: string
  city: string
  postalCode: string
  country: string
}

export interface PlaceOrderRequest {
  customerId: string
  street: string
  city: string
  postalCode: string
  items: PlaceOrderItem[]
}

export interface PlaceOrderItem {
  productId: string
  quantity: number
  unitPrice: number
}

export interface PlaceOrderResponse {
  orderId: string
  message?: string
}
