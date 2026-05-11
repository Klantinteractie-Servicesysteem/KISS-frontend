export interface Feedback {
  url: unknown | URL;
  naam: string; //naam artikel. automatisch vullen
  content: string; //tekst waar de feedback betrekking op heeft
  opmerking: string; //de feedback: wat is er mis
  aanleiding: string; //of bron
  contactgegevens: string; // email of telefoonnummer melder
  currentSection: CurrentFeedbackSection;
}

export interface CurrentFeedbackSection {
  label: string;
  id?: string;
}
