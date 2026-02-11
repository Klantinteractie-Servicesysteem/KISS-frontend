import { fetchLoggedIn, throwIfNotOk } from "@/services";
import type { Contactmoment } from "@/services/openklant/types";

export type ContactmomentDetails = Pick<
  Contactmoment,
  | "gespreksresultaat"
  | "startdatum"
  | "einddatum"
  | "vraag"
  | "specifiekevraag"
  | "verantwoordelijkeAfdeling"
> & {
  bronnen: {
    url: string;
    soort: string;
    titel: string;
  }[];
};

export const writeContactmomentDetails = (
  contactmoment: ContactmomentDetails,
  id: string,
): Promise<unknown> =>
  fetchLoggedIn("/api/contactmomentdetails", {
    method: "PUT",
    body: JSON.stringify({
      ...contactmoment,
      id,
    }),
    headers: { "content-type": "application/json" },
  }).then(throwIfNotOk);
