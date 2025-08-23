namespace GigaSharp;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class KnyadministratorMain {
    private static IServiceProvider services;

    /*
        Set up and start the actual Knyadministrator bot and keep it running indefinitely.
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
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("ADMINISTRATOR_TOKEN"));
        await client.StartAsync();

        //Create interaction framework, load interaction module handler, and hook intraction processing to interaction event
        //From this point on, there is no need to work in a static environment, and for safety purposes, we won't.
        _ = new KnyadministratorMain().SetUp();

        //Despite the fact that we just ran the SetUp method, any tasks that require connection to discord can't be
        //carried out just yet. They need to wait until the connection is ready. Use the ReadyAsync function for that.
        
        // Run task indefinitely.
        await Task.Delay(-1);
    }

    /*
        Initialize all commands.
    */
    private async Task SetUp(){
        if(services == null){
            throw new Exception("Services undefined upon attempting to set up knyadministrator");
        }
        InteractionService service = services.GetRequiredService<InteractionService>();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        client.Ready += ReadyAsync;
        await service.AddModuleAsync(typeof(KnyadministratorCommands), services);
        client.InteractionCreated += async (interaction) =>
        {
            SocketInteractionContext ctx = new SocketInteractionContext(client, interaction);
            await service.ExecuteCommandAsync(ctx, services);
        };
        client.ButtonExecuted += NChanCommands.ButtonHandler;
    }

    /*
        This function is triggered on the bot becoming fully ready for use.
        This effectively serves as post-connection processing for anything that needs a working Discord login.
    */
    private async Task ReadyAsync(){
        if(services == null){
            throw new Exception("Services undefined upon attempting to register knyadministrator commands");
        }
        //Global registry (MAY TAKE UP TO AN HOUR TO TAKE EFFECT)
        await services.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync();
        
        Console.WriteLine("------------ KNYADMINISTRATOR SETUP COMPLETE ------------");
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}
