export function fullName({
  voorletters = "",
  voornaam = "",
  voorvoegselAchternaam = "",
  achternaam = "",
} = {}) {
  return [voornaam || voorletters, voorvoegselAchternaam, achternaam]
    .filter(Boolean)
    .join(" ");
}
