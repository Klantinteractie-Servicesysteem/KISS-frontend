# TabList components

There are three components: `<tab-list>`, `<tab-list-item>` and `<tab-list-data-item>`.
Usage is similar to `ul` and `li`: `<tab-list-item>` and `<tab-list-data-item>` must be contained by a parent `<tab-list>`.
The main use cases are listed below.

```vue
<template>
  <tab-list>
    <tab-list-item label="This label shows up in the first tab">
      <p>This is the first tabpanel</p>
    </tab-list-item>

    <tab-list-item label="Here we use the slot for the tab">
      <template #tab="{ label }">
        <span class="my-special-class">
          {{ label }} this whole span shows up in the second tab
        </span>
      </template>
      <p>This is the second tabpanel</p>
    </tab-list-item>

    <tab-list-item label="This tab is disabled" :disabled="true">
      <p>
        This is the third tabpanel but you can't reach it because it is disabled
      </p>
    </tab-list-item>

    <tab-list-data-item
      label="This is a special tab-list-item. It handles the different states of ServiceData"
      :data="myServiceData"
    >
      <template #success="{ data }">
        <p>
          This is the fourth tabpanel. This only shows up in the success state.
          The value is a boolean in this case: {{ data }}
        </p>
        <p>
          When the ServiceData is in the loading state, we see a spinner in both
          the tabpanel and the tab.
        </p>
        <p>
          When the ServiceData is in the error state, we see a generic error
          message in the tabpanel, and an icon in the tab.
        </p>
      </template>
    </tab-list-data-item>

    <tab-list-data-item
      label="This is another tab-list-data-item, but it is disabled"
      :data="myServiceData"
      :disabled="(data) => data === true"
    >
      <template #success="{ data }">
        <p>
          This is the fifth tabpanel but you can't reach it because it is
          disabled.
        </p>
      </template>
    </tab-list-data-item>
  </tab-list>
</template>

<script lang="ts">
import { TabList, TabListItem, TabListDataItem } from "@components/tabs";
import { ServiceResult } from "@/services";

const myServiceData = ServiceResult.fromPromise(Promise.resolve(true));
</script>
```
