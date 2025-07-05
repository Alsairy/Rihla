# TETCO Design Tokens
## School Transportation Management System

### Color Tokens

#### Primary Colors
```css
:root {
  /* Primary Brand Colors */
  --color-primary-blue: #005F96;
  --color-primary-green: #36BA91;
  --color-light-green: #4CFCB4;
  --color-dark-teal: #0C3C44;
  --color-light-gray: #D1D1D1;
  
  /* RGB Values for JavaScript */
  --color-primary-blue-rgb: 0, 95, 150;
  --color-primary-green-rgb: 54, 186, 145;
  --color-light-green-rgb: 76, 252, 180;
  --color-dark-teal-rgb: 12, 60, 68;
  --color-light-gray-rgb: 209, 209, 209;
}
```

#### Semantic Color System
```css
:root {
  /* Semantic Colors */
  --color-primary: var(--color-primary-blue);
  --color-secondary: var(--color-primary-green);
  --color-accent: var(--color-light-green);
  --color-neutral: var(--color-light-gray);
  --color-text-primary: var(--color-dark-teal);
  
  /* State Colors */
  --color-success: var(--color-primary-green);
  --color-success-light: var(--color-light-green);
  --color-warning: #FFA726;
  --color-error: #EF5350;
  --color-info: var(--color-primary-blue);
  
  /* Background Colors */
  --color-background-primary: #FFFFFF;
  --color-background-secondary: #F8F9FA;
  --color-background-tertiary: var(--color-light-gray);
  --color-background-overlay: rgba(12, 60, 68, 0.8);
  
  /* Text Colors */
  --color-text-primary: var(--color-dark-teal);
  --color-text-secondary: rgba(12, 60, 68, 0.7);
  --color-text-tertiary: rgba(12, 60, 68, 0.5);
  --color-text-inverse: #FFFFFF;
  --color-text-link: var(--color-primary-blue);
  --color-text-link-hover: rgba(0, 95, 150, 0.8);
  
  /* Border Colors */
  --color-border-primary: var(--color-light-gray);
  --color-border-secondary: rgba(209, 209, 209, 0.5);
  --color-border-focus: var(--color-primary-blue);
  --color-border-error: var(--color-error);
  --color-border-success: var(--color-success);
}
```

### Typography Tokens

#### Font Families
```css
:root {
  /* Font Families */
  --font-family-primary: 'EFFRA', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  --font-family-arabic: 'EFFRA', 'Noto Sans Arabic', 'Tahoma', sans-serif;
  --font-family-monospace: 'SF Mono', Monaco, 'Cascadia Code', 'Roboto Mono', monospace;
}
```

#### Font Weights
```css
:root {
  /* Font Weights */
  --font-weight-light: 300;
  --font-weight-regular: 400;
  --font-weight-medium: 500;
  --font-weight-bold: 700;
  --font-weight-heavy: 900;
}
```

#### Font Sizes
```css
:root {
  /* Font Sizes */
  --font-size-xs: 0.75rem;    /* 12px */
  --font-size-sm: 0.875rem;   /* 14px */
  --font-size-base: 1rem;     /* 16px */
  --font-size-lg: 1.125rem;   /* 18px */
  --font-size-xl: 1.25rem;    /* 20px */
  --font-size-2xl: 1.5rem;    /* 24px */
  --font-size-3xl: 1.875rem;  /* 30px */
  --font-size-4xl: 2.25rem;   /* 36px */
  --font-size-5xl: 3rem;      /* 48px */
  --font-size-6xl: 3.75rem;   /* 60px */
}
```

#### Line Heights
```css
:root {
  /* Line Heights */
  --line-height-tight: 1.25;
  --line-height-normal: 1.5;
  --line-height-relaxed: 1.75;
  --line-height-loose: 2;
}
```

### Spacing Tokens

