using HtmlAgilityPack;

namespace GigaSharp;

public class NChanWebScraping{
    public async static Task<bool> Ping(){
        try{
            HttpResponseMessage response = await MasterProcess.GetHttpClient().GetAsync("https://nhentai.xxx/home");
            return response.IsSuccessStatusCode;
        }catch{
            return false;
        }
    }
    public static Book GetBookFromWeb(int id){
        HtmlDocument doc = new HtmlWeb().Load("https://nhentai.xxx/g/"+id);
        if(doc == null) { return null; }
        if(CheckIf404(doc)) { return null; }
        HtmlDocument firstPage = new HtmlWeb().Load("https://nhentai.xxx/g/"+id+@"/1");
        if(firstPage == null) { return null; }

        HtmlNode infoBlock = doc.DocumentNode.SelectSingleNode("//div[@class=\"info\"]");
        Book.BookBuilder builder = new Book.BookBuilder().AddId(id)
            .AddDoc(doc)
            .AddName(infoBlock.Element("h1").InnerText)
            .AddFirstPage(firstPage.DocumentNode.SelectSingleNode("//a[@class=\"fw_img\"]/img").Attributes["data-src"].Value);
        
        HtmlNodeCollection infoBlockCategories = infoBlock.SelectNodes("//li[position()<last()-1]/span");
        foreach(HtmlNode node in infoBlockCategories){
            //NOTE: node.ParentNode.ChildNodes.Count-1 is the 1-based count of how many elements this set should have.
            //I'm fairly sure that by initializing this set with that capacity, we avoid ever having to do the
            //expensive resize operation on adding an element, but I could also be wrong, and the program could be
            //resizing it on the last add without me knowing. Based on Microsoft's documentation, the resize happens if
            //"the Set's count is already equal to its capacity on add", which means it shouldn't, but you know,
            //you can never be too careful.
            //I <3 my useless optimizations
            HashSet<string> elements = new HashSet<string>(node.ParentNode.ChildNodes.Count/2);
            FillInfoBlockArray(elements, node.ParentNode);
            switch (node.InnerText){
                case "Parodies": builder.AddParody(elements);
                    break;
                case "Characters": builder.AddCharacters(elements);
                    break;
                case "Tags": builder.AddTags(elements);
                    break;
                case "Artists": builder.AddArtists(elements);
                    break;
                case "Groups": builder.AddGroups(elements);
                    break;
                case "Languages": builder.AddLanguages(elements);
                    break;
                case "Category": builder.AddCategories(elements);
                    break;
            }
        }
        builder.AddPages(int.Parse(infoBlock.SelectSingleNode("//span[@class=\"tag_name pages\"]").InnerText));

        Book book = builder.Build();
        //Database insertion is quite slow, so we fire and forget the Insert method.
        //We also don't particularly care if this method actually succeeds: Even if a book for some
        //reason can't be inserted, it's not that bad to webscrape it.
        _ = NChanDatabase.InsertBook(book);
        return book;
    }

    private static void FillInfoBlockArray(HashSet<string> infoBlockCategory, HtmlNode htmlReference){
        HtmlNodeCollection blocks = htmlReference.SelectNodes("a");
        foreach(HtmlNode node in blocks){
            HtmlNode name = node.SelectSingleNode("span[@class=\"tag_name\"]");
            HtmlNodeCollection discard = name.SelectNodes("span");
            if(discard != null){
                foreach(HtmlNode child in discard){
                    name.RemoveChild(child);
                }
            }
            infoBlockCategory.Add(name.InnerText);
        }
    }

    public static List<(string, int)> GetBookTagsPopularity(int id){
        HtmlDocument doc = new HtmlWeb().Load("https://nhentai.xxx/g/"+id);
        if(doc == null) { return null; }
        List<(string, int)> result = new List<(string, int)>();
        HtmlNodeCollection infoBlockCategories = doc.DocumentNode.SelectNodes("//div[@class=\"info\"]//li[position()<last()-1]/span");
        foreach(HtmlNode node in infoBlockCategories){
            if(node.InnerText != "Tags"){
                continue;
            }
            HtmlNodeCollection blocks = node.ParentNode.SelectNodes("a");
            foreach(HtmlNode tagNode in blocks){
                HtmlNode name = tagNode.SelectSingleNode("span[@class=\"tag_name\"]");
                HtmlNode count = tagNode.SelectSingleNode("span[@class=\"tag_count \"]");
                HtmlNodeCollection discard = name.SelectNodes("span");
                if(discard != null){
                    foreach(HtmlNode child in discard){
                        name.RemoveChild(child);
                    }
                }
                result.Add((name.InnerText, int.Parse(count.InnerText.Replace("K", "000"))));
            }
            break;
        }
        return result;
    }

    private static bool CheckIf404(HtmlDocument doc){
        return doc.DocumentNode.SelectSingleNode("//div[@class=\"content_box\"]") != null;
    }
}