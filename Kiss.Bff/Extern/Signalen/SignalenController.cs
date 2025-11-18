using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AngleSharp.Io;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Extern.Signalen
{



    [Route("api/signalen")]
    [ApiController]
    public class SignalenController : ControllerBase
    {
        private readonly RegistryConfig _configuration;
        private readonly GetMedewerkerIdentificatie _getMedewerkerIdentificatie;

        public SignalenController(RegistryConfig configuration, GetMedewerkerIdentificatie getMedewerkerIdentificatie)
        {
            _configuration = configuration;
            _getMedewerkerIdentificatie = getMedewerkerIdentificatie;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Postmodel postmodel)
        {


            var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImNmZmJmNGVlMmM0NzAwZWEyYjdjZmQ2NDE5MzQyYmM4NjdkOWFhOTEifQ.eyJpc3MiOiJodHRwczovL21lbGRpbmdlbi5kZW1vLm1laWVyaWpzdGFkLmRlbHRhMTAuY2xvdWQvZGV4Iiwic3ViIjoiRWdWc2IyTmhiQSIsImF1ZCI6InNpZ25hbGVuIiwiZXhwIjoxNzYyODk4NjI2LCJpYXQiOjE3NjI4NTU0MjYsIm5vbmNlIjoiV0JxNmVxSEZJNEk2ekduRm5xRlo4Zz09IiwiYXRfaGFzaCI6Ik1DdFFyWVd6YU5fenRuYzdCNHo0QnciLCJlbWFpbCI6ImFkbWluQG1lbGRpbmdlbi5kZW1vLm1laWVyaWpzdGFkLmRlbHRhMTAuY2xvdWQiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6ImFkbWluIn0.mJ52WxamWsuWgdtvCCVPXCHl_wXwC7VpUlkk-ldqE4AVkUqcOWHV8OTOVdo3Jl_L29h2r5ekdxa3TwWcFYoJeJXMfLCcsphrUVipKcVsfhhn1Oq41kvqvHE19Pgpu1Kg9lOjMEE_TXY2QQgub6BQi5t4HnpBSiuF3ojntLbvHPYgfPMaqtB8PHLjyOQ-O3pHhWG3ZzD2Irro6Y_PLyFEoxf07NufwkhkCvYPGk2z2BPSDSsEVe6xZyZn_WrfWyM1ixHH1V9RuBJpxquB_nebEAyyCvRqe6OPZA77IeieQQt6xPi_dLaegdk6DIP9k6qilAaTv7NnkEMDuW_FRIHshA";
            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);



        

            var response = await client.GetAsync("https://api.meldingen.demo.meierijstad.delta10.cloud/signals/v1/private/categories/");


            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            //"https://api.meldingen.demo.meierijstad.delta10.cloud/signals/v1/private/categories/169"


            var model = new SignaalModel
            {
                Text = postmodel.Text,
                Category = new Category
                {


                    //Category_url = "https://api.meldingen.demo.meierijstad.delta10.cloud/signals/v1/private/categories/169"
                    //,
                    Sub_category = "https://api.meldingen.demo.meierijstad.delta10.cloud/signals/v1/public/terms/categories/overlast-van-dieren/sub_categories/eikenprocessierups"
                    , Text = "de categorie"
                },
                Incident_date_start = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Location = new Location
                {
                    Geometrie = new Geometrie
                    {
                        Coordinates = [5.546464920043946f, 51.6200149693091f]
                    }
                },
                Reporter = new Reporter
                {
                    Email = "mark@icatt.nl"
                }
            };
            //{"location":{"geometrie":["Dit veld is vereist."]},"category":{"category_url":["Ongeldige hyperlink - Ongeldige URL"],"text":["Dit veld mag niet leeg zijn."]}}

            var newSignaalResponse = await client.PostAsJsonAsync("https://api.meldingen.demo.meierijstad.delta10.cloud/signals/v1/private/signals", model);

    

            string newSignaalResponseBody = await newSignaalResponse.Content.ReadAsStringAsync();
            return Ok(newSignaalResponseBody);
        }






      [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            //409eea9c-09ed-4fbc-a660-77cd91000153
            var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImNmZmJmNGVlMmM0NzAwZWEyYjdjZmQ2NDE5MzQyYmM4NjdkOWFhOTEifQ.eyJpc3MiOiJodHRwczovL21lbGRpbmdlbi5kZW1vLm1laWVyaWpzdGFkLmRlbHRhMTAuY2xvdWQvZGV4Iiwic3ViIjoiRWdWc2IyTmhiQSIsImF1ZCI6InNpZ25hbGVuIiwiZXhwIjoxNzYyODk4NjI2LCJpYXQiOjE3NjI4NTU0MjYsIm5vbmNlIjoiV0JxNmVxSEZJNEk2ekduRm5xRlo4Zz09IiwiYXRfaGFzaCI6Ik1DdFFyWVd6YU5fenRuYzdCNHo0QnciLCJlbWFpbCI6ImFkbWluQG1lbGRpbmdlbi5kZW1vLm1laWVyaWpzdGFkLmRlbHRhMTAuY2xvdWQiLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibmFtZSI6ImFkbWluIn0.mJ52WxamWsuWgdtvCCVPXCHl_wXwC7VpUlkk-ldqE4AVkUqcOWHV8OTOVdo3Jl_L29h2r5ekdxa3TwWcFYoJeJXMfLCcsphrUVipKcVsfhhn1Oq41kvqvHE19Pgpu1Kg9lOjMEE_TXY2QQgub6BQi5t4HnpBSiuF3ojntLbvHPYgfPMaqtB8PHLjyOQ-O3pHhWG3ZzD2Irro6Y_PLyFEoxf07NufwkhkCvYPGk2z2BPSDSsEVe6xZyZn_WrfWyM1ixHH1V9RuBJpxquB_nebEAyyCvRqe6OPZA77IeieQQt6xPi_dLaegdk6DIP9k6qilAaTv7NnkEMDuW_FRIHshA";
            var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://api.meldingen.demo.meierijstad.delta10.cloud/signals/v1/private/signals/{id}");

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
           
            return Ok(responseBody);
        }
  



}



    public class Postmodel
    {
        public string Text { get; set; }
    }   

    public class SignalenResponse
    {
        public int Id { get; set; }
        public string Id_display  { get; set; }
        public string Signal_id { get; set; }
        
    }



    public class Location
    {
        public Geometrie Geometrie { get; set; }
     


    }


    public class Geometrie
    {
        public string Type { get => "Point"; }
        public float[] Coordinates { get; set; }
    }

    public class Category
    {
         public string Sub_category { get; set; }
       // public string Category_url { get; set; }
        public string Text { get; set; }
    }

    public class Reporter
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool Sharing_allowed { get; set; }
    }


    public class SignaalModel
    {


        public string Text { get; set; }
        public Location Location { get; set; }
        public Category Category { get; set; }
        public Reporter Reporter { get; set; }
        public string Incident_date_start { get; set; }

        //        {
        //  "source": "string",
        //  "text": "string",
        //  "text_extra": "string",
        //  "status": {
        //    "text": "string",
        //    "state": "m",
        //    "target_api": "sigmax",
        //    "extra_properties": "string",
        //    "send_email": true,
        //    "email_override": "user@example.com"
        //  },
        //  "location": {
        //    "stadsdeel": "A",
        //    "buurt_code": "stri",
        //    "area_type_code": "string",
        //    "area_code": "string",
        //    "area_name": "string",
        //    "address": "string",
        //    "geometrie": {
        //      "type": "Point",
        //      "coordinates": [
        //        12.9721,
        //        77.5933
        //      ]
        //    },
        //    "extra_properties": "string"
        //  },
        //  "category": {
        //    "sub_category": "https://api.example.com/signals/v1/public/terms/categories/1/sub_categories/2/",
        //    "category_url": "https://api.example.com/signals/v1/public/terms/categories/1/sub_categories/2/",
        //    "text": "string"
        //  },
        //  "reporter": {
        //    "email": "user@example.com",
        //    "phone": "string",
        //    "sharing_allowed": true
        //  },
        //  "priority": {
        //    "priority": "low"
        //  },
        //  "type": {
        //    "code": "str",
        //    "created_by": "user@example.com"
        //  },
        //  "incident_date_start": "2025-11-11T11:30:33.355Z",
        //  "incident_date_end": "2025-11-11T11:30:33.355Z",
        //  "operational_date": "2025-11-11T11:30:33.355Z",
        //  "extra_properties": "string",
        //  "notes": [
        //    {
        //      "text": "string"
        //    }
        //  ],
        //  "directing_departments": [
        //    {
        //      "id": 0
        //    }
        //  ],
        //  "routing_departments": [
        //    {
        //      "id": 0
        //    }
        //  ],
        //  "attachments": [
        //    "https://api.example.com/signals/v1/private/signals/1/attachments/1"
        //  ],
        //  "parent": 0,
        //  "assigned_user_email": "user@example.com",
        //  "session": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        //}









    }
}






