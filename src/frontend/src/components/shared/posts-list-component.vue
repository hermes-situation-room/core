<script setup lang="ts">
import {computed, onMounted, ref, watch} from 'vue';
import {useRouter} from 'vue-router';
import {services} from '../../services/api';
import type {PostBo, PostFilter} from '../../types/post';
import type {CreateChatRequest} from '../../types/chat';

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
const currentUserUid = computed(() => localStorage.getItem('userUid') || '');

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

    switch (props.sortBy) {
        case 'newest':
            filtered.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
            break;
        case 'oldest':
            filtered.sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime());
            break;
        case 'title-asc':
            filtered.sort((a, b) => a.title.localeCompare(b.title));
            break;
        case 'title-desc':
            filtered.sort((a, b) => b.title.localeCompare(a.title));
            break;
    }

    return filtered;
});

const loadPosts = async () => {
    loading.value = true;
    try {
        const filter: PostFilter = {
            category: props.postType,
            tags: props.filterTags && props.filterTags.length > 0 ? props.filterTags : undefined
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

const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
    });
};

const sendDirectMessage = async (post: PostBo, event: Event) => {
    event.stopPropagation();

    if (!currentUserUid.value) {
        alert('Please log in to send messages');
        return;
    }

    if (post.creatorUid === currentUserUid.value) {
        return;
    }

    try {
        const existingChatResult = await services.chats.getChatByUserPair(
            currentUserUid.value,
            post.creatorUid
        );

        if (existingChatResult.isSuccess && existingChatResult.data) {
            router.push(`/chat/${existingChatResult.data.uid}`);
            return;
        }

        const chatData: CreateChatRequest = {
            user1Uid: currentUserUid.value,
            user2Uid: post.creatorUid
        };

        const createResult = await services.chats.createChat(chatData);
        
        if (createResult.isSuccess && createResult.data) {
            router.push(`/chat/${createResult.data}`);
        } else {
            alert('Failed to create chat. Please try again.');
        }
    } catch (err) {
        console.error('Error creating chat:', err);
        alert('An error occurred while creating the chat');
    }
};

onMounted(() => {
    loadPosts();
});

watch([() => props.searchQuery, () => props.filterTags, () => props.sortBy], () => {
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
                            <small class="text-muted">
                                <i class="fas fa-user me-1"></i>
                                {{ post.creatorUid === currentUserUid ? 'You' : post.creatorUid.substring(0, 8) + '...' }}
                            </small>
                            <button 
                                v-if="post.creatorUid !== currentUserUid && currentUserUid"
                                class="btn btn-primary btn-sm"
                                @click="sendDirectMessage(post, $event)"
                            >
                                <i class="fas fa-comment me-1"></i>
                                Message
                            </button>
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
    </div>
</template>

