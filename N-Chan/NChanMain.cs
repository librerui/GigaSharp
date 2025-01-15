namespace GigaSharp;

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class NChanMain
{
    private static IServiceProvider? services;
    /*
        Set up and start the actual N-Chan bot and keep it running indefinitely.
    */
    public static async Task StartBot(){
        //Create the collection of services required to perform the bot's functions.
        services = new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>().Rest))
            .BuildServiceProvider();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        client.Log += Log;
        services.GetRequiredService<InteractionService>().Log += Log;

        //Perform the login
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("NCHAN_TOKEN"));
        await client.StartAsync();

        //Create interaction framework, load interaction module handler, and hook intraction processing to interaction event
        //From this point on, there is no need to work in a static environment, and for safety purposes, we won't.
        _ = new NChanMain().SetUp();
        
        // Run task indefinitely.
        await Task.Delay(-1);
    }

    /*
        Initialize all commands. Incomplete for now.
    */
    private async Task SetUp(){
        if(services == null){
            throw new Exception("Services undefined upon attempting to set up n-chan");
        }
        Console.WriteLine("PERFORMING N-CHAN SETUP");
        InteractionService service = services.GetRequiredService<InteractionService>();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        client.Ready += ReadyAsync;
        Console.WriteLine("ADDING MODULE...");
        await service.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        Console.WriteLine("MODULE ADDED!");
        client.InteractionCreated += async (interaction) =>
        {
            SocketInteractionContext ctx = new SocketInteractionContext(client, interaction);
            await service.ExecuteCommandAsync(ctx, services);
        };
    }

    private async Task ReadyAsync(){
        if(services == null){
            throw new Exception("Services undefined upon attempting to register n-chan commands");
        }
        Console.WriteLine("STARTING REGISTRY...");
        //Registering to n-chan test server
        await services.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(990317819059118100);
        Console.WriteLine("COMMANDS REGISTERED!");
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}