import {
  createRouter,
  createWebHistory,
  type NavigationGuard,
} from "vue-router";
import HomeView from "../views/HomeView.vue";
import AfhandelingView from "../views/AfhandelingView.vue";
import ZakenView from "../views/ZakenView.vue";
import KlantenView from "../views/KlantenView.vue";
import KlantDetailView from "../views/KlantDetailView.vue";
import ZaakDetailView from "../views/ZaakDetailView.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { redirectRoute } from "@/features/login";
import BedrijvenView from "@/views/BedrijvenView.vue";

const guardContactMoment: NavigationGuard = (to, from, next) => {
  const contactmoment = useContactmomentStore();
  if (contactmoment.contactmomentLoopt) {
    next();
  } else {
    next("/");
  }
};

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      component: HomeView,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/afhandeling",
      name: "afhandeling",
      component: AfhandelingView,
      beforeEnter: guardContactMoment,
      meta: {
        showNav: false,
        showNotitie: false,
        showSearch: false,
        hideSidebar: true,
      },
    },
    {
      path: "/klanten",
      name: "klanten",
      component: KlantenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/klanten/:klantId",
      name: "klantDetail",
      props: true,
      component: KlantDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/bedrijven",
      name: "bedrijven",
      component: BedrijvenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/zaken",
      name: "zaken",
      component: ZakenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/zaken/:zaakId",
      name: "zaakDetail",
      props: true,
      component: ZaakDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    redirectRoute,
  ],
});

export default router;
