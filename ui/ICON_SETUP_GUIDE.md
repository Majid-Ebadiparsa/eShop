# PWA Icons Setup Guide

## Icon Requirements

The PWA requires icons in the following sizes:
- 72x72
- 96x96
- 128x128
- 144x144
- 152x152
- 192x192 (required for Android)
- 384x384
- 512x512 (required for Android)

## Quick Icon Generation

### Option 1: Using Online Tools (Recommended)

**PWA Asset Generator**:
1. Visit: https://www.pwabuilder.com/imageGenerator
2. Upload a source image (minimum 512x512, PNG with transparent background)
3. Download the generated icons
4. Extract to `public/icons/` directory

**RealFaviconGenerator**:
1. Visit: https://realfavicongenerator.net/
2. Upload your logo/icon
3. Configure PWA settings
4. Download and extract

### Option 2: Using ImageMagick (Command Line)

```bash
# Install ImageMagick first
# Windows: choco install imagemagick
# Mac: brew install imagemagick
# Linux: sudo apt-get install imagemagick

# Navigate to ui/public directory
cd public

# Create icons directory
mkdir icons

# Generate all sizes from a source image (source.png should be 1024x1024)
convert source.png -resize 72x72 icons/icon-72x72.png
convert source.png -resize 96x96 icons/icon-96x96.png
convert source.png -resize 128x128 icons/icon-128x128.png
convert source.png -resize 144x144 icons/icon-144x144.png
convert source.png -resize 152x152 icons/icon-152x152.png
convert source.png -resize 192x192 icons/icon-192x192.png
convert source.png -resize 384x384 icons/icon-384x384.png
convert source.png -resize 512x512 icons/icon-512x512.png
```

### Option 3: Using Node.js Script

Create `generate-icons.js` in `ui/` directory:

```javascript
const sharp = require('sharp')
const fs = require('fs')

const sizes = [72, 96, 128, 144, 152, 192, 384, 512]
const sourceImage = 'source.png'  // Your source image
const outputDir = 'public/icons'

// Create output directory
if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true })
}

// Generate icons
sizes.forEach(size => {
  sharp(sourceImage)
    .resize(size, size)
    .toFile(`${outputDir}/icon-${size}x${size}.png`)
    .then(() => console.log(`Generated ${size}x${size}`))
    .catch(err => console.error(`Error generating ${size}x${size}:`, err))
})
```

Install sharp and run:
```bash
npm install --save-dev sharp
node generate-icons.js
```

## Placeholder Icons (For Testing)

If you don't have icons ready, create simple colored placeholders:

### Using Canvas API (Browser Console)

```javascript
// Run this in browser console on any page
function generatePlaceholderIcon(size) {
  const canvas = document.createElement('canvas')
  canvas.width = size
  canvas.height = size
  const ctx = canvas.getContext('2d')
  
  // Gradient background
  const gradient = ctx.createLinearGradient(0, 0, size, size)
  gradient.addColorStop(0, '#2563eb')
  gradient.addColorStop(1, '#1d4ed8')
  ctx.fillStyle = gradient
  ctx.fillRect(0, 0, size, size)
  
  // Text
  ctx.fillStyle = 'white'
  ctx.font = `bold ${size/3}px Arial`
  ctx.textAlign = 'center'
  ctx.textBaseline = 'middle'
  ctx.fillText('🛍️', size/2, size/2)
  
  // Download
  canvas.toBlob(blob => {
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `icon-${size}x${size}.png`
    a.click()
  })
}

// Generate all sizes
[72, 96, 128, 144, 152, 192, 384, 512].forEach(generatePlaceholderIcon)
```

## Icon Best Practices

### Design Guidelines

1. **Simple & Recognizable**:
   - Clear at small sizes
   - Works in monochrome
   - Distinctive shape

2. **Safe Zone**:
   - Keep important content within 80% of icon area
   - Allows for rounded corners and masking

3. **Maskable Icons**:
   - Full bleed design
   - No transparency at edges
   - Content in safe zone (centered 80%)

4. **File Format**:
   - PNG with transparency
   - Optimized file size
   - sRGB color space

### Testing Icons

1. **Lighthouse**:
   ```bash
   # Run Lighthouse audit
   npm install -g lighthouse
   lighthouse http://localhost:3000 --view
   ```

2. **Maskable.app**:
   - Visit: https://maskable.app/
   - Upload your 512x512 icon
   - Check how it looks with different masks

3. **Browser DevTools**:
   - Chrome DevTools → Application → Manifest
   - Verify all icons load correctly

## File Structure

After generating icons:

```
public/
├── icons/
│   ├── icon-72x72.png
│   ├── icon-96x96.png
│   ├── icon-128x128.png
│   ├── icon-144x144.png
│   ├── icon-152x152.png
│   ├── icon-192x192.png
│   ├── icon-384x384.png
│   └── icon-512x512.png
├── screenshots/
│   ├── desktop-1.png (optional, 1280x720)
│   └── mobile-1.png (optional, 750x1334)
├── manifest.json
├── sw.js
└── offline.html
```

## Screenshots (Optional)

For enhanced PWA listing:

1. **Desktop Screenshot** (1280x720):
   - Capture product catalog page
   - Save as `public/screenshots/desktop-1.png`

2. **Mobile Screenshot** (750x1334):
   - Capture shopping cart
   - Save as `public/screenshots/mobile-1.png`

Use browser DevTools device mode to capture at exact dimensions.

## Verification

After adding icons:

1. **Check Manifest**:
   - Visit: http://localhost:3000/manifest.json
   - Verify all icon paths are accessible

2. **Test Icon Loading**:
   - Visit: http://localhost:3000/icons/icon-192x192.png
   - Should display the icon

3. **PWA Audit**:
   - DevTools → Lighthouse → PWA
   - Should show icons properly configured

## Troubleshooting

### Icons Not Loading

**Check paths**:
```bash
# Icons should be in:
ui/public/icons/icon-192x192.png

# Accessible at:
http://localhost:3000/icons/icon-192x192.png
```

### Icons Look Blurry

- Use source image at least 1024x1024
- Don't upscale smaller images
- Use PNG format, not JPEG

### Icons Don't Show in Install Prompt

- Ensure 192x192 and 512x512 exist
- Check manifest.json syntax
- Clear browser cache and service worker

## Quick Test Icons

For immediate testing, download free icon packs:

- **Material Design Icons**: https://fonts.google.com/icons
- **Iconify**: https://icon-sets.iconify.design/
- **Flaticon**: https://www.flaticon.com/ (check license)

Choose a shopping cart or bag icon in blue (#2563eb).

---

**Note**: The app will function without custom icons, but browsers will use default icons for PWA installation. Add proper icons before production deployment.

