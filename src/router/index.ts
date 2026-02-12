import HomeView from "../views/HomeView.vue";
import AfhandelingView from "../views/AfhandelingView.vue";
import ZakenView from "../views/ZakenView.vue";
import PersonenView from "../views/PersonenView.vue";
import PersoonDetailView from "../views/PersoonDetailView.vue";
import ZaakDetailView from "../views/ZaakDetailView.vue";
import { useContactmomentStore } from "@/stores/contactmoment";
import { redirectRoute } from "@/features/login";
import BedrijvenView from "@/views/BedrijvenView.vue";
import BedrijfDetailView from "@/views/BedrijfDetailView.vue";
import LinksView from "@/views/LinksView.vue";
import ContactenverzoekenView from "@/views/ContactenverzoekenView.vue";
import {
  createRouter,
  createWebHistory,
  type NavigationGuard,
} from "vue-router";
import {
  BEHEER_TAB_PERMISSIONS,
  useUserStore,
  type Permission,
} from "@/stores/user";
//import ContactverzoekenDetailView from "@/views/ContactverzoekenDetailView.vue";

const NieuwsEnWerkinstructiesBeheer = () =>
  import(
    "@/views/Beheer/nieuws-en-werkinstructies/NieuwsEnWerkinstructiesBeheer.vue"
  );
const NieuwsEnWerkinstructieBeheer = () =>
  import(
    "@/views/Beheer/nieuws-en-werkinstructies/NieuwsEnWerkinstructieBeheer.vue"
  );
const SkillsBeheer = () => import("@/views/Beheer/skills/SkillsBeheer.vue");
const SkillBeheer = () => import("@/views/Beheer/skills/SkillBeheer.vue");
const LinksBeheer = () => import("@/views/Beheer/Links/LinksBeheer.vue");
const LinkBeheer = () => import("@/views/Beheer/Links/LinkBeheer.vue");
const GespreksresultaatBeheer = () =>
  import("@/views/Beheer/gespreksresultaten/GespreksresultaatBeheer.vue");
const GespreksresultatenBeheer = () =>
  import("@/views/Beheer/gespreksresultaten/GespreksresultatenBeheer.vue");
const BeheerLayout = () => import("@/views/Beheer/BeheerLayout.vue");
const ContactverzoekFormulierBeheer = () =>
  import(
    "@/views/Beheer/contactverzoek-formulieren/ContactverzoekFormulierBeheer.vue"
  );
const ContactverzoekFormulierenBeheer = () =>
  import(
    "@/views/Beheer/contactverzoek-formulieren/ContactverzoekFormulierenBeheer.vue"
  );

const KanaalBeheer = () => import("@/views/Beheer/Kanalen/KanaalBeheer.vue");
const KanalenBeheer = () => import("@/views/Beheer/Kanalen/KanalenBeheer.vue");
const VacBeheer = () => import("@/views/Beheer/vacs/VacBeheer.vue");
const VacsBeheer = () => import("@/views/Beheer/vacs/VacsBeheer.vue");

const guardContactMoment: NavigationGuard = (to, from, next) => {
  const contactmoment = useContactmomentStore();
  if (contactmoment.contactmomentLoopt) {
    next();
  } else {
    next("/");
  }
};

const guardRequirePermission =
  (permissions: Permission | Permission[]): NavigationGuard =>
  async (to, from, next) => {
    const userStore = useUserStore();
    await userStore.promise;
    if (userStore.user.isLoggedIn && userStore.requirePermission(permissions)) {
      next();
    } else {
      next("/");
    }
  };

