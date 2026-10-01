namespace Tutor_Manager.Models
{
    public class StudyMaterialResult
    {
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
        public int? StudyMaterialId { get; set; }

        public static StudyMaterialResult Success(int id) =>
            new() { Succeeded = true, StudyMaterialId = id };

        public static StudyMaterialResult Failure(string error) =>
            new() { Succeeded = false, ErrorMessage = error };
    }
}