using HtmlAgilityPack;

namespace GigaSharp;

public class WebScraping{
    public static Book GetBookFromWeb(int id){
        try{
            HtmlDocument doc = new HtmlWeb().Load("https://nhentai.xxx/g/"+id);
            if(doc == null) { return null; }

            HtmlNode infoBlock = doc.DocumentNode.SelectSingleNode("//div[@class=\"info\"]");
            Book.BookBuilder builder = new Book.BookBuilder().AddId(id);
            builder.AddName(infoBlock.Element("h1").InnerText);
            builder.AddCover(doc.DocumentNode.SelectSingleNode("//div[@class=\"cover\"]/a/img").Attributes["data-src"].Value);
            

            return builder.Build();
        }catch{
            return null;
        }
    }
}