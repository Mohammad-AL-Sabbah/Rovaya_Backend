namespace Rovaya.DAL.DTO.Response.Profile
{
    public class MedicalProfileResponse
    {
        public int Id { get; set; }
        public string? ChronicDiseases { get; set; }
        public string? CurrentMedications { get; set; }
        public string? Allergies { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}