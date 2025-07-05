# TETCO Component Library
## School Transportation Management System

### Button Components

#### Primary Button
```css
.btn-primary {
  /* Base Styles */
  display: inline-flex;
  align-items: center;
  justify-content: center;
  height: var(--button-height-md);
  padding: 0 var(--button-padding-x-md);
  border: none;
  border-radius: var(--button-border-radius);
  font-family: var(--font-family-primary);
  font-size: var(--font-size-base);
  font-weight: var(--button-font-weight);
  line-height: var(--line-height-normal);
  text-decoration: none;
  cursor: pointer;
  transition: all var(--duration-fast) var(--easing-smooth);
  
  /* Primary Styling */
  background-color: var(--color-primary);
  color: var(--color-text-inverse);
  box-shadow: var(--shadow-sm);
  
  /* States */
  &:hover {
    background-color: rgba(var(--color-primary-rgb), 0.9);
    box-shadow: var(--shadow-md);
    transform: translateY(-1px);
  }
  
  &:active {
    background-color: rgba(var(--color-primary-rgb), 0.8);
    box-shadow: var(--shadow-sm);
    transform: translateY(0);
  }
  
  &:focus {
    outline: none;
    box-shadow: 0 0 0 var(--form-input-focus-ring-width) rgba(var(--color-primary-rgb), 0.3);
  }
  
  &:disabled {
    background-color: var(--color-neutral);
    color: var(--color-text-tertiary);
    cursor: not-allowed;
    box-shadow: none;
    transform: none;
  }
}
```

#### Secondary Button
```css
.btn-secondary {
  /* Inherits from btn-primary */
  @extend .btn-primary;
  
  /* Secondary Styling */
  background-color: transparent;
  color: var(--color-primary);
  border: 1px solid var(--color-primary);
  
  &:hover {
    background-color: var(--color-primary);
    color: var(--color-text-inverse);
  }
  
  &:active {
    background-color: rgba(var(--color-primary-rgb), 0.8);
    color: var(--color-text-inverse);
  }
}
```

#### Success Button
```css
.btn-success {
  @extend .btn-primary;
  background-color: var(--color-success);
  
  &:hover {
    background-color: rgba(var(--color-primary-green-rgb), 0.9);
  }
  
  &:active {
    background-color: rgba(var(--color-primary-green-rgb), 0.8);
  }
}
```

#### Button Sizes
```css
.btn-sm {
  height: var(--button-height-sm);
  padding: 0 var(--button-padding-x-sm);
  font-size: var(--font-size-sm);
}

.btn-lg {
  height: var(--button-height-lg);
  padding: 0 var(--button-padding-x-lg);
  font-size: var(--font-size-lg);
}

.btn-xl {
  height: var(--button-height-xl);
  padding: 0 var(--button-padding-x-xl);
  font-size: var(--font-size-xl);
}
```

#### Button with Icon
```css
.btn-icon {
  gap: var(--spacing-2);
  
  .icon {
    width: 1em;
    height: 1em;
    flex-shrink: 0;
  }
}

.btn-icon-only {
  width: var(--button-height-md);
  padding: 0;
  
  &.btn-sm {
    width: var(--button-height-sm);
  }
  
  &.btn-lg {
    width: var(--button-height-lg);
  }
  
  &.btn-xl {
    width: var(--button-height-xl);
  }
}
```

### Form Components

#### Input Field
```css
.form-input {
  /* Base Styles */
  width: 100%;
  height: var(--form-input-height);
  padding: var(--form-input-padding-y) var(--form-input-padding-x);
  border: var(--form-input-border-width) solid var(--color-border-primary);
  border-radius: var(--form-input-border-radius);
  font-family: var(--font-family-primary);
  font-size: var(--font-size-base);
  line-height: var(--line-height-normal);
  color: var(--color-text-primary);
  background-color: var(--color-background-primary);
  transition: all var(--duration-fast) var(--easing-smooth);
  
  /* States */
  &:hover {
    border-color: var(--color-border-focus);
  }
  
  &:focus {
    outline: none;
    border-color: var(--color-border-focus);
    box-shadow: 0 0 0 var(--form-input-focus-ring-width) rgba(var(--color-primary-rgb), 0.1);
  }
  
  &:invalid {
    border-color: var(--color-border-error);
  }
  
  &:disabled {
    background-color: var(--color-background-tertiary);
    color: var(--color-text-tertiary);
    cursor: not-allowed;
  }
  
  &::placeholder {
    color: var(--color-text-tertiary);
  }
}
```

