using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;
using static System.Net.HttpStatusCode;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Net;
using Kiss.Bff.ZaakGerichtWerken.Contactmomenten;
using Kiss.Bff.Beheer.Faq;
using Kiss.Bff.Beheer.Data;
using Microsoft.EntityFrameworkCore;

using static Kiss.Bff.Intern.Links.Features.LinksController;
using Kiss.Bff.NieuwsEnWerkinstructies.Features;
using Kiss.Bff.Beheer.Verwerking;
using Kiss.Bff.Intern.Links.Features;
using Kiss.Bff.Intern.Gespreksresultaten.Features;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class AuthorizationCheckTests
    {
        private static WebApplicationFactory<Program> s_factory = null!;
        private static HttpClient s_client = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            s_factory = new CustomWebApplicationFactory();
            s_client = s_factory.CreateDefaultClient();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            s_client?.Dispose();
            s_factory?.Dispose();
        }

        public static IEnumerable<object[]> GetControllersWithAuthorizeAttributeAndMethods()
        {
            // Define the controllers and methods to test here
            var controllersWithMethodsToTest = new List<(Type controllerType, string methodName, Type[] parameterTypes)>
                {
                     (typeof(ReadContactmomentenDetails), "Get", new[] { typeof(string), typeof(string), typeof(CancellationToken), typeof(int), typeof(int) }),
                    (typeof(GespreksresultatenController), "PutGespreksresultaat", new[] { typeof(Guid), typeof(GespreksresultaatModel), typeof(CancellationToken) }),
                    (typeof(GespreksresultatenController), "PostGespreksresultaat", new[] { typeof(GespreksresultaatModel), typeof(CancellationToken)}),
                    (typeof(GespreksresultatenController), "DeleteGespreksresultaat", new[] { typeof(Guid), typeof(CancellationToken)}),
                    (typeof(LinksController), "PutLink", new[] { typeof(int), typeof(LinkPutModel),typeof(CancellationToken)}),
                    (typeof(LinksController), "PostLink", new[] { typeof(LinkPostModel) }),
                    (typeof(LinksController), "DeleteLink", new[] { typeof(int) }),
                    (typeof(SkillsController), "PutSkill", new[] { typeof(int), typeof(SkillPutModel), typeof(CancellationToken) }),
                    (typeof(SkillsController), "PostSkill", new[] { typeof(SkillPostModel), typeof(CancellationToken) }),
                    (typeof(GetVerwerkingsLogs), "Get", new Type[0]),
                    // Add more controller, method, and parameter combinations as needed
                };

            foreach (var (controllerType, methodName, parameterTypes) in controllersWithMethodsToTest)
            {
                yield return new object[] { controllerType, methodName, parameterTypes };
            }
        }

        [DataTestMethod]
        [DataRow("/api/postcontactmomenten", "post")]
        [DataRow("/api/internetaak/api/version/objects", "post")]
        [DataRow("/api/faq")]
        [DataRow("/api/contactmomentendetails?id=1")]
        [DataRow("/api/zaaksysteem/deeplinkconfig")]
        [DataRow("/api/KanaalToevoegen", "post")]
        public async Task Test(string url, string method = "get")
        {
            using var request = new HttpRequestMessage(new(method), url);
            using var response = await s_client.SendAsync(request);
            Assert.AreEqual(Unauthorized, response.StatusCode);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetControllersWithAuthorizeAttributeAndMethods), DynamicDataSourceType.Method)]
        public async Task TestAuthorizeAttribute(Type controllerType, string methodName, Type[] parameterTypes)
        {
            // Manually create an instance of the controller
            var dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new BeheerDbContext(dbContextOptions);
            var controller = Activator.CreateInstance(controllerType, dbContext) as ControllerBase;

            // Assert that the controller instance is not null
            Assert.IsNotNull(controller);

            // Retrieve the method to test
            var method = controllerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, null, parameterTypes, null);

            // Assert that the method exists
            Assert.IsNotNull(method);

            // Retrieve the Authorize attribute
            var authorizeAttribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .FirstOrDefault() as AuthorizeAttribute;

            // Assert that the Authorize attribute exists and has the expected policy
            Assert.IsNotNull(authorizeAttribute);
            Assert.AreEqual(Policies.RedactiePolicy, authorizeAttribute.Policy);
        }
    }
}

