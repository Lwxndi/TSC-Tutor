using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services.Academic;
using Tutor_Manager.ViewModels;

namespace Tutor_Manager.Services
{
    public class OfferingService : IOfferingService
    {
        private readonly Tutor_ManagerDatabaseContext _context;

        public OfferingService(Tutor_ManagerDatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<OfferingListItemViewModel>> GetAllOfferingsAsync()
        {
            var offerings = await _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .ToListAsync();

            return offerings.Select(o => new OfferingListItemViewModel
            {
                OfferingId = o.OfferingId,
                SubjectName = o.Subject.SubjectName,
                Grade = o.Grade,
                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
                Type = o.Type,
                Capacity = o.Capacity,
                DeliveryMethod = o.DeliveryMethod,
                Price = o.Price,
                IsActive = o.IsActive,
                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).OrderBy(d => d).ToList()
            }).OrderBy(o => o.SubjectName).ToList();
        }

        public async Task<List<OfferingListItemViewModel>> GetAllOfferingsAsync(OfferingFilterViewModel? filter = null)
        {
            var query = _context.Offerings
                .Include(o => o.Subject)
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.SubjectId.HasValue)
                    query = query.Where(o => o.SubjectId == filter.SubjectId.Value);

                if (filter.Grade.HasValue)
                    query = query.Where(o => o.Grade == filter.Grade.Value);

                if (filter.TutorUserId.HasValue)
                    query = query.Where(o => o.TutorUserId == filter.TutorUserId.Value);

                if (filter.Type.HasValue)
                    query = query.Where(o => o.Type == filter.Type.Value);

                if (filter.DeliveryMethod.HasValue)
                    query = query.Where(o => o.DeliveryMethod == filter.DeliveryMethod.Value);

                if (filter.IsActive.HasValue)
                    query = query.Where(o => o.IsActive == filter.IsActive.Value);
            }

            var offerings = await query.ToListAsync();

