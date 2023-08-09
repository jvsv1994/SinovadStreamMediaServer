using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Transversal.Common;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SinovadMediaServer.Proxy
{
    public class RestService
    {

        private string _restApiUrl = "https://streamapi.sinovad.com/api/v1";

        //private string _restApiUrl = "http://localhost:53363/api/v1";  

        private SharedData _sharedData;

        public RestService(SharedData sharedData) {
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
                        responseMessage = await httpClient.GetAsync(_restApiUrl + path);
                    }
                    if (type == HttpMethodType.POST)
                    {
                        responseMessage = await httpClient.PostAsync(_restApiUrl + path, content);
                    }
                    if (type == HttpMethodType.PUT)
                    {
                        responseMessage = await httpClient.PutAsync(_restApiUrl + path, content);
                    }
                    if (type == HttpMethodType.DELETE)
                    {
                        responseMessage = await httpClient.DeleteAsync(_restApiUrl + path);
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
