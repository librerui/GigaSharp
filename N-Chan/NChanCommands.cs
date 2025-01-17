namespace GigaSharp;

using Discord.Interactions;

public class NChanCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Get a pong!")]
    public async Task Ping(){
        await RespondAsync("Pong! :ping_pong:");
    }

    [SlashCommand("ping-nhentai", "Ping nhentai.xxx to see if the website is down!")]
    public async Task PingNHentai(){
        bool result = await WebScraping.Ping();
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
            Book book = WebScraping.GetBookFromWeb(realId);
            if(book != null){
                await RespondAsync(embed: book.CreateEmbed());
            }else{
                await RespondAsync("I'm sowwy! I couldn't find that book, master :(");
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
            Book book = WebScraping.GetBookFromWeb(realId);
            if(book != null){
                await RespondAsync("Master! That book exists :3\nIt's called: "+book.Name);
            }else{
                await RespondAsync("Master! That book doesn't exist 3:");
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
                Book book = WebScraping.GetBookFromWeb(id);
                if(book != null){
                    await RespondAsync(embed: book.CreateEmbed());
                    break;
                }
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that book, master :(");
        }
    }
}