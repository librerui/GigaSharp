using Discord;
using HtmlAgilityPack;

namespace GigaSharp;

public class Book {
    public int Id{get;private set;}
    public int Pages{get;private set;}
    public string Name{get;private set;}
    public HashSet<string> Languages{get;private set;} //Storing these lists as hash sets rather than arrays
    public HashSet<string> Tags{get;private set;} //technically does waste a bit more memory, but given that Books
    public HashSet<string> Parody{get;private set;} //are generally transient objects that get deleted quickly after
    public HashSet<string> Artists{get;private set;} //being created, and this approach gives us better time efficiency
    public HashSet<string> Groups{get;private set;} //on common database operations like getting a full book, I think
    public HashSet<string> Characters{get;private set;} //it's not a problem and this is a better approach. If you can
    public HashSet<string> Categories{get;private set;} //think of something better, tell me about it and we'll see.
    public HtmlDocument Doc{get;private set;}
    public string FirstPage{get;private set;}

    public Embed CreateEmbed(){
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle(Name)
            .WithDescription("Read me at: https://nhentai.xxx/g/"+Id+"\nPages: "+Pages)
            .WithColor(Color.Purple)
            .WithImageUrl(FirstPage);

        string firstColumn = "";
        if(Parody != null && Parody.Count > 0){
            firstColumn += "**Parody of:**\n"+string.Join("\n", Parody)+"\n";
        }
        if(Characters != null && Characters.Count > 0){
            firstColumn += "**Starring:**\n"+string.Join("\n", Characters)+"\n";
        }
        if(Languages != null && Languages.Count > 0){
            firstColumn += "**Languages:**\n"+string.Join("\n", Languages)+"\n";
        }
        if(Categories != null && Categories.Count > 0){
            firstColumn += "**Categories:**\n"+string.Join("\n", Categories);
        }
        if(firstColumn != ""){
            builder.AddField("Content information:", (firstColumn != "") ? firstColumn : "_ _", true);
        }
        

        if(Tags != null && Tags.Count > 0){
            builder.AddField("Tags:", string.Join("\n", Tags), true);
        }else{
            builder.AddField("Tags:", string.Join("_ _", Tags), true);
        }

        string thirdColumn = "";
        if(Artists != null && Artists.Count > 0){
            thirdColumn += "**Artists:**\n"+string.Join("\n", Artists)+"\n";
        }
        if(Groups != null && Groups.Count > 0){
            thirdColumn += "**Groups:**\n"+string.Join("\n", Groups)+"\n";
        }
        if(thirdColumn != ""){
            builder.AddField("Credit to:", (thirdColumn != "") ? thirdColumn : "_ _", true);
        }
        
        return builder.Build();
    }

    private (double, List<(string, int)>) RateBook(){
        //A book without enough tags is impossible to rate, therefore we return a score of -1
        if(Tags == null || Tags.Count <= 2){
            return (-1, null);
        }
        //IMPORTANT: Right now, we're getting every tag's popularity through webscraping.
        //This is obviously bad, but we need it, because the database doesn't provide an alternative.
        List<(string, int)> tagPopularity = WebScraping.GetBookTagsPopularity(Id);
        tagPopularity.Sort(delegate((string, int) part1, (string, int) part2){
            return part2.Item2.CompareTo(part1.Item2);
        });
        double tagMean = 0;
        foreach((string, int) item in tagPopularity){
            tagMean += item.Item2;
        }
        tagMean /= tagPopularity.Count;
        return (tagMean / 10000, tagPopularity);
    }

    public string CreateRateMessage(){
        string message = "";
        switch(Pages){
            case <=15: message += "It's a short book, with only "+Pages+" pages!\n";
                break;
            case <=50: message += "It's a medium book, with only "+Pages+" pages!\n";
                break;
            case <=150: message += "It's a long book, with up to "+Pages+" pages!\n";
                break;
            default: message += "It's a really long book, with up to "+Pages+" pages!\n";
                break;
        }
        switch(Tags.Count){
            case <=5: message += "It's poorly tagged, with only "+Tags.Count+" tags!\n";
                break;
            case <=10: message += "It's fairly tagged, with "+Tags.Count+" tags!\n";
                break;
            default: message += "It's well tagged, with up to "+Tags.Count+" tags!\n";
                break;
        }
        (double, List<(string, int)>) rateScore = RateBook();
        switch(rateScore.Item1){
            case -1: message += "It really doesn't have many tags, so it's hard to judge it 3:\n";
                break;
            case <5: message += "It's on the weirder side, appealing to niches like "+rateScore.Item2[rateScore.Item2.Count-1].Item1+" and "+rateScore.Item2[rateScore.Item2.Count-2].Item1+"\n";
                break;
            default: message += "It appeals to a wider audience, with tags like "+rateScore.Item2[0].Item1+" and "+rateScore.Item2[1].Item1+"\n";
                break;
        }
        if(Pages <= 20){
            rateScore.Item1 /= 1.5;
        }
        if(rateScore.Item1 < 0){
            rateScore.Item1 = 0;
        }
        message += "\nMy final rating is one of: "+Math.Round(rateScore.Item1, 2)+" ";
        for(int i = 0; i < (int)rateScore.Item1; i++){
            message += ":star:";
        }
        return message;
    }

    public class BookBuilder{
        Book b = new Book();
        public Book Build() { return b; }
        public BookBuilder AddId(int id) { b.Id = id; return this; }
        public BookBuilder AddPages(int pages) { b.Pages = pages; return this; }
        public BookBuilder AddName(string name) { b.Name = name; return this; }
        public BookBuilder AddLanguages(HashSet<string> languages) { b.Languages = languages; return this; }
        public BookBuilder AddTags(HashSet<string> tags) { b.Tags = tags; return this; }
        public BookBuilder AddParody(HashSet<string> parody) { b.Parody = parody; return this; }
        public BookBuilder AddArtists(HashSet<string> artists) { b.Artists = artists; return this; }
        public BookBuilder AddGroups(HashSet<string> groups) { b.Groups = groups; return this; }
        public BookBuilder AddCharacters(HashSet<string> characters) { b.Characters = characters; return this; }
        public BookBuilder AddCategories(HashSet<string> categories) { b.Categories = categories; return this; }
        public BookBuilder AddDoc(HtmlDocument doc) { b.Doc = doc; return this; }
        public BookBuilder AddFirstPage(string firstPage) { b.FirstPage = firstPage; return this; }
    }
}