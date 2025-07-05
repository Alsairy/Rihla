# TETCO Layout Specifications
## School Transportation Management System

### Grid System

#### Container System
```css
.container {
  width: 100%;
  margin-left: auto;
  margin-right: auto;
  padding-left: var(--grid-margin);
  padding-right: var(--grid-margin);
}

.container-sm {
  max-width: var(--container-sm);
}

.container-md {
  max-width: var(--container-md);
}

.container-lg {
  max-width: var(--container-lg);
}

.container-xl {
  max-width: var(--container-xl);
}

.container-2xl {
  max-width: var(--container-2xl);
}

.container-fluid {
  max-width: none;
}
```

#### Flexbox Grid System
```css
.row {
  display: flex;
  flex-wrap: wrap;
  margin-left: calc(var(--grid-gutter) / -2);
  margin-right: calc(var(--grid-gutter) / -2);
}

.col {
  flex: 1 0 0%;
  padding-left: calc(var(--grid-gutter) / 2);
  padding-right: calc(var(--grid-gutter) / 2);
}

/* Column sizes */
.col-1 { flex: 0 0 8.333333%; max-width: 8.333333%; }
.col-2 { flex: 0 0 16.666667%; max-width: 16.666667%; }
.col-3 { flex: 0 0 25%; max-width: 25%; }
.col-4 { flex: 0 0 33.333333%; max-width: 33.333333%; }
.col-5 { flex: 0 0 41.666667%; max-width: 41.666667%; }
.col-6 { flex: 0 0 50%; max-width: 50%; }
.col-7 { flex: 0 0 58.333333%; max-width: 58.333333%; }
.col-8 { flex: 0 0 66.666667%; max-width: 66.666667%; }
.col-9 { flex: 0 0 75%; max-width: 75%; }
.col-10 { flex: 0 0 83.333333%; max-width: 83.333333%; }
.col-11 { flex: 0 0 91.666667%; max-width: 91.666667%; }
.col-12 { flex: 0 0 100%; max-width: 100%; }

/* Responsive columns */
@media (min-width: 640px) {
  .col-sm-1 { flex: 0 0 8.333333%; max-width: 8.333333%; }
  .col-sm-2 { flex: 0 0 16.666667%; max-width: 16.666667%; }
  .col-sm-3 { flex: 0 0 25%; max-width: 25%; }
  .col-sm-4 { flex: 0 0 33.333333%; max-width: 33.333333%; }
  .col-sm-5 { flex: 0 0 41.666667%; max-width: 41.666667%; }
  .col-sm-6 { flex: 0 0 50%; max-width: 50%; }
  .col-sm-7 { flex: 0 0 58.333333%; max-width: 58.333333%; }
  .col-sm-8 { flex: 0 0 66.666667%; max-width: 66.666667%; }
  .col-sm-9 { flex: 0 0 75%; max-width: 75%; }
  .col-sm-10 { flex: 0 0 83.333333%; max-width: 83.333333%; }
  .col-sm-11 { flex: 0 0 91.666667%; max-width: 91.666667%; }
  .col-sm-12 { flex: 0 0 100%; max-width: 100%; }
}

@media (min-width: 768px) {
  .col-md-1 { flex: 0 0 8.333333%; max-width: 8.333333%; }
  .col-md-2 { flex: 0 0 16.666667%; max-width: 16.666667%; }
  .col-md-3 { flex: 0 0 25%; max-width: 25%; }
  .col-md-4 { flex: 0 0 33.333333%; max-width: 33.333333%; }
  .col-md-5 { flex: 0 0 41.666667%; max-width: 41.666667%; }
  .col-md-6 { flex: 0 0 50%; max-width: 50%; }
  .col-md-7 { flex: 0 0 58.333333%; max-width: 58.333333%; }
  .col-md-8 { flex: 0 0 66.666667%; max-width: 66.666667%; }
  .col-md-9 { flex: 0 0 75%; max-width: 75%; }
  .col-md-10 { flex: 0 0 83.333333%; max-width: 83.333333%; }
  .col-md-11 { flex: 0 0 91.666667%; max-width: 91.666667%; }
  .col-md-12 { flex: 0 0 100%; max-width: 100%; }
}

@media (min-width: 1024px) {
  .col-lg-1 { flex: 0 0 8.333333%; max-width: 8.333333%; }
  .col-lg-2 { flex: 0 0 16.666667%; max-width: 16.666667%; }
  .col-lg-3 { flex: 0 0 25%; max-width: 25%; }
  .col-lg-4 { flex: 0 0 33.333333%; max-width: 33.333333%; }
  .col-lg-5 { flex: 0 0 41.666667%; max-width: 41.666667%; }
  .col-lg-6 { flex: 0 0 50%; max-width: 50%; }
  .col-lg-7 { flex: 0 0 58.333333%; max-width: 58.333333%; }
  .col-lg-8 { flex: 0 0 66.666667%; max-width: 66.666667%; }
  .col-lg-9 { flex: 0 0 75%; max-width: 75%; }
  .col-lg-10 { flex: 0 0 83.333333%; max-width: 83.333333%; }
  .col-lg-11 { flex: 0 0 91.666667%; max-width: 91.666667%; }
  .col-lg-12 { flex: 0 0 100%; max-width: 100%; }
}

@media (min-width: 1280px) {
  .col-xl-1 { flex: 0 0 8.333333%; max-width: 8.333333%; }
  .col-xl-2 { flex: 0 0 16.666667%; max-width: 16.666667%; }
  .col-xl-3 { flex: 0 0 25%; max-width: 25%; }
  .col-xl-4 { flex: 0 0 33.333333%; max-width: 33.333333%; }
  .col-xl-5 { flex: 0 0 41.666667%; max-width: 41.666667%; }
  .col-xl-6 { flex: 0 0 50%; max-width: 50%; }
  .col-xl-7 { flex: 0 0 58.333333%; max-width: 58.333333%; }
  .col-xl-8 { flex: 0 0 66.666667%; max-width: 66.666667%; }
  .col-xl-9 { flex: 0 0 75%; max-width: 75%; }
  .col-xl-10 { flex: 0 0 83.333333%; max-width: 83.333333%; }
  .col-xl-11 { flex: 0 0 91.666667%; max-width: 91.666667%; }
  .col-xl-12 { flex: 0 0 100%; max-width: 100%; }
}
```

