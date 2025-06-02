namespace Kiss.Bff.EndToEndTest.Common.Helpers.Api
{

    public static class PageApiResponseExtension
    {
        public static void EnsureSystemIsUp(this IPage page)
        {
           
            page.Response += (_, response) =>
            {
                var request = response.Request;
                var requestHeaders = request.Headers;

                if (response.Status >= 500 && response.Status < 600)
                {
                    var errorMessage = $"Server error:\n" +
                        $"  Method: {request.Method}\n" +
                        $"  URL: {request.Url}\n" +
                        $"  Response status: {response.Status}";
                    throw new Exception(errorMessage); 
                } 
            };

              }
    }
}