#### Spacing Scale
```css
:root {
  /* Spacing Scale */
  --spacing-0: 0;
  --spacing-1: 0.25rem;   /* 4px */
  --spacing-2: 0.5rem;    /* 8px */
  --spacing-3: 0.75rem;   /* 12px */
  --spacing-4: 1rem;      /* 16px */
  --spacing-5: 1.25rem;   /* 20px */
  --spacing-6: 1.5rem;    /* 24px */
  --spacing-8: 2rem;      /* 32px */
  --spacing-10: 2.5rem;   /* 40px */
  --spacing-12: 3rem;     /* 48px */
  --spacing-16: 4rem;     /* 64px */
  --spacing-20: 5rem;     /* 80px */
  --spacing-24: 6rem;     /* 96px */
  --spacing-32: 8rem;     /* 128px */
}
```

#### Component Spacing
```css
:root {
  /* Component Spacing */
  --spacing-component-xs: var(--spacing-2);
  --spacing-component-sm: var(--spacing-3);
  --spacing-component-md: var(--spacing-4);
  --spacing-component-lg: var(--spacing-6);
  --spacing-component-xl: var(--spacing-8);
}
```

### Border Radius Tokens

```css
:root {
  /* Border Radius */
  --border-radius-none: 0;
  --border-radius-sm: 0.125rem;   /* 2px */
  --border-radius-base: 0.25rem;  /* 4px */
  --border-radius-md: 0.375rem;   /* 6px */
  --border-radius-lg: 0.5rem;     /* 8px */
  --border-radius-xl: 0.75rem;    /* 12px */
  --border-radius-2xl: 1rem;      /* 16px */
  --border-radius-full: 9999px;
}
```

### Shadow Tokens

```css
:root {
  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgba(12, 60, 68, 0.05);
  --shadow-base: 0 1px 3px 0 rgba(12, 60, 68, 0.1), 0 1px 2px 0 rgba(12, 60, 68, 0.06);
  --shadow-md: 0 4px 6px -1px rgba(12, 60, 68, 0.1), 0 2px 4px -1px rgba(12, 60, 68, 0.06);
  --shadow-lg: 0 10px 15px -3px rgba(12, 60, 68, 0.1), 0 4px 6px -2px rgba(12, 60, 68, 0.05);
  --shadow-xl: 0 20px 25px -5px rgba(12, 60, 68, 0.1), 0 10px 10px -5px rgba(12, 60, 68, 0.04);
  --shadow-2xl: 0 25px 50px -12px rgba(12, 60, 68, 0.25);
  --shadow-inner: inset 0 2px 4px 0 rgba(12, 60, 68, 0.06);
}
```

### Z-Index Tokens

```css
:root {
  /* Z-Index Scale */
  --z-index-hide: -1;
  --z-index-auto: auto;
  --z-index-base: 0;
  --z-index-docked: 10;
  --z-index-dropdown: 1000;
  --z-index-sticky: 1100;
  --z-index-banner: 1200;
  --z-index-overlay: 1300;
  --z-index-modal: 1400;
  --z-index-popover: 1500;
  --z-index-skipLink: 1600;
  --z-index-toast: 1700;
  --z-index-tooltip: 1800;
}
```

### Animation Tokens

```css
:root {
  /* Animation Duration */
  --duration-instant: 0ms;
  --duration-fast: 150ms;
  --duration-normal: 300ms;
  --duration-slow: 500ms;
  --duration-slower: 750ms;
  
  /* Animation Easing */
  --easing-linear: linear;
  --easing-ease: ease;
  --easing-ease-in: ease-in;
  --easing-ease-out: ease-out;
  --easing-ease-in-out: ease-in-out;
  --easing-bounce: cubic-bezier(0.68, -0.55, 0.265, 1.55);
  --easing-smooth: cubic-bezier(0.4, 0, 0.2, 1);
}
```

### Breakpoint Tokens

```css
:root {
  /* Breakpoints */
  --breakpoint-xs: 320px;
  --breakpoint-sm: 640px;
  --breakpoint-md: 768px;
  --breakpoint-lg: 1024px;
  --breakpoint-xl: 1280px;
  --breakpoint-2xl: 1536px;
}
```

