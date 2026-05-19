/**
 * This is a generic way to enrich data with other data. Look at `@/features/klant/persoon-enricher.ts` for an example
 * We need this because of limitations in the library we use for caching, SWRV.
 * This library does not allow dynamic parallel queries, for example based on a list of records.
 * There is another popular library, `vue-query`, that does support this use case.
 * It's probably a good idea to make the switch at some point, because we could get rid of a lot of custom code.
 * *
 */
import { toReactive } from "@vueuse/core";
import { computed, type Ref } from "vue";
import { type ServiceData, ServiceResult } from "./service-data";

type GetParameters<K, I> = (i: NonNullable<I>) => NonNullable<K> | undefined;

type GetServiceData<K, O> = (
  keyFactory: () => NonNullable<K> | undefined,
) => ServiceData<NonNullable<O> | null>;

type GetInput<I> = () => NonNullable<I>;

export type TwoWayEnricher<Input, Parameters, Output> = (
  getInput: GetInput<Input | Output>,
) => readonly [
  Readonly<Ref<NonNullable<Parameters> | undefined>>,
  ServiceData<NonNullable<Input> | null>,
  ServiceData<NonNullable<Output> | null>,
];

/**
 * Use this function to combine two enrichers that enrich data in the opposite direction, using the same parameters.
 * The result is an enricher that can work in both directions.
 * Look at `@/features/klant/persoon-enricher.ts` for an example
 * @param getDataOneDirection given a factory to get the common parameters, how do we get data of the first type?
 * @param getDataTheOtherDirection given a factory to get the common parameters, how do we get data of the other type?
 * @param getCommonParameters given either of the two types of data, how do we get to the common parameters?
 * @param isLeft given either of the two types of data, how do we know if we're dealing with the left one?
 */
export function combineEnrichers<Left, Right, Parameters>(
  getDataOneDirection: GetServiceData<Parameters, Left>,
  getDataTheOtherDirection: GetServiceData<Parameters, Right>,
  getCommonParameters: GetParameters<Parameters, Left | Right>,
  isLeft: (
    either: NonNullable<Left> | NonNullable<Right>,
  ) => either is NonNullable<Left>,
): TwoWayEnricher<Left, Parameters, Right> {
  return (getEither) => {
    const eitherRef = computed(getEither);

    const parametersRef = computed(() => {
      const either = eitherRef.value;
      return either === undefined ? undefined : getCommonParameters(either);
    });

    const leftData = getDataOneDirection(() => parametersRef.value);
    const rightData = getDataTheOtherDirection(() => parametersRef.value);

    function getLeft(): ServiceData<NonNullable<Left> | null> {
      const either = eitherRef.value;
      if (either !== undefined && isLeft(either))
        return ServiceResult.success(either);
      return leftData;
    }

    function getRight(): ServiceData<NonNullable<Right> | null> {
      const either = eitherRef.value;
      if (either !== undefined && !isLeft(either))
        return ServiceResult.success(either);
      return rightData;
    }

    const left = toReactive(computed(getLeft));
    const right = toReactive(computed(getRight));

    return [parametersRef, left, right];
  };
}
