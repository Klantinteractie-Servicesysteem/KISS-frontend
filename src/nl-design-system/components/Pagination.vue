<template>
  <nav class="denhaag-pagination" v-if="pagination.totalPages > 1">
    <a
      :href="previous.href"
      :class="[
        'denhaag-pagination__link',
        'denhaag-pagination__link--arrow',
        { 'denhaag-pagination__link--disabled': !previous.isActive },
      ]"
      :rel="previous.rel"
      :aria-label="previous.ariaLabel"
      :aria-current="previous.isCurrent ? 'page' : false"
      @click="previous.onClick"
    >
      <svg
        aria-hidden="true"
        class="denhaag-icon"
        width="1em"
        height="1em"
        viewBox="0 0 7 12"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path
          d="M4.9921 10.8143C5.36382 11.1914 5.97222 11.1914 6.34393 10.8143C6.7079 10.4451 6.70822 9.8521 6.34466 9.48248L3.36315 6.45123C2.98039 6.06209 2.98039 5.43791 3.36315 5.04877L6.34466 2.01752C6.70822 1.6479 6.7079 1.05492 6.34394 0.685696C5.97222 0.308599 5.36382 0.308599 4.9921 0.685695L0.692003 5.04799C0.308224 5.43732 0.308224 6.06268 0.692003 6.45201L4.9921 10.8143Z"
          fill="currentColor"
        />
      </svg>
    </a>
    <span role="group" class="denhaag-pagination__links">
      <a
        v-for="(currentLink, i) in otherLinks"
        :key="i"
        :class="{
          'denhaag-pagination__link': true,
          'denhaag-pagination__link--current': currentLink.isCurrent,
        }"
        :aria-current="currentLink.isCurrent ? 'page' : false"
        :href="currentLink.href"
        :rel="currentLink.rel"
        :aria-label="currentLink.ariaLabel"
        @click="currentLink.onClick"
      >
        {{ currentLink.label }}
      </a>
    </span>
    <a
      :href="next.href"
      :class="[
        'denhaag-pagination__link',
        'denhaag-pagination__link--arrow',
        { 'denhaag-pagination__link--disabled': !next.isActive },
      ]"
      :rel="next.rel"
      :aria-label="next.ariaLabel"
      :aria-current="next.isCurrent ? 'page' : false"
      @click="next.onClick"
    >
      <svg
        aria-hidden="true"
        class="denhaag-icon"
        width="1em"
        height="1em"
        viewBox="0 0 7 12"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path
          d="M2.0079 1.1857C1.63618 0.8086 1.02778 0.8086 0.656065 1.18569V1.18569C0.292103 1.55492 0.291779 2.1479 0.655339 2.51752L3.63685 5.54877C4.01961 5.93791 4.01961 6.56209 3.63686 6.95123L0.655339 9.98248C0.291779 10.3521 0.292102 10.9451 0.656065 11.3143V11.3143C1.02778 11.6914 1.63618 11.6914 2.0079 11.3143L6.308 6.95201C6.69178 6.56268 6.69178 5.93732 6.308 5.54799L2.0079 1.1857Z"
          fill="currentColor"
        />
      </svg>
    </a>
  </nav>
</template>

<script lang="ts" setup>
import { computed, watch, type PropType } from "vue";
import type { Paginated } from "@/services";
const props = defineProps({
  pagination: {
    type: Object as PropType<Omit<Paginated<unknown>, "page">>,
    required: true,
  },
  queryParamName: {
    type: String,
    default: "page",
  },
});
const emit = defineEmits<{ (e: "navigate", page: number): void }>();

const createLink = (n: number) => {
  const distanceFromCurrent =
    Math.max(props.pagination.pageNumber, n) -
    Math.min(props.pagination.pageNumber, n);
  const link = {
    onClick(e: Event) {
      e.preventDefault();
      if (link.isActive) {
        emit("navigate", n);
      }
    },
    href: `?${props.queryParamName}=${n}`,
    ariaLabel: "Pagina " + n,
    label: n,
    isCurrent: n === props.pagination.pageNumber,
    isActive:
      n === 1 || n === props.pagination.totalPages || distanceFromCurrent === 1,
    rel: undefined,
  };

  return link;
};

const previous = computed(() => ({
  ...createLink(props.pagination.pageNumber - 1),
  rel: "prev",
  ariaLabel: "Vorige pagina",
  isActive: props.pagination.pageNumber > 1,
}));

const next = computed(() => ({
  ...createLink(props.pagination.pageNumber + 1),
  rel: "next",
  ariaLabel: "Volgende pagina",
  isActive: props.pagination.pageNumber < props.pagination.totalPages,
}));

