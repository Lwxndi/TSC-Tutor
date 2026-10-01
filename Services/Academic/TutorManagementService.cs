using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services
{
    public class TutorManagementService : ITutorManagementService
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public TutorManagementService(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<TutorListItemViewModel>> GetAllTutorsAsync()
        {
            var tutors = await _context.Tutors
                .Include(t => t.User)
                .Include(t => t.SubjectsTaught)
                    .ThenInclude(ts => ts.Subject)
                .ToListAsync();

            return tutors.Select(t => new TutorListItemViewModel
            {
                TutorUserId = t.UserId,
                FullName = $"{t.User.FirstName} {t.User.LastName}",
                TutorNumber = t.TutorNumber,
                IsActive = t.IsActive,
                SubjectGradeSummary = t.SubjectsTaught
                    .GroupBy(ts => ts.Subject.SubjectName)
                    .Select(g => $"{g.Key} ({string.Join(", ", g.Select(x => (int)x.GradeLevel).OrderBy(x => x))})")
                    .ToList()
            }).OrderBy(t => t.FullName).ToList();
        }

        public async Task<TutorSubjectAssignmentViewModel?> GetSubjectAssignmentsAsync(int tutorUserId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .Include(t => t.SubjectsTaught)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            if (tutor == null) return null;

            var allSubjects = await _context.Subjects
                .Include(s => s.Grades)
                .Where(s => s.IsActive)
                .OrderBy(s => s.SubjectName)
                .ToListAsync();

            return new TutorSubjectAssignmentViewModel
            {
                TutorUserId = tutor.UserId,
                TutorName = $"{tutor.User.FirstName} {tutor.User.LastName}",
                Subjects = allSubjects.Select(s => new SubjectAssignmentRow
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    OfferedGrades = s.Grades.Select(g => g.Grade).OrderBy(g => g).ToList(),
                    AssignedGrades = tutor.SubjectsTaught
                        .Where(ts => ts.SubjectId == s.SubjectId)
                        .Select(ts => ts.GradeLevel)
                        .ToList()
                }).ToList()
            };
        }

        public async Task<(bool Success, string? Error)> ToggleSubjectAssignmentAsync(TutorSubjectToggleViewModel model)
        {
            if (model.Assign)
            {
                // Business rule: can't assign a subject/grade TSC doesn't actually offer
                var isOffered = await _context.SubjectGrades
                    .AnyAsync(sg => sg.SubjectId == model.SubjectId && sg.Grade == model.Grade);

                if (!isOffered)
                    return (false, "TSC does not offer this subject at this grade.");

                var alreadyAssigned = await _context.TutorSubjects.AnyAsync(ts =>
                    ts.TutorUserId == model.TutorUserId &&
                    ts.SubjectId == model.SubjectId &&
                    ts.GradeLevel == model.Grade);

                if (!alreadyAssigned)
                {
                    _context.TutorSubjects.Add(new TutorSubject
                    {
                        TutorUserId = model.TutorUserId,
                        SubjectId = model.SubjectId,
                        GradeLevel = model.Grade
                    });
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var existing = await _context.TutorSubjects.FirstOrDefaultAsync(ts =>
                    ts.TutorUserId == model.TutorUserId &&
                    ts.SubjectId == model.SubjectId &&
                    ts.GradeLevel == model.Grade);

                if (existing != null)
                {
                    _context.TutorSubjects.Remove(existing);
                    await _context.SaveChangesAsync();
                }
            }

            return (true, null);
        }

        public async Task<TutorAvailabilityViewModel?> GetAvailabilityAsync(int tutorUserId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .Include(t => t.Availability)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            if (tutor == null) return null;

            return new TutorAvailabilityViewModel
            {
                TutorUserId = tutor.UserId,
                TutorName = $"{tutor.User.FirstName} {tutor.User.LastName}",
                Slots = tutor.Availability
                    .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
                    .Select(a => new AvailabilitySlotViewModel
                    {
                        TutorAvailabilityId = a.TutorAvailabilityId,
                        DayOfWeek = a.DayOfWeek,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime
                    }).ToList()
            };
        }

        public async Task AddAvailabilitySlotAsync(AddAvailabilitySlotViewModel model)
        {
            if (model.EndTime <= model.StartTime)
                throw new InvalidOperationException("End time must be after start time.");

            _context.TutorAvailabilities.Add(new TutorAvailability
            {
                TutorUserId = model.TutorUserId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAvailabilitySlotAsync(int tutorAvailabilityId)
        {
            var slot = await _context.TutorAvailabilities.FindAsync(tutorAvailabilityId);
            if (slot != null)
            {
                _context.TutorAvailabilities.Remove(slot);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TutorUnavailabilityViewModel?> GetUnavailabilityAsync(int tutorUserId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .Include(t => t.Unavailability)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            if (tutor == null) return null;

            return new TutorUnavailabilityViewModel
            {
                TutorUserId = tutor.UserId,
                TutorName = $"{tutor.User.FirstName} {tutor.User.LastName}",
                Entries = tutor.Unavailability
                    .OrderByDescending(u => u.StartDate)
                    .Select(u => new UnavailabilityEntryViewModel
                    {
                        TutorUnavailabilityId = u.TutorUnavailabilityId,
                        StartDate = u.StartDate,
                        EndDate = u.EndDate,
                        Reason = u.Reason
                    }).ToList()
            };
        }

        public async Task AddUnavailabilityAsync(AddUnavailabilityViewModel model)
        {
            if (model.EndDate < model.StartDate)
                throw new InvalidOperationException("End date must be on or after start date.");

            _context.TutorUnavailabilities.Add(new TutorUnavailability
            {
                TutorUserId = model.TutorUserId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Reason = model.Reason
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveUnavailabilityAsync(int tutorUnavailabilityId)
        {
            var entry = await _context.TutorUnavailabilities.FindAsync(tutorUnavailabilityId);
            if (entry != null)
            {
                _context.TutorUnavailabilities.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> SetTutorActiveStatusAsync(int tutorUserId, bool isActive)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            if (tutor == null)
                throw new InvalidOperationException($"Tutor {tutorUserId} not found.");

            tutor.IsActive = isActive;
            await _context.SaveChangesAsync();

            return $"{tutor.User.FirstName} {tutor.User.LastName}";
        }
    }
}