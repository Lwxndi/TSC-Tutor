using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;

namespace Tutor_Manager.Services.DataSeeding
{
    public class TutorSeedService
    {
        private readonly Tutor_ManagerDatabaseContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public TutorSeedService(Tutor_ManagerDatabaseContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        public async Task SeedAsync()
        {
            if (await _context.Tutors.AnyAsync())
                return; // already seeded — never re-run against real data

            var seedTutors = BuildSeedData();

            foreach (var seed in seedTutors)
            {
                var user = new User
                {
                    FirstName = seed.FirstName,
                    LastName = seed.LastName,
                    Email = seed.Email,
                    PhoneNumber = seed.Phone,
                    Gender = seed.Gender,
                    PasswordHash = "PLACEHOLDER",
                    DateCreated = DateTime.Now.AddMonths(-seed.MonthsAgoJoined),
                    IsActive = true
                };
                user.PasswordHash = _hasher.HashPassword(user, "Password@01Pass");

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // need user.UserId for everything below

                _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = 2 }); // Tutor role

                var tutor = new Tutor
                {
                    UserId = user.UserId,
                    Qualification = seed.Qualification,
                    Bio = seed.Bio,
                    VettingStatus = "Approved",
                    DateApproved = DateTime.Now.AddMonths(-seed.MonthsAgoJoined),
                    AccountStatus = AccountStatus.Active,
                    TutorNumber = $"TUT{user.UserId:D8}",
                    IsActive = true
                };
                _context.Tutors.Add(tutor);

                var application = new TutorApplication
                {
                    ReferenceNumber = $"TSC-TA-2026-{user.UserId:D5}",
                    FirstName = seed.FirstName,
                    Surname = seed.LastName,
                    Email = seed.Email,
                    Phone = seed.Phone,
                    LocationPreference = seed.LocationPreference,
                    AreaCity = seed.AreaCity,
                    Achievements = seed.Achievements,
                    StrengthsSelected = seed.Strengths,
                    StrengthsNote = seed.StrengthsNote,
                    ConsentGiven = true,
                    ConsentDate = DateTime.Now.AddMonths(-seed.MonthsAgoJoined),
                    ConsentVersion = "v1",
                    Status = ApplicationStatus.Approved,
                    DateApplied = DateTime.Now.AddMonths(-seed.MonthsAgoJoined).AddDays(-14),
                    DateReviewed = DateTime.Now.AddMonths(-seed.MonthsAgoJoined),
                    ReviewedByAdminId = 1,
                    CreatedTutorId = tutor.UserId,
                    DateCreated = DateTime.Now.AddMonths(-seed.MonthsAgoJoined).AddDays(-14)
                };
                _context.TutorApplications.Add(application);
                await _context.SaveChangesAsync(); // need application.ApplicationId

                foreach (var appliedSubject in seed.AppliedSubjects)
                {
                    _context.TutorApplicationSubjects.Add(new TutorApplicationSubject
                    {
                        ApplicationId = application.ApplicationId,
                        SubjectId = appliedSubject.SubjectId,
                        GradeLevels = appliedSubject.GradeLevels,
                        CompetencyNote = appliedSubject.CompetencyNote,
                        ResultNote = appliedSubject.ResultNote
                    });
                }

                _context.TutorApplicationExperiences.Add(new TutorApplicationExperience
                {
                    ApplicationId = application.ApplicationId,
                    Institution = seed.ExperienceInstitution,
                    SubjectsTaught = seed.ExperienceSubjects,
                    GradeLevels = seed.ExperienceGrades,
                    Duration = seed.ExperienceDuration,
                    Responsibilities = seed.ExperienceResponsibilities,
                    Achievements = seed.ExperienceAchievements
                });

                _context.TutorApplicationQualifications.Add(new TutorApplicationQualification
                {
                    ApplicationId = application.ApplicationId,
                    QualificationType = seed.QualificationType,
                    Institution = seed.QualificationInstitution,
                    FieldOfStudy = seed.FieldOfStudy,
                    YearCompleted = seed.YearCompleted,
                    StudyStatus = "Completed"
                });

                foreach (var slot in seed.Availability)
                {
                    _context.TutorAvailabilities.Add(new TutorAvailability
                    {
                        TutorUserId = tutor.UserId,
                        DayOfWeek = slot.Day,
                        StartTime = slot.Start,
                        EndTime = slot.End
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private record AppliedSubjectSeed(int SubjectId, string GradeLevels, string? CompetencyNote, string? ResultNote);
        private record AvailabilitySeed(DayOfWeek Day, TimeSpan Start, TimeSpan End);

        private record TutorSeed(
            string FirstName, string LastName, string Email, string Phone, Gender Gender,
            string Qualification, string Bio, LocationPreference LocationPreference, string AreaCity,
            string? Achievements, string Strengths, string? StrengthsNote,
            List<AppliedSubjectSeed> AppliedSubjects,
            string ExperienceInstitution, string ExperienceSubjects, string ExperienceGrades,
            string ExperienceDuration, string ExperienceResponsibilities, string? ExperienceAchievements,
            string QualificationType, string QualificationInstitution, string FieldOfStudy, int YearCompleted,
            List<AvailabilitySeed> Availability, int MonthsAgoJoined
        );

        private static List<TutorSeed> BuildSeedData() => new()
        {
            new TutorSeed(
                "Sipho", "Dlamini", "sipho.dlamini@tsctutors.test", "0821234501", Gender.Male,
                "BSc Applied Mathematics (Wits)", "Passionate about making abstract maths concrete through real-world examples.",
                LocationPreference.Remote, "Johannesburg",
                "Top 5% matric results in Mathematics and Physical Sciences.",
                "Patient, Strong subject knowledge, Exam technique coaching", "Enjoys breaking down calculus for struggling learners.",
                new() {
                    new(1, "10,11,12", "Strong", "Distinction in first-year calculus"),
                    new(2, "11,12", "Good", null)
                },
                "Bright Star Tutoring Centre", "Mathematics, Physical Sciences", "10-12", "5 years",
                "Ran weekend exam-prep workshops for matric cohorts.", "Improved average pass rate by 18% over 2 years",
                "Bachelor's Degree", "University of the Witwatersrand", "Applied Mathematics", 2019,
                new() {
                    new(DayOfWeek.Monday, new(15,30,0), new(19,0,0)),
                    new(DayOfWeek.Wednesday, new(15,30,0), new(19,0,0)),
                    new(DayOfWeek.Saturday, new(8,0,0), new(13,0,0))
                }, 14
            ),
            new TutorSeed(
                "Naledi", "Mokoena", "naledi.mokoena@tsctutors.test", "0821234502", Gender.Female,
                "BSc Biological Sciences (UP)", "Believes in hands-on, diagram-driven teaching for Life Sciences.",
                LocationPreference.Remote, "Pretoria",
                null, "Creative, Diagram-based teaching, Encouraging", null,
                new() { new(3, "10,11,12", "Strong", null) },
                "Sunnyside Learning Hub", "Life Sciences", "10-12", "3 years",
                "Developed a visual-notes system for genetics and ecology topics.", null,
                "Bachelor's Degree", "University of Pretoria", "Biological Sciences", 2021,
                new() {
                    new(DayOfWeek.Tuesday, new(15,30,0), new(18,0,0)),
                    new(DayOfWeek.Thursday, new(15,30,0), new(18,0,0)),
                    new(DayOfWeek.Sunday, new(9,0,0), new(13,0,0))
                }, 9
            ),
            new TutorSeed(
                "Thabo", "Nkosi", "thabo.nkosi@tsctutors.test", "0821234503", Gender.Male,
                "BEd Mathematics (UJ)", "Qualified teacher with a classroom-tested, structured approach to Maths.",
                LocationPreference.CapeTown, "Cape Town",
                "8 years as a full-time high school Maths teacher.", "Structured lessons, Exam-focused, Firm but fair", null,
                new() { new(1, "10,11", "Strong", "Consistently produces distinction-level learners") },
                "Rondebosch High School", "Mathematics", "10-11", "8 years",
                "Full-time subject head for Mathematics.", "Mentored 3 junior teachers",
                "Bachelor's Degree", "University of Johannesburg", "Mathematics Education", 2016,
                new() {
                    new(DayOfWeek.Monday, new(16,0,0), new(19,0,0)),
                    new(DayOfWeek.Friday, new(16,0,0), new(19,0,0)),
                    new(DayOfWeek.Saturday, new(8,0,0), new(12,0,0))
                }, 20
            ),
            new TutorSeed(
                "Zanele", "Khumalo", "zanele.khumalo@tsctutors.test", "0821234504", Gender.Female,
                "BCom Accounting (UKZN)", "Focuses on practical bookkeeping examples to make Accounting click.",
                LocationPreference.Remote, "Durban",
                null, "Practical examples, Patient, Detail-oriented", "Uses real invoices/ledgers as teaching props.",
                new() { new(5, "11,12", "Strong", null) },
                "KZN Accounting Academy", "Accounting", "11-12", "4 years",
                "Tutored small groups preparing for final exams.", null,
                "Bachelor's Degree", "University of KwaZulu-Natal", "Accounting", 2020,
                new() {
                    new(DayOfWeek.Wednesday, new(15,30,0), new(18,30,0)),
                    new(DayOfWeek.Saturday, new(8,0,0), new(13,0,0))
                }, 11
            ),
            new TutorSeed(
                "Johan", "van der Merwe", "johan.vandermerwe@tsctutors.test", "0821234505", Gender.Male,
                "National Diploma: Financial Accounting (TUT)", "Specialises in helping learners who struggle with Maths Literacy build confidence.",
                LocationPreference.Remote, "Pretoria",
                null, "Confidence-building, Simple explanations, Very patient", null,
                new() {
                    new(4, "10,11,12", "Good", null),
                    new(5, "10,11", "Good", null)
                },
                "Akasia Community Learning Centre", "Mathematical Literacy, Accounting", "10-12", "6 years",
                "Ran a free weekend tutoring programme for underprivileged learners.", "Recognised by centre for community impact",
                "Diploma", "Tshwane University of Technology", "Financial Accounting", 2018,
                new() {
                    new(DayOfWeek.Tuesday, new(15,30,0), new(19,0,0)),
                    new(DayOfWeek.Thursday, new(15,30,0), new(19,0,0))
                }, 16
            ),
            new TutorSeed(
                "Amahle", "Ngcobo", "amahle.ngcobo@tsctutors.test", "0821234506", Gender.Female,
                "BSc Chemistry (Rhodes)", "Enjoys linking Physical and Life Sciences through real lab experience.",
                LocationPreference.Remote, "Makhanda",
                "Dean's list, 2nd year Chemistry.", "Lab-based examples, Enthusiastic, Cross-subject linking", null,
                new() {
                    new(2, "10,11,12", "Strong", null),
                    new(3, "10,11", "Good", null)
                },
                "Rhodes Science Outreach Programme", "Physical Sciences, Life Sciences", "10-11", "2 years",
                "Assisted first-year chemistry labs and ran school outreach demos.", null,
                "Bachelor's Degree", "Rhodes University", "Chemistry", 2023,
                new() {
                    new(DayOfWeek.Monday, new(15,30,0), new(18,0,0)),
                    new(DayOfWeek.Wednesday, new(15,30,0), new(18,0,0)),
                    new(DayOfWeek.Sunday, new(9,0,0), new(13,0,0))
                }, 5
            ),
            new TutorSeed(
                "Kagiso", "Molefe", "kagiso.molefe@tsctutors.test", "0821234507", Gender.Male,
                "BSc Statistics (Stellenbosch)", "10+ years bridging Pure Maths and Maths Literacy for a wide range of learners.",
                LocationPreference.Remote, "Stellenbosch",
                "Published tutor of the year, local tutoring network, 2022.", "Very experienced, Adaptive teaching style, Calm under pressure", null,
                new() {
                    new(1, "10,11,12", "Strong", "Multiple distinction-level learners produced"),
                    new(4, "10,11,12", "Strong", null)
                },
                "Boland Tutoring Collective", "Mathematics, Mathematical Literacy", "10-12", "10 years",
                "Senior tutor, mentored newer tutors on lesson planning.", "Tutor of the Year 2022",
                "Bachelor's Degree", "Stellenbosch University", "Statistics", 2014,
                new() {
                    new(DayOfWeek.Tuesday, new(15,30,0), new(19,0,0)),
                    new(DayOfWeek.Thursday, new(15,30,0), new(19,0,0)),
                    new(DayOfWeek.Saturday, new(8,0,0), new(13,0,0))
                }, 30
            ),
            new TutorSeed(
                "Precious", "Mahlangu", "precious.mahlangu@tsctutors.test", "0821234508", Gender.Female,
                "BSc Zoology (in progress, final year — NWU)", "Newer tutor, brings fresh energy and up-to-date curriculum knowledge to Life Sciences.",
                LocationPreference.Remote, "Potchefstroom",
                null, "Energetic, Up-to-date with curriculum, Relatable to teenagers", null,
                new() { new(3, "10,11", "Good", null) },
                "NWU Peer Tutoring Programme", "Life Sciences", "10-11", "1 year",
                "Peer tutor for first-year Life Sciences modules.", null,
                "Bachelor's Degree", "North-West University", "Zoology", 2026,
                new() {
                    new(DayOfWeek.Wednesday, new(15,30,0), new(18,0,0)),
                    new(DayOfWeek.Friday, new(15,30,0), new(18,0,0))
                }, 3
            ),
            new TutorSeed(
                "Farhan", "Osman", "farhan.osman@tsctutors.test", "0821234509", Gender.Male,
                "CA(SA) articles completed (Deloitte)", "Brings real accounting-firm experience to Accounting and Maths tutoring.",
                LocationPreference.Remote, "Johannesburg",
                "Completed SAICA articles, CA(SA) qualified.", "Professional insight, Exam strategy, High standards", null,
                new() {
                    new(5, "11,12", "Strong", "Distinction in final-year Accounting"),
                    new(1, "11,12", "Good", null)
                },
                "Deloitte South Africa", "Accounting, Mathematics (informal mentoring)", "11-12", "7 years",
                "Trained junior articles clerks; ran internal financial-literacy sessions.", "SAICA articles completed with distinction",
                "Postgraduate Diploma", "University of Cape Town", "Accounting", 2019,
                new() {
                    new(DayOfWeek.Monday, new(16,0,0), new(19,0,0)),
                    new(DayOfWeek.Saturday, new(8,0,0), new(12,0,0))
                }, 25
            ),
            new TutorSeed(
                "Lindiwe", "Zulu", "lindiwe.zulu@tsctutors.test", "0821234510", Gender.Female,
                "BSc Physics (UKZN)", "Loves demonstrating Physical Sciences concepts with simple home-made experiments.",
                LocationPreference.Remote, "Pietermaritzburg",
                null, "Hands-on demonstrations, Clear explanations, Encouraging", "Uses household items to demonstrate physics principles.",
                new() { new(2, "10,11,12", "Strong", null) },
                "Maritzburg Science Tutors", "Physical Sciences", "10-12", "5 years",
                "Designed a low-cost home-experiment kit for remote learners.", null,
                "Bachelor's Degree", "University of KwaZulu-Natal", "Physics", 2020,
                new() {
                    new(DayOfWeek.Tuesday, new(15,30,0), new(18,30,0)),
                    new(DayOfWeek.Thursday, new(15,30,0), new(18,30,0)),
                    new(DayOfWeek.Sunday, new(9,0,0), new(13,0,0))
                }, 12
            )
        };
    }
}