using HtmlAgilityPack;

namespace GigaSharp;

public class WebScraping{
    public static Book GetBookFromWeb(int id){
        HtmlDocument doc = new HtmlWeb().Load("https://nhentai.xxx/g/"+id);
        if(doc == null) { return null; }
        if(CheckIf404(doc)) { return null; }
        HtmlDocument firstPage = new HtmlWeb().Load("https://nhentai.xxx/g/"+id+"/1");
        if(firstPage != null) { return null; }

        HtmlNode infoBlock = doc.DocumentNode.SelectSingleNode("//div[@class=\"info\"]");
        Book.BookBuilder builder = new Book.BookBuilder().AddId(id)
            .AddDoc(doc)
            .AddName(infoBlock.Element("h1").InnerText)
            .AddCover(doc.DocumentNode.SelectSingleNode("//div[@class=\"cover\"]/a/img").Attributes["data-src"].Value)
            .AddFirstPage(firstPage.DocumentNode.SelectSingleNode("//a[@class=\"fw_img\"]/img").Attributes["data-src"].Value);
        
        HtmlNodeCollection infoBlockCategories = infoBlock.SelectNodes("//li[position()<last()-1]/span");
        foreach(HtmlNode node in infoBlockCategories){
            string[] elements = new string[node.ParentNode.ChildNodes.Count-1];
            FillInfoBlockArray(elements, node);
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
    }

    private static void FillInfoBlockArray(string[] infoBlockCategory, HtmlNode htmlReference){
        htmlReference = htmlReference.NextSibling;
        int i = 0;
        for(i = 0; htmlReference!=null && i < infoBlockCategory.Length; i++){
            HtmlNode name = htmlReference.SelectSingleNode("span[@class=\"tag_name\"]");
            HtmlNodeCollection discard = name.SelectNodes("span");
            if(discard != null){
                foreach(HtmlNode child in discard){
                    name.RemoveChild(child);
                }
            }
            infoBlockCategory[i] = name.InnerText;
            htmlReference = htmlReference.NextSibling;
        }
        if(htmlReference != null || i < infoBlockCategory.Length){
            throw new Exception("Error on filling an info block array! Index at "+i+" and htmlreference null: "+(htmlReference==null));
        }
    }

    //Unused
    /*public static bool DoesBookExist(int id){
        HtmlDocument doc = new HtmlWeb().Load("https://nhentai.xxx/g/"+id);
        return !CheckIf404(doc);
    }*/

    private static bool CheckIf404(HtmlDocument doc){
        return doc.DocumentNode.SelectSingleNode("//div[@class=\"content_box\"]") != null;
    }
}