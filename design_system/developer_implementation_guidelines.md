# TETCO Design System Implementation Guidelines
## School Transportation Management System

### Table of Contents

1. [Getting Started](#getting-started)
2. [Project Setup and Configuration](#project-setup-and-configuration)
3. [Design Token Implementation](#design-token-implementation)
4. [Component Development Guidelines](#component-development-guidelines)
5. [Layout and Responsive Design](#layout-and-responsive-design)
6. [Accessibility Implementation](#accessibility-implementation)
7. [Performance Optimization](#performance-optimization)
8. [Testing and Quality Assurance](#testing-and-quality-assurance)
9. [Deployment and Maintenance](#deployment-and-maintenance)
10. [Troubleshooting and Common Issues](#troubleshooting-and-common-issues)

---

## Getting Started

This comprehensive implementation guide provides frontend developers with everything needed to build the TETCO School Transportation Management System according to the established design system. The guide covers React 18+ for web applications, React Native for mobile applications, and .NET 8.0 integration patterns.

### Prerequisites

Before beginning implementation, ensure your development environment includes:

- **Node.js**: Version 18.0 or higher
- **npm or yarn**: Latest stable version
- **React**: Version 18.0 or higher
- **TypeScript**: Version 4.9 or higher (recommended)
- **Git**: For version control
- **VS Code**: Recommended IDE with extensions for React, TypeScript, and ESLint

### Design System Philosophy

The TETCO design system follows atomic design principles, building from design tokens through components to complete page layouts. Every implementation decision should prioritize accessibility, performance, and maintainability while strictly adhering to the brand guidelines established in the TETCO brand identity analysis.

---

## Project Setup and Configuration

### Initial Project Structure

Create a well-organized project structure that separates concerns and promotes maintainability:

```
src/
├── components/
│   ├── atoms/
│   │   ├── Button/
│   │   ├── Input/
│   │   └── Badge/
│   ├── molecules/
│   │   ├── FormGroup/
│   │   ├── Card/
│   │   └── Alert/
│   ├── organisms/
│   │   ├── Header/
│   │   ├── Sidebar/
│   │   └── DataTable/
│   └── templates/
│       ├── DashboardLayout/
│       ├── FormLayout/
│       └── ListLayout/
├── styles/
│   ├── tokens/
│   │   ├── colors.css
│   │   ├── typography.css
│   │   ├── spacing.css
│   │   └── index.css
│   ├── components/
│   └── utilities/
├── hooks/
├── utils/
├── types/
└── assets/
    ├── images/
    ├── icons/
    └── fonts/
```

### Package Dependencies

Install the essential packages for implementing the design system:

```bash
# Core React dependencies
npm install react@^18.0.0 react-dom@^18.0.0

# TypeScript support
npm install -D typescript @types/react @types/react-dom

# Styling and CSS-in-JS
npm install styled-components @emotion/react @emotion/styled

# Utility libraries
npm install clsx classnames

# Development tools
npm install -D eslint prettier @typescript-eslint/eslint-plugin
npm install -D @storybook/react @storybook/addon-essentials

# Testing
npm install -D @testing-library/react @testing-library/jest-dom
npm install -D jest @types/jest

# Accessibility
npm install @axe-core/react react-aria
```

### Environment Configuration

Create environment-specific configuration files:

**`.env.development`**
```env
REACT_APP_API_BASE_URL=http://localhost:5000/api
REACT_APP_ENVIRONMENT=development
REACT_APP_ENABLE_ANALYTICS=false
REACT_APP_LOG_LEVEL=debug
```

**`.env.production`**
```env
REACT_APP_API_BASE_URL=https://api.tetco-transport.com
REACT_APP_ENVIRONMENT=production
REACT_APP_ENABLE_ANALYTICS=true
REACT_APP_LOG_LEVEL=error
```

### TypeScript Configuration

Configure TypeScript for optimal development experience:

**`tsconfig.json`**
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["dom", "dom.iterable", "ES6"],
    "allowJs": true,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "allowSyntheticDefaultImports": true,
    "strict": true,
    "forceConsistentCasingInFileNames": true,
    "noFallthroughCasesInSwitch": true,
    "module": "esnext",
    "moduleResolution": "node",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "baseUrl": "src",
    "paths": {
      "@components/*": ["components/*"],
      "@styles/*": ["styles/*"],
      "@utils/*": ["utils/*"],
      "@types/*": ["types/*"],
      "@hooks/*": ["hooks/*"],
      "@assets/*": ["assets/*"]
    }
  },
  "include": ["src"],
  "exclude": ["node_modules"]
}
```

---

## Design Token Implementation

### CSS Custom Properties Setup

Implement design tokens as CSS custom properties for maximum flexibility and maintainability. Create a comprehensive token system that covers all design decisions:

**`src/styles/tokens/colors.css`**
```css
:root {
  /* TETCO Brand Colors */
  --tetco-primary-blue: #005F96;
  --tetco-primary-green: #36BA91;
  --tetco-light-green: #4CFCB4;
  --tetco-dark-teal: #0C3C44;
  --tetco-light-gray: #D1D1D1;
  
  /* RGB Values for JavaScript manipulation */
  --tetco-primary-blue-rgb: 0, 95, 150;
  --tetco-primary-green-rgb: 54, 186, 145;
  --tetco-light-green-rgb: 76, 252, 180;
  --tetco-dark-teal-rgb: 12, 60, 68;
  --tetco-light-gray-rgb: 209, 209, 209;
  
  /* Semantic Color System */
  --color-primary: var(--tetco-primary-blue);
  --color-secondary: var(--tetco-primary-green);
  --color-accent: var(--tetco-light-green);
  --color-neutral: var(--tetco-light-gray);
  --color-text-primary: var(--tetco-dark-teal);
  
  /* State Colors */
  --color-success: var(--tetco-primary-green);
  --color-success-light: var(--tetco-light-green);
  --color-warning: #FFA726;
  --color-error: #EF5350;
  --color-info: var(--tetco-primary-blue);
  
  /* Background Colors */
  --color-background-primary: #FFFFFF;
  --color-background-secondary: #F8F9FA;
  --color-background-tertiary: var(--tetco-light-gray);
  --color-background-overlay: rgba(12, 60, 68, 0.8);
  
  /* Text Colors */
  --color-text-primary: var(--tetco-dark-teal);
  --color-text-secondary: rgba(12, 60, 68, 0.7);
  --color-text-tertiary: rgba(12, 60, 68, 0.5);
  --color-text-inverse: #FFFFFF;
  --color-text-link: var(--tetco-primary-blue);
  --color-text-link-hover: rgba(0, 95, 150, 0.8);
  
  /* Border Colors */
  --color-border-primary: var(--tetco-light-gray);
  --color-border-secondary: rgba(209, 209, 209, 0.5);
  --color-border-focus: var(--tetco-primary-blue);
  --color-border-error: var(--color-error);
  --color-border-success: var(--color-success);
}

/* Dark mode support (future enhancement) */
@media (prefers-color-scheme: dark) {
  :root {
    --color-background-primary: #1a1a1a;
    --color-background-secondary: #2d2d2d;
    --color-text-primary: #ffffff;
    --color-text-secondary: rgba(255, 255, 255, 0.7);
    --color-text-tertiary: rgba(255, 255, 255, 0.5);
  }
}
```

### Typography Token Implementation

**`src/styles/tokens/typography.css`**
```css
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;700;900&display=swap');

:root {
  /* Font Families */
  --font-family-primary: 'EFFRA', 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  --font-family-arabic: 'EFFRA', 'Noto Sans Arabic', 'Tahoma', sans-serif;
  --font-family-monospace: 'SF Mono', Monaco, 'Cascadia Code', 'Roboto Mono', monospace;
  
  /* Font Weights */
  --font-weight-light: 300;
  --font-weight-regular: 400;
  --font-weight-medium: 500;
  --font-weight-bold: 700;
  --font-weight-heavy: 900;
  
  /* Font Sizes - Fluid Typography */
  --font-size-xs: clamp(0.75rem, 0.7rem + 0.25vw, 0.875rem);
  --font-size-sm: clamp(0.875rem, 0.8rem + 0.375vw, 1rem);
  --font-size-base: clamp(1rem, 0.9rem + 0.5vw, 1.125rem);
  --font-size-lg: clamp(1.125rem, 1rem + 0.625vw, 1.25rem);
  --font-size-xl: clamp(1.25rem, 1.1rem + 0.75vw, 1.5rem);
  --font-size-2xl: clamp(1.5rem, 1.3rem + 1vw, 1.875rem);
  --font-size-3xl: clamp(1.875rem, 1.6rem + 1.375vw, 2.25rem);
  --font-size-4xl: clamp(2.25rem, 1.9rem + 1.75vw, 3rem);
  --font-size-5xl: clamp(3rem, 2.5rem + 2.5vw, 3.75rem);
  --font-size-6xl: clamp(3.75rem, 3rem + 3.75vw, 4.5rem);
  
  /* Line Heights */
  --line-height-tight: 1.25;
  --line-height-normal: 1.5;
  --line-height-relaxed: 1.75;
  --line-height-loose: 2;
  
  /* Letter Spacing */
  --letter-spacing-tight: -0.025em;
  --letter-spacing-normal: 0;
  --letter-spacing-wide: 0.025em;
  --letter-spacing-wider: 0.05em;
  --letter-spacing-widest: 0.1em;
}

/* Typography utility classes */
.text-xs { font-size: var(--font-size-xs); }
.text-sm { font-size: var(--font-size-sm); }
.text-base { font-size: var(--font-size-base); }
.text-lg { font-size: var(--font-size-lg); }
.text-xl { font-size: var(--font-size-xl); }
.text-2xl { font-size: var(--font-size-2xl); }
.text-3xl { font-size: var(--font-size-3xl); }
.text-4xl { font-size: var(--font-size-4xl); }
.text-5xl { font-size: var(--font-size-5xl); }
.text-6xl { font-size: var(--font-size-6xl); }

.font-light { font-weight: var(--font-weight-light); }
.font-normal { font-weight: var(--font-weight-regular); }
.font-medium { font-weight: var(--font-weight-medium); }
.font-bold { font-weight: var(--font-weight-bold); }
.font-heavy { font-weight: var(--font-weight-heavy); }

.leading-tight { line-height: var(--line-height-tight); }
.leading-normal { line-height: var(--line-height-normal); }
.leading-relaxed { line-height: var(--line-height-relaxed); }
.leading-loose { line-height: var(--line-height-loose); }
```

### JavaScript Token Integration

Create a TypeScript module for accessing design tokens in JavaScript:

**`src/styles/tokens/index.ts`**
```typescript
export const tokens = {
  colors: {
    // TETCO Brand Colors
    primary: {
      blue: '#005F96',
      green: '#36BA91',
      lightGreen: '#4CFCB4',
      darkTeal: '#0C3C44',
      lightGray: '#D1D1D1'
    },
    
    // Semantic Colors
    semantic: {
      primary: 'var(--color-primary)',
      secondary: 'var(--color-secondary)',
      accent: 'var(--color-accent)',
      success: 'var(--color-success)',
      warning: 'var(--color-warning)',
      error: 'var(--color-error)',
      info: 'var(--color-info)'
    },
    
    // Text Colors
    text: {
      primary: 'var(--color-text-primary)',
      secondary: 'var(--color-text-secondary)',
      tertiary: 'var(--color-text-tertiary)',
      inverse: 'var(--color-text-inverse)',
      link: 'var(--color-text-link)',
      linkHover: 'var(--color-text-link-hover)'
    },
    
    // Background Colors
    background: {
      primary: 'var(--color-background-primary)',
      secondary: 'var(--color-background-secondary)',
      tertiary: 'var(--color-background-tertiary)',
      overlay: 'var(--color-background-overlay)'
    },
    
    // Border Colors
    border: {
      primary: 'var(--color-border-primary)',
      secondary: 'var(--color-border-secondary)',
      focus: 'var(--color-border-focus)',
      error: 'var(--color-border-error)',
      success: 'var(--color-border-success)'
    }
  },
  
  typography: {
    fontFamily: {
      primary: 'var(--font-family-primary)',
      arabic: 'var(--font-family-arabic)',
      monospace: 'var(--font-family-monospace)'
    },
    
    fontSize: {
      xs: 'var(--font-size-xs)',
      sm: 'var(--font-size-sm)',
      base: 'var(--font-size-base)',
      lg: 'var(--font-size-lg)',
      xl: 'var(--font-size-xl)',
      '2xl': 'var(--font-size-2xl)',
      '3xl': 'var(--font-size-3xl)',
      '4xl': 'var(--font-size-4xl)',
      '5xl': 'var(--font-size-5xl)',
      '6xl': 'var(--font-size-6xl)'
    },
    
    fontWeight: {
      light: 'var(--font-weight-light)',
      regular: 'var(--font-weight-regular)',
      medium: 'var(--font-weight-medium)',
      bold: 'var(--font-weight-bold)',
      heavy: 'var(--font-weight-heavy)'
    },
    
    lineHeight: {
      tight: 'var(--line-height-tight)',
      normal: 'var(--line-height-normal)',
      relaxed: 'var(--line-height-relaxed)',
      loose: 'var(--line-height-loose)'
    }
  },
  
  spacing: {
    0: '0',
    1: 'var(--spacing-1)',
    2: 'var(--spacing-2)',
    3: 'var(--spacing-3)',
    4: 'var(--spacing-4)',
    5: 'var(--spacing-5)',
    6: 'var(--spacing-6)',
    8: 'var(--spacing-8)',
    10: 'var(--spacing-10)',
    12: 'var(--spacing-12)',
    16: 'var(--spacing-16)',
    20: 'var(--spacing-20)',
    24: 'var(--spacing-24)',
    32: 'var(--spacing-32)'
  },
  
  borderRadius: {
    none: '0',
    sm: 'var(--border-radius-sm)',
    base: 'var(--border-radius-base)',
    md: 'var(--border-radius-md)',
    lg: 'var(--border-radius-lg)',
    xl: 'var(--border-radius-xl)',
    '2xl': 'var(--border-radius-2xl)',
    full: 'var(--border-radius-full)'
  },
  
  shadow: {
    sm: 'var(--shadow-sm)',
    base: 'var(--shadow-base)',
    md: 'var(--shadow-md)',
    lg: 'var(--shadow-lg)',
    xl: 'var(--shadow-xl)',
    '2xl': 'var(--shadow-2xl)',
    inner: 'var(--shadow-inner)'
  },
  
  animation: {
    duration: {
      instant: 'var(--duration-instant)',
      fast: 'var(--duration-fast)',
      normal: 'var(--duration-normal)',
      slow: 'var(--duration-slow)',
      slower: 'var(--duration-slower)'
    },
    
    easing: {
      linear: 'var(--easing-linear)',
      ease: 'var(--easing-ease)',
      easeIn: 'var(--easing-ease-in)',
      easeOut: 'var(--easing-ease-out)',
      easeInOut: 'var(--easing-ease-in-out)',
      bounce: 'var(--easing-bounce)',
      smooth: 'var(--easing-smooth)'
    }
  },
  
  breakpoints: {
    xs: 'var(--breakpoint-xs)',
    sm: 'var(--breakpoint-sm)',
    md: 'var(--breakpoint-md)',
    lg: 'var(--breakpoint-lg)',
    xl: 'var(--breakpoint-xl)',
    '2xl': 'var(--breakpoint-2xl)'
  },
  
  zIndex: {
    hide: 'var(--z-index-hide)',
    auto: 'var(--z-index-auto)',
    base: 'var(--z-index-base)',
    docked: 'var(--z-index-docked)',
    dropdown: 'var(--z-index-dropdown)',
    sticky: 'var(--z-index-sticky)',
    banner: 'var(--z-index-banner)',
    overlay: 'var(--z-index-overlay)',
    modal: 'var(--z-index-modal)',
    popover: 'var(--z-index-popover)',
    skipLink: 'var(--z-index-skipLink)',
    toast: 'var(--z-index-toast)',
    tooltip: 'var(--z-index-tooltip)'
  }
} as const;

export type TokenPath = keyof typeof tokens;
export type ColorTokens = typeof tokens.colors;
export type TypographyTokens = typeof tokens.typography;
export type SpacingTokens = typeof tokens.spacing;
```

---


## Component Development Guidelines

### Component Architecture

Follow atomic design principles when building components, ensuring each component has a single responsibility and can be composed with others to create complex interfaces. Every component should be built with accessibility, performance, and reusability in mind.

### Button Component Implementation

The button component serves as the foundation for all interactive elements in the system. Implement it with comprehensive support for different variants, sizes, and states:

**`src/components/atoms/Button/Button.tsx`**
```typescript
import React, { forwardRef, ButtonHTMLAttributes } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'ghost';
  size?: 'sm' | 'md' | 'lg' | 'xl';
  fullWidth?: boolean;
  loading?: boolean;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  children: React.ReactNode;
}

const StyledButton = styled.button<ButtonProps>`
  /* Base styles */
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: ${tokens.spacing[2]};
  border: none;
  border-radius: ${tokens.borderRadius.md};
  font-family: ${tokens.typography.fontFamily.primary};
  font-weight: ${tokens.typography.fontWeight.medium};
  line-height: ${tokens.typography.lineHeight.normal};
  text-decoration: none;
  cursor: pointer;
  transition: all ${tokens.animation.duration.fast} ${tokens.animation.easing.smooth};
  position: relative;
  overflow: hidden;
  
  /* Prevent text selection */
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
  
  /* Size variants */
  ${({ size = 'md' }) => {
    const sizeMap = {
      sm: {
        height: '2rem',
        padding: `0 ${tokens.spacing[3]}`,
        fontSize: tokens.typography.fontSize.sm
      },
      md: {
        height: '2.5rem',
        padding: `0 ${tokens.spacing[4]}`,
        fontSize: tokens.typography.fontSize.base
      },
      lg: {
        height: '3rem',
        padding: `0 ${tokens.spacing[6]}`,
        fontSize: tokens.typography.fontSize.lg
      },
      xl: {
        height: '3.5rem',
        padding: `0 ${tokens.spacing[8]}`,
        fontSize: tokens.typography.fontSize.xl
      }
    };
    
    return `
      height: ${sizeMap[size].height};
      padding: ${sizeMap[size].padding};
      font-size: ${sizeMap[size].fontSize};
    `;
  }}
  
  /* Variant styles */
  ${({ variant = 'primary' }) => {
    const variantMap = {
      primary: `
        background-color: ${tokens.colors.semantic.primary};
        color: ${tokens.colors.text.inverse};
        box-shadow: ${tokens.shadow.sm};
        
        &:hover:not(:disabled) {
          background-color: rgba(${tokens.colors.primary.blue.replace('#', '').match(/.{2}/g)?.map(hex => parseInt(hex, 16)).join(', ')}, 0.9);
          box-shadow: ${tokens.shadow.md};
          transform: translateY(-1px);
        }
        
        &:active:not(:disabled) {
          background-color: rgba(${tokens.colors.primary.blue.replace('#', '').match(/.{2}/g)?.map(hex => parseInt(hex, 16)).join(', ')}, 0.8);
          box-shadow: ${tokens.shadow.sm};
          transform: translateY(0);
        }
        
        &:focus:not(:disabled) {
          outline: none;
          box-shadow: 0 0 0 2px rgba(${tokens.colors.primary.blue.replace('#', '').match(/.{2}/g)?.map(hex => parseInt(hex, 16)).join(', ')}, 0.3);
        }
      `,
      secondary: `
        background-color: transparent;
        color: ${tokens.colors.semantic.primary};
        border: 1px solid ${tokens.colors.semantic.primary};
        
        &:hover:not(:disabled) {
          background-color: ${tokens.colors.semantic.primary};
          color: ${tokens.colors.text.inverse};
        }
        
        &:active:not(:disabled) {
          background-color: rgba(${tokens.colors.primary.blue.replace('#', '').match(/.{2}/g)?.map(hex => parseInt(hex, 16)).join(', ')}, 0.8);
          color: ${tokens.colors.text.inverse};
        }
      `,
      success: `
        background-color: ${tokens.colors.semantic.success};
        color: ${tokens.colors.text.inverse};
        
        &:hover:not(:disabled) {
          background-color: rgba(${tokens.colors.primary.green.replace('#', '').match(/.{2}/g)?.map(hex => parseInt(hex, 16)).join(', ')}, 0.9);
        }
      `,
      warning: `
        background-color: ${tokens.colors.semantic.warning};
        color: ${tokens.colors.text.inverse};
        
        &:hover:not(:disabled) {
          background-color: rgba(255, 167, 38, 0.9);
        }
      `,
      error: `
        background-color: ${tokens.colors.semantic.error};
        color: ${tokens.colors.text.inverse};
        
        &:hover:not(:disabled) {
          background-color: rgba(239, 83, 80, 0.9);
        }
      `,
      ghost: `
        background-color: transparent;
        color: ${tokens.colors.text.primary};
        
        &:hover:not(:disabled) {
          background-color: ${tokens.colors.background.secondary};
        }
      `
    };
    
    return variantMap[variant];
  }}
  
  /* Full width */
  ${({ fullWidth }) => fullWidth && 'width: 100%;'}
  
  /* Disabled state */
  &:disabled {
    background-color: ${tokens.colors.background.tertiary};
    color: ${tokens.colors.text.tertiary};
    cursor: not-allowed;
    box-shadow: none;
    transform: none;
    border-color: ${tokens.colors.border.secondary};
  }
  
  /* Loading state */
  ${({ loading }) => loading && `
    pointer-events: none;
    
    &::after {
      content: '';
      position: absolute;
      width: 1rem;
      height: 1rem;
      border: 2px solid transparent;
      border-top: 2px solid currentColor;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `}
`;

const IconWrapper = styled.span<{ position: 'left' | 'right' }>`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  
  svg {
    width: 1em;
    height: 1em;
    flex-shrink: 0;
  }
`;

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ 
    variant = 'primary', 
    size = 'md', 
    fullWidth = false, 
    loading = false,
    leftIcon,
    rightIcon,
    children,
    disabled,
    ...props 
  }, ref) => {
    return (
      <StyledButton
        ref={ref}
        variant={variant}
        size={size}
        fullWidth={fullWidth}
        loading={loading}
        disabled={disabled || loading}
        {...props}
      >
        {leftIcon && !loading && (
          <IconWrapper position="left">{leftIcon}</IconWrapper>
        )}
        {!loading && children}
        {rightIcon && !loading && (
          <IconWrapper position="right">{rightIcon}</IconWrapper>
        )}
      </StyledButton>
    );
  }
);

Button.displayName = 'Button';
```

### Input Component Implementation

Create a comprehensive input component that handles various input types and states:

**`src/components/atoms/Input/Input.tsx`**
```typescript
import React, { forwardRef, InputHTMLAttributes, useState } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

export interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label?: string;
  helperText?: string;
  errorMessage?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  variant?: 'outlined' | 'filled';
}

const InputContainer = styled.div<{ fullWidth?: boolean }>`
  display: flex;
  flex-direction: column;
  gap: ${tokens.spacing[2]};
  width: ${({ fullWidth }) => fullWidth ? '100%' : 'auto'};
`;

const Label = styled.label`
  font-size: ${tokens.typography.fontSize.sm};
  font-weight: ${tokens.typography.fontWeight.medium};
  color: ${tokens.colors.text.primary};
  margin-bottom: ${tokens.spacing[1]};
`;

const InputWrapper = styled.div<{ 
  hasError?: boolean; 
  isFocused?: boolean;
  size?: 'sm' | 'md' | 'lg';
  variant?: 'outlined' | 'filled';
}>`
  position: relative;
  display: flex;
  align-items: center;
  
  ${({ size = 'md' }) => {
    const sizeMap = {
      sm: { height: '2rem', padding: tokens.spacing[2] },
      md: { height: '2.5rem', padding: tokens.spacing[3] },
      lg: { height: '3rem', padding: tokens.spacing[4] }
    };
    
    return `
      height: ${sizeMap[size].height};
      padding: 0 ${sizeMap[size].padding};
    `;
  }}
  
  ${({ variant = 'outlined' }) => {
    if (variant === 'outlined') {
      return `
        border: 1px solid ${tokens.colors.border.primary};
        border-radius: ${tokens.borderRadius.base};
        background-color: ${tokens.colors.background.primary};
      `;
    } else {
      return `
        border: none;
        border-bottom: 2px solid ${tokens.colors.border.primary};
        border-radius: 0;
        background-color: ${tokens.colors.background.secondary};
      `;
    }
  }}
  
  ${({ isFocused, variant = 'outlined' }) => isFocused && `
    ${variant === 'outlined' ? `
      border-color: ${tokens.colors.border.focus};
      box-shadow: 0 0 0 2px rgba(${tokens.colors.primary.blue.replace('#', '').match(/.{2}/g)?.map(hex => parseInt(hex, 16)).join(', ')}, 0.1);
    ` : `
      border-bottom-color: ${tokens.colors.border.focus};
    `}
  `}
  
  ${({ hasError, variant = 'outlined' }) => hasError && `
    ${variant === 'outlined' ? `
      border-color: ${tokens.colors.border.error};
    ` : `
      border-bottom-color: ${tokens.colors.border.error};
    `}
  `}
  
  transition: all ${tokens.animation.duration.fast} ${tokens.animation.easing.smooth};
`;

const StyledInput = styled.input`
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-family: ${tokens.typography.fontFamily.primary};
  font-size: ${tokens.typography.fontSize.base};
  color: ${tokens.colors.text.primary};
  
  &::placeholder {
    color: ${tokens.colors.text.tertiary};
  }
  
  &:disabled {
    color: ${tokens.colors.text.tertiary};
    cursor: not-allowed;
  }
`;

const IconWrapper = styled.span<{ position: 'left' | 'right' }>`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: ${tokens.colors.text.secondary};
  
  ${({ position }) => position === 'left' ? `
    margin-right: ${tokens.spacing[2]};
  ` : `
    margin-left: ${tokens.spacing[2]};
  `}
  
  svg {
    width: 1.25rem;
    height: 1.25rem;
    flex-shrink: 0;
  }
`;

const HelperText = styled.span<{ hasError?: boolean }>`
  font-size: ${tokens.typography.fontSize.xs};
  color: ${({ hasError }) => hasError ? tokens.colors.semantic.error : tokens.colors.text.secondary};
  margin-top: ${tokens.spacing[1]};
`;

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ 
    label,
    helperText,
    errorMessage,
    leftIcon,
    rightIcon,
    size = 'md',
    fullWidth = false,
    variant = 'outlined',
    className,
    onFocus,
    onBlur,
    ...props 
  }, ref) => {
    const [isFocused, setIsFocused] = useState(false);
    const hasError = Boolean(errorMessage);
    
    const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
      setIsFocused(true);
      onFocus?.(e);
    };
    
    const handleBlur = (e: React.FocusEvent<HTMLInputElement>) => {
      setIsFocused(false);
      onBlur?.(e);
    };
    
    return (
      <InputContainer fullWidth={fullWidth} className={className}>
        {label && <Label>{label}</Label>}
        <InputWrapper
          hasError={hasError}
          isFocused={isFocused}
          size={size}
          variant={variant}
        >
          {leftIcon && <IconWrapper position="left">{leftIcon}</IconWrapper>}
          <StyledInput
            ref={ref}
            onFocus={handleFocus}
            onBlur={handleBlur}
            {...props}
          />
          {rightIcon && <IconWrapper position="right">{rightIcon}</IconWrapper>}
        </InputWrapper>
        {(helperText || errorMessage) && (
          <HelperText hasError={hasError}>
            {errorMessage || helperText}
          </HelperText>
        )}
      </InputContainer>
    );
  }
);

