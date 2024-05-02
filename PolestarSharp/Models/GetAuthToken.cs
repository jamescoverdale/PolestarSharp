namespace PolestarSharp.Models;

public class Auth
{
    public aData data { get; set; }
}

public class aData
{
    public GetAuthToken getAuthToken { get; set; }
}

public class GetAuthToken
{
    public string id_token { get; set; }
    public string access_token { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
}