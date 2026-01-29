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
using Kiss.Bff.Intern.Seed.Features;
using Kiss.Bff.Intern.ContactmomentDetails.Features;
using Kiss.Bff.Config.Permissions;

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

        public static IEnumerable<object[]> GetControllersMethodsWithRedactiePolicyAuthorizeAttribute()
        {
            // Define the controllers and methods to test here
            var controllersWithMethodsToTest = new List<(Type controllerType, string methodName, Type[] parameterTypes)>
                {
                    (typeof(GetVerwerkingsLogs), "Get", new Type[0]),

                    // Add more controller, method, and parameter combinations as needed
                };

            foreach (var (controllerType, methodName, parameterTypes) in controllersWithMethodsToTest)
            {
                yield return new object[] { controllerType, methodName, parameterTypes };
            }
        }

        public static IEnumerable<object[]> GetControllersMethodsWithRequirePermissionAttribute()
        {
            // Define the controllers and methods to test here
            var controllersWithMethodsToTest = new List<(Type controllerType, string methodName, Type[] parameterTypes, RequirePermissionTo[] requiredPermissions)>
                {
                    (typeof(GespreksresultatenController), "PutGespreksresultaat", new[] { typeof(Guid), typeof(GespreksresultaatModel), typeof(CancellationToken) }, [RequirePermissionTo.gespreksresultatenbeheer]),
                    (typeof(GespreksresultatenController), "PostGespreksresultaat", new[] { typeof(GespreksresultaatModel), typeof(CancellationToken)}, [RequirePermissionTo.gespreksresultatenbeheer]),
                    (typeof(GespreksresultatenController), "DeleteGespreksresultaat", new[] { typeof(Guid), typeof(CancellationToken)}, [RequirePermissionTo.gespreksresultatenbeheer]),
                    (typeof(LinksController), "GetLinks", new Type[0], [RequirePermissionTo.linksread, RequirePermissionTo.linksbeheer]),
                    (typeof(LinksController), "PutLink", new[] { typeof(int), typeof(LinkPutModel),typeof(CancellationToken)}, [RequirePermissionTo.linksbeheer]),
                    (typeof(LinksController), "PostLink", new[] { typeof(LinkPostModel) }, [RequirePermissionTo.linksbeheer]),
                    (typeof(LinksController), "DeleteLink", new[] { typeof(int) }, [RequirePermissionTo.linksbeheer]),
                    (typeof(SkillsController), "PutSkill", new[] { typeof(int), typeof(SkillPutModel), typeof(CancellationToken) }, [RequirePermissionTo.skillsbeheer]),
                    (typeof(SkillsController), "PostSkill", new[] { typeof(SkillPostModel), typeof(CancellationToken) }, [RequirePermissionTo.skillsbeheer]),
                    (typeof(BerichtenController), "GetBerichten", new Type[0], [RequirePermissionTo.berichtenread, RequirePermissionTo.berichtenbeheer]),
                    (typeof(BerichtenController), "PostBericht", new[] { typeof(BerichtPostModel), typeof(CancellationToken) }, [RequirePermissionTo.berichtenbeheer]),
                    (typeof(BerichtenController), "PutBericht", new[] { typeof(int),typeof(BerichtPutModel), typeof(CancellationToken) }, [RequirePermissionTo.berichtenbeheer]),
                    (typeof(BerichtenController), "DeleteBericht", new[] { typeof(int), typeof(CancellationToken) }, [RequirePermissionTo.berichtenbeheer]),

                    // Add more controller, method, and parameter combinations as needed
                };

            foreach (var (controllerType, methodName, parameterTypes, requiredPermissions) in controllersWithMethodsToTest)
            {
                yield return new object[] { controllerType, methodName, parameterTypes, requiredPermissions };
            }
        }

        [DataTestMethod]
        [DataRow("/api/postcontactmomenten", "post")]
        [DataRow("/api/internetaak/api/version/objects", "post")]
        [DataRow("/api/faq")]
        [DataRow("/api/contactmomentendetails?id=1")]
        [DataRow("/api/environment/registers")]
        [DataRow("/api/KanaalToevoegen", "post")]
        public async Task CallingEnpointsWithoutCredetialsShouldResultInAUnauthorizedResponse(string url, string method = "get")
        {
            using var request = new HttpRequestMessage(new(method), url);
            using var response = await s_client.SendAsync(request);
            Assert.AreEqual(Unauthorized, response.StatusCode);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetControllersMethodsWithRedactiePolicyAuthorizeAttribute), DynamicDataSourceType.Method)]
        public void TestAuthorizeAttribute(Type controllerType, string methodName, Type[] parameterTypes)
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

            // Assert that the method has the right auth attribute
            Assert.AreEqual(Policies.RedactiePolicy, authorizeAttribute?.Policy);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetControllersMethodsWithRequirePermissionAttribute), DynamicDataSourceType.Method)]
        public void TestPermissionAttribute(Type controllerType, string methodName, Type[] parameterTypes, RequirePermissionTo[] requiredPermissions)
        {
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

            // Retrieve the permission attribute
            var permissionAttribute = method.GetCustomAttributes(typeof(RequirePermissionAttribute), true)
                .FirstOrDefault() as RequirePermissionAttribute;

            // Assert that the method has the right permission
            CollectionAssert.AreEquivalent(new List<RequirePermissionTo>(requiredPermissions), permissionAttribute?.Permissions);
        }


        [TestMethod]
        public void TestAuthorizationOfManagementInformatieEndpoint()
        {
            var controllerType = typeof(ContactmomentDetailsRapportageOverzicht);

            var dbContext = new BeheerDbContext(new DbContextOptions<BeheerDbContext>());
            var controller = Activator.CreateInstance(controllerType, dbContext) as ControllerBase;

            Assert.IsNotNull(controller);

            var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.AreEqual(1, methods.Length);

            for (var i = 0; i < methods.Length; i += 1)
            {
                var authorizeAttribute = methods[i].GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault() as AuthorizeAttribute;

                Assert.IsNotNull(authorizeAttribute);
                Assert.AreEqual(Policies.ExternSysteemPolicy, authorizeAttribute.Policy);
            }
        }

    }
}


