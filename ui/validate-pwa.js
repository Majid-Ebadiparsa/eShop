#!/usr/bin/env node

/**
 * PWA Setup Validation Script
 * Verifies that all PWA components are correctly configured
 */

const fs = require('fs')
const path = require('path')

const ROOT = path.join(__dirname)
const PUBLIC = path.join(ROOT, 'public')

let errors = 0
let warnings = 0
let passed = 0

function check(name, condition, level = 'error') {
  if (condition) {
    console.log(`✅ ${name}`)
    passed++
  } else {
    if (level === 'error') {
      console.error(`❌ ${name}`)
      errors++
    } else {
      console.warn(`⚠️  ${name}`)
      warnings++
    }
  }
}

console.log('\n🔍 Validating PWA Setup...\n')

// Check manifest.json
console.log('📄 Checking manifest.json...')
const manifestPath = path.join(PUBLIC, 'manifest.json')
let manifest
try {
  manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'))
  check('manifest.json exists and is valid JSON', true)
  check('manifest has name', manifest.name && manifest.name.length > 0)
  check('manifest has short_name', manifest.short_name && manifest.short_name.length > 0)
  check('manifest has start_url', manifest.start_url)
  check('manifest has display mode', manifest.display)
  check('manifest has theme_color', manifest.theme_color)
  check('manifest has icons array', Array.isArray(manifest.icons))
  
  if (manifest.icons) {
    check('manifest has 192x192 icon', manifest.icons.some(i => i.sizes === '192x192'))
    check('manifest has 512x512 icon', manifest.icons.some(i => i.sizes === '512x512'))
    check('manifest has at least 2 icons', manifest.icons.length >= 2)
    check('manifest has maskable icon', manifest.icons.some(i => i.purpose && i.purpose.includes('maskable')), 'warning')
  }
  
  check('manifest has shortcuts', Array.isArray(manifest.shortcuts) && manifest.shortcuts.length > 0, 'warning')
} catch (e) {
  check('manifest.json exists and is valid JSON', false)
}

// Check service worker
console.log('\n🔧 Checking service worker...')
const swPath = path.join(PUBLIC, 'sw.js')
try {
  const sw = fs.readFileSync(swPath, 'utf8')
  check('sw.js exists', true)
  check('sw.js has install event', sw.includes("addEventListener('install'"))
  check('sw.js has activate event', sw.includes("addEventListener('activate'"))
  check('sw.js has fetch event', sw.includes("addEventListener('fetch'"))
  check('sw.js has cache name', sw.includes('CACHE_NAME'))
  check('sw.js has offline URL', sw.includes('OFFLINE_URL') || sw.includes('/offline.html'))
  check('sw.js has cache.addAll', sw.includes('cache.addAll') || sw.includes('cache.add'))
} catch (e) {
  check('sw.js exists', false)
}

// Check offline page
console.log('\n📱 Checking offline page...')
const offlinePath = path.join(PUBLIC, 'offline.html')
try {
  const offline = fs.readFileSync(offlinePath, 'utf8')
  check('offline.html exists', true)
  check('offline.html has DOCTYPE', offline.includes('<!DOCTYPE html>'))
  check('offline.html has title', offline.includes('<title>'))
  check('offline.html is standalone', !offline.includes('_next/') && !offline.includes('import '))
} catch (e) {
  check('offline.html exists', false)
}

// Check PWA component
console.log('\n⚛️  Checking PWA component...')
const pwaComponentPath = path.join(ROOT, 'components', 'PWAInstallPrompt.tsx')
try {
  const component = fs.readFileSync(pwaComponentPath, 'utf8')
  check('PWAInstallPrompt.tsx exists', true)
  check('PWAInstallPrompt registers service worker', component.includes('serviceWorker') && component.includes('.register'))
  check('PWAInstallPrompt handles beforeinstallprompt', component.includes('beforeinstallprompt'))
  check('PWAInstallPrompt is client component', component.includes("'use client'"))
} catch (e) {
  check('PWAInstallPrompt.tsx exists', false)
}

// Check layout integration
console.log('\n🎨 Checking layout integration...')
const layoutPath = path.join(ROOT, 'app', 'layout.tsx')
try {
  const layout = fs.readFileSync(layoutPath, 'utf8')
  check('layout.tsx has PWAInstallPrompt import', layout.includes('PWAInstallPrompt'))
  check('layout.tsx renders PWAInstallPrompt', layout.includes('<PWAInstallPrompt'))
  check('layout.tsx has manifest in metadata', layout.includes("manifest: '/manifest.json'") || layout.includes('manifest:'))
  check('layout.tsx has themeColor', layout.includes('themeColor'))
  check('layout.tsx has appleWebApp metadata', layout.includes('appleWebApp'), 'warning')
} catch (e) {
  check('layout.tsx integration', false)
}

// Check icon directory
console.log('\n🎨 Checking icons...')
const iconsPath = path.join(PUBLIC, 'icons')
if (fs.existsSync(iconsPath)) {
  const icons = fs.readdirSync(iconsPath).filter(f => f.endsWith('.png'))
  check('icons directory exists', true)
  check('has 192x192 icon', icons.some(i => i.includes('192x192')), 'warning')
  check('has 512x512 icon', icons.some(i => i.includes('512x512')), 'warning')
  check('has at least 2 icon sizes', icons.length >= 2, 'warning')
} else {
  check('icons directory exists', false, 'warning')
  console.log('   ℹ️  See ICON_SETUP_GUIDE.md to generate icons')
}

// Check documentation
console.log('\n📚 Checking documentation...')
const docsToCheck = [
  'ICON_SETUP_GUIDE.md',
  '../STEP19_PWA_SETUP_SUMMARY.md',
  '../STEP19_QUICK_START_GUIDE.md'
]

docsToCheck.forEach(doc => {
  const docPath = path.join(ROOT, doc)
  check(`${path.basename(doc)} exists`, fs.existsSync(docPath), 'warning')
})

// Summary
console.log('\n' + '='.repeat(50))
console.log('📊 Summary:')
console.log('='.repeat(50))
console.log(`✅ Passed:   ${passed}`)
console.log(`⚠️  Warnings: ${warnings}`)
console.log(`❌ Errors:   ${errors}`)
console.log('='.repeat(50))

if (errors === 0) {
  console.log('\n✨ PWA setup is valid! Ready for testing.')
  if (warnings > 0) {
    console.log(`\n⚠️  ${warnings} warning(s) found. These are optional but recommended.`)
  }
  console.log('\n📖 Next Steps:')
  console.log('   1. Generate icons: See ICON_SETUP_GUIDE.md')
  console.log('   2. Test PWA: npm run dev')
  console.log('   3. Run Lighthouse audit')
  console.log('   4. Test installation on desktop/mobile')
  console.log('   5. Test offline functionality\n')
  process.exit(0)
} else {
  console.log(`\n❌ ${errors} critical error(s) found. Please fix before proceeding.\n`)
  process.exit(1)
}

