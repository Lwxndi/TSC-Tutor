//using Microsoft.EntityFrameworkCore;
//using Tutor_Manager.Models;
//using Tutor_Manager.Models.Enums;
//using Tutor_Manager.Services.Notifications;
//using Tutor_Manager.ViewModels;
//using Tutor_Manager.ViewModels.Sessions;

//namespace Tutor_Manager.Services
//{
//    public class SessionService : ISessionService
//    {
//        private readonly Tutor_ManagerDatabaseContext _context;
//        private readonly INotificationService _notificationService;
//        private readonly ILogger<SessionService> _logger;

//        public SessionService(Tutor_ManagerDatabaseContext context, INotificationService notificationService, ILogger<SessionService> logger)
//        {
//            _context = context;
//            _notificationService = notificationService;
//            _logger = logger;
//        }

//        public async Task<List<SessionListItemViewModel>> GetSessionsForOfferingAsync(int offeringId)
//        {
//            var sessions = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.Subject)
//                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
//                .Where(s => s.OfferingId == offeringId)
//                .OrderByDescending(s => s.Date).ThenBy(s => s.StartTime)
//                .ToListAsync();

//            return sessions.Select(s =>
//            {
//                var effectiveTutor = s.OverrideTutor ?? s.Offering.Tutor;
//                return new SessionListItemViewModel
//                {
//                    SessionId = s.SessionId,
//                    OfferingId = s.OfferingId,
//                    SubjectName = s.Offering.Subject.SubjectName,
//                    Grade = s.Offering.Grade,
//                    TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                    Date = s.Date,
//                    StartTime = s.StartTime,
//                    EndTime = s.EndTime,
//                    Status = s.Status,
//                    JitsiLink = s.JitsiLink
//                };
//            }).ToList();
//        }

//        public async Task<CreateSessionViewModel?> GetCreateFormDataAsync(int offeringId)
//        {
//            var offering = await _context.Offerings
//                .Include(o => o.Subject)
//                .Include(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(o => o.TeachingDays)
//                .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

//            if (offering == null) return null;

//            var settings = await _context.TscSettings.FirstAsync();
//            var startTime = settings.EarliestTeachingTime;
//            var endTime = startTime.Add(TimeSpan.FromMinutes(offering.DurationMinutes));

//            return new CreateSessionViewModel
//            {
//                OfferingId = offering.OfferingId,
//                Date = GetNextTeachingDate(offering.TeachingDays.Select(td => td.DayOfWeek)),
//                StartTime = startTime,
//                EndTime = endTime,
//                SubjectName = offering.Subject.SubjectName,
//                Grade = offering.Grade,
//                TutorName = $"{offering.Tutor.User.FirstName} {offering.Tutor.User.LastName}",
//                OfferingTeachingDays = offering.TeachingDays.Select(td => td.DayOfWeek).ToList(),
//                DeliveryMethod = offering.DeliveryMethod
//            };
//        }

//        public async Task<(bool Success, string? Error, int? SessionId)> CreateSessionAsync(CreateSessionViewModel model)
//        {
//            var offering = await _context.Offerings
//                .Include(o => o.TeachingDays)
//                .FirstOrDefaultAsync(o => o.OfferingId == model.OfferingId);

//            if (offering == null)
//                return (false, "Offering not found.", null);

//            if (model.Date == default)
//                return (false, "Please select a valid session date.", null);

//            var validation = await ValidateSessionSlotAsync(offering, model.Date, model.StartTime, model.EndTime);
//            if (!validation.IsValid)
//                return (false, validation.Error, null);

//            var session = new Session
//            {
//                OfferingId = model.OfferingId,
//                Date = model.Date.Date,
//                StartTime = model.StartTime,
//                EndTime = model.EndTime,
//                Status = SessionStatus.Scheduled,
//                JitsiLink = offering.DeliveryMethod == DeliveryMethod.Remote
//                    ? GenerateJitsiLink(offering.OfferingId, model.Date)
//                    : null
//            };

//            _context.Sessions.Add(session);
//            await _context.SaveChangesAsync();

//            return (true, null, session.SessionId);
//        }

//        public async Task<SessionIndexViewModel> GetSessionIndexAsync(SessionFilterViewModel filter)
//        {
//            var query = _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.Subject)
//                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
//                .AsQueryable();

//            if (filter.SubjectId.HasValue)
//                query = query.Where(s => s.Offering.SubjectId == filter.SubjectId.Value);

//            if (filter.Grade.HasValue)
//                query = query.Where(s => s.Offering.Grade == filter.Grade.Value);

//            if (filter.TutorUserId.HasValue)
//                query = query.Where(s => s.Offering.TutorUserId == filter.TutorUserId.Value
//                                       || s.OverrideTutorUserId == filter.TutorUserId.Value);

//            if (filter.Status.HasValue)
//                query = query.Where(s => s.Status == filter.Status.Value);

//            var (from, to) = ResolveDateRange(filter.DateRange);
//            query = query.Where(s => s.Date.Date >= from && s.Date.Date <= to);

//            var sessions = await query
//                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
//                .ToListAsync();

//            return new SessionIndexViewModel
//            {
//                Sessions = sessions.Select(s =>
//                {
//                    var effectiveTutor = s.OverrideTutor ?? s.Offering.Tutor;
//                    return new SessionListItemViewModel
//                    {
//                        SessionId = s.SessionId,
//                        OfferingId = s.OfferingId,
//                        SubjectName = s.Offering.Subject.SubjectName,
//                        Grade = s.Offering.Grade,
//                        TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                        Date = s.Date,
//                        StartTime = s.StartTime,
//                        EndTime = s.EndTime,
//                        Status = s.Status,
//                        JitsiLink = s.JitsiLink
//                    };
//                }).ToList(),
//                Subjects = await GetSubjectDropdownAsync(),
//                Tutors = await GetTutorDropdownAsync(),
//                Filter = filter
//            };
//        }

//        public async Task<List<OfferingPickerItem>> GetOfferingPickerListAsync()
//        {
//            var offerings = await _context.Offerings
//                .Include(o => o.Subject)
//                .Include(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(o => o.TeachingDays)
//                .Where(o => o.IsActive)
//                .ToListAsync();

//            return offerings.Select(o => new OfferingPickerItem
//            {
//                OfferingId = o.OfferingId,
//                SubjectName = o.Subject.SubjectName,
//                Grade = o.Grade,
//                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
//                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).ToList()
//            })
//            .OrderBy(o => o.SubjectName).ThenBy(o => o.Grade)
//            .ToList();
//        }

//        public async Task<SessionDetailViewModel?> GetDetailsAsync(int sessionId)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.Subject)
//                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
//                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

//            if (session == null) return null;

//            var effectiveTutor = session.OverrideTutor ?? session.Offering.Tutor;

//            return new SessionDetailViewModel
//            {
//                SessionId = session.SessionId,
//                OfferingId = session.OfferingId,
//                SubjectName = session.Offering.Subject.SubjectName,
//                Grade = session.Offering.Grade,
//                TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                TutorUserId = effectiveTutor.UserId,
//                IsSubstitute = session.OverrideTutorUserId.HasValue, // add this bool to SessionDetailViewModel
//                DeliveryMethod = session.Offering.DeliveryMethod,
//                Date = session.Date,
//                StartTime = session.StartTime,
//                EndTime = session.EndTime,
//                Status = session.Status,
//                JitsiLink = session.JitsiLink
//            };
//        }

//        public async Task<(bool Success, string? Error)> CancelSessionAsync(int sessionId, string reason)
//        {
//            var session = await _context.Sessions.FindAsync(sessionId);
//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status == SessionStatus.Cancelled)
//                return (false, "Session is already cancelled.");

//            session.Status = SessionStatus.Cancelled;
//            session.CancellationReason = reason;
//            await _context.SaveChangesAsync();

//            return (true, null);
//        }

//        public async Task<RescheduleSessionViewModel?> GetRescheduleFormDataAsync(int sessionId)
//        {
//            var session = await _context.Sessions.FindAsync(sessionId);
//            if (session == null) return null;

//            return new RescheduleSessionViewModel
//            {
//                SessionId = session.SessionId,
//                NewDate = session.Date,
//                NewStartTime = session.StartTime,
//                NewEndTime = session.EndTime
//            };
//        }

//        public async Task<(bool Success, string? Error)> RescheduleSessionAsync(RescheduleSessionViewModel model)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.TeachingDays)
//                .FirstOrDefaultAsync(s => s.SessionId == model.SessionId);

