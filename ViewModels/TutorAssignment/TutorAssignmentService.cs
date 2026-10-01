using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels.TutorAssignment;

namespace Tutor_Manager.Services
{
    public class TutorAssignmentService : ITutorAssignmentService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IAiRecommendationService _aiService;

        public TutorAssignmentService(Tutor_ManagerDatabaseContext context, IAiRecommendationService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        public async Task<TutorAssignmentIndexViewModel> GetGroupedTutorsAsync()
        {
            var tutors = await _context.Tutors
                .Include(t => t.User)
                .Include(t => t.SubjectsTaught).ThenInclude(ts => ts.Subject)
                .Include(t => t.Unavailability)
                .ToListAsync();

            var result = new TutorAssignmentIndexViewModel();
            var today = DateTime.Today;

            foreach (var t in tutors)
            {
                var item = new TutorAssignmentListItem
                {
                    TutorUserId = t.UserId,
                    FullName = $"{t.User.FirstName} {t.User.LastName}",
                    CurrentAssignments = t.SubjectsTaught
                        .GroupBy(ts => ts.Subject.SubjectName)
                        .Select(g => $"{g.Key} ({string.Join(", ", g.Select(x => (int)x.GradeLevel))})")
                        .ToList(),
                    HasRecommendation = await _context.AiRecommendations.AnyAsync(r => r.TutorUserId == t.UserId)
                };

                var onLeave = t.Unavailability.Any(u => today >= u.StartDate.Date && today <= u.EndDate.Date);

                if (!t.IsActive)
                {
                    item.Status = TutorAssignmentStatus.Inactive;
                    result.Inactive.Add(item);
                }
                else if (onLeave)
                {
                    item.Status = TutorAssignmentStatus.OnLeave;
                    result.OnLeave.Add(item);
                }
                else if (!t.SubjectsTaught.Any())
                {
                    item.Status = TutorAssignmentStatus.Unassigned;
                    result.Unassigned.Add(item);
                }
                else
                {
                    item.Status = TutorAssignmentStatus.Assigned;
                    result.Assigned.Add(item);
                }
            }

            return result;
        }

        public async Task<ManageTutorViewModel?> GetManageTutorAsync(int tutorUserId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .Include(t => t.SubjectsTaught).ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            if (tutor == null) return null;

            return new ManageTutorViewModel
            {
                TutorUserId = tutor.UserId,
                FullName = $"{tutor.User.FirstName} {tutor.User.LastName}",
                Qualification = tutor.Qualification,
                Bio = tutor.Bio,
                TutorNumber = tutor.TutorNumber,
                IsActive = tutor.IsActive,
                CurrentAssignments = tutor.SubjectsTaught
                    .Select(ts => $"{ts.Subject.SubjectName} (Grade {(int)ts.GradeLevel})")
                    .ToList(),
                LatestRecommendation = await _aiService.GetLatestRecommendationAsync(tutorUserId)
            };
        }

        public async Task<(bool Success, string? Error)> AssignAsync(int tutorUserId, int subjectId, Grade grade, int assignedByUserId)
        {
            var existing = await _context.TutorSubjects
                .Where(ts => ts.TutorUserId == tutorUserId)
                .ToListAsync();

            var alreadyThisCombo = existing.Any(ts => ts.SubjectId == subjectId && ts.GradeLevel == grade);
            if (alreadyThisCombo)
                return (false, "This subject/grade is already assigned.");

            var distinctSubjects = existing.Select(ts => ts.SubjectId).Distinct().ToHashSet();
            distinctSubjects.Add(subjectId);
            if (distinctSubjects.Count > 3)
                return (false, "Tutor cannot be assigned more than 3 subjects.");

            var distinctGrades = existing.Select(ts => ts.GradeLevel).Distinct().ToHashSet();
            distinctGrades.Add(grade);
            if (distinctGrades.Count > 3)
                return (false, "Tutor cannot be assigned more than 3 unique grades.");

            var subjectGradeOffered = await _context.SubjectGrades
                .AnyAsync(sg => sg.SubjectId == subjectId && sg.Grade == grade);
            if (!subjectGradeOffered)
                return (false, "TSC does not offer this subject at this grade.");

            _context.TutorSubjects.Add(new TutorSubject
            {
                TutorUserId = tutorUserId,
                SubjectId = subjectId,
                GradeLevel = grade,
                AssignedByUserId = assignedByUserId,
                AssignedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Warning)> CheckRemovalImpactAsync(int tutorUserId, int subjectId, Grade grade)
        {
            var inUse = await _context.Offerings.AnyAsync(o =>
                o.TutorUserId == tutorUserId && o.SubjectId == subjectId && o.Grade == grade && o.IsActive);

            return inUse
                ? (false, "This assignment is used by an active Offering. Removing it may affect scheduled classes.")
                : (true, null);
        }
    }
}