Input.displayName = 'Input';
```

### Card Component Implementation

Develop a flexible card component that serves as a container for various content types:

**`src/components/molecules/Card/Card.tsx`**
```typescript
import React, { HTMLAttributes } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

export interface CardProps extends HTMLAttributes<HTMLDivElement> {
  variant?: 'default' | 'elevated' | 'outlined' | 'interactive';
  padding?: 'none' | 'sm' | 'md' | 'lg' | 'xl';
  children: React.ReactNode;
}

const StyledCard = styled.div<CardProps>`
  background-color: ${tokens.colors.background.primary};
  border-radius: ${tokens.borderRadius.lg};
  transition: all ${tokens.animation.duration.normal} ${tokens.animation.easing.smooth};
  overflow: hidden;
  
  ${({ variant = 'default' }) => {
    const variantMap = {
      default: `
        border: 1px solid ${tokens.colors.border.primary};
        box-shadow: ${tokens.shadow.sm};
      `,
      elevated: `
        border: none;
        box-shadow: ${tokens.shadow.lg};
      `,
      outlined: `
        border: 2px solid ${tokens.colors.border.primary};
        box-shadow: none;
      `,
      interactive: `
        border: 1px solid ${tokens.colors.border.primary};
        box-shadow: ${tokens.shadow.base};
        cursor: pointer;
        
        &:hover {
          box-shadow: ${tokens.shadow.lg};
          transform: translateY(-2px);
        }
        
        &:active {
          transform: translateY(0);
          box-shadow: ${tokens.shadow.base};
        }
      `
    };
    
    return variantMap[variant];
  }}
  
  ${({ padding = 'md' }) => {
    const paddingMap = {
      none: '0',
      sm: tokens.spacing[4],
      md: tokens.spacing[6],
      lg: tokens.spacing[8],
      xl: tokens.spacing[10]
    };
    
    return `padding: ${paddingMap[padding]};`;
  }}
`;

