import { asyncComputed } from "@vueuse/core";
import { ref, type Ref } from "vue";

type UseLoaderReturnType<T> = {
  loading: Ref<boolean>;
  error: Ref<any>;
  data: Ref<T | undefined>;
};

/**
 * Wrapper around asyncComputed from vueuse
 * @param handler: async function
 * @returns refs for the loading state, error state and data
 */
export const useLoader = <T>(
  handler: (signal: AbortSignal) => Promise<T> | undefined,
): UseLoaderReturnType<T> => {
  const loading = ref(false);
  const error = ref();

  const data = asyncComputed<T | undefined>(
    (onCancel) => {
      const controller = new AbortController();
      onCancel(() => controller.abort());
      error.value = false;
      return handler(controller.signal);
    },
    undefined,
    {
      onError(e) {
        error.value = e;
      },
      evaluating: loading,
    },
  );
  return {
    loading,
    error,
    data,
  };
};