//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status == SessionStatus.Cancelled)
//                return (false, "Cannot reschedule a cancelled session.");

//            var validation = await ValidateSessionSlotAsync(
//                session.Offering, model.NewDate, model.NewStartTime, model.NewEndTime,
//                tutorUserIdOverride: session.OverrideTutorUserId,
//                excludeSessionId: session.SessionId);

//            if (!validation.IsValid)
//                return (false, validation.Error);

//            session.RescheduledFromDate = session.Date;
//            session.RescheduledFromStartTime = session.StartTime;
//            session.RescheduledFromEndTime = session.EndTime;

//            session.Date = model.NewDate.Date;
//            session.StartTime = model.NewStartTime;
//            session.EndTime = model.NewEndTime;
//            session.Status = SessionStatus.Rescheduled;

//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        // ── FULL validation engine — every rule from both spec documents ──
//        // BUG FIXED: every check below now uses `tutorUserId` (override-aware),
//        // not `offering.TutorUserId` directly — previously the override
//        // parameter was computed but never actually used in any check.
//        private async Task<(bool IsValid, string? Error)> ValidateSessionSlotAsync(
//            Offering offering, DateTime date, TimeSpan startTime, TimeSpan endTime,
//            int? tutorUserIdOverride = null, int? excludeSessionId = null)
//        {
//            var tutorUserId = tutorUserIdOverride ?? offering.TutorUserId;

//            if (endTime <= startTime)
//                return (false, "End time must be after start time.");

//            if (date.Date < offering.StartDate.Date || date.Date > offering.EndDate.Date)
//                return (false, "Date falls outside the Offering's active period.");

//            var sessionDay = date.DayOfWeek;
//            if (!offering.TeachingDays.Any(td => td.DayOfWeek == sessionDay))
//                return (false, $"{sessionDay} is not a teaching day for this offering.");

//            var settings = await _context.TscSettings.FirstAsync();

//            if (startTime < settings.EarliestTeachingTime || endTime > settings.LatestTeachingTime)
//                return (false, $"Session must fall within TSC operating hours ({settings.EarliestTeachingTime:hh\\:mm}–{settings.LatestTeachingTime:hh\\:mm}).");

//            var durationMinutes = (endTime - startTime).TotalMinutes;
//            if (durationMinutes < settings.MinSessionDurationMinutes || durationMinutes > settings.MaxSessionDurationMinutes)
//                return (false, $"Session duration must be between {settings.MinSessionDurationMinutes} and {settings.MaxSessionDurationMinutes} minutes.");

//            var dayType = (sessionDay == DayOfWeek.Saturday || sessionDay == DayOfWeek.Sunday)
//     ? DayType.Weekend : DayType.Weekday;

//            var window = await _context.OfferingTimeWindows.FirstOrDefaultAsync(w =>
//                w.DayType == dayType &&
//                w.DeliveryMethod == offering.DeliveryMethod &&
//                w.OfferingType == offering.Type);

//            if (window == null)
//                return (false, $"No configured time window for {dayType} sessions with this delivery/type.");

//            if (startTime < window.WindowStart || endTime > window.WindowEnd)
//                return (false, $"Session must fall within TSC's {dayType} operating hours for this delivery/type ({window.WindowStart:hh\\:mm}–{window.WindowEnd:hh\\:mm}).");

//            var onLeave = await _context.TutorUnavailabilities.AnyAsync(u =>
//                u.TutorUserId == tutorUserId &&
//                date.Date >= u.StartDate.Date &&
//                date.Date <= u.EndDate.Date);

//            if (onLeave)
//                return (false, "Tutor is on recorded leave for this date.");

//            // NOTE: matches sessions where this tutor is EITHER the offering's
//            // regular tutor OR the override — a substitute's own conflicts
//            // (from other offerings, or other substitute assignments) must
//            // also be caught, not just the regular tutor's.
//            var sameDaySessionsQuery = _context.Sessions
//                .Include(s => s.Offering)
//                .Where(s => (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId)
//                            && s.Date.Date == date.Date
//                            && s.Status != SessionStatus.Cancelled);

//            if (excludeSessionId.HasValue)
//                sameDaySessionsQuery = sameDaySessionsQuery.Where(s => s.SessionId != excludeSessionId.Value);

//            var sameDaySessions = await sameDaySessionsQuery.ToListAsync();

//            if (sameDaySessions.Any(s => startTime < s.EndTime && endTime > s.StartTime))
//                return (false, "Tutor already has an overlapping session at this time.");

//            if (sameDaySessions.Count + 1 > settings.MaxSessionsPerTutorPerDay)
//                return (false, $"Tutor would exceed the maximum of {settings.MaxSessionsPerTutorPerDay} sessions per day.");

//            var existingDayMinutes = sameDaySessions.Sum(s => (s.EndTime - s.StartTime).TotalMinutes);
//            if ((existingDayMinutes + durationMinutes) / 60.0 > settings.MaxTutoringHoursPerTutorPerDay)
//                return (false, $"Tutor would exceed the maximum of {settings.MaxTutoringHoursPerTutorPerDay} tutoring hours per day.");

//            var daySubjectIds = sameDaySessions.Select(s => s.Offering.SubjectId).ToHashSet();
//            daySubjectIds.Add(offering.SubjectId);
//            if (daySubjectIds.Count > settings.MaxSubjectsPerTutorPerDay)
//                return (false, $"Tutor would exceed the maximum of {settings.MaxSubjectsPerTutorPerDay} different subjects per day.");

//            var minBreak = TimeSpan.FromMinutes(settings.MinBreakBetweenSessionsMinutes);
//            foreach (var s in sameDaySessions)
//            {
//                var gapBefore = startTime - s.EndTime;
//                var gapAfter = s.StartTime - endTime;
//                var gap = gapBefore >= TimeSpan.Zero ? gapBefore : gapAfter;

//                if (gap >= TimeSpan.Zero && gap < minBreak)
//                    return (false, $"Sessions must have at least {settings.MinBreakBetweenSessionsMinutes} minutes between them.");
//            }

//            var longerBreak = TimeSpan.FromMinutes(settings.LongerBreakMinutes);
//            var chain = sameDaySessions
//                .Where(s => s.StartTime < endTime)
//                .OrderByDescending(s => s.EndTime)
//                .ToList();

//            var consecutiveCount = 0;
//            var cursor = startTime;
//            foreach (var s in chain)
//            {
//                var gap = cursor - s.EndTime;
//                if (gap >= TimeSpan.Zero && gap < longerBreak)
//                {
//                    consecutiveCount++;
//                    cursor = s.StartTime;
//                }
//                else
//                {
//                    break;
//                }
//            }

//            if (consecutiveCount >= settings.MaxConsecutiveSessionsBeforeLongerBreak)
//                return (false, $"After {settings.MaxConsecutiveSessionsBeforeLongerBreak} consecutive sessions, a break of at least {settings.LongerBreakMinutes} minutes is required.");

//            var weekStart = date.Date.AddDays(-(int)date.DayOfWeek);
//            var weekEnd = weekStart.AddDays(6);

//            var weekSessionsQuery = _context.Sessions
//                .Include(s => s.Offering)
//                .Where(s => (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId)
//                            && s.Date.Date >= weekStart && s.Date.Date <= weekEnd
//                            && s.Status != SessionStatus.Cancelled);

//            if (excludeSessionId.HasValue)
//                weekSessionsQuery = weekSessionsQuery.Where(s => s.SessionId != excludeSessionId.Value);

//            var weekSessions = await weekSessionsQuery.ToListAsync();

//            var weekSubjectIds = weekSessions.Select(s => s.Offering.SubjectId).ToHashSet();
//            weekSubjectIds.Add(offering.SubjectId);
//            if (weekSubjectIds.Count > settings.MaxSubjectsPerTutorPerWeek)
//                return (false, $"Tutor would exceed the maximum of {settings.MaxSubjectsPerTutorPerWeek} different subjects per week.");

//            var existingWeekMinutes = weekSessions.Sum(s => (s.EndTime - s.StartTime).TotalMinutes);
//            if ((existingWeekMinutes + durationMinutes) / 60.0 > settings.MaxWeeklyTutoringHours)
//                return (false, $"Tutor would exceed the maximum of {settings.MaxWeeklyTutoringHours} tutoring hours per week.");

//            return (true, null);
//        }

//        private static string GenerateJitsiLink(int offeringId, DateTime date)
//        {
//            var roomName = $"TSC-Offering{offeringId}-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6]}";
//            return $"https://meet.jit.si/{roomName}";
//        }

