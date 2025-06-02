namespace Kiss.Bff.EndToEndTest.Common.Helpers.Api
{

    public static class PageApiResponseExtension
    {

        public static void HandleResponseStatus(this IPage page, int expectedStatus = 200)
        {
            page.Response += (_, response) =>
            {
                var request = response.Request;
                var requestHeaders = request.Headers;

                if (response.Status != expectedStatus)
                {
                    var errorMessage = $"Request failed:\n" +
                        $"  Method: {request.Method}\n" +
                        $"  URL: {request.Url}\n" + 
                        $"  Expected status: {expectedStatus}, but got {response.Status}";
                    throw new  Exception(errorMessage);
                }
            };
        }
    }
}
