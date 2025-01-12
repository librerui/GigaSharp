namespace GigaSharp;

public class MasterProcess
{
    public static async Task StartBots(){

        //Start as many bots as you wish, one after the other.
        //All bots will run indefinitely and cannot be restarted if shut down.
        _ = NChanMain.StartBot();

        var baseuri = Environment.GetEnvironmentVariable("HOST_URL");
        if(baseuri != null){
            //Infinite 5-minute loop that performs a GET request to the render URL.
            HttpClient renderClient = new HttpClient(){
                BaseAddress = new Uri(baseuri)
            };
            while(true){
                HttpResponseMessage response = await renderClient.GetAsync("Home");
                Console.WriteLine("Render pinged. Ping success: " + response.IsSuccessStatusCode);
                await Task.Delay(300000);
            }
        }
    }
}