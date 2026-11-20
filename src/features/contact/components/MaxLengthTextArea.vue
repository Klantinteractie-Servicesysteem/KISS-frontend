<template>
  <label :for="id" class="utrecht-form-label">{{ label }}</label>
  <div>
    <p v-if="hint" :id="hintId" class="utrecht-form-field-description">
      {{ hint }}
    </p>
    <p
      v-if="maxlength"
      :id="maxlengthId"
      class="utrecht-form-field-description"
    >
      (maximaal {{ maxlength }} tekens)
    </p>
    <textarea
      ref="textareaRef"
      :id="id"
      class="utrecht-textarea"
      :rows="rows"
      :aria-describedby="describedBy"
      :maxlength="maxlength"
      :required="required"
      v-model="model"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from "vue";

const props = withDefaults(
  defineProps<{
    maxlength: number;
    id: string;
    label?: string;
    hint?: string;
    rows?: number;
    required?: boolean;
  }>(),
  {
    label: "Informatie voor inwoner / bedrijf",
    hint: "Deze tekst kan zichtbaar zijn voor de inwoner of het bedrijf, bijvoorbeeld in een Mijn-omgeving.",
    rows: 7,
  },
);

const hintId = computed(() => `${props.id}-hint`);
const maxlengthId = computed(() => `${props.id}-maxlength`);
const describedBy = computed(
  () =>
    [
      props.hint ? hintId.value : undefined,
      props.maxlength ? maxlengthId.value : undefined,
    ]
      .filter(Boolean)
      .join(" ") || undefined,
);

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
