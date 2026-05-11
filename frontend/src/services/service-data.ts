import { toReactive } from "@vueuse/core";
import useSWRV from "swrv";
import { reactive, computed, watch } from "vue";

const refreshInterval =
  Number.parseInt(import.meta.env.VITE_API_REFRESH_INTERVAL_MS, 10) ||
  undefined;

const logError = import.meta.env.DEV
  ? (e: unknown) => console.error(e)
  :  
    () => {};

// sometimes, a query can return one or zero results.
// in the case of zero results, we can NEVER return undefined from a fetcher.
// because in that case, there is no way to discern between a succesful query with zero results vs a failed or loading query.
export type NotUndefined<T> = T extends undefined ? never : T;

export type ServiceData<T> =
  | {
      submitted: true;
      state: "loading";
      loading: true;
      success: false;
      error: false;
    }
  | {
      submitted: true;
      state: "error";
      error: Error;
      loading: false;
      success: false;
    }
  | {
      submitted: true;
      state: "success";
      data: T;
      loading: false;
      success: true;
      error: false;
    }
  | {
      submitted: false;
      state: "init";
      loading: false;
      success: false;
      error: false;
    };

export type Submitter<TIn, TOut> = ServiceData<TOut> & {
  submit: (params: TIn) => Promise<TOut>;
  reset: () => void;
};

interface FetcherConfig<T> {
  /**
   * data to initialize the ServiceData, so we won't start with a loading state.
   */
  initialData?: NotUndefined<T>;
  /**
   * if the url alone is not enough to identify a unique request, you can supply a function that does this in stead.
   */
  getUniqueId?: () => string;
  poll?: true;
}

export const ServiceResult = {
  success<T>(data: T): ServiceData<T> {
    return {
      state: "success",
      data,
      error: false,
      success: true,
      loading: false,
      submitted: true,
    };
  },
  loading() {
    return {
      state: "loading",
      data: undefined,
      error: false,
      success: false,
      loading: true,
      submitted: true,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as ServiceData<any>;
  },
  error(error: Error) {
    return {
      state: "error",
      data: undefined,
      error,
      success: false,
      loading: false,
      submitted: true,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as ServiceData<any>;
  },
  init() {
    return {
      state: "init",
      data: undefined,
      error: false,
      success: false,
      loading: false,
      submitted: false,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } as ServiceData<any>;
  },

  fromPromise<T = unknown>(
    promise: Promise<NotUndefined<T>>,
  ): ServiceData<NotUndefined<T>> {
    const result = reactive(ServiceResult.loading());

    promise
      .then((r) => {
        Object.assign(result, ServiceResult.success(r));
      })
      .catch((e) => {
        Object.assign(
          result,
          ServiceResult.error(e instanceof Error ? e : new Error(e)),
        );
      });

    return result;
  },

  fromSubmitter<TIn, TOut>(
    submitter: (params: TIn) => Promise<TOut>,
  ): Submitter<TIn, TOut> {
    const result = reactive({
      ...ServiceResult.init(),
      reset() {
        Object.assign(result, ServiceResult.init());
      },
      submit(params: TIn): Promise<TOut> {
        Object.assign(result, ServiceResult.loading());
        return submitter(params)
          .then((r) => {
            Object.assign(result, ServiceResult.success(r));
            return r;
          })
          .catch((e) => {
            Object.assign(
              result,
              ServiceResult.error(e instanceof Error ? e : new Error(e)),
            );
            throw e;
          });
      },
    });
    return result;
  },

  /**
   * @param url either the url or a function to return a dynamic url. this is also used to identify a unique request, unless you supply a function to do this in the config.
   * @param fetcher a function to fetch the data
   * @param config optional configuration for the fetcher
   */
  fromFetcher<T>(
    url: string | (() => string),
    fetcher: (url: string) => Promise<NotUndefined<T>>,
    config?: FetcherConfig<T>,
  ): ServiceData<NotUndefined<T>> & { refresh: () => Promise<void> } {
    const getUrl = typeof url === "string" ? () => url : url;
    const getRequestUniqueId = config?.getUniqueId || getUrl;
    const fetcherWithoutParameters = () => fetcher(getUrl());

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { data, error, mutate } = useSWRV<NotUndefined<T>, any>(
      getRequestUniqueId,
      fetcherWithoutParameters,
      {
        refreshInterval: config?.poll ? refreshInterval : undefined,
        dedupingInterval: 0, //caching levert onvoorspelbaar gedrag op. caching uitzetten kan niet, bij wijze van experiment proberen deduping uitzetten een vergelijkbaar effect heeft
        revalidateOnFocus: false, //gegevens niet automatisch herhalen bij scherm focus: minder onnodige calls, eenvoudiger debuggen, voorspelbaarder gedrag voor gebruiker
      },
    );

    if (data.value === undefined && config?.initialData !== undefined) {
      data.value = config.initialData;
    }

    const requestId = computed(getRequestUniqueId);

    watch(requestId, (n, o) => {
      if (!n || (n && n !== o)) {
        data.value = undefined;
      }
    });

    function getResult(): ServiceData<NotUndefined<T>> {
      const e = error.value;
      if (e) {
        logError(e);
        const errorInstance = e instanceof Error ? e : new Error(e);
        return ServiceResult.error(errorInstance);
      }
      if (!requestId.value) return ServiceResult.init();

      if (data.value === undefined) return ServiceResult.loading();

      return ServiceResult.success(data.value);
    }

    const result = reactive({
      ...ServiceResult.init(),
      refresh: () => mutate(),
    });

    watch(getResult, (r) => Object.assign(result, r), { immediate: true });

    return result;
  },
};

export function mapServiceData<TIn, TOut>(
  input: ServiceData<TIn>,
  mapper: (i: TIn) => TOut,
): ServiceData<TOut> {
  const result = computed(() =>
    !input.success
      ? input
      : {
          ...input,
          data: mapper(input.data),
        },
  );
  return toReactive(result);
}
