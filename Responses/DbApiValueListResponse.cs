using System.Net;
using System.Net.Http.Json;

namespace LBV_Database_Client.Responses
{
    public class DbApiValueListResponse<T>
    {
        private HttpStatusCode status = HttpStatusCode.InternalServerError;
        private List<T> list = [];

        public DbApiValueListResponse(HttpResponseMessage? response)
        {
            if (response is not null)
            {
                status = response.StatusCode;
                list = response.Content.ReadFromJsonAsync<List<T>>().Result ?? [];
            }
        }

        public HttpStatusCode Status { get { return status; } }
        public List<T> List { get { return list; } }
    }
}