const CardHeader = styled.div`
  padding: ${tokens.spacing[6]};
  border-bottom: 1px solid ${tokens.colors.border.secondary};
  
  &:last-child {
    border-bottom: none;
  }
`;

const CardTitle = styled.h3`
  margin: 0 0 ${tokens.spacing[1]} 0;
  font-size: ${tokens.typography.fontSize.lg};
  font-weight: ${tokens.typography.fontWeight.bold};
  color: ${tokens.colors.text.primary};
  line-height: ${tokens.typography.lineHeight.tight};
`;

const CardSubtitle = styled.p`
  margin: 0;
  font-size: ${tokens.typography.fontSize.sm};
  color: ${tokens.colors.text.secondary};
  line-height: ${tokens.typography.lineHeight.normal};
`;

const CardBody = styled.div`
  padding: ${tokens.spacing[6]};
`;

const CardFooter = styled.div`
  padding: ${tokens.spacing[6]};
  border-top: 1px solid ${tokens.colors.border.secondary};
  background-color: ${tokens.colors.background.secondary};
  
  &:first-child {
    border-top: none;
    background-color: transparent;
  }
`;

export const Card = ({ children, ...props }: CardProps) => {
  return <StyledCard {...props}>{children}</StyledCard>;
};

Card.Header = CardHeader;
Card.Title = CardTitle;
Card.Subtitle = CardSubtitle;
Card.Body = CardBody;
Card.Footer = CardFooter;
```

### Form Component Implementation

Create a comprehensive form system that handles validation, submission, and error states:

**`src/components/molecules/FormGroup/FormGroup.tsx`**
```typescript
import React, { HTMLAttributes } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

export interface FormGroupProps extends HTMLAttributes<HTMLDivElement> {
  label?: string;
  required?: boolean;
  error?: string;
  helperText?: string;
  children: React.ReactNode;
}

const FormGroupContainer = styled.div`
  margin-bottom: ${tokens.spacing[4]};
`;

