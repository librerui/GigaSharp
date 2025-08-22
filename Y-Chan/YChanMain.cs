namespace GigaSharp;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class YChanMain
{
    private bool running = false;
    private static IServiceProvider services;

    public static async Task StartBot(){
        services = new ServiceCollection()
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig{
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>().Rest))
            .BuildServiceProvider();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        client.Log += Log;
        services.GetRequiredService<InteractionService>().Log += Log;

        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("YCHAN_TOKEN"));
        await client.StartAsync();

        _ = new YChanMain().SetUp();

        await Task.Delay(-1);
    }

    private async Task SetUp(){
        if(services == null){
            throw new Exception("Services undefined upon attempting to set up y-chan");
        }
        InteractionService service = services.GetRequiredService<InteractionService>();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        client.Ready += ReadyAsync;
        await service.AddModuleAsync(typeof(YChanFeatures), services);
        client.InteractionCreated += async (interaction) =>
        {
            SocketInteractionContext ctx = new SocketInteractionContext(client, interaction);
            await service.ExecuteCommandAsync(ctx, services);
        };
        client.ButtonExecuted += YChanFeatures.ButtonHandler;
        client.MessageReceived += YChanFeatures.MessageScanner;

        //Load the boafeira font
        YChanWebAccess.InitializeFont();
    }

    private async Task ReadyAsync(){
        if(running) { return; } //There are occasionally server disconnects where the previous session can't be restored.
        running = true; //These two lines prevent this method from re-running on reconnecting in those occasions.
        if(services == null){
            throw new Exception("Services undefined upon attempting to register y-chan commands");
        }
        //Global registry (MAY TAKE UP TO AN HOUR TO TAKE EFFECT)
        //COMMENT THIS LINE OUT WHEN TESTING OUT NEW COMMANDS - BE CAREFUL ABOUT DUPLICATE COMMANDS GLOBALLY AND ON GUILDS
        await services.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync();
        //Registering to the test server
        //await services.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(990317819059118100);
        
        Console.WriteLine("------------ Y-CHAN SETUP COMPLETE ------------");
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine("--- Y-CHAN LOG ---\n"+msg.ToString());
        return Task.CompletedTask;
    }
}