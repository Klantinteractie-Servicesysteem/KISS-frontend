import type {
  ContactmomentKlant,
  ContactmomentState,
} from "@/stores/contactmoment";

export function getKlantInfo(contactmoment: ContactmomentState) {
  const klanten = contactmoment.vragen
    .flatMap(({ klanten }) => klanten)
    .filter(({ shouldStore }) => shouldStore)
    .map(({ klant }) => klant);

  const infos = klanten.map(_getKlantInfo);

  return infos.find((info) => info.name || info.contact);
}

function _getKlantInfo(klant: ContactmomentKlant) {
  const name =
    [klant.voornaam, klant.voorvoegselAchternaam, klant.achternaam]
      .filter(Boolean)
      .join(" ") || klant.bedrijfsnaam;

  const email = klant.emailadres;
  const phone = klant.telefoonnummer;

  const contact = email || phone;

  return {
    name,
    contact,
  };
}