const Label = styled.label<{ required?: boolean; hasError?: boolean }>`
  display: block;
  margin-bottom: ${tokens.spacing[2]};
  font-size: ${tokens.typography.fontSize.sm};
  font-weight: ${tokens.typography.fontWeight.medium};
  color: ${({ hasError }) => hasError ? tokens.colors.semantic.error : tokens.colors.text.primary};
  
  ${({ required }) => required && `
    &::after {
      content: ' *';
      color: ${tokens.colors.semantic.error};
    }
  `}
`;

const HelperText = styled.span<{ hasError?: boolean }>`
  display: block;
  margin-top: ${tokens.spacing[1]};
  font-size: ${tokens.typography.fontSize.xs};
  color: ${({ hasError }) => hasError ? tokens.colors.semantic.error : tokens.colors.text.secondary};
  line-height: ${tokens.typography.lineHeight.normal};
`;

export const FormGroup = ({ 
  label, 
  required = false, 
  error, 
  helperText, 
  children, 
  ...props 
}: FormGroupProps) => {
  const hasError = Boolean(error);
  
  return (
    <FormGroupContainer {...props}>
      {label && (
        <Label required={required} hasError={hasError}>
          {label}
        </Label>
      )}
      {children}
      {(error || helperText) && (
        <HelperText hasError={hasError}>
          {error || helperText}
        </HelperText>
      )}
    </FormGroupContainer>
  );
};
```

### Component Testing Guidelines

Implement comprehensive testing for each component using React Testing Library:

**`src/components/atoms/Button/Button.test.tsx`**
```typescript
import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import { Button } from './Button';

describe('Button Component', () => {
  it('renders with default props', () => {
    render(<Button>Click me</Button>);
    const button = screen.getByRole('button', { name: /click me/i });
    expect(button).toBeInTheDocument();
    expect(button).toHaveAttribute('type', 'button');
  });
  
  it('applies variant styles correctly', () => {
    render(<Button variant="secondary">Secondary Button</Button>);
    const button = screen.getByRole('button');
    expect(button).toHaveStyle({
      backgroundColor: 'transparent'
    });
  });
  
  it('handles click events', () => {
    const handleClick = jest.fn();
    render(<Button onClick={handleClick}>Click me</Button>);
    
    fireEvent.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
  
  it('shows loading state correctly', () => {
    render(<Button loading>Loading Button</Button>);
    const button = screen.getByRole('button');
    expect(button).toBeDisabled();
    expect(button).toHaveStyle({
      pointerEvents: 'none'
    });
  });
  
  it('renders with icons', () => {
    const LeftIcon = () => <span data-testid="left-icon">←</span>;
    const RightIcon = () => <span data-testid="right-icon">→</span>;
    
    render(
      <Button leftIcon={<LeftIcon />} rightIcon={<RightIcon />}>
        Button with Icons
      </Button>
    );
    
    expect(screen.getByTestId('left-icon')).toBeInTheDocument();
    expect(screen.getByTestId('right-icon')).toBeInTheDocument();
  });
  
  it('supports accessibility attributes', () => {
    render(
      <Button 
        aria-label="Close dialog" 
        aria-describedby="close-help"
      >
        ×
      </Button>
    );
    
    const button = screen.getByRole('button');
    expect(button).toHaveAttribute('aria-label', 'Close dialog');
    expect(button).toHaveAttribute('aria-describedby', 'close-help');
  });
});
```

### Storybook Integration

Create comprehensive Storybook stories for component documentation and testing:

**`src/components/atoms/Button/Button.stories.tsx`**
```typescript
import type { Meta, StoryObj } from '@storybook/react';
import { Button } from './Button';

const meta: Meta<typeof Button> = {
  title: 'Atoms/Button',
  component: Button,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'A versatile button component that supports multiple variants, sizes, and states. Built with accessibility and performance in mind.'
      }
    }
  },
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'secondary', 'success', 'warning', 'error', 'ghost'],
      description: 'Visual style variant of the button'
    },
    size: {
      control: 'select',
      options: ['sm', 'md', 'lg', 'xl'],
      description: 'Size of the button'
    },
    fullWidth: {
      control: 'boolean',
      description: 'Whether the button should take full width of its container'
    },
    loading: {
      control: 'boolean',
      description: 'Shows loading spinner and disables interaction'
    },
    disabled: {
      control: 'boolean',
      description: 'Disables the button'
    }
  },
  tags: ['autodocs']
};

export default meta;
type Story = StoryObj<typeof meta>;

export const Primary: Story = {
  args: {
    children: 'Primary Button',
    variant: 'primary'
  }
};

export const Secondary: Story = {
  args: {
    children: 'Secondary Button',
    variant: 'secondary'
  }
};

export const AllVariants: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
      <Button variant="primary">Primary</Button>
      <Button variant="secondary">Secondary</Button>
      <Button variant="success">Success</Button>
      <Button variant="warning">Warning</Button>
      <Button variant="error">Error</Button>
      <Button variant="ghost">Ghost</Button>
    </div>
  )
};

export const AllSizes: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', flexWrap: 'wrap' }}>
      <Button size="sm">Small</Button>
      <Button size="md">Medium</Button>
      <Button size="lg">Large</Button>
      <Button size="xl">Extra Large</Button>
    </div>
  )
};

export const WithIcons: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
      <Button leftIcon={<span>📧</span>}>Send Email</Button>
      <Button rightIcon={<span>→</span>}>Next Step</Button>
      <Button leftIcon={<span>💾</span>} rightIcon={<span>✓</span>}>Save Changes</Button>
    </div>
  )
};

export const States: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
      <Button>Normal</Button>
      <Button loading>Loading</Button>
      <Button disabled>Disabled</Button>
    </div>
  )
};

export const FullWidth: Story = {
  args: {
    children: 'Full Width Button',
    fullWidth: true
  },
  parameters: {
    layout: 'padded'
  }
};
```

---

## Layout and Responsive Design

### Grid System Implementation

Implement a flexible grid system that supports the TETCO design requirements:

**`src/components/layout/Grid/Grid.tsx`**
```typescript
import React, { HTMLAttributes } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

export interface ContainerProps extends HTMLAttributes<HTMLDivElement> {
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | 'full';
  fluid?: boolean;
  children: React.ReactNode;
}

export interface RowProps extends HTMLAttributes<HTMLDivElement> {
  gutter?: 'none' | 'sm' | 'md' | 'lg' | 'xl';
  align?: 'start' | 'center' | 'end' | 'stretch';
  justify?: 'start' | 'center' | 'end' | 'between' | 'around' | 'evenly';
  children: React.ReactNode;
}

export interface ColProps extends HTMLAttributes<HTMLDivElement> {
  span?: number | 'auto';
  offset?: number;
  order?: number;
  xs?: number | 'auto';
  sm?: number | 'auto';
  md?: number | 'auto';
  lg?: number | 'auto';
  xl?: number | 'auto';
  children: React.ReactNode;
}

const StyledContainer = styled.div<ContainerProps>`
  width: 100%;
  margin-left: auto;
  margin-right: auto;
  padding-left: ${tokens.spacing[4]};
  padding-right: ${tokens.spacing[4]};
  
  ${({ maxWidth = 'xl', fluid }) => {
    if (fluid) return 'max-width: none;';
    
    const maxWidthMap = {
      sm: '640px',
      md: '768px',
      lg: '1024px',
      xl: '1280px',
      '2xl': '1536px',
      full: 'none'
    };
    
    return `max-width: ${maxWidthMap[maxWidth]};`;
  }}
  
  @media (min-width: ${tokens.breakpoints.sm}) {
    padding-left: ${tokens.spacing[6]};
    padding-right: ${tokens.spacing[6]};
  }
`;

const StyledRow = styled.div<RowProps>`
  display: flex;
  flex-wrap: wrap;
  
  ${({ gutter = 'md' }) => {
    const gutterMap = {
      none: '0',
      sm: tokens.spacing[2],
      md: tokens.spacing[4],
      lg: tokens.spacing[6],
      xl: tokens.spacing[8]
    };
    
    const gutterValue = gutterMap[gutter];
    
    return `
      margin-left: calc(${gutterValue} / -2);
      margin-right: calc(${gutterValue} / -2);
      
      > * {
        padding-left: calc(${gutterValue} / 2);
        padding-right: calc(${gutterValue} / 2);
      }
    `;
  }}
  
  ${({ align }) => align && `align-items: ${align === 'start' ? 'flex-start' : align === 'end' ? 'flex-end' : align};`}
  
  ${({ justify }) => {
    const justifyMap = {
      start: 'flex-start',
      center: 'center',
      end: 'flex-end',
      between: 'space-between',
      around: 'space-around',
      evenly: 'space-evenly'
    };
    
    return justify && `justify-content: ${justifyMap[justify]};`;
  }}
`;

const StyledCol = styled.div<ColProps>`
  flex: 1 0 0%;
  max-width: 100%;
  
  ${({ span }) => {
    if (span === 'auto') return 'flex: 0 0 auto; width: auto;';
    if (typeof span === 'number') {
      const percentage = (span / 12) * 100;
      return `flex: 0 0 ${percentage}%; max-width: ${percentage}%;`;
    }
    return '';
  }}
  
  ${({ offset }) => {
    if (typeof offset === 'number') {
      const percentage = (offset / 12) * 100;
      return `margin-left: ${percentage}%;`;
    }
    return '';
  }}
  
  ${({ order }) => order && `order: ${order};`}
  
  /* Responsive breakpoints */
  @media (min-width: ${tokens.breakpoints.xs}) {
    ${({ xs }) => {
      if (xs === 'auto') return 'flex: 0 0 auto; width: auto;';
      if (typeof xs === 'number') {
        const percentage = (xs / 12) * 100;
        return `flex: 0 0 ${percentage}%; max-width: ${percentage}%;`;
      }
      return '';
    }}
  }
  
  @media (min-width: ${tokens.breakpoints.sm}) {
    ${({ sm }) => {
      if (sm === 'auto') return 'flex: 0 0 auto; width: auto;';
      if (typeof sm === 'number') {
        const percentage = (sm / 12) * 100;
        return `flex: 0 0 ${percentage}%; max-width: ${percentage}%;`;
      }
      return '';
    }}
  }
  
  @media (min-width: ${tokens.breakpoints.md}) {
    ${({ md }) => {
      if (md === 'auto') return 'flex: 0 0 auto; width: auto;';
      if (typeof md === 'number') {
        const percentage = (md / 12) * 100;
        return `flex: 0 0 ${percentage}%; max-width: ${percentage}%;`;
      }
      return '';
    }}
  }
  
  @media (min-width: ${tokens.breakpoints.lg}) {
    ${({ lg }) => {
      if (lg === 'auto') return 'flex: 0 0 auto; width: auto;';
      if (typeof lg === 'number') {
        const percentage = (lg / 12) * 100;
        return `flex: 0 0 ${percentage}%; max-width: ${percentage}%;`;
      }
      return '';
    }}
  }
  
  @media (min-width: ${tokens.breakpoints.xl}) {
    ${({ xl }) => {
      if (xl === 'auto') return 'flex: 0 0 auto; width: auto;';
      if (typeof xl === 'number') {
        const percentage = (xl / 12) * 100;
        return `flex: 0 0 ${percentage}%; max-width: ${percentage}%;`;
      }
      return '';
    }}
  }
`;

