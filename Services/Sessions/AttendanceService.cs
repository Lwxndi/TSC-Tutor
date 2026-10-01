using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public AttendanceService(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        public Task<MarkAttendanceViewModel?> GetForTutorAsync(int sessionId, int tutorUserId)
            => BuildAsync(sessionId, tutorUserId, isAdmin: false);

        public Task<MarkAttendanceViewModel?> GetForAdminAsync(int sessionId)
            => BuildAsync(sessionId, null, isAdmin: true);

        public Task<(bool Success, string? Error)> SaveForTutorAsync(MarkAttendanceViewModel model, int tutorUserId)
            => SaveAsync(model, tutorUserId, tutorUserId, isAdmin: false);

        public Task<(bool Success, string? Error)> SaveForAdminAsync(MarkAttendanceViewModel model, int adminUserId)
            => SaveAsync(model, null, adminUserId, isAdmin: true);

        public async Task<int> GetUnmarkedCountAsync(int sessionId)
        {
            return await _context.SessionAttendances
                .CountAsync(a => a.SessionId == sessionId && !a.IsMarked);
        }

        // ───────────── internals ─────────────

        private async Task<Session?> LoadSessionAsync(int sessionId, int? tutorUserId)
        {
            var query = _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Where(s => s.SessionId == sessionId);

            if (tutorUserId.HasValue)
                query = query.Where(s => (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId.Value);

            return await query.FirstOrDefaultAsync();
        }

        private static bool IsOpenForAttendance(Session s)
            => s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Completed;

        // Adds any Active learner who doesn't have a row yet. Rows start unmarked.
        private async Task EnsureRosterAsync(Session session)
        {
            var existing = await _context.SessionAttendances
                .Where(a => a.SessionId == session.SessionId)
                .Select(a => a.LearnerUserId)
                .ToListAsync();

            var activeLearnerIds = await _context.Enrollments
                .Where(e => e.OfferingId == session.OfferingId && e.Status == EnrollmentStatus.Active)
                .Select(e => e.LearnerUserId)
                .Distinct()
                .ToListAsync();

            var missing = activeLearnerIds.Except(existing).ToList();
            if (!missing.Any()) return;

            foreach (var learnerId in missing)
                _context.SessionAttendances.Add(new SessionAttendance
                {
                    SessionId = session.SessionId,
                    LearnerUserId = learnerId
                });

            await _context.SaveChangesAsync();
        }

        private async Task<MarkAttendanceViewModel?> BuildAsync(int sessionId, int? tutorUserId, bool isAdmin)
        {
            var session = await LoadSessionAsync(sessionId, tutorUserId);
            if (session == null) return null;

            var canEdit = IsOpenForAttendance(session);
            if (canEdit) await EnsureRosterAsync(session);

            var rows = await _context.SessionAttendances
                .Where(a => a.SessionId == sessionId)
                .Include(a => a.Learner).ThenInclude(l => l.User)
                .ToListAsync();

            return new MarkAttendanceViewModel
            {
                SessionId = session.SessionId,
                SubjectName = session.Offering.Subject.SubjectName,
                GradeNumber = (int)session.Offering.Grade,
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Status = session.Status,
                IsHybrid = session.Offering.DeliveryMethod == DeliveryMethod.Hybrid,
                DurationMinutes = (int)(session.EndTime - session.StartTime).TotalMinutes,
                IsAdminEdit = isAdmin,
                CanEdit = canEdit,
                Rows = rows
                    .OrderBy(a => a.Learner.User.LastName).ThenBy(a => a.Learner.User.FirstName)
                    .Select(a => new AttendanceRowViewModel
                    {
                        LearnerUserId = a.LearnerUserId,
                        LearnerName = $"{a.Learner.User.FirstName} {a.Learner.User.LastName}",
                        Status = a.Status,
                        MinutesLate = a.MinutesLate,
                        AttendedVia = a.AttendedVia,
                        Note = a.Note,
                        IsMarked = a.IsMarked,
                        Locked = !isAdmin && session.Status == SessionStatus.Completed && a.IsMarked
                    }).ToList()
            };
        }

        private async Task<(bool Success, string? Error)> SaveAsync(
            MarkAttendanceViewModel model, int? tutorUserId, int markedByUserId, bool isAdmin)
        {
            var session = await LoadSessionAsync(model.SessionId, tutorUserId);
            if (session == null)
                return (false, "Session not found.");

            if (!IsOpenForAttendance(session))
                return (false, "Start the session before taking attendance.");

            var isHybrid = session.Offering.DeliveryMethod == DeliveryMethod.Hybrid;
            var duration = (int)(session.EndTime - session.StartTime).TotalMinutes;

            var existing = await _context.SessionAttendances
                .Where(a => a.SessionId == session.SessionId)
                .ToDictionaryAsync(a => a.LearnerUserId);

            foreach (var row in model.Rows ?? new List<AttendanceRowViewModel>())
            {
                // Only rows that belong to this session's roster are ever touched
                if (!existing.TryGetValue(row.LearnerUserId, out var att)) continue;

                // Tutors can't change rows already marked once the session is completed
                if (!isAdmin && session.Status == SessionStatus.Completed && att.IsMarked) continue;

                if (!Enum.IsDefined(typeof(AttendanceStatus), row.Status))
                    return (false, "Invalid attendance status.");

                att.Status = row.Status;

                if (row.Status == AttendanceStatus.Present)
                {
                    var late = row.MinutesLate ?? 0;
                    if (late < 0 || late > duration)
                        return (false, $"Minutes late must be between 0 and {duration}.");

                    att.MinutesLate = late > 0 ? late : null;
                    att.AttendedVia = isHybrid ? (row.AttendedVia ?? AttendedVia.InPerson) : null;
                }
                else
                {
                    att.MinutesLate = null;
                    att.AttendedVia = null;
                }

                var note = row.Note?.Trim();
                att.Note = string.IsNullOrEmpty(note) ? null : (note.Length > 500 ? note[..500] : note);

                att.IsMarked = true;
                att.MarkedAt = DateTime.UtcNow;
                att.MarkedByUserId = markedByUserId;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}