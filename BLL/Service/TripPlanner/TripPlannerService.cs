using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rovaya.DAL.Data;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;


public class TripPlannerService : ITripPlannerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApplicationDbContext _context; // غيّر الاسم حسب مشروعك
    private readonly IConfiguration _configuration;

    // حدود فلسطين التاريخية التقريبية (نفس حدود التطبيق وملف Python) — تُستخدم في تعديل الرحلة
    private const double MinLat = 29.4, MaxLat = 33.4, MinLng = 34.2, MaxLng = 35.95;

    private static readonly HashSet<string> AllowedActivityTypes =
        new(StringComparer.OrdinalIgnoreCase) { "visit", "food", "activity" };

    private static readonly Regex HmRegex = new(@"^\d{1,2}:\d{2}$", RegexOptions.Compiled);

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


    // ══════════════════════════════════════════════════════════════
    // 8. تعديل رحلة محفوظة (حذف / إضافة أنشطة) — الجديد
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// يطبّق عمليات التعديل على نسخة في الذاكرة، ولا يحفظ إلا إذا نجحت كل العمليات (all-or-nothing).
    /// </summary>
    public async Task<UpdateTripResult> UpdateTripAsync(string userId, int tripId, UpdateTripRequest request)
    {
        try
        {
            if (request?.Operations == null || request.Operations.Count == 0)
                return UpdateTripResult.Fail(400, "لا توجد عمليات تعديل.");
            if (request.Operations.Count > 40)
                return UpdateTripResult.Fail(400, "عدد عمليات التعديل كبير جدًا.");

            // الملكية: الرحلة يجب أن تخص هذا المستخدم
            var trip = await _context.UserTrips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null)
                return UpdateTripResult.Fail(404, "الرحلة غير موجودة.");

            if (JsonNode.Parse(trip.ItineraryJson) is not JsonObject plan || plan["days"] is not JsonArray days)
                return UpdateTripResult.Fail(500, "بيانات الرحلة المخزنة غير صالحة.");

            var people = ResolvePeople(plan, days, request.NumberOfPeople);

            foreach (var op in request.Operations)
            {
                var day = days.OfType<JsonObject>().FirstOrDefault(d => GetInt(d["day_number"]) == op.DayNumber);
                if (day == null)
                    return UpdateTripResult.Fail(400, $"اليوم {op.DayNumber} غير موجود في الرحلة.");

                if (day["activities"] is not JsonArray acts)
                {
                    acts = new JsonArray();
                    day["activities"] = acts;
                }

                switch (op.Type?.Trim().ToLowerInvariant())
                {
                    case "remove_activity":
                        {
                            if (op.ActivityIndex is null || op.ActivityIndex < 0 || op.ActivityIndex >= acts.Count)
                                return UpdateTripResult.Fail(400, $"رقم النشاط غير صالح في اليوم {op.DayNumber}.");
                            acts.RemoveAt(op.ActivityIndex.Value);
                            break;
                        }

                    case "add_activity":
                        {
                            var error = AddActivityToDay(acts, op.Activity);
                            if (error != null) return UpdateTripResult.Fail(400, error);
                            break;
                        }

                    default:
                        return UpdateTripResult.Fail(400, $"نوع العملية غير مدعوم: {op.Type}");
                }
            }

            // إعادة حساب التكاليف بالكود (مثل ملف Python)
            decimal total = 0m;
            foreach (var d in days.OfType<JsonObject>())
            {
                var perPerson = (d["activities"] as JsonArray)?
                    .OfType<JsonObject>()
                    .Sum(a => GetDecimal(a["cost_per_person"])) ?? 0m;

                var dayCost = Math.Round(perPerson * people, 2);
                d["daily_cost"] = dayCost;
                total += dayCost;
            }
            total = Math.Round(total, 2);
            plan["total_estimated_cost"] = total;
            plan["number_of_people"] = people;

            var json = plan.ToJsonString(new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            });

            trip.ItineraryJson = json;
            trip.TotalBudget = total;
            await _context.SaveChangesAsync();

            var element = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
            Console.WriteLine($"✅ تم تعديل الرحلة {tripId} ({request.Operations.Count} عملية)");
            return UpdateTripResult.Ok("تم تحديث الرحلة بنجاح.", element);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"❌ JSON غير صالح في UpdateTripAsync: {ex.Message}");
            return UpdateTripResult.Fail(500, "تعذر قراءة بيانات الرحلة.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ خطأ في UpdateTripAsync: {ex.Message}");
            return UpdateTripResult.Fail(500, "حدث خطأ أثناء تعديل الرحلة.");
        }
    }

    // ───────────────────────── مساعدات تعديل الرحلة ─────────────────────────

    /// <summary>يضيف نشاطًا لنهاية اليوم (أو حسب start_time) ويرجع رسالة خطأ أو null.</summary>
    private static string? AddActivityToDay(JsonArray acts, NewActivityDto? a)
    {
        if (a == null) return "بيانات النشاط الجديد مفقودة.";

        var name = (a.PlaceName ?? "").Trim();
        if (name.Length == 0) return "اسم المكان مطلوب.";
        if (name.Length > 120) return "اسم المكان طويل جدًا.";

        if (double.IsNaN(a.Latitude) || double.IsNaN(a.Longitude) ||
            a.Latitude < MinLat || a.Latitude > MaxLat || a.Longitude < MinLng || a.Longitude > MaxLng)
            return "موقع المكان خارج حدود فلسطين أو غير صالح.";

        var cost = a.CostPerPerson ?? 0m;
        if (cost < 0 || cost > 10000) return "تكلفة غير صالحة.";

        var type = AllowedActivityTypes.Contains(a.ActivityType ?? "") ? a.ActivityType!.ToLowerInvariant() : "visit";
        var duration = Math.Clamp(a.DurationMinutes ?? 60, 15, 480);

        // وقت البداية: المطلوب، أو بعد آخر نشاط بـ 30 دقيقة، أو 09:00 لو اليوم فاضي
        TimeSpan? requestedStart = null;
        if (!string.IsNullOrWhiteSpace(a.StartTime))
        {
            requestedStart = ParseHm(a.StartTime);
            if (requestedStart == null) return "صيغة وقت البدء غير صالحة (HH:mm).";
        }

        TimeSpan? lastEnd = null;
        foreach (var existing in acts.OfType<JsonObject>())
        {
            var e = ParseHm(GetString(existing["end_time"]));
            if (e != null && (lastEnd == null || e > lastEnd)) lastEnd = e;
        }

        var start = requestedStart
                    ?? (lastEnd != null ? lastEnd.Value + TimeSpan.FromMinutes(30) : TimeSpan.FromHours(9));
        var end = start + TimeSpan.FromMinutes(duration);

        if (end >= TimeSpan.FromHours(24))
            return "لا يتسع اليوم لهذا النشاط. اختر يومًا آخر أو احذف نشاطًا.";

        var nameEn = string.IsNullOrWhiteSpace(a.PlaceNameEn) ? name : a.PlaceNameEn!.Trim();
        var desc = ClipText(a.Description, 500);
        var descEn = ClipText(a.DescriptionEn, 500);

        var node = new JsonObject
        {
            ["time"] = start.ToString(@"hh\:mm"),
            ["end_time"] = end.ToString(@"hh\:mm"),
            ["place_id"] = null,
            ["place_name"] = name,
            ["place_name_en"] = nameEn,
            ["latitude"] = a.Latitude,
            ["longitude"] = a.Longitude,
            ["activity_type"] = type,
            ["description"] = string.IsNullOrEmpty(desc) ? "مكان أضفته بنفسك إلى الرحلة." : desc,
            ["description_en"] = string.IsNullOrEmpty(descEn) ? "A place you added to the trip." : descEn,
            ["cost_per_person"] = cost,
            ["tips"] = ClipText(a.Tips, 300),
            ["tips_en"] = ClipText(a.TipsEn, 300),
            ["is_from_dataset"] = false,
            ["is_custom"] = true
        };

        acts.Add(node);

        // ترتيب زمني ثابت (الأنشطة بدون وقت صالح تذهب للنهاية)
        var sorted = acts.OfType<JsonObject>()
            .OrderBy(x => ParseHm(GetString(x["time"])) ?? TimeSpan.MaxValue)
            .ToList();
        acts.Clear();
        foreach (var x in sorted) acts.Add(x);

        return null;
    }

    /// <summary>عدد الأشخاص: من الطلب، أو من الخطة، أو يُستنتج من daily_cost ÷ مجموع التكاليف.</summary>
    private static int ResolvePeople(JsonObject plan, JsonArray days, int? requested)
    {
        if (requested is > 0) return Math.Min(requested.Value, 50);

        var stored = GetInt(plan["number_of_people"]);
        if (stored is > 0) return stored.Value;

        foreach (var d in days.OfType<JsonObject>())
        {
            var perPerson = (d["activities"] as JsonArray)?
                .OfType<JsonObject>()
                .Sum(a => GetDecimal(a["cost_per_person"])) ?? 0m;
            var daily = GetDecimal(d["daily_cost"]);
            if (perPerson > 0 && daily > 0)
                return Math.Max((int)Math.Round(daily / perPerson), 1);
        }
        return 1;
    }

    private static TimeSpan? ParseHm(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || !HmRegex.IsMatch(s.Trim())) return null;
        return TimeSpan.TryParse(s.Trim(), CultureInfo.InvariantCulture, out var t) && t < TimeSpan.FromHours(24)
            ? t
            : null;
    }

    private static string ClipText(string? s, int max)
    {
        var t = (s ?? "").Trim();
        return t.Length > max ? t[..max] : t;
    }

    private static string GetString(JsonNode? n) =>
        n is JsonValue v && v.TryGetValue<string>(out var s) ? s ?? "" : "";

    private static int? GetInt(JsonNode? n) =>
        n is JsonValue v && v.TryGetValue<int>(out var i) ? i : null;

    private static decimal GetDecimal(JsonNode? n) =>
        n is JsonValue v && v.TryGetValue<decimal>(out var d) ? d : 0m;
}