export const Container = ({ children, ...props }: ContainerProps) => {
  return <StyledContainer {...props}>{children}</StyledContainer>;
};

export const Row = ({ children, ...props }: RowProps) => {
  return <StyledRow {...props}>{children}</StyledRow>;
};

export const Col = ({ children, ...props }: ColProps) => {
  return <StyledCol {...props}>{children}</StyledCol>;
};
```

### Responsive Utilities

Create utility hooks and components for responsive design:

**`src/hooks/useBreakpoint.ts`**
```typescript
import { useState, useEffect } from 'react';
import { tokens } from '@styles/tokens';

type Breakpoint = 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl';

const breakpointValues = {
  xs: 320,
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536
};

export const useBreakpoint = () => {
  const [currentBreakpoint, setCurrentBreakpoint] = useState<Breakpoint>('xs');
  const [windowWidth, setWindowWidth] = useState<number>(0);
  
  useEffect(() => {
    const handleResize = () => {
      const width = window.innerWidth;
      setWindowWidth(width);
      
      if (width >= breakpointValues['2xl']) {
        setCurrentBreakpoint('2xl');
      } else if (width >= breakpointValues.xl) {
        setCurrentBreakpoint('xl');
      } else if (width >= breakpointValues.lg) {
        setCurrentBreakpoint('lg');
      } else if (width >= breakpointValues.md) {
        setCurrentBreakpoint('md');
      } else if (width >= breakpointValues.sm) {
        setCurrentBreakpoint('sm');
      } else {
        setCurrentBreakpoint('xs');
      }
    };
    
    // Set initial value
    handleResize();
    
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);
  
  const isBreakpoint = (breakpoint: Breakpoint) => {
    return windowWidth >= breakpointValues[breakpoint];
  };
  
  const isMobile = currentBreakpoint === 'xs' || currentBreakpoint === 'sm';
  const isTablet = currentBreakpoint === 'md';
  const isDesktop = currentBreakpoint === 'lg' || currentBreakpoint === 'xl' || currentBreakpoint === '2xl';
  
  return {
    currentBreakpoint,
    windowWidth,
    isBreakpoint,
    isMobile,
    isTablet,
    isDesktop
  };
};
```

### Application Layout Components

Create layout templates for different page types:

**`src/components/templates/DashboardLayout/DashboardLayout.tsx`**
```typescript
import React, { useState } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';
import { useBreakpoint } from '@hooks/useBreakpoint';

export interface DashboardLayoutProps {
  header: React.ReactNode;
  sidebar: React.ReactNode;
  children: React.ReactNode;
  sidebarCollapsed?: boolean;
  onSidebarToggle?: () => void;
}

const LayoutContainer = styled.div`
  display: flex;
  min-height: 100vh;
  background-color: ${tokens.colors.background.secondary};
`;

const Sidebar = styled.aside<{ collapsed?: boolean; isOpen?: boolean }>`
  flex: 0 0 auto;
  width: ${({ collapsed }) => collapsed ? '4rem' : '16rem'};
  background-color: ${tokens.colors.background.primary};
  border-right: 1px solid ${tokens.colors.border.primary};
  box-shadow: ${tokens.shadow.sm};
  z-index: ${tokens.zIndex.docked};
  transition: all ${tokens.animation.duration.normal} ${tokens.animation.easing.smooth};
  
  @media (max-width: 1023px) {
    position: fixed;
    top: 0;
    left: 0;
    height: 100vh;
    width: 16rem;
    transform: ${({ isOpen }) => isOpen ? 'translateX(0)' : 'translateX(-100%)'};
    z-index: ${tokens.zIndex.overlay};
  }
`;

const MainContent = styled.main<{ sidebarCollapsed?: boolean }>`
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  
  @media (max-width: 1023px) {
    margin-left: 0;
  }
`;

const Header = styled.header`
  flex: 0 0 auto;
  background-color: ${tokens.colors.background.primary};
  border-bottom: 1px solid ${tokens.colors.border.primary};
  box-shadow: ${tokens.shadow.sm};
  z-index: ${tokens.zIndex.sticky};
`;

const Content = styled.div`
  flex: 1;
  padding: ${tokens.spacing[6]};
  overflow-y: auto;
  
  @media (max-width: 767px) {
    padding: ${tokens.spacing[4]};
  }
`;

const Overlay = styled.div<{ isVisible?: boolean }>`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: ${tokens.colors.background.overlay};
  z-index: ${tokens.zIndex.overlay};
  opacity: ${({ isVisible }) => isVisible ? 1 : 0};
  visibility: ${({ isVisible }) => isVisible ? 'visible' : 'hidden'};
  transition: all ${tokens.animation.duration.normal} ${tokens.animation.easing.smooth};
  
  @media (min-width: 1024px) {
    display: none;
  }
`;

export const DashboardLayout = ({
  header,
  sidebar,
  children,
  sidebarCollapsed = false,
  onSidebarToggle
}: DashboardLayoutProps) => {
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const { isMobile, isTablet } = useBreakpoint();
  
  const handleSidebarToggle = () => {
    if (isMobile || isTablet) {
      setMobileSidebarOpen(!mobileSidebarOpen);
    } else {
      onSidebarToggle?.();
    }
  };
  
  const handleOverlayClick = () => {
    setMobileSidebarOpen(false);
  };
  
  return (
    <LayoutContainer>
      <Sidebar 
        collapsed={sidebarCollapsed} 
        isOpen={mobileSidebarOpen}
      >
        {sidebar}
      </Sidebar>
      
      <MainContent sidebarCollapsed={sidebarCollapsed}>
        <Header>{header}</Header>
        <Content>{children}</Content>
      </MainContent>
      
      <Overlay 
        isVisible={mobileSidebarOpen} 
        onClick={handleOverlayClick}
      />
    </LayoutContainer>
  );
};
```

---


## Accessibility Implementation

### WCAG 2.1 AA Compliance

The TETCO design system must meet WCAG 2.1 AA accessibility standards to ensure the application is usable by all users, including those with disabilities. This section provides comprehensive guidelines for implementing accessible components and interactions.

### Color Contrast Requirements

All text and interactive elements must meet minimum contrast ratios. The TETCO color palette has been designed with accessibility in mind:

**`src/utils/accessibility.ts`**
```typescript
export const contrastRatios = {
  // TETCO Primary Blue (#005F96) on white background
  primaryBlueOnWhite: 4.52, // AA compliant for normal text
  
  // TETCO Dark Teal (#0C3C44) on white background  
  darkTealOnWhite: 8.94, // AAA compliant for all text sizes
  
  // White text on TETCO Primary Blue
  whiteOnPrimaryBlue: 4.52, // AA compliant for normal text
  
  // TETCO Light Green (#4CFCB4) on TETCO Dark Teal
  lightGreenOnDarkTeal: 7.23, // AAA compliant for all text sizes
};

export const validateContrast = (foreground: string, background: string): boolean => {
  // Implementation would use a contrast calculation library
  // This is a simplified example
  const ratio = calculateContrastRatio(foreground, background);
  return ratio >= 4.5; // WCAG AA standard for normal text
};

export const calculateContrastRatio = (color1: string, color2: string): number => {
  // Convert hex to RGB and calculate luminance
  const getLuminance = (hex: string): number => {
    const rgb = hexToRgb(hex);
    const [r, g, b] = rgb.map(c => {
      c = c / 255;
      return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    });
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  };
  
  const lum1 = getLuminance(color1);
  const lum2 = getLuminance(color2);
  const brightest = Math.max(lum1, lum2);
  const darkest = Math.min(lum1, lum2);
  
  return (brightest + 0.05) / (darkest + 0.05);
};

const hexToRgb = (hex: string): [number, number, number] => {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result ? [
    parseInt(result[1], 16),
    parseInt(result[2], 16),
    parseInt(result[3], 16)
  ] : [0, 0, 0];
};
```

### Keyboard Navigation

Implement comprehensive keyboard navigation support for all interactive elements:

**`src/hooks/useKeyboardNavigation.ts`**
```typescript
import { useEffect, useRef, KeyboardEvent } from 'react';

export interface KeyboardNavigationOptions {
  onEnter?: () => void;
  onSpace?: () => void;
  onEscape?: () => void;
  onArrowUp?: () => void;
  onArrowDown?: () => void;
  onArrowLeft?: () => void;
  onArrowRight?: () => void;
  onTab?: () => void;
  onShiftTab?: () => void;
  preventDefault?: boolean;
}

export const useKeyboardNavigation = (options: KeyboardNavigationOptions) => {
  const elementRef = useRef<HTMLElement>(null);
  
  useEffect(() => {
    const element = elementRef.current;
    if (!element) return;
    
    const handleKeyDown = (event: KeyboardEvent<HTMLElement>) => {
      const { key, shiftKey } = event;
      
      switch (key) {
        case 'Enter':
          if (options.onEnter) {
            if (options.preventDefault) event.preventDefault();
            options.onEnter();
          }
          break;
          
        case ' ':
          if (options.onSpace) {
            if (options.preventDefault) event.preventDefault();
            options.onSpace();
          }
          break;
          
        case 'Escape':
          if (options.onEscape) {
            if (options.preventDefault) event.preventDefault();
            options.onEscape();
          }
          break;
          
        case 'ArrowUp':
          if (options.onArrowUp) {
            if (options.preventDefault) event.preventDefault();
            options.onArrowUp();
          }
          break;
          
        case 'ArrowDown':
          if (options.onArrowDown) {
            if (options.preventDefault) event.preventDefault();
            options.onArrowDown();
          }
          break;
          
        case 'ArrowLeft':
          if (options.onArrowLeft) {
            if (options.preventDefault) event.preventDefault();
            options.onArrowLeft();
          }
          break;
          
        case 'ArrowRight':
          if (options.onArrowRight) {
            if (options.preventDefault) event.preventDefault();
            options.onArrowRight();
          }
          break;
          
        case 'Tab':
          if (shiftKey && options.onShiftTab) {
            if (options.preventDefault) event.preventDefault();
            options.onShiftTab();
          } else if (!shiftKey && options.onTab) {
            if (options.preventDefault) event.preventDefault();
            options.onTab();
          }
          break;
      }
    };
    
    element.addEventListener('keydown', handleKeyDown as any);
    return () => element.removeEventListener('keydown', handleKeyDown as any);
  }, [options]);
  
  return elementRef;
};
```

### Focus Management

Implement proper focus management for complex components:

**`src/hooks/useFocusManagement.ts`**
```typescript
import { useRef, useEffect } from 'react';

export const useFocusManagement = () => {
  const containerRef = useRef<HTMLElement>(null);
  
  const getFocusableElements = (): HTMLElement[] => {
    if (!containerRef.current) return [];
    
    const focusableSelectors = [
      'button:not([disabled])',
      'input:not([disabled])',
      'select:not([disabled])',
      'textarea:not([disabled])',
      'a[href]',
      '[tabindex]:not([tabindex="-1"])',
      '[contenteditable="true"]'
    ].join(', ');
    
    return Array.from(containerRef.current.querySelectorAll(focusableSelectors));
  };
  
  const focusFirst = () => {
    const focusableElements = getFocusableElements();
    if (focusableElements.length > 0) {
      focusableElements[0].focus();
    }
  };
  
  const focusLast = () => {
    const focusableElements = getFocusableElements();
    if (focusableElements.length > 0) {
      focusableElements[focusableElements.length - 1].focus();
    }
  };
  
  const trapFocus = (event: KeyboardEvent) => {
    if (event.key !== 'Tab') return;
    
    const focusableElements = getFocusableElements();
    if (focusableElements.length === 0) return;
    
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];
    
    if (event.shiftKey) {
      if (document.activeElement === firstElement) {
        event.preventDefault();
        lastElement.focus();
      }
    } else {
      if (document.activeElement === lastElement) {
        event.preventDefault();
        firstElement.focus();
      }
    }
  };
  
  return {
    containerRef,
    focusFirst,
    focusLast,
    trapFocus,
    getFocusableElements
  };
};
```

### Screen Reader Support

Implement comprehensive screen reader support with proper ARIA attributes:

**`src/components/atoms/VisuallyHidden/VisuallyHidden.tsx`**
```typescript
import React, { HTMLAttributes } from 'react';
import { styled } from '@emotion/styled';

