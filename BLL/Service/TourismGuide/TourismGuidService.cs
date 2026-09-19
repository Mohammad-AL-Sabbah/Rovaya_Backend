using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Rovaya.DAL.DTO.Request.TourismGuid;
using Rovaya.DAL.DTO.Response.TourismGuid;
using Rovaya.DAL.Models.TourismGuide;
using Rovaya.DAL.Repository.TourismGuid;

namespace Rovaya.BLL.Service.TourismGuide
{
    public class TourismGuidService : ITourismGuidService
    {
        private readonly ITourismGuidRepository _repository;

        public TourismGuidService(ITourismGuidRepository repository)
        {
            _repository = repository;
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
            string? imageUrl = null;
            if (request.Image != null)
            {
                imageUrl = await SaveImageAsync(request.Image);
            }

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
            if (existingCategory == null)
                return false;

            if (request.Image != null)
            {
                string? imageUrl = await SaveImageAsync(request.Image);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    existingCategory.ImageUrl = imageUrl;
                }
            }

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
                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Name))
                                existingTranslation.Name = incomingTranslation.Name;

                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Description))
                                existingTranslation.Description = incomingTranslation.Description;
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
            string? imageUrl = null;
            if (request.Image != null)
            {
                imageUrl = await SaveImageAsync(request.Image);
            }

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
                CategoryId = request.CategoryId,
                ParentPlaceId = request.ParentPlaceId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                Translations = translations
            };

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
            if (existingPlace == null)
                return false;

            if (request.Image != null)
            {
                string? imageUrl = await SaveImageAsync(request.Image);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    existingPlace.ImageUrl = imageUrl;
                }
            }

            if (request.CategoryId > 0)
                existingPlace.CategoryId = request.CategoryId;

            existingPlace.ParentPlaceId = request.ParentPlaceId;

            if (request.Latitude != 0)
                existingPlace.Latitude = request.Latitude;

            if (request.Longitude != 0)
                existingPlace.Longitude = request.Longitude;

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
                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Name))
                                existingTranslation.Name = incomingTranslation.Name;

                            if (!string.IsNullOrWhiteSpace(incomingTranslation.Description))
                                existingTranslation.Description = incomingTranslation.Description;
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
            var section = new CitySection
            {
                CityId = request.CityId,
                Title = request.Title,
                Content = request.Content,
                Order = request.Order,
                ImagesJson = request.Images != null ? JsonSerializer.Serialize(request.Images) : null,
                VideosJson = request.Videos != null ? JsonSerializer.Serialize(request.Videos) : null
            };

            var created = await _repository.CreateSectionAsync(section);
            return MapSectionToResponse(created);
        }

        public async Task<bool> UpdateSectionAsync(UpdateCitySectionRequest request)
        {
            var section = new CitySection
            {
                Id = request.Id,
                Title = request.Title,
                Content = request.Content,
                Order = request.Order,
                ImagesJson = request.Images != null ? JsonSerializer.Serialize(request.Images) : null,
                VideosJson = request.Videos != null ? JsonSerializer.Serialize(request.Videos) : null
            };

            return await _repository.UpdateSectionAsync(section);
        }

        public async Task<bool> DeleteSectionAsync(int id)
        {
            return await _repository.DeleteSectionAsync(id);
        }

        // ================= File Upload =================

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tourism");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string extension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}{extension}";
            string fullPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/tourism/{uniqueFileName}";
        }

        // ================= Mappers =================

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
                ImageUrl = category.ImageUrl
            };
        }

        private TourismPlaceResponse MapToPlaceResponse(TourismGuidePlace place, string langCode)
        {
            var translation = place.Translations.FirstOrDefault(t => t.LanguageCode == langCode)
                              ?? place.Translations.FirstOrDefault();

            return new TourismPlaceResponse
            {
                Id = place.Id,
                CategoryId = place.CategoryId,
                ParentPlaceId = place.ParentPlaceId,
                Name = translation?.Name ?? string.Empty,
                Description = translation?.Description ?? string.Empty,
                LanguageCode = translation?.LanguageCode ?? langCode,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                ImageUrl = place.ImageUrl,
                SubPlaces = place.SubPlaces?.Select(sp => MapToPlaceResponse(sp, langCode)).ToList() ?? new List<TourismPlaceResponse>()
            };
        }

        private CitySectionResponse MapSectionToResponse(CitySection section)
        {
            return new CitySectionResponse
            {
                Id = section.Id,
                CityId = section.CityId,
                Title = section.Title,
                Content = section.Content,
                Order = section.Order,
                Images = string.IsNullOrEmpty(section.ImagesJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(section.ImagesJson) ?? new List<string>(),
                Videos = string.IsNullOrEmpty(section.VideosJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(section.VideosJson) ?? new List<string>(),
                CreatedAt = section.CreatedAt
            };
        }


        // ================= Cities =================

        public async Task<IEnumerable<TourismPlaceResponse>> GetAllCitiesAsync(string langCode, int? categoryId, string? searchName)
        {
            // جلب كل المدن (ParentPlaceId = null)
            var cities = await _repository.GetAllCitiesAsync(langCode, categoryId, searchName);
            return cities.Select(c => MapToPlaceResponse(c, langCode));
        }

        public async Task<CityDetailsResponse?> GetCityDetailsAsync(int id, string langCode)
        {
            var city = await _repository.GetPlaceByIdAsync(id, langCode);
            if (city == null) return null;

            // جلب أقسام المدينة
            var sections = await _repository.GetSectionsByCityIdAsync(id);
            var sectionResponses = sections.Select(MapSectionToResponse).ToList();

            // جلب الأماكن السياحية داخل المدينة
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
                ImageUrl = city.ImageUrl,
                Sections = sectionResponses.OrderBy(s => s.Order).ToList(),
                Places = placeResponses
            };
        }



    }
}