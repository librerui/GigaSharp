using HtmlAgilityPack;

namespace GigaSharp;

public class WebScraping{
    public static Book GetBookFromWeb(int id){
        try{
            HtmlDocument doc = new HtmlWeb().Load("https://nhentai.xxx/g/"+id);
            if(doc == null) { return null; }

            Console.WriteLine("Found html doc.");
            HtmlNode infoBlock = doc.DocumentNode.SelectSingleNode("//div[@class=\"info\"]");
            Book.BookBuilder builder = new Book.BookBuilder().AddId(id)
                .AddDoc(doc)
                .AddName(infoBlock.Element("h1").InnerText)
                .AddCover(doc.DocumentNode.SelectSingleNode("//div[@class=\"cover\"]/a/img").Attributes["data-src"].Value);
            Console.WriteLine("Name: "+infoBlock.Element("h1").InnerText);
            
            HtmlNodeCollection infoBlockCategories = infoBlock.SelectNodes("li/span");
            foreach(HtmlNode node in infoBlockCategories){
                Console.WriteLine("Processing category "+node.InnerText);
                string[] elements = new string[node.ParentNode.ChildNodes.Count-1];
                fillInfoBlockArray(elements, node);
                switch (node.InnerText){
                    case "Parodies:": builder.AddParody(elements);
                        break;
                    case "Characters:": builder.AddCharacters(elements);
                        break;
                    case "Tags:": builder.AddTags(elements);
                        break;
                    case "Artists:": builder.AddArtists(elements);
                        break;
                    case "Groups:": builder.AddGroups(elements);
                        break;
                    case "Languages:": builder.AddLanguages(elements);
                        break;
                    case "Categories:": builder.AddCategories(elements);
                        break;
                }
            }
            builder.AddPages(int.Parse(infoBlock.SelectSingleNode("//span[@class=\"tag_name pages\"]").InnerText));

            return builder.Build();
        }catch(Exception e){
            Console.WriteLine("Exception thrown somewhere: "+e.Message);
            Console.WriteLine(e.StackTrace);
            return null;
        }
    }

    private static void fillInfoBlockArray(string[] infoBlockCategory, HtmlNode htmlReference){
        htmlReference = htmlReference.NextSibling;
        int i = 0;
        Console.WriteLine("Attempting to fill array...");
        for(i = 0; htmlReference!=null && i < infoBlockCategory.Length; i++){
            HtmlNode name = htmlReference.SelectSingleNode("/span[@class=\"tag_name\"]");
            /*IEnumerable<HtmlNode> discard = name.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element);
            foreach(HtmlNode child in discard){
                name.RemoveChild(child);
            }*/
            Console.WriteLine("Block "+name.InnerText+" found.");
            infoBlockCategory[i] = name.InnerText;
            htmlReference = htmlReference.NextSibling;
        }
        Console.WriteLine("Array filled.");
        if(htmlReference != null || i < infoBlockCategory.Length){
            Console.WriteLine("Index at "+i+" and htmlreference null: "+(htmlReference==null));
            //throw new Exception("Something has gone wrong.");
        }
    }
}