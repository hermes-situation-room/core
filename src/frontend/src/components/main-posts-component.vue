<script setup lang="ts">
import {computed, onMounted, ref} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import CreatePostModal from './create-post-modal.vue';
import {services} from '../services/api';
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const notification = useNotification();

const searchQuery = ref('');
const showCreateModal = ref(false);
const showFilterModal = ref(false);
const refreshKey = ref(0);

const availableTags = ref<string[]>([]);
const selectedFilterTags = ref<string[]>([]);
const loadingTags = ref(false);

type SortOption = 'newest' | 'oldest' | 'title-asc' | 'title-desc';
const sortBy = ref<SortOption>('newest');
const showSortDropdown = ref(false);

const currentTab = computed(() => {
    const path = route.path;
    if (path.includes('/journalist')) return 'journalist';
    if (path.includes('/activist')) return 'activist';
    return 'journalist';
});

const sortLabel = computed(() => {
    switch (sortBy.value) {
        case 'newest': return 'Newest First';
        case 'oldest': return 'Oldest First';
        case 'title-asc': return 'Title (A-Z)';
        case 'title-desc': return 'Title (Z-A)';
        default: return 'Sort';
    }
});

const switchTab = (tab: 'journalist' | 'activist') => {
    switch (tab) {
        case 'journalist':
            router.push('/journalist');
            break;
        case 'activist':
            router.push('/activist');
            break;
    }
};

const handleSearch = () => {
    // Is handled in the subcomponent via props
};

const openCreateModal = () => {
    if (!authStore.isAuthenticated.value) {
        router.push('/login');
        return;
    }
    showCreateModal.value = true;
};

const closeCreateModal = () => {
    showCreateModal.value = false;
};

const handlePostCreated = () => {
    refreshKey.value++;
    if (authStore.userType.value) {
        router.push('/' + authStore.userType.value);
    }
};

const loadTags = async () => {
    loadingTags.value = true;
    try {
        const result = await services.tags.getAllTags();
        if (result.isSuccess && result.data) {
            availableTags.value = result.data;
        } else {
            notification.error(result.responseMessage || 'Failed to load tags');
        }
    } catch (err) {
        notification.error('Error loading tags');
    } finally {
        loadingTags.value = false;
    }
};

const openFilterModal = () => {
    showFilterModal.value = true;
};

const closeFilterModal = () => {
    showFilterModal.value = false;
};

const toggleFilterTag = (tag: string) => {
    const index = selectedFilterTags.value.indexOf(tag);
    if (index > -1) {
        selectedFilterTags.value.splice(index, 1);
    } else {
        selectedFilterTags.value.push(tag);
    }
};

const isFilterTagSelected = (tag: string) => {
    return selectedFilterTags.value.includes(tag);
};

const clearFilters = () => {
    selectedFilterTags.value = [];
    refreshKey.value++;
};

const applyFilters = () => {
    closeFilterModal();
    refreshKey.value++;
};

const toggleSortDropdown = () => {
    showSortDropdown.value = !showSortDropdown.value;
};

const selectSort = (option: SortOption) => {
    sortBy.value = option;
    showSortDropdown.value = false;
};

const closeSortDropdown = () => {
    showSortDropdown.value = false;
};

onMounted(() => {
    loadTags();
    
    document.addEventListener('click', (e) => {
        const target = e.target as HTMLElement;
        if (!target.closest('.dropdown')) {
            closeSortDropdown();
        }
    });
});
</script>

