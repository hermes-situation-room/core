<script setup lang="ts">
import {watch, onMounted, onBeforeUnmount, ref, computed} from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {services} from '../services/api';
import type {PostBo} from '../types/post'
import { useAuthStore } from '../stores/auth-store';
import { useNotification } from '../composables/use-notification.ts';
import type { UserProfileBo } from '../types/user.ts';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const notification = useNotification();

type SortOption = 'newest' | 'oldest' | 'title-asc' | 'title-desc';
interface Props {
    userProfile: UserProfileBo
}

const props = defineProps<Props>()

const currentUserUid = computed(() => authStore.userId.value || '');
const userType = computed(() => {
    if (props.userProfile.userName != null){
        return 'activist';
    } else {
        return 'journalist';
    }
})

const currentPage = ref(1);
const postsPerPage = 12;
const hasMorePosts = ref(true);

const searchQuery = ref('');
const sortBy = ref<SortOption>('newest')
const showSortDropdown = ref(false);

const loadingPosts = ref(false)
const posts = ref<PostBo[]>([]);
const displayName = ref('');

let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;
const filteredPosts = computed(() => posts.value);

const sortLabel = computed(() => {
    switch (sortBy.value) {
        case 'newest': return 'Newest First';
        case 'oldest': return 'Oldest First';
        case 'title-asc': return 'Title (A-Z)';
        case 'title-desc': return 'Title (Z-A)';
        default: return 'Sort';
    }
});

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

const loadPosts = async () => {
    loadingPosts.value = true;
    try {
        const offset = (currentPage.value - 1) * postsPerPage;

        const postsResult = await services.posts.getPostsByUser(props.userProfile.uid, postsPerPage, offset, searchQuery.value, sortBy.value);
        if (postsResult.isSuccess && postsResult.data) {
            posts.value = postsResult.data; 
            
            // Check if there are more posts (if we got less than postsPerPage, we're at the end)
            hasMorePosts.value = postsResult.data.length === postsPerPage;
            
            // Update URL to show page 1 explicitly when pagination is needed
            if (currentPage.value === 1 && hasMorePosts.value && !route.query.page) {
                router.replace({
                    query: {
                        ...route.query,
                        page: '1'
                    }
                });
            }
            
            const displayNameResult = await services.users.getDisplayName(props.userProfile.uid);
            displayName.value = displayNameResult.isSuccess && displayNameResult.data ? displayNameResult.data : 'Error loading User'
        } else {
            notification.error(postsResult.responseMessage || 'Failed to load posts');
        }
    } catch (error) {
        notification.error('Error loading posts');
    } finally {
        loadingPosts.value = false;
    }
};

const formatDate = (dateString: string) => {
    const date = new Date(dateString + 'Z') || new Date().toISOString();
    return date.toLocaleDateString(navigator.language || 'en-US', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
    });
};

const viewPost = (postId: string) => {
    router.push({
        path: `/post/${postId}`,
        query: {category: userType.value}
    });
};

const goToPage = (page: number) => {
    currentPage.value = page;
    
    // Update URL with current page
    router.push({
        query: {
            ...route.query,
            page: page.toString()
        }
    });
    
    loadPosts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

const nextPage = () => {
    if (hasMorePosts.value) {
        goToPage(currentPage.value + 1);
    }
};

const previousPage = () => {
    if (currentPage.value > 1) {
        goToPage(currentPage.value - 1);
    }
};

watch(() => searchQuery.value, () => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer);
    }
    
    searchDebounceTimer = setTimeout(() => {
        currentPage.value = 1; // Reset to first page when search changes
        
        // Remove page parameter temporarily - will be set after loading if needed
        router.push({
            query: {
                ...route.query,
                page: undefined
            }
        });
        
        loadPosts();
    }, 500); // 500ms debounce (half a second)
});

watch(() => sortBy.value, () => {
    currentPage.value = 1; // reset pagination
    router.push({ query: { ...route.query, page: undefined } });
    loadPosts(); // reload posts
}, { immediate: true });

onMounted(() => {
    loadPosts();
    
    document.addEventListener('click', (e) => {
        const target = e.target as HTMLElement;
        if (!target.closest('.dropdown')) {
            closeSortDropdown();
        }
    });
});

onBeforeUnmount(() => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer);
    }
});
</script>

<template>
    <div class="p-4">
        <div v-if="posts">
            <h1>Posts</h1>
            <div class="mb-2">
                <div class="input-group">
                    <span class="input-group-text bg-white border-end-0">
                        <i class="fas fa-search text-muted"></i>
                    </span>
                    <input
                        v-model="searchQuery"
                        type="text"
                        class="form-control border-start-0"
                        placeholder="Search posts..."
                    />
                </div>
            </div>
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
        </div>
       
        <div class="w-100">
            <div v-if="loadingPosts" class="d-flex justify-content-center align-items-center py-5">
                <div class="text-center">
                    <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-muted">Loading posts...</div>
                </div>
            </div>
            

            <div v-else-if="filteredPosts.length > 0" class="d-flex flex-column gap-3 mt-3">
                <div 
                    v-for="post in filteredPosts" 
                    :key="post.uid"
                    class=""
                >
                    <div class="card h-100 shadow-sm d-flex flex-column">
                        <div class="card-body flex-grow-1" style="cursor: pointer;" @click="viewPost(post.uid)">
                            <h5 class="card-title">{{ post.title }}</h5>
                            <p class="card-text text-muted small">{{ post.description }}</p>
                            <div class="d-flex flex-wrap gap-1 mb-2">
                                <span v-for="tag in post.tags.slice(0, 3)" :key="tag" class="badge bg-info text-white">
                                    {{ tag }}
                                </span>
                                <span v-if="post.tags.length > 3" class="badge bg-secondary text-white">
                                    +{{ post.tags.length - 3 }}
                                </span>
                            </div>
                            <small class="text-muted">{{ formatDate(post.timestamp) }}</small>
                        </div>
                        <div class="card-footer bg-light border-top">
                            <div class="d-flex justify-content-between align-items-center">
                                <small class="text-muted d-flex align-items-center">
                                    <i class="fas fa-user me-1"></i>
                                    <span v-if="currentUserUid && post.creatorUid === currentUserUid">You</span>
                                    <a 
                                        v-else
                                        href="#" 
                                        class="text-primary text-decoration-none"
                                        @click.prevent.stop="router.push({ path: '/profile', query: { id: post.creatorUid } })"
                                    >
                                        {{ displayName }}
                                    </a>
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <div v-else class="text-center py-5">
                <i class="fas fa-inbox fa-3x text-muted mb-3"></i>
                <h5 class="text-muted">No posts found</h5>
                <p class="text-muted">Try adjusting your filters or search query</p>
            </div>

            <!-- Pagination Controls -->
            <div v-if="!loadPosts && (currentPage > 1 || hasMorePosts)" class="d-flex justify-content-center align-items-center gap-3 mt-4">
                <button 
                    class="btn btn-outline-primary"
                    :disabled="currentPage === 1"
                    @click="previousPage"
                >
                    <i class="fas fa-chevron-left me-2"></i>Previous
                </button>
                
                <div class="d-flex align-items-center gap-2">
                    <span class="text-muted">Page {{ currentPage }}</span>
                </div>
                
                <button 
                    class="btn btn-outline-primary"
                    :disabled="!hasMorePosts"
                    @click="nextPage"
                >
                    Next<i class="fas fa-chevron-right ms-2"></i>
                </button>
            </div>
        </div>
    </div>
</template>