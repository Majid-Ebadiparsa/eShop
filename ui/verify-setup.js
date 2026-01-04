#!/usr/bin/env node

/**
 * Verification script for Step 14: Next.js UI Setup
 * Run with: node verify-setup.js
 */

const fs = require('fs');
const path = require('path');

const colors = {
  green: '\x1b[32m',
  red: '\x1b[31m',
  yellow: '\x1b[33m',
  reset: '\x1b[0m',
  blue: '\x1b[34m'
};

function checkFile(filePath, description) {
  const exists = fs.existsSync(filePath);
  const status = exists ? `${colors.green}✓${colors.reset}` : `${colors.red}✗${colors.reset}`;
  console.log(`${status} ${description}: ${filePath}`);
  return exists;
}

function checkDirectory(dirPath, description) {
  const exists = fs.existsSync(dirPath) && fs.statSync(dirPath).isDirectory();
  const status = exists ? `${colors.green}✓${colors.reset}` : `${colors.red}✗${colors.reset}`;
  console.log(`${status} ${description}: ${dirPath}`);
  return exists;
}

console.log(`${colors.blue}======================================${colors.reset}`);
console.log(`${colors.blue}Step 14: Next.js UI Setup Verification${colors.reset}`);
console.log(`${colors.blue}======================================${colors.reset}\n`);

let totalChecks = 0;
let passedChecks = 0;

// Configuration Files
console.log(`${colors.yellow}Configuration Files:${colors.reset}`);
totalChecks++; if (checkFile('package.json', 'package.json')) passedChecks++;
totalChecks++; if (checkFile('tsconfig.json', 'tsconfig.json')) passedChecks++;
totalChecks++; if (checkFile('next.config.js', 'next.config.js')) passedChecks++;
totalChecks++; if (checkFile('tailwind.config.ts', 'tailwind.config.ts')) passedChecks++;
totalChecks++; if (checkFile('postcss.config.js', 'postcss.config.js')) passedChecks++;
totalChecks++; if (checkFile('.env.local.example', '.env.local.example')) passedChecks++;
totalChecks++; if (checkFile('.gitignore', '.gitignore')) passedChecks++;
console.log();

// App Directory
console.log(`${colors.yellow}App Directory (Pages):${colors.reset}`);
totalChecks++; if (checkDirectory('app', 'app directory')) passedChecks++;
totalChecks++; if (checkFile('app/layout.tsx', 'Root layout')) passedChecks++;
totalChecks++; if (checkFile('app/page.tsx', 'Home page')) passedChecks++;
totalChecks++; if (checkFile('app/globals.css', 'Global styles')) passedChecks++;
totalChecks++; if (checkFile('app/products/page.tsx', 'Products page')) passedChecks++;
totalChecks++; if (checkFile('app/orders/page.tsx', 'Orders page')) passedChecks++;
totalChecks++; if (checkFile('app/invoices/page.tsx', 'Invoices page')) passedChecks++;
console.log();

// Components
console.log(`${colors.yellow}Components:${colors.reset}`);
totalChecks++; if (checkDirectory('components', 'components directory')) passedChecks++;
totalChecks++; if (checkFile('components/Navigation.tsx', 'Navigation component')) passedChecks++;
console.log();

// Libraries
console.log(`${colors.yellow}Libraries:${colors.reset}`);
totalChecks++; if (checkDirectory('lib', 'lib directory')) passedChecks++;
totalChecks++; if (checkFile('lib/api-client.ts', 'API client')) passedChecks++;
console.log();

// Types
console.log(`${colors.yellow}TypeScript Types:${colors.reset}`);
totalChecks++; if (checkDirectory('types', 'types directory')) passedChecks++;
totalChecks++; if (checkFile('types/index.ts', 'Type definitions')) passedChecks++;
console.log();

// Public Assets
console.log(`${colors.yellow}Public Assets:${colors.reset}`);
totalChecks++; if (checkDirectory('public', 'public directory')) passedChecks++;
totalChecks++; if (checkFile('public/manifest.json', 'PWA manifest')) passedChecks++;
totalChecks++; if (checkFile('public/robots.txt', 'robots.txt')) passedChecks++;
console.log();

