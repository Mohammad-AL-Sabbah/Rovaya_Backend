using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Rovaya.DAL.DTO.Request.TourismGuid;
using Rovaya.DAL.DTO.Response.TourismGuid;
using Rovaya.DAL.Models.TourismGuide;
using Rovaya.DAL.Repository.TourismGuid;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Rovaya.BLL.Service.TourismGuide
{
    public class TourismGuidService : ITourismGuidService
    {
        private readonly ITourismGuidRepository _repository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public TourismGuidService(
            ITourismGuidRepository repository,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _repository = repository;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        // ================= Categories =================

        public async Task<IEnumerable<TourismGuidResponse>> GetAllCategoriesAsync(string langCode)
        {
            var categories = await _repository.GetAllCategoriesAsync(langCode);
            return categories.Select(c => MapToCategoryResponse(c, langCode));
        }

        public async Task<TourismGuidResponse?> GetCategoryByIdAsync(int id, string langCode)
        {
            var category = await _repository.GetCategoryByIdAsync(id, langCode);
            return category == null ? null : MapToCategoryResponse(category, langCode);
        }

        public async Task<TourismGuidResponse> CreateCategoryAsync(TourismGuidRequest request)
        {
            string? imageUrl = await SaveImageAsync(request.Image);
            var translations = new List<TourismGuideCategoryTranslation>();

            if (!string.IsNullOrEmpty(request.TranslationsJson))
            {
                var translationDtos = JsonSerializer.Deserialize<List<TourismTranslationGuidRequest>>(request.TranslationsJson);
                if (translationDtos != null)
                {
                    translations = translationDtos.Select(t => new TourismGuideCategoryTranslation
                    {
                        LanguageCode = t.LanguageCode,
                        Name = t.Name,
                        Description = t.Description
                    }).ToList();
                }
            }

            var categoryEntity = new TourismGuideCategory
            {
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                TourismGuideTranslations = translations
            };

            foreach (var translation in categoryEntity.TourismGuideTranslations)
            {
                translation.TourismGuideCategory = categoryEntity;
            }

            var created = await _repository.CreateCategoryAsync(categoryEntity);
            var firstLang = translations.FirstOrDefault()?.LanguageCode ?? "ar";
            return MapToCategoryResponse(created, firstLang);
        }

        public async Task<bool> UpdateCategoryAsync(int id, TourismGuidRequest request)
        {
            var existingCategory = await _repository.GetCategoryByIdAsync(id, langCode: null);
            if (existingCategory == null) return false;

            existingCategory.ImageUrl = await SaveImageAsync(request.Image, existingCategory.ImageUrl);

            if (!string.IsNullOrEmpty(request.TranslationsJson))
            {
                var translationDtos = JsonSerializer.Deserialize<List<TourismTranslationGuidRequest>>(request.TranslationsJson);
                if (translationDtos != null && translationDtos.Any())
                {
                    foreach (var incomingTranslation in translationDtos)
                    {
                        var existingTranslation = existingCategory.TourismGuideTranslations
                            .FirstOrDefault(t => t.LanguageCode == incomingTranslation.LanguageCode);

                        if (existingTranslation != null)
                        {
                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Name)) existingTranslation.Name = incomingTranslation.Name;
                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Description)) existingTranslation.Description = incomingTranslation.Description;
                        }
                        else
                        {
                            existingCategory.TourismGuideTranslations.Add(new TourismGuideCategoryTranslation
                            {
                                TourismGuideCategoryId = existingCategory.Id,
                                LanguageCode = incomingTranslation.LanguageCode,
                                Name = incomingTranslation.Name ?? string.Empty,
                                Description = incomingTranslation.Description
                            });
                        }
                    }
                }
            }
            return await _repository.UpdateCategoryAsync(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _repository.GetCategoryByIdAsync(id, langCode: null);
            if (category != null && !string.IsNullOrEmpty(category.ImageUrl))
            {
                DeleteImageFromFileSystem(category.ImageUrl);
            }
            return await _repository.DeleteCategoryAsync(id);
        }

        // ================= Places =================

        public async Task<IEnumerable<TourismPlaceResponse>> GetAllPlacesAsync(TourismPlaceSearchRequest searchRequest)
        {
            var places = await _repository.GetAllPlacesAsync(searchRequest.LangCode, searchRequest.CategoryId, searchRequest.SearchName);
            return places.Select(p => MapToPlaceResponse(p, searchRequest.LangCode));
        }

        public async Task<TourismPlaceResponse?> GetPlaceByIdAsync(int id, string langCode)
        {
            var place = await _repository.GetPlaceByIdAsync(id, langCode);
            return place == null ? null : MapToPlaceResponse(place, langCode);
        }

        public async Task<TourismPlaceResponse> CreatePlaceAsync(CreateTourismPlaceRequest request)
        {
            string? imageUrl = await SaveImageAsync(request.Image);
            var translations = new List<TourismGuidePlaceTranslation>();

            if (!string.IsNullOrEmpty(request.TranslationsJson))
            {
                var translationDtos = JsonSerializer.Deserialize<List<TourismPlaceTranslationRequest>>(request.TranslationsJson);
                if (translationDtos != null)
                {
                    translations = translationDtos.Select(t => new TourismGuidePlaceTranslation
                    {
                        LanguageCode = t.LanguageCode,
                        Name = t.Name,
                        Description = t.Description
                    }).ToList();
                }
            }

            var placeEntity = new TourismGuidePlace
            {
                ParentPlaceId = request.ParentPlaceId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                Translations = translations
            };

            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var categories = await _repository.GetCategoriesByIdsAsync(request.CategoryIds);
                placeEntity.Categories = categories;
            }

            foreach (var translation in placeEntity.Translations)
            {
                translation.TourismGuidePlace = placeEntity;
            }

            var created = await _repository.CreatePlaceAsync(placeEntity);
            var firstLang = translations.FirstOrDefault()?.LanguageCode ?? "ar";
            return MapToPlaceResponse(created, firstLang);
        }

        public async Task<bool> UpdatePlaceAsync(UpdateTourismPlaceRequest request)
        {
            var existingPlace = await _repository.GetPlaceByIdAsync(request.Id, langCode: null);
            if (existingPlace == null) return false;

            existingPlace.ImageUrl = await SaveImageAsync(request.Image, existingPlace.ImageUrl);
            existingPlace.ParentPlaceId = request.ParentPlaceId;
            existingPlace.Latitude = request.Latitude;
            existingPlace.Longitude = request.Longitude;

            existingPlace.Categories.Clear();
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var newCategories = await _repository.GetCategoriesByIdsAsync(request.CategoryIds);
                foreach (var cat in newCategories)
                {
                    existingPlace.Categories.Add(cat);
                }
            }

            if (!string.IsNullOrEmpty(request.TranslationsJson))
            {
                var translationDtos = JsonSerializer.Deserialize<List<TourismPlaceTranslationRequest>>(request.TranslationsJson);
                if (translationDtos != null && translationDtos.Any())
                {
                    foreach (var incomingTranslation in translationDtos)
                    {
                        var existingTranslation = existingPlace.Translations
                            .FirstOrDefault(t => t.LanguageCode == incomingTranslation.LanguageCode);

                        if (existingTranslation != null)
                        {
                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Name)) existingTranslation.Name = incomingTranslation.Name;
                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Description)) existingTranslation.Description = incomingTranslation.Description;
                        }
                        else
                        {
                            existingPlace.Translations.Add(new TourismGuidePlaceTranslation
                            {
                                TourismGuidePlaceId = existingPlace.Id,
                                LanguageCode = incomingTranslation.LanguageCode,
                                Name = incomingTranslation.Name ?? string.Empty,
                                Description = incomingTranslation.Description
                            });
                        }
                    }
                }
            }

            return await _repository.UpdatePlaceAsync(existingPlace);
        }

        public async Task<bool> DeletePlaceAsync(int id)
        {
            var place = await _repository.GetPlaceByIdAsync(id, langCode: null);
            if (place != null && !string.IsNullOrEmpty(place.ImageUrl))
            {
                DeleteImageFromFileSystem(place.ImageUrl);
            }
            return await _repository.DeletePlaceAsync(id);
        }

        // ================= City Sections =================

        public async Task<IEnumerable<CitySectionResponse>> GetSectionsByCityIdAsync(int cityId)
        {
            var sections = await _repository.GetSectionsByCityIdAsync(cityId);
            return sections.Select(MapSectionToResponse);
        }

        public async Task<CitySectionResponse?> GetSectionByIdAsync(int id)
        {
            var section = await _repository.GetSectionByIdAsync(id);
            return section == null ? null : MapSectionToResponse(section);
        }

        public async Task<CitySectionResponse> CreateSectionAsync(CreateCitySectionRequest request)
        {
            var images = new List<string>(request.ExistingImages ?? new List<string>());
            var videos = new List<string>(request.ExistingVideos ?? new List<string>());

            // ✅ رفع الصور الجديدة (متعددة)
            if (request.ImageFiles != null)
            {
                foreach (var file in request.ImageFiles)
                {
                    var imageUrl = await SaveImageAsync(file);
                    if (imageUrl != null) images.Add(imageUrl);
                }
            }

            // ✅ رفع الفيديوهات الجديدة (متعددة)
            if (request.VideoFiles != null)
            {
                foreach (var file in request.VideoFiles)
                {
                    var videoUrl = await SaveVideoAsync(file);
                    if (videoUrl != null) videos.Add(videoUrl);
                }
            }

            var section = new CitySection
            {
                CityId = request.CityId,
                Title = request.Title,
                TitleEn = request.TitleEn,
                Content = request.Content,
                ContentEn = request.ContentEn,
                Order = request.Order,
                ImagesJson = images.Count > 0 ? JsonSerializer.Serialize(images) : null,
                VideosJson = videos.Count > 0 ? JsonSerializer.Serialize(videos) : null
            };
            var created = await _repository.CreateSectionAsync(section);
            return MapSectionToResponse(created);
        }

        public async Task<bool> UpdateSectionAsync(UpdateCitySectionRequest request)
        {
            var existing = await _repository.GetSectionByIdAsync(request.Id);
            if (existing == null) return false;

            var images = new List<string>(request.ExistingImages ?? new List<string>());
            var videos = new List<string>(request.ExistingVideos ?? new List<string>());

            // ✅ رفع الصور الجديدة (متعددة)
            if (request.ImageFiles != null)
            {
                foreach (var file in request.ImageFiles)
                {
                    var imageUrl = await SaveImageAsync(file);
                    if (imageUrl != null) images.Add(imageUrl);
                }
            }

            // ✅ رفع الفيديوهات الجديدة (متعددة)
            if (request.VideoFiles != null)
            {
                foreach (var file in request.VideoFiles)
                {
                    var videoUrl = await SaveVideoAsync(file);
                    if (videoUrl != null) videos.Add(videoUrl);
                }
            }

            existing.Title = request.Title;
            existing.TitleEn = request.TitleEn;
            existing.Content = request.Content;
            existing.ContentEn = request.ContentEn;
            existing.Order = request.Order;
            existing.ImagesJson = images.Count > 0 ? JsonSerializer.Serialize(images) : null;
            existing.VideosJson = videos.Count > 0 ? JsonSerializer.Serialize(videos) : null;

            await _repository.UpdateSectionAsync(existing);
            return true;
        }

        public async Task<bool> DeleteSectionAsync(int id)
        {
            return await _repository.DeleteSectionAsync(id);
        }

        // ================= Cities =================

        public async Task<IEnumerable<TourismPlaceResponse>> GetAllCitiesAsync(string langCode, int? categoryId, string? searchName)
        {
            var cities = await _repository.GetAllCitiesAsync(langCode, categoryId, searchName);
            return cities.Select(c => MapToPlaceResponse(c, langCode));
        }

        public async Task<CityDetailsResponse?> GetCityDetailsAsync(int id, string langCode)
        {
            var city = await _repository.GetPlaceByIdAsync(id, langCode);
            if (city == null) return null;

            var sections = await _repository.GetSectionsByCityIdAsync(id);
            var sectionResponses = sections.Select(MapSectionToResponse).ToList();

            var places = await _repository.GetPlacesByParentIdAsync(id, langCode);
            var placeResponses = places.Select(p => MapToPlaceResponse(p, langCode)).ToList();

            return new CityDetailsResponse
            {
                Id = city.Id,
                Name = city.Translations.FirstOrDefault(t => t.LanguageCode == langCode)?.Name ?? string.Empty,
                Description = city.Translations.FirstOrDefault(t => t.LanguageCode == langCode)?.Description ?? string.Empty,
                LanguageCode = langCode,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                ImageUrl = GetFullImageUrl(city.ImageUrl),
                Sections = sectionResponses.OrderBy(s => s.Order).ToList(),
                Places = placeResponses
            };
        }

        // ================= Helpers (Images & Videos & Mapping) =================

        private async Task<string?> SaveImageAsync(IFormFile? file, string? oldImageUrl = null)
        {
            if (file == null || file.Length == 0) return oldImageUrl;

            if (!string.IsNullOrEmpty(oldImageUrl) && !oldImageUrl.StartsWith("http"))
            {
                DeleteImageFromFileSystem(oldImageUrl);
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tourism");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/tourism/{uniqueFileName}";
        }

        // دالة حفظ الفيديو
        private async Task<string?> SaveVideoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "videos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/videos/{uniqueFileName}";
        }

        private void DeleteImageFromFileSystem(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http")) return;
            var fileName = imageUrl.Replace("/uploads/tourism/", "");
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tourism", fileName);
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }

        private string GetFullImageUrl(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return string.Empty;
            if (relativeUrl.StartsWith("http://") || relativeUrl.StartsWith("https://")) return relativeUrl;

            var appUrl = _configuration["AppUrl"];
            if (!appUrl.StartsWith("http://") && !appUrl.StartsWith("https://")) appUrl = "https://" + appUrl;

            return $"{appUrl}{relativeUrl}";
        }

        private TourismGuidResponse MapToCategoryResponse(TourismGuideCategory category, string langCode)
        {
            var translation = category.TourismGuideTranslations.FirstOrDefault(t => t.LanguageCode == langCode)
                              ?? category.TourismGuideTranslations.FirstOrDefault();

            return new TourismGuidResponse
            {
                Id = category.Id,
                Name = translation?.Name ?? string.Empty,
                Description = translation?.Description ?? string.Empty,
                LanguageCode = translation?.LanguageCode ?? langCode,
                ImageUrl = GetFullImageUrl(category.ImageUrl)
            };
        }

        private TourismPlaceResponse MapToPlaceResponse(TourismGuidePlace place, string langCode)
        {
            var translation = place.Translations.FirstOrDefault(t => t.LanguageCode == langCode)
                              ?? place.Translations.FirstOrDefault();

            var categoryIds = new List<int>();
            var categoryNames = new List<string>();

            if (place.Categories != null)
            {
                foreach (var category in place.Categories)
                {
                    categoryIds.Add(category.Id);

                    var catTranslation = category.TourismGuideTranslations?
                        .FirstOrDefault(t => t.LanguageCode == langCode)
                        ?? category.TourismGuideTranslations?.FirstOrDefault();

                    if (catTranslation != null && !string.IsNullOrEmpty(catTranslation.Name))
                    {
                        categoryNames.Add(catTranslation.Name);
                    }
                }
            }

            return new TourismPlaceResponse
            {
                Id = place.Id,
                CategoryIds = categoryIds,
                CategoryNames = categoryNames,
                ParentPlaceId = place.ParentPlaceId,
                ParentPlaceName = place.ParentPlace?.Translations
                    .FirstOrDefault(t => t.LanguageCode == langCode)?.Name,
                Name = translation?.Name ?? string.Empty,
                Description = translation?.Description ?? string.Empty,
                LanguageCode = translation?.LanguageCode ?? langCode,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                ImageUrl = GetFullImageUrl(place.ImageUrl),
                SubPlaces = place.SubPlaces?.Select(sp => MapToPlaceResponse(sp, langCode)).ToList() ?? new List<TourismPlaceResponse>()
            };
        }

        private CitySectionResponse MapSectionToResponse(CitySection section)
        {
            var images = string.IsNullOrEmpty(section.ImagesJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(section.ImagesJson) ?? new List<string>();

            var videos = string.IsNullOrEmpty(section.VideosJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(section.VideosJson) ?? new List<string>();

            // ✅ تحويل مسارات الصور والفيديو إلى روابط كاملة
            var fullImageUrls = images.Select(img => GetFullImageUrl(img)).ToList();
            var fullVideoUrls = videos.Select(vid => GetFullImageUrl(vid)).ToList();

            return new CitySectionResponse
            {
                Id = section.Id,
                CityId = section.CityId,
                Title = section.Title,
                TitleEn = section.TitleEn,
                Content = section.Content,
                ContentEn = section.ContentEn,
                Order = section.Order,
                Images = fullImageUrls,  // ✅ روابط كاملة
                Videos = fullVideoUrls,  // ✅ روابط كاملة
                CreatedAt = section.CreatedAt
            };
        }
    }
}