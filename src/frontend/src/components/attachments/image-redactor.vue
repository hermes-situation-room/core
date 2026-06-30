<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue';

/**
 * In-app image redaction (E2EE_DESIGN §2.2.3.2 / §7 spirit: the original never leaves the device).
 * The user draws rectangles over an image to blur or black them out; the redaction is BURNED into
 * the canvas pixels and only the redacted result is exported as a Blob for upload. The source file
 * is never uploaded.
 */
const props = defineProps<{
    file: File;
}>();

const emit = defineEmits<{
    (e: 'confirm', blob: Blob, fileName: string): void;
    (e: 'cancel'): void;
}>();

interface Rect { x: number; y: number; w: number; h: number; mode: 'blur' | 'blackout'; }

const canvasRef = ref<HTMLCanvasElement | null>(null);
const mode = ref<'blur' | 'blackout'>('blur');
const rects = ref<Rect[]>([]);
const loadError = ref(false);
const exporting = ref(false);

let sourceImage: HTMLImageElement | null = null;
let objectUrl: string | null = null;
// Display-to-natural scale so pointer coords map to source pixels.
let scale = 1;
let drawing = false;
let startX = 0;
let startY = 0;
let currentRect: Rect | null = null;

const MAX_DISPLAY_WIDTH = 640;

const loadImage = () => {
    objectUrl = URL.createObjectURL(props.file);
    const image = new Image();
    image.onload = () => {
        sourceImage = image;
        const canvas = canvasRef.value;
        if (!canvas) return;
        const naturalWidth = image.naturalWidth || image.width;
        const naturalHeight = image.naturalHeight || image.height;
        canvas.width = naturalWidth;
        canvas.height = naturalHeight;
        const displayWidth = Math.min(naturalWidth, MAX_DISPLAY_WIDTH);
        scale = displayWidth / naturalWidth;
        canvas.style.width = `${displayWidth}px`;
        canvas.style.height = `${naturalHeight * scale}px`;
        redraw();
    };
    image.onerror = () => { loadError.value = true; };
    image.src = objectUrl;
};

// Repaint the original image, then all committed redactions, then the in-progress rectangle.
const redraw = (preview?: Rect) => {
    const canvas = canvasRef.value;
    const context = canvas?.getContext('2d');
    if (!canvas || !context || !sourceImage) return;
    context.drawImage(sourceImage, 0, 0, canvas.width, canvas.height);
    for (const rect of rects.value) {
        paintRect(context, rect);
    }
    if (preview) {
        context.save();
        context.strokeStyle = '#d63384';
        context.lineWidth = Math.max(2, 2 / scale);
        context.setLineDash([6 / scale, 4 / scale]);
        context.strokeRect(preview.x, preview.y, preview.w, preview.h);
        context.restore();
    }
};

const paintRect = (context: CanvasRenderingContext2D, rect: Rect) => {
    const x = Math.round(rect.x);
    const y = Math.round(rect.y);
    const w = Math.round(rect.w);
    const h = Math.round(rect.h);
    if (w < 1 || h < 1) return;
    if (rect.mode === 'blackout') {
        context.save();
        context.fillStyle = '#000000';
        context.fillRect(x, y, w, h);
        context.restore();
        return;
    }
    // Pixelate (irreversible blur): downscale the region into an offscreen canvas and scale it
    // back up. Burns the destruction into the pixels — there is no original underneath.
    const blocks = Math.max(1, Math.round(Math.min(w, h) / 12));
    const offscreen = document.createElement('canvas');
    offscreen.width = blocks;
    offscreen.height = blocks;
    const offContext = offscreen.getContext('2d');
    if (!offContext) return;
    offContext.drawImage(context.canvas, x, y, w, h, 0, 0, blocks, blocks);
    context.save();
    context.imageSmoothingEnabled = false;
    context.drawImage(offscreen, 0, 0, blocks, blocks, x, y, w, h);
    context.restore();
};

const toCanvasCoords = (event: PointerEvent) => {
    const canvas = canvasRef.value!;
    const bounds = canvas.getBoundingClientRect();
    return {
        x: (event.clientX - bounds.left) / scale,
        y: (event.clientY - bounds.top) / scale,
    };
};

