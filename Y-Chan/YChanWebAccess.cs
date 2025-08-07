namespace GigaSharp;

public class YChanWebAccess
{
    public static async Task<string> GetFunFact(){
        var factsapiuri = Environment.GetEnvironmentVariable("FUNFACTS_API");
        //https://thefact.space/random
        HttpClient funfactsapi = new HttpClient(){
            BaseAddress = new Uri(factsapiuri)
        };
        HttpResponseMessage res = await funfactsapi.GetAsync((string)null);
        string content = await res.Content.ReadAsStringAsync();
        int factstart = content.IndexOf(':', content.IndexOf(':')+1)+2;
        return content.Substring(factstart, content.IndexOf("\",\"source\":")-factstart);
    }
}