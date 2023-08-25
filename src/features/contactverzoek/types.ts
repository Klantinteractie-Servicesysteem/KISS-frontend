export interface ContactVerzoekVragenSet {
  id: string;
  naam: string;
  vraagAntwoord: VraagAntwoord[];
  afdelingId: string;
}

export interface VraagAntwoord {
  vraag: string;
  antwoord: string;
}
