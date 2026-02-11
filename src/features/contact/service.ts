import type {
  CheckboxVraag,
  DropdownVraag,
  InputVraag,
  TextareaVraag,
  Vraag,
} from "@/features/types";

export function isInputVraag(question: Vraag): question is InputVraag {
  return (
    "description" in question &&
    "input" in question &&
    question.description.trim() !== "" &&
    question.questiontype === "input"
  );
}

export function isTextareaVraag(question: Vraag): question is TextareaVraag {
  return (
    question.description.trim() !== "" &&
    "description" in question &&
    question.questiontype === "textarea" &&
    "textarea" in question
  );
}

export function isDropdownVraag(question: Vraag): question is DropdownVraag {
  return (
    question.questiontype === "dropdown" &&
    "description" in question &&
    question.description.trim() !== "" &&
    "options" in question &&
    Array.isArray(question.options) &&
    question.options.length > 0 &&
    !question.options.includes("")
  );
}

export function isCheckboxVraag(question: Vraag): question is CheckboxVraag {
  return (
    question.questiontype === "checkbox" &&
    "description" in question &&
    question.description.trim() !== "" &&
    "options" in question &&
    Array.isArray(question.options) &&
    question.options.length > 0 &&
    !question.options.includes("")
  );
}