### Layout Patterns

#### Application Shell Layout
```css
.app-shell {
  display: flex;
  min-height: 100vh;
  background-color: var(--color-background-secondary);
}

.app-sidebar {
  flex: 0 0 auto;
  width: 16rem;
  background-color: var(--color-background-primary);
  border-right: 1px solid var(--color-border-primary);
  box-shadow: var(--shadow-sm);
  z-index: var(--z-index-docked);
  
  @media (max-width: 1023px) {
    position: fixed;
    top: 0;
    left: 0;
    height: 100vh;
    transform: translateX(-100%);
    transition: transform var(--duration-normal) var(--easing-smooth);
    
    &.open {
      transform: translateX(0);
    }
  }
}

.app-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  
  @media (max-width: 1023px) {
    margin-left: 0;
  }
}

.app-header {
  flex: 0 0 auto;
  background-color: var(--color-background-primary);
  border-bottom: 1px solid var(--color-border-primary);
  box-shadow: var(--shadow-sm);
  z-index: var(--z-index-sticky);
}

.app-content {
  flex: 1;
  padding: var(--spacing-6);
  overflow-y: auto;
}
```

#### Dashboard Layout
```css
.dashboard {
  display: grid;
  gap: var(--spacing-6);
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  
  @media (min-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }
  
  @media (min-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);
  }
  
  @media (min-width: 1280px) {
    grid-template-columns: repeat(4, 1fr);
  }
}

.dashboard-widget {
  background-color: var(--color-background-primary);
  border-radius: var(--card-border-radius);
  box-shadow: var(--card-shadow);
  padding: var(--card-padding);
  
  &.widget-full {
    grid-column: 1 / -1;
  }
  
  &.widget-half {
    @media (min-width: 768px) {
      grid-column: span 2;
    }
  }
  
  &.widget-large {
    @media (min-width: 1024px) {
      grid-column: span 2;
      grid-row: span 2;
    }
  }
}
```

#### Form Layout
```css
.form-layout {
  max-width: 42rem;
  margin: 0 auto;
  
  .form-section {
    margin-bottom: var(--spacing-8);
    
    .form-section-title {
      font-size: var(--font-size-lg);
      font-weight: var(--font-weight-bold);
      color: var(--color-text-primary);
      margin-bottom: var(--spacing-4);
      padding-bottom: var(--spacing-2);
      border-bottom: 1px solid var(--color-border-secondary);
    }
    
    .form-section-description {
      color: var(--color-text-secondary);
      margin-bottom: var(--spacing-6);
    }
  }
  
  .form-row {
    display: grid;
    gap: var(--spacing-4);
    margin-bottom: var(--spacing-4);
    
    @media (min-width: 768px) {
      grid-template-columns: repeat(2, 1fr);
      
      &.form-row-single {
        grid-template-columns: 1fr;
      }
      
      &.form-row-thirds {
        grid-template-columns: repeat(3, 1fr);
      }
    }
  }
  
  .form-actions {
    display: flex;
    gap: var(--spacing-3);
    justify-content: flex-end;
    padding-top: var(--spacing-6);
    border-top: 1px solid var(--color-border-secondary);
    
    @media (max-width: 767px) {
      flex-direction: column;
      
      .btn {
        width: 100%;
      }
    }
  }
}
```

