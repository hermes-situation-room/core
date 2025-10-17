<script setup lang="ts">
import { ref, watch } from 'vue';
import { DEFAULT_COLORS, DEFAULT_COLOR_NAMES, DEFAULT_COLOR } from '../types/profile-icon.ts';

const props = defineProps<{
    modelValue: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const defaultColors = DEFAULT_COLORS;
const defaultColorNames = DEFAULT_COLOR_NAMES;
const selectedColor = ref(props.modelValue || DEFAULT_COLOR);
const customColor = ref(props.modelValue || DEFAULT_COLOR);

watch(() => props.modelValue, (newValue) => {
    selectedColor.value = newValue;
    customColor.value = newValue;
});

function selectColor(color: string) {
    selectedColor.value = color;
    customColor.value = color;
    emit('update:modelValue', color);
}

function selectCustomColor(event: Event) {
    const target = event.target as HTMLInputElement;
    const color = target.value;
    selectedColor.value = color;
    customColor.value = color;
    emit('update:modelValue', color);
}

function selectCustomColorFromText(event: Event) {
    const target = event.target as HTMLInputElement;
    const color = target.value;

    if (/^#[0-9A-Fa-f]{6}$/.test(color)) {
        selectedColor.value = color;
        customColor.value = color;
        emit('update:modelValue', color);
    }
}
</script>

<template>
    <div>
        <h6 class="mb-3">Choose Color</h6>
        
        <div class="mb-3">
            <div class="row g-2">
                <div v-for="(color, index) in defaultColors" :key="index" class="col-2">
                    <div 
                        class="rounded border"
                        :class="{ 'border-dark border-3': selectedColor === color }"
                        :style="{ backgroundColor: color, height: '40px', cursor: 'pointer' }"
                        @click="selectColor(color)"
                        :title="defaultColorNames[index]"
                    ></div>
                </div>
            </div>
        </div>
        
        <div class="border-top pt-3">
            <label class="form-label small mb-2">Or choose a custom color:</label>
            <div class="d-flex align-items-center gap-2">
                <input 
                    type="color" 
                    class="form-control form-control-color"
                    :value="customColor"
                    @input="selectCustomColor($event)"
                    style="width: 50px; height: 38px;"
                />
                <input 
                    type="text" 
                    class="form-control form-control-sm"
                    :value="customColor"
                    @input="selectCustomColorFromText($event)"
                    placeholder="#000000"
                    maxlength="7"
                    style="max-width: 100px;"
                />
            </div>
        </div>
    </div>
</template>
