export interface Werkbericht {
  id: string;
  title: string;
  date: Date;
  content: string;
  type: string;
  skills: string[];
  read: boolean;
  url: string;
}
