namespace GigaSharp;

using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

//Enable these commands for use anywhere, even DMs, and with both guild and user installs.
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class NChanCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Get a pong!")]
    public async Task Ping(){
        await RespondAsync("Pong! :ping_pong:");
    }

    [SlashCommand("ping-nhentai", "Ping nhentai.xxx to see if the website is down!")]
    public async Task PingNHentai(){
        bool result = await NChanWebScraping.Ping();
        if(result){
            await RespondAsync("Master! I think nhentai is up :3");
        }else{
            await RespondAsync("Master! I think nhentai is down 3:");
        }
    }

    [SlashCommand("book", "Get information about a particular book! Identify it using its nhentai ID!")]
    public async Task Book(string id){
        try{
            int realId = int.Parse(id);
            Book book = NChanDatabase.GetBook(realId);
            if(book != null){
                await RespondAsync(embed: book.CreateEmbed());
            }else{
                book = NChanWebScraping.GetBookFromWeb(realId);
                if(book != null){
                    await RespondAsync(embed: book.CreateEmbed());
                }else{
                    await RespondAsync("I'm sowwy! I couldn't find that book, master :(");
                }
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that book, master :(");
        }
    }

    [SlashCommand("exists", "Check if any book exists with your specified ID!")]
    public async Task Exists(string id){
        try{
            int realId = int.Parse(id);
            Book book = NChanDatabase.GetBook(realId);
            if(book != null){
                await RespondAsync("Master! That book exists :3\nIt's called: "+book.Name);
            }else{
                book = NChanWebScraping.GetBookFromWeb(realId);
                if(book != null){
                    await RespondAsync("Master! That book exists :3\nIt's called: "+book.Name);
                }else{
                    await RespondAsync("Master! That book doesn't exist 3:");
                }
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that command, master :(");
        }
    }

    [SlashCommand("random", "Get information about a random book!")]
    public async Task Random(){
        try{
            while(true){
                int id = new Random().Next(1, 600001);
                Book book = NChanDatabase.GetBook(id);
                if(book != null){
                    await RespondAsync(embed: book.CreateEmbed());
                    break;
                }else{
                    book = NChanWebScraping.GetBookFromWeb(id);
                    if(book != null){
                        await RespondAsync(embed: book.CreateEmbed());
                        break;
                    }
                }
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing the chosen book, master :(");
        }
    }

    [SlashCommand("rate", "Get a rating for a book of your choice!")]
    public async Task Rate(string id){
        try{
            int realId = int.Parse(id);
            Book book = NChanDatabase.GetBook(realId);
            if(book != null){
                await RespondAsync(book.CreateRateMessage());
            }else{
                book = NChanWebScraping.GetBookFromWeb(realId);
                if(book != null){
                    await RespondAsync(book.CreateRateMessage());
                }else{
                    await RespondAsync("Master! That book doesn't exist 3:");
                }
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that command, master :(");
        }
    }

    [SlashCommand("read", "Read a book of your choice, page by page!")]
    public async Task Read(string id){
        try{
            int realId = int.Parse(id);
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Previous page", "readCommand prev");
            builder.WithButton("Next page", "readCommand next");
            builder.WithButton("Go to start page", "readCommand goToStart");
            Book book = NChanDatabase.GetBook(realId);
            if(book != null){
                builder.WithButton("Go to end page", "readCommand goToEnd "+book.Pages);
                builder.WithButton("Open book on site", url: "https://nhentai.xxx/g/"+book.Id, style: ButtonStyle.Link);
                await RespondAsync(embed: new EmbedBuilder().WithTitle(book.Name).WithImageUrl(book.FirstPage).Build(), components: builder.Build());
            }else{
                book = NChanWebScraping.GetBookFromWeb(realId);
                if(book != null){
                    builder.WithButton("Go to end page", "readCommand goToEnd "+book.Pages);
                    builder.WithButton("Open book on site", url: "https://nhentai.xxx/g/"+book.Id, style: ButtonStyle.Link);
                    await RespondAsync(embed: new EmbedBuilder().WithTitle(book.Name).WithImageUrl(book.FirstPage).Build(), components: builder.Build());
                }else{
                    await RespondAsync("Master! That book doesn't exist 3:");
                }
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that command, master :(");
        }
    }

    //-------------------------------------------------------------------------------------------
    //----------------------- BELOW THIS IS EVENT HANDLERS, NOT COMMANDS ------------------------
    //-------------------------------------------------------------------------------------------

    //This function is the master button handler function.
    //Whenever ANY buttons are pressed, this function will fire and receive the pressed button as
    //an argument. You must check the button's CustomID to know how to proceed.
    //In order to pass as much information as we can, custom IDs will be allowed to have "arguments"
    //of sorts separated by whitespaces.
    //WARNING: Don't try to create follow-up messages here, it doesn't work for some reasons.
    public static async Task ButtonHandler(SocketMessageComponent component){
        string[] args = component.Data.CustomId.Split(" ");
        switch(args[0]){
            case "readCommand":
                Embed embed = component.Message.Embeds.First();
                string url = embed.Image.ToString();
                int filenameDot = url.LastIndexOf(".");
                int slashBeforeFilename = url.LastIndexOf("/") + 1;
                int pageNumberDigitCount = filenameDot - slashBeforeFilename;
                int pageNumber = int.Parse(url.Substring(slashBeforeFilename, pageNumberDigitCount));
                url = url.Remove(slashBeforeFilename, pageNumberDigitCount);
                switch(args[1]){
                    case "prev": pageNumber--;
                        break;
                    case "next": pageNumber++;
                        break;
                    case "goToStart": pageNumber = 1;
                        break;
                    case "goToEnd": pageNumber = int.Parse(args[2]);
                        break;
                }
                url = url.Insert(slashBeforeFilename, pageNumber.ToString());
                await component.UpdateAsync(x => x.Embed = new EmbedBuilder().WithTitle(embed.Title).WithImageUrl(url).Build());
                break;
        }
    }
}