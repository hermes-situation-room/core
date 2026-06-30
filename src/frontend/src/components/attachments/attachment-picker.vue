<script setup lang="ts">
import { ref } from 'vue';
import { services } from '../../services/api';
import type { AttachmentDto } from '../../services/api/attachments-api';
import { useNotification } from '../../composables/use-notification.ts';
import ImageRedactor from './image-redactor.vue';
import AttachmentList from './attachment-list.vue';

/**
 * Attach-file control for post/message composers. Files are uploaded immediately (UNLINKED); the
 * parent reads `modelValue` (uploaded AttachmentDto[]) and passes their uids as `attachmentIds`
 * on post/message creation, where the backend links them. For IMAGES the in-app redactor opens
 * first so only the redacted pixels are uploaded — the original never leaves the device. For
 * VIDEO, per-frame redaction is out of scope (a follow-up): videos upload as-is with a notice.
 */
const props = withDefaults(defineProps<{
    modelValue: AttachmentDto[];
    disabled?: boolean;
    compact?: boolean;
    // When set, uploads are linked immediately to this post/message (used when the
    // post/message already exists, e.g. editing — no `attachmentIds` round-trip needed).
    linkPostId?: string;
    linkMessageId?: string;
}>(), {
    disabled: false,
    compact: false,
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: AttachmentDto[]): void;
}>();

const notification = useNotification();

const fileInputRef = ref<HTMLInputElement | null>(null);
const uploading = ref(false);
// Image awaiting redaction before upload.
const pendingImage = ref<File | null>(null);
// Remaining selected files to process after the current image is redacted/uploaded.
const pendingQueue = ref<File[]>([]);

const isImageType = (type: string) => type.startsWith('image/');
const isVideoType = (type: string) => type.startsWith('video/');

const openFilePicker = () => {
    if (!props.disabled && !uploading.value) {
        fileInputRef.value?.click();
    }
};

const uploadFile = async (file: File | Blob, fileName: string) => {
    uploading.value = true;
    try {
        const result = await services.attachments.upload(file, {
            fileName,
            postId: props.linkPostId,
            messageId: props.linkMessageId,
        });
        if (result.isSuccess && result.data) {
            emit('update:modelValue', [...props.modelValue, result.data]);
        } else {
            notification.error(result.responseMessage || 'Failed to upload attachment');
        }
    } catch (error) {
        notification.error('Failed to upload attachment');
    } finally {
        uploading.value = false;
    }
};

// Process queued files until an image is hit (handed to the redactor) or the queue drains.
const processQueue = async () => {
    while (pendingQueue.value.length > 0) {
        const file = pendingQueue.value.shift()!;
        if (isImageType(file.type)) {
            // Redact images one at a time; processing resumes from onRedactConfirm/onRedactCancel.
            pendingImage.value = file;
            return;
        }
        if (isVideoType(file.type)) {
            notification.info('Video is uploaded as-is. In-app video redaction is a planned follow-up; trim or redact before selecting.');
        }
        await uploadFile(file, file.name);
    }
};

const onFilesSelected = async (event: Event) => {
    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    // Reset the input so selecting the same file again re-triggers change.
    input.value = '';

    pendingQueue.value.push(...files);
    await processQueue();
};

const onRedactConfirm = async (blob: Blob, fileName: string) => {
    pendingImage.value = null;
    await uploadFile(blob, fileName);
    await processQueue();
};

const onRedactCancel = async () => {
    pendingImage.value = null;
    // Skip the cancelled image and continue with any remaining selected files.
    await processQueue();
};

const removeAttachment = async (attachment: AttachmentDto) => {
    // Best-effort delete on the server (owner-only); always drop from the local list.
    try {
        await services.attachments.delete(attachment.uid);
    } catch {
        // ignore; unlinked orphans are harmless and owner-scoped
    }
    emit('update:modelValue', props.modelValue.filter(a => a.uid !== attachment.uid));
};
</script>

<template>
    <div>
        <input
            ref="fileInputRef"
            type="file"
            class="d-none"
            accept="image/*,video/*,application/pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,text/plain,.csv"
            multiple
            @change="onFilesSelected"
        />
        <button
            type="button"
            class="btn btn-outline-secondary"
            :class="compact ? 'btn-sm' : ''"
            :disabled="disabled || uploading"
            title="Attach a file (images can be redacted before upload)"
            @click="openFilePicker"
        >
            <span v-if="uploading" class="spinner-border spinner-border-sm me-1" role="status"></span>
            <i v-else class="fas fa-paperclip me-1"></i>
            Attach
        </button>

        <AttachmentList
            :attachments="modelValue"
            removable
            :compact="compact"
            @remove="removeAttachment"
        />

        <ImageRedactor
            v-if="pendingImage"
            :file="pendingImage"
            @confirm="onRedactConfirm"
            @cancel="onRedactCancel"
        />
    </div>
</template>
