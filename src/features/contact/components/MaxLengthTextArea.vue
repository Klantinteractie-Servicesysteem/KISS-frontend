<template>
  <label :for="id" class="utrecht-form-label">
    Notitie
    <span v-if="maxlength" class="utrecht-form-field-description">
      (maximaal {{ maxlength }} tekens)
    </span>
  </label>
  <textarea
    ref="textareaRef"
    :id="id"
    class="utrecht-textarea"
    :maxlength="maxlength"
    :required="required"
    :value="modelValue"
    @input="onInput"
  />
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from "vue";

const props = defineProps<{
  modelValue: string;
  maxlength: number;
  id: string;
  required?: boolean;
}>();

const emit = defineEmits(["update:modelValue"]);

const textareaRef = ref<HTMLTextAreaElement | null>(null);

const onInput = (e: Event) => {
  const target = e.target as HTMLTextAreaElement;
  emit("update:modelValue", target.value);
  validate();
};

watch(
  () => props.modelValue,
  () => {
    nextTick(() => validate());
  },
  { immediate: true },
);

const validate = () => {
  const el = textareaRef.value;
  if (!el) return;

  const val = el.value;
  const length = val.length;

  if (props.maxlength && length > props.maxlength) {
    const over = length - props.maxlength;
    el.setCustomValidity(
      `Dit veld bevat ${length} tekens (maximaal ${props.maxlength} toegestaan). Verwijder ${over} teken${over > 1 ? "s" : ""}.`,
    );
  } else {
    el.setCustomValidity("");
  }

  el.reportValidity();
};
</script>
