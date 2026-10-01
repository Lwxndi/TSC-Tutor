namespace Tutor_Manager.Models
{
    public class StudyMaterial
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public int TutorUserId { get; set; }
        public User Tutor { get; set; }
        public string TopicChapterLabel { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsActive { get; set; }

        public ICollection<StudyMaterialFile> Files { get; set; }
    }
}
