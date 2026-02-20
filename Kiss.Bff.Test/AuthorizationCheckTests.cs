using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;
using static System.Net.HttpStatusCode;
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
using Kiss.Bff.Intern.Environment;
using Microsoft.Extensions.Configuration;
using Kiss.Bff.Extern;

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

        /// <summary>
        /// Data source for controllers whose individual methods are expected to carry the RedactiePolicy <see cref="AuthorizeAttribute"/>.
        /// </summary>
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

        /// <summary>
        /// Data source for controllers whose class-level <see cref="AuthorizeAttribute"/> is expected to carry the specified policy.
        /// Provides the constructor arguments required to instantiate each controller.
        /// Provides the specific policy that needs to be verified.
        /// </summary>
        public static IEnumerable<object[]> GetControllersWithSpecificPolicyAuthorizeAttribute()
        {
            var dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new BeheerDbContext(dbContextOptions);
            var configuration = new ConfigurationBuilder().Build();
            var registryConfig = new RegistryConfig { Systemen = [] };

            // Define the controllers and methods to test here
            var controllersWithMethodsToTest = new List<(Type controllerType, object[] controllerParameters, string policyToVerify)>
                {
                    (typeof(SeedController), new object[] { dbContext, new BerichtenService(), new SkillsService(), new LinksService(), new GespreksresultatenService() }, Policies.RedactiePolicy),
                    (typeof(KissConnectionsController), new object[] { configuration, registryConfig }, Policies.KcmOrRedactiePolicy),
                };

            foreach (var (controllerType, controllerParameters, policyToVerify) in controllersWithMethodsToTest)
            {
                yield return new object[] { controllerType, controllerParameters, policyToVerify };
            }
        }

        /// <summary>
        /// Data source for controller methods expected to carry a <see cref="RequirePermissionAttribute"/> with specific permissions.
        /// Provides the specific permissions that need to be verified.
        /// </summary>
        public static IEnumerable<object[]> GetControllersMethodsWithRequirePermissionAttribute()
        {
            // Define the controllers and methods to test here
            var controllersWithMethodsToTest = new List<(Type controllerType, string methodName, Type[] parameterTypes, RequirePermissionTo[] requiredPermissions)>
                {
                    (typeof(GespreksresultatenController), "PutGespreksresultaat", new[] { typeof(Guid), typeof(GespreksresultaatModel), typeof(CancellationToken) }, [RequirePermissionTo.gespreksresultatenbeheer]),
                    (typeof(GespreksresultatenController), "PostGespreksresultaat", new[] { typeof(GespreksresultaatModel), typeof(CancellationToken)}, [RequirePermissionTo.gespreksresultatenbeheer]),
                    (typeof(GespreksresultatenController), "DeleteGespreksresultaat", new[] { typeof(Guid), typeof(CancellationToken)}, [RequirePermissionTo.gespreksresultatenbeheer]),
                    (typeof(LinksController), "GetLinks", new Type[0], [RequirePermissionTo.linksread]),
                    (typeof(LinksController), "PutLink", new[] { typeof(int), typeof(LinkPutModel),typeof(CancellationToken)}, [RequirePermissionTo.linksbeheer]),
                    (typeof(LinksController), "PostLink", new[] { typeof(LinkPostModel) }, [RequirePermissionTo.linksbeheer]),
                    (typeof(LinksController), "DeleteLink", new[] { typeof(int) }, [RequirePermissionTo.linksbeheer]),
                    (typeof(SkillsController), "GetSkills", new Type[0], [RequirePermissionTo.skillsread]),
                    (typeof(SkillsController), "PutSkill", new[] { typeof(int), typeof(SkillPutModel), typeof(CancellationToken) }, [RequirePermissionTo.skillsbeheer]),
                    (typeof(SkillsController), "PostSkill", new[] { typeof(SkillPostModel), typeof(CancellationToken) }, [RequirePermissionTo.skillsbeheer]),
                    (typeof(SkillsController), "DeleteSkill", new[] { typeof(int), typeof(CancellationToken) }, [RequirePermissionTo.skillsbeheer]),
                    (typeof(BerichtenController), "GetBerichten", new Type[0], [RequirePermissionTo.berichtenread]),
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
        /// <summary>
        /// Verifies that the listed endpoints return 401 Unauthorized when called without credentials.
        /// </summary>
        public async Task CallingEnpointsWithoutCredetialsShouldResultInAUnauthorizedResponse(string url, string method = "get")
        {
            using var request = new HttpRequestMessage(new(method), url);
            using var response = await s_client.SendAsync(request);
            Assert.AreEqual(Unauthorized, response.StatusCode);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetControllersWithSpecificPolicyAuthorizeAttribute), DynamicDataSourceType.Method)]
        /// <summary>
        /// Verifies that the controller class itself carries an <see cref="AuthorizeAttribute"/>.
        /// </summary>
        public void TestControllerAuthorizeAttribute(Type controllerType, object[] controllerParameters, string policyToVerify)
        {
            object? controller = null;
            try
            {
                controller = Activator.CreateInstance(controllerType, controllerParameters) as ControllerBase;
            }
            catch (Exception)
            {
                // Running the test in Visual Studio results in marking the test as 'not run' instead of failed when controller creation throws an error. This forces the test to fail if there is something wrong.
                Assert.Fail($"Controller {controllerType.Name} not found or failed to initialize.");
            }

            Assert.IsNotNull(controller);

            var authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .FirstOrDefault() as AuthorizeAttribute;

            Assert.IsNotNull(authorizeAttribute, $"Controller {controllerType.Name} is missing a class-level Authorize attribute.");

            // Assert that the method has the right auth attribute
            Assert.AreEqual(policyToVerify, authorizeAttribute?.Policy);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetControllersMethodsWithRedactiePolicyAuthorizeAttribute), DynamicDataSourceType.Method)]
        /// <summary>
        /// Verifies that the specified controller method (or its controller class) carries an <see cref="AuthorizeAttribute"/> with the RedactiePolicy.
        /// </summary>
        public void TestAuthorizeAttribute(Type controllerType, string methodName, Type[] parameterTypes)
        {
            // Manually create an instance of the controller
            var dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new BeheerDbContext(dbContextOptions);
            object? controller = null;
            try
            {
                controller = Activator.CreateInstance(controllerType, dbContext) as ControllerBase;
            }
            catch (Exception)
            {
                // Running the test in Visual Studio results in marking the test as 'not run' instead of failed when controller creation throws an error. This forces the test to fail if there is something wrong.
                Assert.Fail($"Controller {controllerType.Name} not found or failed to initialize.");
            }

            // Assert that the controller instance is not null
            Assert.IsNotNull(controller);

            // Retrieve the method to test
            var method = controllerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, null, parameterTypes, null);

            // Assert that the method exists
            Assert.IsNotNull(method);

            // Retrieve the Authorize attribute
            var authorizeAttribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .FirstOrDefault() as AuthorizeAttribute;

            // If the method does not have a role specified, check if the controller has a general Authorize attribute.
            if (authorizeAttribute == null)
            {
                authorizeAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true)
                .FirstOrDefault() as AuthorizeAttribute;
            }

            // Assert that the method has the right auth attribute
            Assert.AreEqual(Policies.RedactiePolicy, authorizeAttribute?.Policy);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetControllersMethodsWithRequirePermissionAttribute), DynamicDataSourceType.Method)]
        /// <summary>
        /// Verifies that the specified controller method carries a <see cref="RequirePermissionAttribute"/> with the expected permissions.
        /// </summary>
        public void TestPermissionAttribute(Type controllerType, string methodName, Type[] parameterTypes, RequirePermissionTo[] requiredPermissions)
        {
            var dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var dbContext = new BeheerDbContext(dbContextOptions);
            object? controller = null;
            try
            {
                controller = Activator.CreateInstance(controllerType, dbContext) as ControllerBase;
            }
            catch (Exception)
            {
                // Running the test in Visual Studio results in marking the test as 'not run' instead of failed when controller creation throws an error. This forces the test to fail if there is something wrong.
                Assert.Fail($"Controller {controllerType.Name} not found or failed to initialize.");
            }

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
            object? controller = null;
            try
            {
                controller = Activator.CreateInstance(controllerType, dbContext) as ControllerBase;
            }
            catch (Exception)
            {
                // Running the test in Visual Studio results in marking the test as 'not run' instead of failed when controller creation throws an error. This forces the test to fail if there is something wrong.
                Assert.Fail($"Controller {controllerType.Name} not found or failed to initialize.");
            }

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


