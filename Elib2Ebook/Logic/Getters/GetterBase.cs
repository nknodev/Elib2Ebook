using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Elib2Ebook.Configs;
using Elib2Ebook.Types.Book;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Elib2Ebook.Extensions;

namespace Elib2Ebook.Logic.Getters; 

public abstract class GetterBase : IDisposable {
    private readonly IdnMapping _idn = new();
    protected readonly BookGetterConfig _config;

    protected GetterBase(BookGetterConfig config) {
        _config = config;
    }

    protected abstract Uri SystemUrl { get; }

    protected virtual string GetId(Uri url) {
        return url.Segments.Last().Trim('/');
    }

    public virtual bool IsSameUrl(Uri url) {
        return string.Equals(_idn.GetAscii(SystemUrl.Host).Replace("www.", ""), _idn.GetAscii(url.Host).Replace("www.", ""), StringComparison.InvariantCultureIgnoreCase);
    }

    protected virtual HttpRequestMessage GetImageRequestMessage(Uri uri) {
        return new HttpRequestMessage(HttpMethod.Get, uri);
    }
    /// <summary>
    /// Получение изображения
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    protected async Task<Image> GetImage(Uri uri) {
        Console.WriteLine($"Загружаем картинку {uri.ToString().CoverQuotes()}");
        try {
            using var response = await _config.Client.SendWithTriesAsync(() => GetImageRequestMessage(uri));
            return response is { StatusCode: HttpStatusCode.OK } ? 
                new Image(uri.GetFileName(), await response.Content.ReadAsByteArrayAsync()) : 
                null;
        } catch (Exception) {
            return null;
        }
    }
        
    /// <summary>
    /// Загрузка изображений отдельной части
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="baseUri"></param>
    protected async Task<IEnumerable<Image>> GetImages(HtmlDocument doc, Uri baseUri) {
        var images = new List<Image>();
        foreach (var img in doc.QuerySelectorAll("img")) {
            var path = img.Attributes["src"]?.Value ?? img.Attributes["data-src"]?.Value;
            if (string.IsNullOrWhiteSpace(path)) {
                img.Remove();
                continue;
            }
        
            if (!Uri.TryCreate(baseUri, path, out var uri)) {
                img.Remove();
                continue;
            }
                
            var image = await GetImage(uri);
            if (image?.Content == null || image.Content.Length == 0) {
                img.Remove();
                continue;
            }
                
            // Костыль. Исправление урла картинки, что бы она отображась в книге
            if (img.Attributes["src"] == null) {
                img.Attributes.Add("src", uri.GetFileName());
            } else {
                img.Attributes["src"].Value = uri.GetFileName();
            }
            
            images.Add(image);
        }

        return images;
    }

    public virtual Task Authorize() {
        return Task.CompletedTask;
    }

    public abstract Task<Book> Get(Uri url);
        
    public virtual Task Init() {
        _config.Client.DefaultRequestVersion = HttpVersion.Version20;
        _config.Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.0 Safari/605.1.15");
        _config.Client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _config.Client.DefaultRequestHeaders.Add("Accept-Language", "ru");
        _config.Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        
        return Task.CompletedTask;
    }

    public void Dispose() {
        _config?.Dispose();
    }
}