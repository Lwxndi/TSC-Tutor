using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services
{
    public class AcademicService : IAcademicService
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public AcademicService(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<SubjectListItemViewModel>> GetAllSubjectsAsync()
        {
            return await _context.Subjects
                .Select(s => new SubjectListItemViewModel
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    IsActive = s.IsActive,
                    Grades = s.Grades.Select(g => g.Grade).OrderBy(g => g).ToList()
                })
                .OrderBy(s => s.SubjectName)
                .ToListAsync();
        }

        public async Task<SubjectEditViewModel?> GetSubjectForEditAsync(int subjectId)
        {
            var subject = await _context.Subjects
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return null;

            return new SubjectEditViewModel
            {
                SubjectId = subject.SubjectId,
                SubjectName = subject.SubjectName,
                IsActive = subject.IsActive,
                Grade10 = subject.Grades.Any(g => g.Grade == Grade.Grade10),
                Grade11 = subject.Grades.Any(g => g.Grade == Grade.Grade11),
                Grade12 = subject.Grades.Any(g => g.Grade == Grade.Grade12)
            };
        }

        public async Task<int> CreateSubjectAsync(SubjectEditViewModel model)
        {
            var subject = new Subject
            {
                SubjectName = model.SubjectName,
                IsActive = model.IsActive
            };

            foreach (var grade in SelectedGrades(model))
            {
                subject.Grades.Add(new SubjectGrade { Grade = grade });
            }

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return subject.SubjectId;
        }

        public async Task UpdateSubjectAsync(SubjectEditViewModel model)
        {
            var subject = await _context.Subjects
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.SubjectId == model.SubjectId);

            if (subject == null)
                throw new InvalidOperationException($"Subject {model.SubjectId} not found.");

            subject.SubjectName = model.SubjectName;
            subject.IsActive = model.IsActive;

            var selected = SelectedGrades(model).ToHashSet();
            var current = subject.Grades.Select(g => g.Grade).ToHashSet();

            subject.Grades
                .Where(g => !selected.Contains(g.Grade))
                .ToList()
                .ForEach(g => _context.Remove(g));

            foreach (var grade in selected.Except(current))
            {
                subject.Grades.Add(new SubjectGrade { SubjectId = subject.SubjectId, Grade = grade });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> SetSubjectActiveStatusAsync(int subjectId, bool isActive)
        {
            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null)
                throw new InvalidOperationException($"Subject {subjectId} not found.");

            subject.IsActive = isActive;
            await _context.SaveChangesAsync();

            return subject.SubjectName;
        }

        public async Task<List<Grade>> GetGradesForSubjectAsync(int subjectId)
        {
            return await _context.SubjectGrades
                .Where(sg => sg.SubjectId == subjectId)
                .Select(sg => sg.Grade)
                .ToListAsync();
        }

        public async Task<bool> IsSubjectOfferedForGradeAsync(int subjectId, Grade grade)
        {
            return await _context.SubjectGrades
                .AnyAsync(sg => sg.SubjectId == subjectId && sg.Grade == grade);
        }

        public async Task<List<int>> GetTutorUserIdsForSubjectAsync(int subjectId)
        {
            return await _context.TutorSubjects
                .Where(ts => ts.SubjectId == subjectId)
                .Select(ts => ts.TutorUserId)
                .Distinct()
                .ToListAsync();
        }

        private static IEnumerable<Grade> SelectedGrades(SubjectEditViewModel model)
        {
            if (model.Grade10) yield return Grade.Grade10;
            if (model.Grade11) yield return Grade.Grade11;
            if (model.Grade12) yield return Grade.Grade12;
        }
    }
}