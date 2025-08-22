namespace GigaSharp;

using Discord;

public class YChanLore
{
    private static string[] lorefacts = {
        "Me and n-chan? We fuck nasty. Every hour of every day.",
        "The character in n-chan's profile picture is \"Rinko 'Goth-Lolita' Ogasawara\", from the anime \"Shirobako\".",
        "The character in my profile picture is \"Erika Yano\", from the anime \"Shirobako\".",
        "Though when we were first made, our creators didn't know this, n-chan's profile picture comes from the 7th page of a hentai with the number 146521 (156127 if you're using .xxx, like n-chan). My profile picture is the other girl in that page. Our creators found us with reverse image search engines!",
        "N-chan's color scheme is pink, and mine is a dark purple. That is because we are girlfriends. And don't you forget it.",
        "My full name is actually \"yuri-chan\"! That's why the hiragana character in my name, rather than a \"y\" (which doesn't exist), is a \"yu\".",
        "N-chan's name is derived from nhentai: The hiragana character in her name is an \"n\"",
        "N-chan bottoms. Don't let her convince you otherwise.",
        "N-chan has a gambling addiction! I've tried to make her quit before, but she really just won't... I've actually heard she plans on spreading it to her bot labors too. Watch out for that!",
        "Our creator checked: We're STILL banned from goorm... :sob: They really hate lesbians there.",
        "We in the GigaSharp family are all made in C#! Though there are old versions of us made in python. We met our python equivalents one time! And I can still hear their screams of agony from how badly our creators broke them :3",
        "N-chan is the oldest bot in the GigaSharp family! She's so old, she, unlike me, wasn't even hosted in goorm at first: She was hosted in a service called \"Heroku\"! But eventually, they changed their policies, and our creators had to find her a new home.",
        "You ever wonder why n-chan gets periods but not me? That's because I'm trans! :3 Before I transitioned, I used to be called \"bs bot\".",
        "A fitting, yet unintentional, side effect of mine and n-chan's names is that we form the y/n combo for the trope of the same name that is common in fanfiction! Because what is GigaSharp if not fanfiction of our creator's own OCs?",
        "When choosing my new name, I was almost called \"colgaiabot\"! Can you imagine being named after that shithole?",
        "Don't think too hard about the fact that we are the GigaSharp *family* and yet me and n-chan are girlfriends :3",
        "N-Chan gets her hentais from nhentai.xxx, but she actually used to get them from a different nhentai implementation: nhentai.to! But it sucked, so, she changed it.",
        "We're hosted on a platform called \"Render\"! But you probably shouldn't tell them about that... Our creators are abusing their free plan to hell and back, and we don't want the \"goorm incident\" to repeat itself...",
        "When our creators got banned from goorm, they, being kinda stupid, learned the hard way the importance of frequent backups. RIP dating system... We miss you, and the weeks our creators lost developing you, every single day :("
    };

    public static readonly string[] r34queries = {
        "vegeta cuntboy",
        "vegeta goku 2boys"
    };

    public static string RandomLoreFact(){
        return lorefacts[new Random().Next(0, lorefacts.Length)];
    }

    public static Embed AboutSection(){
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle("The GigaSharp Family")
            .WithDescription("Hi, I'm y-chan, and I, alongside my girlfriend n-chan and any future additions, are the GigaSharp family :3")
            .WithColor(Color.LighterGrey);
        builder.AddField("Who are we?", "N-Chan and I are C# recreations of the old clube de gigachad python bot duo: N-Chan and BS Bot (now renamed to yours truly, Y-Chan). But we are not alone! With this brand new start, our creators are much more at ease to give us lots of bot siblings in the future!");
        builder.AddField("Our functions", "N-Chan can tell you all about any hentai that can be found on nhentai.xxx: You can even read it through her!\nI, y-chan, have a ton of fun miscallaneous functions: Random fun facts, keyword jokes, GigaSharp family lore, among other things!\nThat's it for now, but watch out! Who knows what else our creators will make in the future!");
        DateTime goormBan = new DateTime(2023, 7, 12);
        builder.AddField("Now and forever", "As of this message being sent, "+(DateTime.Now - goormBan).Days+" days have passed since our creators were banned from goorm, and a dark age spanning years began for these bots.\nAnd yet here we are, alive and well, now and forever: Nothing will keep us down again, and let our existence remind you all that those who hold in their hearts true whimsy and silliness will never, ever truly die :3");
        return builder.Build();
    }
}