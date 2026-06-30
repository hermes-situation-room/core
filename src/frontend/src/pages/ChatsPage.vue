<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import ChatsListComponent from '../components/chats-list-component.vue';
import ChatDetailComponent from '../components/chat-detail-component.vue';
import { useRoute } from 'vue-router';

const route = useRoute();

const selectedChatId = ref<undefined | string>(undefined);

onMounted(() => {
    selectedChatId.value = route.params.id as string | undefined;
});

watch(
    () => route.params.id,
    () => selectedChatId.value = route.params.id as string | undefined
)
</script>

<template>
    <!--
      Responsive layout is driven purely by CSS media queries (Bootstrap `md`
      breakpoint at 768px), not by JS viewport measurement.
      - Desktop (>= md): list and detail are shown side by side (1/3 + 2/3).
      - Mobile  (<  md): the list is shown by default; once a chat is selected
        (`chat-open`) the detail takes over full width and the list is hidden.
    -->
    <div class="chats" :class="{ 'chat-open': !!selectedChatId }">
        <div class="chat-list-wrapper">
            <ChatsListComponent v-model:chat-selected="selectedChatId" />
        </div>
        <div class="chat-detail-wrapper">
            <ChatDetailComponent v-model:chat-id="selectedChatId" />
        </div>
    </div>
</template>

<style lang="scss">
.chats {
    display: flex;
    flex-direction: row;
    position: relative;

    .chat-list-wrapper {
        border-right: 1px solid var(--border-color);
        overflow-y: hidden;

        .chat-list-header {
            border-bottom: 1px solid var(--border-color);
            box-shadow: var(--shadow-sm);
        }

        .chat-list {
            height: calc(100vh - 155px);
            overflow-y: auto;
        }
    }

    .chat-detail-wrapper {
        .chat-header {
            border-bottom: 1px solid var(--border-color);
            box-shadow: var(--shadow-sm);
        }
    }

    .scrollable {
        overflow-y: auto;      /* allow vertical scrolling */
        scrollbar-width: none; /* Firefox */
        -ms-overflow-style: none; /* IE 10+ */
    }

    /* Chrome, Safari, Edge */
    .scrollable::-webkit-scrollbar {
        display: none;
    }

    /* Mobile: one pane at a time. Show the list by default; when a chat is
       open, swap to the detail pane full-width. */
    @media (max-width: 767.98px) {
        .chat-list-wrapper {
            flex: 0 0 100%;
            max-width: 100%;
            border-right: none;
        }

        .chat-detail-wrapper {
            display: none;
        }

        &.chat-open {
            .chat-list-wrapper {
                display: none;
            }

            .chat-detail-wrapper {
                display: block;
                flex: 0 0 100%;
                max-width: 100%;
            }
        }
    }

    /* Desktop / tablet: list (1/3) + detail (2/3) side by side. */
    @media (min-width: 768px) {
        .chat-list-wrapper {
            flex: 0 0 calc(100% / 3);
            max-width: calc(100% / 3);
        }

        .chat-detail-wrapper {
            flex: 0 0 calc(100% / 3 * 2);
            max-width: calc(100% / 3 * 2);
        }
    }
}
</style>
