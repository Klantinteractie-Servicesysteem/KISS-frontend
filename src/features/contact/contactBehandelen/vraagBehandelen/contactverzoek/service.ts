import { fetchLoggedIn, ServiceResult, throwIfNotOk } from "@/services";
import type {
  ContactVerzoekVragenSet,
  Vraag,
  InputVraag,
  TextareaVraag,
  DropdownVraag,
  CheckboxVraag,
  TypeOrganisatorischeEenheid,
} from "@/features/types";

const contactMomentVragenSets = "/api/contactverzoekvragensets";

type ServerContactVerzoekVragenSet = {
  id: number;
  titel: string;
  jsonVragen: string;
  organisatorischeEenheidId: string;
  organisatorischeEenheidSoort: TypeOrganisatorischeEenheid;
};

///////////////////////////////////////////////
//vragensets

export async function fetchVragenSets(url: string) {
  return fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then((response) => response.json())
    .then((data) => {
      return mapToClientContactVerzoekVragenSets(data);
    });
}

export function useVragenSets() {
  return ServiceResult.fromFetcher(
    () => contactMomentVragenSets,
    fetchVragenSets,
  );
}

function mapToClientContactVerzoekVragenSets(
  serverDataArray: ServerContactVerzoekVragenSet[],
): ContactVerzoekVragenSet[] {
  return serverDataArray.map((serverData) => {
    const parsedQuestions = mapSchemaToVragen(
      safeJSONParse(serverData.jsonVragen, {}),
    );
    return {
      id: serverData.id,
      titel: serverData.titel,
      vraagAntwoord: parsedQuestions,
      organisatorischeEenheidId: serverData.organisatorischeEenheidId,
      organisatorischeEenheidSoort: serverData.organisatorischeEenheidSoort,
    };
  });
}

function safeJSONParse<T>(jsonString: string, defaultValue: T): T {
  try {
    return JSON.parse(jsonString);
  } catch (e) {
    return defaultValue;
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mapSchemaToVragen(schema: any): Vraag[] {
  if (!schema || !schema.properties) {
    return [];
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return Object.values(schema.properties).map((property: any) => {
    const questionType = property.questiontype;

    const baseVraag: Vraag = {
      description: property.description,
      questiontype: questionType,
    };

    switch (questionType) {
      case "dropdown":
        return {
          ...baseVraag,
          options: property.items?.options || [],
          selectedDropdown: "",
        } as DropdownVraag;

      case "checkbox":
        return {
          ...baseVraag,
          options: property.items?.options || [],
          selectedCheckbox: [],
        } as CheckboxVraag;

      case "input":
        return { ...baseVraag, input: "" } as InputVraag;

      case "textarea":
        return { ...baseVraag, textarea: "" } as TextareaVraag;

      default:
        return baseVraag;
    }
  });
}

//////////////////////////////////////////////
