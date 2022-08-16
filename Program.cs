using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    /*
        The number that will receive the call. Test accounts are limited to verified numbers.
        The number must be in E.164 Format, e.g. Netherlands 0639111222 -> +31639111222
    */ 
    private const string _to = "<REPLACE_WITH_TO_NUMBER>";

    /*
        The number that will be displayed on the phone when making the call. Test accounts are limited to the free test number.
        The number must be in E.164 Format, e.g. Netherlands 0639111222 -> +31639111222
    */ 
    private const string _from = "<REPLACE_WITH_FROM_NUMBER>";

    /*
        The key from one of your Voice Apps, found here https://dashboard.sinch.com/voice/apps
    */
    private const string _key = "<REPLACE_WITH_APP_KEY>";

    /*
        The secret from the Voice App that uses the key above, found here https://dashboard.sinch.com/voice/apps
    */
    private const string _secret = "<REPLACE_WITH_APP_SECRET>";

    private const string _sinchUrl = "https://calling.api.sinch.com/calling/v1/callouts";
    private const string _timeHeader = "x-timestamp";

    static async Task Main(string[] args)
    {
        await MakeACall();
    }

    private static async Task MakeACall() 
    {
        using (var _client = new HttpClient())
        {
            var requestBody = GetCalloutRequestBody();
            var timestamp = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

            var b64decodedApplicationSecret = Convert.FromBase64String(_secret);
            var stringToSign = await BuildStringToSignAsync(requestBody, timestamp);

            var authAuthAppValue = _key + ":" + GetSignature(b64decodedApplicationSecret, stringToSign);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _sinchUrl);
            requestMessage.Headers.TryAddWithoutValidation("authorization", "application " + authAuthAppValue);
            requestMessage.Headers.Add(_timeHeader, timestamp);
            requestMessage.Content = requestBody;

            var request = await _client.SendAsync(requestMessage);

            request.EnsureSuccessStatusCode();
            
            Console.WriteLine(request.StatusCode + " " + request.ReasonPhrase);
        }
    }

    private static string GetSignature(byte[] secret, string stringToSign)
    {
        using (var sha = new HMACSHA256(secret))
        {
            return Convert.ToBase64String(
                sha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign))
            );
        }
    }

    private static async Task<string> BuildStringToSignAsync(StringContent requestBody, string timestamp)
    {
        var sb = new StringBuilder();

        sb.Append("POST");
        sb.AppendLine();

        using (var md5 = MD5.Create())
		{
            sb.Append(Convert.ToBase64String(md5.ComputeHash(await requestBody.ReadAsByteArrayAsync().ConfigureAwait(false))));
        }
        sb.AppendLine();

        sb.Append("application/json; charset=utf-8");
        sb.AppendLine();

        sb.Append(_timeHeader);
        sb.Append(":");
        sb.Append(timestamp);
        sb.AppendLine();

        sb.Append("/calling/v1/callouts/");

        return sb.ToString();
    }

    public static StringContent GetCalloutRequestBody()
    {
        var myData = new
        {
            method = "ttsCallout",
            ttsCallout = new 
            {
                cli = _from,
                destination = new 
                {
                    type = "number",
                    endpoint = _to
                },
                locale = "en-US",
                text = "Hello, this is a call from Sinch. Congratulations! You made your first call."
            },
        };
        
        return new StringContent(
            JsonSerializer.Serialize(myData),
            Encoding.UTF8,
            Application.Json
        );
    }
}
