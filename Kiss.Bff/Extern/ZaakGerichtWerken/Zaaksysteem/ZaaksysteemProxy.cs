using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem
{
    [ApiController]
    public class ZaaksysteemProxy : ControllerBase
    {
        private readonly IEnumerable<ZaaksysteemConfig> _configs;
        private readonly ILogger<ZaaksysteemProxy> _logger;

        public ZaaksysteemProxy(
            IEnumerable<ZaaksysteemConfig> zakenProxyConfigs,
            ILogger<ZaaksysteemProxy> logger)
        {
            _configs = zakenProxyConfigs;
            _logger = logger;
        }

        /// <summary>
        /// Wordt gebruikt voor het proxien van alle zaaksysteem calls waarbij we al weten in welk zaaksysteem de gegevens zitten
        /// gebruik wanneer de bron niet bekend is een custom functie volgens het stramien van Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.GetZaken
        /// </summary>
        /// <param name="path"></param>
        /// <param name="zaaksysteemId"></param>
        /// <returns></returns>
        [HttpGet("api/zaken/{**path}")]
        [HttpGet("api/documenten/{**path}")]
        public IActionResult Get(
            string path,
            [FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId)
        {
            var config = _configs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();

            if (config == null)
            {
                _logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}", zaaksysteemId);
                return Problem(
                    title: "Configuratieprobleem",
                    detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + zaaksysteemId,
                    statusCode: 500
                );
            }

            return new ProxyResult(() =>
            {
                var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.ApplyZaaksysteemHeaders(config, User);
                return message;
            });
        }





        /// <summary>
        /// Wordt gebruikt voor het proxien van alle zaaksysteem calls waarbij we al weten in welk zaaksysteem de gegevens zitten
        /// gebruik wanneer de bron niet bekend is een custom functie volgens het stramien van Kiss.Bff.Extern.ZaakGerichtWerken.Zaaksysteem.GetZaken
        /// </summary>
        /// <param name="path"></param>
        /// <param name="zaaksysteemId"></param>
        /// <returns></returns>
        [HttpPost("api/zaken/{**path}")]         
        public IActionResult Post(
            string path,
            [FromHeader(Name = "ZaaksysteemId")] string zaaksysteemId,
            [FromBody] object model
            )        {

            //OpenKlant1 contactmomenten kunnen dienen OpenZaak bij de bijbehorende zaak opgeslagen te worden
            //voor OpenKlant2 klantcontacten is deze mogelijkheid er niet en het ligt niet in de lijn der verwachting dat die er nog komt
            //Het posten naar het Zaaksysteem om cotnactmomenten toe te voegen is dus alleen nodig bij gebruik van OpenKlant1, maar... 
            //de enige OpenKlant1 implementatie die ingebruik lijkt te zijn is de e-Suite en die implementeert deze call nou net niet
            //Feitelijk is dit dus zinloos op dit moment.
            //Wellicht komen er in de toekomst wel andere functionaliteiten waarbij we naar OpenZaak moeten kunnen posten. We laten dit als voorwerkt daarvoor staan.

            return Ok();

            //var config = _configs.FilterByZaakSysteemId(zaaksysteemId).SingleOrDefault();

            //if (config == null)
            //{
            //    _logger.LogError("Geen zaaksysteem gevonden voor ZaaksysteemId {ZaaksysteemId}", zaaksysteemId);
            //    return Problem(
            //        title: "Configuratieprobleem",
            //        detail: "Geen zaaksysteem gevonden voor ZaaksysteemId " + zaaksysteemId,
            //        statusCode: 500
            //    );
            //}

            //return new ProxyResult(() =>
            //{
            //    var url = $"{config.BaseUrl.AsSpan().TrimEnd('/')}/{path}{Request?.QueryString}";

 

            //    var message = new HttpRequestMessage(HttpMethod.Post, url)
            //    {
            //        Content = JsonContent.Create(model)
            //    }; ;
        
            //    message.Headers.ApplyZaaksysteemHeaders(config, User);
            //    return message;
            //});




        }
    }
}
