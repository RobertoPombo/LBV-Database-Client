using System.Net;
using System.Net.Http.Json;

namespace LBV_Database_Client.Responses
{
    public class DbApiValueResponse<T> where T : struct
    {
        private HttpStatusCode status = HttpStatusCode.InternalServerError;
        private T? _value = null;

        public DbApiValueResponse(HttpResponseMessage? response)
        {
            if (response is not null)
            {
                status = response.StatusCode;
                _value = response.Content.ReadFromJsonAsync<T>().Result;
            }
        }

        public HttpStatusCode Status { get { return status; } }
        public T? Value { get { return _value; } }
    }
}
