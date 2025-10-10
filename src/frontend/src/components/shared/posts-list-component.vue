<script setup lang="ts">
import {computed, onMounted, ref, watch} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../../services/api';
import type {PostBo, PostFilter} from '../../types/post';

interface Props {
    postType: 'activist' | 'journalist';
    searchQuery?: string;
}

const props = withDefaults(defineProps<Props>(), {
    searchQuery: ''
});

const router = useRouter();

const posts = ref<PostBo[]>([]);
const loading = ref(false);

const filteredPosts = computed(() => {
    let filtered = [...posts.value];

    if (props.searchQuery) {
        const query = props.searchQuery.toLowerCase();
        filtered = filtered.filter(post =>
            post.title.toLowerCase().includes(query) ||
            post.content.toLowerCase().includes(query) ||
            post.description.toLowerCase().includes(query) ||
            post.tags.some(tag => tag.toLowerCase().includes(query))
        );
    }

    return filtered;
});

const loadPosts = async () => {
    loading.value = true;
    try {
        const filter: PostFilter = {
            category: props.postType,
            tags: props.searchQuery ? [props.searchQuery] : undefined
        };

        const result = await services.posts.getPostsWithFilter(filter);
        if (result.isSuccess && result.data) {
            posts.value = result.data;
        } else {
            console.error('Failed to load posts:', result.responseMessage);
        }
    } catch (error) {
        console.error('Error loading posts:', error);
    } finally {
        loading.value = false;
    }
};

const viewPost = (postId: string) => {
    router.push({
        path: `/post/${postId}`,
        query: {category: props.postType}
    });
};

const createPost = async () => {
    // TODO: Implement create post functionality
    console.log('Create post clicked');
};

const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
    });
};

onMounted(() => {
    loadPosts();
});

watch(() => props.searchQuery, () => {
    loadPosts();
}, {immediate: true});
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

        <div v-else class="row g-4">
            <div 
                v-for="post in filteredPosts" 
                :key="post.uid"
                class="col-lg-4 col-md-6 col-sm-12"
            >
                <div class="card h-100 shadow-sm" style="cursor: pointer;" @click="viewPost(post.uid)">
                    <div class="card-body">
                        <h5 class="card-title">{{ post.title }}</h5>
                        <p class="card-text text-muted">{{ post.description }}</p>
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
                </div>
            </div>

            <div class="col-lg-4 col-md-6 col-sm-12">
                <div class="card h-100 border-2 border-dashed border-secondary d-flex align-items-center justify-content-center text-center" style="min-height: 200px; cursor: pointer;" @click="createPost">
                    <div class="card-body">
                        <h5 class="card-title">Create New Post</h5>
                        <p class="card-text text-muted">Share your thoughts and ideas with the community</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