//        private static (DateTime From, DateTime To) ResolveDateRange(string dateRange)
//        {
//            var today = DateTime.Today;
//            return dateRange switch
//            {
//                "Today" => (today, today),
//                "ThisWeek" => (today, today.AddDays(7)),
//                "ThisMonth" => (today, today.AddMonths(1)),
//                "All" => (today, today.AddYears(1)),
//                _ => (today, today)
//            };
//        }

//        private async Task<List<SubjectDropdownItem>> GetSubjectDropdownAsync()
//        {
//            return await _context.Subjects
//                .Where(s => s.IsActive)
//                .OrderBy(s => s.SubjectName)
//                .Select(s => new SubjectDropdownItem { SubjectId = s.SubjectId, SubjectName = s.SubjectName })
//                .ToListAsync();
//        }

//        private async Task<List<TutorDropdownItem>> GetTutorDropdownAsync()
//        {
//            var tutors = await _context.Tutors
//                .Include(t => t.User)
//                .Where(t => t.IsActive)
//                .ToListAsync();

//            return tutors
//                .Select(t => new TutorDropdownItem { TutorUserId = t.UserId, FullName = $"{t.User.FirstName} {t.User.LastName}" })
//                .OrderBy(t => t.FullName)
//                .ToList();
//        }

//        private static DateTime GetNextTeachingDate(IEnumerable<DayOfWeek> teachingDays)
//        {
//            var days = teachingDays.ToHashSet();
//            var candidate = DateTime.Today;
//            for (int i = 0; i < 7; i++)
//            {
//                if (days.Contains(candidate.DayOfWeek))
//                    return candidate;
//                candidate = candidate.AddDays(1);
//            }
//            return DateTime.Today;
//        }

//        public async Task<List<SubjectDropdownItem>> GetActiveSubjectsAsync()
//        {
//            return await _context.Subjects
//                .Where(s => s.IsActive)
//                .OrderBy(s => s.SubjectName)
//                .Select(s => new SubjectDropdownItem { SubjectId = s.SubjectId, SubjectName = s.SubjectName })
//                .ToListAsync();
//        }

//        public async Task<List<MatchingOfferingItem>> GetMatchingOfferingsAsync(int subjectId, Grade grade)
//        {
//            var offerings = await _context.Offerings
//                .Include(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(o => o.TeachingDays)
//                .Where(o => o.SubjectId == subjectId && o.Grade == grade && o.IsActive)
//                .ToListAsync();

//            return offerings.Select(o => new MatchingOfferingItem
//            {
//                OfferingId = o.OfferingId,
//                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
//                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).ToList(),
//                DeliveryMethod = o.DeliveryMethod
//            }).ToList();
//        }

//        public async Task<GenerationPreviewViewModel> PreviewGenerationAsync(GenerateSessionsRequestViewModel request)
//        {
//            var result = new GenerationPreviewViewModel();

//            var offerings = await _context.Offerings
//                .Include(o => o.Subject)
//                .Include(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(o => o.TeachingDays)
//                .Where(o => request.SelectedOfferingIds.Contains(o.OfferingId))
//                .ToListAsync();

//            foreach (var offering in offerings)
//            {
//                var teachingDays = offering.TeachingDays.Select(td => td.DayOfWeek).ToHashSet();

//                var rangeStart = request.StartDate.Date > offering.StartDate.Date ? request.StartDate.Date : offering.StartDate.Date;
//                var rangeEnd = request.EndDate.Date < offering.EndDate.Date ? request.EndDate.Date : offering.EndDate.Date;

//                for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
//                {
//                    if (!teachingDays.Contains(date.DayOfWeek))
//                        continue;

//                    var startTime = offering.StartTime;
//                    var endTime = offering.EndTime;

//                    var validation = await ValidateSessionSlotAsync(offering, date, startTime, endTime);

//                    var candidate = new SessionCandidateViewModel
//                    {
//                        OfferingId = offering.OfferingId,
//                        SubjectName = offering.Subject.SubjectName,
//                        Grade = offering.Grade,
//                        TutorName = $"{offering.Tutor.User.FirstName} {offering.Tutor.User.LastName}",
//                        Date = date,
//                        StartTime = startTime,
//                        EndTime = endTime,
//                        IsValid = validation.IsValid,
//                        Error = validation.Error
//                    };

//                    (candidate.IsValid ? result.ValidCandidates : result.InvalidCandidates).Add(candidate);
//                }
//            }

//            return result;
//        }

//        public async Task<(int Created, int Skipped)> ConfirmGenerationAsync(List<SessionCandidateViewModel> candidates)
//        {
//            var created = 0;
//            var skipped = 0;

//            var offeringIds = candidates.Select(c => c.OfferingId).Distinct().ToList();
//            var offerings = await _context.Offerings
//                .Include(o => o.TeachingDays)
//                .Where(o => offeringIds.Contains(o.OfferingId))
//                .ToDictionaryAsync(o => o.OfferingId);

//            foreach (var candidate in candidates)
//            {
//                if (!offerings.TryGetValue(candidate.OfferingId, out var offering))
//                {
//                    skipped++;
//                    continue;
//                }

//                var validation = await ValidateSessionSlotAsync(offering, candidate.Date, candidate.StartTime, candidate.EndTime);

//                if (!validation.IsValid)
//                {
//                    skipped++;
//                    continue;
//                }

//                var session = new Session
//                {
//                    OfferingId = offering.OfferingId,
//                    Date = candidate.Date.Date,
//                    StartTime = candidate.StartTime,
//                    EndTime = candidate.EndTime,
//                    Status = SessionStatus.Scheduled,
//                    JitsiLink = offering.DeliveryMethod == DeliveryMethod.Remote
//                        ? GenerateJitsiLink(offering.OfferingId, candidate.Date)
//                        : null
//                };

//                _context.Sessions.Add(session);
//                created++;
//            }

//            await _context.SaveChangesAsync();
//            return (created, skipped);
//        }

//        public async Task<(bool Success, string? Error)> StartSessionAsync(int sessionId)
//        {
//            var session = await _context.Sessions.FindAsync(sessionId);
//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status != SessionStatus.Scheduled)
//                return (false, $"Cannot start a session with status {session.Status}.");

//            session.Status = SessionStatus.InProgress;
//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        public async Task<(bool Success, string? Error)> CompleteSessionAsync(int sessionId, string? notes)
//        {
//            var session = await _context.Sessions.FindAsync(sessionId);
//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status != SessionStatus.InProgress && session.Status != SessionStatus.Scheduled)
//                return (false, $"Cannot complete a session with status {session.Status}.");

//            session.Status = SessionStatus.Completed;
//            session.TutorNotes = notes;
//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        public async Task<List<SessionListItemViewModel>> GetMySessionsTodayAsync(int tutorUserId)
//        {
//            var today = DateTime.Today;

//            var sessions = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.Subject)
//                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
//                .Where(s => (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId)
//                            && s.Date.Date == today)
//                .OrderBy(s => s.StartTime)
//                .ToListAsync();

//            return sessions.Select(s =>
//            {
//                var effectiveTutor = s.OverrideTutor ?? s.Offering.Tutor;
//                return new SessionListItemViewModel
//                {
//                    SessionId = s.SessionId,
//                    OfferingId = s.OfferingId,
//                    SubjectName = s.Offering.Subject.SubjectName,
//                    Grade = s.Offering.Grade,
//                    TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                    Date = s.Date,
//                    StartTime = s.StartTime,
//                    EndTime = s.EndTime,
//                    Status = s.Status,
//                    JitsiLink = s.JitsiLink
//                };
//            }).ToList();
//        }

//        public async Task<(bool Success, string? Error)> StartSessionAsync(int sessionId, int tutorUserId)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering)
//                .FirstOrDefaultAsync(s => s.SessionId == sessionId
//                    && (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId));

//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status != SessionStatus.Scheduled)
//                return (false, $"Cannot start a session with status {session.Status}.");

//            session.Status = SessionStatus.InProgress;
//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        public async Task<(bool Success, string? Error)> CompleteSessionAsync(int sessionId, int tutorUserId, string? notes)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.Subject)
//                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
//                .FirstOrDefaultAsync(s => s.SessionId == sessionId
//                    && (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId));

//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status != SessionStatus.InProgress && session.Status != SessionStatus.Scheduled)
//                return (false, $"Cannot complete a session with status {session.Status}.");

