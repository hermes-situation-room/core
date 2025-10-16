<script setup lang="ts">
import {ICON_MAP, ProfileIcon} from '../types/profileIcon.ts';
import ColorPicker from './color-picker.vue';

interface ProfileIconSelection {
    icon: ProfileIcon;
    color: string;
}

interface Props {
    modelValue: ProfileIconSelection;
}

interface Emits {
    (e: 'update:modelValue', value: ProfileIconSelection): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const availableIcons = Object.values(ProfileIcon);

function selectIcon(icon: ProfileIcon) {
    emit('update:modelValue', {...props.modelValue, icon});
}

function selectColor(color: string) {
    emit('update:modelValue', {...props.modelValue, color});
}

function getIconClass(icon: ProfileIcon): string {
    const iconKey = icon as keyof typeof ICON_MAP;
    return ICON_MAP[iconKey] || ICON_MAP.User;
}
</script>

<template>
    <div class="card">
        <div class="card-body">
            <div class="mb-3">
                <label class="form-label fw-bold">Profile Icon</label>
                <div class="row g-2">
                    <div
                        v-for="icon in availableIcons"
                        :key="icon"
                        class="col-2"
                    >
                        <button
                            type="button"
                            class="btn w-100 p-2"
                            :class="modelValue.icon === icon ? 'btn-primary' : 'btn-outline-secondary'"
                            @click="selectIcon(icon)"
                            :title="icon"
                        >
                            <i :class="['fas', getIconClass(icon)]"></i>
                        </button>
                    </div>
                </div>
            </div>

            <div class="mb-3">
                <ColorPicker v-model="modelValue.color" @update:modelValue="selectColor"/>
            </div>

            <div class="border-top pt-3">
                <label class="form-label fw-bold">Preview</label>
                <div class="text-center bg-light rounded d-flex align-items-center justify-content-center"
                     style="min-height: 140px; padding: 20px 16px; position: relative;">
                    <div
                        :key="`${modelValue.icon}-${modelValue.color}`"
                        class="d-flex align-items-center justify-content-center"
                        style="width: 100%; height: 100%;"
                    >
                        <i
                            :class="['fas', getIconClass(modelValue.icon)]"
                            :style="{ 
                                color: modelValue.color, 
                                fontSize: '4.5rem',
                                lineHeight: '1',
                                display: 'block',
                                marginTop: '8px'
                            }"
                        ></i>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
