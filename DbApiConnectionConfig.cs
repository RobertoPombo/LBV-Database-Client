using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

using LBV_Basics;
using LBV_Basics.Models;
using LBV_Basics.Configs;
using LBV_Database_Client.Requests;

namespace LBV_Database_Client
{
    public class DbApiConnectionConfig : ConnectionConfig
    {
        public static readonly List<DbApiConnectionConfig> List = [];
        private static readonly string path = GlobalValues.ConfigDirectory + "config dbapi.json";

        public DbApiConnectionConfig()
        {
            List.Add(this);
            Name = name;
            UpdateDbApiRequests();
        }

        private string name = "Preset #1";
        private bool isActive = false;

        public string Name
        {
            get { return name; }
            set
            {
                if (value.Length > 0)
                {
                    name = value;
                    int nr = 1;
                    string delimiter = " #";
                    string defName = name;
                    string[] defNameList = defName.Split(delimiter);
                    if (defNameList.Length > 1 && Int32.TryParse(defNameList[^1], out _)) { defName = defName[..^(defNameList[^1].Length + delimiter.Length)]; }
                    while (!IsUniqueName())
                    {
                        name = defName + delimiter + nr.ToString();
                        nr++; if (nr == int.MaxValue) { break; }
                    }
                }
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { if (value != isActive) { if (value) { DeactivateAllConnections(); } isActive = value; } }
        }

        [JsonIgnore] public string BaseUrl
        {
            get
            {
                string baseUrl = string.Empty;
                if (ProtocolType != ProtocolType.None) { baseUrl += ProtocolType.ToString() + "://"; }
                if (NetworkType == NetworkType.Localhost) { baseUrl += NetworkType.ToString().ToLower(); }
                else if (IpAdressType == IpAdressType.IPv4) { baseUrl += Ipv4.ToString(); }
                else if (IpAdressType == IpAdressType.IPv6) { baseUrl += "[" + Ipv6.ToString() + "]"; }
                return baseUrl + ":" + Port.ToString();
            }
        }

        public async Task<HttpResponseMessage?> SendRequest(string modelTypename, HttpRequestType requestType, string? path = null, dynamic? objDto = null, string? varName = null)
        {
            path ??= string.Empty;
            string url = string.Join("/", [BaseUrl, modelTypename, requestType.ToString()]);
            using HttpClient httpClient = new();
            {
                try
                {
                    switch (requestType)
                    {
                        case HttpRequestType.Get:
                            if (objDto is null) { return await httpClient.GetAsync(url + path); }
                            else if (varName is null) { return await httpClient.PutAsync(url + path, JsonContent.Create(objDto)); }
                            else { return await httpClient.GetAsync(url + path + "?" + varName + "=" + objDto.ToString()); }
                        case HttpRequestType.Delete:
                            if (objDto is null) { return await httpClient.DeleteAsync(url + path); }
                            else if (varName is null) { return await httpClient.PutAsync(url + path, JsonContent.Create(objDto)); }
                            else { return await httpClient.DeleteAsync(url + path + "?" + varName + "=" + objDto.ToString()); }
                        case HttpRequestType.Add:
                            if (objDto is not null && varName is null) { return await httpClient.PostAsync(url + path, JsonContent.Create(objDto)); }
                            else if (objDto is not null) { return await httpClient.GetAsync(url + path + "?" + varName + "=" + objDto.ToString()); }
                            else { return await httpClient.GetAsync(url + path); }
                        case HttpRequestType.Update:
                            if (objDto is not null && varName is null) { return await httpClient.PutAsync(url + path, JsonContent.Create(objDto)); }
                            else if (objDto is not null) { return await httpClient.GetAsync(url + path + "?" + varName + "=" + objDto.ToString()); }
                            else { return await httpClient.GetAsync(url + path); }
                        default: return null;
                    }
                }
                catch { }
                GlobalValues.CurrentLogText = "Connection to GTRC-Database-API failed!";
                return null;
            }
        }

        public bool IsUniqueName()
        {
            int listIndexThis = List.IndexOf(this);
            for (int conNr = 0; conNr < List.Count; conNr++)
            {
                if (List[conNr].Name == name && conNr != listIndexThis) { return false; }
            }
            return true;
        }

        public async Task<bool> Connectivity()
        {
            foreach (Type modelType in GlobalValues.ModelTypeList)
            {
                HttpResponseMessage? Response = await SendRequest(modelType.Name, HttpRequestType.Get);
                if (Response is null) { return false; }
            }
            return true;
        }

        public static void LoadJson()
        {
            if (!File.Exists(path)) { File.WriteAllText(path, JsonConvert.SerializeObject(List, Formatting.Indented), Encoding.Unicode); }
            try
            {
                List.Clear();
                _ = JsonConvert.DeserializeObject<List<DbApiConnectionConfig>>(File.ReadAllText(path, Encoding.Unicode)) ?? [];
                GlobalValues.CurrentLogText = "GTRC-Database-API connection settings restored.";
            }
            catch { GlobalValues.CurrentLogText = "Restore GTRC-Database-API connection settings failed!"; }
            if (List.Count == 0) { _ = new DbApiConnectionConfig(); }
        }

        public static void SaveJson()
        {
            string text = JsonConvert.SerializeObject(List, Formatting.Indented);
            File.WriteAllText(path, text, Encoding.Unicode);
            GlobalValues.CurrentLogText = "GTRC-Database-API connection settings saved.";
        }

        public static DbApiConnectionConfig? GetActiveConnection()
        {
            foreach (DbApiConnectionConfig con in List) { if (con.IsActive) { con.UpdateDbApiRequests(); return con; } }
            return null;
        }

        public static void DeactivateAllConnections()
        {
            DbApiConnectionConfig? con = GetActiveConnection();
            if (con is not null) { con.IsActive = false; }
        }

        [JsonIgnore] public DbApiRequest<User> User { get; set; } = new();
        [JsonIgnore] public DbApiRequest<Ticket> Ticket { get; set; } = new();

        public void UpdateDbApiRequests()
        {
            User = new(this);
            Ticket = new(this);
        }
    }
}
