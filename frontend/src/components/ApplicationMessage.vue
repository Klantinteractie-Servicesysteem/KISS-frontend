<template>
  <article :class="[messageType, { fade }]">
    <slot>{{ message }}</slot>
  </article>
</template>

<script lang="ts" setup>
import { toRefs, onMounted, ref, type PropType } from "vue";

const fade = ref(false);
const props = defineProps({
  messageType: {
    type: String as PropType<"error" | "confirm" | "warning">,
    validator: (value) => {
      return value == "error" || value == "confirm" || value == "warning";
    },
    default: "confirm",
  },
  message: String,
  autoClose: Boolean,
});

const { messageType, message } = toRefs(props);

onMounted(() => {
  setTimeout(() => {
    fade.value = props.autoClose;
  }, 3000);
});
</script>

<style lang="scss" scoped>
article {
  color: var(--color-black);
  padding: var(--spacing-default);
  border-style: solid;
  border-width: 1px;
  border-color: currentcolor;

  --utrecht-paragraph-color: currentcolor;
}

.error {
  background-color: var(--color-error);
}

.confirm {
  background-color: var(--color-accent);
}

.warning {
  background-color: var(--color-warning-background);
  color: var(--color-warning);
}

.fade {
  visibility: hidden;
  opacity: 0;
  transition:
    visibility 0s linear 2000ms,
    opacity 2000ms;
}
</style>
