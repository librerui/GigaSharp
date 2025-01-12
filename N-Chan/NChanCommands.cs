namespace GigaSharp;

using Discord.Interactions;

public class NChanCommands : InteractionModuleBase
{
    [SlashCommand("echo-ping", "Receive a pong, followed by an echo of the input")]
    public async Task EchoPing(string input){
        await RespondAsync("Pong! "+input);
    }
}