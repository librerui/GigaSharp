namespace GigaSharp;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class NChanMain
{
    /*
        Set up and start the actual N-Chan bot and keep it running indefinitely.
    */
    public static async Task StartBot(){
        //Create the DiscordSocketClient and hook our logging method to the log event
        DiscordSocketClient client = new DiscordSocketClient();
        client.Log += Log;

        //Perform the login (currently logging into the test bot, not nchan)
        //await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("NCHAN_TOKEN"));
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("TEST_TOKEN"));
        await client.StartAsync();

        //Create interaction framework, load interaction module handler, and hook intraction processing to interaction event
        _ = InitCommands(client);
        
        // Run task indefinitely.
        await Task.Delay(-1);
    }

    /*
        Initialize all commands. Incomplete for now.
    */
    private static async Task InitCommands(DiscordSocketClient client){
        Console.WriteLine("REGISTERING COMMANDS...");
        InteractionService service = new InteractionService(client.Rest);
        Console.WriteLine("ADDING MODULE...");
        await service.AddModulesAsync(typeof(NChanCommands).Assembly, null);
        Console.WriteLine("MODULE ADDED!");
        client.InteractionCreated += async (interaction) =>
        {
            SocketInteractionContext ctx = new SocketInteractionContext(client, interaction);
            await service.ExecuteCommandAsync(ctx, null);
        };
        Console.WriteLine("STARTING REGISTRY...");
        await service.RegisterCommandsToGuildAsync(1181165855522959420);
        Console.WriteLine("COMMANDS REGISTERED!");
    } 

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}