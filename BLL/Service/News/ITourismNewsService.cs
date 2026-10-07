using Rovaya.DAL.DTO.Response.News;

namespace Rovaya.BLL.Service.External
{
    public interface ITourismNewsService
    {
        Task<TourismNewsResponse> GetLatestNewsAsync(int count = 10);
        Task<TourismNewsItem?> GetNewsByIdAsync(int id);
        Task<string?> GetNewsImageUrlAsync(int mediaId);
    }
}