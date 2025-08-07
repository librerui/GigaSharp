namespace GigaSharp;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;

[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class YChanFeatures : InteractionModuleBase<SocketInteractionContext>
{

    [SlashCommand("funfact", "Get a random fun fact!")]
    public async Task FunFact(){
        try{
            await RespondAsync(await YChanWebAccess.GetFunFact());
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("I'm sowwy! There was an error processing that request, master :(");
        }
    }
    
    //-------------------------------------------------------------------------------------------
    //----------------------- BELOW THIS IS EVENT HANDLERS, NOT COMMANDS ------------------------
    //-------------------------------------------------------------------------------------------

    public static async Task ButtonHandler(SocketMessageComponent component){
        string[] args = component.Data.CustomId.Split(" ");
        switch(args[0]){
            case "yesgrind": await component.Channel.SendMessageAsync("*emoji* ai sim? muito bem jovem " + component.User.Username + " isso é o que interessa *emoji* :thumbsup: muito foco no trabalho e nos pés *emoji* :thumbsup:");
                break;
            case "nogrind": await component.Channel.SendMessageAsync("*emoji* não? estou severamente desapontado sr(a) " + component.User.Username + " vou ter de o(a) prender :raised_hand: *emoji* mãos no teclado e pés onde eu os consiga ver :gun: *emoji* :anger:");
                break;
        }
    }

    public static async Task MessageScanner(SocketMessage message){

        //We first check if this is a message sent by a *user* (as opposed to another bot or a silent message)
        //We then check if this is a message sent in a guild channel (as opposed to a DM)
        if(message is not SocketUserMessage || message.Channel is not SocketTextChannel){
            return;
        }

        if(message.Content.ToLower().Contains("grind")){
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Sim", "yesgrind", ButtonStyle.Success);
            builder.WithButton("Não", "nogrind", ButtonStyle.Danger);
            await message.Channel.SendMessageAsync("*emoji* estou sim departamento do grind *emoji* era para saber se o(a) sr(a) ja grindou hoje", components: builder.Build());
        }
    }
}