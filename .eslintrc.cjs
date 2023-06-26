/* eslint-env node */
require("@rushstack/eslint-patch/modern-module-resolution");

module.exports = {
  root: true,
  extends: [
    "plugin:vue/vue3-essential",
    "eslint:recommended",
    "@vue/eslint-config-typescript/recommended",
    "eslint-config-prettier",
  ],
  env: {
    "vue/setup-compiler-macros": true,
  },
  rules: {
    "vue/multi-word-component-names": [
      "error",
      {
        ignores: ["Pagination", "Paragraph"],
      },
    ],
  },
};
