namespace GigaSharp;

using System.Diagnostics;
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

    //This does little more than kick off the start of all other bots and also keep the ping loop runing forever.
    public static async Task StartBots(){

        //Begin the database setup.
        SetupDatabase();

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
}