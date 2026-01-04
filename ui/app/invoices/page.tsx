'use client'

import { FileText } from 'lucide-react'

export default function InvoicesPage() {
  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex items-center mb-8">
        <FileText className="w-8 h-8 text-primary-600 mr-3" />
        <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
          Invoices
        </h1>
      </div>

      <div className="bg-white dark:bg-gray-800 rounded-lg shadow-md p-8 text-center">
        <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
          Coming Soon
        </h2>
        <p className="text-gray-600 dark:text-gray-300">
          Invoice management will be integrated in future steps
        </p>
      </div>
    </div>
  )
}

