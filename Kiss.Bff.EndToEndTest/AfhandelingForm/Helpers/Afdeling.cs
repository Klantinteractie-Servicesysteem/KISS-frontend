using System.Text.Json;

namespace Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers
{
    public class AfdelingValidationHelper
    {
        public class ValidationConfig
        {
            public string ExpectedTitle { get; set; }
            public string DataSource { get; set; }
            public bool ExpectsUppercaseViolation { get; set; }
        }

        public class ValidationResult
        {
            public bool FoundCorrectProperty { get; set; }
            public bool FoundViolatingProperty { get; set; }
            public bool HasExpectedData { get; set; }
            public bool TargetItemFound { get; set; }
            public List<string> FoundProperties { get; set; } = new List<string>();
        }

        // Configuration mapping - easily maintainable
        private static readonly Dictionary<string, ValidationConfig> ConfigMappings = new()
        {
            // Kennisbank configurations
            ["Kennisbank Wegenverkeerswet, ontheffing"] = new()
            {
                DataSource = "Kennisbank",
                ExpectedTitle = "Wegenverkeerswet, ontheffing",
                ExpectsUppercaseViolation = false
            },
            ["Kennisbank Wegenverkeerswet, ontheffing - afdelingNaam"] = new()
            {
                DataSource = "Kennisbank",
                ExpectedTitle = "Wegenverkeerswet, ontheffing - afdelingNaam",
                ExpectsUppercaseViolation = true
            },

            // VAC configurations
            ["VAC This VAC has a property afdelingnaam with lower case n"] = new()
            {
                DataSource = "VAC",
                ExpectedTitle = "This VAC has a property afdelingnaam with lower case n",
                ExpectsUppercaseViolation = false
            },
            ["VAC This VAC has a property afdelingNaam with Upper Case N"] = new()
            {
                DataSource = "VAC",
                ExpectedTitle = "This VAC has a property afdelingNaam with Upper Case N",
                ExpectsUppercaseViolation = true
            }
        };

        public static ValidationConfig DetermineValidationConfig(string resultName)
        {
            if (ConfigMappings.TryGetValue(resultName, out var config))
            {
                return config;
            }

            throw new ArgumentException($"No configuration found for resultName: '{resultName}'. " +
                                      $"Available configurations: {string.Join(", ", ConfigMappings.Keys)}");
        }

        // Rest of the helper methods remain the same...
        public static async Task<List<string>> CaptureSearchResponses(IPage page, string dataSource)
        {
            var capturedResponses = new List<string>();

            await page.RouteAsync("**", async route =>
            {
                var url = route.Request.Url;
                var isSearchRequest = url.Contains("elasticsearch") ||
                                    url.Contains("search") ||
                                    url.Contains("kennisbank") ||
                                    url.Contains("overige") ||
                                    url.Contains("suggest") ||
                                    url.Contains("vac");

                if (isSearchRequest)
                {
                    var response = await route.FetchAsync();
                    var responseText = await response.TextAsync();

                    if (responseText.Contains(dataSource))
                    {
                        capturedResponses.Add(responseText);
                    }

                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = response.Status,
                        Headers = response.Headers,
                        Body = responseText
                    });
                }
                else
                {
                    await route.ContinueAsync();
                }
            });

            return capturedResponses;
        }

        public static string FindTargetResponse(List<string> capturedResponses, string expectedTitle, string dataSource)
        {
            foreach (var response in capturedResponses)
            {
                if (response.Contains(expectedTitle))
                {
                    return response;
                }
            }

            return capturedResponses.FirstOrDefault(r => r.Contains(dataSource)) ?? string.Empty;
        }

        public static ValidationResult ValidateAfdelingProperties(string targetResponse, ValidationConfig config, string expectedAfdeling)
        {
            var result = new ValidationResult();
            var json = JsonDocument.Parse(targetResponse);

            if (json.RootElement.TryGetProperty("hits", out var hitsProperty) &&
                hitsProperty.TryGetProperty("hits", out var hits))
            {
                foreach (var hit in hits.EnumerateArray())
                {
                    var source = hit.GetProperty("_source");
                    var title = source.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : "";

                    if (title != config.ExpectedTitle) continue;

                    result.TargetItemFound = true;


                    if (source.TryGetProperty(config.DataSource, out var dataSourceObject))
                    {
                        result.HasExpectedData = true;

                        if (dataSourceObject.TryGetProperty("afdelingen", out var afdelingen))
                        {
                            foreach (var afdeling in afdelingen.EnumerateArray())
                            {
                                foreach (var property in afdeling.EnumerateObject())
                                {
                                    result.FoundProperties.Add($"{property.Name}={property.Value.GetString()}");

                                    if (property.Name == "afdelingnaam" && property.Value.GetString() == expectedAfdeling)
                                    {
                                        result.FoundCorrectProperty = true;
                                    }
                                    else if (property.Name == "afdelingNaam" && property.Value.GetString() == expectedAfdeling)
                                    {
                                        result.FoundViolatingProperty = true;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }

            return result;
        }

        public static void AssertValidationResults(ValidationResult result, ValidationConfig config, string expectedAfdeling)
        {
            if (!result.TargetItemFound)
            {
                Assert.Fail($"Target item with title '{config.ExpectedTitle}' was not found in the search response.");
            }

            if (!result.HasExpectedData)
            {
                Assert.Fail($"No {config.DataSource} data found in the target item '{config.ExpectedTitle}'.");
            }

            if (config.ExpectsUppercaseViolation)
            {
                if (!result.FoundViolatingProperty)
                {
                    Assert.Fail($"Expected to find 'afdelingNaam' (uppercase N) property with value '{expectedAfdeling}' in item '{config.ExpectedTitle}' but it was not found. Found properties: {string.Join(", ", result.FoundProperties)}");
                }
            }
            else
            {
                if (result.FoundViolatingProperty)
                {
                    Assert.Fail($"Found 'afdelingNaam' (uppercase N) property in item '{config.ExpectedTitle}' which violates the naming convention. Expected only 'afdelingnaam' (lowercase).");
                }

                if (!result.FoundCorrectProperty)
                {
                    Assert.Fail($"Expected 'afdelingnaam' (lowercase) property with value '{expectedAfdeling}' not found in item '{config.ExpectedTitle}'. Found properties: {string.Join(", ", result.FoundProperties)}");
                }
            }
        }
    }
}