const otherLinks = computed(() => {
  if (
    props.pagination.totalPages < 1 ||
    props.pagination.pageNumber > props.pagination.totalPages
  ) {
    return [];
  }

  const result = [];
  if (props.pagination.pageNumber > 2) {
    result.push(createLink(1));
  }
  if (props.pagination.pageNumber > 3) {
    result.push(createLink(props.pagination.pageNumber - 2));
  }
  if (previous.value?.isActive) {
    result.push(previous.value);
  }
  result.push(createLink(props.pagination.pageNumber));
  if (next.value?.isActive) {
    result.push(next.value);
  }
  if (props.pagination.pageNumber < props.pagination.totalPages - 2) {
    result.push(createLink(props.pagination.totalPages - 1));
  }
  if (props.pagination.totalPages > props.pagination.pageNumber + 1) {
    result.push(createLink(props.pagination.totalPages));
  }
  return result;
});

watch(
  () => props.pagination,
  (pagination) => {
    if (
      pagination.pageNumber < 1 ||
      pagination.pageNumber > pagination.totalPages
    ) {
      emit("navigate", 1);
    }
  },
  { immediate: true }
);
</script>

<style lang="scss">
.denhaag-pagination {
  --denhaag-pagination-align: center;

  align-items: center;
  display: flex;
  flex-direction: row;
  justify-content: var(--denhaag-pagination-align);
}

@media (width >= 640px) {
  .denhaag-pagination,
  .denhaag-pagination--start {
    --denhaag-pagination-align: flex-start;
  }

  .denhaag-pagination--center {
    --denhaag-pagination-align: center;
  }

  .denhaag-pagination--end {
    --denhaag-pagination-align: end;
  }
}

.denhaag-pagination__links {
  display: flex;
  flex-direction: row;
}

.denhaag-pagination__link {
  aspect-ratio: 1 / 1;
  align-items: center;
  background-color: var(--denhaag-pagination-background-color, transparent);
  border-radius: var(
    --denhaag-pagination-border-radius,
    var(--denhaag-border-radius)
  );
  color: var(--denhaag-pagination-color, var(--denhaag-pagination-link-color));
  display: flex;
  height: var(--denhaag-pagination-size);
  justify-content: center;
  position: relative;
  text-align: center;
  text-decoration: none;
  width: var(--denhaag-pagination-size);
  font-style: normal;
  font-weight: var(--denhaag-pagination-font-weight);
  font-size: var(--denhaag-pagination-font-size);
}

.denhaag-pagination__link:hover,
.denhaag-pagination__link--hover {
  --denhaag-pagination-color: var(--denhaag-pagination-link-hover-color);
}

.denhaag-pagination__link:focus,
.denhaag-pagination__link--focus {
  outline: var(--denhaag-link-focus-outline);
  border-radius: 0;
}

.denhaag-pagination__link:disabled,
.denhaag-pagination__link--disabled {
  --denhaag-pagination-color: var(--denhaag-pagination-link-disabled-color);

  pointer-events: none;
}

.denhaag-pagination__link--arrow {
  --denhaag-pagination-size: var(--denhaag-pagination-arrow-size);
  --denhaag-pagination-font-size: var(--denhaag-pagination-arrow-font-size);
}

[dir="rtl"] .denhaag-pagination__link--arrow {
  transform: scaleX(-1);
}

.denhaag-pagination__link + .denhaag-pagination__links,
.denhaag-pagination__links + .denhaag-pagination__link {
  margin-inline-start: var(--denhaag-pagination-margin-inline, 10px);
}

.denhaag-pagination__link:not(:first-child, :last-child, .denhaag-pagination__link--current, [rel]) {
  overflow: hidden;
  pointer-events: none;
  text-indent: calc(var(--denhaag-pagination-size) * 2);
}

.denhaag-pagination__link:not(:first-child, :last-child, .denhaag-pagination__link--current, [rel])::before {
  content: "...";
  left: calc(50% - 7px);
  line-height: 0;
  position: absolute;
  text-indent: 0;
  vertical-align: baseline;
}
.denhaag-pagination .denhaag-icon {
  font-size: inherit;
}
.denhaag-pagination__link--current {
  --denhaag-pagination-color: var(--denhaag-pagination-link-current-color);
  --denhaag-pagination-font-weight: var(
    --denhaag-pagination-link-current-font-weight
  );
}

.denhaag-pagination__link--current.denhaag-pagination__link:disabled,
.denhaag-pagination__link--current.denhaag-pagination__link--disabled {
  --denhaag-pagination-color: var(
    --denhaag-pagination-link-current-disabled-color
  );
  --denhaag-pagination-background-color: var(
    --denhaag-pagination-link-current-disabled-background-color
  );
}
</style>
