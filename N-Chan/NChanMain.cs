namespace GigaSharp;

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class NChanMain
{
    //You might wonder why this is a ulong HashSet rather than an IMessageChannel HashSet,
    //which would allow us to send messages directly with its contents.
    //This is because the IMessageChannel class doesn't have a GetHashCode or Equals override,
    //and thus doesn't work for storage in a HashSet.
    private static HashSet<ulong> botdChannels;
    private bool running = false;
    private static IServiceProvider services;
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
            throw new Exception("Services undefined upon attempting to set up n-chan");
        }
        InteractionService service = services.GetRequiredService<InteractionService>();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        client.Ready += ReadyAsync;
        await service.AddModuleAsync(typeof(NChanCommands), services);
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
        if(running) { return; } //There are occasionally server disconnects where the previous session can't be restored.
        running = true; //These two lines prevent this method from re-running on reconnecting in those occasions.
        //Such a thing would create 2 separate book of the day threads, which is bad.
        if(services == null){
            throw new Exception("Services undefined upon attempting to register n-chan commands");
        }
        //Global registry (MAY TAKE UP TO AN HOUR TO TAKE EFFECT)
        //COMMENT THIS LINE OUT WHEN TESTING OUT NEW COMMANDS - BE CAREFUL ABOUT DUPLICATE COMMANDS GLOBALLY AND ON GUILDS
        await services.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync();
        //Registering to n-chan test server
        //await services.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(990317819059118100);
        
        Console.WriteLine("------------ N-CHAN SETUP COMPLETE, STARTING BOOK OF THE DAY ------------");

        botdChannels =
        [
            //By default, the n-chan-peak channel is a book of the day channel.
            1062831763098959982,
            //The line below this one sets the general channel in n-chan test as a book of the day channel.
            //client.GetChannel(990317819059118103) as IMessageChannel,
        ];
        
        //Start the book of the day and run indefinitely
        _ = BookOfTheDay();
    }

    public static void AddBotdChannel(ulong id){
        IMessageChannel channel = services.GetRequiredService<DiscordSocketClient>().GetChannel(id) as IMessageChannel;
        if(channel != null){botdChannels.Add(id);}
        else { throw new Exception("Channel could not be found."); }
    }
    //This function is essentially pointless right now because we don't need to verify anything before
    //trying to remove the channel, but it stays here anyways.
    public static void RemoveBotdChannel(ulong id){
        /*IMessageChannel channel = services.GetRequiredService<DiscordSocketClient>().GetChannel(id) as IMessageChannel;
        if(channel != null){botdChannels.Remove(id);}
        else { throw new Exception("Channel could not be found."); }*/
        botdChannels.Remove(id);
    }

    private async Task BookOfTheDay(){
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        while(true){
            if(DateTime.UtcNow.Hour != 10){
                //Wait an hour
                await Task.Delay(3600000);
                continue;
            }
            Console.WriteLine("NCHAN: ATTEMPTING BOOK OF THE DAY...");
            Book book = null;
            while(book == null){
                int id = new Random().Next(1, 600001);
                book = NChanDatabase.GetBook(id);
                if(book == null){
                    book = WebScraping.GetBookFromWeb(id);
                    if(book == null){
                        continue;
                    }
                }
            }
            Embed embed = book.CreateEmbed();
            Console.WriteLine("NCHAN: BOOK OF THE DAY RECEIVED. SENDING TO CHANNELS.");
            foreach(ulong id in botdChannels){
                IMessageChannel channel = client.GetChannel(id) as IMessageChannel;
                if(channel == null){
                    Console.WriteLine("N-CHAN BOOK OF THE DAY ERROR: Channel with ID "+ id + " in list, but not found by n-chan client.");
                    continue;
                }
                await channel.SendMessageAsync("Today's ("+DateTime.Now.Date.ToString("dd/MM/yyyy")+") book of the day is:", embed: embed);
                await channel.SendMessageAsync(book.CreateRateMessage());
                Console.WriteLine("NCHAN: SENT TO CHANNEL "+id);
            }
            //Wait an hour
            await Task.Delay(3600000);
        }
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}