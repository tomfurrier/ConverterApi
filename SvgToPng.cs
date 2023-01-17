using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Svg;
using System.Net.Http;
using System.Drawing.Imaging;
using System.Drawing;

namespace ConverterApi;

public static class SvgToPng
{
    [FunctionName("SvgToPng")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        var url = req.Query["url"];
        _ = int.TryParse(req.Query["width"], out int width);
        _ = int.TryParse(req.Query["height"], out int height);

        using var httpClient = new HttpClient();
        var svgString = await httpClient.GetStringAsync(url);
        var svg = SvgDocument.FromSvg<SvgDocument>(svgString);
        var bitmap = svg.Draw(width, height);
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);

        return new FileContentResult(ImageToByteArray(bitmap), "image/png");
    }

    private static byte[] ImageToByteArray(Image image)
    {
        var converter = new ImageConverter();
        return (byte[])converter.ConvertTo(image, typeof(byte[]));
    }
}
