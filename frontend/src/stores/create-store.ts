import { inject, provide, ref, type InjectionKey } from "vue";

// het type dat de ref functie van vue retourneert is ingewikkeld en kan per versie verschillen
// op deze manier kunnen we het type 'uitrekenen' en verandert het mee met versie updates van vue
const refWithValueSpecified = <T>(v: T) => ref(v);
type RefReturnType<T> = ReturnType<typeof refWithValueSpecified<T>>;
type RefValue<T> = RefReturnType<T>["value"];

const injectionKey = Symbol() as InjectionKey<StoreImplementation>;
const storeMap = new Map<string, Store<unknown>>();

export type CreateStoreParams<T> = {
  stateId: string;
  stateFactory: () => T;
  onExistingState?: (state: RefValue<T>) => void | Promise<void>;
  onNewState?: (state: RefValue<T>) => void | Promise<void>;
};

export type Store<T> = RefReturnType<T> & {
  reset: () => void;
};

export type StoreImplementation = <T>(params: CreateStoreParams<T>) => Store<T>;

export function defaultStoreImplementation<T>({
  stateId,
  stateFactory,
  onExistingState,
  onNewState,
}: CreateStoreParams<T>): Store<T> {
  let store = storeMap.get(stateId) as Store<T> | undefined;

  if (!store) {
    store = Object.assign(ref(stateFactory()), {
      reset() {
        if (store) {
          store.value = stateFactory();
        }
      },
    });

    storeMap.set(stateId, store);
    onNewState?.(store.value);
  } else {
    onExistingState?.(store.value);
  }

  return store;
}

export function provideStoreImplementation(
  implementation: StoreImplementation,
) {
  provide(injectionKey, implementation);
}

export function ensureState<T>(params: CreateStoreParams<T>): Store<T> {
  const implementation = inject(injectionKey, defaultStoreImplementation);
  return implementation(params);
}

export function resetState(stateId: string) {
  const mapValue = storeMap.get(stateId);
  if (mapValue) {
    mapValue.reset();
  }
}

export function resetAllState() {
  for (const store of storeMap.values()) {
    store.reset();
  }
}
