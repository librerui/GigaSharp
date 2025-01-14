namespace GigaSharp;

using Discord.Interactions;
using Discord.WebSocket;

public class NChanCommands : InteractionModuleBase<SocketInteractionContext<SocketInteraction>>
{
    [SlashCommand("echo-ping", "Receive a pong, followed by an echo of the input")]
    public async Task EchoPing(string input){
        await RespondAsync("Pong! "+input);
    }
}