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
      path: "/contactverzoeken",
      name: "contactverzoeken",
      component: ContactenverzoekenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    // {
    //   path: "/contactverzoeken/:contactId",
    //   name: "contactverzoekDetail",
    //   props: true,
    //   component: ContactverzoekenDetailView,
    //   beforeEnter: guardContactMoment,
    //   meta: { showNav: true, showNotitie: true, showSearch: true },
    // },
    {
      path: "/personen",
      name: "personen",
      component: PersonenView,
      beforeEnter: guardContactMoment,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/personen/:persoonId",
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
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },
    {
      path: "/bedrijven/:bedrijfId",
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
    {
      path: "/links",
      name: "links",
      component: LinksView,
      meta: { showNav: true, showNotitie: true, showSearch: true },
    },

    {
      path: "/beheer",
      name: "Beheer",
      component: BeheerLayout,
      props: true,
      meta: {},
      children: [
        {
          path: "NieuwsEnWerkinstructies",
          name: "NieuwsEnWerkinstructiesBeheer",
          component: NieuwsEnWerkinstructiesBeheer,
          meta: {},
        },
        {
          path: "Skills",
          name: "SkillsBeheer",
          component: SkillsBeheer,
          meta: {},
        },
        {
          path: "Links",
          name: "LinksBeheer",
          component: LinksBeheer,
          meta: {},
        },
        {
          path: "gespreksresultaten",
          name: "GespreksresultatenBeheer",
          component: GespreksresultatenBeheer,
          meta: {},
        },
        {
          path: "NieuwsEnWerkinstructie/:id?",
          name: "NieuwsEnWerkinstructieBeheer",
          component: NieuwsEnWerkinstructieBeheer,
          props: true,
          meta: {},
        },
        {
          path: "Skill/:id?",
          name: "SkillBeheer",
          component: SkillBeheer,
          props: true,
          meta: {},
        },
        {
          path: "Link/:id?",
          name: "LinkBeheer",
          component: LinkBeheer,
          props: true,
          meta: {},
        },
        {
          path: "gespreksresultaat/:id?",
          name: "GespreksresultaatBeheer",
          component: GespreksresultaatBeheer,
          props: true,
          meta: {},
        },
        {
          path: "Contactverzoekformulieren",
          name: "ContactverzoekformulierenBeheer",
          component: ContactverzoekFormulierenBeheer,
          meta: {},
        },
        {
          path: "Contactverzoekformulier/:id?",
          name: "Contactverzoekformulier",
          component: ContactverzoekFormulierBeheer,
          props: true,
          meta: {},
        },
        {
          path: "kanalen",
          name: "KanalenBeheer",
          component: KanalenBeheer,
          meta: {},
        },
        {
          path: "kanaal/:id?",
          name: "KanaalBeheer",
          component: KanaalBeheer,
          props: true,
          meta: {},
        },
      ],
    },
    redirectRoute,
  ],
});

export default router;
