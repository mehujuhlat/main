using client.App_Code;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

public class RecaptchaService
{
    private readonly HttpClient _httpClient;
   
    public RecaptchaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyCaptcha(string token)
    {
        var response = await _httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={AppSecrets.SecretKey}&response={token}");
        var result = JsonConvert.DeserializeObject<RecaptchaResponse>(response);
        return result.Success;
    }

    private class RecaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}