#### Select Dropdown
```css
.form-select {
  @extend .form-input;
  background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 20 20'%3e%3cpath stroke='%236b7280' stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='m6 8 4 4 4-4'/%3e%3c/svg%3e");
  background-position: right var(--form-input-padding-x) center;
  background-repeat: no-repeat;
  background-size: 1.5em 1.5em;
  padding-right: calc(var(--form-input-padding-x) + 2em);
  cursor: pointer;
}
```

#### Checkbox
```css
.form-checkbox {
  appearance: none;
  width: 1.25rem;
  height: 1.25rem;
  border: var(--form-input-border-width) solid var(--color-border-primary);
  border-radius: var(--border-radius-sm);
  background-color: var(--color-background-primary);
  cursor: pointer;
  position: relative;
  transition: all var(--duration-fast) var(--easing-smooth);
  
  &:checked {
    background-color: var(--color-primary);
    border-color: var(--color-primary);
  }
  
  &:checked::after {
    content: '';
    position: absolute;
    top: 2px;
    left: 6px;
    width: 4px;
    height: 8px;
    border: 2px solid white;
    border-top: 0;
    border-left: 0;
    transform: rotate(45deg);
  }
  
  &:focus {
    outline: none;
    box-shadow: 0 0 0 var(--form-input-focus-ring-width) rgba(var(--color-primary-rgb), 0.3);
  }
}
```

#### Radio Button
```css
.form-radio {
  appearance: none;
  width: 1.25rem;
  height: 1.25rem;
  border: var(--form-input-border-width) solid var(--color-border-primary);
  border-radius: var(--border-radius-full);
  background-color: var(--color-background-primary);
  cursor: pointer;
  position: relative;
  transition: all var(--duration-fast) var(--easing-smooth);
  
  &:checked {
    background-color: var(--color-primary);
    border-color: var(--color-primary);
  }
  
  &:checked::after {
    content: '';
    position: absolute;
    top: 3px;
    left: 3px;
    width: 8px;
    height: 8px;
    border-radius: var(--border-radius-full);
    background-color: white;
  }
  
  &:focus {
    outline: none;
    box-shadow: 0 0 0 var(--form-input-focus-ring-width) rgba(var(--color-primary-rgb), 0.3);
  }
}
```

#### Form Group
```css
.form-group {
  margin-bottom: var(--spacing-4);
  
  .form-label {
    display: block;
    margin-bottom: var(--spacing-2);
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-text-primary);
  }
  
  .form-help {
    margin-top: var(--spacing-1);
    font-size: var(--font-size-xs);
    color: var(--color-text-secondary);
  }
  
  .form-error {
    margin-top: var(--spacing-1);
    font-size: var(--font-size-xs);
    color: var(--color-error);
  }
  
  &.has-error {
    .form-input,
    .form-select {
      border-color: var(--color-border-error);
    }
    
    .form-label {
      color: var(--color-error);
    }
  }
}
```

### Card Components

#### Base Card
```css
.card {
  background-color: var(--color-background-primary);
  border: var(--card-border-width) solid var(--color-border-primary);
  border-radius: var(--card-border-radius);
  box-shadow: var(--card-shadow);
  transition: all var(--duration-normal) var(--easing-smooth);
  
  &:hover {
    box-shadow: var(--card-shadow-hover);
  }
}

.card-header {
  padding: var(--card-padding);
  border-bottom: 1px solid var(--color-border-secondary);
  
  .card-title {
    margin: 0;
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-bold);
    color: var(--color-text-primary);
  }
  
  .card-subtitle {
    margin: var(--spacing-1) 0 0 0;
    font-size: var(--font-size-sm);
    color: var(--color-text-secondary);
  }
}

.card-body {
  padding: var(--card-padding);
}

.card-footer {
  padding: var(--card-padding);
  border-top: 1px solid var(--color-border-secondary);
  background-color: var(--color-background-secondary);
}
```

