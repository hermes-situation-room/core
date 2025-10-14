<script setup lang="ts">
import { ref } from 'vue'

const showModal = ref(false)
const showAlert = ref(true)
const selectedTab = ref('buttons')

const tabs = [
  { id: 'buttons', label: 'Buttons' },
  { id: 'forms', label: 'Forms' },
  { id: 'cards', label: 'Cards' },
  { id: 'badges', label: 'Badges' },
  { id: 'alerts', label: 'Alerts' },
  { id: 'animations', label: 'Animations' },
  { id: 'layout', label: 'Layout' },
]

const openModal = () => {
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}
</script>

<template>
  <div class="container py-8">
    <div class="text-center mb-8">
      <h1 class="mb-4">🎨 Design System Showcase</h1>
      <p class="text-muted fs-lg">iOS 26-Inspired Modern Design System</p>
    </div>

    <!-- Tab Navigation -->
    <div class="tabs mb-8">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        :class="['tab', { active: selectedTab === tab.id }]"
        @click="selectedTab = tab.id"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- Buttons Section -->
    <div v-if="selectedTab === 'buttons'" class="animate-fade-in">
      <h2 class="mb-6">Buttons</h2>

      <div class="card mb-6">
        <div class="card-body">
          <h4 class="mb-4">Primary Buttons</h4>
          <div class="d-flex flex-wrap gap-3 mb-6">
            <button class="btn btn-primary">Primary</button>
            <button class="btn btn-secondary">Secondary</button>
            <button class="btn btn-accent">Accent</button>
            <button class="btn btn-success">Success</button>
            <button class="btn btn-warning">Warning</button>
            <button class="btn btn-error">Error</button>
            <button class="btn btn-dark">Dark</button>
            <button class="btn btn-light">Light</button>
          </div>

          <h4 class="mb-4">Outline Buttons</h4>
          <div class="d-flex flex-wrap gap-3 mb-6">
            <button class="btn btn-outline-primary">Outline Primary</button>
            <button class="btn btn-outline-secondary">Outline Secondary</button>
            <button class="btn btn-outline-danger">Outline Danger</button>
          </div>

          <h4 class="mb-4">Button Sizes</h4>
          <div class="d-flex flex-wrap gap-3 align-items-center mb-6">
            <button class="btn btn-primary btn-sm">Small</button>
            <button class="btn btn-primary">Medium</button>
            <button class="btn btn-primary btn-lg">Large</button>
          </div>

          <h4 class="mb-4">Special Buttons</h4>
          <div class="d-flex flex-wrap gap-3 mb-6">
            <button class="btn btn-glass">Glass Effect</button>
            <button class="btn btn-link">Link Button</button>
            <button class="btn btn-primary" disabled>Disabled</button>
          </div>

          <h4 class="mb-4">Button with Icons</h4>
          <div class="d-flex flex-wrap gap-3">
            <button class="btn btn-primary">
              <i class="fas fa-plus"></i>
              <span>Create New</span>
            </button>
            <button class="btn btn-secondary">
              <i class="fas fa-search"></i>
              <span>Search</span>
            </button>
            <button class="btn btn-error">
              <i class="fas fa-trash"></i>
              <span>Delete</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Forms Section -->
    <div v-if="selectedTab === 'forms'" class="animate-fade-in">
      <h2 class="mb-6">Forms</h2>

      <div class="row g-6">
        <div class="col-12 col-lg-6">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Input Fields</h4>

              <div class="form-group">
                <label class="form-label">Email Address</label>
                <input type="email" class="form-control" placeholder="Enter email">
              </div>

              <div class="form-group">
                <label class="form-label">Password</label>
                <input type="password" class="form-control" placeholder="Enter password">
              </div>

              <div class="form-group">
                <label class="form-label">Small Input</label>
                <input type="text" class="form-control form-control-sm" placeholder="Small size">
              </div>

              <div class="form-group">
                <label class="form-label">Large Input</label>
                <input type="text" class="form-control form-control-lg" placeholder="Large size">
              </div>

              <div class="form-group">
                <label class="form-label">Disabled Input</label>
                <input type="text" class="form-control" placeholder="Disabled" disabled>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 col-lg-6">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Other Form Elements</h4>

              <div class="form-group">
                <label class="form-label">Textarea</label>
                <textarea class="form-control" placeholder="Enter message..."></textarea>
              </div>

              <div class="form-group">
                <label class="form-label">Select</label>
                <select class="form-control">
                  <option>Option 1</option>
                  <option>Option 2</option>
                  <option>Option 3</option>
                </select>
              </div>

              <div class="form-check">
                <input type="checkbox" id="check1" checked>
                <label for="check1">Checkbox option</label>
              </div>

              <div class="form-check">
                <input type="checkbox" id="check2">
                <label for="check2">Another checkbox</label>
              </div>

              <div class="form-check">
                <input type="radio" id="radio1" name="radio" checked>
                <label for="radio1">Radio option 1</label>
              </div>

              <div class="form-check">
                <input type="radio" id="radio2" name="radio">
                <label for="radio2">Radio option 2</label>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Input Groups</h4>

              <div class="form-group">
                <label class="form-label">Search</label>
                <div class="input-group">
                  <span class="input-group-text">
                    <i class="fas fa-search"></i>
                  </span>
                  <input type="text" class="form-control" placeholder="Search...">
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Email</label>
                <div class="input-group">
                  <span class="input-group-text">
                    <i class="fas fa-envelope"></i>
                  </span>
                  <input type="email" class="form-control" placeholder="Enter email">
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Cards Section -->
    <div v-if="selectedTab === 'cards'" class="animate-fade-in">
      <h2 class="mb-6">Cards</h2>

      <div class="row g-4">
        <div class="col-12 col-md-6 col-lg-4">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">Standard Card</h5>
              <p class="card-text">This is a standard card with elevation and hover effects.</p>
              <button class="btn btn-primary btn-sm">Action</button>
            </div>
          </div>
        </div>

        <div class="col-12 col-md-6 col-lg-4">
          <div class="card card-interactive">
            <div class="card-body">
              <h5 class="card-title">Interactive Card</h5>
              <p class="card-text">Click me! I have hover and active states.</p>
            </div>
          </div>
        </div>

        <div class="col-12 col-md-6 col-lg-4">
          <div class="card card-glass">
            <div class="card-body">
              <h5 class="card-title">Glass Card</h5>
              <p class="card-text">Modern glassmorphism effect with blur.</p>
            </div>
          </div>
        </div>

        <div class="col-12">
          <div class="card">
            <div class="card-header">
              Card with Header
            </div>
            <div class="card-body">
              <h5 class="card-title">Card Title</h5>
              <p class="card-text">Card body content goes here.</p>
            </div>
            <div class="card-footer">
              Card Footer
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Badges Section -->
    <div v-if="selectedTab === 'badges'" class="animate-fade-in">
      <h2 class="mb-6">Badges & Tags</h2>

      <div class="card">
        <div class="card-body">
          <h4 class="mb-4">Standard Badges</h4>
          <div class="d-flex flex-wrap gap-2 mb-6">
            <span class="badge badge-primary">Primary</span>
            <span class="badge badge-secondary">Secondary</span>
            <span class="badge badge-success">Success</span>
            <span class="badge badge-warning">Warning</span>
            <span class="badge badge-error">Error</span>
            <span class="badge badge-light">Light</span>
            <span class="badge badge-dark">Dark</span>
          </div>

          <h4 class="mb-4">Outline Badges</h4>
          <div class="d-flex flex-wrap gap-2 mb-6">
            <span class="badge badge-outline">Outline</span>
          </div>

          <h4 class="mb-4">Badge with Dot</h4>
          <div class="d-flex flex-wrap gap-2">
            <span class="badge badge-dot badge-success">Active</span>
            <span class="badge badge-dot badge-error">Offline</span>
            <span class="badge badge-dot badge-warning">Away</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Alerts Section -->
    <div v-if="selectedTab === 'alerts'" class="animate-fade-in">
      <h2 class="mb-6">Alerts</h2>

      <div class="alert alert-success">
        <i class="fas fa-check-circle"></i>
        <span>This is a success alert with an icon!</span>
      </div>

      <div class="alert alert-info">
        <i class="fas fa-info-circle"></i>
        <span>This is an info alert with important information.</span>
      </div>

      <div class="alert alert-warning">
        <i class="fas fa-exclamation-triangle"></i>
        <span>This is a warning alert. Please be careful!</span>
      </div>

      <div class="alert alert-error">
        <i class="fas fa-times-circle"></i>
        <span>This is an error alert. Something went wrong!</span>
      </div>

      <div v-if="showAlert" class="alert alert-success alert-dismissible">
        <i class="fas fa-check-circle"></i>
        <span>This is a dismissible alert!</span>
        <button class="btn-close" @click="showAlert = false"></button>
      </div>
    </div>

    <!-- Animations Section -->
    <div v-if="selectedTab === 'animations'" class="animate-fade-in">
      <h2 class="mb-6">Animations</h2>

      <div class="row g-4">
        <div class="col-12 col-md-6">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Fade Animations</h4>
              <div class="animate-fade-in mb-3">
                <div class="card card-glass">
                  <div class="card-body text-center">Fade In</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12 col-md-6">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Slide Animations</h4>
              <div class="animate-slide-in-up mb-3">
                <div class="card card-glass">
                  <div class="card-body text-center">Slide In Up</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Hover Effects</h4>
              <div class="row g-3">
                <div class="col-12 col-md-4">
                  <div class="card hover-lift">
                    <div class="card-body text-center">
                      <i class="fas fa-arrow-up fs-2xl mb-3"></i>
                      <p class="mb-0">Hover Lift</p>
                    </div>
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="card hover-scale">
                    <div class="card-body text-center">
                      <i class="fas fa-expand fs-2xl mb-3"></i>
                      <p class="mb-0">Hover Scale</p>
                    </div>
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="card card-interactive">
                    <div class="card-body text-center">
                      <i class="fas fa-hand-pointer fs-2xl mb-3"></i>
                      <p class="mb-0">Interactive</p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-12">
          <div class="card">
            <div class="card-body">
              <h4 class="mb-4">Loading States</h4>
              <div class="d-flex flex-wrap gap-4 align-items-center">
                <div class="spinner-border"></div>
                <div class="spinner-border spinner-border-sm"></div>
                <div class="spinner-border spinner-border-lg"></div>
                <div class="animate-pulse">Pulsing text...</div>
                <div class="loading-shimmer" style="width: 200px; height: 40px; border-radius: 8px;"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Layout Section -->
    <div v-if="selectedTab === 'layout'" class="animate-fade-in">
      <h2 class="mb-6">Layout Utilities</h2>

      <div class="card mb-6">
        <div class="card-body">
          <h4 class="mb-4">Flexbox Layout</h4>
          <div class="d-flex justify-content-between align-items-center p-4 bg-secondary rounded-lg mb-3">
            <div class="bg-primary text-white p-3 rounded">Left</div>
            <div class="bg-primary text-white p-3 rounded">Center</div>
            <div class="bg-primary text-white p-3 rounded">Right</div>
          </div>

          <div class="d-flex flex-column gap-3 p-4 bg-secondary rounded-lg">
            <div class="bg-primary text-white p-3 rounded">Item 1</div>
            <div class="bg-primary text-white p-3 rounded">Item 2</div>
            <div class="bg-primary text-white p-3 rounded">Item 3</div>
          </div>
        </div>
      </div>

      <div class="card mb-6">
        <div class="card-body">
          <h4 class="mb-4">Grid System</h4>
          <div class="row g-3">
            <div class="col-12 col-md-6 col-lg-4">
              <div class="bg-primary text-white p-4 rounded text-center">Col 1</div>
            </div>
            <div class="col-12 col-md-6 col-lg-4">
              <div class="bg-primary text-white p-4 rounded text-center">Col 2</div>
            </div>
            <div class="col-12 col-md-6 col-lg-4">
              <div class="bg-primary text-white p-4 rounded text-center">Col 3</div>
            </div>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-body">
          <h4 class="mb-4">Spacing Examples</h4>
          <div class="d-flex flex-column gap-4">
            <div class="p-4 bg-secondary rounded">Padding: p-4</div>
            <div class="p-6 bg-secondary rounded">Padding: p-6</div>
            <div class="p-8 bg-secondary rounded">Padding: p-8</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Demo Button -->
    <div class="text-center mt-8">
      <button class="btn btn-primary btn-lg" @click="openModal">
        <i class="fas fa-window-maximize"></i>
        <span>Show Modal Demo</span>
      </button>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal">
      <div class="modal-backdrop" @click="closeModal"></div>
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas fa-star me-2"></i>
              Modal Example
            </h5>
            <button class="btn-close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <p>This is a beautiful modal with smooth animations!</p>
            <p class="mb-0">Features:</p>
            <ul>
              <li>Scale-in animation</li>
              <li>Blurred backdrop</li>
              <li>Smooth transitions</li>
              <li>iOS-inspired design</li>
            </ul>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="closeModal">Cancel</button>
            <button class="btn btn-primary" @click="closeModal">Confirm</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Additional component-specific styles if needed */
.tabs {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}
</style>

