//using Microsoft.EntityFrameworkCore;
//using Tutor_Manager.Models;
//using Tutor_Manager.Models.Enums;
//using Tutor_Manager.ViewModels;

//namespace Tutor_Manager.Services
//{
//    public class EnrolmentService : IEnrolmentService
//    {
//        private readonly Tutor_ManagerDatabaseContext _context;

//        public EnrolmentService(Tutor_ManagerDatabaseContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<EnrolmentListItemViewModel>> GetAllEnrolmentsAsync()
//        {
//            var enrolments = await _context.Enrolments
//                .Include(e => e.Learner).ThenInclude(l => l.User)
//                .Include(e => e.Offering).ThenInclude(o => o.Subject)
//                .Include(e => e.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .OrderByDescending(e => e.CreatedAt)
//                .ToListAsync();

//            return enrolments.Select(MapToListItem).ToList();
//        }

//        public async Task<EnrollLearnerViewModel> GetEnrollFormDataAsync()
//        {
//            return new EnrollLearnerViewModel
//            {
//                AvailableLearners = await GetLearnerDropdownAsync(),
//                AvailableOfferings = await GetOfferingDropdownAsync()
//            };
//        }

//        public async Task<(bool Success, string? Error, int? EnrolmentId)> EnrollLearnerAsync(EnrollLearnerViewModel model)
//        {
//            var offering = await _context.Offerings.FirstOrDefaultAsync(o => o.OfferingId == model.OfferingId);
//            if (offering == null)
//                return (false, "Offering not found.", null);

//            if (!offering.IsActive)
//                return (false, "Cannot enroll into an inactive offering.", null);

//            var learner = await _context.Learners.FirstOrDefaultAsync(l => l.UserId == model.LearnerUserId);
//            if (learner == null)
//                return (false, "Learner not found.", null);

//            // Duplicate check — same learner already actively enrolled in this offering
//            var alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
//                e.LearnerUserId == model.LearnerUserId &&
//                e.OfferingId == model.OfferingId &&
//                (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));

//            if (alreadyEnrolled)
//                return (false, "This learner is already enrolled in this offering.", null);

//            // Capacity check
//            var activeCount = await _context.Enrolments.CountAsync(e =>
//                e.OfferingId == model.OfferingId &&
//                (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));

//            if (activeCount >= offering.Capacity)
//                return (false, $"This offering is at capacity ({offering.Capacity}/{offering.Capacity}).", null);

//            var enrolment = new Enrollment
//            {
//                LearnerUserId = model.LearnerUserId,
//                OfferingId = model.OfferingId,
//                StartDate = model.StartDate,
//                Status = EnrollmentStatus.Active,
//                Price = offering.Price, // snapshot at enrolment time
//                CreatedAt = DateTime.Now
//            };

//            _context.Enrolments.Add(enrolment);
//            await _context.SaveChangesAsync();

//            return (true, null, enrolment.EnrolmentId);
//        }

//        public async Task<(bool Success, string? Error)> CancelEnrolmentAsync(int enrolmentId, string reason)
//        {
//            var enrolment = await _context.Enrolments.FindAsync(enrolmentId);
//            if (enrolment == null)
//                return (false, "Enrolment not found.");

//            if (enrolment.Status == EnrollmentStatus.Cancelled)
//                return (false, "Enrolment is already cancelled.");

//            enrolment.Status = EnrollmentStatus.Cancelled;
//            enrolment.CancellationReason = reason;
//            enrolment.EndDate = DateTime.Today;
//            await _context.SaveChangesAsync();

//            return (true, null);
//        }

//        public async Task<List<EnrolmentListItemViewModel>> GetEnrolmentsForLearnerAsync(int learnerUserId)
//        {
//            var enrolments = await _context.Enrolments
//                .Include(e => e.Learner).ThenInclude(l => l.User)
//                .Include(e => e.Offering).ThenInclude(o => o.Subject)
//                .Include(e => e.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Where(e => e.LearnerUserId == learnerUserId)
//                .ToListAsync();

//            return enrolments.Select(MapToListItem).ToList();
//        }

//        public async Task<List<EnrolmentListItemViewModel>> GetEnrolmentsForOfferingAsync(int offeringId)
//        {
//            var enrolments = await _context.Enrolments
//                .Include(e => e.Learner).ThenInclude(l => l.User)
//                .Include(e => e.Offering).ThenInclude(o => o.Subject)
//                .Include(e => e.Offering).ThenInclude(o => o.Tutor).ThenInclude(t => t.User)
//                .Where(e => e.OfferingId == offeringId)
//                .ToListAsync();

//            return enrolments.Select(MapToListItem).ToList();
//        }

//        private static EnrolmentListItemViewModel MapToListItem(Enrollment e) => new()
//        {
//            EnrolmentId = e.EnrolmentId,
//            LearnerName = $"{e.Learner.User.FirstName} {e.Learner.User.LastName}",
//            SubjectName = e.Offering.Subject.SubjectName,
//            Grade = e.Offering.Grade,
//            TutorName = $"{e.Offering.Tutor.User.FirstName} {e.Offering.Tutor.User.LastName}",
//            StartDate = e.StartDate,
//            EndDate = e.EndDate,
//            Status = e.Status,
//            Price = e.Price
//        };

//        private async Task<List<LearnerDropdownItem>> GetLearnerDropdownAsync()
//        {
//            var learners = await _context.Learners.Include(l => l.User).ToListAsync();
//            return learners
//                .Select(l => new LearnerDropdownItem
//                {
//                    LearnerUserId = l.UserId,
//                    FullName = $"{l.User.FirstName} {l.User.LastName}",
//                    GradeLevel = (Grade?)l.GradeLevel
//                })
//                .OrderBy(l => l.FullName)
//                .ToList();
//        }

//        private async Task<List<OfferingDropdownItem>> GetOfferingDropdownAsync()
//        {
//            var offerings = await _context.Offerings
//                .Include(o => o.Subject)
//                .Include(o => o.Tutor).ThenInclude(t => t.User)
//                .Where(o => o.IsActive)
//                .ToListAsync();

//            var result = new List<OfferingDropdownItem>();
//            foreach (var o in offerings)
//            {
//                var count = await _context.Enrolments.CountAsync(e =>
//                    e.OfferingId == o.OfferingId &&
//                    (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));

//                result.Add(new OfferingDropdownItem
//                {
//                    OfferingId = o.OfferingId,
//                    SubjectName = o.Subject.SubjectName,
//                    Grade = o.Grade,
//                    TutorName = $"{o.Tutor.User.FirstName} {o.Tutor.User.LastName}",
//                    Capacity = o.Capacity,
//                    CurrentEnrolments = count
//                });
//            }

//            return result.OrderBy(o => o.SubjectName).ThenBy(o => o.Grade).ToList();
//        }
//    }
//}