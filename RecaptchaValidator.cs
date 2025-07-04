using System.Text.Json;
using System.Text.Json.Serialization;
using Roller.Models;

namespace Roller;

public interface IRecaptchaValidator
{
    Task<bool> IsValidAsync(string token);
}

public class RecaptchaValidator(IConfiguration configuration, HttpClient httpClient) : IRecaptchaValidator
{
    public async Task<bool> IsValidAsync(string token)
    {
        var parameters = new Dictionary<string, string?>()
        {
            {"secret", configuration.GetValue<string>("RecaptchaSecret")},
            {"response", token},
        };
        
        var recaptchaReq = new FormUrlEncodedContent(parameters);
        
        var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", recaptchaReq);
       
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(responseContent, JsonSerializerOptions.Web);
        // recaptcha success false olsa da bana status olarak 200 dönüyor
        return recaptchaResponse.Success;
    }
}

public class RecaptchaResponse
{
    public bool Success { get; set; }
    public string Hostname { get; set; }
    [JsonPropertyName("error-codes")]
    public List<string> ErrorCodes { get; set; }
}