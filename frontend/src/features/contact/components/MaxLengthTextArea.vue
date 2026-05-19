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
    v-model="model"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";

const props = defineProps<{
  maxlength: number;
  id: string;
  required?: boolean;
}>();

const textareaRef = ref<HTMLTextAreaElement | null>(null);

const model = defineModel<string>("modelValue", { required: true });

const validate = () => {
  const el = textareaRef.value;
  if (!el) return;

  const val = el.value;
  const length = val.length;

  if (length > props.maxlength) {
    const over = length - props.maxlength;

    el.setCustomValidity(
      `Dit veld bevat ${length} tekens (maximaal ${props.maxlength} toegestaan). Verwijder ${over} teken${over > 1 ? "s" : ""}.`,
    );
  } else {
    el.setCustomValidity("");
  }

  el.reportValidity();
};

watch(model, validate);

onMounted(() => validate());
</script>
