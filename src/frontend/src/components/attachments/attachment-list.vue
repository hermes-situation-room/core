<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from 'vue';
import { services } from '../../services/api';
import type { AttachmentDto } from '../../services/api/attachments-api';

/**
 * Renders attachments linked to a post or message. Images/videos are fetched through the
 * AUTHORIZED download endpoint (cookie-credentialed) into object URLs and previewed inline;
 * other files (pdf/docs) are shown as a download link. Optionally allows removing pending
 * attachments (used in editors before/after upload).
 */
const props = withDefaults(defineProps<{
    attachments: AttachmentDto[];
    removable?: boolean;
    compact?: boolean;
}>(), {
    removable: false,
    compact: false,
});

const emit = defineEmits<{
    (e: 'remove', attachment: AttachmentDto): void;
}>();

// uid -> object URL for previewable media we have fetched.
const mediaUrls = ref<Map<string, string>>(new Map());
const loadingUids = ref<Set<string>>(new Set());

const isImage = (attachment: AttachmentDto) => attachment.contentType.startsWith('image/');
const isVideo = (attachment: AttachmentDto) => attachment.contentType.startsWith('video/');
const isPreviewable = (attachment: AttachmentDto) => isImage(attachment) || isVideo(attachment);

const downloadUrl = (uid: string) => services.attachments.getDownloadUrl(uid);

const formatSize = (bytes: number): string => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

const fileIcon = (contentType: string): string => {
    if (contentType === 'application/pdf') return 'fa-file-pdf';
    if (contentType.includes('word')) return 'fa-file-word';
    if (contentType.includes('sheet') || contentType.includes('excel') || contentType === 'text/csv') return 'fa-file-excel';
    if (contentType.includes('presentation') || contentType.includes('powerpoint')) return 'fa-file-powerpoint';
    if (contentType.startsWith('text/')) return 'fa-file-lines';
    return 'fa-file';
};

const fetchMedia = async (attachment: AttachmentDto) => {
    if (mediaUrls.value.has(attachment.uid) || loadingUids.value.has(attachment.uid)) return;
    loadingUids.value.add(attachment.uid);
    try {
        const result = await services.attachments.download(attachment.uid);
        if (result.isSuccess && result.data) {
            mediaUrls.value.set(attachment.uid, URL.createObjectURL(result.data));
        }
    } finally {
        loadingUids.value.delete(attachment.uid);
    }
};

const revokeAll = () => {
    mediaUrls.value.forEach((url) => URL.revokeObjectURL(url));
    mediaUrls.value = new Map();
};

const syncMedia = () => {
    const liveUids = new Set(props.attachments.map(a => a.uid));
    // Revoke URLs for attachments no longer present.
    mediaUrls.value.forEach((url, uid) => {
        if (!liveUids.has(uid)) {
            URL.revokeObjectURL(url);
            mediaUrls.value.delete(uid);
        }
    });
    props.attachments.filter(isPreviewable).forEach(fetchMedia);
};

const forceDownload = async (attachment: AttachmentDto) => {
    const result = await services.attachments.download(attachment.uid);
    if (!result.isSuccess || !result.data) return;
    const url = URL.createObjectURL(result.data);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = attachment.fileName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
};

watch(() => props.attachments.map(a => a.uid).join(','), syncMedia, { immediate: true });
onBeforeUnmount(revokeAll);
</script>

<template>
    <div v-if="attachments.length" class="d-flex flex-wrap gap-2 mt-2">
        <div
            v-for="attachment in attachments"
            :key="attachment.uid"
            class="position-relative border rounded overflow-hidden"
            :style="{ background: 'var(--bg-secondary)', maxWidth: compact ? '160px' : '240px' }"
        >
            <button
                v-if="removable"
                type="button"
                class="btn btn-sm btn-danger position-absolute top-0 end-0 m-1 py-0 px-1"
                style="z-index: 2; line-height: 1.2;"
                title="Remove attachment"
                @click="emit('remove', attachment)"
            >
                <i class="fas fa-times"></i>
            </button>

            <!-- Image preview -->
            <img
                v-if="isImage(attachment) && mediaUrls.get(attachment.uid)"
                :src="mediaUrls.get(attachment.uid)"
                :alt="attachment.fileName"
                class="d-block"
                style="max-width: 100%; max-height: 220px; object-fit: contain;"
            />
            <!-- Video preview -->
            <video
                v-else-if="isVideo(attachment) && mediaUrls.get(attachment.uid)"
                :src="mediaUrls.get(attachment.uid)"
                controls
                class="d-block"
                style="max-width: 100%; max-height: 220px;"
            ></video>
            <!-- Loading placeholder for media still fetching -->
            <div
                v-else-if="isPreviewable(attachment)"
                class="d-flex align-items-center justify-content-center text-muted"
                style="width: 140px; height: 100px;"
            >
                <span class="spinner-border spinner-border-sm"></span>
            </div>
            <!-- Document: icon + download -->
            <button
                v-else
                type="button"
                class="btn d-flex align-items-center gap-2 text-start w-100 p-2"
                style="background: transparent;"
                :title="`Download ${attachment.fileName}`"
                @click="forceDownload(attachment)"
            >
                <i class="fas fa-2x" :class="fileIcon(attachment.contentType)" style="color: var(--link-color);"></i>
                <span class="small text-truncate" style="max-width: 150px;">
                    {{ attachment.fileName }}
                    <span class="d-block text-muted">{{ formatSize(attachment.sizeBytes) }}</span>
                </span>
            </button>

            <!-- Caption / download link under previewable media -->
            <a
                v-if="isPreviewable(attachment)"
                :href="downloadUrl(attachment.uid)"
                target="_blank"
                rel="noopener"
                class="d-block small text-truncate px-2 py-1 text-decoration-none"
                :title="attachment.fileName"
                @click.prevent="forceDownload(attachment)"
            >
                <i class="fas fa-download me-1"></i>{{ attachment.fileName }}
            </a>
        </div>
    </div>
</template>
