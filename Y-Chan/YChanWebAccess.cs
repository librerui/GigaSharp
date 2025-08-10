namespace GigaSharp;

public class YChanWebAccess
{
    public static async Task<string> GetFunFact(){
        var factsapiuri = Environment.GetEnvironmentVariable("FUNFACTS_API");
        //https://thefact.space/random
        HttpClient funfactsapi = new HttpClient(){
            BaseAddress = new Uri(factsapiuri)
        };
        int delaycounter = 0;
        Task<HttpResponseMessage> res = funfactsapi.GetAsync((string)null);
        while(!res.IsCompleted){
            delaycounter++;
            await Task.Delay(500);
            if(delaycounter >= 5){
                res.Dispose();
                return "I'm sowwy! The fun facts API did not respond, master :(";
            }
        }
        string content = await res.Result.Content.ReadAsStringAsync();
        int factstart = content.IndexOf(':', content.IndexOf(':')+1)+2;
        return content.Substring(factstart, content.IndexOf("\",\"source\":")-factstart);
    }
}