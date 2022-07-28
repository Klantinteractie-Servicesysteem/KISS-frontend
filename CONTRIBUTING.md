# Contribution guidelines

If you want to contribute to this repository, we ask you to follow these guidelines.

## Reporting bugs
If you encounter a bug in this component, please check if an issue already exists in the issue section of this repository.

If such an issue does not exist you can create one [here](/../../issues/new?assignees=&labels=&template=bug_report.md&title=).

Make sure you answer each step in detail.

## Requesting new features
Before requesting a new feature please check if the feature isn't listed in either [roadmap](ROADMAP.md) or the issue tab in Github.

If this is not the case you can create one [here](/../../issues/new?assignees=&labels=&template=feature_request.md&title=).

Make sure you answer each step in detail.

## Forking the repository
To start you must first fork the repository, which can be done [here](/../../fork).
Once this is done you can clone it to your local machine.

## Making the changes
On your local machine, create a new branch on the development branch.
please use the following naming convention for your branch name:
- `issue/issue-number`
- `feature/feature-name`

Once you have made changes or additions to the code, you can commit them (try to keep the commit message descriptive but short). 
Be sure to format your commit message to include the issue number.

## Code quality & conventions
To keep the code clean and readable, we use the following coding standards:
- [Style CI](https://styleci.io/)
- [Better code hub](https://bettercodehub.com/repositories)
- [PSR-02](https://docs.opnsense.org/development/guidelines/psr2.html)
- [Public Code](https://publiccode.net/)

### DocBlocks
We use [DocBlock](https://en.wikipedia.org/wiki/Docblock) annotations in our projects.
Docblock annotations are a tool to embed metadata inside the documentation section which can then be processed by some tool.

A few examples of how we use this in our projects:

```vue
    /**
     * @var string Name of this application
     *
     * @example application name
     */
    private $name;


    /**
     * Function description comes here.
     *
     * @param string $code the code received by id-vault oauth endpoint.
     * @param string $applicationId id of your id-vault application.
     * @param string $secret secret of your id-vault application.
     * @param string $state (optional) A random string used by your application to identify a unique session
     *
     * @return array|false information about what this function returns
     */
    public function authenticateUser(string $code, string $applicationId, string $secret, string $state = '')
    {

    }
```

You can read more about this here:
- [DocBlock](https://en.wikipedia.org/wiki/Docblock)

### Postman
To ensure your changes work we can test this using [postman](https://www.postman.com/).
Make sure to provide a working postman test script in your pull request.

### Testing and acceptence
For all branches, when a pull request is started
- Codebase is checker for vulnerablities trough [dependabot](https://github.com/dependabot)
- [Style CI](https://styleci.io/) is run to determine the quality of psr-02 compliance
- [Better code hub](https://www.bettercodehub.com/) is run to determine codebase quallity
- Containers are build from the codebase
- Containers are checked for vulnerablities trough [snyk](https://snyk.io/)

PR’s can only be accepted when all of the above checks are completed succesfully

Only for development and acceptation afther PR acceptence
- The (symanthic) version number is upped
- The changelog is filled with the released features
- Containers are published as artifact on github
- After each succefull container build the container is  deployed to the appropriate environment. (dev fot  developement and Demodamfor acceptation)
- After the containers  are deployed to the appropriate envirnoment an Pen Test tool (insert link) is activated. The results of the pen test are mailed to the senior developer.
- After each deployment the API’s are tested trough the “api test voorziening”, the results of these test are visable as a badge on de repositories readme.
- After each deployment the frontend is checked for WCAG compliance
- Helm files are publsiched to artifacthub

### Definition of Done (Dutch only)

- De aanpassingen staan op de devomgeving (https://kiss-dev.commonground.nu/) en voldoen aan alle acceptatiecriteria.

- De applicatie voldoet aan de ontwerpprincipes van Gebruiker Centraal.
  De eindgebruiker is betrokken is gedurende het hele project betrokken bij ontwerp en specificatie. De applicatie wordt regelmatig getest door de gebruikers.

- Code is gereviewd zijn door een andere developer.
  Er wordt ontwikkeld in feature branches en gedeployed vanaf de development branch. zodoende is in github inzichtelijk wie aan een story gewerkt heeft en wie een pull request approved heeft.

- Automatische tests
  Alle Automatische tests zijn uitgevoerd en geslaagd (zichtbaar als badges op de readme)

  - Unit Tests (Nieuwe unit tests worden toegevoegd naar inzicht van de developer.)
  - Snyk (vulnerability en OWASP scanning)
  - Docker vulnerablity check
  - Bettercodehub (code quality)
  - Aanwezigheid ongewenste variabelen (tokens, secrets etc) (alleen indien we hier een tool voor kunnen vinden, anders schrappen we dit uit de DoD en wordt het eventueel een audit/review onderdeel)

- Handmatige tests
  Alle handmatige tests zijn uitgevoerd en geslaagd (afgevinkt en bij voorkeur een rapport toegevoegd aan de story)
  
  - Functioneel testen.
  - WCAG. wordt handmatig gecontroleerd mbv een tool.
  - Pentests (frontend en backend) Wordt periodiek uitgevoerd (maandelijks). we maken de afspraak dat resultaten uit een vorige pentest bij de eerstvolgende story opgelost worden. De oplossing wordt echter pas gecontroleerd bij de volgende Pentest.

- Documentatie

  - Documentatie voor contributors: code voorzien in incode documentatie dit wordt bij de code review beoordeeld
  - Documentatie voor eindegebruikers: is geen onderdeel van het project?
  - Documentatie voor implementatiepartners: Changes.md is aangevuld met release notes aan de hand van symantic versioning en per release werken we een implementatie handleiding bij


De DoD is een waarborg voor (een deel van) de architectuurkaders

_Deze Defition of Done is gebaseerd op de inhoud van Discussion #36, op 29-6-2022. _



## Prepare your Pull Request

When preparing your Pull Request you have to keep the following things in mind:
- try not to break backwards compatibility, PRs that break backwards compatibility have less chance to be merged;
- make sure you follow the coding standards defined in the code quality section
- write clear and descriptive commit messages

If your pull request is a feature request make sure to add it to the [changelog](CHANGELOG.md).

In the pull request description, give as much detail as possible about your changes (don’t hesitate to give code examples to illustrate your points). If your pull request is about adding a new feature or modifying an existing one, explain the rationale for the changes. The pull request description helps the code review and it serves as a reference when the code is merged (the pull request description and all its associated comments are part of the merge commit message).
Make sure your pull requests also refers to at least one or more of the issues you worked on.

Whenever you feel that your code is ready for submission you can make a pull request to the development branch.

Keep in mind that when you create a PR you transfer the ownership of your code.

While the prerequisites above must be satisfied prior to having your pull request reviewed, the reviewer(s) may ask you to complete additional design work, tests, or other changes before your pull request can be ultimately accepted.
