using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services
{
    public interface IAcademicService
    {
        Task<List<SubjectListItemViewModel>> GetAllSubjectsAsync();
        Task<SubjectEditViewModel?> GetSubjectForEditAsync(int subjectId);
        Task<int> CreateSubjectAsync(SubjectEditViewModel model);
        Task UpdateSubjectAsync(SubjectEditViewModel model);
        Task<string> SetSubjectActiveStatusAsync(int subjectId, bool isActive);

        Task<List<Grade>> GetGradesForSubjectAsync(int subjectId);
        Task<bool> IsSubjectOfferedForGradeAsync(int subjectId, Grade grade);
        Task<List<int>> GetTutorUserIdsForSubjectAsync(int subjectId);
    }
}