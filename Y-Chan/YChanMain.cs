namespace GigaSharp;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class YChanMain
{
    private static HashSet<ulong> dailyGreetingChannels;
    public static bool Running{get; private set;}
    public static bool spoilerDailyGreeting = true;
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
        if(Running) { return; }
        
        if(services == null){
            throw new Exception("Services undefined upon attempting to register y-chan commands");
        }
        //Global registry (MAY TAKE UP TO AN HOUR TO TAKE EFFECT)
        await services.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync();

        dailyGreetingChannels =
        [
            //By default, the bot-chat channel gets a daily greeting.
            1062831579514290306
        ];
        
        Running = true;

        Console.WriteLine("------------ Y-CHAN SETUP COMPLETE ------------");
    }

    public static void AddGreetingChannel(ulong id){
        IMessageChannel channel = services.GetRequiredService<DiscordSocketClient>().GetChannel(id) as IMessageChannel;
        if(channel != null){dailyGreetingChannels.Add(id);}
        else { throw new Exception("Channel could not be found."); }
    }
    //This function is essentially pointless right now because we don't need to verify anything before
    //trying to remove the channel, but it stays here anyways.
    public static void RemoveGreetingChannel(ulong id){
        dailyGreetingChannels.Remove(id);
    }

    public static async Task DailyGreeting(){
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        Console.WriteLine("YCHAN: ATTEMPTING DAILY GREETING...");
        string funfact = await YChanWebAccess.GetFunFact();
        string lorefact = YChanLore.RandomLoreFact();
        Stream boafeiraImg = await YChanWebAccess.GetBoafeiraImage();
        Console.WriteLine("YCHAN: GREETING READY. SENDING TO CHANNELS.");
        foreach(ulong id in dailyGreetingChannels){
            IMessageChannel channel = client.GetChannel(id) as IMessageChannel;
            if(channel == null){
                Console.WriteLine("Y-CHAN GREETING ERROR: Channel with ID "+ id + " in list, but not found by y-chan client.");
                continue;
            }
            await channel.SendFileAsync(new FileAttachment(boafeiraImg, "boafeira.jpeg", isSpoiler: spoilerDailyGreeting));
            await channel.SendMessageAsync("good morning knyigachads!!!\nToday's fun fact is: "+funfact+"\nToday's lore fact is: "+lorefact);
            Console.WriteLine("YCHAN: SENT TO CHANNEL "+id);
        }
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine("--- Y-CHAN LOG ---\n"+msg.ToString());
        return Task.CompletedTask;
    }

    public static async Task Shutdown(){
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        await client.LogoutAsync();
        await client.StopAsync();
    }
}