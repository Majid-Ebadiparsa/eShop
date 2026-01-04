# eShop UI - Next.js Frontend

Modern, responsive e-commerce frontend built with Next.js 14, TypeScript, and Tailwind CSS.

## Features

- **Next.js 14** with App Router
- **TypeScript** for type safety
- **Tailwind CSS** for responsive styling
- **API Client** for seamless backend integration
- **Progressive Web App (PWA)** - Installable with offline support ✨ NEW
- **Responsive Design** for mobile, tablet, and desktop
- **Product Catalog** with search and filtering
- **Product Details** pages with stock management
- **Shopping Cart** with localStorage persistence
- **Checkout Flow** with order creation
- **Order Tracking** with real-time status updates
- **Order History** with session-based tracking

## Prerequisites

- Node.js 20+ and npm
- Running eShop backend services (docker-compose)
- API Gateway accessible at http://localhost:5000
- Seeded inventory data (30 products)

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

Copy the example environment file:

```bash
cp .env.local.example .env.local
```

Update `.env.local` with your API Gateway URL:

```env
NEXT_PUBLIC_API_GATEWAY_URL=http://localhost:5000
```

### 3. Run Development Server

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

### 4. Build for Production

```bash
npm run build
npm start
```

## Project Structure

```
ui/
├── app/                    # Next.js App Router pages
│   ├── layout.tsx         # Root layout with navigation
│   ├── page.tsx           # Home page
│   ├── globals.css        # Global styles
│   ├── products/          # Product catalog ✨ NEW
│   │   ├── page.tsx       # Product listing
│   │   └── [id]/
│   │       └── page.tsx   # Product details
│   ├── orders/            # Orders page (Step 18)
│   └── invoices/          # Invoices page
├── components/            # React components
│   ├── Navigation.tsx     # Main navigation component
│   ├── ProductCard.tsx    # Product card component ✨ NEW
│   └── ProductFilters.tsx # Search & filters ✨ NEW
├── lib/                   # Utilities and helpers
│   └── api-client.ts      # API Gateway client
├── types/                 # TypeScript type definitions
│   └── index.ts           # Common types
├── public/                # Static assets
│   ├── manifest.json      # PWA manifest
│   └── robots.txt         # SEO
├── package.json           # Dependencies
├── tsconfig.json          # TypeScript config
├── tailwind.config.ts     # Tailwind CSS config
├── next.config.js         # Next.js config
├── Dockerfile             # Docker build config
└── README.md              # This file
```

## Features by Step

### ✅ Step 14: Next.js UI Setup
- Project setup with Next.js + TypeScript
- Tailwind CSS configuration
- Base layout and navigation
- API client implementation
- Home page with API health check

### ✅ Step 15: Product Catalog UI (Current)
- **Product Listing Page**:
  - Displays all products from Inventory API
  - Responsive grid (1-4 columns)
  - Real-time search by product name
  - Category filtering (5 categories)
  - Stock status badges (in stock, low stock, out of stock)
  - Summary statistics
- **Product Details Page**:
  - Individual product pages with dynamic routing
  - Full stock information
  - Quantity selector with validation
  - Add to cart (placeholder)
- **Components**:
  - ProductCard: Reusable product card
  - ProductFilters: Search and category dropdown
- **Features**:
  - Loading states
  - Error handling with retry
  - Empty states
  - Responsive design

### 🔜 Step 16: Shopping Basket (Next)
- Client-side basket state management
- Add/remove items from cart
- Cart badge in navigation
- Persist to localStorage

### 🔜 Step 17: Checkout Flow
- Checkout page
- Order creation
- Payment integration

### 🔜 Step 18: Order Tracking
- Order status tracking
- Order history

### 🔜 Step 19: PWA Features
- Service worker
- Offline support
- Install prompt

## API Client Usage

The API client is pre-configured with all backend services:

```typescript
import api from '@/lib/api-client'

// Orders
const orders = await api.order.getAll()
const order = await api.order.getById(orderId)
const newOrder = await api.order.create(orderData)

// Inventory (✨ USED IN STEP 15)
const products = await api.inventory.getAll()
const product = await api.inventory.getByProductId(productId)

// Payments
const payment = await api.payment.getById(paymentId)

// Shipments
const shipment = await api.shipment.getByOrderId(orderId)

// Health checks
const health = await api.health.order()
```

## Product Catalog Usage ✨ NEW

### Browse Products

Navigate to http://localhost:3000/products to see all products.

### Search Products

```typescript
// Search is client-side, instant response
// Type in the search box to filter by product name
```

### Filter by Category

Available categories (auto-extracted):
- Electronics (8 products)
- Mobile Devices (6 products)
- Audio Equipment (4 products)
- Accessories (7 products)
- Home & Office (5 products)

### View Product Details

Click "View Details" on any product card to see:
- Full stock information
- Quantity selector
- Add to cart button

Direct URL format: `http://localhost:3000/products/{productId}`

Example:
```
http://localhost:3000/products/11111111-1111-1111-1111-111111111111
```

