using Microsoft.Extensions.Options;
using SinovadMediaServer.Common;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Enums;
using SinovadMediaServer.Shared;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SinovadMediaServer.Proxy
{
    public class RestService
    {

        private IOptions<MyConfig> _config;

        private SharedData _sharedData;

        public RestService(IOptions<MyConfig> config, SharedData sharedData) {
            _config = config;
            _sharedData = sharedData;
        }

        public async Task<Response<Data>> ExecuteHttpMethodAsync<Data>(HttpMethodType type, string path, Object content)
        {
            var requestContent = new StringContent("");
            if (content != null)
            {
                requestContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
            }
            return await PerformExecuteHttpMethodAsync<Data>(type, path, requestContent);
        }

        public async Task<Response<Data>> ExecuteHttpMethodAsync<Data>(HttpMethodType type, string path)
        {
            return await PerformExecuteHttpMethodAsync<Data>(type, path, new StringContent(""));
        }

        private async Task<Response<Data>> PerformExecuteHttpMethodAsync<Data>(HttpMethodType type,string path,StringContent content)
        {  
            var response = new Response<Data>();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    if (_sharedData.ApiToken != null)
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sharedData.ApiToken);
                    }
                    var responseMessage = new HttpResponseMessage();
                    if (type == HttpMethodType.GET)
                    {
                        responseMessage = await httpClient.GetAsync(_config.Value.RestApiUrl + path);
                    }
                    if (type == HttpMethodType.POST)
                    {
                        responseMessage = await httpClient.PostAsync(_config.Value.RestApiUrl + path, content);
                    }
                    if (type == HttpMethodType.PUT)
                    {
                        responseMessage = await httpClient.PutAsync(_config.Value.RestApiUrl + path, content);
                    }
                    if (type == HttpMethodType.DELETE)
                    {
                        responseMessage = await httpClient.DeleteAsync(_config.Value.RestApiUrl + path);
                    }
                    var taskResponse = await responseMessage.Content.ReadAsStringAsync();
                    response = JsonSerializer.Deserialize<Response<Data>>(taskResponse);
                    if (responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(response.Message);
                    }
                }
            }catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

    }
}
