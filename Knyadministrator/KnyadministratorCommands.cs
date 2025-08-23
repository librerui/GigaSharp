namespace GigaSharp;

using Discord;
using Discord.Interactions;

//Enable these commands for use anywhere, even DMs, and with both guild and user installs.
[CommandContextType(InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel)]
[IntegrationType(ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall)]
public class KnyadministratorCommands : InteractionModuleBase<SocketInteractionContext>{
    [SlashCommand("attention", "Call the administrator to attention!")]
    public async Task Ping(){
        await RespondAsync("Present, sir! :saluting_face:");
    }

    [SlashCommand("botd_add", "Add a channel with the specified ulong ID to n-chan's book of the day list!")]
    public async Task BotdAdd(string channelId){
        try{
            ulong id = ulong.Parse(channelId);
            NChanMain.AddBotdChannel(id);
            await RespondAsync("Channel added to n-chan's book of the day registry, sir! :saluting_face:");
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("Apologies, but something went wrong, sir! :saluting_face:");
        }
    }

    [SlashCommand("botd_remove", "Remove a channel with the specified ulong ID from n-chan's book of the day list!")]
    public async Task BotdRemove(string channelId){
        try{
            ulong id = ulong.Parse(channelId);
            NChanMain.RemoveBotdChannel(id);
            await RespondAsync("Channel removed from n-chan's book of the day registry, sir! :saluting_face:");
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("Apologies, but something went wrong, sir! :saluting_face:");
        }
    }

    [SlashCommand("greeting_add", "Add a channel with the specified ulong ID to y-chan's greeting list!")]
    public async Task GreetingAdd(string channelId){
        try{
            ulong id = ulong.Parse(channelId);
            YChanMain.AddGreetingChannel(id);
            await RespondAsync("Channel added to y-chan's greeting registry, sir! :saluting_face:");
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("Apologies, but something went wrong, sir! :saluting_face:");
        }
    }

    [SlashCommand("greeting_remove", "Remove a channel with the specified ulong ID from y-chan's greeting list!")]
    public async Task GreetingRemove(string channelId){
        try{
            ulong id = ulong.Parse(channelId);
            YChanMain.RemoveGreetingChannel(id);
            await RespondAsync("Channel removed from y-chan's greeting registry, sir! :saluting_face:");
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("Apologies, but something went wrong, sir! :saluting_face:");
        }
    }

    [SlashCommand("greeting_spoiler", "Set whether to spoiler the y-chan daily greeting or not!")]
    public async Task GreetingSpoiler(bool status){
        try{
            YChanMain.spoilerDailyGreeting = status;
            await RespondAsync("Y-Chan's daily greeting spoilering set to "+status+", sir! :saluting_face:");
        }catch(Exception e){
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            await RespondAsync("Apologies, but something went wrong, sir! :saluting_face:");
        }
    }
}
