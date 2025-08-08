<template>
  <!-- https://web.dev/building-a-toast-component/ -->
  <transition-group name="toast" tag="section">
    <output
      v-for="{ text, type, key, dismiss } in messages"
      :key="key"
      role="status"
      :class="type"
    >
      <span>{{ text }}</span>
      <button
        v-if="dismiss"
        class="icon icon-after xmark icon-only"
        type="button"
        title="Gelezen"
        @click="dismiss"
      />
    </output>
  </transition-group>
</template>

<script lang="ts" setup>
import { messages } from "@/stores/toast";
</script>

<style scoped lang="scss">
section {
  position: fixed;
  z-index: 1;
  inset-block-end: 0;
  inset-inline: 0;
  padding-block-end: 5vh;
  display: grid;
  justify-items: center;
  justify-content: center;
  gap: 1vh;

  &:not(:has(button)) {
    pointer-events: none;
  }
}

output {
  max-inline-size: min(25ch, 90vw);
  padding: var(--spacing-default);
  border-radius: var(--radius-default);
  font-size: 1rem;
  color: var(--color-black);
}

.confirm {
  background-color: var(--color-success);
}

.error {
  background-color: var(--color-error);
}

.toast-move, /* apply transition to moving elements */
.toast-enter-active,
.toast-leave-active {
  transition: all 0.5s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
}

/* ensure leaving items are taken out of layout flow so that moving
   animations can be calculated correctly. */
.toast-leave-active {
  position: absolute;
}

button {
  cursor: pointer;
  float: inline-end;
  block-size: var(--line-height-default);
}
</style>
