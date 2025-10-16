<script setup lang="ts">
import { ProfileIcon, ProfileIconColor, ICON_MAP, COLOR_MAP } from '../types/profile-icon.ts';

interface ProfileIconSelection {
    icon: ProfileIcon;
    color: ProfileIconColor;
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
const availableColors = Object.values(ProfileIconColor);

function selectIcon(icon: ProfileIcon) {
    emit('update:modelValue', { ...props.modelValue, icon });
}

function selectColor(color: ProfileIconColor) {
    emit('update:modelValue', { ...props.modelValue, color });
}

function getIconClass(icon: ProfileIcon): string {
    const iconKey = icon as keyof typeof ICON_MAP;
    return ICON_MAP[iconKey] || ICON_MAP.User;
}

function getColorHex(color: ProfileIconColor): string {
    return COLOR_MAP[color] || COLOR_MAP[ProfileIconColor.Blue];
}

function getColorName(color: ProfileIconColor): string {
    return color.toString()
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
        <label class="form-label fw-bold">Icon Color</label>
        <div class="row g-2">
          <div 
            v-for="color in availableColors"
            :key="color"
            class="col"
          >
            <button
              type="button"
              class="btn w-100 p-3 position-relative"
              :class="modelValue.color === color ? 'border border-dark border-3' : 'border'"
              :style="{ backgroundColor: getColorHex(color) }"
              @click="selectColor(color)"
              :title="getColorName(color)"
            >
              <i v-if="modelValue.color === color" class="fas fa-check text-white position-absolute top-50 start-50 translate-middle"></i>
            </button>
          </div>
        </div>
      </div>

      <div class="border-top pt-3">
        <label class="form-label fw-bold">Preview</label>
        <div class="text-center bg-light rounded p-4">
          <i 
            :class="['fas', getIconClass(modelValue.icon)]"
            :style="{ color: getColorHex(modelValue.color), fontSize: '4rem' }"
          ></i>
        </div>
      </div>
    </div>
  </div>
</template>