//            session.Status = SessionStatus.Completed;
//            session.TutorNotes = notes;
//            await _context.SaveChangesAsync();

//            var effectiveTutor = session.OverrideTutor ?? session.Offering.Tutor;

//            var data = new Dictionary<string, string>
//            {
//                ["TutorName"] = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                ["Subject"] = session.Offering.Subject.SubjectName,
//                ["Grade"] = ((int)session.Offering.Grade).ToString(),
//                ["Date"] = session.Date.ToString("dd MMM yyyy")
//            };

//            var adminUserIds = await _context.Set<Administrator>()
//                .Select(a => a.UserId)
//                .ToListAsync();

//            foreach (var adminId in adminUserIds)
//                await SafeNotifyAsync(adminId, NotificationType.SessionCompleted, data);

//            // TODO (Phase 6): notify enrolled Learners once Enrollment/Session↔Learner link exists.

//            return (true, null);
//        }

//        public async Task<SessionListItemViewModel?> GetSessionForTutorAsync(int sessionId, int tutorUserId)
//        {
//            var s = await _context.Sessions
//                .Include(x => x.Offering).ThenInclude(o => o.Subject)
//                .Include(x => x.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(x => x.OverrideTutor).ThenInclude(t => t!.User)
//                .FirstOrDefaultAsync(x => x.SessionId == sessionId
//                    && (x.Offering.TutorUserId == tutorUserId || x.OverrideTutorUserId == tutorUserId));

//            if (s == null) return null;

//            var effectiveTutor = s.OverrideTutor ?? s.Offering.Tutor;

//            return new SessionListItemViewModel
//            {
//                SessionId = s.SessionId,
//                OfferingId = s.OfferingId,
//                SubjectName = s.Offering.Subject.SubjectName,
//                Grade = s.Offering.Grade,
//                TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                Date = s.Date,
//                StartTime = s.StartTime,
//                EndTime = s.EndTime,
//                Status = s.Status,
//                JitsiLink = s.JitsiLink
//            };
//        }

//        private async Task SafeNotifyAsync(int userId, NotificationType type, Dictionary<string, string> data)
//        {
//            try
//            {
//                await _notificationService.SendAsync(userId, type, data);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Notification failed for user {UserId}, type {Type}", userId, type);
//            }
//        }

//        public async Task<GenerationPreviewViewModel> GetSystemTimetablePreviewAsync(int? subjectId, Grade? grade, DateTime startDate, DateTime endDate)
//        {
//            var query = _context.Offerings.Where(o => o.IsActive).AsQueryable();

//            if (subjectId.HasValue)
//                query = query.Where(o => o.SubjectId == subjectId.Value);

//            if (grade.HasValue)
//                query = query.Where(o => o.Grade == grade.Value);

//            var offeringIds = await query.Select(o => o.OfferingId).ToListAsync();

//            if (!offeringIds.Any())
//                return new GenerationPreviewViewModel();

//            var request = new GenerateSessionsRequestViewModel
//            {
//                SelectedOfferingIds = offeringIds,
//                StartDate = startDate,
//                EndDate = endDate
//            };

//            return await PreviewGenerationAsync(request);
//        }

//        // ── Substitute tutor assignment ──

//        public async Task<(bool Success, string? Error)> AssignOverrideTutorAsync(int sessionId, int tutorUserId)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.TeachingDays)
//                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status is SessionStatus.Completed or SessionStatus.Cancelled)
//                return (false, $"Cannot assign a substitute to a session with status {session.Status}.");

//            var validation = await ValidateSessionSlotAsync(
//                session.Offering, session.Date, session.StartTime, session.EndTime,
//                tutorUserIdOverride: tutorUserId, excludeSessionId: session.SessionId);

//            if (!validation.IsValid)
//                return (false, validation.Error);

//            session.OverrideTutorUserId = tutorUserId;
//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        public async Task<(bool Success, string? Error)> RemoveOverrideTutorAsync(int sessionId)
//        {
//            var session = await _context.Sessions.FindAsync(sessionId);
//            if (session == null)
//                return (false, "Session not found.");

//            session.OverrideTutorUserId = null;
//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        public async Task<List<TutorDropdownItem>> GetAvailableSubstitutesAsync(int sessionId)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.TeachingDays)
//                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

//            if (session == null) return new();

//            var sessionDay = session.Date.DayOfWeek;

//            var candidateIds = await _context.TutorAvailabilities
//                .Where(a => a.DayOfWeek == sessionDay
//                            && a.StartTime <= session.StartTime
//                            && a.EndTime >= session.EndTime
//                            && a.TutorUserId != session.Offering.TutorUserId) // exclude the regular tutor — this is for substitutes
//                .Select(a => a.TutorUserId)
//                .Distinct()
//                .ToListAsync();

//            var tutors = await _context.Tutors
//                .Include(t => t.User)
//                .Where(t => candidateIds.Contains(t.UserId) && t.IsActive)
//                .ToListAsync();

//            var result = new List<TutorDropdownItem>();
//            foreach (var t in tutors)
//            {
//                var validation = await ValidateSessionSlotAsync(
//                    session.Offering, session.Date, session.StartTime, session.EndTime,
//                    tutorUserIdOverride: t.UserId, excludeSessionId: session.SessionId);

//                if (validation.IsValid)
//                    result.Add(new TutorDropdownItem { TutorUserId = t.UserId, FullName = $"{t.User.FirstName} {t.User.LastName}" });
//            }

//            return result.OrderBy(t => t.FullName).ToList();
//        }

//        public async Task<(bool Success, string? Error)> CompleteSessionAsync(int sessionId, CompleteSessionViewModel notes)
//        {
//            var session = await _context.Sessions.FindAsync(sessionId);
//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status != SessionStatus.InProgress && session.Status != SessionStatus.Scheduled)
//                return (false, $"Cannot complete a session with status {session.Status}.");

//            session.Status = SessionStatus.Completed;
//            session.TopicsCovered = notes.TopicsCovered;
//            session.Performance = notes.Performance;
//            session.HomeworkAssigned = notes.HomeworkAssigned;
//            session.TutorComments = notes.TutorComments;
//            await _context.SaveChangesAsync();
//            return (true, null);
//        }

//        public async Task<(bool Success, string? Error)> CompleteSessionAsync(int sessionId, int tutorUserId, CompleteSessionViewModel notes)
//        {
//            var session = await _context.Sessions
//                .Include(s => s.Offering).ThenInclude(o => o.Subject)
//                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
//                .FirstOrDefaultAsync(s => s.SessionId == sessionId
//                    && (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId));

//            if (session == null)
//                return (false, "Session not found.");

//            if (session.Status != SessionStatus.InProgress && session.Status != SessionStatus.Scheduled)
//                return (false, $"Cannot complete a session with status {session.Status}.");

//            session.Status = SessionStatus.Completed;
//            session.TopicsCovered = notes.TopicsCovered;
//            session.Performance = notes.Performance;
//            session.HomeworkAssigned = notes.HomeworkAssigned;
//            session.TutorComments = notes.TutorComments;
//            await _context.SaveChangesAsync();

//            var effectiveTutor = session.OverrideTutor ?? session.Offering.Tutor;

//            var data = new Dictionary<string, string>
//            {
//                ["TutorName"] = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
//                ["Subject"] = session.Offering.Subject.SubjectName,
//                ["Grade"] = ((int)session.Offering.Grade).ToString(),
//                ["Date"] = session.Date.ToString("dd MMM yyyy")
//            };

//            var adminUserIds = await _context.Set<Administrator>().Select(a => a.UserId).ToListAsync();
//            foreach (var adminId in adminUserIds)
//                await SafeNotifyAsync(adminId, NotificationType.SessionCompleted, data);

//            return (true, null);
//        }

//        private static TimeSpan ResolveStartTime(Offering offering, DayOfWeek day, OfferingTeachingDay? teachingDay = null)
//        {
//            if (teachingDay?.StartTimeOverride.HasValue == true)
//                return teachingDay.StartTimeOverride.Value;

//            bool isWeekend = day is DayOfWeek.Saturday or DayOfWeek.Sunday;
//            var tiered = isWeekend ? offering.WeekendStartTime : offering.WeekdayStartTime;

//            return tiered ?? offering.StartTime; // fallback for offerings created before this change
//        }
//    }
//}


using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.EnrollmentServices;
using Tutor_Manager.Services.Notifications;
using Tutor_Manager.ViewModels;
using Tutor_Manager.ViewModels.Sessions;

