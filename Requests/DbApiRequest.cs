using Newtonsoft.Json;
using System.Net;

using LBV_Basics;
using LBV_Basics.Models.Common;
using LBV_Basics.Models.DTOs;
using LBV_Database_Client.Responses;

namespace LBV_Database_Client.Requests
{
    public class DbApiRequest<ModelType>(DbApiConnectionConfig? connection = null) where ModelType : class, IBaseModel, new()
    {
        public static readonly string Model = typeof(ModelType).Name;

        public HttpResponseMessage? Response;

        public static async Task<DbApiObjectResponse<ModelType>> ReturnAsObject(HttpResponseMessage? response)
        {
            DbApiObjectResponse<ModelType> obj = new();
            if (response is not null)
            {
                obj.Status = response.StatusCode;
                obj.Object = JsonConvert.DeserializeObject<ModelType>(await response.Content.ReadAsStringAsync()) ?? new();
            }
            return obj;
        }

        public static async Task<DbApiListResponse<ModelType>> ReturnAsList(HttpResponseMessage? response)
        {
            DbApiListResponse<ModelType> obj = new();
            if (response is not null)
            {
                obj.Status = response.StatusCode;
                obj.List = JsonConvert.DeserializeObject<List<ModelType>>(await response.Content.ReadAsStringAsync()) ?? [];
            }
            return obj;
        }

        public async Task<DbApiListResponse<ModelType>> GetAll()
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get); }
            return await ReturnAsList(Response);
        }

        public async Task<DbApiObjectResponse<ModelType>> GetById(int id)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get, "/" + id.ToString()); }
            return await ReturnAsObject(Response);
        }

        public async Task<DbApiObjectResponse<ModelType>> GetByUniqProps(UniqPropsDto<ModelType> objDto)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get, "/ByUniqProps/" + objDto.Index.ToString(), objDto.Dto); }
            return await ReturnAsObject(Response);
        }

        public async Task<DbApiListResponse<ModelType>> GetByProps(AddDto<ModelType> objDto)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get, "/ByProps", objDto.Dto); }
            return await ReturnAsList(Response);
        }

        public async Task<DbApiListResponse<ModelType>> GetByFilter(FilterDtos<ModelType> objDto)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get, "/ByFilter", objDto.Dto); }
            return await ReturnAsList(Response);
        }

        public async Task<DbApiObjectResponse<ModelType>> GetTemp()
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get, "/Temp"); }
            return await ReturnAsObject(Response);
        }

        public async Task<DbApiObjectResponse<ModelType>> Add(AddDto<ModelType> objDto)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Add, objDto: objDto.Dto); }
            return await ReturnAsObject(Response);
        }

        public async Task<HttpStatusCode> Delete(int id, bool force = false)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Delete, "/" + id.ToString() + "/" + force.ToString()); }
            return Response?.StatusCode ?? HttpStatusCode.InternalServerError;
        }

        public async Task<DbApiObjectResponse<ModelType>> Update(UpdateDto<ModelType> objDto)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Update, objDto: objDto.Dto); }
            return await ReturnAsObject(Response);
        }

        public async Task<bool> HasChildObjects(int id)
        {
            if (connection is not null) { Response = await connection.SendRequest(Model, HttpRequestType.Get, "/HasChildObjects/" + id.ToString()); }
            bool content = false;
            if (Response is not null) { _ = bool.TryParse(await Response.Content.ReadAsStringAsync() ?? false.ToString(), out content); }
            return content;
        }

        public async Task<DbApiListResponse<ModelType>> GetChildObjects(Type parentModelType, int parentId)
        {
            if (connection is not null) { Response = await connection.SendRequest(parentModelType.Name, HttpRequestType.Get, "/" + Model + "/" + parentId.ToString()); }
            return await ReturnAsList(Response);
        }
    }
}