const guardBeheertab =
  (permissions: Permission | Permission[]): NavigationGuard =>
  async (to, from, next) => {
    const userStore = useUserStore();
    await userStore.promise;
    if (
      userStore.user.isLoggedIn &&
      userStore.user.permissions.some((p) => permissions.includes(p))
    ) {
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
      path: "/contactverzoeken",
      name: "contactverzoeken",
      component: ContactenverzoekenView,
      beforeEnter: guardContactMoment,
      meta: {
        showNav: true,
        showNotitie: true,
        showSearch: true,
        backTitle: "contactverzoeken zoeken",
      },
    },
    {
      path: "/personen",
      name: "personen",
      component: PersonenView,
      beforeEnter: guardContactMoment,
      meta: {
        showNav: true,
        showNotitie: true,
        showSearch: true,
        backTitle: "personen zoeken",
      },
    },
    {
      path: "/personen/:internalKlantId",
      name: "persoonDetail",
      props: true,
      component: PersoonDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/bedrijven",
      name: "bedrijven",
      component: BedrijvenView,
      beforeEnter: guardContactMoment,
      meta: {
        showNav: true,
        showNotitie: true,
        showSearch: true,
        backTitle: "bedrijven zoeken",
      },
    },
    {
      path: "/bedrijven/:internalKlantId",
      name: "bedrijfDetail",
      props: true,
      component: BedrijfDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/zaken",
      name: "zaken",
      component: ZakenView,
      beforeEnter: guardContactMoment,
      meta: {
        showNav: true,
        showNotitie: true,
        showSearch: true,
        backTitle: "zaken zoeken",
      },
    },
    {
      path: "/zaken/:zaakId",
      name: "zaakDetail",
      // als je props op true zet, worden alleen de path parameters als props meegegeven aan de component
      // op deze manier geldt dit ook voor de query parameters.
      props: ({ query = {}, params = {} }) => ({ ...query, ...params }),
      component: ZaakDetailView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/links",
      name: "links",
      beforeEnter: guardRequirePermission(["linksread"]),
      component: LinksView,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },

    {
      path: "/beheer",
      name: "Beheer",
      component: BeheerLayout,
      beforeEnter: guardBeheertab(BEHEER_TAB_PERMISSIONS),
      props: () => ({}), // Don't pass params to BeheerLayout
      meta: { hideSidebar: true },
      children: [
        {
          path: "NieuwsEnWerkinstructies",
          name: "NieuwsEnWerkinstructiesBeheer",
          beforeEnter: guardRequirePermission("berichtenbeheer"),
          component: NieuwsEnWerkinstructiesBeheer,
          meta: {},
        },
        {
          path: "Skills",
          name: "SkillsBeheer",
          beforeEnter: guardRequirePermission("skillsbeheer"),
          component: SkillsBeheer,
          meta: {},
        },
        {
          path: "Links",
          name: "LinksBeheer",
          beforeEnter: guardRequirePermission("linksbeheer"),
          component: LinksBeheer,
          meta: {},
        },
        {
          path: "gespreksresultaten",
          name: "GespreksresultatenBeheer",
          beforeEnter: guardRequirePermission("gespreksresultatenbeheer"),
          component: GespreksresultatenBeheer,
          meta: {},
        },
        {
          path: "NieuwsEnWerkinstructie/:id?",
          name: "NieuwsEnWerkinstructieBeheer",
          beforeEnter: guardRequirePermission("berichtenbeheer"),
          component: NieuwsEnWerkinstructieBeheer,
          props: true,
          meta: {},
        },
        {
          path: "Skill/:id?",
          name: "SkillBeheer",
          beforeEnter: guardRequirePermission("skillsbeheer"),
          component: SkillBeheer,
          props: true,
          meta: {},
        },
        {
          path: "Link/:id?",
          name: "LinkBeheer",
          beforeEnter: guardRequirePermission("linksbeheer"),
          component: LinkBeheer,
          props: true,
          meta: {},
        },
        {
          path: "gespreksresultaat/:id?",
          name: "GespreksresultaatBeheer",
          beforeEnter: guardRequirePermission("gespreksresultatenbeheer"),
          component: GespreksresultaatBeheer,
          props: true,
          meta: {},
        },
        {
          path: "formulieren-contactverzoek-afdeling",
          name: "FormulierenContactverzoekAfdelingenBeheer",
          beforeEnter: guardRequirePermission(
            "contactformulierenafdelingenbeheer",
          ),
          component: ContactverzoekFormulierenBeheer,
          props: { soort: "afdeling" },
          meta: {},
        },
        {
          path: "formulier-contactverzoek-afdeling/:id?",
          name: "FormulierContactverzoekAfdelingenBeheer",
          beforeEnter: guardRequirePermission(
            "contactformulierenafdelingenbeheer",
          ),
          component: ContactverzoekFormulierBeheer,
          props: (route) => ({
            ...route.params,
            soort: "afdeling",
          }),
          meta: {},
        },
        {
          path: "formulieren-contactverzoek-groep",
          name: "FormulierenContactverzoekGroepenBeheer",
          beforeEnter: guardRequirePermission(
            "contactformulierengroepenbeheer",
          ),
          component: ContactverzoekFormulierenBeheer,
          props: { soort: "groep" },
          meta: {},
        },
        {
          path: "formulier-contactverzoek-groep/:id?",
          name: "FormulierContactverzoekGroepenBeheer",
          beforeEnter: guardRequirePermission(
            "contactformulierengroepenbeheer",
          ),
          component: ContactverzoekFormulierBeheer,
          props: (route) => ({
            ...route.params,
            soort: "groep",
          }),
          meta: {},
        },
        {
          path: "kanalen",
          name: "KanalenBeheer",
          beforeEnter: guardRequirePermission("kanalenbeheer"),
          component: KanalenBeheer,
          meta: {},
        },
        {
          path: "kanaal/:id?",
          name: "KanaalBeheer",
          beforeEnter: guardRequirePermission("kanalenbeheer"),
          component: KanaalBeheer,
          props: true,
          meta: {},
        },
        {
          path: "vacs",
          name: "VacsBeheer",
          beforeEnter: guardRequirePermission("vacsbeheer"),
          component: VacsBeheer,
          meta: {},
        },
        {
          path: "vac/:uuid?",
          name: "VacBeheer",
          beforeEnter: guardRequirePermission("vacsbeheer"),
          component: VacBeheer,
          props: true,
          meta: {},
        },
      ],
    },
    redirectRoute,
  ],
});

router.beforeEach(async (to, from, next) => {
  // Check if the user is logged in for all routes except 'home'
  if (to.name !== "home") {
    const userStore = useUserStore();
    await userStore.promise;
    if (userStore.user.isLoggedIn) {
      next();
    } else {
      next("/");
    }
  } else {
    next();
  }
});

export default router;
