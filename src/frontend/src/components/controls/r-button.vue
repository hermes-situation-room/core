<script setup lang="ts">
import { defineProps, defineEmits, computed } from 'vue';
import router from '../../router';

// Define props for the component
const props = defineProps({
  label: {
    type: String,
    required: true
  },
  icon: {
    type: String,
    required: false
  },
  to: {
    type: String,
    required: false,
  },
  classes: {
    type: String,
    required: false
  }
});

// Define emits for the component
const emit = defineEmits(['click']);

// Handle button click event
const handleClick = () => {
  if (props.to) {
    router.push(props.to);
  }
  emit('click');
};

</script>

<template>
  <div class="r-button btn" :class="classes">
    <i v-if="icon" class="fa" :class="`fa-${icon}`"></i>
    <button @click="handleClick">{{ label }}</button>
  </div>
</template>