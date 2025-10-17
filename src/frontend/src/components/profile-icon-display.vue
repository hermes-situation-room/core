<script setup lang="ts">
import { computed } from 'vue';
import { ProfileIcon, ICON_MAP, DEFAULT_COLOR } from '../types/profile-icon.ts';

interface Props {
    icon?: string;
    color?: string;
    size?: 'sm' | 'md' | 'lg' | 'xl';
}

const props = withDefaults(defineProps<Props>(), {
    icon: ProfileIcon.User,
    color: DEFAULT_COLOR,
    size: 'md'
});

const iconClass = computed(() => {
    const iconKey = props.icon as keyof typeof ICON_MAP;
    return ICON_MAP[iconKey] || ICON_MAP.User;
});

const iconColor = computed(() => {
    if (props.color && /^#[0-9A-Fa-f]{6}$/.test(props.color)) {
        return props.color;
    }
    return DEFAULT_COLOR;
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
