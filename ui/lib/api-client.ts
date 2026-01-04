import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse, AxiosError } from 'axios'

const API_GATEWAY_URL = process.env.NEXT_PUBLIC_API_GATEWAY_URL || 'http://localhost:5000'

// Create axios instance with default configuration
const apiClient: AxiosInstance = axios.create({
  baseURL: API_GATEWAY_URL,
  timeout: 30000, // 30 seconds
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor for adding auth tokens or logging
apiClient.interceptors.request.use(
  (config) => {
    // Add timestamp for debugging
    console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`)
    
    // Add auth token if available (future implementation)
    // const token = localStorage.getItem('authToken')
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`
    // }
    
    return config
  },
  (error) => {
    console.error('[API] Request error:', error)
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    console.log(`[API] Response ${response.status} from ${response.config.url}`)
    return response
  },
  (error: AxiosError) => {
    if (error.response) {
      // Server responded with error status
      console.error(`[API] Error ${error.response.status}:`, error.response.data)
      
      // Handle specific error codes
      switch (error.response.status) {
        case 401:
          // Unauthorized - redirect to login (future implementation)
          console.error('Unauthorized access')
          break
        case 404:
          console.error('Resource not found')
          break
        case 500:
          console.error('Server error')
          break
      }
    } else if (error.request) {
      // Request made but no response received
      console.error('[API] No response received:', error.message)
    } else {
      // Error in request setup
      console.error('[API] Request setup error:', error.message)
    }
    
    return Promise.reject(error)
  }
)

// Typed API methods
export const api = {
  // Generic methods
  get: <T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> =>
    apiClient.get<T>(url, config),
  
  post: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> =>
    apiClient.post<T>(url, data, config),
  
  put: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> =>
    apiClient.put<T>(url, data, config),
  
  patch: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> =>
    apiClient.patch<T>(url, data, config),
  
  delete: <T = any>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> =>
    apiClient.delete<T>(url, config),

  // Service-specific methods
  order: {
    getAll: () => api.get('/api/order'),
    getById: (id: string) => api.get(`/api/order/${id}`),
    create: (data: any) => api.post('/api/order', data),
  },

  inventory: {
    getAll: () => api.get('/api/inventory'),
    getByProductId: (productId: string) => api.get(`/api/inventory/${productId}`),
    seed: () => api.post('/api/inventory/seed'),
  },

  payment: {
    getById: (id: string) => api.get(`/api/payment/${id}`),
    initiate: (data: any) => api.post('/api/payment/initiate', data),
    refund: (id: string, data: any) => api.post(`/api/payment/${id}/refund`, data),
    cancel: (id: string) => api.post(`/api/payment/${id}/cancel`),
  },

  shipment: {
    getById: (id: string) => api.get(`/api/shipment/${id}`),
    getByOrderId: (orderId: string) => api.get(`/api/shipment/order/${orderId}`),
  },

  invoice: {
    getAll: () => api.get('/api/v1/invoices'),
    getById: (id: string) => api.get(`/api/v1/invoices/${id}`),
    create: (data: any) => api.post('/api/v1/invoices', data),
  },

  health: {
    order: () => api.get('/health/order/ready'),
    inventory: () => api.get('/health/inventory/ready'),
    payment: () => api.get('/health/payment/ready'),
    delivery: () => api.get('/health/delivery/ready'),
    invoice: () => api.get('/health/invoice/ready'),
    monitor: () => api.get('/api/health/status'),
  },
}

export { apiClient }
export default api

