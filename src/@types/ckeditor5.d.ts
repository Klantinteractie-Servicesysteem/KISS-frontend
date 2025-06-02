declare module 'ckeditor5' {
  export const ClassicEditor: any;
  export const Essentials: any;
  export const Autoformat: any;
  export const Bold: any;
  export const Italic: any;
  export const BlockQuote: any;
  export const Heading: any;
  export const Link: any;
  export const List: any;
  export const Paragraph: any;
  export const Table: any;
  export const TableToolbar: any;
  
  export interface EditorConfig {
    toolbar?: {
      items?: string[];
      [key: string]: any;
    };
    [key: string]: any;
  }
}
