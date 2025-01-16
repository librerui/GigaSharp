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
        try{
            Book book = WebScraping.GetBookFromWeb(id);
            if(book != null){
                await RespondAsync("BOOK RECEIVED!!!\nName: "+book.Name+
                "\nFirst tag: "+book.Tags[0]+"\nLast tag: "+book.Tags[book.Tags.Length-1]+
                "\nFirst character: "+book.Characters[0]+"\nLast character: "+book.Characters[book.Characters.Length-1]+
                "\nPages: "+book.Pages+"\nParody: "+book.Parody[0]);
            }else{
                await RespondAsync("I'm sowwy! I couldn't find that book, master :(");
            }
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that book, master :(");
        }
    }
}