#### Interactive Card
```css
.card-interactive {
  @extend .card;
  cursor: pointer;
  
  &:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-lg);
  }
  
  &:active {
    transform: translateY(0);
    box-shadow: var(--card-shadow);
  }
}
```

### Navigation Components

#### Navigation Bar
```css
.navbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-4) var(--spacing-6);
  background-color: var(--color-primary);
  color: var(--color-text-inverse);
  box-shadow: var(--shadow-md);
  
  .navbar-brand {
    display: flex;
    align-items: center;
    gap: var(--spacing-3);
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-bold);
    text-decoration: none;
    color: inherit;
    
    .navbar-logo {
      height: 2rem;
      width: auto;
    }
  }
  
  .navbar-nav {
    display: flex;
    align-items: center;
    gap: var(--spacing-6);
    list-style: none;
    margin: 0;
    padding: 0;
  }
  
  .navbar-item {
    .navbar-link {
      color: inherit;
      text-decoration: none;
      font-weight: var(--font-weight-medium);
      padding: var(--spacing-2) var(--spacing-3);
      border-radius: var(--border-radius-base);
      transition: all var(--duration-fast) var(--easing-smooth);
      
      &:hover {
        background-color: rgba(255, 255, 255, 0.1);
      }
      
      &.active {
        background-color: rgba(255, 255, 255, 0.2);
      }
    }
  }
}
```

#### Sidebar Navigation
```css
.sidebar {
  width: 16rem;
  height: 100vh;
  background-color: var(--color-background-primary);
  border-right: 1px solid var(--color-border-primary);
  box-shadow: var(--shadow-sm);
  
  .sidebar-header {
    padding: var(--spacing-6);
    border-bottom: 1px solid var(--color-border-secondary);
  }
  
  .sidebar-nav {
    padding: var(--spacing-4) 0;
    
    .nav-group {
      margin-bottom: var(--spacing-6);
      
      .nav-group-title {
        padding: 0 var(--spacing-6) var(--spacing-2);
        font-size: var(--font-size-xs);
        font-weight: var(--font-weight-bold);
        color: var(--color-text-tertiary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
      }
      
      .nav-item {
        .nav-link {
          display: flex;
          align-items: center;
          gap: var(--spacing-3);
          padding: var(--spacing-3) var(--spacing-6);
          color: var(--color-text-secondary);
          text-decoration: none;
          transition: all var(--duration-fast) var(--easing-smooth);
          
          .nav-icon {
            width: 1.25rem;
            height: 1.25rem;
            flex-shrink: 0;
          }
          
          &:hover {
            background-color: var(--color-background-secondary);
            color: var(--color-text-primary);
          }
          
          &.active {
            background-color: rgba(var(--color-primary-rgb), 0.1);
            color: var(--color-primary);
            border-right: 3px solid var(--color-primary);
          }
        }
      }
    }
  }
}
```

### Table Components

#### Data Table
```css
.table {
  width: 100%;
  border-collapse: collapse;
  background-color: var(--color-background-primary);
  border-radius: var(--border-radius-lg);
  overflow: hidden;
  box-shadow: var(--shadow-base);
  
  .table-header {
    background-color: var(--color-background-secondary);
    
    .table-header-cell {
      padding: var(--spacing-4) var(--spacing-6);
      text-align: left;
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-bold);
      color: var(--color-text-primary);
      border-bottom: 1px solid var(--color-border-primary);
    }
  }
  
  .table-body {
    .table-row {
      transition: background-color var(--duration-fast) var(--easing-smooth);
      
      &:hover {
        background-color: var(--color-background-secondary);
      }
      
      &:not(:last-child) {
        border-bottom: 1px solid var(--color-border-secondary);
      }
      
      .table-cell {
        padding: var(--spacing-4) var(--spacing-6);
        font-size: var(--font-size-base);
        color: var(--color-text-primary);
        vertical-align: middle;
      }
    }
  }
}
```

### Modal Components

