using System.Net;

using LBV_Basics.Models.Common;

namespace LBV_Database_Client.Responses
{
    public class DbApiObjectResponse<ModelType> where ModelType : class, IBaseModel, new()
    {
        public HttpStatusCode Status { get; set; } = HttpStatusCode.InternalServerError;
        public ModelType Object { get; set; } = new();
    }
}