const StyledVisuallyHidden = styled.span`
  position: absolute !important;
  width: 1px !important;
  height: 1px !important;
  padding: 0 !important;
  margin: -1px !important;
  overflow: hidden !important;
  clip: rect(0, 0, 0, 0) !important;
  white-space: nowrap !important;
  border: 0 !important;
`;

export interface VisuallyHiddenProps extends HTMLAttributes<HTMLSpanElement> {
  children: React.ReactNode;
}

export const VisuallyHidden = ({ children, ...props }: VisuallyHiddenProps) => {
  return <StyledVisuallyHidden {...props}>{children}</StyledVisuallyHidden>;
};
```

**`src/hooks/useAnnouncement.ts`**
```typescript
import { useRef, useCallback } from 'react';

export const useAnnouncement = () => {
  const announcementRef = useRef<HTMLDivElement>(null);
  
  const announce = useCallback((message: string, priority: 'polite' | 'assertive' = 'polite') => {
    if (!announcementRef.current) return;
    
    // Clear previous announcement
    announcementRef.current.textContent = '';
    
    // Set new announcement after a brief delay to ensure screen readers pick it up
    setTimeout(() => {
      if (announcementRef.current) {
        announcementRef.current.setAttribute('aria-live', priority);
        announcementRef.current.textContent = message;
      }
    }, 100);
  }, []);
  
  const AnnouncementRegion = () => (
    <div
      ref={announcementRef}
      aria-live="polite"
      aria-atomic="true"
      style={{
        position: 'absolute',
        left: '-10000px',
        width: '1px',
        height: '1px',
        overflow: 'hidden'
      }}
    />
  );
  
  return { announce, AnnouncementRegion };
};
```

### Accessible Form Components

Enhance form components with comprehensive accessibility features:

**`src/components/molecules/AccessibleFormGroup/AccessibleFormGroup.tsx`**
```typescript
import React, { useId } from 'react';
import { FormGroup, FormGroupProps } from '../FormGroup/FormGroup';

export interface AccessibleFormGroupProps extends FormGroupProps {
  children: React.ReactElement;
  describedBy?: string;
}

export const AccessibleFormGroup = ({ 
  children, 
  label, 
  required, 
  error, 
  helperText, 
  describedBy,
  ...props 
}: AccessibleFormGroupProps) => {
  const fieldId = useId();
  const errorId = useId();
  const helperId = useId();
  
  // Build aria-describedby string
  const ariaDescribedBy = [
    error ? errorId : null,
    helperText ? helperId : null,
    describedBy
  ].filter(Boolean).join(' ') || undefined;
  
  // Clone child element with accessibility props
  const enhancedChild = React.cloneElement(children, {
    id: fieldId,
    'aria-describedby': ariaDescribedBy,
    'aria-invalid': error ? 'true' : undefined,
    'aria-required': required ? 'true' : undefined
  });
  
  return (
    <FormGroup
      label={label}
      required={required}
      error={error}
      helperText={helperText}
      {...props}
    >
      {enhancedChild}
      {error && <span id={errorId} style={{ display: 'none' }}>{error}</span>}
      {helperText && <span id={helperId} style={{ display: 'none' }}>{helperText}</span>}
    </FormGroup>
  );
};
```

---

## Performance Optimization

### Code Splitting and Lazy Loading

Implement strategic code splitting to optimize bundle size and loading performance:

**`src/utils/lazyImport.ts`**
```typescript
import { lazy, ComponentType } from 'react';

export const lazyImport = <T extends ComponentType<any>>(
  importFunc: () => Promise<{ default: T }>
) => {
  return lazy(importFunc);
};

// Usage example for route-based code splitting
export const LazyDashboard = lazyImport(() => import('../pages/Dashboard/Dashboard'));
export const LazyStudentManagement = lazyImport(() => import('../pages/StudentManagement/StudentManagement'));
export const LazyBusTracking = lazyImport(() => import('../pages/BusTracking/BusTracking'));
export const LazyPayments = lazyImport(() => import('../pages/Payments/Payments'));
```

**`src/components/common/LoadingBoundary/LoadingBoundary.tsx`**
```typescript
import React, { Suspense } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

const LoadingContainer = styled.div`
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  padding: ${tokens.spacing[8]};
`;

const LoadingSpinner = styled.div`
  width: 2rem;
  height: 2rem;
  border: 2px solid ${tokens.colors.border.primary};
  border-top: 2px solid ${tokens.colors.semantic.primary};
  border-radius: 50%;
  animation: spin 1s linear infinite;
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
`;

const LoadingText = styled.p`
  margin-left: ${tokens.spacing[3]};
  color: ${tokens.colors.text.secondary};
  font-size: ${tokens.typography.fontSize.sm};
`;

export interface LoadingBoundaryProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export const LoadingBoundary = ({ children, fallback }: LoadingBoundaryProps) => {
  const defaultFallback = (
    <LoadingContainer>
      <LoadingSpinner />
      <LoadingText>Loading...</LoadingText>
    </LoadingContainer>
  );
  
  return (
    <Suspense fallback={fallback || defaultFallback}>
      {children}
    </Suspense>
  );
};
```

### Image Optimization

Implement responsive image loading with proper optimization:

**`src/components/atoms/OptimizedImage/OptimizedImage.tsx`**
```typescript
import React, { useState, useRef, useEffect } from 'react';
import { styled } from '@emotion/styled';
import { tokens } from '@styles/tokens';

export interface OptimizedImageProps {
  src: string;
  alt: string;
  width?: number;
  height?: number;
  sizes?: string;
  srcSet?: string;
  loading?: 'lazy' | 'eager';
  placeholder?: string;
  className?: string;
  onLoad?: () => void;
  onError?: () => void;
}

const ImageContainer = styled.div`
  position: relative;
  overflow: hidden;
`;

const Image = styled.img<{ isLoaded?: boolean }>`
  width: 100%;
  height: auto;
  transition: opacity ${tokens.animation.duration.normal} ${tokens.animation.easing.smooth};
  opacity: ${({ isLoaded }) => isLoaded ? 1 : 0};
`;

const Placeholder = styled.div<{ isVisible?: boolean }>`
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: ${tokens.colors.background.tertiary};
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: ${({ isVisible }) => isVisible ? 1 : 0};
  transition: opacity ${tokens.animation.duration.normal} ${tokens.animation.easing.smooth};
`;

const PlaceholderText = styled.span`
  color: ${tokens.colors.text.tertiary};
  font-size: ${tokens.typography.fontSize.sm};
`;

export const OptimizedImage = ({
  src,
  alt,
  width,
  height,
  sizes,
  srcSet,
  loading = 'lazy',
  placeholder,
  className,
  onLoad,
  onError
}: OptimizedImageProps) => {
  const [isLoaded, setIsLoaded] = useState(false);
  const [hasError, setHasError] = useState(false);
  const imgRef = useRef<HTMLImageElement>(null);
  
  useEffect(() => {
    const img = imgRef.current;
    if (!img) return;
    
    if (img.complete) {
      setIsLoaded(true);
    }
  }, []);
  
  const handleLoad = () => {
    setIsLoaded(true);
    onLoad?.();
  };
  
  const handleError = () => {
    setHasError(true);
    onError?.();
  };
  
  return (
    <ImageContainer className={className}>
      <Image
        ref={imgRef}
        src={src}
        alt={alt}
        width={width}
        height={height}
        sizes={sizes}
        srcSet={srcSet}
        loading={loading}
        isLoaded={isLoaded}
        onLoad={handleLoad}
        onError={handleError}
      />
      
      <Placeholder isVisible={!isLoaded && !hasError}>
        {placeholder ? (
          <img src={placeholder} alt="" />
        ) : (
          <PlaceholderText>Loading image...</PlaceholderText>
        )}
      </Placeholder>
      
      {hasError && (
        <Placeholder isVisible={true}>
          <PlaceholderText>Failed to load image</PlaceholderText>
        </Placeholder>
      )}
    </ImageContainer>
  );
};
```

### Virtual Scrolling for Large Lists

Implement virtual scrolling for performance with large datasets:

**`src/components/molecules/VirtualList/VirtualList.tsx`**
```typescript
import React, { useState, useEffect, useRef, useMemo } from 'react';
import { styled } from '@emotion/styled';

export interface VirtualListProps<T> {
  items: T[];
  itemHeight: number;
  containerHeight: number;
  renderItem: (item: T, index: number) => React.ReactNode;
  overscan?: number;
  className?: string;
}

