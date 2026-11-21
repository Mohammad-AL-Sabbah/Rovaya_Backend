
public interface ITripPlannerService
{
    Task<object> GenerateTripAsync(GenerateTripRequest request, string userId);
   

    Task<bool> SaveTripAsync(string userId, object tripPlan);

    Task<List<UserTrip>> GetUserTripsAsync(string userId);
    Task<object> ModifyTripAsync(ModifyTripRequest request);
    Task<UserTrip?> GetActiveTripAsync(string userId);
    Task<PlaceDetailsResponse> GetPlaceDetailsAsync(
        string placeName, string? placeNameEn = null, string? city = null, string? description = null);

    Task<bool> ActivateTripAsync(string userId, int tripId);
    Task<bool> ArchiveTripAsync(string userId, int tripId);
    Task<bool> DeleteTripAsync(string userId, int tripId);
    Task<UpdateTripResult> UpdateTripAsync(string userId, int tripId, UpdateTripRequest request);
}