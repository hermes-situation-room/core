<script setup lang="ts">
import { computed } from 'vue';
import { ProfileIcon, ProfileIconColor, ICON_MAP, COLOR_MAP } from '../types/profile-icon.ts';

interface Props {
    icon?: string;
    color?: string;
    size?: 'sm' | 'md' | 'lg' | 'xl';
}

const props = withDefaults(defineProps<Props>(), {
    icon: ProfileIcon.User,
    color: ProfileIconColor.Blue,
    size: 'md'
});

const iconClass = computed(() => {
    const iconKey = props.icon as keyof typeof ICON_MAP;
    return ICON_MAP[iconKey] || ICON_MAP.User;
});

const iconColor = computed(() => {
    const colorKey = props.color as ProfileIconColor;
    return COLOR_MAP[colorKey] || COLOR_MAP[ProfileIconColor.Blue];
});

const sizeStyle = computed(() => {
    const sizes = {
        sm: { width: '32px', height: '32px' },
        md: { width: '48px', height: '48px' },
        lg: { width: '64px', height: '64px' },
        xl: { width: '96px', height: '96px' }
    };
    return sizes[props.size];
});

const iconSize = computed(() => {
    const sizes = {
        sm: '16px',
        md: '24px',
        lg: '32px',
        xl: '48px'
    };
    return sizes[props.size];
});
</script>

<template>
  <div 
    class="rounded-circle bg-light d-inline-flex align-items-center justify-content-center"
    :style="sizeStyle"
  >
    <i 
      :class="['fas', iconClass]"
      :style="{ color: iconColor, fontSize: iconSize }"
    ></i>
  </div>
</template>