#### Modal Overlay
```css
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--color-background-overlay);
  z-index: var(--z-index-modal);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-4);
  
  &.entering {
    animation: modal-fade-in var(--duration-normal) var(--easing-smooth);
  }
  
  &.exiting {
    animation: modal-fade-out var(--duration-normal) var(--easing-smooth);
  }
}

.modal {
  background-color: var(--color-background-primary);
  border-radius: var(--border-radius-xl);
  box-shadow: var(--shadow-2xl);
  max-width: 32rem;
  width: 100%;
  max-height: 90vh;
  overflow: hidden;
  
  &.entering {
    animation: modal-scale-in var(--duration-normal) var(--easing-bounce);
  }
  
  &.exiting {
    animation: modal-scale-out var(--duration-normal) var(--easing-smooth);
  }
}

.modal-header {
  padding: var(--spacing-6);
  border-bottom: 1px solid var(--color-border-secondary);
  display: flex;
  align-items: center;
  justify-content: space-between;
  
  .modal-title {
    margin: 0;
    font-size: var(--font-size-xl);
    font-weight: var(--font-weight-bold);
    color: var(--color-text-primary);
  }
  
  .modal-close {
    background: none;
    border: none;
    font-size: var(--font-size-xl);
    color: var(--color-text-secondary);
    cursor: pointer;
    padding: var(--spacing-1);
    border-radius: var(--border-radius-base);
    transition: all var(--duration-fast) var(--easing-smooth);
    
    &:hover {
      background-color: var(--color-background-secondary);
      color: var(--color-text-primary);
    }
  }
}

.modal-body {
  padding: var(--spacing-6);
  overflow-y: auto;
}

.modal-footer {
  padding: var(--spacing-6);
  border-top: 1px solid var(--color-border-secondary);
  display: flex;
  gap: var(--spacing-3);
  justify-content: flex-end;
}
```

### Alert Components

#### Alert Base
```css
.alert {
  padding: var(--spacing-4);
  border-radius: var(--border-radius-md);
  border: 1px solid transparent;
  margin-bottom: var(--spacing-4);
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-3);
  
  .alert-icon {
    width: 1.25rem;
    height: 1.25rem;
    flex-shrink: 0;
    margin-top: 0.125rem;
  }
  
  .alert-content {
    flex: 1;
    
    .alert-title {
      font-weight: var(--font-weight-bold);
      margin-bottom: var(--spacing-1);
    }
    
    .alert-message {
      font-size: var(--font-size-sm);
      line-height: var(--line-height-relaxed);
    }
  }
  
  .alert-close {
    background: none;
    border: none;
    cursor: pointer;
    padding: var(--spacing-1);
    border-radius: var(--border-radius-base);
    opacity: 0.7;
    transition: opacity var(--duration-fast) var(--easing-smooth);
    
    &:hover {
      opacity: 1;
    }
  }
}

.alert-success {
  background-color: rgba(var(--color-primary-green-rgb), 0.1);
  border-color: var(--color-success);
  color: var(--color-success);
}

.alert-warning {
  background-color: rgba(255, 167, 38, 0.1);
  border-color: var(--color-warning);
  color: var(--color-warning);
}

.alert-error {
  background-color: rgba(239, 83, 80, 0.1);
  border-color: var(--color-error);
  color: var(--color-error);
}

.alert-info {
  background-color: rgba(var(--color-primary-rgb), 0.1);
  border-color: var(--color-info);
  color: var(--color-info);
}
```

### Badge Components

```css
.badge {
  display: inline-flex;
  align-items: center;
  padding: var(--spacing-1) var(--spacing-2);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  border-radius: var(--border-radius-full);
  text-transform: uppercase;
  letter-spacing: 0.025em;
  
  &.badge-primary {
    background-color: var(--color-primary);
    color: var(--color-text-inverse);
  }
  
  &.badge-success {
    background-color: var(--color-success);
    color: var(--color-text-inverse);
  }
  
  &.badge-warning {
    background-color: var(--color-warning);
    color: var(--color-text-inverse);
  }
  
  &.badge-error {
    background-color: var(--color-error);
    color: var(--color-text-inverse);
  }
  
  &.badge-outline {
    background-color: transparent;
    border: 1px solid currentColor;
  }
}
```

### Animation Keyframes

