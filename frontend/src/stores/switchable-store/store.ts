import {
  provideStoreImplementation,
  type CreateStoreParams,
  type Store,
  defaultStoreImplementation,
} from "../create-store";

const savedStateKey = Symbol();

type StateMapValue<T = unknown> = Pick<CreateStoreParams<T>, "stateFactory"> & {
  value: T;
};
type StateMapKey = string;
type StateMap = Map<StateMapKey, StateMapValue>;

export interface Session {
  enable(): void;
}

type _Session = Session & {
  [savedStateKey]: StateMap;
};

let currentSession = _createSession();

function _createSession(): _Session {
  const thisSession: _Session = {
    [savedStateKey]: new Map<StateMapKey, StateMapValue>(),
    enable() {
      if (currentSession === thisSession) return;

      if (!currentSession) {
        currentSession = thisSession;
        return;
      }

      const currentState = currentSession[savedStateKey];
      const thisState = thisSession[savedStateKey];

      for (const [stateId, currentValue] of currentState) {
        const storeValue = defaultStoreImplementation({
          stateId,
          stateFactory: currentValue.stateFactory,
        });

        let thisValue = thisState.get(stateId);

        if (!thisValue) {
          thisValue = {
            ...currentValue,
            value: currentValue.stateFactory(),
          };
          thisState.set(stateId, thisValue);
        }

        currentValue.value = storeValue.value;
        storeValue.value = thisValue.value;
      }

      currentSession = thisSession;
    },
  };

  return thisSession;
}

export function createSession() {
  return _createSession() as Session;
}

export function provideSwitchableStore() {
  provideStoreImplementation(createSwitchableStore);
}

function createSwitchableStore<T>(params: CreateStoreParams<T>): Store<T> {
  const { stateId, stateFactory } = params;

  const store = defaultStoreImplementation(params);

  const sessionState = currentSession[savedStateKey];

  if (!sessionState.has(stateId)) {
    sessionState.set(stateId, {
      stateFactory,
      value: store.value,
    });
  }

  return store as Store<T>;
}