            return offerings.Select(o => new OfferingListItemViewModel
            {
                OfferingId = o.OfferingId,
                SubjectName = o.Subject.SubjectName,
                Grade = o.Grade,
                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
                Type = o.Type,
                Capacity = o.Capacity,
                DeliveryMethod = o.DeliveryMethod,
                Price = o.Price,
                IsActive = o.IsActive,
                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).OrderBy(d => d).ToList()
            }).OrderBy(o => o.SubjectName).ToList();
        }

        public async Task<List<TutorDropdownItem>> GetAllTutorsForFilterAsync()
        {
            var tutors = await _context.Tutors
                .Include(t => t.User)
                .ToListAsync();

            return tutors
                .Select(t => new TutorDropdownItem { TutorUserId = t.UserId, FullName = $"{t.User.FirstName} {t.User.LastName}" })
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public async Task<OfferingEditViewModel> GetCreateFormDataAsync()
        {
            return new OfferingEditViewModel
            {
                AvailableSubjects = await GetSubjectDropdownAsync(),
                AvailableTutors = await GetTutorDropdownAsync()
            };
        }

        public async Task<OfferingEditViewModel?> GetForEditAsync(int offeringId)
        {
            var offering = await _context.Offerings
                .Include(o => o.TeachingDays)
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

            if (offering == null) return null;

            return new OfferingEditViewModel
            {
                OfferingId = offering.OfferingId,
                SubjectId = offering.SubjectId,
                Grade = offering.Grade,
                TutorUserId = offering.TutorUserId,
                Type = offering.Type,
                Capacity = offering.Capacity,
                DeliveryMethod = offering.DeliveryMethod,
                DurationMinutes = offering.DurationMinutes,
                Price = offering.Price,
                IsActive = offering.IsActive,
                StartDate = offering.StartDate,
                EndDate = offering.EndDate,
                BillingType = offering.BillingType,
                StartTime = offering.StartTime,
                WeekdayStartTime = offering.WeekdayStartTime,
                WeekendStartTime = offering.WeekendStartTime,
                SelectedTeachingDays = offering.TeachingDays.Select(td => td.DayOfWeek).ToList(),
                DayOverrides = offering.TeachingDays
                    .Where(td => td.StartTimeOverride.HasValue)
                    .ToDictionary(td => td.DayOfWeek, td => td.StartTimeOverride),
                AvailableSubjects = await GetSubjectDropdownAsync(),
                AvailableTutors = await GetTutorDropdownAsync()
            };
        }

        public async Task<(bool Success, string? Error, int? OfferingId)> CreateOfferingAsync(OfferingEditViewModel model)
        {
            var (isValid, error) = await ValidateAsync(model);
            if (!isValid) return (false, error, null);

            var offering = new Offering
            {
                SubjectId = model.SubjectId,
                Grade = model.Grade,
                TutorUserId = model.TutorUserId,
                Type = model.Type,
                Capacity = model.Type == OfferingType.OneOnOne ? 1 : model.Capacity,
                DeliveryMethod = model.DeliveryMethod,
                DurationMinutes = model.DurationMinutes,
                Price = model.Price,
                IsActive = model.IsActive,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                BillingType = model.BillingType,
                StartTime = ResolveFallbackStartTime(model),
                WeekdayStartTime = model.WeekdayStartTime,
                WeekendStartTime = model.WeekendStartTime,
            };

            foreach (var day in model.SelectedTeachingDays)
            {
                offering.TeachingDays.Add(new OfferingTeachingDay
                {
                    DayOfWeek = day,
                    StartTimeOverride = model.DayOverrides.GetValueOrDefault(day)
                });
            }

            _context.Offerings.Add(offering);
            await _context.SaveChangesAsync();

            return (true, null, offering.OfferingId);
        }

        public async Task<(bool Success, string? Error)> UpdateOfferingAsync(OfferingEditViewModel model)
        {
            var (isValid, error) = await ValidateAsync(model);
            if (!isValid) return (false, error);

            var offering = await _context.Offerings
                .Include(o => o.TeachingDays)
                .FirstOrDefaultAsync(o => o.OfferingId == model.OfferingId);

            if (offering == null)
                return (false, "Offering not found.");

            offering.SubjectId = model.SubjectId;
            offering.Grade = model.Grade;
            offering.TutorUserId = model.TutorUserId;
            offering.Type = model.Type;
            offering.Capacity = model.Type == OfferingType.OneOnOne ? 1 : model.Capacity;
            offering.DeliveryMethod = model.DeliveryMethod;
            offering.DurationMinutes = model.DurationMinutes;
            offering.Price = model.Price;
            offering.IsActive = model.IsActive;
            offering.StartDate = model.StartDate;
            offering.EndDate = model.EndDate;
            offering.BillingType = model.BillingType;
            offering.StartTime = ResolveFallbackStartTime(model);
            offering.WeekdayStartTime = model.WeekdayStartTime;
            offering.WeekendStartTime = model.WeekendStartTime;

            var selectedDays = model.SelectedTeachingDays.ToHashSet();
            var currentDays = offering.TeachingDays.Select(td => td.DayOfWeek).ToHashSet();

            offering.TeachingDays
                .Where(td => !selectedDays.Contains(td.DayOfWeek))
                .ToList()
                .ForEach(td => _context.Remove(td));

            foreach (var day in selectedDays.Except(currentDays))
            {
                offering.TeachingDays.Add(new OfferingTeachingDay
                {
                    OfferingId = offering.OfferingId,
                    DayOfWeek = day,
                    StartTimeOverride = model.DayOverrides.GetValueOrDefault(day)
                });
            }

            // Days that stayed selected still need their override synced —
            // the loop above only handles newly-added days.
            foreach (var teachingDay in offering.TeachingDays.Where(td => selectedDays.Contains(td.DayOfWeek)))
            {
                teachingDay.StartTimeOverride = model.DayOverrides.GetValueOrDefault(teachingDay.DayOfWeek);
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<string> SetOfferingActiveStatusAsync(int offeringId, bool isActive)
        {
            var offering = await _context.Offerings
                .Include(o => o.Subject)
                .FirstOrDefaultAsync(o => o.OfferingId == offeringId);

            if (offering == null)
                throw new InvalidOperationException($"Offering {offeringId} not found.");

            offering.IsActive = isActive;
            await _context.SaveChangesAsync();

            return offering.Subject.SubjectName;
        }

        public async Task<List<string>> GetValidStartTimesAsync(
            DeliveryMethod deliveryMethod, OfferingType type, List<DayOfWeek> days, int durationMinutes, int stepMinutes = 15)
        {
            var dayTypes = days
                .Select(d => (d == DayOfWeek.Saturday || d == DayOfWeek.Sunday) ? DayType.Weekend : DayType.Weekday)
                .Distinct().ToList();

            var windows = await _context.OfferingTimeWindows
                .Where(w => w.DeliveryMethod == deliveryMethod && w.OfferingType == type && dayTypes.Contains(w.DayType))
                .ToListAsync();

            if (windows.Count != dayTypes.Count) return new List<string>();

            var start = windows.Max(w => w.WindowStart);
            var end = windows.Min(w => w.WindowEnd);

            var options = new List<string>();
            for (var t = start; t.Add(TimeSpan.FromMinutes(durationMinutes)) <= end; t = t.Add(TimeSpan.FromMinutes(stepMinutes)))
                options.Add(t.ToString(@"hh\:mm"));

            return options;
        }

        private static TimeSpan ResolveStartTimeForDay(OfferingEditViewModel model, DayOfWeek day)
        {
            if (model.DayOverrides.TryGetValue(day, out var overrideTime) && overrideTime.HasValue)
                return overrideTime.Value;

            bool isWeekend = day is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var tiered = isWeekend ? model.WeekendStartTime : model.WeekdayStartTime;

            return tiered ?? model.StartTime;
        }

        private static TimeSpan ResolveFallbackStartTime(OfferingEditViewModel model)
        {
            var firstDay = model.SelectedTeachingDays.FirstOrDefault();
            return model.SelectedTeachingDays.Any()
                ? ResolveStartTimeForDay(model, firstDay)
                : model.StartTime;
        }

        // ---- Tutor-load / TSC settings enforcement ----

        private async Task<TscSettings> GetSettingsAsync()
        {
            return await _context.TscSettings.FirstOrDefaultAsync() ?? new TscSettings();
        }

        // Checks a proposed offering (create or edit) against the tutor's existing
        // active offerings and the configured TscSettings caps. Used both at
        // save time (via ValidateAsync) and by the live pre-submit check endpoint.
        // excludeOfferingId lets an edit ignore its own prior state when comparing.
        public async Task<(bool IsValid, string? Error)> ValidateTutorLoadAsync(
            int tutorUserId, int subjectId, OfferingType type, DeliveryMethod deliveryMethod,
            List<DayOfWeek> days, Dictionary<DayOfWeek, TimeSpan?> dayOverrides,
            TimeSpan? weekdayStartTime, TimeSpan? weekendStartTime, TimeSpan legacyStartTime,
            int durationMinutes, int? excludeOfferingId)
        {
            if (tutorUserId <= 0 || !days.Any())
                return (true, null); // nothing meaningful to check yet

            var settings = await GetSettingsAsync();

            var otherOfferings = await _context.Offerings
                .Include(o => o.TeachingDays)
                .Where(o => o.TutorUserId == tutorUserId
                         && o.IsActive
                         && (excludeOfferingId == null || o.OfferingId != excludeOfferingId))
                .ToListAsync();

            TimeSpan ResolveDay(DayOfWeek d)
            {
                if (dayOverrides.TryGetValue(d, out var ov) && ov.HasValue) return ov.Value;
                bool weekend = d is DayOfWeek.Saturday or DayOfWeek.Sunday;
                return (weekend ? weekendStartTime : weekdayStartTime) ?? legacyStartTime;
            }

            var newDuration = TimeSpan.FromMinutes(durationMinutes);

            foreach (var day in days)
            {
                var sameDay = otherOfferings
                    .Where(o => o.TeachingDays.Any(td => td.DayOfWeek == day))
                    .ToList();

                if (sameDay.Count + 1 > settings.MaxSessionsPerTutorPerDay)
                    return (false, $"This tutor would exceed {settings.MaxSessionsPerTutorPerDay} sessions on {day}.");

                var subjectsThatDay = sameDay.Select(o => o.SubjectId).Distinct().ToHashSet();
                subjectsThatDay.Add(subjectId);
                if (subjectsThatDay.Count > settings.MaxSubjectsPerTutorPerDay)
                    return (false, $"This tutor would exceed {settings.MaxSubjectsPerTutorPerDay} subjects on {day}.");

                var minutesThatDay = sameDay.Sum(o => o.DurationMinutes) + durationMinutes;
                if (minutesThatDay > settings.MaxTutoringHoursPerTutorPerDay * 60)
                    return (false, $"This tutor would exceed {settings.MaxTutoringHoursPerTutorPerDay} teaching hours on {day}.");

                var newStart = ResolveDay(day);
                var newEnd = newStart.Add(newDuration);
                var buffer = TimeSpan.FromMinutes(settings.MinBreakBetweenSessionsMinutes);

                foreach (var existing in sameDay)
                {
                    var td = existing.TeachingDays.First(t => t.DayOfWeek == day);
                    var exStart = td.StartTimeOverride
                        ?? (day is DayOfWeek.Saturday or DayOfWeek.Sunday ? existing.WeekendStartTime : existing.WeekdayStartTime)
                        ?? existing.StartTime;
                    var exEnd = exStart.Add(TimeSpan.FromMinutes(existing.DurationMinutes));

                    bool tooClose = newStart < exEnd.Add(buffer) && exStart < newEnd.Add(buffer);
                    if (tooClose)
                        return (false, $"This tutor is already booked {exStart:hh\\:mm}–{exEnd:hh\\:mm} on {day} (needs a {settings.MinBreakBetweenSessionsMinutes} min gap).");
                }
            }

            var allSubjects = otherOfferings.Select(o => o.SubjectId).Distinct().ToHashSet();
            allSubjects.Add(subjectId);
            if (allSubjects.Count > settings.MaxSubjectsPerTutorPerWeek)
                return (false, $"This tutor would exceed {settings.MaxSubjectsPerTutorPerWeek} subjects for the week.");

            var weeklyMinutes = otherOfferings.Sum(o => o.DurationMinutes * o.TeachingDays.Count)
                              + durationMinutes * days.Count;
            if (weeklyMinutes > settings.MaxWeeklyTutoringHours * 60)
                return (false, $"This tutor would exceed {settings.MaxWeeklyTutoringHours} hours for the week.");

            return (true, null);
        }

        private async Task<(bool IsValid, string? Error)> ValidateAsync(OfferingEditViewModel model)
        {
            var subjectGradeOffered = await _context.SubjectGrades
                .AnyAsync(sg => sg.SubjectId == model.SubjectId && sg.Grade == model.Grade);

            if (!subjectGradeOffered)
                return (false, "TSC does not offer this subject at this grade.");

            var tutorApproved = await _context.TutorSubjects
                .AnyAsync(ts => ts.TutorUserId == model.TutorUserId
                              && ts.SubjectId == model.SubjectId
                              && ts.GradeLevel == model.Grade);

            if (!tutorApproved)
                return (false, "This tutor is not approved to teach this subject at this grade.");

            if (!model.SelectedTeachingDays.Any())
                return (false, "At least one teaching day must be selected.");

            if (model.EndDate < model.StartDate)
                return (false, "End date cannot be earlier than start date.");

            if (model.Type == OfferingType.OneOnOne && model.Capacity != 1)
                model.Capacity = 1;

            if (model.Type != OfferingType.OneOnOne && model.Capacity > 25)
                return (false, "Capacity cannot exceed 25 learners per session.");

            // Safeguarding rule: Physical One-on-One is never permitted, no exceptions.
            if (model.DeliveryMethod == DeliveryMethod.Physical && model.Type == OfferingType.OneOnOne)
                return (false, "One-on-one tutoring cannot be delivered in person. Please select Online delivery.");

            if (model.DeliveryMethod == DeliveryMethod.Hybrid && model.Type == OfferingType.OneOnOne)
                return (false, "Hybrid delivery only applies to Group offerings. One-on-one sessions must be Physical or Remote.");

            var dayTypesSelected = model.SelectedTeachingDays
                .Select(d => (d == DayOfWeek.Saturday || d == DayOfWeek.Sunday) ? DayType.Weekend : DayType.Weekday)
                .Distinct()
                .ToList();

            var windows = await _context.OfferingTimeWindows
                .Where(w => w.DeliveryMethod == model.DeliveryMethod
                         && w.OfferingType == model.Type
                         && dayTypesSelected.Contains(w.DayType))
                .ToListAsync();

            var windowByType = new Dictionary<DayType, OfferingTimeWindow>();
            foreach (var dayType in dayTypesSelected)
            {
                var window = windows.FirstOrDefault(w => w.DayType == dayType);
                if (window == null)
                    return (false, $"No configured time window for {dayType} sessions with this delivery/type combination.");
                windowByType[dayType] = window;
            }

            var duration = TimeSpan.FromMinutes(model.DurationMinutes);

            foreach (var day in model.SelectedTeachingDays)
            {
                var dayType = (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) ? DayType.Weekend : DayType.Weekday;
                var window = windowByType[dayType];
                var startTime = ResolveStartTimeForDay(model, day);
                var proposedEnd = startTime.Add(duration);

                if (startTime < window.WindowStart || proposedEnd > window.WindowEnd)
                    return (false, $"Can't book a session at {startTime:hh\\:mm} on {day} ({dayType}) — allowed window is {window.WindowStart:hh\\:mm}–{window.WindowEnd:hh\\:mm}.");
            }

            // Tutor-load / TSC settings check — was previously missing entirely.
            var (loadOk, loadError) = await ValidateTutorLoadAsync(
                model.TutorUserId, model.SubjectId, model.Type, model.DeliveryMethod,
                model.SelectedTeachingDays, model.DayOverrides, model.WeekdayStartTime,
                model.WeekendStartTime, model.StartTime, model.DurationMinutes,
                model.OfferingId == 0 ? null : model.OfferingId);

            if (!loadOk) return (false, loadError);

            return (true, null);
        }

        public async Task<List<SubjectDropdownItem>> GetSubjectDropdownAsync()
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

        public async Task<string?> GetTutorEmailAsync(int tutorUserId)
        {
            var tutor = await _context.Tutors
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == tutorUserId);

            return tutor?.User.Email;
        }

        public async Task<List<TutorDropdownItem>> GetEligibleTutorsAsync(int subjectId, Grade? grade)
        {
            var query = _context.TutorSubjects
                .Where(ts => ts.SubjectId == subjectId);

            if (grade.HasValue)
            {
                query = query.Where(ts => ts.GradeLevel == grade.Value);
            }

            var tutorIds = await query
                .Select(ts => ts.TutorUserId)
                .Distinct()
                .ToListAsync();

            var tutors = await _context.Tutors
                .Include(t => t.User)
                .Where(t => tutorIds.Contains(t.UserId) && t.IsActive)
                .ToListAsync();

            return tutors
                .Select(t => new TutorDropdownItem { TutorUserId = t.UserId, FullName = $"{t.User.FirstName} {t.User.LastName}" })
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public async Task<List<OfferingListItemViewModel>> GetOfferingsForSubjectGradeAsync(int subjectId, Grade grade)
        {
            var offerings = await _context.Offerings
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .Where(o => o.SubjectId == subjectId && o.Grade == grade)
                .ToListAsync();

            return offerings.Select(o => new OfferingListItemViewModel
            {
                OfferingId = o.OfferingId,
                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
                Type = o.Type,
                DeliveryMethod = o.DeliveryMethod,
                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).OrderBy(d => d).ToList(),
                IsActive = o.IsActive
            }).ToList();
        }

        public async Task<List<OfferingListItemViewModel>> GetOfferingsForSubjectAsync(int subjectId)
        {
            var offerings = await _context.Offerings
                .Include(o => o.Tutor).ThenInclude(t => t.User)
                .Include(o => o.TeachingDays)
                .Where(o => o.SubjectId == subjectId)
                .ToListAsync();

            return offerings.Select(o => new OfferingListItemViewModel
            {
                OfferingId = o.OfferingId,
                Grade = o.Grade,
                TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
                Type = o.Type,
                DeliveryMethod = o.DeliveryMethod,
                TeachingDays = o.TeachingDays.Select(td => td.DayOfWeek).OrderBy(d => d).ToList(),
                IsActive = o.IsActive
            }).OrderBy(o => o.Grade).ToList();
        }
    }
}