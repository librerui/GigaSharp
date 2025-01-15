namespace GigaSharp;

using Discord.Interactions;

public class NChanCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("echo-ping", "Receive a pong, followed by an echo of the input")]
    public async Task EchoPing(string input){
        await RespondAsync("Pong! "+input);
    }

    [SlashCommand("book", "Get information about a particular book!")]
    public async Task Book(int id){
        Book book = WebScraping.GetBookFromWeb(id);
        if(book != null){
            await RespondAsync("BOOK RECEIVED!!!\nName: "+book.Name);
        }else{
            await RespondAsync("Something went wrong retrieving that book's information!");
        }
    }
}