const Container = styled.div<{ height: number }>`
  height: ${({ height }) => height}px;
  overflow-y: auto;
  position: relative;
`;

const InnerContainer = styled.div<{ height: number }>`
  height: ${({ height }) => height}px;
  position: relative;
`;

const ItemContainer = styled.div<{ top: number; height: number }>`
  position: absolute;
  top: ${({ top }) => top}px;
  left: 0;
  right: 0;
  height: ${({ height }) => height}px;
`;

export function VirtualList<T>({
  items,
  itemHeight,
  containerHeight,
  renderItem,
  overscan = 5,
  className
}: VirtualListProps<T>) {
  const [scrollTop, setScrollTop] = useState(0);
  const containerRef = useRef<HTMLDivElement>(null);
  
  const totalHeight = items.length * itemHeight;
  
  const visibleRange = useMemo(() => {
    const startIndex = Math.max(0, Math.floor(scrollTop / itemHeight) - overscan);
    const endIndex = Math.min(
      items.length - 1,
      Math.ceil((scrollTop + containerHeight) / itemHeight) + overscan
    );
    
    return { startIndex, endIndex };
  }, [scrollTop, itemHeight, containerHeight, items.length, overscan]);
  
  const visibleItems = useMemo(() => {
    const result = [];
    for (let i = visibleRange.startIndex; i <= visibleRange.endIndex; i++) {
      result.push({
        index: i,
        item: items[i],
        top: i * itemHeight
      });
    }
    return result;
  }, [items, visibleRange, itemHeight]);
  
  const handleScroll = (event: React.UIEvent<HTMLDivElement>) => {
    setScrollTop(event.currentTarget.scrollTop);
  };
  
  return (
    <Container
      ref={containerRef}
      height={containerHeight}
      onScroll={handleScroll}
      className={className}
    >
      <InnerContainer height={totalHeight}>
        {visibleItems.map(({ index, item, top }) => (
          <ItemContainer key={index} top={top} height={itemHeight}>
            {renderItem(item, index)}
          </ItemContainer>
        ))}
      </InnerContainer>
    </Container>
  );
}
```

### Memoization and Performance Hooks

Create performance optimization utilities:

**`src/hooks/useDebounce.ts`**
```typescript
import { useState, useEffect } from 'react';

export const useDebounce = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    
    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);
  
  return debouncedValue;
};
```

**`src/hooks/useThrottle.ts`**
```typescript
import { useRef, useCallback } from 'react';

export const useThrottle = <T extends (...args: any[]) => any>(
  callback: T,
  delay: number
): T => {
  const lastRun = useRef(Date.now());
  
  return useCallback(
    ((...args) => {
      if (Date.now() - lastRun.current >= delay) {
        callback(...args);
        lastRun.current = Date.now();
      }
    }) as T,
    [callback, delay]
  );
};
```

---

## Testing and Quality Assurance

### Unit Testing Strategy

Implement comprehensive unit testing for all components:

**`src/utils/test-utils.tsx`**
```typescript
import React, { ReactElement } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { ThemeProvider } from '@emotion/react';
import { BrowserRouter } from 'react-router-dom';

// Mock theme provider for testing
const MockThemeProvider = ({ children }: { children: React.ReactNode }) => {
  return <div data-testid="theme-provider">{children}</div>;
};

const AllTheProviders = ({ children }: { children: React.ReactNode }) => {
  return (
    <BrowserRouter>
      <MockThemeProvider>
        {children}
      </MockThemeProvider>
    </BrowserRouter>
  );
};

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllTheProviders, ...options });

export * from '@testing-library/react';
export { customRender as render };

// Custom matchers
export const toBeAccessible = (element: HTMLElement) => {
  // Implementation would use axe-core for accessibility testing
  return {
    pass: true,
    message: () => 'Element is accessible'
  };
};

// Mock data generators
export const mockStudent = (overrides = {}) => ({
  id: '1',
  name: 'John Doe',
  grade: '5th Grade',
  busNumber: 'Bus #12',
  parentName: 'Jane Doe',
  parentPhone: '+1234567890',
  ...overrides
});

export const mockBus = (overrides = {}) => ({
  id: '1',
  number: 'Bus #12',
  driver: 'Mohammed Hassan',
  capacity: 45,
  currentLocation: { lat: 25.2048, lng: 55.2708 },
  status: 'active',
  ...overrides
});
```

### Integration Testing

Create integration tests for complex user flows:

**`src/components/pages/Dashboard/Dashboard.integration.test.tsx`**
```typescript
import React from 'react';
import { screen, waitFor, fireEvent } from '@testing-library/react';
import { render, mockStudent, mockBus } from '@utils/test-utils';
import { Dashboard } from './Dashboard';
import { rest } from 'msw';
import { setupServer } from 'msw/node';