<template>
    <div class="d-flex flex-column" style="height: calc(100vh - 60px); overflow: hidden;">
        <div class="bg-white border-bottom flex-shrink-0" style="z-index: 1000;">
            <div class="container">
                <div class="d-md-none py-3">
                    <div class="mb-2">
                        <div class="btn-group w-100" role="group">
                            <button
                                @click="switchTab('journalist')"
                                :class="['btn', 'py-2', currentTab === 'journalist' ? 'btn-dark' : 'btn-outline-dark']"
                            >
                                Journalist
                            </button>
                            <button
                                @click="switchTab('activist')"
                                :class="['btn', 'py-2', currentTab === 'activist' ? 'btn-dark' : 'btn-outline-dark']"
                            >
                                Activist
                            </button>
                        </div>
                    </div>
                    
                    <div class="mb-2">
                        <div class="input-group">
                            <span class="input-group-text bg-white border-end-0">
                                <i class="fas fa-search text-muted"></i>
                            </span>
                            <input
                                v-model="searchQuery"
                                @input="handleSearch"
                                type="text"
                                class="form-control border-start-0"
                                placeholder="Search posts..."
                            />
                        </div>
                    </div>
                    
                    <div class="d-flex gap-2">
                        <button 
                            class="btn btn-outline-secondary position-relative flex-fill py-2"
                            @click="openFilterModal"
                        >
                            <i class="fas fa-filter me-1"></i>
                            <span class="d-none d-sm-inline">Filter</span>
                            <span 
                                v-if="selectedFilterTags.length > 0" 
                                class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-primary"
                            >
                                {{ selectedFilterTags.length }}
                            </span>
                        </button>
                        
                        <div class="dropdown flex-fill">
                            <button 
                                class="btn btn-outline-secondary dropdown-toggle w-100 py-2"
                                type="button"
                                @click="toggleSortDropdown"
                            >
                                <i class="fas fa-sort me-1"></i>
                                <span class="d-none d-sm-inline">{{ sortLabel }}</span>
                            </button>
                            <ul 
                                :class="['dropdown-menu', 'w-100', { 'show': showSortDropdown }]"
                            >
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('newest')"
                                        :class="{ 'active': sortBy === 'newest' }"
                                    >
                                        <i class="fas fa-clock me-2"></i>Newest First
                                    </a>
                                </li>
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('oldest')"
                                        :class="{ 'active': sortBy === 'oldest' }"
                                    >
                                        <i class="fas fa-clock-rotate-left me-2"></i>Oldest First
                                    </a>
                                </li>
                                <li><hr class="dropdown-divider"></li>
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('title-asc')"
                                        :class="{ 'active': sortBy === 'title-asc' }"
                                    >
                                        <i class="fas fa-arrow-down-a-z me-2"></i>Title (A-Z)
                                    </a>
                                </li>
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('title-desc')"
                                        :class="{ 'active': sortBy === 'title-desc' }"
                                    >
                                        <i class="fas fa-arrow-up-z-a me-2"></i>Title (Z-A)
                                    </a>
                                </li>
                            </ul>
                        </div>
                        
                        <button 
                            class="btn btn-primary flex-fill py-2"
                            @click="openCreateModal"
                        >
                            <i :class="authStore.isAuthenticated.value ? 'fas fa-plus' : 'fas fa-sign-in-alt'" class="me-1"></i>
                            <span class="d-none d-sm-inline">{{ authStore.isAuthenticated.value ? 'Create Post' : 'Login to Post' }}</span>
                            <span class="d-sm-none">{{ authStore.isAuthenticated.value ? 'Create' : 'Login' }}</span>
                        </button>
                    </div>
                </div>

                <div class="d-none d-md-flex py-3">
                    <div class="d-flex gap-2 w-100 align-items-center">
                        <button 
                            class="btn btn-outline-secondary position-relative flex-shrink-0"
                            @click="openFilterModal"
                        >
                            <i class="fas fa-filter me-1"></i>
                            <span class="d-none d-lg-inline">Filter</span>
                            <span 
                                v-if="selectedFilterTags.length > 0" 
                                class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-primary"
                            >
                                {{ selectedFilterTags.length }}
                            </span>
                        </button>
                        
                        <div class="input-group flex-grow-1" style="min-width: 200px;">
                            <span class="input-group-text bg-white border-end-0">
                                <i class="fas fa-search text-muted"></i>
                            </span>
                            <input
                                v-model="searchQuery"
                                @input="handleSearch"
                                type="text"
                                class="form-control border-start-0"
                                placeholder="Search posts..."
                            />
                        </div>
                        
                        <div class="btn-group flex-shrink-0" role="group">
                            <button
                                @click="switchTab('journalist')"
                                :class="['btn', currentTab === 'journalist' ? 'btn-dark' : 'btn-outline-dark']"
                            >
                                <span class="d-none d-lg-inline">Journalist</span>
                                <span class="d-lg-none">J</span>
                            </button>
                            <button
                                @click="switchTab('activist')"
                                :class="['btn', currentTab === 'activist' ? 'btn-dark' : 'btn-outline-dark']"
                            >
                                <span class="d-none d-lg-inline">Activist</span>
                                <span class="d-lg-none">A</span>
                            </button>
                        </div>
                        
                        <div class="dropdown flex-shrink-0">
                            <button 
                                class="btn btn-outline-secondary dropdown-toggle"
                                type="button"
                                @click="toggleSortDropdown"
                            >
                                <i class="fas fa-sort me-1"></i>
                                <span class="d-none d-lg-inline">{{ sortLabel }}</span>
                                <span class="d-lg-none">Sort</span>
                            </button>
                            <ul 
                                :class="['dropdown-menu', 'dropdown-menu-end', { 'show': showSortDropdown }]"
                                style="min-width: 180px;"
                            >
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('newest')"
                                        :class="{ 'active': sortBy === 'newest' }"
                                    >
                                        <i class="fas fa-clock me-2"></i>Newest First
                                    </a>
                                </li>
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('oldest')"
                                        :class="{ 'active': sortBy === 'oldest' }"
                                    >
                                        <i class="fas fa-clock-rotate-left me-2"></i>Oldest First
                                    </a>
                                </li>
                                <li><hr class="dropdown-divider"></li>
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('title-asc')"
                                        :class="{ 'active': sortBy === 'title-asc' }"
                                    >
                                        <i class="fas fa-arrow-down-a-z me-2"></i>Title (A-Z)
                                    </a>
                                </li>
                                <li>
                                    <a 
                                        class="dropdown-item" 
                                        href="#"
                                        @click.prevent="selectSort('title-desc')"
                                        :class="{ 'active': sortBy === 'title-desc' }"
                                    >
                                        <i class="fas fa-arrow-up-z-a me-2"></i>Title (Z-A)
                                    </a>
                                </li>
                            </ul>
                        </div>
                        
                        <button 
                            class="btn btn-primary flex-shrink-0"
                            @click="openCreateModal"
                        >
                            <i :class="authStore.isAuthenticated.value ? 'fas fa-plus' : 'fas fa-sign-in-alt'" class="me-1"></i>
                            <span class="d-none d-lg-inline">{{ authStore.isAuthenticated.value ? 'Create Post' : 'Login to Post' }}</span>
                            <span class="d-lg-none">{{ authStore.isAuthenticated.value ? 'Create' : 'Login' }}</span>
                        </button>
                    </div>
                </div>

            </div>
        </div>
            
        <div v-if="selectedFilterTags.length > 0" class="bg-light border-bottom flex-shrink-0">
            <div class="container py-2">
                <div class="d-flex align-items-start flex-column flex-sm-row gap-2">
                    <small class="text-muted fw-bold text-nowrap">Active Filters:</small>
                    <div class="d-flex align-items-center flex-wrap gap-2 flex-grow-1">
                        <span
                            v-for="tag in selectedFilterTags"
                            :key="tag"
                            class="badge bg-primary py-1 px-2 d-flex align-items-center"
                        >
                            {{ tag }}
                            <button
                                type="button"
                                class="btn-close btn-close-white ms-2"
                                style="font-size: 0.6rem;"
                                @click="toggleFilterTag(tag); refreshKey++"
                                aria-label="Remove filter"
                            ></button>
                        </span>
                        <button
                            class="btn btn-sm btn-outline-danger"
                            @click="clearFilters"
                        >
                            Clear All
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div class="flex-grow-1 overflow-auto">
            <div class="container py-3 py-md-4">
                <router-view 
                    :key="refreshKey" 
                    :search-query="searchQuery"
                    :filter-tags="selectedFilterTags"
                    :sort-by="sortBy"
                />
            </div>
        </div>

        <CreatePostModal 
            :show="showCreateModal"
            @close="closeCreateModal"
            @post-created="handlePostCreated"
        />

        <div 
            v-if="showFilterModal"
            class="modal fade show d-block" 
            tabindex="-1" 
            style="background-color: rgba(0, 0, 0, 0.5);"
            @click.self="closeFilterModal"
        >
            <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable modal-fullscreen-sm-down">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="fas fa-filter me-2"></i>Filter Posts by Tags
                        </h5>
                        <button 
                            type="button" 
                            class="btn-close" 
                            @click="closeFilterModal"
                        ></button>
                    </div>
                    <div class="modal-body">
                        <div v-if="loadingTags" class="text-center py-4">
                            <div class="spinner-border text-primary" role="status">
                                <span class="visually-hidden">Loading...</span>
                            </div>
                            <div class="text-muted mt-2">Loading tags...</div>
                        </div>
                        <div v-else>
                            <p class="text-muted mb-3">
                                Select one or more tags to filter posts. Posts matching any of the selected tags will be shown.
                            </p>
                            <div class="d-flex flex-wrap gap-2">
                                <span
                                    v-for="tag in availableTags"
                                    :key="tag"
                                    @click="toggleFilterTag(tag)"
                                    :class="[
                                        'badge',
                                        'py-2',
                                        'px-3',
                                        'fs-6',
                                        isFilterTagSelected(tag) ? 'bg-primary' : 'bg-secondary'
                                    ]"
                                    style="cursor: pointer;"
                                >
                                    <i v-if="isFilterTagSelected(tag)" class="fas fa-check me-1"></i>
                                    {{ tag }}
                                </span>
                            </div>
                            <div v-if="selectedFilterTags.length > 0" class="mt-3">
                                <div class="alert alert-info mb-0">
                                    <strong>{{ selectedFilterTags.length }}</strong> tag{{ selectedFilterTags.length !== 1 ? 's' : '' }} selected
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button 
                            type="button" 
                            class="btn btn-outline-secondary" 
                            @click="clearFilters"
                            :disabled="selectedFilterTags.length === 0"
                        >
                            Clear All
                        </button>
                        <button 
                            type="button" 
                            class="btn btn-secondary" 
                            @click="closeFilterModal"
                        >
                            Cancel
                        </button>
                        <button 
                            type="button" 
                            class="btn btn-primary" 
                            @click="applyFilters"
                        >
                            Apply Filters
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
