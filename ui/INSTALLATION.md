# Installation Instructions

## Quick Start (5 minutes)

```bash
# 1. Navigate to UI directory
cd src/eShop/ui

# 2. Install dependencies
npm install

# 3. Create environment file
cp .env.local.example .env.local

# 4. Start development server
npm run dev

# 5. Open browser
# Visit: http://localhost:3000
```

## Prerequisites

- **Node.js 20+** and npm
- **Backend services running** (docker-compose)
- **API Gateway** accessible at http://localhost:5000

## Detailed Installation

### Step 1: Verify Prerequisites

Check Node.js version:
```bash
node --version
# Should show v20.x.x or higher
```

Check backend services:
```bash
cd ..
docker-compose ps
# All services should be "Up"
```

Test API Gateway:
```bash
curl http://localhost:5000/health/order/ready
# Should return: {"status":"Healthy"}
```

### Step 2: Install Dependencies

```bash
cd ui
npm install
```

This installs:
- Next.js 14.2.0
- React 18.3.0
- TypeScript 5.3.0
- Tailwind CSS 3.4.1
- Axios 1.6.0
- Lucide React 0.344.0

**Expected time**: 2-3 minutes on first install

### Step 3: Configure Environment

```bash
cp .env.local.example .env.local
```

The `.env.local` file should contain:
```env
NEXT_PUBLIC_API_GATEWAY_URL=http://localhost:5000
```

**For Docker deployment**, update to:
```env
NEXT_PUBLIC_API_GATEWAY_URL=http://apigateway:8080
```

### Step 4: Verify Setup

Run the verification script:
```bash
node verify-setup.js
```

This checks:
- ✓ All configuration files present
- ✓ All pages created
- ✓ Components exist
- ✓ API client configured
- ✓ Dependencies installed

### Step 5: Start Development Server

```bash
npm run dev
```

Expected output:
```
> eshop-ui@1.0.0 dev
> next dev

   ▲ Next.js 14.2.0
   - Local:        http://localhost:3000

 ✓ Ready in 3.2s
```

### Step 6: Test in Browser

Open http://localhost:3000

**Expected behavior**:
- ✓ Home page loads
- ✓ Navigation visible (eShop logo, menu items)
- ✓ API Gateway Status shows "Connected Successfully" (green)
- ✓ Features grid displays 4 cards
- ✓ Architecture info section at bottom

## Troubleshooting

### Issue: npm install fails

```bash
# Clear cache and retry
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
```

### Issue: Port 3000 already in use

```bash
# Use different port
PORT=3001 npm run dev
```

Or kill existing process:
```bash
# Windows PowerShell
netstat -ano | findstr :3000
# Note the PID, then:
taskkill /PID <PID> /F
```

### Issue: API connection error

**Check 1**: Backend running?
```bash
docker-compose ps
```

**Check 2**: API Gateway accessible?
```bash
curl http://localhost:5000/health/order/ready
```

**Check 3**: Environment variable set?
```bash
cat .env.local
# Should show: NEXT_PUBLIC_API_GATEWAY_URL=http://localhost:5000
```

**Check 4**: Restart dev server
```bash
# Stop with Ctrl+C, then:
npm run dev
```

### Issue: Blank page or errors

```bash
# Clean build
rm -rf .next
npm run dev
```

### Issue: TypeScript errors

```bash
# Check for type errors
npx tsc --noEmit
```

## Production Build

### Build for production:
```bash
npm run build
```

### Run production server:
```bash
npm start
```

### Verify build:
Open http://localhost:3000

Should see optimized production build.

## Docker Deployment

### Build image:
```bash
docker build -t eshop-ui:latest .
```

### Run container:
```bash
docker run -p 3000:3000 \
  -e NEXT_PUBLIC_API_GATEWAY_URL=http://host.docker.internal:5000 \
  eshop-ui:latest
```

**Note**: Use `host.docker.internal` on Windows/Mac to access host's localhost.

### Test:
```bash
curl http://localhost:3000
```

## Verification Checklist

Before marking installation complete:

- [ ] `npm install` completed without errors
- [ ] `node verify-setup.js` shows 100% success
- [ ] `.env.local` exists with correct API Gateway URL
- [ ] `npm run dev` starts successfully
- [ ] http://localhost:3000 loads
- [ ] Home page shows green API status
- [ ] Navigation menu works (desktop & mobile)
- [ ] All pages accessible (Products, Orders, Invoices)
- [ ] Console shows no critical errors
- [ ] Responsive design works (resize browser)

## Next Steps

After successful installation:

1. **Explore the UI**:
   - Home page: http://localhost:3000
   - Products: http://localhost:3000/products (placeholder)
   - Orders: http://localhost:3000/orders (placeholder)

2. **Review documentation**:
   - `README.md` - Full project documentation
   - `STEP14_QUICK_START_GUIDE.md` - Testing guide
   - `STEP14_NEXTJS_UI_SETUP_SUMMARY.md` - Implementation details

3. **Prepare for Step 15**:
   - Product catalog implementation
   - Real inventory data integration
   - Product cards and details

## Support

For issues:
1. Check this troubleshooting section
2. Review `README.md`
3. Check browser console for errors
4. Verify backend services are healthy
5. Review `STEP14_QUICK_START_GUIDE.md`

## File Structure

```
ui/
├── app/                       # Next.js pages
├── components/                # React components
├── lib/                       # Utilities (API client)
├── types/                     # TypeScript types
├── public/                    # Static assets
├── package.json              # Dependencies
├── tsconfig.json             # TypeScript config
├── tailwind.config.ts        # Tailwind config
├── next.config.js            # Next.js config
├── Dockerfile                # Docker build
├── README.md                 # Documentation
├── INSTALLATION.md           # This file
└── verify-setup.js           # Verification script
```

## Available Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Start development server |
| `npm run build` | Build for production |
| `npm start` | Run production server |
| `npm run lint` | Run ESLint |
| `node verify-setup.js` | Verify setup |

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `NEXT_PUBLIC_API_GATEWAY_URL` | `http://localhost:5000` | API Gateway URL |
| `PORT` | `3000` | Development server port |

## System Requirements

- **Node.js**: 20.x or higher
- **npm**: 9.x or higher
- **OS**: Windows 10+, macOS 10.15+, Linux
- **Browser**: Chrome, Firefox, Safari, Edge (latest)
- **RAM**: 4GB minimum, 8GB recommended
- **Disk**: 500MB for node_modules

## Success Indicators

✅ **Installation successful if**:
- No errors during `npm install`
- Dev server starts on http://localhost:3000
- Home page loads with green API status
- All navigation links work
- Console shows API requests succeeding

❌ **Installation needs attention if**:
- npm install shows errors
- Dev server won't start
- Blank page or React errors
- Red API status on home page
- 404 errors in console

---

**Installation complete?** Proceed to testing with `STEP14_QUICK_START_GUIDE.md`

