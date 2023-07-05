export const berichtTypes = ["Werkinstructie", "Nieuws"] as const;

export type Berichttype = (typeof berichtTypes)[number];

export interface Werkbericht {
  id: string;
  title: string;
  date: Date;
  modified?: Date;
  content: string;
  type: Berichttype;
  skills: { id: number; naam: string }[];
  read: boolean;
  url: string;
  featured: boolean;
}