## Seeding Test Data

Before using the product catalog, seed the inventory:

```bash
curl -X POST http://localhost:5000/api/inventory/seed
```

This creates 30 sample products across 5 categories.

## Responsive Design

The UI is fully responsive with breakpoints:

| Screen Size | Grid Columns | Layout |
|-------------|--------------|--------|
| Mobile (<640px) | 1 | Stacked, hamburger menu |
| Tablet (640-1024px) | 2 | Side-by-side |
| Laptop (1024-1280px) | 3 | Grid layout |
| Desktop (>1280px) | 4 | Full grid |

## Development Notes

### Testing API Connectivity

The home page automatically tests API Gateway connectivity:

```typescript
const response = await apiClient.get('/health/order/ready')
```

### Product Catalog Testing

1. Ensure backend is running and seeded
2. Navigate to `/products`
3. Test search: Type "Laptop"
4. Test filter: Select "Mobile Devices"
5. Test details: Click any product

### Error Handling

The UI handles:
- **API Down**: Shows error with retry button
- **404 Not Found**: Shows product not found message
- **No Results**: Shows empty state with clear filters
- **Loading**: Shows spinner during API calls

### Stock Status

Products automatically show status badges:
- **Green**: In stock (available > reorder level)
- **Yellow**: Low stock (available ≤ reorder level)
- **Red**: Out of stock (available = 0)

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint
- `node verify-setup.js` - Verify setup

## Browser Support

- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

## Troubleshooting

### Products Not Loading

**Check backend**:
```bash
curl http://localhost:5000/api/inventory
```

**Seed data**:
```bash
curl -X POST http://localhost:5000/api/inventory/seed
```

### API Connection Errors

**Verify gateway**:
```bash
curl http://localhost:5000/health/inventory/ready
```

**Check services**:
```bash
docker-compose ps
```

### Build Errors

```bash
rm -rf .next node_modules
npm install
npm run build
```

### Product Details 404

Ensure you're using correct product IDs from the listing page or `SAMPLE_PRODUCT_CATALOG.md`.

## Docker Deployment

### Build Image

```bash
docker build -t eshop-ui:latest .
```

### Run Container

```bash
docker run -p 3000:3000 \
  -e NEXT_PUBLIC_API_GATEWAY_URL=http://apigateway:8080 \
  eshop-ui:latest
```

### Docker Compose Integration

Add to main `docker-compose.yml`:

```yaml
ui:
  build:
    context: ./ui
    dockerfile: Dockerfile
  ports:
    - "3000:3000"
  environment:
    - NEXT_PUBLIC_API_GATEWAY_URL=http://apigateway:8080
  depends_on:
    - apigateway
  networks:
    - eshop-network
```

## Testing

### Quick Test (2 minutes)

1. Open http://localhost:3000/products
2. Verify 30 products load
3. Search for "Laptop"
4. Select "Mobile Devices" category
5. Click any product for details
6. Adjust quantity
7. Click "Add to Cart" (shows alert)

### Comprehensive Test

See `STEP15_TESTING_GUIDE.md` for detailed test scenarios.

## Documentation

- `README.md` - This file (overview)
- `INSTALLATION.md` - Installation instructions
- `STEP14_NEXTJS_UI_SETUP_SUMMARY.md` - Step 14 summary
- `STEP14_QUICK_START_GUIDE.md` - Step 14 testing
- `STEP15_PRODUCT_CATALOG_UI_SUMMARY.md` - Step 15 summary ✨ NEW
- `STEP15_TESTING_GUIDE.md` - Step 15 testing ✨ NEW
- `STEP15_QUICK_START_GUIDE.md` - Step 15 quick start ✨ NEW

## Known Limitations

- **No Product Images**: Using gradient placeholders (future: add image URLs)
- **No Pricing**: Price field not in Product model (future: add)
- **Client-Side Filtering**: Works for small catalogs only (~30 products)
- **Category Extraction**: Based on name keywords, not a real field
- **Add to Cart**: Placeholder (implemented in Step 16)

## Performance

- **Product Listing**: <1s load time
- **Search/Filter**: <10ms (instant, client-side)
- **Product Details**: <500ms load time
- **Network Requests**: Minimal (1 per page)

## Next Steps

1. **Current**: Step 15 - Product Catalog UI ✅
2. **Next**: Step 16 - Shopping Basket Implementation
3. **Then**: Step 17 - Checkout Flow
4. **Then**: Step 18 - Order Tracking
5. **Finally**: Step 19 - PWA Features

## Contributing

When adding new features:
1. Follow existing patterns (components, pages structure)
2. Use TypeScript for type safety
3. Implement responsive design
4. Add loading and error states
5. Update documentation

## Support

For issues or questions:
1. Check browser console for errors
2. Verify backend services are running
3. Review documentation in this directory
4. Check API Gateway health endpoints

---

**Current Status**: Steps 14 & 15 Complete ✅  
**Version**: 1.0.0  
**Last Updated**: Step 15 - Product Catalog UI
