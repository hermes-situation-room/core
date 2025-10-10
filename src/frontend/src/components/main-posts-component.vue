<script setup lang="ts">
import {computed, onMounted, onUnmounted, ref} from 'vue';
import {useRoute, useRouter} from 'vue-router';
import CreatePostModal from './create-post-modal.vue';

const router = useRouter();
const route = useRoute();

const searchQuery = ref('');
const isScrolled = ref(false);
const showCreateModal = ref(false);
const refreshKey = ref(0);

const currentTab = computed(() => {
    const path = route.path;
    if (path.includes('/journalist')) return 'journalist';
    if (path.includes('/activist')) return 'activist';
    return 'journalist';
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
    // Trigger search in child components
    // This will be handled by the child components watching the search query
};

const handleScroll = () => {
    isScrolled.value = window.scrollY > 50;
};

const openCreateModal = () => {
    showCreateModal.value = true;
};

const closeCreateModal = () => {
    showCreateModal.value = false;
};

const handlePostCreated = () => {
    // Refresh the posts list by incrementing the key
    refreshKey.value++;
};

onMounted(() => {
    window.addEventListener('scroll', handleScroll);
});

onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll);
});
</script>

<template>
    <div class="container-fluid">
        <div class="bg-white border-bottom sticky-top" style="z-index: 1000;">
            <div class="container">
                <div class="row align-items-center py-3">
                    <div class="col-auto">
                        <button class="btn btn-outline-secondary">
                            Filter
                        </button>
                    </div>

                    <div class="col-md-4">
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

                    <div class="col-auto">
                        <div class="btn-group" role="group">
                            <button
                                @click="switchTab('journalist')"
                                :class="['btn', currentTab === 'journalist' ? 'btn-dark' : 'btn-outline-dark']"
                            >
                                Journalist
                            </button>
                            <button
                                @click="switchTab('activist')"
                                :class="['btn', currentTab === 'activist' ? 'btn-dark' : 'btn-outline-dark']"
                            >
                                Activist
                            </button>
                        </div>
                    </div>

                    <div class="col-auto ms-auto">
                        <button 
                            class="btn btn-primary me-2"
                            @click="openCreateModal"
                        >
                            <i class="fas fa-plus me-1"></i>
                            Create Post
                        </button>
                        <button class="btn btn-outline-secondary">
                            Sort
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <div class="container py-4">
            <router-view :key="refreshKey" :search-query="searchQuery"/>
        </div>

        <CreatePostModal 
            :show="showCreateModal"
            :post-type="currentTab"
            @close="closeCreateModal"
            @post-created="handlePostCreated"
        />
    </div>
</template>