const onPointerDown = (event: PointerEvent) => {
    if (!sourceImage) return;
    drawing = true;
    const point = toCanvasCoords(event);
    startX = point.x;
    startY = point.y;
    canvasRef.value?.setPointerCapture(event.pointerId);
};

const onPointerMove = (event: PointerEvent) => {
    if (!drawing) return;
    const point = toCanvasCoords(event);
    currentRect = {
        x: Math.min(startX, point.x),
        y: Math.min(startY, point.y),
        w: Math.abs(point.x - startX),
        h: Math.abs(point.y - startY),
        mode: mode.value,
    };
    redraw(currentRect);
};

const onPointerUp = () => {
    if (!drawing) return;
    drawing = false;
    if (currentRect && currentRect.w > 3 && currentRect.h > 3) {
        rects.value.push(currentRect);
    }
    currentRect = null;
    redraw();
};

const undo = () => {
    rects.value.pop();
    redraw();
};

const clearAll = () => {
    rects.value = [];
    redraw();
};

const confirmRedaction = () => {
    const canvas = canvasRef.value;
    if (!canvas) return;
    exporting.value = true;
    // Export to the same family; force PNG for lossless burn unless the source was a JPEG.
    const outputType = props.file.type === 'image/jpeg' ? 'image/jpeg' : 'image/png';
    canvas.toBlob((blob) => {
        exporting.value = false;
        if (blob) {
            const baseName = props.file.name.replace(/\.[^.]+$/, '');
            const extension = outputType === 'image/jpeg' ? 'jpg' : 'png';
            emit('confirm', blob, `${baseName}-redacted.${extension}`);
        }
    }, outputType, 0.92);
};

onMounted(loadImage);
onBeforeUnmount(() => {
    if (objectUrl) URL.revokeObjectURL(objectUrl);
});
</script>

<template>
    <div class="modal fade show d-block" tabindex="-1" style="background-color: var(--modal-backdrop); z-index: 1070;">
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <i class="fas fa-user-secret me-2"></i>
                        Redact image before upload
                    </h5>
                    <button type="button" class="btn-close" @click="emit('cancel')"></button>
                </div>
                <div class="modal-body">
                    <div v-if="loadError" class="alert alert-danger">
                        Could not load this image for redaction.
                    </div>
                    <template v-else>
                        <p class="text-muted small mb-2">
                            <i class="fas fa-info-circle me-1"></i>
                            Drag on the image to draw a box over faces or text. The redaction is burned
                            into the pixels on your device — only the redacted image is uploaded.
                        </p>
                        <div class="btn-group btn-group-sm mb-3" role="group">
                            <input id="redactBlur" type="radio" class="btn-check" value="blur" v-model="mode" />
                            <label class="btn btn-outline-primary" for="redactBlur">
                                <i class="fas fa-mask me-1"></i> Pixelate
                            </label>
                            <input id="redactBlackout" type="radio" class="btn-check" value="blackout" v-model="mode" />
                            <label class="btn btn-outline-primary" for="redactBlackout">
                                <i class="fas fa-square me-1"></i> Black out
                            </label>
                        </div>
                        <div class="d-flex justify-content-center">
                            <canvas
                                ref="canvasRef"
                                class="border rounded"
                                style="max-width: 100%; touch-action: none; cursor: crosshair;"
                                @pointerdown="onPointerDown"
                                @pointermove="onPointerMove"
                                @pointerup="onPointerUp"
                                @pointercancel="onPointerUp"
                            ></canvas>
                        </div>
                        <div class="d-flex gap-2 mt-3">
                            <button type="button" class="btn btn-outline-secondary btn-sm" :disabled="!rects.length" @click="undo">
                                <i class="fas fa-undo me-1"></i> Undo
                            </button>
                            <button type="button" class="btn btn-outline-secondary btn-sm" :disabled="!rects.length" @click="clearAll">
                                <i class="fas fa-trash me-1"></i> Clear
                            </button>
                            <span class="ms-auto small text-muted align-self-center">
                                {{ rects.length }} region{{ rects.length === 1 ? '' : 's' }} redacted
                            </span>
                        </div>
                    </template>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" @click="emit('cancel')">Cancel</button>
                    <button
                        type="button"
                        class="btn btn-primary"
                        :disabled="loadError || exporting"
                        @click="confirmRedaction"
                    >
                        <span v-if="exporting" class="spinner-border spinner-border-sm me-2" role="status"></span>
                        Use redacted image
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
