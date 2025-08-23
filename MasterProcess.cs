namespace GigaSharp;

using Microsoft.Data.Sqlite;

public class MasterProcess
{
    //This flag marks whether or not the master database is currently ready for use.
    //By default it isn't, as the database needs to be downloaded or set up.
    public static bool databaseReady = false;
    //This flag marks whether or not a modification is currently underway in the database.
    //This is necessary because two transactions can't occur at the same time, and thus time out
    //very easily: We want to control when they even start to avoid that.
    public static bool modificationOngoing = false;
    //This is the absolute path to the sql directory in which all scripts are stored.
    //All bots will reference this string in order to get the script for any particular database operation they want.
    public static string sqlScriptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql");
    //This is the connection string to the master SQLite database shared by the entire program.
    //To avoid relative path complications and ensure every database access has the same features enabled,
    //it will be stored here and referenced in every bot's database operations. The DataSource field must also
    //be declared as an absolute path in order to avoid relative path complications when this string is referenced
    //in each bot's subdirectories.
    public static string databaseConnectionString = new SqliteConnectionStringBuilder(){
            DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db/GigaSharp.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();
    //This is the global http client used by the whole gigasharp family.
    //In C#, it's good practice to either use HttpClientFactory to create several short-lived clients
    //that are later disposed, *or* to use one global singleton long-running client. The initialization
    //you see basically means that all long-running connections are refreshed every 5 minutes.
    //This is done in order to respect time-to-live durations, but the measure of 5 minutes is arbitrary,
    //seeing as this currently isn't relevant and may only become relevant with future bots.
    private static HttpClient gigaSharpHttpClient = new HttpClient(new SocketsHttpHandler{
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    });
    //This is just the method to retrieve our singleton http client.
    public static HttpClient GetHttpClient(){return gigaSharpHttpClient;}

    //This does little more than kick off the start of all other bots and also keep the ping loop runing forever.
    public static async Task StartBots(){

        // Hooks the ShudownHook method to the ProcessExit event of the application domain.
        // There's... *several* ways to create a shutdown hook in C#, but a lot of the ones I found
        // didn't APPEAR to be for .NET (mostly .NET Framework, which is different, and Windows Desktop,
        // whatever that is), but this one I'm nearly sure is.
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(ShutdownHook);

        //Begin the database setup.
        SetupDatabase();

        //Start as many bots as you wish, one after the other.
        //All bots will run indefinitely and cannot be restarted if shut down.
        _ = NChanMain.StartBot();
        _ = YChanMain.StartBot();
        _ = KnyadministratorMain.StartBot();

        // Begin running the daily events.
        _ = DailyEventsTimer();

        var baseuri = Environment.GetEnvironmentVariable("HOST_URL");
        if(baseuri != null){
            //Infinite 5-minute loop that performs a GET request to the render URL.
            HttpClient renderClient = new HttpClient(){
                BaseAddress = new Uri(baseuri)
            };
            while(true){
                HttpResponseMessage response = await renderClient.GetAsync("Home");
                if(!response.IsSuccessStatusCode){
                    Console.WriteLine("WARNING: Render ping failed!");
                }
                await Task.Delay(60000);
            }
        }
    }

    // Shutdown event handler. Exit event handlers have a timeout (I think, this might actually
    // only apply to .NET Framework, but to be safe, we'll assume they are anyways), so, we can't
    // just ping render constantly until we get a response. Instead, we'll ping render once, and then
    // run another N-Chan, who will hopefully herself ping render a lot and keep the container going.
    public static void ShutdownHook(object sender, EventArgs e){
        // We don't actually particularly care about the result of this ping: The app is shutting down anyways.
        // To avoid making this handler async and potentially causing issues, we don't await this.
        new HttpClient(){
            BaseAddress = new Uri(Environment.GetEnvironmentVariable("HOST_URL"))
        }.GetAsync("Home");

        //Clean up usage of the HTTP client.
        gigaSharpHttpClient.CancelPendingRequests();
        gigaSharpHttpClient.Dispose();

        //------- THE FOLLOWING SECTION IS OUTDATED, THE RESTART METHOD IS BROKEN ----------
        //-------------- KEPT HERE FOR FUTURE REFERENCE -------------
        
        // Imma keep it real with you chief, I have *no* idea if this will actually work.
        // The problem is that docker containers don't necessarily include a shell, and I'm pretty sure ours doesn't.
        // The information I found online about running commands in a container is abysmal and applies only to when
        // you, the user, want to run a command inside a container from outside the container. There's basically nothing
        // about running commands from a process inside the container.
        // I'm assuming that a shell isn't necessary here since we can just run the dotnet app directly rather than
        // go through the middleman of the shell, but still.
        /*ProcessStartInfo psi = new ProcessStartInfo("dotnet", "/app/GigaSharp.dll");
        psi.CreateNoWindow = true;
        Process process = Process.Start(psi);
        if(process == null){
            Console.WriteLine("WARNING: ATTEMPTED TO START NEW GIGASHARP AND FAILED.");
        }else{
            Console.WriteLine("--- NEW GIGASHARP PROCESS STARTED ---");
        }*/
    }

    public static async void SetupDatabase(){

        //First, we attempt to download the current database file being stored remotely.
        //TODO: Implement this feature.

        //In the event that the above download fails, we will have to create the database ourselves.
        SqliteConnection conn = new SqliteConnection(databaseConnectionString);
        conn.Open();
        //Each bot that requires a database will have its own individual database management class, with its own
        //CreateDatabase method, to create whatever database schema each bot requires. Call them all here.
        NChanDatabase.CreateDatabase(conn);
        conn.Close();
        databaseReady = true;
        Console.WriteLine("---------- DATABASE IS READY ----------");
    }

    public static async Task DailyEventsTimer(){
        while(!NChanMain.Running || !YChanMain.Running){
            await Task.Delay(1000);
        }
        while(true){
            if(DateTime.UtcNow.Hour != 10){
                //Wait an hour
                await Task.Delay(3600000);
                continue;
            }
            _ = NChanMain.BookOfTheDay();
            _ = YChanMain.DailyGreeting();
            //Wait an hour
            await Task.Delay(3600000);
        }
    }
}