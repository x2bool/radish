using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rbmk.Utils.Apis.Objects;

namespace Rbmk.Utils.Apis
{
    public class ApiClient : IApiClient
    {   
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<ApiResult<Api.ReleaseList>> GetReleases(string product)
        {
            var response = await _httpClient.GetAsync($"/releases?product={product}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResult<Api.ReleaseList>>(content);
        }
    }
}