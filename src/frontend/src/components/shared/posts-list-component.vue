<script setup lang="ts">
import {computed, onMounted, onBeforeUnmount, ref, watch} from 'vue';
import {useRouter, useRoute} from 'vue-router';
import {services} from '../../services/api';
import type {PostBo, PostFilter} from '../../types/post';
import { useAuthStore } from '../../stores/auth-store';
import { useNotification } from '../../composables/use-notification.ts';

type SortOption = 'newest' | 'oldest' | 'title-asc' | 'title-desc';

interface Props {
    postType: 'activist' | 'journalist';
    searchQuery?: string;
    filterTags?: string[];
    sortBy?: SortOption;
}

const props = withDefaults(defineProps<Props>(), {
    searchQuery: '',
    filterTags: () => [],
    sortBy: 'newest'
});

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const notification = useNotification();
const currentUserUid = computed(() => authStore.userId.value || '');

const posts = ref<PostBo[]>([]);
const loading = ref(false);
const displayNames = ref<Map<string, string>>(new Map());

// Pagination state - initialize from URL query parameter
const currentPage = ref(1);
const postsPerPage = 12;
const hasMorePosts = ref(true);

// Debounce timer for search
let searchDebounceTimer: ReturnType<typeof setTimeout> | null = null;

// Initialize page from URL
const initializePageFromUrl = () => {
    const pageParam = route.query.page;
    if (pageParam) {
        const pageNum = parseInt(pageParam as string, 10);
        if (!isNaN(pageNum) && pageNum > 0) {
            currentPage.value = pageNum;
        }
    }
};

const filteredPosts = computed(() => posts.value);

const loadPosts = async () => {
    loading.value = true;
    try {
        const offset = (currentPage.value - 1) * postsPerPage;
        const filter: PostFilter = {
            category: props.postType,
            tags: props.filterTags && props.filterTags.length > 0 ? props.filterTags : undefined,
            limit: postsPerPage,
            offset: offset,
            query: props.searchQuery || undefined,
            sortBy: props.sortBy || undefined
        };

        const result = await services.posts.getPostsWithFilter(filter);
        if (result.isSuccess && result.data) {
            posts.value = result.data;
            
            // Check if there are more posts (if we got less than postsPerPage, we're at the end)
            hasMorePosts.value = result.data.length === postsPerPage;
            
            // Update URL to show page 1 explicitly when pagination is needed
            if (currentPage.value === 1 && hasMorePosts.value && !route.query.page) {
                router.replace({
                    query: {
                        ...route.query,
                        page: '1'
                    }
                });
            }
            
            const uniqueCreators = [...new Set(posts.value.map(p => p.creatorUid))];
            for (const creatorUid of uniqueCreators) {
                if (creatorUid && creatorUid !== currentUserUid.value) {
                    const displayNameResult = await services.users.getDisplayName(creatorUid);
                    if (displayNameResult.isSuccess && displayNameResult.data) {
                        displayNames.value.set(creatorUid, displayNameResult.data);
                    }
                }
            }
        } else {
            notification.error(result.responseMessage || 'Failed to load posts');
        }
    } catch (error) {
        notification.error('Error loading posts');
    } finally {
        loading.value = false;
    }
};

const getDisplayName = (creatorUid: string): string => {
    if (currentUserUid.value && creatorUid === currentUserUid.value) {
        return 'You';
    }
    return displayNames.value.get(creatorUid) || creatorUid.substring(0, 8) + '...';
};

const viewPost = (postId: string) => {
    router.push({
        path: `/post/${postId}`,
        query: {category: props.postType}
    });
};

const formatDate = (dateString: string) => {
    const date = new Date(dateString + 'Z') || new Date().toISOString();
    return date.toLocaleDateString(navigator.language || 'en-US', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
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

onMounted(() => {
    initializePageFromUrl();
    loadPosts();
});

// Watch search query with debounce
watch(() => props.searchQuery, () => {
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

// Watch filter tags and sort options without debounce (immediate response)
watch([() => props.filterTags, () => props.sortBy], () => {
    currentPage.value = 1; // Reset to first page when filters change
    
    // Remove page parameter temporarily - will be set after loading if needed
    router.push({
        query: {
            ...route.query,
            page: undefined
        }
    });
    
    loadPosts();
}, {immediate: true});

// Cleanup debounce timer on unmount
onBeforeUnmount(() => {
    if (searchDebounceTimer) {
        clearTimeout(searchDebounceTimer);
    }
});
</script>

<template>
    <div class="w-100">
        <div v-if="loading" class="d-flex justify-content-center align-items-center py-5">
            <div class="text-center">
                <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div class="text-muted">Loading posts...</div>
            </div>
        </div>

        <div v-else-if="filteredPosts.length > 0" class="row g-3 g-md-4">
            <div 
                v-for="post in filteredPosts" 
                :key="post.uid"
                class="col-12 col-sm-6 col-lg-4"
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
                                    {{ getDisplayName(post.creatorUid) }}
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
        <div v-if="!loading && (currentPage > 1 || hasMorePosts)" class="d-flex justify-content-center align-items-center gap-3 mt-4">
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
</template>

