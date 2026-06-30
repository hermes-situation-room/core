<script setup lang="ts">
import { useConfirm } from '../../composables/use-confirm';

const { current, accept, cancel } = useConfirm();
</script>

<template>
    <div
        v-if="current"
        class="modal fade show d-block"
        tabindex="-1"
        style="background-color: var(--modal-backdrop); z-index: 1070;"
        @click.self="cancel"
    >
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <i
                            v-if="current.danger"
                            class="fas fa-exclamation-triangle text-danger me-2"
                        ></i>
                        {{ current.title }}
                    </h5>
                    <button
                        type="button"
                        class="btn-close"
                        aria-label="Close"
                        @click="cancel"
                    ></button>
                </div>
                <div class="modal-body">
                    <p class="mb-0">{{ current.message }}</p>
                </div>
                <div class="modal-footer">
                    <button
                        type="button"
                        class="btn btn-secondary"
                        @click="cancel"
                    >
                        {{ current.cancelLabel }}
                    </button>
                    <button
                        type="button"
                        class="btn"
                        :class="current.danger ? 'btn-danger' : 'btn-primary'"
                        @click="accept"
                    >
                        {{ current.confirmLabel }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
