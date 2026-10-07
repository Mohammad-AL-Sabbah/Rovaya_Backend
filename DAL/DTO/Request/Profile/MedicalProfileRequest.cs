using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Profile
{
    public class MedicalProfileRequest
    {
        [MaxLength(500, ErrorMessage = "الأمراض المزمنة لا يمكن أن تتجاوز 500 حرف")]
        public string? ChronicDiseases { get; set; }

        [MaxLength(500, ErrorMessage = "الأدوية الحالية لا يمكن أن تتجاوز 500 حرف")]
        public string? CurrentMedications { get; set; }

        [MaxLength(500, ErrorMessage = "الحساسية لا يمكن أن تتجاوز 500 حرف")]
        public string? Allergies { get; set; }
    }
}