<template>
  <p
    :class="{
      'utrecht-paragraph': true,
      'utrecht-paragraph--distanced': true,
      'utrecht-paragraph--lead': lead,
    }"
  >
    <slot />
  </p>
</template>
<script setup lang="ts">
defineProps({
  lead: {
    type: Boolean,
    default: false,
  },
});
</script>

<style lang="scss">
/**
 * @license EUPL-1.2
 * Copyright (c) 2021 The Knights Who Say NIH! B.V.
 */

.utrecht-paragraph,
p {
  color: var(--utrecht-paragraph-color, var(--utrecht-document-color, inherit));
  font-family: var(
    --utrecht-paragraph-font-family,
    var(--utrecht-document-font-family, inherit)
  );
  font-size: var(
    --utrecht-paragraph-font-size,
    var(--utrecht-document-font-size, inherit)
  );
  font-weight: var(--utrecht-paragraph-font-weight, inherit);
  line-height: var(
    --utrecht-paragraph-line-height,
    var(--utrecht-document-line-height, normal)
  );
  margin-block: var(--utrecht-paragraph-margin-block-start)
    var(--utrecht-paragraph-margin-block-end);
}

.utrecht-paragraph--lead {
  font-size: var(--utrecht-paragraph-lead-font-size, inherit);
  font-weight: var(--utrecht-paragraph-lead-font-weight, inherit);
  line-height: var(--utrecht-paragraph-lead-line-height, inherit);
}

.utrecht-paragraph--distanced {
  --utrecht-paragraph-margin-block-start: var(
    --utrecht-paragraph-distanced-margin-block-start
  );
  --utrecht-paragraph-margin-block-end: var(
    --utrecht-paragraph-distanced-margin-block-end
  );
}

.denhaag-paragraph--detail {
  font-family: var(--denhaag-posttypelabel-font-family, inherit);
  font-size: var(--denhaag-posttypelabel-font-size, 0.75rem);
  letter-spacing: var(--denhaag-posttypelabel-letter-spacing, 1px);
  line-height: var(--denhaag-posttypelabel-line-height, 1.5);
  text-transform: var(--denhaag-posttypelabel-text-transform, uppercase);
}

.utrecht-paragraph + .utrecht-paragraph,
p + p {
  --utrecht-paragraph-margin-block-start: var(
    --utrecht-paragraph-paragraph-margin-block-start,
    inherit
  );
}

.utrecht-paragraph--lead + .utrecht-paragraph {
  --utrecht-paragraph-margin-block-start: var(
    --utrecht-paragraph-lead-paragraph-margin-block-start,
    inherit
  );
}

.denhaag-ordered-list + .utrecht-paragraph,
.denhaag-unordered-list + .utrecht-paragraph,
.denhaag-list + .utrecht-paragraph,
.denhaag-list__wrapper + .utrecht-paragraph {
  --utrecht-paragraph-margin-block-start: var(
    --utrecht-paragraph-list-margin-block-start,
    inherit
  );
}

.denhaag-ordered-list + .utrecht-paragraph--lead,
.denhaag-unordered-list + .utrecht-paragraph--lead,
.denhaag-list + .utrecht-paragraph--lead,
.denhaag-list__wrapper + .utrecht-paragraph--lead {
  --utrecht-paragraph-margin-block-start: var(
    --utrecht-paragraph-lead-list-margin-block-start,
    inherit
  );
}
</style>
