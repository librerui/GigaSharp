namespace GigaSharp;

using System.Net;
using HtmlAgilityPack;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

public class YChanWebAccess
{
    //C#'s DayOfTheWeek property is 0-based but starts on sunday
    private static string[] weekdays = ["domingo", "segunda", "terça", "quarta", "quinta", "sexta", "sábado"];
    private static FontCollection fonts = new FontCollection();

    public static void InitializeFont(){
        //Apparently loading fonts is an expensive operation, so, we only do it once, and then
        //get them further down the line.
        fonts.Add("featureassist/impact.ttf");
    }

    public static async Task<string> GetFunFact(){
        //https://thefact.space/random
        HttpResponseMessage res = await MasterProcess.GetHttpClient().GetAsync(Environment.GetEnvironmentVariable("FUNFACTS_API"));
        string content = await res.Content.ReadAsStringAsync();
        int factstart = content.IndexOf(':', content.IndexOf(':')+1)+2;
        return content.Substring(factstart, content.IndexOf("\",\"source\":")-factstart);
    }

    //In order to avoid creating files (which would introduce a lot of overhead and complications
    //involving the way docker implements container filesystems), we work exclusively with streams
    //when processing these images.
    public static async Task<Stream> GetBoafeiraImage(){
        HttpResponseMessage res = await MasterProcess.GetHttpClient().GetAsync("https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit=999&tags="+YChanLore.r34queries[new Random().Next(0, YChanLore.r34queries.Length)]+Environment.GetEnvironmentVariable("R34_API"));
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(await res.Content.ReadAsStringAsync());
        int num = new Random().Next(0, int.Parse(doc.DocumentNode.SelectSingleNode("posts").Attributes["count"].Value));
        string selectedImgUrl = doc.DocumentNode.SelectNodes("//post").ElementAt(num).Attributes["file_url"].Value;
        Image selectedImg = Image.Load(await MasterProcess.GetHttpClient().GetStreamAsync(selectedImgUrl));
        
        RichTextOptions options = new RichTextOptions(fonts.Get("Impact").CreateFont(selectedImg.Height/8)){
            Origin = new PointF(selectedImg.Width/20, selectedImg.Height / 4),
            WrappingLength = selectedImg.Width - (selectedImg.Width/19)
        };
        selectedImg.Mutate(x => x.DrawText(options, "Boa "+weekdays[(int)DateTime.UtcNow.DayOfWeek]+"-feira", Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2)));

        MemoryStream imgStream = new MemoryStream();
        selectedImg.Save(imgStream, new JpegEncoder()); //The decision to use a jpeg encoder is arbitrary and can change.
        selectedImg.Dispose();
        return imgStream;
    }
}