<script setup lang="ts">
import { ref } from 'vue';
import { useTextFormatting } from '../composables/use-text-formatting.ts';

const { formatText, getAllowedColors } = useTextFormatting();

interface Props {
    // The textarea's current content (v-model:modelValue).
    modelValue: string;
    // The id of the textarea this toolbar controls, used to find the element
    // so markup can be inserted at the caret position.
    targetId: string;
    disabled?: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const showPreview = ref(false);
const selectedColor = ref<string>('red');
const allowedColors = getAllowedColors();

const getTextarea = (): HTMLTextAreaElement | null => {
    return document.getElementById(props.targetId) as HTMLTextAreaElement | null;
};

/**
 * Wraps the current selection (or inserts a placeholder) with the given
 * before/after markup at the caret position of the target textarea.
 */
const wrapSelection = (before: string, after: string, placeholder: string) => {
    if (props.disabled) return;

    const textarea = getTextarea();
    const current = props.modelValue;

    if (!textarea) {
        emit('update:modelValue', `${current}${before}${placeholder}${after}`);
        return;
    }

    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = current.slice(start, end) || placeholder;
    const updated = `${current.slice(0, start)}${before}${selected}${after}${current.slice(end)}`;

    emit('update:modelValue', updated);

    // Restore focus and place the caret around the inserted text after the DOM updates.
    const caretStart = start + before.length;
    const caretEnd = caretStart + selected.length;
    requestAnimationFrame(() => {
        textarea.focus();
        textarea.setSelectionRange(caretStart, caretEnd);
    });
};

/**
 * Inserts line-prefixed markup (used for bullet lists) at the caret.
 */
const insertLinePrefix = (prefix: string, placeholder: string) => {
    if (props.disabled) return;

    const textarea = getTextarea();
    const current = props.modelValue;

    if (!textarea) {
        const separator = current && !current.endsWith('\n') ? '\n' : '';
        emit('update:modelValue', `${current}${separator}${prefix}${placeholder}`);
        return;
    }

    const start = textarea.selectionStart;
    const before = current.slice(0, start);
    const separator = before && !before.endsWith('\n') ? '\n' : '';
    const insertion = `${separator}${prefix}${placeholder}`;
    const updated = `${before}${insertion}${current.slice(start)}`;

    emit('update:modelValue', updated);

    const caretEnd = start + insertion.length;
    requestAnimationFrame(() => {
        textarea.focus();
        textarea.setSelectionRange(caretEnd, caretEnd);
    });
};

const applyBold = () => wrapSelection('**', '**', 'bold text');
const applyItalic = () => wrapSelection('*', '*', 'italic text');
const applyBullet = () => insertLinePrefix('- ', 'list item');
const applyColor = () => wrapSelection(`[color=${selectedColor.value}]`, '[/color]', 'colored text');

const togglePreview = () => {
    showPreview.value = !showPreview.value;
};
</script>

<template>
    <div class="formatting-toolbar">
        <div class="btn-toolbar align-items-center gap-2 mb-2" role="toolbar" aria-label="Text formatting">
            <div class="btn-group btn-group-sm" role="group">
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    title="Bold (**text**)"
                    :disabled="disabled"
                    @click="applyBold"
                >
                    <i class="fas fa-bold"></i>
                </button>
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    title="Italic (*text*)"
                    :disabled="disabled"
                    @click="applyItalic"
                >
                    <i class="fas fa-italic"></i>
                </button>
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    title="Bullet list (- item)"
                    :disabled="disabled"
                    @click="applyBullet"
                >
                    <i class="fas fa-list-ul"></i>
                </button>
            </div>

            <div class="btn-group btn-group-sm" role="group">
                <button
                    type="button"
                    class="btn btn-outline-secondary"
                    :title="`Color text ([color=${selectedColor}]...[/color])`"
                    :disabled="disabled"
                    @click="applyColor"
                >
                    <i class="fas fa-palette" :style="{ color: selectedColor }"></i>
                    Color
                </button>
                <select
                    class="form-select form-select-sm color-select"
                    v-model="selectedColor"
                    :disabled="disabled"
                    aria-label="Select text color"
                >
                    <option v-for="color in allowedColors" :key="color" :value="color">
                        {{ color }}
                    </option>
                </select>
            </div>

            <button
                type="button"
                class="btn btn-sm"
                :class="showPreview ? 'btn-secondary' : 'btn-outline-secondary'"
                :disabled="disabled"
                @click="togglePreview"
            >
                <i class="fas fa-eye me-1"></i>
                {{ showPreview ? 'Hide preview' : 'Preview' }}
            </button>
        </div>

        <p class="form-text mb-2">
            <i class="fas fa-info-circle me-1"></i>
            Select text then click a button, or type markup directly:
            <code>**bold**</code>, <code>*italic*</code>, <code>- list</code>,
            <code>[color=red]text[/color]</code>.
        </p>

        <div v-if="showPreview" class="preview-pane border rounded p-3 mb-2">
            <div class="text-muted small mb-2">
                <i class="fas fa-eye me-1"></i>Live preview
            </div>
            <div
                v-if="modelValue.trim()"
                class="formatted-content"
                v-html="formatText(modelValue)"
            ></div>
            <div v-else class="text-muted fst-italic">Nothing to preview yet.</div>
        </div>
    </div>
</template>

<style scoped>
.preview-pane {
    background-color: var(--bg-secondary);
    border-color: var(--border-color) !important;
    color: var(--text-primary);
}

.color-select {
    max-width: 7.5rem;
}

.formatting-toolbar :deep(.formatted-content ul) {
    margin-bottom: 0;
}
</style>
