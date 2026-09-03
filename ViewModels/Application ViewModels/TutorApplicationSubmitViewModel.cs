using System.ComponentModel.DataAnnotations;

namespace Tutor_Manager.ViewModels.TutorApplication
{
    public class TutorApplicationSubmitViewModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required(ErrorMessage = "You must consent to the processing of your information to submit.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must consent to the processing of your information to submit.")]
        public bool ConsentGiven { get; set; }

        public string ConsentVersion { get; set; } = "1.0";
    }
}