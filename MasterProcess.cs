namespace GigaSharp;

using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;

public class MasterProcess
{

    // --- DATABASE ---
    #region 
    public static bool databaseReady = false;
    //In SQLite, two transactions can't occur at the same time, and thus time out
    //very easily: We want to control when they even start to avoid that.
    public static bool databaseModificationOngoing = false;
    public static string sqlScriptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql");
    public static string databaseConnectionString = new SqliteConnectionStringBuilder(){
            DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db/GigaSharp.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        }.ToString();
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
    #endregion
    
    // --- HTTP CONNECTIONS ---
    #region
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
    #endregion

    // --- BOT CONTROLS ---
    #region 
    private static bool botsRunning = false;
    public static void StartBots(){
        if(botsRunning){return;}
        //Start as many bots as you wish, one after the other.
        //All bots will run until either Knyadministrator's shutdown command,
        //or a higher force stops them.
        _ = NChanMain.StartBot();
        _ = YChanMain.StartBot();
        _ = KnyadministratorMain.StartBot();
        botsRunning = true;
    }
    public static async Task StopBots(){
        if(!botsRunning){return;}
        //Run every bot's shutdown method, one after the other.
        await NChanMain.Shutdown();
        await YChanMain.Shutdown();
        await KnyadministratorMain.Shutdown();
        botsRunning = false;
    }
    #endregion

    // --- PROCESS LIFETIME ---
    #region 
    private static PosixSignalRegistration[] registers = new PosixSignalRegistration[2];
    public static void LifetimeConfiguration(){

        // Hooks the ShudownHook method to the ProcessExit event of the application domain.
        // There's... *several* ways to create a shutdown hook in C#, but a lot of the ones I found
        // didn't APPEAR to be for .NET (mostly .NET Framework, which is different, and Windows Desktop,
        // whatever that is), but this one I'm nearly sure is.
        AppDomain.CurrentDomain.ProcessExit += new EventHandler(ShutdownHook);

        registers[0] = PosixSignalRegistration.Create(PosixSignal.SIGTERM, SigtermHandler);
        registers[1] = PosixSignalRegistration.Create(PosixSignal.SIGINT, SigintHandler);
    }
    public static void SigtermHandler(PosixSignalContext context){
        Console.WriteLine("------ RECEIVED SIGTERM SIGNAL ------");
    }
    public static void SigintHandler(PosixSignalContext context){
        Program.shutdownApproved = true;
    }
    // Exit event handlers have a timeout (I think, this might actually
    // only apply to .NET Framework, but to be safe, we'll assume they are anyways), so, we can't
    // just do whatever we want.
    public static void ShutdownHook(object sender, EventArgs e){
        
        //Clean up usage of the HTTP client.
        gigaSharpHttpClient.CancelPendingRequests();
        gigaSharpHttpClient.Dispose();
    }
    public static async Task GracefulShutdown(){
        await StopBots();
        Program.Shutdown();
    }
    #endregion

    public static async void RunMasterProcess(){

        LifetimeConfiguration(); // See "process lifetime" region

        SetupDatabase(); // See "database" region

        _ = DailyEventsTimer();

        var baseuri = Environment.GetEnvironmentVariable("HOST_URL");
        if(baseuri != null){
            //Infinite 1-minute loop that performs a GET request to the render URL.
            HttpClient renderClient = new HttpClient(){
                BaseAddress = new Uri(baseuri)
            };
            while(true){
                try{
                    //we also call the fun facts api because it's also hosted on render and we want to keep
                    //it running
                    await new HttpClient().GetAsync(Environment.GetEnvironmentVariable("FUNFACTS_API"));
                    HttpResponseMessage response = await renderClient.GetAsync("Home");
                    if(!response.IsSuccessStatusCode){
                        Console.WriteLine("WARNING: Render ping failed!");
                    }
                    await Task.Delay(60000);
                }catch(Exception){
                    Console.WriteLine("--- ERROR ON RENDER PINGS, TRYING AGAIN... ---");
                    await Task.Delay(1000);
                }
            }
        }
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