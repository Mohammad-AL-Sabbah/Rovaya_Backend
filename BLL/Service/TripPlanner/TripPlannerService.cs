using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rovaya.DAL.Data;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;


public class TripPlannerService : ITripPlannerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context; // غيّر الاسم حسب مشروعك
    private readonly IConfiguration _configuration;

    public TripPlannerService(IHttpClientFactory httpClientFactory, ApplicationDbContext context, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _configuration = configuration;
    }

    // 1. توليد خطة جديدة
    public async Task<object> GenerateTripAsync(GenerateTripRequest request, string userId)
    {
        try
        {
            Console.WriteLine("🔄 [TripPlannerService] جاري الاتصال بـ Python API على Render...");

            var client = _httpClientFactory.CreateClient("PythonTripPlanner");

            // ✅ استخدام StringContent مع تحديد Content-Type صراحةً
            var jsonRequest = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 إرسال الطلب إلى: {client.BaseAddress}/api/generate-trip");
            Console.WriteLine($"📋 البيانات المرسلة: {jsonRequest}");

            var response = await client.PostAsync("/api/generate-trip", content);

            Console.WriteLine($"📥 استجابة من Render: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ خطأ من Render: {errorContent}");
                throw new Exception($"فشل في الاتصال بخدمة الذكاء الاصطناعي: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Console.WriteLine("✅ تم استلام البيانات بنجاح من Render");

            // حفظ الخطة تلقائياً
            if (result.TryGetProperty("success", out var success) && success.GetBoolean())
            {
                Console.WriteLine(" جاري حفظ الرحلة في قاعدة البيانات...");
                var plan = result.GetProperty("plan");
                await SaveTripAsync(userId, plan);
                Console.WriteLine("✅ تم حفظ الرحلة بنجاح");
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" استثناء في GenerateTripAsync: {ex.Message}");
            Console.WriteLine($" التفاصيل: {ex.InnerException?.Message}");
            throw;
        }
    }




    // 2. تعديل خطة موجودة
    public async Task<object> ModifyTripAsync(ModifyTripRequest request)
    {
        try
        {
            Console.WriteLine("🔄 [TripPlannerService] جاري تعديل الخطة...");

            var client = _httpClientFactory.CreateClient("PythonTripPlanner");

            var jsonRequest = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Console.WriteLine($"📤 إرسال طلب التعديل إلى: {client.BaseAddress}/api/modify-trip");

            var response = await client.PostAsync("/api/modify-trip", content);

            Console.WriteLine($"📥 استجابة من Render: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ خطأ من Render: {errorContent}");
                throw new Exception($"فشل في تعديل الخطة: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            Console.WriteLine("✅ تم تعديل الخطة بنجاح");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 استثناء في ModifyTripAsync: {ex.Message}");
            throw;
        }
    }
    // 3. ميزة "اعرف أكثر" (توليد ذكي عند الطلب)
    // 3. ميزة "اعرف أكثر" (توليد ذكي عند الطلب)
    // 3. ميزة "اعرف أكثر" (توليد ذكي عند الطلب مع الصورة)
    public async Task<PlaceDetailsResponse> GetPlaceDetailsAsync(
       string placeName, string? placeNameEn = null, string? city = null, string? description = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PythonTripPlanner");

            var requestBody = new
            {
                place_name = placeName,
                place_name_en = placeNameEn,
                city = city,
                description = description
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/place-details", content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ خطأ من Render: {err}");
                throw new Exception($"فشل في الحصول على تفاصيل المكان: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<PlaceDetailsResponse>();
            if (result != null && result.Success) return result;

            throw new Exception("لم يتم العثور على تفاصيل المكان");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 استثناء في GetPlaceDetailsAsync: {ex.Message}");
            throw;
        }
    }





    public async Task<bool> SaveTripAsync(string userId, object tripPlan)
    {
        try
        {
            var jsonElement = (JsonElement)tripPlan;

            var cleanJson = System.Text.Json.JsonSerializer.Serialize(jsonElement, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            });

            var trip = new UserTrip
            {
                UserId = userId,
                TripTitle = jsonElement.TryGetProperty("trip_title", out var title) ? title.GetString()! : "رحلة غير معنونة",
                TotalBudget = jsonElement.TryGetProperty("total_estimated_cost", out var cost) ? cost.GetDecimal() : 0,
                ItineraryJson = cleanJson,
                StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Status = TripStatus.Archived  // ✅ كل خطة جديدة تُحفظ في الأرشيف تلقائياً
            };

            _context.UserTrips.Add(trip);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ تم حفظ الخطة في الأرشيف");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" خطأ في حفظ الرحلة: {ex.Message}");
            return false;
        }
    }


    // 6. جلب الخطة النشطة فقط
    public async Task<UserTrip?> GetActiveTripAsync(string userId)
    {
        return await _context.UserTrips
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Status == TripStatus.Active);
    }

    // 7. تفعيل خطة معينة (وجعل الباقي Drafts)
    // 7. تفعيل خطة معينة (وجعل الباقي في الأرشيف)
    public async Task<bool> ActivateTripAsync(string userId, int tripId)
    {
        try
        {
            // أولاً: جعل كل الخطط النشطة للمستخدم تعود للأرشيف
            var currentActiveTrips = await _context.UserTrips
                .Where(t => t.UserId == userId && t.Status == TripStatus.Active)
                .ToListAsync();

            foreach (var trip in currentActiveTrips)
            {
                trip.Status = TripStatus.Archived;  // ✅ العودة للأرشيف
            }

            // ثانياً: تفعيل الخطة المطلوبة
            var tripToActivate = await _context.UserTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (tripToActivate == null)
            {
                Console.WriteLine($"❌ الخطة {tripId} غير موجودة");
                return false;
            }

            tripToActivate.Status = TripStatus.Active;  // ✅ هذه تصبح نشطة
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ تم تفعيل الخطة {tripId} وإرجاع الباقي للأرشيف");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ خطأ في تفعيل الخطة: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ArchiveTripAsync(string userId, int tripId)
    {
        try
        {
            var trip = await _context.UserTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
            {
                return false;
            }

            trip.Status = TripStatus.Archived;

            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ تم أرشفة الخطة {tripId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ خطأ في أرشفة الخطة: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteTripAsync(string userId, int tripId)
    {
        try
        {
            var trip = await _context.UserTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);

            if (trip == null)
            {
                return false;
            }

            _context.UserTrips.Remove(trip);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ تم حذف الخطة {tripId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ خطأ في حذف الخطة: {ex.Message}");
            return false;
        }
    }


    public async Task<List<UserTrip>> GetUserTripsAsync(string userId)
    {
        return await _context.UserTrips
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}