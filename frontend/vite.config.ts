import { fileURLToPath, URL } from "url";
import { defineConfig, type ProxyOptions } from "vite";
import vue from "@vitejs/plugin-vue";
//import ckeditor5 from "@ckeditor/vite-plugin-ckeditor5";

const proxyCalls = [
  "/api",
  "/signin-oidc",
  "/signout-callback-oidc",
  "/healthz",
];

const getProxy = (): Record<string, ProxyOptions> | undefined => {
  const target = process.env.BFF_URL;
  if (!target) return undefined;
  const redirectOpts: ProxyOptions = {
    target,
    secure: false,
    headers: {
      "x-forwarded-proto": "https",
    },
  };
  return Object.fromEntries(proxyCalls.map((key) => [key, redirectOpts]));
};

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: getProxy(),
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  build: {
    assetsInlineLimit: 0,
  },
  test: {
    coverage: {
      all: true,
    },
  },
});
