import { flushPromises } from "@vue/test-utils";
import { combineEnrichers, ServiceResult, type ServiceData } from "@/services";

import { describe, expect, test } from "vitest";

describe("service-data-enricher", () => {
  test("should attempt to retrieve data of type Right, when the input is of type Left", async () => {
    type LeftType = {
      _typeOf: "left";
      dataLeftOnly: string;
      dataBoth: string;
    };

    type RightType = {
      _typeOf: "right";
      dataRightOnly: string;
      dataBoth: string;
    };

    function left(): ServiceData<LeftType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "bothLeft",
          dataLeftOnly: "left",
          _typeOf: "left",
        })
      );
    }

    function right(): ServiceData<RightType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "bothRight",
          dataRightOnly: "right",
          _typeOf: "right",
        })
      );
    }

    const isLeft = (
      leftOrRight: LeftType | RightType
    ): leftOrRight is LeftType => {
      return leftOrRight._typeOf === "left";
    };

    const testEnricher = combineEnrichers(
      left,
      right,
      (either) => either.dataBoth, //wordt als parameter meegegeven bij het ophalen van het andere object. afhankelijk van de input die je meegeeft aan de functie die combineEnrichers creeert is dat left of right
      isLeft
    );

    const [common, leftData, rightData] = testEnricher(() => {
      return {
        _typeOf: "left",
        dataLeftOnly: "leftInput",
        dataBoth: "bothLeftInput",
      };
    });

    //de params die door de enricher gebruikt zijn om de andere kant op te halen
    expect(common.value).toMatch("bothLeftInput");

    //de aan de testenricher meegegeven data is gebruikt voor de leftdata
    expect((leftData as any)["data"].dataLeftOnly).toMatch("leftInput");

    //de aan de testenricher meegegeven data is gebruikt voor de leftdata
    expect((leftData as any)["data"].dataBoth).toMatch("bothLeftInput");

    //left bevat de data die aan testenricher meegegeven is. right gebruikt de data die de right functie retourneert.
    //right retourneert een promise, die moeten we een zetje geven:
    await flushPromises();

    //de door right geretourneerde data is gebruikt voor de rightdata
    expect((rightData as any)["data"].dataRightOnly).toMatch("right");

    //de door right geretourneerde data is gebruikt voor de rightdata
    expect((rightData as any)["data"].dataBoth).toMatch("bothRight");

    //de aan de testenricher meegegeven data is gebruikt voor de leftdata
    expect(leftData.success).toBeTruthy();
    expect(rightData.success).toBeTruthy();
  });

  test("should attempt to retrieve data of type Left, when the input is of type Right", async () => {
    type LeftType = {
      _typeOf: "left";
      dataLeftOnly: string;
      dataBoth: string;
    };

    type RightType = {
      _typeOf: "right";
      dataRightOnly: string;
      dataBoth: string;
    };

    function left(): ServiceData<LeftType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "bothLeft",
          dataLeftOnly: "left",
          _typeOf: "left",
        })
      );
    }

    function right(): ServiceData<RightType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "bothRight",
          dataRightOnly: "right",
          _typeOf: "right",
        })
      );
    }

    const isLeft = (
      leftOrRight: LeftType | RightType
    ): leftOrRight is LeftType => {
      return leftOrRight._typeOf === "left";
    };

    const testEnricher = combineEnrichers(
      left,
      right,
      (either) => either.dataBoth, //wordt als parameter meegegeven bij het ophalen van het andere object. afhankelijk van de input die je meegeeft aan de functie die combineEnrichers creeert is dat left of right
      isLeft
    );

    const [common, leftData, rightData] = testEnricher(() => {
      return {
        _typeOf: "right",
        dataRightOnly: "rightInput",
        dataBoth: "bothRightInput",
      };
    });

    //de params die door de enricher gebruikt zijn om de andere kant op te halen
    expect(common.value).toMatch("bothRightInput");

    //de door right geretourneerde data is gebruikt voor de rightdata
    expect((rightData as any)["data"].dataRightOnly).toMatch("rightInput");

    //de door right geretourneerde data is gebruikt voor de rightdata
    expect((rightData as any)["data"].dataBoth).toMatch("bothRightInput");

    //left bevat de data die aan testenricher meegegeven is. right gebruikt de data die de right functie retourneert.
    //right retourneert een promise, die moeten we een zetje geven:
    await flushPromises();

    //de aan de testenricher meegegeven data is gebruikt voor de leftdata
    expect((leftData as any)["data"].dataLeftOnly).toMatch("left");

    //de aan de testenricher meegegeven data is gebruikt voor de leftdata
    expect((leftData as any)["data"].dataBoth).toMatch("bothLeft");

    //de aan de testenricher meegegeven data is gebruikt voor de leftdata
    expect(leftData.success).toBeTruthy();
    expect(rightData.success).toBeTruthy();
  });

  test("should return the input, in side dictated by the isLeft function. If Isleft returns the wrong answer the other side wil be empty", async () => {
    type LeftType = {
      _typeOf: "left";
      dataLeftOnly: string;
      dataBoth: string;
    };

    type RightType = {
      _typeOf: "right";
      dataRightOnly: string;
      dataBoth: string;
    };

    function left(): ServiceData<LeftType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "bothLeft",
          dataLeftOnly: "left",
          _typeOf: "left",
        })
      );
    }

    function right(): ServiceData<RightType> {
      return ServiceResult.fromPromise(
        Promise.resolve({
          dataBoth: "bothRight",
          dataRightOnly: "right",
          _typeOf: "right",
        })
      );
    }

    const isLeft = (
      leftOrRight: LeftType | RightType
    ): leftOrRight is LeftType => {
      return leftOrRight._typeOf === "right"; //wrong!
    };

    const testEnricher = combineEnrichers(
      left,
      right,
      (either) => either.dataBoth, //wordt als parameter meegegeven bij het ophalen van het andere object. afhankelijk van de input die je meegeeft aan de functie die combineEnrichers creeert is dat left of right
      isLeft
    );

    const [common, leftData, rightData] = testEnricher(() => {
      return {
        _typeOf: "left",
        dataLeftOnly: "leftInput",
        dataBoth: "bothLeftInput",
      };
    });

    //de params die door de enricher gebruikt zijn om de andere kant op te halen
    expect(common.value).toMatch("bothLeftInput");

    //de geinjecteerde data wordt nu als rightdata geretourneerd en de left data is undefined (is dit echt wenselijk?)
    expect((rightData as any)["data"].dataLeftOnly).toMatch("leftInput");
    expect((leftData as any)["data"]).toBeUndefined();

    expect(leftData.success).toBeFalsy();
    expect(rightData.success).toBeTruthy();
  });
});