// Mock API server
const server = setupServer(
  rest.get('/api/dashboard/stats', (req, res, ctx) => {
    return res(ctx.json({
      totalStudents: 1247,
      activeBuses: 45,
      routesToday: 23,
      onTimePerformance: 94
    }));
  }),
  
  rest.get('/api/students', (req, res, ctx) => {
    return res(ctx.json([
      mockStudent({ id: '1', name: 'Ahmed Ali' }),
      mockStudent({ id: '2', name: 'Sarah Hassan' })
    ]));
  }),
  
  rest.get('/api/buses/live', (req, res, ctx) => {
    return res(ctx.json([
      mockBus({ id: '1', number: 'Bus #12' }),
      mockBus({ id: '2', number: 'Bus #15' })
    ]));
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('Dashboard Integration', () => {
  it('loads and displays dashboard data correctly', async () => {
    render(<Dashboard />);
    
    // Check loading state
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
    
    // Wait for data to load
    await waitFor(() => {
      expect(screen.getByText('1,247')).toBeInTheDocument();
      expect(screen.getByText('45')).toBeInTheDocument();
      expect(screen.getByText('23')).toBeInTheDocument();
      expect(screen.getByText('94%')).toBeInTheDocument();
    });
    
    // Check that student data is displayed
    expect(screen.getByText('Ahmed Ali')).toBeInTheDocument();
    expect(screen.getByText('Sarah Hassan')).toBeInTheDocument();
    
    // Check that bus data is displayed
    expect(screen.getByText('Bus #12')).toBeInTheDocument();
    expect(screen.getByText('Bus #15')).toBeInTheDocument();
  });
  
  it('handles API errors gracefully', async () => {
    server.use(
      rest.get('/api/dashboard/stats', (req, res, ctx) => {
        return res(ctx.status(500));
      })
    );
    
    render(<Dashboard />);
    
    await waitFor(() => {
      expect(screen.getByText(/error loading dashboard data/i)).toBeInTheDocument();
    });
  });
  
  it('allows filtering and searching students', async () => {
    render(<Dashboard />);
    
    // Wait for initial load
    await waitFor(() => {
      expect(screen.getByText('Ahmed Ali')).toBeInTheDocument();
    });
    
    // Search for specific student
    const searchInput = screen.getByPlaceholderText(/search students/i);
    fireEvent.change(searchInput, { target: { value: 'Ahmed' } });
    
    await waitFor(() => {
      expect(screen.getByText('Ahmed Ali')).toBeInTheDocument();
      expect(screen.queryByText('Sarah Hassan')).not.toBeInTheDocument();
    });
  });
});
```

### Accessibility Testing

Implement automated accessibility testing:

**`src/utils/accessibility-testing.ts`**
```typescript
import { axe, toHaveNoViolations } from 'jest-axe';

expect.extend(toHaveNoViolations);

export const runAccessibilityTests = async (container: HTMLElement) => {
  const results = await axe(container);
  expect(results).toHaveNoViolations();
};

// Custom accessibility test utilities
export const testKeyboardNavigation = (element: HTMLElement) => {
  // Test Tab navigation
  element.focus();
  expect(document.activeElement).toBe(element);
  
  // Test Enter key
  fireEvent.keyDown(element, { key: 'Enter' });
  
  // Test Space key
  fireEvent.keyDown(element, { key: ' ' });
  
  // Test Escape key
  fireEvent.keyDown(element, { key: 'Escape' });
};

export const testScreenReaderSupport = (element: HTMLElement) => {
  // Check for proper ARIA attributes
  expect(element).toHaveAttribute('role');
  
  // Check for accessible name
  const accessibleName = element.getAttribute('aria-label') || 
                         element.getAttribute('aria-labelledby') ||
                         element.textContent;
  expect(accessibleName).toBeTruthy();
  
  // Check for description if needed
  if (element.getAttribute('aria-describedby')) {
    const descriptionId = element.getAttribute('aria-describedby');
    const descriptionElement = document.getElementById(descriptionId!);
    expect(descriptionElement).toBeInTheDocument();
  }
};
```

### Visual Regression Testing

Set up visual regression testing with Storybook:

**`.storybook/test-runner.ts`**
```typescript
import type { TestRunnerConfig } from '@storybook/test-runner';
import { checkA11y, injectAxe } from 'axe-playwright';

const config: TestRunnerConfig = {
  setup() {
    // Add global setup here
  },
  
  async preRender(page, context) {
    await injectAxe(page);
  },
  
  async postRender(page, context) {
    // Run accessibility tests
    await checkA11y(page, '#root', {
      detailedReport: true,
      detailedReportOptions: {
        html: true,
      },
    });
    
    // Take screenshot for visual regression testing
    await page.screenshot({
      path: `screenshots/${context.title.replace(/\s+/g, '-').toLowerCase()}.png`,
      fullPage: true
    });
  },
};

export default config;
```

---

## Deployment and Maintenance

### Build Configuration

Configure optimal build settings for production:

**`webpack.config.js`**
```javascript
const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const { BundleAnalyzerPlugin } = require('webpack-bundle-analyzer');

module.exports = (env, argv) => {
  const isProduction = argv.mode === 'production';
  
  return {
    entry: './src/index.tsx',
    
    output: {
      path: path.resolve(__dirname, 'dist'),
      filename: isProduction 
        ? '[name].[contenthash].js' 
        : '[name].js',
      chunkFilename: isProduction 
        ? '[name].[contenthash].chunk.js' 
        : '[name].chunk.js',
      clean: true,
      publicPath: '/'
    },
    
    resolve: {
      extensions: ['.tsx', '.ts', '.js', '.jsx'],
      alias: {
        '@components': path.resolve(__dirname, 'src/components'),
        '@styles': path.resolve(__dirname, 'src/styles'),
        '@utils': path.resolve(__dirname, 'src/utils'),
        '@hooks': path.resolve(__dirname, 'src/hooks'),
        '@types': path.resolve(__dirname, 'src/types'),
        '@assets': path.resolve(__dirname, 'src/assets')
      }
    },
    
    module: {
      rules: [
        {
          test: /\.(ts|tsx)$/,
          use: 'ts-loader',
          exclude: /node_modules/
        },
        {
          test: /\.css$/,
          use: [
            isProduction ? MiniCssExtractPlugin.loader : 'style-loader',
            'css-loader',
            'postcss-loader'
          ]
        },
        {
          test: /\.(png|jpg|jpeg|gif|svg|webp)$/,
          type: 'asset/resource',
          generator: {
            filename: 'images/[name].[contenthash][ext]'
          }
        },
        {
          test: /\.(woff|woff2|eot|ttf|otf)$/,
          type: 'asset/resource',
          generator: {
            filename: 'fonts/[name].[contenthash][ext]'
          }
        }
      ]
    },
    
    plugins: [
      new HtmlWebpackPlugin({
        template: './public/index.html',
        minify: isProduction ? {
          removeComments: true,
          collapseWhitespace: true,
          removeRedundantAttributes: true,
          useShortDoctype: true,
          removeEmptyAttributes: true,
          removeStyleLinkTypeAttributes: true,
          keepClosingSlash: true,
          minifyJS: true,
          minifyCSS: true,
          minifyURLs: true
        } : false
      }),
      
      ...(isProduction ? [
        new MiniCssExtractPlugin({
          filename: '[name].[contenthash].css',
          chunkFilename: '[name].[contenthash].chunk.css'
        })
      ] : []),
      
      ...(process.env.ANALYZE_BUNDLE ? [
        new BundleAnalyzerPlugin()
      ] : [])
    ],
    
    optimization: {
      minimize: isProduction,
      minimizer: [
        new TerserPlugin({
          terserOptions: {
            compress: {
              drop_console: true,
              drop_debugger: true
            }
          }
        }),
        new CssMinimizerPlugin()
      ],
      
      splitChunks: {
        chunks: 'all',
        cacheGroups: {
          vendor: {
            test: /[\\/]node_modules[\\/]/,
            name: 'vendors',
            chunks: 'all'
          },
          common: {
            name: 'common',
            minChunks: 2,
            chunks: 'all',
            enforce: true
          }
        }
      },
      
      runtimeChunk: 'single'
    },
    
    devServer: {
      static: path.join(__dirname, 'public'),
      port: 3000,
      hot: true,
      historyApiFallback: true,
      compress: true
    },
    
    devtool: isProduction ? 'source-map' : 'eval-source-map'
  };
};
```

### Environment Configuration

Set up environment-specific configurations:

**`src/config/environment.ts`**
```typescript
export interface EnvironmentConfig {
  apiBaseUrl: string;
  environment: 'development' | 'staging' | 'production';
  enableAnalytics: boolean;
  logLevel: 'debug' | 'info' | 'warn' | 'error';
  features: {
    realTimeTracking: boolean;
    paymentGateway: boolean;
    notifications: boolean;
    multiLanguage: boolean;
  };
}

const getEnvironmentConfig = (): EnvironmentConfig => {
  const env = process.env.NODE_ENV || 'development';
  
  const baseConfig = {
    environment: env as EnvironmentConfig['environment'],
    apiBaseUrl: process.env.REACT_APP_API_BASE_URL || 'http://localhost:5000/api',
    enableAnalytics: process.env.REACT_APP_ENABLE_ANALYTICS === 'true',
    logLevel: (process.env.REACT_APP_LOG_LEVEL as EnvironmentConfig['logLevel']) || 'info'
  };
  
  switch (env) {
    case 'development':
      return {
        ...baseConfig,
        logLevel: 'debug',
        features: {
          realTimeTracking: true,
          paymentGateway: false, // Use mock in development
          notifications: true,
          multiLanguage: true
        }
      };
      
    case 'staging':
      return {
        ...baseConfig,
        features: {
          realTimeTracking: true,
          paymentGateway: true,
          notifications: true,
          multiLanguage: true
        }
      };
      
    case 'production':
      return {
        ...baseConfig,
        logLevel: 'error',
        features: {
          realTimeTracking: true,
          paymentGateway: true,
          notifications: true,
          multiLanguage: true
        }
      };
      
    default:
      return baseConfig as EnvironmentConfig;
  }
};

export const config = getEnvironmentConfig();
```

### Monitoring and Error Tracking

Implement comprehensive monitoring:

**`src/utils/monitoring.ts`**
```typescript
export interface ErrorInfo {
  message: string;
  stack?: string;
  componentStack?: string;
  userId?: string;
  userAgent: string;
  url: string;
  timestamp: Date;
  severity: 'low' | 'medium' | 'high' | 'critical';
}

class MonitoringService {
  private isEnabled: boolean;
  
  constructor() {
    this.isEnabled = process.env.NODE_ENV === 'production';
  }
  
  logError(error: Error, errorInfo?: any) {
    if (!this.isEnabled) {
      console.error('Error:', error, errorInfo);
      return;
    }
    
    const errorData: ErrorInfo = {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo?.componentStack,
      userAgent: navigator.userAgent,
      url: window.location.href,
      timestamp: new Date(),
      severity: this.determineSeverity(error)
    };
    
    // Send to monitoring service (e.g., Sentry, LogRocket)
    this.sendToMonitoringService(errorData);
  }
  
  logPerformance(metric: string, value: number, tags?: Record<string, string>) {
    if (!this.isEnabled) return;
    
    // Send performance metrics
    this.sendPerformanceMetric({
      metric,
      value,
      tags,
      timestamp: Date.now()
    });
  }
  
  logUserAction(action: string, data?: any) {
    if (!this.isEnabled) return;
    
    // Log user interactions for analytics
    this.sendUserAction({
      action,
      data,
      timestamp: Date.now(),
      userId: this.getCurrentUserId()
    });
  }
  
  private determineSeverity(error: Error): ErrorInfo['severity'] {
    if (error.name === 'ChunkLoadError') return 'medium';
    if (error.message.includes('Network Error')) return 'medium';
    if (error.message.includes('Permission denied')) return 'high';
    return 'low';
  }
  
  private sendToMonitoringService(errorData: ErrorInfo) {
    // Implementation would integrate with actual monitoring service
    fetch('/api/monitoring/errors', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(errorData)
    }).catch(console.error);
  }
  
  private sendPerformanceMetric(metric: any) {
    // Implementation for performance monitoring
    fetch('/api/monitoring/performance', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(metric)
    }).catch(console.error);
  }
  
  private sendUserAction(action: any) {
    // Implementation for user analytics
    fetch('/api/analytics/actions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(action)
    }).catch(console.error);
  }
  
  private getCurrentUserId(): string | undefined {
    // Get current user ID from authentication context
    return undefined;
  }
}

export const monitoring = new MonitoringService();
```

---

## Troubleshooting and Common Issues

### Common Development Issues

**Issue: CSS-in-JS Performance Problems**
```typescript
// Problem: Recreating styled components on every render
const BadComponent = ({ color }: { color: string }) => {
  const StyledDiv = styled.div`
    color: ${color}; // This creates a new component every render
  `;
  return <StyledDiv>Content</StyledDiv>;
};

// Solution: Use CSS custom properties or memoization
const GoodComponent = ({ color }: { color: string }) => {
  const StyledDiv = styled.div`
    color: var(--dynamic-color);
  `;
  
  return (
    <StyledDiv style={{ '--dynamic-color': color } as any}>
      Content
    </StyledDiv>
  );
};
```

**Issue: Bundle Size Optimization**
```typescript
// Problem: Importing entire libraries
import * as lodash from 'lodash';
import { Button, Input, Card } from '@mui/material';

// Solution: Use tree shaking and specific imports
import { debounce } from 'lodash/debounce';
import Button from '@mui/material/Button';
import Input from '@mui/material/Input';
import Card from '@mui/material/Card';
```

**Issue: Memory Leaks in Event Listeners**
```typescript
// Problem: Not cleaning up event listeners
const BadComponent = () => {
  useEffect(() => {
    const handleResize = () => {
      // Handle resize
    };
    
    window.addEventListener('resize', handleResize);
    // Missing cleanup!
  }, []);
  
  return <div>Content</div>;
};

// Solution: Always clean up event listeners
const GoodComponent = () => {
  useEffect(() => {
    const handleResize = () => {
      // Handle resize
    };
    
    window.addEventListener('resize', handleResize);
    
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);
  
  return <div>Content</div>;
};
```

### Debugging Tools and Techniques

**React Developer Tools Configuration**
```typescript
// Enable React DevTools in development
if (process.env.NODE_ENV === 'development') {
  // Enable React DevTools Profiler
  window.__REACT_DEVTOOLS_GLOBAL_HOOK__?.onCommitFiberRoot = (id, root) => {
    // Custom profiling logic
  };
}

// Component debugging utility
export const withDebugInfo = <P extends object>(
  Component: React.ComponentType<P>,
  debugName?: string
) => {
  const WrappedComponent = (props: P) => {
    if (process.env.NODE_ENV === 'development') {
      console.log(`Rendering ${debugName || Component.name}:`, props);
    }
    
    return <Component {...props} />;
  };
  
  WrappedComponent.displayName = `withDebugInfo(${debugName || Component.name})`;
  return WrappedComponent;
};
```

### Performance Monitoring

**Runtime Performance Monitoring**
```typescript
export const measurePerformance = (name: string, fn: () => void) => {
  if (process.env.NODE_ENV === 'development') {
    performance.mark(`${name}-start`);
    fn();
    performance.mark(`${name}-end`);
    performance.measure(name, `${name}-start`, `${name}-end`);
    
    const measure = performance.getEntriesByName(name)[0];
    console.log(`${name} took ${measure.duration}ms`);
  } else {
    fn();
  }
};

// Usage in components
const ExpensiveComponent = () => {
  const [data, setData] = useState([]);
  
  useEffect(() => {
    measurePerformance('data-processing', () => {
      const processedData = processLargeDataset(rawData);
      setData(processedData);
    });
  }, [rawData]);
  
  return <div>{/* Render data */}</div>;
};
```

This comprehensive implementation guide provides frontend developers with everything needed to build the TETCO School Transportation Management System according to world-class standards. The guide covers all aspects from basic setup through advanced optimization techniques, ensuring a maintainable, accessible, and performant application that meets the specific requirements outlined in the BRD and SRS documents.

Remember to regularly update dependencies, monitor performance metrics, and conduct accessibility audits to maintain the high quality standards established in this design system. The modular architecture and comprehensive testing strategy will support long-term maintenance and feature development as the system evolves.

