namespace GigaSharp;

using System.Diagnostics;
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
            case "yesgrind": await component.Channel.SendMessageAsync("<:ness_happy:1095829617459335329> ai sim? muito bem jovem " + component.User.Username + " isso é o que interessa <:grindr:1095827706182111342> :thumbsup: muito foco no trabalho e nos pés <:ness_happy:1095829617459335329> :thumbsup:");
                await component.UpdateAsync(x => x.Components = null);
                break;
            case "nogrind": await component.Channel.SendMessageAsync("<:ness_really:1109543571813568652> não? estou severamente desapontado sr(a) " + component.User.Username + " vou ter de o(a) prender :raised_hand: <:grindr:1095827706182111342> mãos no teclado e pés onde eu os consiga ver :gun: <:ness_really:1109543571813568652> :anger:");
                await component.UpdateAsync(x => x.Components = null);
                break;
        }
    }

    public static async Task MessageScanner(SocketMessage message){

        //We first check if this is a message sent by a *user* (as opposed to another bot or a silent message)
        //We then check if this is a message sent in a guild channel (as opposed to a DM)
        SocketUserMessage userMessage = message as SocketUserMessage;
        if(userMessage == null || message.Author.IsBot){
            return;
        }
        SocketTextChannel msgChannel = userMessage.Channel as SocketTextChannel;
        if(msgChannel == null){
            return;
        }

        string text = userMessage.Content.ToLower();

        if(text.Contains("grind")){
            ComponentBuilder builder = new ComponentBuilder();
            builder.WithButton("Sim", "yesgrind", ButtonStyle.Success);
            builder.WithButton("Não", "nogrind", ButtonStyle.Danger);
            await msgChannel.SendMessageAsync("<:ness_call:1095827816441991209> estou sim departamento do grind <:grindr:1095827706182111342> era para saber se o(a) sr(a) ja grindou hoje", components: builder.Build());
        }

        if(text.Contains("gohan") || text.Contains("arroz")){
            await msgChannel.SendFileAsync("featureassist/gohan"+new Random().Next(1, 7)+".gif");
        }

        if(text.Contains("massa")){
            await msgChannel.SendFileAsync("featureassist/massa.gif");
        }
        if(text.Contains("batata")){
            await msgChannel.SendFileAsync("featureassist/batata.gif");
        }
        if(text.Contains("legumes")){
            await msgChannel.SendFileAsync("featureassist/legumes.gif");
        }

        /*if(text.Contains("inspect")){
            Console.WriteLine("---- STARTING ANALYSIS ----");
            Console.WriteLine("Current domain base directory: " + AppDomain.CurrentDomain.BaseDirectory);
            ProcessStartInfo psi = new ProcessStartInfo("pwd");
            psi.CreateNoWindow = true;
            Process process = Process.Start(psi);
            process.WaitForExit();
            ProcessStartInfo psi2 = new ProcessStartInfo("ls");
            psi2.CreateNoWindow = true;
            Process process2 = Process.Start(psi2);
            process2.WaitForExit();
            Console.WriteLine("---- ENDING ANALYSIS ----");
            await msgChannel.SendMessageAsync("Inspection complete! :3");
        }*/
    }
}