import type {
  ContactmomentKlant,
  ContactmomentState,
} from "@/stores/contactmoment";

export function getKlantInfo(contactmoment: ContactmomentState) {
  const klanten = contactmoment.vragen
    .filter((vraag) => vraag.klantToStore)
    .map((vraag) => vraag.klantToStore);

  const infos = klanten.map(_getKlantInfo);

  // it just rturns the first one.
  // due to circumstances that's almost always the right one
  // however not always. this should be replaced with
  // a proper mechanism to get the info of the 'current' klant
  return infos.find((info) => info.name || info.contact);
}

function _getKlantInfo(klant: ContactmomentKlant | undefined) {
  const name =
    [klant?.voornaam, klant?.voorvoegselAchternaam, klant?.achternaam]
      .filter(Boolean)
      .join(" ") || klant?.bedrijfsnaam;

  const email = klant?.emailadressen.find(Boolean);
  const phone = klant?.telefoonnummers.find(Boolean);

  const contact = email || phone;

  return {
    name,
    contact,
  };
}