// Docker
console.log(`${colors.yellow}Docker:${colors.reset}`);
totalChecks++; if (checkFile('Dockerfile', 'Dockerfile')) passedChecks++;
totalChecks++; if (checkFile('.dockerignore', '.dockerignore')) passedChecks++;
console.log();

// Documentation
console.log(`${colors.yellow}Documentation:${colors.reset}`);
totalChecks++; if (checkFile('README.md', 'README')) passedChecks++;
console.log();

// Check package.json content
console.log(`${colors.yellow}Dependency Check:${colors.reset}`);
try {
  const packageJson = JSON.parse(fs.readFileSync('package.json', 'utf8'));
  const requiredDeps = ['next', 'react', 'react-dom', 'axios', 'lucide-react'];
  const requiredDevDeps = ['typescript', 'tailwindcss', '@types/react'];
  
  let depsOk = true;
  requiredDeps.forEach(dep => {
    const exists = packageJson.dependencies && packageJson.dependencies[dep];
    if (exists) {
      console.log(`${colors.green}✓${colors.reset} Dependency: ${dep}`);
      totalChecks++; passedChecks++;
    } else {
      console.log(`${colors.red}✗${colors.reset} Missing dependency: ${dep}`);
      totalChecks++;
      depsOk = false;
    }
  });
  
  requiredDevDeps.forEach(dep => {
    const exists = packageJson.devDependencies && packageJson.devDependencies[dep];
    if (exists) {
      console.log(`${colors.green}✓${colors.reset} Dev dependency: ${dep}`);
      totalChecks++; passedChecks++;
    } else {
      console.log(`${colors.red}✗${colors.reset} Missing dev dependency: ${dep}`);
      totalChecks++;
      depsOk = false;
    }
  });
} catch (err) {
  console.log(`${colors.red}✗${colors.reset} Error reading package.json: ${err.message}`);
}
console.log();

// Environment check
console.log(`${colors.yellow}Environment:${colors.reset}`);
const envExists = fs.existsSync('.env.local');
if (envExists) {
  console.log(`${colors.green}✓${colors.reset} .env.local exists`);
  totalChecks++; passedChecks++;
} else {
  console.log(`${colors.yellow}⚠${colors.reset} .env.local not found (copy from .env.local.example)`);
  totalChecks++;
}
console.log();

// Node modules check
console.log(`${colors.yellow}Installation:${colors.reset}`);
const nodeModulesExists = fs.existsSync('node_modules');
if (nodeModulesExists) {
  console.log(`${colors.green}✓${colors.reset} node_modules exists (dependencies installed)`);
  totalChecks++; passedChecks++;
} else {
  console.log(`${colors.yellow}⚠${colors.reset} node_modules not found (run: npm install)`);
  totalChecks++;
}
console.log();

// Summary
console.log(`${colors.blue}======================================${colors.reset}`);
console.log(`${colors.blue}Summary${colors.reset}`);
console.log(`${colors.blue}======================================${colors.reset}`);
console.log(`Total checks: ${totalChecks}`);
console.log(`Passed: ${colors.green}${passedChecks}${colors.reset}`);
console.log(`Failed: ${colors.red}${totalChecks - passedChecks}${colors.reset}`);

const percentage = ((passedChecks / totalChecks) * 100).toFixed(1);
console.log(`Success rate: ${percentage}%\n`);

if (passedChecks === totalChecks) {
  console.log(`${colors.green}✓ All checks passed! Setup is complete.${colors.reset}`);
  console.log(`\nNext steps:`);
  console.log(`  1. Copy .env.local.example to .env.local (if not done)`);
  console.log(`  2. Run: npm install`);
  console.log(`  3. Run: npm run dev`);
  console.log(`  4. Open: http://localhost:3000\n`);
  process.exit(0);
} else {
  console.log(`${colors.red}✗ Some checks failed. Please review the output above.${colors.reset}\n`);
  process.exit(1);
}

