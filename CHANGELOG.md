# Change history


## vX.X.X

### New features

- Herontwerp Contactverzoek invoeren #705
- Verplaatsen van OnderwerpLinks #805
- Genereren KISS mockup content [interne data] #977
- Versienummer zichtbaar in de website #978
- Anonieme Contactverzoeken zoeken #810
- Klantcontacten bij een Zaak tonen #809
- Soorten digitaal adres aanpassen naar gebruik enums #891
- Tonen van digitale adressen aanpassen naar gebruik enums #974
- Digitale adressen opslaan conform validatieregels OK2 #939
- Partij-identificatoren aanpassen naar gebruik enums #890
- Contactverzoekformuliertjes / vragensetjes óók voor groepen #954
- Pagesize meegeven bij ophalen contactverzoeken en contactmomenten #896
- VAC items vindbaar maken voor beheer #1004
- VAC items toevoegen #1005
- VAC item bewerken #1006
- VAC beheerfunctionaliteit kunnen verbergen  #1007
- VAC item verwijderen #1008
- Kan geen tekst selecteren binnen harmonica componenten #945


### Warnings and deployment notes
Zie installatiehandleiding voor instructies
- Nieuwe Environment Variabelen nodig: `VAC_OBJECTEN_BASE_URL`, `VAC_OBJECT_TYPE_URL` , `VAC_OBJECT_TYPE_VERSION`, `VAC_OBJECTEN_TOKEN` 
- Minimale lengte van secrets is verhoogd van 16 naar 32 tekens
- Om de functionaliteit rondom beheer van VAC-items te kunnen gebruiken, moet je nieuwe environment variables toevoegen:  `USE_VACS`, én de Deze mag ook ontbreken. 
  
### Bugfixes

- Afdwingen max token lifetime van /api/contactmomentendetails #915
- Duidelijker paginering in /api/contactmomentendetails #914
- Verschillen objecttypes met community concepts wegwerken #986

### Maintenance
 - Upgrade vue from 3.4.31 to 3.5.11  PR #928
 - Upgrade vue from 3.5.11 to 3.5.12 PR #966
 - 3 vulnerabilities in the nuget dependencies of this project PR #927
 - Upgrade ckeditor5 from 42.0.0 to 42.0.2  PR #929 
 - Upgrade ckeditor5 from 42.0.2 to 43.1.1 PR #963
 - Upgrade ckeditor5 from 43.1.1 to 43.3.0 PR #964 
 - Upgrade pinia from 2.1.7 to 2.2.4   PR #930 
 - Upgrade pinia from 2.2.4 to 2.2.5 PR #967
 - Upgrade vue-router from 4.4.0 to 4.4.5 PR #931
 - Upgrade dompurify from 2.5.5 to 2.5.7  PR #965
 - Upgrade .Net naar v8 PR #968
  
