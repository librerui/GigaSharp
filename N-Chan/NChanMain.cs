namespace GigaSharp;

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

public class NChanMain
{
    public static HashSet<IMessageChannel> botdChannels;
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
        client.ButtonExecuted += NChanCommands.ButtonHandler;
    }

    private async Task ReadyAsync(){
        if(services == null){
            throw new Exception("Services undefined upon attempting to register n-chan commands");
        }
        Console.WriteLine("STARTING REGISTRY...");
        //Registering to n-chan test server
        await services.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(990317819059118100);
        Console.WriteLine("COMMANDS REGISTERED!");

        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        Console.WriteLine("INTERACTIONS SET UP - STARTING BOOK OF THE DAY");
        botdChannels =
        [
            //By default, the n-chan-peak channel is a book of the day channel.
            //botdChannels.Add(client.GetChannel(1062831763098959982) as IMessageChannel);
            //The line below this one sets the general channel in n-chan test as a book of the day channel.
            //client.GetChannel(990317819059118103) as IMessageChannel,
        ];
        
        //Start the book of the day and run indefinitely
        _ = BookOfTheDay();
    }

    private async Task BookOfTheDay(){
        while(true){
            //Wait an hour
            await Task.Delay(3600000);
            if(DateTime.UtcNow.Hour != 10){
                continue;
            }
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
            foreach(IMessageChannel channel in botdChannels){
                await channel.SendMessageAsync("Today's ("+DateTime.Now.Date.ToString("dd/MM/yyyy")+") book of the day is:", embed: embed);
                await channel.SendMessageAsync(book.CreateRateMessage());
            }
        }
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}