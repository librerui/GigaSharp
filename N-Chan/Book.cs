using Discord;
using HtmlAgilityPack;

namespace GigaSharp;

public class Book {
    public int Id{get;private set;}
    public int Pages{get;private set;}
    public string Name{get;private set;}
    public string Cover{get;private set;}
    public string[] Languages{get;private set;}
    public string[] Tags{get;private set;}
    public string[] Parody{get;private set;}
    public string[] Artists{get;private set;}
    public string[] Groups{get;private set;}
    public string[] Characters{get;private set;}
    public string[] Categories{get;private set;}
    public HtmlDocument Doc{get;private set;}

    public Embed CreateEmbed(){
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle(Name)
            .WithDescription("Read me at: https://nhentai.xxx/g/"+Id+"\nPages: "+Pages)
            .WithColor(Color.Purple)
            .WithImageUrl(Cover)
            .AddField("Parody of:", string.Join("\n", Parody))
            .AddField("Tags:", string.Join("\n", Tags), true)
            .AddField("Language:", string.Join("\n", Languages), true);

        if(Parody != null && Parody.Length > 0){
            builder.AddField("Parody of:", string.Join("\n", Parody));
        }
        if(Characters != null && Characters.Length > 0){
            builder.AddField("Starring:", string.Join("\n", Characters));
        }
        if(Tags != null && Tags.Length > 0){
            builder.AddField("Tags:", string.Join("\n", Tags), true);
        }
        if(Artists != null && Artists.Length > 0){
            builder.AddField("Artists:", string.Join("\n", Artists));
        }
        if(Groups != null && Groups.Length > 0){
            builder.AddField("Groups:", string.Join("\n", Groups));
        }
        if(Languages != null && Languages.Length > 0){
            builder.AddField("Languages:", string.Join("\n", Languages));
        }
        if(Categories != null && Categories.Length > 0){
            builder.AddField("Categories:", string.Join("\n", Categories));
        }
        
        return builder.Build();
    }

    public class BookBuilder{
        Book b = new Book();
        public Book Build() { return b; }
        public BookBuilder AddId(int id) { b.Id = id; return this; }
        public BookBuilder AddPages(int pages) { b.Pages = pages; return this; }
        public BookBuilder AddName(string name) { b.Name = name; return this; }
        public BookBuilder AddCover(string cover) { b.Cover = cover; return this; }
        public BookBuilder AddLanguages(string[] languages) { b.Languages = languages; return this; }
        public BookBuilder AddTags(string[] tags) { b.Tags = tags; return this; }
        public BookBuilder AddParody(string[] parody) { b.Parody = parody; return this; }
        public BookBuilder AddArtists(string[] artists) { b.Artists = artists; return this; }
        public BookBuilder AddGroups(string[] groups) { b.Groups = groups; return this; }
        public BookBuilder AddCharacters(string[] characters) { b.Characters = characters; return this; }
        public BookBuilder AddCategories(string[] categories) { b.Categories = categories; return this; }
        public BookBuilder AddDoc(HtmlDocument doc) { b.Doc = doc; return this; }
    }
}