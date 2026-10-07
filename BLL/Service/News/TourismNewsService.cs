using Microsoft.Extensions.Configuration;
using Rovaya.DAL.DTO.External.News;
using Rovaya.DAL.DTO.Response.News;
using System.Net.Http.Json;
using System.Text.Json;

namespace Rovaya.BLL.Service.External
{
    public class TourismNewsService : ITourismNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public TourismNewsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = _configuration["ExternalAPIs:TourismMinistryBaseUrl"] ?? "https://tourism.ps/wp-json/wp/v2";
        }

        public async Task<TourismNewsResponse> GetLatestNewsAsync(int count = 10)
        {
            try
            {
                // نستخدم _embed لمحاولة جلب بيانات الوسائط مضمنة لتسريع العملية
                var response = await _httpClient.GetAsync($"{_baseUrl}/posts?per_page={count}&_embed");

                if (!response.IsSuccessStatusCode)
                {
                    return new TourismNewsResponse
                    {
                        Success = false,
                        Message = "فشل في جلب الأخبار من وزارة السياحة"
                    };
                }

                var externalNews = await response.Content.ReadFromJsonAsync<List<ExternalNewsResponse>>();

                if (externalNews == null || !externalNews.Any())
                {
                    return new TourismNewsResponse
                    {
                        Success = true,
                        Message = "لا توجد أخبار حالياً",
                        Articles = new List<TourismNewsItem>(),
                        TotalCount = 0
                    };
                }

                var newsItems = new List<TourismNewsItem>();

                foreach (var news in externalNews)
                {
                    string? imageUrl = null;

                    // إذا كان هناك صورة مميزة، نستخرج رابطها المباشر الفعلي
                    if (news.Links?.WpFeaturedMedia != null && news.Links.WpFeaturedMedia.Any())
                    {
                        var mediaEndpointUrl = news.Links.WpFeaturedMedia.First().Href;
                        imageUrl = await ExtractActualImageUrlAsync(mediaEndpointUrl);
                    }

                    newsItems.Add(new TourismNewsItem
                    {
                        Id = news.Id,
                        Title = news.Title.Rendered,
                        Summary = StripHtmlTags(news.Excerpt.Rendered),
                        Content = news.Content.Rendered,
                        ImageUrl = imageUrl, // الآن هذا هو الرابط المباشر للصورة
                        PublishedDate = news.Date,
                        ModifiedDate = news.Modified,
                        Slug = news.Slug,
                        SourceLink = news.Link,
                        Categories = news.Categories
                    });
                }

                return new TourismNewsResponse
                {
                    Success = true,
                    Message = $"تم جلب {newsItems.Count} خبر بنجاح",
                    Articles = newsItems,
                    TotalCount = newsItems.Count
                };
            }
            catch (Exception ex)
            {
                return new TourismNewsResponse
                {
                    Success = false,
                    Message = $"حدث خطأ أثناء جلب الأخبار: {ex.Message}"
                };
            }
        }

        public async Task<TourismNewsItem?> GetNewsByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/posts/{id}?_embed");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var externalNews = await response.Content.ReadFromJsonAsync<ExternalNewsResponse>();

                if (externalNews == null)
                {
                    return null;
                }

                string? imageUrl = null;
                if (externalNews.Links?.WpFeaturedMedia != null && externalNews.Links.WpFeaturedMedia.Any())
                {
                    var mediaEndpointUrl = externalNews.Links.WpFeaturedMedia.First().Href;
                    imageUrl = await ExtractActualImageUrlAsync(mediaEndpointUrl);
                }

                return new TourismNewsItem
                {
                    Id = externalNews.Id,
                    Title = externalNews.Title.Rendered,
                    Summary = StripHtmlTags(externalNews.Excerpt.Rendered),
                    Content = externalNews.Content.Rendered,
                    ImageUrl = imageUrl,
                    PublishedDate = externalNews.Date,
                    ModifiedDate = externalNews.Modified,
                    Slug = externalNews.Slug,
                    SourceLink = externalNews.Link,
                    Categories = externalNews.Categories
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetNewsImageUrlAsync(int mediaId)
        {
            return await ExtractActualImageUrlAsync($"{_baseUrl}/media/{mediaId}");
        }

        /// <summary>
        /// دالة مساعدة لاستخراج رابط الصورة المباشر الفعلي من رابط الـ Media Endpoint
        /// </summary>
        private async Task<string?> ExtractActualImageUrlAsync(string mediaEndpointUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(mediaEndpointUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var root = jsonDoc.RootElement;

                // الطريقة 1: من guid.rendered (وهو الرابط المباشر للملف المرفوع كما ظهر في الـ Screenshot)
                if (root.TryGetProperty("guid", out var guid) &&
                    guid.TryGetProperty("rendered", out var renderedGuid))
                {
                    var guidUrl = renderedGuid.GetString();
                    if (!string.IsNullOrEmpty(guidUrl))
                    {
                        return guidUrl;
                    }
                }

                // الطريقة 2: من media_details.sizes (الأفضل للأداء، نجيب حجم medium)
                if (root.TryGetProperty("media_details", out var mediaDetails) &&
                    mediaDetails.TryGetProperty("sizes", out var sizes))
                {
                    if (sizes.TryGetProperty("medium", out var mediumSize) &&
                        mediumSize.TryGetProperty("source_url", out var mediumUrl))
                    {
                        return mediumUrl.GetString();
                    }

                    // fallback: أي حجم متاح
                    foreach (var property in sizes.EnumerateObject())
                    {
                        if (property.Value.TryGetProperty("source_url", out var sizeUrl))
                        {
                            return sizeUrl.GetString();
                        }
                    }
                }

                // الطريقة 3: من source_url مباشرة كآخر حل
                if (root.TryGetProperty("source_url", out var fullUrl))
                {
                    return fullUrl.GetString();
                }

                return null;
            }
            catch
            {
                // في حال فشل جلب تفاصيل الصورة، نعود بقيمة فارغة ولا نوقف التطبيق
                return null;
            }
        }

        /// <summary>
        /// دالة لتنظيف وسوم HTML من الملخص
        /// </summary>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // إزالة وسوم HTML
            var withoutTags = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);

            // إزالة الكيانات HTML مثل &nbsp; &hellip;
            withoutTags = System.Text.RegularExpressions.Regex.Replace(withoutTags, "&[a-zA-Z]+;", string.Empty);

            // تنظيف المسافات الزائدة
            return System.Text.RegularExpressions.Regex.Replace(withoutTags, @"\s+", " ").Trim();
        }
    }
}