#### List Layout
```css
.list-layout {
  .list-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--spacing-6);
    
    .list-title {
      font-size: var(--font-size-2xl);
      font-weight: var(--font-weight-bold);
      color: var(--color-text-primary);
      margin: 0;
    }
    
    .list-actions {
      display: flex;
      gap: var(--spacing-3);
      
      @media (max-width: 767px) {
        flex-direction: column;
        width: 100%;
        
        .btn {
          width: 100%;
        }
      }
    }
    
    @media (max-width: 767px) {
      flex-direction: column;
      align-items: stretch;
      gap: var(--spacing-4);
    }
  }
  
  .list-filters {
    background-color: var(--color-background-primary);
    border-radius: var(--border-radius-lg);
    padding: var(--spacing-4);
    margin-bottom: var(--spacing-6);
    box-shadow: var(--shadow-sm);
    
    .filter-row {
      display: grid;
      gap: var(--spacing-4);
      align-items: end;
      
      @media (min-width: 768px) {
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)) auto;
      }
    }
  }
  
  .list-content {
    background-color: var(--color-background-primary);
    border-radius: var(--border-radius-lg);
    box-shadow: var(--shadow-base);
    overflow: hidden;
  }
}
```

### Responsive Design Guidelines

#### Mobile-First Approach
```css
/* Base styles (mobile) */
.responsive-element {
  font-size: var(--font-size-sm);
  padding: var(--spacing-3);
}

/* Small screens and up */
@media (min-width: 640px) {
  .responsive-element {
    font-size: var(--font-size-base);
    padding: var(--spacing-4);
  }
}

/* Medium screens and up */
@media (min-width: 768px) {
  .responsive-element {
    font-size: var(--font-size-lg);
    padding: var(--spacing-5);
  }
}

/* Large screens and up */
@media (min-width: 1024px) {
  .responsive-element {
    font-size: var(--font-size-xl);
    padding: var(--spacing-6);
  }
}

/* Extra large screens and up */
@media (min-width: 1280px) {
  .responsive-element {
    font-size: var(--font-size-2xl);
    padding: var(--spacing-8);
  }
}
```

#### Responsive Utilities
```css
/* Display utilities */
.d-block { display: block; }
.d-none { display: none; }

@media (min-width: 640px) {
  .d-sm-block { display: block; }
  .d-sm-none { display: none; }
}

@media (min-width: 768px) {
  .d-md-block { display: block; }
  .d-md-none { display: none; }
}

@media (min-width: 1024px) {
  .d-lg-block { display: block; }
  .d-lg-none { display: none; }
}

@media (min-width: 1280px) {
  .d-xl-block { display: block; }
  .d-xl-none { display: none; }
}

/* Text alignment utilities */
.text-left { text-align: left; }
.text-center { text-align: center; }
.text-right { text-align: right; }

@media (min-width: 640px) {
  .text-sm-left { text-align: left; }
  .text-sm-center { text-align: center; }
  .text-sm-right { text-align: right; }
}

@media (min-width: 768px) {
  .text-md-left { text-align: left; }
  .text-md-center { text-align: center; }
  .text-md-right { text-align: right; }
}

@media (min-width: 1024px) {
  .text-lg-left { text-align: left; }
  .text-lg-center { text-align: center; }
  .text-lg-right { text-align: right; }
}
```

### RTL Layout Support

#### RTL Base Styles
```css
[dir="rtl"] {
  .app-sidebar {
    border-right: none;
    border-left: 1px solid var(--color-border-primary);
    
    @media (max-width: 1023px) {
      left: auto;
      right: 0;
      transform: translateX(100%);
      
      &.open {
        transform: translateX(0);
      }
    }
  }
  
  .sidebar-nav {
    .nav-item {
      .nav-link {
        &.active {
          border-right: none;
          border-left: 3px solid var(--color-primary);
        }
      }
    }
  }
  
  .form-actions {
    justify-content: flex-start;
  }
  
  .list-header {
    .list-actions {
      order: -1;
    }
  }
}
```

#### RTL Grid System
```css
[dir="rtl"] {
  .row {
    margin-left: calc(var(--grid-gutter) / -2);
    margin-right: calc(var(--grid-gutter) / -2);
  }
  
  .col {
    padding-left: calc(var(--grid-gutter) / 2);
    padding-right: calc(var(--grid-gutter) / 2);
  }
}
```

### Layout Components

