using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using PolestarSharp.Models;

namespace PolestarSharp;

public class PolestarApi
{
    private readonly string _username;
    private readonly string _password;
    private readonly string _vin;
    
    private const string POLESTAR_BASE_URL = "https://pc-api.polestar.com/eu-north-1";
    private const string POLESTAR_API_URL_V2 = $"{POLESTAR_BASE_URL}/mystar-v2";
    private const string POLESTAR_API_URL = $"{POLESTAR_BASE_URL}/my-star";

    private readonly HttpClient _httpClient;
    public Token Token;

    public PolestarApi(string username, string password, string vin)
    {
        _username = username;
        _password = password;
        _vin = vin;
        
        HttpClientHandler httpHandler = new HttpClientHandler()
        {
            UseCookies = true
        };
        
        _httpClient = new HttpClient(httpHandler);
    }

    public async Task<bool> Login()
    {
        try
        {
            var apitoken = await GetAccessToken();
            Token = apitoken;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<BatteryData> GetBatteryInfo()
    {
        var battery = await SendQuery<BatteryData>(
            POLESTAR_API_URL_V2,
            "query GetBatteryData($vin: String!) { getBatteryData(vin: $vin) { averageEnergyConsumptionKwhPer100Km batteryChargeLevelPercentage chargerConnectionStatus chargingCurrentAmps chargingPowerWatts chargingStatus estimatedChargingTimeMinutesToTargetDistance estimatedChargingTimeToFullMinutes estimatedDistanceToEmptyKm estimatedDistanceToEmptyMiles eventUpdatedTimestamp { iso unix __typename } __typename } }",
            "GetBatteryData",
            new { vin = _vin });

        return battery;
    }

    public async Task<OdometerData> GetOdometerData()
    {
        var odometer = await SendQuery<OdometerData>(
            POLESTAR_API_URL_V2,
            "query GetOdometerData($vin: String!) { getOdometerData(vin: $vin) { averageSpeedKmPerHour eventUpdatedTimestamp { iso unix __typename } odometerMeters tripMeterAutomaticKm tripMeterManualKm __typename } }",
            "GetOdometerData",
            new { vin = _vin });

        return odometer;
    }

    private async Task<string> GetLoginFlowTokens()
    {
        string url = "https://polestarid.eu.polestar.com/as/authorization.oauth2?response_type=code&client_id=polmystar&redirect_uri=https://www.polestar.com%2Fsign-in-callback&scope=openid+profile+email+customer%3Aattributes";
        
        var data = await _httpClient.GetAsync(url);
        var resp = data.RequestMessage.RequestUri.AbsoluteUri;
        var pathToken = Regex.Match(resp, "resumePath=(\\w+)").Groups.Values.ElementAt(1);

        return pathToken.Value;
    }

    private async Task<Token> GetAccessToken()
    {
        var pathToken = await GetLoginFlowTokens();
        var tokenRequestCode = await PerformLogin(pathToken);
        return await GetApiToken(tokenRequestCode);
    }

    private async Task<string> PerformLogin(string pathToken)
    {
        string url = $"https://polestarid.eu.polestar.com/as/{pathToken}/resume/as/authorization.ping";
        
        var formData = new List<KeyValuePair<string, string>>
        {
            new ("pf.username", _username),
            new ("pf.pass", _password)
        };

        // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
        HttpContent content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.PostAsync(url, content);
        var urlResp = response.RequestMessage.RequestUri.AbsoluteUri;
        var data = Regex.Match(urlResp, "code=([^&]+)").Groups.Values.ElementAt(1);

        return data.Value;
    }
    
    private async Task<Token> GetApiToken(string tokenRequestCode)
    {
        var query = "query getAuthToken($code: String!) { getAuthToken(code: $code) { id_token access_token refresh_token expires_in } }";
        var response = await SendQuery<Auth>($"{POLESTAR_BASE_URL}/auth", query, "getAuthToken", new { code = tokenRequestCode }, false);
        
        return new Token()
        {
            access_token = response.data.getAuthToken.access_token,
            refresh_token = response.data.getAuthToken.refresh_token,
            expires_in = response.data.getAuthToken.expires_in
        };
    }

    private async Task<T> SendQuery<T>(string url, string query, string operationName, object variables,
        bool useAuth = true)
    {
        var req = new
        {
            query,
            operationName,
            variables
        };

        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
        {
            string json = System.Text.Json.JsonSerializer.Serialize(req);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            if (useAuth)
            {
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {Token.access_token}");
            }

            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadFromJsonAsync<T>();
            return data;
        }
    }
}