```css
@keyframes modal-fade-in {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes modal-fade-out {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}

@keyframes modal-scale-in {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes modal-scale-out {
  from {
    opacity: 1;
    transform: scale(1);
  }
  to {
    opacity: 0;
    transform: scale(0.9);
  }
}

@keyframes slide-in-right {
  from {
    transform: translateX(100%);
  }
  to {
    transform: translateX(0);
  }
}

@keyframes slide-out-right {
  from {
    transform: translateX(0);
  }
  to {
    transform: translateX(100%);
  }
}

@keyframes fade-in-up {
  from {
    opacity: 0;
    transform: translateY(1rem);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

### Utility Classes

#### Spacing Utilities
```css
/* Margin utilities */
.m-0 { margin: 0; }
.m-1 { margin: var(--spacing-1); }
.m-2 { margin: var(--spacing-2); }
.m-3 { margin: var(--spacing-3); }
.m-4 { margin: var(--spacing-4); }
.m-5 { margin: var(--spacing-5); }
.m-6 { margin: var(--spacing-6); }

/* Padding utilities */
.p-0 { padding: 0; }
.p-1 { padding: var(--spacing-1); }
.p-2 { padding: var(--spacing-2); }
.p-3 { padding: var(--spacing-3); }
.p-4 { padding: var(--spacing-4); }
.p-5 { padding: var(--spacing-5); }
.p-6 { padding: var(--spacing-6); }

/* Directional spacing */
.mt-0 { margin-top: 0; }
.mt-1 { margin-top: var(--spacing-1); }
.mt-2 { margin-top: var(--spacing-2); }
.mt-3 { margin-top: var(--spacing-3); }
.mt-4 { margin-top: var(--spacing-4); }

.mb-0 { margin-bottom: 0; }
.mb-1 { margin-bottom: var(--spacing-1); }
.mb-2 { margin-bottom: var(--spacing-2); }
.mb-3 { margin-bottom: var(--spacing-3); }
.mb-4 { margin-bottom: var(--spacing-4); }

.ml-0 { margin-left: 0; }
.ml-1 { margin-left: var(--spacing-1); }
.ml-2 { margin-left: var(--spacing-2); }
.ml-3 { margin-left: var(--spacing-3); }
.ml-4 { margin-left: var(--spacing-4); }

.mr-0 { margin-right: 0; }
.mr-1 { margin-right: var(--spacing-1); }
.mr-2 { margin-right: var(--spacing-2); }
.mr-3 { margin-right: var(--spacing-3); }
.mr-4 { margin-right: var(--spacing-4); }
```

#### Text Utilities
```css
.text-left { text-align: left; }
.text-center { text-align: center; }
.text-right { text-align: right; }
.text-justify { text-align: justify; }

.text-primary { color: var(--color-primary); }
.text-secondary { color: var(--color-secondary); }
.text-success { color: var(--color-success); }
.text-warning { color: var(--color-warning); }
.text-error { color: var(--color-error); }

.text-xs { font-size: var(--font-size-xs); }
.text-sm { font-size: var(--font-size-sm); }
.text-base { font-size: var(--font-size-base); }
.text-lg { font-size: var(--font-size-lg); }
.text-xl { font-size: var(--font-size-xl); }

.font-light { font-weight: var(--font-weight-light); }
.font-normal { font-weight: var(--font-weight-regular); }
.font-medium { font-weight: var(--font-weight-medium); }
.font-bold { font-weight: var(--font-weight-bold); }
.font-heavy { font-weight: var(--font-weight-heavy); }
```

#### Display Utilities
```css
.d-none { display: none; }
.d-block { display: block; }
.d-inline { display: inline; }
.d-inline-block { display: inline-block; }
.d-flex { display: flex; }
.d-inline-flex { display: inline-flex; }
.d-grid { display: grid; }

.flex-row { flex-direction: row; }
.flex-column { flex-direction: column; }
.flex-wrap { flex-wrap: wrap; }
.flex-nowrap { flex-wrap: nowrap; }

.justify-start { justify-content: flex-start; }
.justify-center { justify-content: center; }
.justify-end { justify-content: flex-end; }
.justify-between { justify-content: space-between; }
.justify-around { justify-content: space-around; }

.items-start { align-items: flex-start; }
.items-center { align-items: center; }
.items-end { align-items: flex-end; }
.items-stretch { align-items: stretch; }
```

This comprehensive component library provides all the essential UI components needed to build the School Transportation Management System with consistent TETCO branding and excellent user experience.