#### Page Header
```css
.page-header {
  margin-bottom: var(--spacing-8);
  
  .page-title {
    font-size: var(--font-size-3xl);
    font-weight: var(--font-weight-bold);
    color: var(--color-text-primary);
    margin: 0 0 var(--spacing-2) 0;
    
    @media (max-width: 767px) {
      font-size: var(--font-size-2xl);
    }
  }
  
  .page-subtitle {
    font-size: var(--font-size-lg);
    color: var(--color-text-secondary);
    margin: 0 0 var(--spacing-4) 0;
    
    @media (max-width: 767px) {
      font-size: var(--font-size-base);
    }
  }
  
  .page-actions {
    display: flex;
    gap: var(--spacing-3);
    
    @media (max-width: 767px) {
      flex-direction: column;
      
      .btn {
        width: 100%;
      }
    }
  }
  
  &.page-header-with-actions {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    
    @media (max-width: 767px) {
      flex-direction: column;
      gap: var(--spacing-4);
    }
  }
}
```

#### Content Section
```css
.content-section {
  background-color: var(--color-background-primary);
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-base);
  margin-bottom: var(--spacing-6);
  overflow: hidden;
  
  .section-header {
    padding: var(--spacing-6);
    border-bottom: 1px solid var(--color-border-secondary);
    
    .section-title {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-bold);
      color: var(--color-text-primary);
      margin: 0 0 var(--spacing-1) 0;
    }
    
    .section-description {
      color: var(--color-text-secondary);
      margin: 0;
    }
  }
  
  .section-content {
    padding: var(--spacing-6);
  }
  
  .section-footer {
    padding: var(--spacing-6);
    border-top: 1px solid var(--color-border-secondary);
    background-color: var(--color-background-secondary);
  }
}
```

#### Stats Grid
```css
.stats-grid {
  display: grid;
  gap: var(--spacing-4);
  grid-template-columns: 1fr;
  
  @media (min-width: 640px) {
    grid-template-columns: repeat(2, 1fr);
  }
  
  @media (min-width: 1024px) {
    grid-template-columns: repeat(4, 1fr);
  }
}

.stat-card {
  background-color: var(--color-background-primary);
  border-radius: var(--border-radius-lg);
  padding: var(--spacing-6);
  box-shadow: var(--shadow-base);
  text-align: center;
  
  .stat-value {
    font-size: var(--font-size-3xl);
    font-weight: var(--font-weight-bold);
    color: var(--color-primary);
    margin: 0 0 var(--spacing-2) 0;
    
    @media (max-width: 767px) {
      font-size: var(--font-size-2xl);
    }
  }
  
  .stat-label {
    font-size: var(--font-size-sm);
    color: var(--color-text-secondary);
    margin: 0;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }
  
  .stat-change {
    font-size: var(--font-size-xs);
    margin-top: var(--spacing-2);
    
    &.positive {
      color: var(--color-success);
    }
    
    &.negative {
      color: var(--color-error);
    }
  }
}
```

### Accessibility Layout Guidelines

#### Focus Management
```css
.skip-link {
  position: absolute;
  top: -40px;
  left: 6px;
  background: var(--color-primary);
  color: var(--color-text-inverse);
  padding: 8px;
  text-decoration: none;
  border-radius: var(--border-radius-base);
  z-index: var(--z-index-skipLink);
  
  &:focus {
    top: 6px;
  }
}

.focus-trap {
  &:focus {
    outline: 2px solid var(--color-primary);
    outline-offset: 2px;
  }
}
```

#### Screen Reader Support
```css
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.sr-only-focusable:focus {
  position: static;
  width: auto;
  height: auto;
  padding: inherit;
  margin: inherit;
  overflow: visible;
  clip: auto;
  white-space: normal;
}
```

### Print Styles

```css
@media print {
  .no-print {
    display: none !important;
  }
  
  .page-break-before {
    page-break-before: always;
  }
  
  .page-break-after {
    page-break-after: always;
  }
  
  .page-break-inside-avoid {
    page-break-inside: avoid;
  }
  
  body {
    font-size: 12pt;
    line-height: 1.4;
    color: #000;
    background: #fff;
  }
  
  .app-sidebar,
  .app-header {
    display: none;
  }
  
  .app-content {
    padding: 0;
  }
  
  .card {
    box-shadow: none;
    border: 1px solid #ccc;
  }
  
  .btn {
    display: none;
  }
  
  a[href]:after {
    content: " (" attr(href) ")";
  }
  
  a[href^="#"]:after,
  a[href^="javascript:"]:after {
    content: "";
  }
}
```

These layout specifications provide a comprehensive foundation for creating consistent, responsive, and accessible layouts throughout the School Transportation Management System while maintaining the TETCO brand identity.