namespace Tutor_Manager.Services
{
    public class SessionService : ISessionService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SessionService> _logger;
        private readonly IEnrollmentService _enrollmentService;

        public SessionService(Tutor_ManagerDatabaseContext context, INotificationService notificationService, ILogger<SessionService> logger, IEnrollmentService enrollmentService)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
            _enrollmentService = enrollmentService;
        }

        // ───────────────────────── Listing / admin views ─────────────────────────

        public async Task<List<SessionListItemViewModel>> GetSessionsForOfferingAsync(int offeringId)
        {
            var sessions = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .Where(s => s.OfferingId == offeringId)
                .OrderByDescending(s => s.Date).ThenBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        public async Task<CreateSessionViewModel?> GetCreateFormDataAsync(int offeringId)
        {
            var offering = await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

            if (offering == null) return null;

            var nextDate = GetNextTeachingDate(offering.TeachingDays.Select(td => td.DayOfWeek));
            var teachingDay = offering.TeachingDays.FirstOrDefault(td => td.DayOfWeek == nextDate.DayOfWeek);

            var startTime = ResolveStartTime(offering, nextDate.DayOfWeek, teachingDay);
            var endTime = startTime.Add(TimeSpan.FromMinutes(offering.DurationMinutes));

            return new CreateSessionViewModel
            {
                OfferingId = offering.OfferingId,
                Date = nextDate,
                StartTime = startTime,
                EndTime = endTime,
                SubjectName = offering.Subject.SubjectName,
                Grade = offering.Grade,
                TutorName = $"{offering.Tutor.User.FirstName} {offering.Tutor.User.LastName}",
                OfferingTeachingDays = offering.TeachingDays.Select(td => td.DayOfWeek).ToList(),
                DeliveryMethod = offering.DeliveryMethod
            };
        }

        public async Task<(bool Success, string? Error, int? SessionId)> CreateSessionAsync(CreateSessionViewModel model)
        {
            var offering = await _context.Offerings
                .Include(o => o.TeachingDays)
                .FirstOrDefaultAsync(o => o.OfferingId == model.OfferingId);

            if (offering == null)
                return (false, "Offering not found.", null);

            if (model.Date == default)
                return (false, "Please select a valid session date.", null);

            var validation = await ValidateSessionSlotAsync(offering, model.Date, model.StartTime, model.EndTime);
            if (!validation.IsValid)
                return (false, validation.Error, null);

            var session = new Session
            {
                OfferingId = model.OfferingId,
                Date = model.Date.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Status = SessionStatus.Scheduled,
                JitsiLink = NeedsMeetingLink(offering)
                    ? GenerateJitsiLink(offering.OfferingId, model.Date)
                    : null
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return (true, null, session.SessionId);
        }

        public async Task<SessionIndexViewModel> GetSessionIndexAsync(SessionFilterViewModel filter)
        {
            var query = _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .AsQueryable();

            if (filter.SubjectId.HasValue)
                query = query.Where(s => s.Offering.SubjectId == filter.SubjectId.Value);

            if (filter.Grade.HasValue)
                query = query.Where(s => s.Offering.Grade == filter.Grade.Value);

            if (filter.TutorUserId.HasValue)
                query = query.Where(s => s.Offering.TutorUserId == filter.TutorUserId.Value
                                       || s.OverrideTutorUserId == filter.TutorUserId.Value);

            if (filter.Status.HasValue)
                query = query.Where(s => s.Status == filter.Status.Value);

            var (from, to) = ResolveDateRange(filter.DateRange);
            query = query.Where(s => s.Date.Date >= from && s.Date.Date <= to);

            var sessions = await query
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToListAsync();

            return new SessionIndexViewModel
            {
                Sessions = sessions.Select(MapToListItem).ToList(),
                Subjects = await GetSubjectDropdownAsync(),
                Tutors = await GetTutorDropdownAsync(),
                Filter = filter
            };
        }

        public async Task<List<OfferingPickerItem>> GetOfferingPickerListAsync()
        {
            var offerings = await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .Where(o => o.IsActive)
                .ToListAsync();

            return offerings.Select(o => new OfferingPickerItem
            {
                OfferingId = o.OfferingId,
                SubjectName = o.Subject.SubjectName,
                Grade = o.Grade,
                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).ToList()
            })
            .OrderBy(o => o.SubjectName).ThenBy(o => o.Grade)
            .ToList();
        }

        public async Task<SessionDetailViewModel?> GetDetailsAsync(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null) return null;

            var effectiveTutor = session.OverrideTutor ?? session.Offering.Tutor;

            return new SessionDetailViewModel
            {
                SessionId = session.SessionId,
                OfferingId = session.OfferingId,
                SubjectName = session.Offering.Subject.SubjectName,
                Grade = session.Offering.Grade,
                TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
                TutorUserId = effectiveTutor.UserId,
                IsSubstitute = session.OverrideTutorUserId.HasValue,
                DeliveryMethod = session.Offering.DeliveryMethod,
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Status = session.Status,
                JitsiLink = session.JitsiLink
            };
        }

        // ───────────────────────── Cancel / reschedule ─────────────────────────

        public async Task<(bool Success, string? Error)> CancelSessionAsync(int sessionId, string reason, int? cancelledByUserId = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return (false, "A cancellation reason is required.");

            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
                return (false, "Session not found.");

            if (session.Status == SessionStatus.Cancelled)
                return (false, "Session is already cancelled.");

            if (session.Status == SessionStatus.Completed)
                return (false, "A completed session can't be cancelled.");

            session.Status = SessionStatus.Cancelled;
            session.CancellationReason = reason.Trim();
            session.CancelledAt = DateTime.UtcNow;
            session.CancelledByUserId = cancelledByUserId;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CancelSessionForTutorAsync(int sessionId, int tutorUserId, string reason)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId
                    && (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId);

            if (session == null)
                return (false, "Session not found.");

            if (session.Status != SessionStatus.Scheduled && session.Status != SessionStatus.Rescheduled)
                return (false, "Only sessions that haven't started can be cancelled.");

            return await CancelSessionAsync(sessionId, reason, tutorUserId);
        }

        public async Task<RescheduleSessionViewModel?> GetRescheduleFormDataAsync(int sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null) return null;

            return new RescheduleSessionViewModel
            {
                SessionId = session.SessionId,
                NewDate = session.Date,
                NewStartTime = session.StartTime,
                NewEndTime = session.EndTime
            };
        }

        public async Task<(bool Success, string? Error)> RescheduleSessionAsync(RescheduleSessionViewModel model)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.TeachingDays)
                .FirstOrDefaultAsync(s => s.SessionId == model.SessionId);

            if (session == null)
                return (false, "Session not found.");

            if (session.Status == SessionStatus.Cancelled)
                return (false, "Cannot reschedule a cancelled session.");

            if (session.Status == SessionStatus.Completed)
                return (false, "Cannot reschedule a completed session.");

            var validation = await ValidateSessionSlotAsync(
                session.Offering, model.NewDate, model.NewStartTime, model.NewEndTime,
                tutorUserIdOverride: session.OverrideTutorUserId,
                excludeSessionId: session.SessionId);

            if (!validation.IsValid)
                return (false, validation.Error);

            session.RescheduledFromDate = session.Date;
            session.RescheduledFromStartTime = session.StartTime;
            session.RescheduledFromEndTime = session.EndTime;
            session.RescheduleReason = model.RescheduleReason;

            session.Date = model.NewDate.Date;
            session.StartTime = model.NewStartTime;
            session.EndTime = model.NewEndTime;
            session.Status = SessionStatus.Rescheduled;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        // ───────────────────────── Validation engine ─────────────────────────

        private async Task<(bool IsValid, string? Error)> ValidateSessionSlotAsync(
            Offering offering, DateTime date, TimeSpan startTime, TimeSpan endTime,
            int? tutorUserIdOverride = null, int? excludeSessionId = null)
        {
            var tutorUserId = tutorUserIdOverride ?? offering.TutorUserId;

            if (endTime <= startTime)
                return (false, "End time must be after start time.");

            if (date.Date < offering.StartDate.Date || date.Date > offering.EndDate.Date)
                return (false, "Date falls outside the Offering's active period.");

            var sessionDay = date.DayOfWeek;
            if (!offering.TeachingDays.Any(td => td.DayOfWeek == sessionDay))
                return (false, $"{sessionDay} is not a teaching day for this offering.");

            var settings = await _context.TscSettings.FirstAsync();

            if (startTime < settings.EarliestTeachingTime || endTime > settings.LatestTeachingTime)
                return (false, $"Session must fall within TSC operating hours ({settings.EarliestTeachingTime:hh\\:mm}–{settings.LatestTeachingTime:hh\\:mm}).");

            var durationMinutes = (endTime - startTime).TotalMinutes;
            if (durationMinutes < settings.MinSessionDurationMinutes || durationMinutes > settings.MaxSessionDurationMinutes)
                return (false, $"Session duration must be between {settings.MinSessionDurationMinutes} and {settings.MaxSessionDurationMinutes} minutes.");

            var dayType = (sessionDay == DayOfWeek.Saturday || sessionDay == DayOfWeek.Sunday)
                ? DayType.Weekend : DayType.Weekday;

            var window = await _context.OfferingTimeWindows.FirstOrDefaultAsync(w =>
                w.DayType == dayType &&
                w.DeliveryMethod == offering.DeliveryMethod &&
                w.OfferingType == offering.Type);

            if (window == null)
                return (false, $"No configured time window for {dayType} sessions with this delivery/type.");

            if (startTime < window.WindowStart || endTime > window.WindowEnd)
                return (false, $"Session must fall within TSC's {dayType} operating hours for this delivery/type ({window.WindowStart:hh\\:mm}–{window.WindowEnd:hh\\:mm}).");

            var onLeave = await _context.TutorUnavailabilities.AnyAsync(u =>
                u.TutorUserId == tutorUserId &&
                date.Date >= u.StartDate.Date &&
                date.Date <= u.EndDate.Date);

            if (onLeave)
                return (false, "Tutor is on recorded leave for this date.");

            var sameDaySessionsQuery = _context.Sessions
                .Include(s => s.Offering)
                .Where(s => (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId)
                            && s.Date.Date == date.Date
                            && s.Status != SessionStatus.Cancelled);

            if (excludeSessionId.HasValue)
                sameDaySessionsQuery = sameDaySessionsQuery.Where(s => s.SessionId != excludeSessionId.Value);

            var sameDaySessions = await sameDaySessionsQuery.ToListAsync();

            if (sameDaySessions.Any(s => startTime < s.EndTime && endTime > s.StartTime))
                return (false, "Tutor already has an overlapping session at this time.");

            if (sameDaySessions.Count + 1 > settings.MaxSessionsPerTutorPerDay)
                return (false, $"Tutor would exceed the maximum of {settings.MaxSessionsPerTutorPerDay} sessions per day.");

            var existingDayMinutes = sameDaySessions.Sum(s => (s.EndTime - s.StartTime).TotalMinutes);
            if ((existingDayMinutes + durationMinutes) / 60.0 > settings.MaxTutoringHoursPerTutorPerDay)
                return (false, $"Tutor would exceed the maximum of {settings.MaxTutoringHoursPerTutorPerDay} tutoring hours per day.");

            var daySubjectIds = sameDaySessions.Select(s => s.Offering.SubjectId).ToHashSet();
            daySubjectIds.Add(offering.SubjectId);
            if (daySubjectIds.Count > settings.MaxSubjectsPerTutorPerDay)
                return (false, $"Tutor would exceed the maximum of {settings.MaxSubjectsPerTutorPerDay} different subjects per day.");

            var minBreak = TimeSpan.FromMinutes(settings.MinBreakBetweenSessionsMinutes);
            foreach (var s in sameDaySessions)
            {
                var gapBefore = startTime - s.EndTime;
                var gapAfter = s.StartTime - endTime;
                var gap = gapBefore >= TimeSpan.Zero ? gapBefore : gapAfter;

                if (gap >= TimeSpan.Zero && gap < minBreak)
                    return (false, $"Sessions must have at least {settings.MinBreakBetweenSessionsMinutes} minutes between them.");
            }

            var longerBreak = TimeSpan.FromMinutes(settings.LongerBreakMinutes);
            var chain = sameDaySessions
                .Where(s => s.StartTime < endTime)
                .OrderByDescending(s => s.EndTime)
                .ToList();

            var consecutiveCount = 0;
            var cursor = startTime;
            foreach (var s in chain)
            {
                var gap = cursor - s.EndTime;
                if (gap >= TimeSpan.Zero && gap < longerBreak)
                {
                    consecutiveCount++;
                    cursor = s.StartTime;
                }
                else
                {
                    break;
                }
            }

            if (consecutiveCount >= settings.MaxConsecutiveSessionsBeforeLongerBreak)
                return (false, $"After {settings.MaxConsecutiveSessionsBeforeLongerBreak} consecutive sessions, a break of at least {settings.LongerBreakMinutes} minutes is required.");

            var weekStart = date.Date.AddDays(-(int)date.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var weekSessionsQuery = _context.Sessions
                .Include(s => s.Offering)
                .Where(s => (s.Offering.TutorUserId == tutorUserId || s.OverrideTutorUserId == tutorUserId)
                            && s.Date.Date >= weekStart && s.Date.Date <= weekEnd
                            && s.Status != SessionStatus.Cancelled);

            if (excludeSessionId.HasValue)
                weekSessionsQuery = weekSessionsQuery.Where(s => s.SessionId != excludeSessionId.Value);

            var weekSessions = await weekSessionsQuery.ToListAsync();

            var weekSubjectIds = weekSessions.Select(s => s.Offering.SubjectId).ToHashSet();
            weekSubjectIds.Add(offering.SubjectId);
            if (weekSubjectIds.Count > settings.MaxSubjectsPerTutorPerWeek)
                return (false, $"Tutor would exceed the maximum of {settings.MaxSubjectsPerTutorPerWeek} different subjects per week.");

            var existingWeekMinutes = weekSessions.Sum(s => (s.EndTime - s.StartTime).TotalMinutes);
            if ((existingWeekMinutes + durationMinutes) / 60.0 > settings.MaxWeeklyTutoringHours)
                return (false, $"Tutor would exceed the maximum of {settings.MaxWeeklyTutoringHours} tutoring hours per week.");

            return (true, null);
        }

        // ───────────────────────── Helpers ─────────────────────────

        private static bool NeedsMeetingLink(Offering offering)
            => offering.DeliveryMethod is DeliveryMethod.Remote or DeliveryMethod.Hybrid;

        private static string GenerateJitsiLink(int offeringId, DateTime date)
        {
            var roomName = $"TSC-Offering{offeringId}-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6]}";
            return $"https://meet.jit.si/{roomName}";
        }

        private static (DateTime From, DateTime To) ResolveDateRange(string dateRange)
        {
            var today = DateTime.Today;
            return dateRange switch
            {
                "Today" => (today, today),
                "ThisWeek" => (today, today.AddDays(7)),
                "ThisMonth" => (today, today.AddMonths(1)),
                "All" => (today, today.AddYears(1)),
                _ => (today, today)
            };
        }

        private async Task<List<SubjectDropdownItem>> GetSubjectDropdownAsync()
        {
            return await _context.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.SubjectName)
                .Select(s => new SubjectDropdownItem { SubjectId = s.SubjectId, SubjectName = s.SubjectName })
                .ToListAsync();
        }

        private async Task<List<TutorDropdownItem>> GetTutorDropdownAsync()
        {
            var tutors = await _context.Tutors
                .Include(t => t.User)
                .Where(t => t.IsActive)
                .ToListAsync();

            return tutors
                .Select(t => new TutorDropdownItem { TutorUserId = t.UserId, FullName = $"{t.User.FirstName} {t.User.LastName}" })
                .OrderBy(t => t.FullName)
                .ToList();
        }

        private static DateTime GetNextTeachingDate(IEnumerable<DayOfWeek> teachingDays)
        {
            var days = teachingDays.ToHashSet();
            var candidate = DateTime.Today;
            for (int i = 0; i < 7; i++)
            {
                if (days.Contains(candidate.DayOfWeek))
                    return candidate;
                candidate = candidate.AddDays(1);
            }
            return DateTime.Today;
        }

        private static TimeSpan ResolveStartTime(Offering offering, DayOfWeek day, OfferingTeachingDay? teachingDay = null)
        {
            if (teachingDay?.StartTimeOverride.HasValue == true)
                return teachingDay.StartTimeOverride.Value;

            bool isWeekend = day is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var tiered = isWeekend ? offering.WeekendStartTime : offering.WeekdayStartTime;

            return tiered ?? offering.StartTime;
        }

        // Shared mapping used by every list/timetable method.
        private static SessionListItemViewModel MapToListItem(Session s)
        {
            var effectiveTutor = s.OverrideTutor ?? s.Offering.Tutor;
            return new SessionListItemViewModel
            {
                SessionId = s.SessionId,
                OfferingId = s.OfferingId,
                SubjectName = s.Offering.Subject.SubjectName,
                Grade = s.Offering.Grade,
                TutorName = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
                Date = s.Date,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Status = s.Status,
                JitsiLink = s.JitsiLink,
                DeliveryMethod = s.Offering.DeliveryMethod,
                Venue = s.Venue,
                UnmarkedCount = s.Attendance?.Count(a => !a.IsMarked) ?? 0
            };
        }

        private async Task SafeNotifyAsync(int userId, NotificationType type, Dictionary<string, string> data)
        {
            try
            {
                await _notificationService.SendAsync(userId, type, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification failed for user {UserId}, type {Type}", userId, type);
            }
        }

        // Creates the attendance roster from Active enrollments. Safe to call repeatedly:
        // only adds learners who don't already have a row. Rows start unmarked.
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

            foreach (var learnerId in activeLearnerIds.Except(existing))
            {
                _context.SessionAttendances.Add(new SessionAttendance
                {
                    SessionId = session.SessionId,
                    LearnerUserId = learnerId
                });
            }
        }

        // ───────────────────────── Generation ─────────────────────────

        public async Task<List<SubjectDropdownItem>> GetActiveSubjectsAsync()
        {
            return await _context.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.SubjectName)
                .Select(s => new SubjectDropdownItem { SubjectId = s.SubjectId, SubjectName = s.SubjectName })
                .ToListAsync();
        }

        public async Task<List<MatchingOfferingItem>> GetMatchingOfferingsAsync(int subjectId, Grade grade)
        {
            var offerings = await _context.Offerings
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .Where(o => o.SubjectId == subjectId && o.Grade == grade && o.IsActive)
                .ToListAsync();

            return offerings.Select(o => new MatchingOfferingItem
            {
                OfferingId = o.OfferingId,
                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).ToList(),
                DeliveryMethod = o.DeliveryMethod
            }).ToList();
        }

        public async Task<GenerationPreviewViewModel> PreviewGenerationAsync(GenerateSessionsRequestViewModel request)
        {
            var result = new GenerationPreviewViewModel();

            var offerings = await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .Where(o => request.SelectedOfferingIds.Contains(o.OfferingId))
                .ToListAsync();

            foreach (var offering in offerings)
            {
                var teachingDaysByDow = offering.TeachingDays.ToDictionary(td => td.DayOfWeek);

                var rangeStart = request.StartDate.Date > offering.StartDate.Date ? request.StartDate.Date : offering.StartDate.Date;
                var rangeEnd = request.EndDate.Date < offering.EndDate.Date ? request.EndDate.Date : offering.EndDate.Date;

                for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
                {
                    if (!teachingDaysByDow.TryGetValue(date.DayOfWeek, out var teachingDay))
                        continue;

                    var startTime = ResolveStartTime(offering, date.DayOfWeek, teachingDay);
                    var endTime = startTime.Add(TimeSpan.FromMinutes(offering.DurationMinutes));

                    var validation = await ValidateSessionSlotAsync(offering, date, startTime, endTime);

                    var candidate = new SessionCandidateViewModel
                    {
                        OfferingId = offering.OfferingId,
                        SubjectName = offering.Subject.SubjectName,
                        Grade = offering.Grade,
                        TutorName = $"{offering.Tutor.User.FirstName} {offering.Tutor.User.LastName}",
                        Date = date,
                        StartTime = startTime,
                        EndTime = endTime,
                        IsValid = validation.IsValid,
                        Error = validation.Error
                    };

                    (candidate.IsValid ? result.ValidCandidates : result.InvalidCandidates).Add(candidate);
                }
            }

            return result;
        }

        public async Task<(int Created, int Skipped)> ConfirmGenerationAsync(List<SessionCandidateViewModel> candidates)
        {
            var created = 0;
            var skipped = 0;

            var offeringIds = candidates.Select(c => c.OfferingId).Distinct().ToList();
            var offerings = await _context.Offerings
                .Include(o => o.TeachingDays)
                .Where(o => offeringIds.Contains(o.OfferingId))
                .ToDictionaryAsync(o => o.OfferingId);

            foreach (var candidate in candidates)
            {
                if (!offerings.TryGetValue(candidate.OfferingId, out var offering))
                {
                    skipped++;
                    continue;
                }

                var validation = await ValidateSessionSlotAsync(offering, candidate.Date, candidate.StartTime, candidate.EndTime);

                if (!validation.IsValid)
                {
                    skipped++;
                    continue;
                }

                var session = new Session
                {
                    OfferingId = offering.OfferingId,
                    Date = candidate.Date.Date,
                    StartTime = candidate.StartTime,
                    EndTime = candidate.EndTime,
                    Status = SessionStatus.Scheduled,
                    JitsiLink = NeedsMeetingLink(offering)
                        ? GenerateJitsiLink(offering.OfferingId, candidate.Date)
                        : null
                };

                _context.Sessions.Add(session);
                created++;
            }

            await _context.SaveChangesAsync();
            return (created, skipped);
        }

        // Automatic generation entry point, called right after an Offering is created.
        public async Task<(int Created, int Skipped)> GenerateSessionsForOfferingAsync(int offeringId)
        {
            var offering = await _context.Offerings.FindAsync(offeringId);
            if (offering == null) return (0, 0);

            var request = new GenerateSessionsRequestViewModel
            {
                SelectedOfferingIds = new List<int> { offeringId },
                StartDate = offering.StartDate,
                EndDate = offering.EndDate
            };

            var preview = await PreviewGenerationAsync(request);
            return await ConfirmGenerationAsync(preview.ValidCandidates);
        }

        public async Task<(int Removed, int Created, int Skipped)> RegenerateFutureSessionsForOfferingAsync(int offeringId)
        {
            var offering = await _context.Offerings.FindAsync(offeringId);
            if (offering == null) return (0, 0, 0);

            var today = DateTime.Today;

            var staleSessions = await _context.Sessions
                .Where(s => s.OfferingId == offeringId
                         && s.Date.Date >= today
                         && (s.Status == SessionStatus.Scheduled || s.Status == SessionStatus.Rescheduled))
                .ToListAsync();

            _context.Sessions.RemoveRange(staleSessions);
            await _context.SaveChangesAsync();

            var regenStart = offering.StartDate.Date > today ? offering.StartDate.Date : today;
            if (regenStart > offering.EndDate.Date)
                return (staleSessions.Count, 0, 0); // offering's window has already fully passed

            var request = new GenerateSessionsRequestViewModel
            {
                SelectedOfferingIds = new List<int> { offeringId },
                StartDate = regenStart,
                EndDate = offering.EndDate
            };

            var preview = await PreviewGenerationAsync(request);
            var (created, skipped) = await ConfirmGenerationAsync(preview.ValidCandidates);

            return (staleSessions.Count, created, skipped);
        }

        public async Task<GenerationPreviewViewModel> GetSystemTimetablePreviewAsync(int? subjectId, Grade? grade, DateTime startDate, DateTime endDate)
        {
            var query = _context.Offerings.Where(o => o.IsActive).AsQueryable();

            if (subjectId.HasValue)
                query = query.Where(o => o.SubjectId == subjectId.Value);

            if (grade.HasValue)
                query = query.Where(o => o.Grade == grade.Value);

            var offeringIds = await query.Select(o => o.OfferingId).ToListAsync();

            if (!offeringIds.Any())
                return new GenerationPreviewViewModel();

            var request = new GenerateSessionsRequestViewModel
            {
                SelectedOfferingIds = offeringIds,
                StartDate = startDate,
                EndDate = endDate
            };

            return await PreviewGenerationAsync(request);
        }

        // ───────────────────────── Tutor session flow ─────────────────────────

        public async Task<List<SessionListItemViewModel>> GetMySessionsTodayAsync(int tutorUserId)
        {
            var today = DateTime.Today;

            var sessions = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .Where(s => (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId
                            && s.Date.Date == today)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        public async Task<TutorSessionBoardViewModel> GetTutorSessionBoardAsync(int tutorUserId)
        {
            var today = DateTime.Today;

            var sessions = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .Include(s => s.Attendance)
                .Where(s => (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId
                            && s.Status != SessionStatus.Cancelled
                            && s.Date.Date >= today.AddDays(-60)
                            && s.Date.Date <= today.AddDays(30))
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToListAsync();

            bool notStarted(Session s) => s.Status == SessionStatus.Scheduled || s.Status == SessionStatus.Rescheduled;

            return new TutorSessionBoardViewModel
            {
                Today = sessions.Where(s => s.Date.Date == today).Select(MapToListItem).ToList(),
                Upcoming = sessions.Where(s => s.Date.Date > today && notStarted(s)).Select(MapToListItem).ToList(),
                NeedsWrapUp = sessions
                    .Where(s => s.Date.Date < today &&
                                (notStarted(s)
                                 || s.Status == SessionStatus.InProgress
                                 || (s.Status == SessionStatus.Completed && s.Attendance.Any(a => !a.IsMarked))))
                    .OrderByDescending(s => s.Date)
                    .Select(MapToListItem).ToList()
            };
        }

        public async Task<(bool Success, string? Error)> StartSessionAsync(int sessionId, int tutorUserId)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId
                    && (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId);

            if (session == null)
                return (false, "Session not found.");

            if (session.Status != SessionStatus.Scheduled && session.Status != SessionStatus.Rescheduled)
                return (false, $"Cannot start a session with status {session.Status}.");

            if (session.Date.Date > DateTime.Today)
                return (false, "This session is on a future date and can't be started yet.");

            session.Status = SessionStatus.InProgress;
            session.StartedAt = DateTime.UtcNow;

            // Backfills a link for Hybrid sessions generated before Hybrid got links
            if (NeedsMeetingLink(session.Offering) && string.IsNullOrEmpty(session.JitsiLink))
                session.JitsiLink = GenerateJitsiLink(session.OfferingId, session.Date);

            await EnsureRosterAsync(session);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> CompleteSessionAsync(int sessionId, int tutorUserId, CompleteSessionViewModel notes)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId
                    && (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId);

            if (session == null)
                return (false, "Session not found.");

            // Past sessions the tutor forgot to Start can still be wrapped up
            var pastUnstarted = (session.Status == SessionStatus.Scheduled || session.Status == SessionStatus.Rescheduled)
                                && session.Date.Date < DateTime.Today;

            if (session.Status != SessionStatus.InProgress && !pastUnstarted)
                return (false, session.Status == SessionStatus.Completed
                    ? "Session is already completed."
                    : "Start the session before completing it.");

            session.Status = SessionStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;
            if (notes.Notes != null) session.TutorNotes = notes.Notes;
            session.TopicsCovered = notes.TopicsCovered;
            session.Performance = notes.Performance;
            session.HomeworkAssigned = notes.HomeworkAssigned;
            session.TutorComments = notes.TutorComments;

            await EnsureRosterAsync(session);
            await _context.SaveChangesAsync();

            var effectiveTutor = session.OverrideTutor ?? session.Offering.Tutor;

            var data = new Dictionary<string, string>
            {
                ["TutorName"] = $"{effectiveTutor.User.FirstName} {effectiveTutor.User.LastName}",
                ["Subject"] = session.Offering.Subject.SubjectName,
                ["Grade"] = ((int)session.Offering.Grade).ToString(),
                ["Date"] = session.Date.ToString("dd MMM yyyy")
            };

            var adminUserIds = await _context.Set<Administrator>().Select(a => a.UserId).ToListAsync();
            foreach (var adminId in adminUserIds)
                await SafeNotifyAsync(adminId, NotificationType.SessionCompleted, data);

            return (true, null);
        }

        public async Task<SessionListItemViewModel?> GetSessionForTutorAsync(int sessionId, int tutorUserId)
        {
            var s = await _context.Sessions
                .Include(x => x.Offering).ThenInclude(o => o.Subject)
                .Include(x => x.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(x => x.OverrideTutor).ThenInclude(t => t!.User)
                .FirstOrDefaultAsync(x => x.SessionId == sessionId
                    && (x.OverrideTutorUserId ?? x.Offering.TutorUserId) == tutorUserId);

            return s == null ? null : MapToListItem(s);
        }

        // ───────────────────────── Substitutes ─────────────────────────

        public async Task<(bool Success, string? Error)> AssignOverrideTutorAsync(int sessionId, int tutorUserId)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.TeachingDays)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null)
                return (false, "Session not found.");

            if (session.Status is SessionStatus.Completed or SessionStatus.Cancelled)
                return (false, $"Cannot assign a substitute to a session with status {session.Status}.");

            var validation = await ValidateSessionSlotAsync(
                session.Offering, session.Date, session.StartTime, session.EndTime,
                tutorUserIdOverride: tutorUserId, excludeSessionId: session.SessionId);

            if (!validation.IsValid)
                return (false, validation.Error);

            session.OverrideTutorUserId = tutorUserId;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> RemoveOverrideTutorAsync(int sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session == null)
                return (false, "Session not found.");

            session.OverrideTutorUserId = null;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<TutorDropdownItem>> GetAvailableSubstitutesAsync(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.TeachingDays)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null) return new();

            var sessionDay = session.Date.DayOfWeek;

            var candidateIds = await _context.TutorAvailabilities
                .Where(a => a.DayOfWeek == sessionDay
                            && a.StartTime <= session.StartTime
                            && a.EndTime >= session.EndTime
                            && a.TutorUserId != session.Offering.TutorUserId)
                .Select(a => a.TutorUserId)
                .Distinct()
                .ToListAsync();

            var tutors = await _context.Tutors
                .Include(t => t.User)
                .Where(t => candidateIds.Contains(t.UserId) && t.IsActive)
                .ToListAsync();

            var result = new List<TutorDropdownItem>();
            foreach (var t in tutors)
            {
                var validation = await ValidateSessionSlotAsync(
                    session.Offering, session.Date, session.StartTime, session.EndTime,
                    tutorUserIdOverride: t.UserId, excludeSessionId: session.SessionId);

                if (validation.IsValid)
                    result.Add(new TutorDropdownItem { TutorUserId = t.UserId, FullName = $"{t.User.FirstName} {t.User.LastName}" });
            }

            return result.OrderBy(t => t.FullName).ToList();
        }

        // ───────────────────────── Timetables ─────────────────────────

        public async Task<List<SessionListItemViewModel>> GetTutorTimetableAsync(int tutorUserId, DateTime startDate, DateTime endDate)
        {
            var sessions = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .Where(s => (s.OverrideTutorUserId ?? s.Offering.TutorUserId) == tutorUserId
                            && s.Date.Date >= startDate.Date
                            && s.Date.Date <= endDate.Date
                            && s.Status != SessionStatus.Cancelled)
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        public async Task<List<SessionListItemViewModel>> GetLearnerTimetableAsync(int learnerUserId, DateTime startDate, DateTime endDate)
        {
            var enrollments = await _enrollmentService.GetForLearnerAsync(learnerUserId);
            var activeOfferingIds = enrollments
                .Where(e => e.Status == EnrollmentStatus.Active)
                .Select(e => e.OfferingId)
                .ToHashSet();

            if (!activeOfferingIds.Any())
                return new List<SessionListItemViewModel>();

            var sessions = await _context.Sessions
                .Include(s => s.Offering).ThenInclude(o => o.Subject)
                .Include(s => s.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
                .Include(s => s.OverrideTutor).ThenInclude(t => t!.User)
                .Where(s => activeOfferingIds.Contains(s.OfferingId)
                            && s.Date.Date >= startDate.Date
                            && s.Date.Date <= endDate.Date
                            && s.Status != SessionStatus.Cancelled)
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToListAsync();

            return sessions.Select(MapToListItem).ToList();
        }

        public async Task<List<LearnerTimetableViewModel>> GetGuardianTimetableAsync(int guardianUserId, DateTime startDate, DateTime endDate)
        {
            var enrollments = await _enrollmentService.GetForGuardianAsync(guardianUserId);

            var byLearner = enrollments
                .GroupBy(e => e.LearnerUserId)
                .ToList();

            var result = new List<LearnerTimetableViewModel>();

            foreach (var group in byLearner)
            {
                var learnerUserId = group.Key;
                var learnerName = $"{group.First().Learner.User.FirstName} {group.First().Learner.User.LastName}";

                var sessions = await GetLearnerTimetableAsync(learnerUserId, startDate, endDate);

                result.Add(new LearnerTimetableViewModel
                {
                    LearnerUserId = learnerUserId,
                    LearnerName = learnerName,
                    Sessions = sessions
                });
            }

            return result.OrderBy(r => r.LearnerName).ToList();
        }
    }
}