### Grid Tokens

```css
:root {
  /* Grid System */
  --grid-columns: 12;
  --grid-gutter: var(--spacing-6);
  --grid-margin: var(--spacing-4);
  
  /* Container Max Widths */
  --container-sm: 640px;
  --container-md: 768px;
  --container-lg: 1024px;
  --container-xl: 1280px;
  --container-2xl: 1536px;
}
```

### Component-Specific Tokens

#### Button Tokens
```css
:root {
  /* Button Tokens */
  --button-height-sm: 2rem;
  --button-height-md: 2.5rem;
  --button-height-lg: 3rem;
  --button-height-xl: 3.5rem;
  
  --button-padding-x-sm: var(--spacing-3);
  --button-padding-x-md: var(--spacing-4);
  --button-padding-x-lg: var(--spacing-6);
  --button-padding-x-xl: var(--spacing-8);
  
  --button-border-radius: var(--border-radius-md);
  --button-font-weight: var(--font-weight-medium);
}
```

#### Form Tokens
```css
:root {
  /* Form Tokens */
  --form-input-height: 2.5rem;
  --form-input-padding-x: var(--spacing-3);
  --form-input-padding-y: var(--spacing-2);
  --form-input-border-radius: var(--border-radius-base);
  --form-input-border-width: 1px;
  --form-input-focus-ring-width: 2px;
  --form-input-focus-ring-offset: 2px;
}
```

#### Card Tokens
```css
:root {
  /* Card Tokens */
  --card-padding: var(--spacing-6);
  --card-border-radius: var(--border-radius-lg);
  --card-border-width: 1px;
  --card-shadow: var(--shadow-base);
  --card-shadow-hover: var(--shadow-md);
}
```

### RTL Support Tokens

```css
:root {
  /* RTL Support */
  --direction: ltr;
  --text-align-start: left;
  --text-align-end: right;
  --border-start: border-left;
  --border-end: border-right;
  --margin-start: margin-left;
  --margin-end: margin-right;
  --padding-start: padding-left;
  --padding-end: padding-right;
}

[dir="rtl"] {
  --direction: rtl;
  --text-align-start: right;
  --text-align-end: left;
  --border-start: border-right;
  --border-end: border-left;
  --margin-start: margin-right;
  --margin-end: margin-left;
  --padding-start: padding-right;
  --padding-end: padding-left;
}
```

### Usage Guidelines

#### Implementation Notes
1. **CSS Custom Properties**: All tokens are defined as CSS custom properties for maximum flexibility
2. **Semantic Naming**: Use semantic color names (primary, secondary) rather than literal color names
3. **Consistent Spacing**: Use the spacing scale consistently across all components
4. **Responsive Design**: Leverage breakpoint tokens for consistent responsive behavior
5. **RTL Support**: Use logical properties and RTL tokens for proper Arabic language support

#### JavaScript Integration
```javascript
// Example: Accessing design tokens in JavaScript
const tokens = {
  colors: {
    primary: 'var(--color-primary)',
    secondary: 'var(--color-secondary)',
    // ... other colors
  },
  spacing: {
    sm: 'var(--spacing-component-sm)',
    md: 'var(--spacing-component-md)',
    // ... other spacing values
  }
};
```

#### SCSS Integration
```scss
// Example: Using design tokens in SCSS
@import 'design-tokens';

.button {
  height: var(--button-height-md);
  padding: 0 var(--button-padding-x-md);
  border-radius: var(--button-border-radius);
  font-weight: var(--button-font-weight);
  background-color: var(--color-primary);
  color: var(--color-text-inverse);
  
  &:hover {
    background-color: rgba(var(--color-primary-rgb), 0.9);
  }
}
```

These design tokens provide a comprehensive foundation for implementing the TETCO brand identity consistently across the entire School Transportation Management System.

