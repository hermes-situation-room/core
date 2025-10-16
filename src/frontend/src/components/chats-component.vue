<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import chatsListComponent from './chats-list-component.vue';
import chatDetailComponent from './chat-detail-component.vue';
import { useRoute } from 'vue-router';

const route = useRoute();

onMounted(() => {
    console.log("Mounted");
    selectedChatId.value = route.params.id as string | undefined;
});


const viewportWidth = ref(window.innerWidth);

function updateWidth() {
  viewportWidth.value = window.innerWidth
}

onMounted(() => {
  window.addEventListener('resize', updateWidth)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateWidth)
})

watch(
    () => route.params.id,
    () => selectedChatId.value = route.params.id as string | undefined
)

const isMobile = computed(() => viewportWidth.value < 768);
const isMobileChatOpen = computed(() => isMobile.value && !!selectedChatId.value);
const selectedChatId = ref<undefined | string>(undefined)

</script>

<template>
    <div class="chats" :class="{combined: !isMobile}">
        <div class="chat-list-wrapper"
            :class="{listMobileOpen: isMobile && !isMobileChatOpen}"
        >
            <chats-list-component
                v-if="!isMobile || (isMobile && !isMobileChatOpen)"
                v-model:chat-selected="selectedChatId"
            >
            </chats-list-component>
        </div>
        <div 
            class="chat-detail-wrapper"
            :class="{detailMobileOpen: isMobile && isMobileChatOpen}"
        >
            <chat-detail-component
                v-if="!isMobile || (isMobile && isMobileChatOpen)"
                v-model:chat-id="selectedChatId"
                :is-mobile="isMobile"
            >
            </chat-detail-component>
        </div>
    </div>
</template>

<style lang="scss">
.chats {
    display: flex;
    flex-direction: row;
    position: relative;
    &.combined {
        .chat-detail-wrapper {
            flex: 0 0 calc(100% / 3 * 2);
        }
        .chat-list-wrapper {
            flex: 0 0 calc(100% / 3);
        }
    }
    .chat-detail-wrapper{
        &.detailMobileOpen {
            flex: 0 0 100%;
        }
        .chat-header {
            border-bottom: 1px solid rgba(0,0,0,.125);
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
    }
    .chat-list-wrapper {
        &.listMobileOpen {
            flex: 0 0 100%;
        }
        border-right: 1px solid rgba(0,0,0,.125);
        overflow-y: hidden;
        .chat-list-header {
            border-bottom: 1px solid rgba(0,0,0,.125);
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .chat-list {
            height: calc(100vh - 155px);
            overflow-y: auto;
        }
    }
    .scrollable {
      overflow-y: auto;     /* allow vertical scrolling */
      scrollbar-width: none; /* Firefox */
      -ms-overflow-style: none; /* IE 10+ */
    }

    /* Chrome, Safari, Edge */
    .scrollable::-webkit-scrollbar {
      display: none;
    }
    }
</style>