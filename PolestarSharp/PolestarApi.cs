using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using PolestarSharp.Models;

namespace PolestarSharp;

public class PolestarApi
{
    public DateTime TokenExpiryDateTimeUtc = DateTime.MinValue;
    
    private readonly string _username;
    private readonly string _password;
    private readonly string _vin;
    
    private const string POLESTAR_BASE_URL = "https://pc-api.polestar.com/eu-north-1";
    private const string POLESTAR_REDIRECT_URI = "https://www.polestar.com/sign-in-callback";
    private const string POLESTAR_API_URL_V2 = $"{POLESTAR_BASE_URL}/mystar-v2";
    private const string CLIENT_ID = "l3oopkc_10";
    private const string CODE_VERIFIER = "2024-polestarsharp-dotnet-client-polestar-2024";
    private const string CODE_CHALLENGE = "GmU_FRSNd3V-b6HiKuEQ3sEJCkt8IQjNz0CLb_sDsZs";

    private readonly HttpClient _httpClient;
    private Token _token;

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
            _token = apitoken;
            TokenExpiryDateTimeUtc = DateTime.UtcNow.AddSeconds(_token.expires_in);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CarTelematicsData> GetCarTelematics()
    { 
        var carTelematics = await SendQuery<CarTelematicsData>(
            POLESTAR_API_URL_V2,
            """
            query CarTelematics($vin:String!) {
                carTelematics(vin: $vin) {
                    health {
                        brakeFluidLevelWarning
                        daysToService
                        distanceToServiceKm
                        engineCoolantLevelWarning
                        eventUpdatedTimestamp { iso unix }
                        oilLevelWarning
                        serviceWarning
                    }
                    battery {
                        averageEnergyConsumptionKwhPer100Km
                        batteryChargeLevelPercentage
                        chargerConnectionStatus
                        chargingCurrentAmps
                        chargingPowerWatts
                        chargingStatus
                        estimatedChargingTimeMinutesToTargetDistance
                        estimatedChargingTimeToFullMinutes
                        estimatedDistanceToEmptyKm
                        estimatedDistanceToEmptyMiles
                        eventUpdatedTimestamp { iso unix }
                    }
                    odometer {
                        averageSpeedKmPerHour
                        eventUpdatedTimestamp { iso unix }
                        odometerMeters
                        tripMeterAutomaticKm
                        tripMeterManualKm
                    }
                }
            }
            """,
            "GetBatteryData",
            new { vin = _vin });

        return carTelematics; //carTelematics;
    }
    
    private async Task<string> GetLoginFlowTokens()
    {
        string url = $"https://polestarid.eu.polestar.com/as/authorization.oauth2" +
                     $"?response_type=code" +
                     $"&client_id={CLIENT_ID}" +
                     $"&redirect_uri=https://www.polestar.com%2Fsign-in-callback" +
                     $"&scope=openid+profile+email+customer%3Aattributes" +
                     $"&state=ea5aa2860f894a9287a4819dd5ada85c" +
                     $"&code_challenge={CODE_CHALLENGE}" +
                     $"&code_challenge_method=S256";
        
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
        string url = $"https://polestarid.eu.polestar.com/as/{pathToken}/resume/as/authorization.ping?client_id={CLIENT_ID}";
        
        var formData = new List<KeyValuePair<string, string>>
        {
            new ("pf.username", _username),
            new ("pf.pass", _password)
        };

        // Encodes the key-value pairs for the ContentType 'application/x-www-form-urlencoded'
        HttpContent content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.PostAsync(url, content);
        var urlResp = response.RequestMessage.RequestUri.AbsoluteUri;
        var data = Regex.Match(urlResp, "code=([^&]+)");
        var uidMatch = Regex.Match(urlResp, "uid=([^&]+)");
        
        if(data.Success == false && uidMatch.Success)
            return await AcceptTermsOfService(uidMatch.Groups?.Values?.ElementAt(1).Value, pathToken);

        return data.Groups.Values.ElementAt(1).Value;
    }

    private async Task<string> AcceptTermsOfService(string uid, string pathToken)
    {
        string url = $"https://polestarid.eu.polestar.com/as/{pathToken}/resume/as/authorization.ping?client_id={CLIENT_ID}";
        
        var formData = new List<KeyValuePair<string, string>>
        {
            new ("pf.submit", "True"),
            new ("subject", uid)
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
        var tokenEndpoint = "https://polestarid.eu.polestar.com/as/token.oauth2";

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", tokenRequestCode },
            { "code_verifier", CODE_VERIFIER },
            { "client_id", CLIENT_ID },
            { "redirect_uri", POLESTAR_REDIRECT_URI }
        };

        var content = new FormUrlEncodedContent(requestBody);
        var response = await _httpClient.PostAsync(tokenEndpoint, content);

        response.EnsureSuccessStatusCode();

        var apiCreds = await response.Content.ReadFromJsonAsync<GetAuthToken>();
        
        return new Token()
        {
            access_token = apiCreds.access_token,
            refresh_token = apiCreds.refresh_token,
            expires_in = apiCreds.expires_in
        };
    }

    private async Task<T> SendQuery<T>(string url, string query, string operationName, object variables,
        bool useAuth = true)
    {
        var req = new
        {
            query,
            variables
        };

        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
        {
            string json = System.Text.Json.JsonSerializer.Serialize(req);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            if (useAuth)
            {
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_token.access_token}");
            }

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}