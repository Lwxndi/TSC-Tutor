//using Microsoft.EntityFrameworkCore;
//using Tutor_Manager.Models;
//using Tutor_Manager.Models.Enums;
//using Tutor_Manager.Services;
//public class Tutor_ManagerDatabaseContext(DbContextOptions<Tutor_ManagerDatabaseContext> options) : DbContext(options)
//{
//    public DbSet<User> Users { get; set; } = default!;
//    public DbSet<Role> Roles { get; set; } = default!;
//    public DbSet<UserRole> UserRoles { get; set; } = default!;

//    public DbSet<Tutor> Tutors { get; set; } = default!;
//    public DbSet<Parent> Parents { get; set; } = default!;
//    public DbSet<Learner> Learners { get; set; } = default!;
//    public DbSet<Administrator> Administrators { get; set; } = default!;

//    public DbSet<LearnerGuardian> LearnerGuardians { get; set; } = default!;

//    public DbSet<Subject> Subjects { get; set; } = default!;
//    public DbSet<TutorSubject> TutorSubjects { get; set; } = default!;
//    public DbSet<LearnerSubject> LearnerSubjects { get; set; } = default!;
//    public DbSet<Notification> Notifications { get; set; } = default!;

//    // Tutor Application related tables
//    public DbSet<TutorApplication> TutorApplications { get; set; } = default!;
//    public DbSet<TutorApplicationSubject> TutorApplicationSubjects { get; set; } = default!;
//    public DbSet<TutorApplicationExperience> TutorApplicationExperiences { get; set; } = default!;
//    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; } = default!;
//    public DbSet<AccountActivationToken> AccountActivationTokens { get; set; } = default!;
//    public DbSet<TutorApplicationQualification> TutorApplicationQualifications { get; set; } = default!;
//    public DbSet<SubjectGrade> SubjectGrades { get; set; } = default!;
//    public DbSet<TutorUnavailability> TutorUnavailabilities { get; set; } = default!;
//    public DbSet<TutorAvailability> TutorAvailabilities { get; set; } = default!;
//    public DbSet<Offering> Offerings { get; set; } = default!;
//    public DbSet<OfferingTeachingDay> OfferingTeachingDays { get; set; } = default!;
//    public DbSet<OfferingTimeWindow> OfferingTimeWindows { get; set; } = default!;

//    public DbSet<TscSettings> TscSettings { get; set; } = default!;
//    public DbSet<Session> Sessions { get; set; } = default!;
//    public DbSet<AiRecommendation> AiRecommendations { get; set; } = default!;

//    public DbSet<StudyMaterial> StudyMaterials { get; set; } = default!;

//    public DbSet<StudyMaterialFile> StudyMaterialFiles { get; set; } = default!;

//    // Learner Enrollment related tables
//    public DbSet<Enrollment> Enrollments { get; set; } = default!;
//    public DbSet<EnrollmentRedirectHistory> EnrollmentRedirectHistories { get; set; } = default!;

//    //Quizz related tables

//    public DbSet<Quiz> Quizzes { get; set; } = default!;
//    public DbSet<LearnerQuizz> LearnerQuizzes { get; set; } = default!;
//    public DbSet<QuizQuestion> QuizQuestions { get; set; } = default!;
//    public DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; } = default!;
//    public DbSet<QuizAttempt> QuizAttempts { get; set; } = default!;
//    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = default!;
//    public DbSet<QuizAttemptSelectedOption> QuizAttemptSelectedOptions { get; set; } = default!;


//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {

//        // Learner Enrollment relationships

//        modelBuilder.Entity<Enrollment>()
//    .HasOne(e => e.Learner)
//    .WithMany()
//    .HasForeignKey(e => e.LearnerUserId)
//    .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.Offering)
//            .WithMany()
//            .HasForeignKey(e => e.OfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.RequestedOffering)
//            .WithMany()
//            .HasForeignKey(e => e.RequestedOfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.DecidedByAdmin)
//            .WithMany()
//            .HasForeignKey(e => e.DecidedByAdminUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.WithdrawnByAdmin)
//            .WithMany()
//            .HasForeignKey(e => e.WithdrawnByAdminUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<EnrollmentRedirectHistory>()
//            .HasOne(h => h.Enrollment)
//            .WithMany(e => e.RedirectHistory)
//            .HasForeignKey(h => h.EnrollmentId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<EnrollmentRedirectHistory>()
//            .HasOne(h => h.PreviousOffering)
//            .WithMany()
//            .HasForeignKey(h => h.PreviousOfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<EnrollmentRedirectHistory>()
//            .HasOne(h => h.NewOffering)
//            .WithMany()
//            .HasForeignKey(h => h.NewOfferingId)
//            .OnDelete(DeleteBehavior.Restrict);





//        //modelBuilder.Entity<Enrollment>()
//        //    .HasOne(e => e.Learner)
//        //    .WithMany()
//        //    .HasForeignKey(e => e.LearnerUserId)
//        //    .OnDelete(DeleteBehavior.Restrict);

//        //modelBuilder.Entity<Enrollment>()
//        //    .HasOne(e => e.Offering)
//        //    .WithMany()
//        //    .HasForeignKey(e => e.OfferingId)
//        //    .OnDelete(DeleteBehavior.Restrict);

//        //modelBuilder.Entity<Enrollment>()
//        //    .Property(e => e.Price)
//        //    .HasPrecision(10, 2);
//        //modelBuilder.Entity<UserRole>()
//        //    .HasKey(ur => new { ur.UserId, ur.RoleId });

//        // Configure the many-to-many relationship between Learners and Parents (Guardians)

//        modelBuilder.Entity<LearnerGuardian>()
//            .HasKey(lg => new { lg.LearnerUserId, lg.ParentUserId });

//        modelBuilder.Entity<LearnerGuardian>()
//            .HasOne(lg => lg.Parent)
//            .WithMany(p => p.Learners)
//            .HasForeignKey(lg => lg.ParentUserId)
//            .OnDelete(DeleteBehavior.NoAction);

//        modelBuilder.Entity<TutorSubject>()
//             .HasKey(ts => new { ts.TutorUserId, ts.SubjectId, ts.GradeLevel });

//        modelBuilder.Entity<TutorSubject>()
//            .HasOne(ts => ts.Tutor)
//            .WithMany(t => t.SubjectsTaught)
//            .HasForeignKey(ts => ts.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<TutorSubject>()
//            .HasOne(ts => ts.Subject)
//            .WithMany(s => s.Tutors)
//            .HasForeignKey(ts => ts.SubjectId)
//            .OnDelete(DeleteBehavior.NoAction);

//        modelBuilder.Entity<TutorAvailability>()
//            .HasOne(a => a.Tutor)
//            .WithMany(t => t.Availability)
//            .HasForeignKey(a => a.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<TutorUnavailability>()
//            .HasOne(u => u.Tutor)
//            .WithMany(t => t.Unavailability)
//            .HasForeignKey(u => u.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<LearnerSubject>()
//            .HasKey(ls => new { ls.LearnerUserId, ls.SubjectId });

//        modelBuilder.Entity<Offering>()
//            .HasOne(o => o.Subject)
//            .WithMany()
//            .HasForeignKey(o => o.SubjectId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Offering>()
//            .HasOne(o => o.Tutor)
//            .WithMany()
//            .HasForeignKey(o => o.TutorUserId)
//            .OnDelete(DeleteBehavior.Restrict); // preserve historical offerings if tutor is deactivated

//        modelBuilder.Entity<OfferingTeachingDay>()
//            .HasOne(td => td.Offering)
//            .WithMany(o => o.TeachingDays)
//            .HasForeignKey(td => td.OfferingId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<Offering>()
//            .Property(o => o.Price)
//            .HasPrecision(10, 2); // up to 99,999,999.99 — plenty of headroom for a monthly tutoring rate

//        modelBuilder.Entity<Session>()
//           .HasOne(s => s.Offering)
//           .WithMany()
//           .HasForeignKey(s => s.OfferingId)
//           .OnDelete(DeleteBehavior.Restrict); // preserve session history

//        modelBuilder.Entity<AiRecommendation>()
//            .HasOne(r => r.Tutor)
//            .WithMany()
//            .HasForeignKey(r => r.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        // Quiz related configurations
//        modelBuilder.Entity<LearnerQuizz>()
//     .HasKey(lq => new { lq.QuizId, lq.LearnerUserId });

//        modelBuilder.Entity<LearnerQuizz>()
//            .HasOne(lq => lq.Quiz)
//            .WithMany(q => q.AudienceLearners)
//            .HasForeignKey(lq => lq.QuizId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<LearnerQuizz>()
//            .HasOne(lq => lq.Learner)
//            .WithMany()
//            .HasForeignKey(lq => lq.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Quiz>()
//            .HasOne(q => q.Tutor)
//            .WithMany()
//            .HasForeignKey(q => q.TutorUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Quiz>()
//            .HasOne(q => q.Offering)
//            .WithMany()
//            .HasForeignKey(q => q.OfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Quiz>()
//            .HasOne(q => q.StudyMaterial)
//            .WithMany()
//            .HasForeignKey(q => q.StudyMaterialId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizQuestion>()
//            .HasOne(q => q.Quiz)
//            .WithMany(quiz => quiz.Questions)
//            .HasForeignKey(q => q.QuizId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizQuestionOption>()
//            .HasOne(o => o.QuizQuestion)
//            .WithMany(q => q.Options)
//            .HasForeignKey(o => o.QuizQuestionId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizAttempt>()
//    .HasOne(a => a.Quiz)
//    .WithMany()
//    .HasForeignKey(a => a.QuizId)
//    .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizAttempt>()
//            .HasOne(a => a.Learner)
//            .WithMany()
//            .HasForeignKey(a => a.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizAttemptAnswer>()
//            .HasOne(a => a.QuizAttempt)
//            .WithMany(qa => qa.Answers)
//            .HasForeignKey(a => a.QuizAttemptId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizAttemptAnswer>()
//            .HasOne(a => a.QuizQuestion)
//            .WithMany()
//            .HasForeignKey(a => a.QuizQuestionId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizAttemptSelectedOption>()
//            .HasOne(o => o.QuizAttemptAnswer)
//            .WithMany(a => a.SelectedOptions)
//            .HasForeignKey(o => o.QuizAttemptAnswerId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizAttemptSelectedOption>()
//            .HasOne(o => o.QuizQuestionOption)
//            .WithMany()
//            .HasForeignKey(o => o.QuizQuestionOptionId)
//            .OnDelete(DeleteBehavior.Restrict);


//        // Single TscSettings seed — 7:00–21:00 outer bound, 60–150 min sessions,
//        // per the Offering/Session redesign lock-in.
//        modelBuilder.Entity<TscSettings>().HasData(
//            new TscSettings
//            {
//                TscSettingsId = 1,
//                EarliestTeachingTime = new TimeSpan(7, 0, 0),
//                LatestTeachingTime = new TimeSpan(21, 0, 0),
//                MinSessionDurationMinutes = 60,
//                MaxSessionDurationMinutes = 150,
//                StandardSessionDurationMinutes = 120,
//                MaxSessionsPerTutorPerDay = 4,
//                MaxTutoringHoursPerTutorPerDay = 8,
//                MaxSubjectsPerTutorPerDay = 2,
//                MaxSubjectsPerTutorPerWeek = 2,
//                MinBreakBetweenSessionsMinutes = 30,
//                MaxConsecutiveSessionsBeforeLongerBreak = 2,
//                LongerBreakMinutes = 60,
//                MaxWeeklyTutoringHours = 40
//            }
//        );

//        modelBuilder.Entity<OfferingTimeWindow>().HasData(
//            new OfferingTimeWindow { OfferingTimeWindowId = 1, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 2, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 3, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(18, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 4, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 5, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 6, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) }
//        );

//        modelBuilder.Entity<Role>().HasData(
//            new Role { RoleId = 1, RoleName = "Learner" },
//            new Role { RoleId = 2, RoleName = "Tutor" },
//            new Role { RoleId = 3, RoleName = "Parent" },
//            new Role { RoleId = 4, RoleName = "Admin" }
//        );

//        modelBuilder.Entity<Subject>().HasData(
//            new Subject { SubjectId = 1, SubjectName = "Mathematics" },
//            new Subject { SubjectId = 2, SubjectName = "Physical Sciences" },
//            new Subject { SubjectId = 3, SubjectName = "Life Sciences" },
//            new Subject { SubjectId = 4, SubjectName = "Mathematical Literacy" },
//            new Subject { SubjectId = 5, SubjectName = "Accounting" }
//        );

//        modelBuilder.Entity<SubjectGrade>().HasData(
//            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade12 }, // Mathematics: 10-12
//            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade12 }, // Physical Sciences: 10-12
//            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade12 }, // Life Sciences: 10-12
//            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade12 }, // Maths Lit: 10-12
//            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade12 }  // Accounting: 10-12
//        );

//        modelBuilder.Entity<SubjectGrade>()
//         .HasKey(sg => new { sg.SubjectId, sg.Grade });

//        modelBuilder.Entity<SubjectGrade>()
//            .HasOne(sg => sg.Subject)
//            .WithMany(s => s.Grades)
//            .HasForeignKey(sg => sg.SubjectId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<User>().HasData(new User
//        {
//            UserId = 1,
//            FirstName = "Admin",
//            LastName = "AdminVila",
//            Email = "lwandinhlengethwa25@gmail.com",
//            PhoneNumber = "0000000000", // placeholder - replace with Michael's real number
//            PasswordHash = "AQAAAAEAAYagAAAAEIvwxMAJ/t2/rwo6a30o39WIwMzC2nLntC98owiYso24zSx1fLtYAYjOcSRddprXUw==",
//            DateCreated = new DateTime(2026, 1, 1),
//            IsActive = true
//        });

//        //modelBuilder.Entity<OfferingTimeWindow>().HasData(
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 1, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 2, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 3, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 4, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 5, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 6, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(20, 0, 0) }
//        //);

//        modelBuilder.Entity<UserRole>().HasData(new UserRole
//        {
//            UserId = 1,
//            RoleId = 4 // Admin, per your existing Role seed
//        });

//        modelBuilder.Entity<Administrator>().HasData(new Administrator
//        {
//            UserId = 1,
//            Position = "Owner / Head Tutor"
//        });

//        modelBuilder.Entity<TutorApplication>()
//            .HasIndex(a => a.ReferenceNumber)
//            .IsUnique();

//        base.OnModelCreating(modelBuilder);
//    }
//}














//using Microsoft.EntityFrameworkCore;
//using Tutor_Manager.Models;
//using Tutor_Manager.Models.Enums;
//using Tutor_Manager.Services;
//public class Tutor_ManagerDatabaseContext(DbContextOptions<Tutor_ManagerDatabaseContext> options) : DbContext(options)
//{
//    public DbSet<User> Users { get; set; } = default!;
//    public DbSet<Role> Roles { get; set; } = default!;
//    public DbSet<UserRole> UserRoles { get; set; } = default!;

//    public DbSet<Tutor> Tutors { get; set; } = default!;
//    public DbSet<Parent> Parents { get; set; } = default!;
//    public DbSet<Learner> Learners { get; set; } = default!;
//    public DbSet<Administrator> Administrators { get; set; } = default!;

//    public DbSet<LearnerGuardian> LearnerGuardians { get; set; } = default!;

//    public DbSet<Subject> Subjects { get; set; } = default!;
//    public DbSet<TutorSubject> TutorSubjects { get; set; } = default!;
//    public DbSet<LearnerSubject> LearnerSubjects { get; set; } = default!;
//    public DbSet<Notification> Notifications { get; set; } = default!;

//    // Tutor Application related tables
//    public DbSet<TutorApplication> TutorApplications { get; set; } = default!;
//    public DbSet<TutorApplicationSubject> TutorApplicationSubjects { get; set; } = default!;
//    public DbSet<TutorApplicationExperience> TutorApplicationExperiences { get; set; } = default!;
//    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; } = default!;
//    public DbSet<AccountActivationToken> AccountActivationTokens { get; set; } = default!;
//    public DbSet<TutorApplicationQualification> TutorApplicationQualifications { get; set; } = default!;
//    public DbSet<SubjectGrade> SubjectGrades { get; set; } = default!;
//    public DbSet<TutorUnavailability> TutorUnavailabilities { get; set; } = default!;
//    public DbSet<TutorAvailability> TutorAvailabilities { get; set; } = default!;
//    public DbSet<Offering> Offerings { get; set; } = default!;
//    public DbSet<OfferingTeachingDay> OfferingTeachingDays { get; set; } = default!;
//    public DbSet<OfferingTimeWindow> OfferingTimeWindows { get; set; } = default!;

//    public DbSet<TscSettings> TscSettings { get; set; } = default!;
//    public DbSet<Session> Sessions { get; set; } = default!;
//    public DbSet<AiRecommendation> AiRecommendations { get; set; } = default!;

//    public DbSet<StudyMaterial> StudyMaterials { get; set; } = default!;

//    public DbSet<StudyMaterialFile> StudyMaterialFiles { get; set; } = default!;

//    // Learner Enrollment related tables
//    public DbSet<Enrollment> Enrollments { get; set; } = default!;
//    public DbSet<EnrollmentRedirectHistory> EnrollmentRedirectHistories { get; set; } = default!;

//    //Quizz related tables

//    public DbSet<Quiz> Quizzes { get; set; } = default!;
//    public DbSet<LearnerQuizz> LearnerQuizzes { get; set; } = default!;
//    public DbSet<QuizQuestion> QuizQuestions { get; set; } = default!;
//    public DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; } = default!;
//    public DbSet<QuizAttempt> QuizAttempts { get; set; } = default!;
//    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = default!;
//    public DbSet<QuizAttemptSelectedOption> QuizAttemptSelectedOptions { get; set; } = default!;

//    //Assessment related tables
//    public DbSet<Assessment> Assessments { get; set; } = default!;
//    public DbSet<AssessmentFile> AssessmentFiles { get; set; } = default!;
//    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; } = default!;
//    public DbSet<AssessmentLearner> AssessmentLearners { get; set; } = default!;
//    public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; } = default!;
//    public DbSet<AssessmentAttemptAnswer> AssessmentAttemptAnswers { get; set; } = default!;

//    // Payments related tables (Phase B)

//    public DbSet<Invoice> Invoices { get; set; } = default!;
//    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; } = default!;
//    public DbSet<Payment> Payments { get; set; } = default!;
//    public DbSet<PaymentInvoice> PaymentInvoices { get; set; } = default!;


//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<UserRole>()
//            .HasKey(ur => new { ur.UserId, ur.RoleId });
//        // Learner Enrollment relationships

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.Learner)
//            .WithMany()
//            .HasForeignKey(e => e.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.Offering)
//            .WithMany()
//            .HasForeignKey(e => e.OfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.RequestedOffering)
//            .WithMany()
//            .HasForeignKey(e => e.RequestedOfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.DecidedByAdmin)
//            .WithMany()
//            .HasForeignKey(e => e.DecidedByAdminUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Enrollment>()
//            .HasOne(e => e.WithdrawnByAdmin)
//            .WithMany()
//            .HasForeignKey(e => e.WithdrawnByAdminUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        // Spec §12 — at most one non-final Enrollment per (Learner, Offering).
//        // EnrollmentStatus order: Pending=0, Waitlisted=1, Active=2, Rejected=3,
//        // Withdrawn=4, Suspended=5 — only 0,1,2,5 are enforced here. Rejected and
//        // Withdrawn are deliberately excluded so a fresh request is always possible
//        // after either. If EnrollmentStatus's declared order ever changes, or Status
//        // is ever mapped with HasConversion<string>(), this filter string must be
//        // updated to match or it will silently protect the wrong states.
//        modelBuilder.Entity<Enrollment>()
//           .HasIndex(e => new { e.LearnerUserId, e.OfferingId })
//           .IsUnique()
//           .HasFilter("[Status] IN (0, 1, 2, 5, 6)");

//        modelBuilder.Entity<EnrollmentRedirectHistory>()
//            .HasOne(h => h.Enrollment)
//            .WithMany(e => e.RedirectHistory)
//            .HasForeignKey(h => h.EnrollmentId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<EnrollmentRedirectHistory>()
//            .HasOne(h => h.PreviousOffering)
//            .WithMany()
//            .HasForeignKey(h => h.PreviousOfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<EnrollmentRedirectHistory>()
//            .HasOne(h => h.NewOffering)
//            .WithMany()
//            .HasForeignKey(h => h.NewOfferingId)
//            .OnDelete(DeleteBehavior.Restrict);


//        //Assessment relationships
//        modelBuilder.Entity<Assessment>()
//            .HasOne(a => a.Tutor)
//            .WithMany()
//            .HasForeignKey(a => a.TutorUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Assessment>()
//            .HasOne(a => a.Subject)
//            .WithMany()
//            .HasForeignKey(a => a.SubjectId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<AssessmentLearner>()
//    .HasKey(al => new { al.AssessmentId, al.LearnerUserId });

//        modelBuilder.Entity<AssessmentLearner>()
//            .HasOne(al => al.Assessment)
//            .WithMany(a => a.AudienceLearners)
//            .HasForeignKey(al => al.AssessmentId)
//            .OnDelete(DeleteBehavior.Restrict); // same cascade-cycle caution as LearnerQuizz

//        modelBuilder.Entity<Assessment>()
//            .HasOne(a => a.Offering)
//            .WithMany()
//            .HasForeignKey(a => a.OfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<AssessmentAttempt>()
//            .HasOne(a => a.Assessment)
//            .WithMany()
//            .HasForeignKey(a => a.AssessmentId)
//            .OnDelete(DeleteBehavior.Restrict);
//        modelBuilder.Entity<AssessmentAttempt>()
//            .HasOne(a => a.Learner)
//            .WithMany()
//            .HasForeignKey(a => a.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<AssessmentAttemptAnswer>()
//            .HasOne(a => a.AssessmentAttempt)
//            .WithMany(a => a.Answers)
//            .HasForeignKey(a => a.AssessmentAttemptId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<AssessmentAttemptAnswer>()
//            .HasOne(a => a.AssessmentQuestion)
//            .WithMany()
//            .HasForeignKey(a => a.AssessmentQuestionId)
//            .OnDelete(DeleteBehavior.Restrict);



//        // Payments relationships (Phase B)

//        modelBuilder.Entity<Invoice>()
//    .HasOne(i => i.CarriedForwardIntoInvoice)
//    .WithMany()
//    .HasForeignKey(i => i.CarriedForwardIntoInvoiceId)
//    .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Payment>()
//            .HasOne(p => p.Guardian)
//            .WithMany()
//            .HasForeignKey(p => p.GuardianUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Payment>()
//            .Property(p => p.Amount)
//            .HasPrecision(10, 2);

//        // One Payment row per Stripe session (ignores rows not yet sent to Stripe).
//        modelBuilder.Entity<Payment>()
//            .HasIndex(p => p.StripeCheckoutSessionId)
//            .IsUnique()
//            .HasFilter("[StripeCheckoutSessionId] IS NOT NULL");

//        modelBuilder.Entity<PaymentInvoice>()
//            .HasKey(pi => new { pi.PaymentId, pi.InvoiceId });

//        modelBuilder.Entity<PaymentInvoice>()
//            .HasOne(pi => pi.Payment)
//            .WithMany(p => p.PaymentInvoices)
//            .HasForeignKey(pi => pi.PaymentId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<PaymentInvoice>()
//            .HasOne(pi => pi.Invoice)
//            .WithMany(i => i.PaymentInvoices)
//            .HasForeignKey(pi => pi.InvoiceId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<PaymentInvoice>()
//            .Property(pi => pi.AmountApplied)
//            .HasPrecision(10, 2);

//        modelBuilder.Entity<Invoice>()
//    .HasOne(i => i.Guardian)
//    .WithMany()
//    .HasForeignKey(i => i.GuardianUserId)
//    .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Invoice>()
//            .Property(i => i.TotalAmountDue)
//            .HasPrecision(10, 2);

//        modelBuilder.Entity<Invoice>()
//            .Property(i => i.TotalAmountPaidSoFar)
//            .HasPrecision(10, 2);

//        modelBuilder.Entity<InvoiceLineItem>()
//            .HasOne(li => li.Invoice)
//            .WithMany(i => i.LineItems)
//            .HasForeignKey(li => li.InvoiceId)
//            .OnDelete(DeleteBehavior.Cascade); // deleting an Invoice legitimately removes its own line items

//        modelBuilder.Entity<InvoiceLineItem>()
//            .HasOne(li => li.Learner)
//            .WithMany()
//            .HasForeignKey(li => li.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<InvoiceLineItem>()
//            .HasOne(li => li.Enrollment)
//            .WithMany()
//            .HasForeignKey(li => li.EnrollmentId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<InvoiceLineItem>()
//            .HasOne(li => li.OfferingSnapshot)
//            .WithMany()
//            .HasForeignKey(li => li.OfferingSnapshotId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<InvoiceLineItem>()
//            .Property(li => li.AmountDue)
//            .HasPrecision(10, 2);

//        modelBuilder.Entity<InvoiceLineItem>()
//            .Property(li => li.AmountPaid)
//            .HasPrecision(10, 2);



//        // Configure the many-to-many relationship between Learners and Parents (Guardians)

//        modelBuilder.Entity<LearnerGuardian>()
//            .HasKey(lg => new { lg.LearnerUserId, lg.ParentUserId });

//        modelBuilder.Entity<LearnerGuardian>()
//            .HasOne(lg => lg.Parent)
//            .WithMany(p => p.Learners)
//            .HasForeignKey(lg => lg.ParentUserId)
//            .OnDelete(DeleteBehavior.NoAction);

//        modelBuilder.Entity<TutorSubject>()
//             .HasKey(ts => new { ts.TutorUserId, ts.SubjectId, ts.GradeLevel });

//        modelBuilder.Entity<TutorSubject>()
//            .HasOne(ts => ts.Tutor)
//            .WithMany(t => t.SubjectsTaught)
//            .HasForeignKey(ts => ts.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<TutorSubject>()
//            .HasOne(ts => ts.Subject)
//            .WithMany(s => s.Tutors)
//            .HasForeignKey(ts => ts.SubjectId)
//            .OnDelete(DeleteBehavior.NoAction);

//        modelBuilder.Entity<TutorAvailability>()
//            .HasOne(a => a.Tutor)
//            .WithMany(t => t.Availability)
//            .HasForeignKey(a => a.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<TutorUnavailability>()
//            .HasOne(u => u.Tutor)
//            .WithMany(t => t.Unavailability)
//            .HasForeignKey(u => u.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<LearnerSubject>()
//            .HasKey(ls => new { ls.LearnerUserId, ls.SubjectId });

//        modelBuilder.Entity<Offering>()
//            .HasOne(o => o.Subject)
//            .WithMany()
//            .HasForeignKey(o => o.SubjectId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Offering>()
//            .HasOne(o => o.Tutor)
//            .WithMany()
//            .HasForeignKey(o => o.TutorUserId)
//            .OnDelete(DeleteBehavior.Restrict); // preserve historical offerings if tutor is deactivated

//        modelBuilder.Entity<OfferingTeachingDay>()
//            .HasOne(td => td.Offering)
//            .WithMany(o => o.TeachingDays)
//            .HasForeignKey(td => td.OfferingId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<Offering>()
//            .Property(o => o.Price)
//            .HasPrecision(10, 2); // up to 99,999,999.99 — plenty of headroom for a monthly tutoring rate

//        modelBuilder.Entity<Session>()
//           .HasOne(s => s.Offering)
//           .WithMany()
//           .HasForeignKey(s => s.OfferingId)
//           .OnDelete(DeleteBehavior.Restrict); // preserve session history

//        modelBuilder.Entity<AiRecommendation>()
//            .HasOne(r => r.Tutor)
//            .WithMany()
//            .HasForeignKey(r => r.TutorUserId)
//            .OnDelete(DeleteBehavior.Cascade);

//        // Quiz related configurations
//        modelBuilder.Entity<LearnerQuizz>()
//     .HasKey(lq => new { lq.QuizId, lq.LearnerUserId });

//        modelBuilder.Entity<LearnerQuizz>()
//            .HasOne(lq => lq.Quiz)
//            .WithMany(q => q.AudienceLearners)
//            .HasForeignKey(lq => lq.QuizId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<LearnerQuizz>()
//            .HasOne(lq => lq.Learner)
//            .WithMany()
//            .HasForeignKey(lq => lq.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Quiz>()
//            .HasOne(q => q.Tutor)
//            .WithMany()
//            .HasForeignKey(q => q.TutorUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Quiz>()
//            .HasOne(q => q.Offering)
//            .WithMany()
//            .HasForeignKey(q => q.OfferingId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<Quiz>()
//            .HasOne(q => q.StudyMaterial)
//            .WithMany()
//            .HasForeignKey(q => q.StudyMaterialId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizQuestion>()
//            .HasOne(q => q.Quiz)
//            .WithMany(quiz => quiz.Questions)
//            .HasForeignKey(q => q.QuizId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizQuestionOption>()
//            .HasOne(o => o.QuizQuestion)
//            .WithMany(q => q.Options)
//            .HasForeignKey(o => o.QuizQuestionId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizAttempt>()
//    .HasOne(a => a.Quiz)
//    .WithMany()
//    .HasForeignKey(a => a.QuizId)
//    .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizAttempt>()
//            .HasOne(a => a.Learner)
//            .WithMany()
//            .HasForeignKey(a => a.LearnerUserId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizAttemptAnswer>()
//            .HasOne(a => a.QuizAttempt)
//            .WithMany(qa => qa.Answers)
//            .HasForeignKey(a => a.QuizAttemptId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizAttemptAnswer>()
//            .HasOne(a => a.QuizQuestion)
//            .WithMany()
//            .HasForeignKey(a => a.QuizQuestionId)
//            .OnDelete(DeleteBehavior.Restrict);

//        modelBuilder.Entity<QuizAttemptSelectedOption>()
//            .HasOne(o => o.QuizAttemptAnswer)
//            .WithMany(a => a.SelectedOptions)
//            .HasForeignKey(o => o.QuizAttemptAnswerId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<QuizAttemptSelectedOption>()
//            .HasOne(o => o.QuizQuestionOption)
//            .WithMany()
//            .HasForeignKey(o => o.QuizQuestionOptionId)
//            .OnDelete(DeleteBehavior.Restrict);


//        // Single TscSettings seed — 7:00–21:00 outer bound, 60–150 min sessions,
//        // per the Offering/Session redesign lock-in.
//        modelBuilder.Entity<TscSettings>().HasData(
//            new TscSettings
//            {
//                TscSettingsId = 1,
//                EarliestTeachingTime = new TimeSpan(7, 0, 0),
//                LatestTeachingTime = new TimeSpan(21, 0, 0),
//                MinSessionDurationMinutes = 60,
//                MaxSessionDurationMinutes = 150,
//                StandardSessionDurationMinutes = 120,
//                MaxSessionsPerTutorPerDay = 4,
//                MaxTutoringHoursPerTutorPerDay = 8,
//                MaxSubjectsPerTutorPerDay = 2,
//                MaxSubjectsPerTutorPerWeek = 2,
//                MinBreakBetweenSessionsMinutes = 30,
//                MaxConsecutiveSessionsBeforeLongerBreak = 2,
//                LongerBreakMinutes = 60,
//                MaxWeeklyTutoringHours = 40
//            }
//        );

//        modelBuilder.Entity<OfferingTimeWindow>().HasData(
//            new OfferingTimeWindow { OfferingTimeWindowId = 1, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 2, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 3, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(18, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 4, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 5, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 6, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 7, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Hybrid, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//            new OfferingTimeWindow { OfferingTimeWindowId = 8, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Hybrid, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) }
//        );

//        modelBuilder.Entity<Role>().HasData(
//            new Role { RoleId = 1, RoleName = "Learner" },
//            new Role { RoleId = 2, RoleName = "Tutor" },
//            new Role { RoleId = 3, RoleName = "Parent" },
//            new Role { RoleId = 4, RoleName = "Admin" }
//        );

//        modelBuilder.Entity<Subject>().HasData(
//            new Subject { SubjectId = 1, SubjectName = "Mathematics" },
//            new Subject { SubjectId = 2, SubjectName = "Physical Sciences" },
//            new Subject { SubjectId = 3, SubjectName = "Life Sciences" },
//            new Subject { SubjectId = 4, SubjectName = "Mathematical Literacy" },
//            new Subject { SubjectId = 5, SubjectName = "Accounting" }
//        );

//        modelBuilder.Entity<SubjectGrade>().HasData(
//            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade12 }, // Mathematics: 10-12
//            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade12 }, // Physical Sciences: 10-12
//            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade12 }, // Life Sciences: 10-12
//            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade12 }, // Maths Lit: 10-12
//            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade10 },
//            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade11 },
//            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade12 }  // Accounting: 10-12
//        );

//        modelBuilder.Entity<SubjectGrade>()
//         .HasKey(sg => new { sg.SubjectId, sg.Grade });

//        modelBuilder.Entity<SubjectGrade>()
//            .HasOne(sg => sg.Subject)
//            .WithMany(s => s.Grades)
//            .HasForeignKey(sg => sg.SubjectId)
//            .OnDelete(DeleteBehavior.Cascade);

//        modelBuilder.Entity<User>().HasData(new User
//        {
//            UserId = 1,
//            FirstName = "Admin",
//            LastName = "AdminVila",
//            Email = "lwandinhlengethwa25@gmail.com",
//            PhoneNumber = "0000000000", // placeholder - replace with Michael's real number
//            PasswordHash = "AQAAAAEAAYagAAAAEIvwxMAJ/t2/rwo6a30o39WIwMzC2nLntC98owiYso24zSx1fLtYAYjOcSRddprXUw==",
//            DateCreated = new DateTime(2026, 1, 1),
//            IsActive = true
//        });

//        //modelBuilder.Entity<OfferingTimeWindow>().HasData(
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 1, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 2, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 3, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 4, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 5, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(20, 0, 0) },
//        //    new OfferingTimeWindow { OfferingTimeWindowId = 6, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(20, 0, 0) }
//        //);

//        modelBuilder.Entity<UserRole>().HasData(new UserRole
//        {
//            UserId = 1,
//            RoleId = 4 // Admin, per your existing Role seed
//        });

//        modelBuilder.Entity<Administrator>().HasData(new Administrator
//        {
//            UserId = 1,
//            Position = "Owner / Head Tutor"
//        });

//        modelBuilder.Entity<TutorApplication>()
//            .HasIndex(a => a.ReferenceNumber)
//            .IsUnique();

//        base.OnModelCreating(modelBuilder);
//    }
//}





using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Models.Enums;
using Tutor_Manager.Services;

public class Tutor_ManagerDatabaseContext(DbContextOptions<Tutor_ManagerDatabaseContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<UserRole> UserRoles { get; set; } = default!;

    public DbSet<Tutor> Tutors { get; set; } = default!;
    public DbSet<Parent> Parents { get; set; } = default!;
    public DbSet<Learner> Learners { get; set; } = default!;
    public DbSet<Administrator> Administrators { get; set; } = default!;

    public DbSet<LearnerGuardian> LearnerGuardians { get; set; } = default!;

    public DbSet<Subject> Subjects { get; set; } = default!;
    public DbSet<TutorSubject> TutorSubjects { get; set; } = default!;
    public DbSet<LearnerSubject> LearnerSubjects { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; } = default!;

    // Tutor Application related tables
    public DbSet<TutorApplication> TutorApplications { get; set; } = default!;
    public DbSet<TutorApplicationSubject> TutorApplicationSubjects { get; set; } = default!;
    public DbSet<TutorApplicationExperience> TutorApplicationExperiences { get; set; } = default!;
    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; } = default!;
    public DbSet<AccountActivationToken> AccountActivationTokens { get; set; } = default!;
    public DbSet<TutorApplicationQualification> TutorApplicationQualifications { get; set; } = default!;
    public DbSet<SubjectGrade> SubjectGrades { get; set; } = default!;
    public DbSet<TutorUnavailability> TutorUnavailabilities { get; set; } = default!;
    public DbSet<TutorAvailability> TutorAvailabilities { get; set; } = default!;
    public DbSet<Offering> Offerings { get; set; } = default!;
    public DbSet<OfferingTeachingDay> OfferingTeachingDays { get; set; } = default!;
    public DbSet<OfferingTimeWindow> OfferingTimeWindows { get; set; } = default!;

    public DbSet<TscSettings> TscSettings { get; set; } = default!;
    public DbSet<Session> Sessions { get; set; } = default!;
    public DbSet<AiRecommendation> AiRecommendations { get; set; } = default!;

    public DbSet<StudyMaterial> StudyMaterials { get; set; } = default!;
    public DbSet<StudyMaterialFile> StudyMaterialFiles { get; set; } = default!;

    // Session redesign tables
    public DbSet<SessionAttendance> SessionAttendances { get; set; } = default!;
    public DbSet<SessionNote> SessionNotes { get; set; } = default!;
    public DbSet<SessionTopic> SessionTopics { get; set; } = default!;
    public DbSet<SessionLearnerRemark> SessionLearnerRemarks { get; set; } = default!;
    public DbSet<SessionActivity> SessionActivities { get; set; } = default!;
    public DbSet<SessionMaterial> SessionMaterials { get; set; } = default!;
    public DbSet<SessionActivityMaterial> SessionActivityMaterials { get; set; } = default!;
    public DbSet<SessionActivityResult> SessionActivityResults { get; set; } = default!;
    public DbSet<SessionFeedback> SessionFeedbacks { get; set; } = default!;

    // Learner Enrollment related tables
    public DbSet<Enrollment> Enrollments { get; set; } = default!;
    public DbSet<EnrollmentRedirectHistory> EnrollmentRedirectHistories { get; set; } = default!;

    // Quiz related tables
    public DbSet<Quiz> Quizzes { get; set; } = default!;
    public DbSet<LearnerQuizz> LearnerQuizzes { get; set; } = default!;
    public DbSet<QuizQuestion> QuizQuestions { get; set; } = default!;
    public DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; } = default!;
    public DbSet<QuizAttempt> QuizAttempts { get; set; } = default!;
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = default!;
    public DbSet<QuizAttemptSelectedOption> QuizAttemptSelectedOptions { get; set; } = default!;

    // Assessment related tables
    public DbSet<Assessment> Assessments { get; set; } = default!;
    public DbSet<AssessmentFile> AssessmentFiles { get; set; } = default!;
    public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; } = default!;
    public DbSet<AssessmentLearner> AssessmentLearners { get; set; } = default!;
    public DbSet<AssessmentAttempt> AssessmentAttempts { get; set; } = default!;
    public DbSet<AssessmentAttemptAnswer> AssessmentAttemptAnswers { get; set; } = default!;

    // Payments related tables (Phase B)
    public DbSet<Invoice> Invoices { get; set; } = default!;
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; } = default!;
    public DbSet<Payment> Payments { get; set; } = default!;
    public DbSet<PaymentInvoice> PaymentInvoices { get; set; } = default!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // ───────────────── Learner Enrollment relationships ─────────────────

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Learner)
            .WithMany()
            .HasForeignKey(e => e.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Offering)
            .WithMany()
            .HasForeignKey(e => e.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.RequestedOffering)
            .WithMany()
            .HasForeignKey(e => e.RequestedOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.DecidedByAdmin)
            .WithMany()
            .HasForeignKey(e => e.DecidedByAdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.WithdrawnByAdmin)
            .WithMany()
            .HasForeignKey(e => e.WithdrawnByAdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Spec §12 — at most one non-final Enrollment per (Learner, Offering).
        // EnrollmentStatus order: Pending=0, Waitlisted=1, Active=2, Rejected=3,
        // Withdrawn=4, Suspended=5 — only 0,1,2,5 are enforced here. Rejected and
        // Withdrawn are deliberately excluded so a fresh request is always possible
        // after either. If EnrollmentStatus's declared order ever changes, or Status
        // is ever mapped with HasConversion<string>(), this filter string must be
        // updated to match or it will silently protect the wrong states.
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.LearnerUserId, e.OfferingId })
            .IsUnique()
            .HasFilter("[Status] IN (0, 1, 2, 5, 6)");

        modelBuilder.Entity<EnrollmentRedirectHistory>()
            .HasOne(h => h.Enrollment)
            .WithMany(e => e.RedirectHistory)
            .HasForeignKey(h => h.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EnrollmentRedirectHistory>()
            .HasOne(h => h.PreviousOffering)
            .WithMany()
            .HasForeignKey(h => h.PreviousOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EnrollmentRedirectHistory>()
            .HasOne(h => h.NewOffering)
            .WithMany()
            .HasForeignKey(h => h.NewOfferingId)
            .OnDelete(DeleteBehavior.Restrict);


        // ───────────────── Assessment relationships ─────────────────

        modelBuilder.Entity<Assessment>()
            .HasOne(a => a.Tutor)
            .WithMany()
            .HasForeignKey(a => a.TutorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Assessment>()
            .HasOne(a => a.Subject)
            .WithMany()
            .HasForeignKey(a => a.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssessmentLearner>()
            .HasKey(al => new { al.AssessmentId, al.LearnerUserId });

        modelBuilder.Entity<AssessmentLearner>()
            .HasOne(al => al.Assessment)
            .WithMany(a => a.AudienceLearners)
            .HasForeignKey(al => al.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict); // same cascade-cycle caution as LearnerQuizz

        modelBuilder.Entity<Assessment>()
            .HasOne(a => a.Offering)
            .WithMany()
            .HasForeignKey(a => a.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssessmentAttempt>()
            .HasOne(a => a.Assessment)
            .WithMany()
            .HasForeignKey(a => a.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssessmentAttempt>()
            .HasOne(a => a.Learner)
            .WithMany()
            .HasForeignKey(a => a.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AssessmentAttemptAnswer>()
            .HasOne(a => a.AssessmentAttempt)
            .WithMany(a => a.Answers)
            .HasForeignKey(a => a.AssessmentAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssessmentAttemptAnswer>()
            .HasOne(a => a.AssessmentQuestion)
            .WithMany()
            .HasForeignKey(a => a.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Restrict);


        // ───────────────── Payments relationships (Phase B) ─────────────────

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.CarriedForwardIntoInvoice)
            .WithMany()
            .HasForeignKey(i => i.CarriedForwardIntoInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Guardian)
            .WithMany()
            .HasForeignKey(p => p.GuardianUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        // One Payment row per Stripe session (ignores rows not yet sent to Stripe).
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.StripeCheckoutSessionId)
            .IsUnique()
            .HasFilter("[StripeCheckoutSessionId] IS NOT NULL");

        modelBuilder.Entity<PaymentInvoice>()
            .HasKey(pi => new { pi.PaymentId, pi.InvoiceId });

        modelBuilder.Entity<PaymentInvoice>()
            .HasOne(pi => pi.Payment)
            .WithMany(p => p.PaymentInvoices)
            .HasForeignKey(pi => pi.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PaymentInvoice>()
            .HasOne(pi => pi.Invoice)
            .WithMany(i => i.PaymentInvoices)
            .HasForeignKey(pi => pi.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaymentInvoice>()
            .Property(pi => pi.AmountApplied)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Guardian)
            .WithMany()
            .HasForeignKey(i => i.GuardianUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmountDue)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmountPaidSoFar)
            .HasPrecision(10, 2);

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(li => li.Invoice)
            .WithMany(i => i.LineItems)
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade); // deleting an Invoice legitimately removes its own line items

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(li => li.Learner)
            .WithMany()
            .HasForeignKey(li => li.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(li => li.Enrollment)
            .WithMany()
            .HasForeignKey(li => li.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InvoiceLineItem>()
            .HasOne(li => li.OfferingSnapshot)
            .WithMany()
            .HasForeignKey(li => li.OfferingSnapshotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InvoiceLineItem>()
            .Property(li => li.AmountDue)
            .HasPrecision(10, 2);

        modelBuilder.Entity<InvoiceLineItem>()
            .Property(li => li.AmountPaid)
            .HasPrecision(10, 2);


        // ───────────────── Learners ↔ Parents (Guardians) ─────────────────

        modelBuilder.Entity<LearnerGuardian>()
            .HasKey(lg => new { lg.LearnerUserId, lg.ParentUserId });

        modelBuilder.Entity<LearnerGuardian>()
            .HasOne(lg => lg.Parent)
            .WithMany(p => p.Learners)
            .HasForeignKey(lg => lg.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TutorSubject>()
            .HasKey(ts => new { ts.TutorUserId, ts.SubjectId, ts.GradeLevel });

        modelBuilder.Entity<TutorSubject>()
            .HasOne(ts => ts.Tutor)
            .WithMany(t => t.SubjectsTaught)
            .HasForeignKey(ts => ts.TutorUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TutorSubject>()
            .HasOne(ts => ts.Subject)
            .WithMany(s => s.Tutors)
            .HasForeignKey(ts => ts.SubjectId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TutorAvailability>()
            .HasOne(a => a.Tutor)
            .WithMany(t => t.Availability)
            .HasForeignKey(a => a.TutorUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TutorUnavailability>()
            .HasOne(u => u.Tutor)
            .WithMany(t => t.Unavailability)
            .HasForeignKey(u => u.TutorUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearnerSubject>()
            .HasKey(ls => new { ls.LearnerUserId, ls.SubjectId });

        // ───────────────── Offerings ─────────────────

        modelBuilder.Entity<Offering>()
            .HasOne(o => o.Subject)
            .WithMany()
            .HasForeignKey(o => o.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Offering>()
            .HasOne(o => o.Tutor)
            .WithMany()
            .HasForeignKey(o => o.TutorUserId)
            .OnDelete(DeleteBehavior.Restrict); // preserve historical offerings if tutor is deactivated

        modelBuilder.Entity<OfferingTeachingDay>()
            .HasOne(td => td.Offering)
            .WithMany(o => o.TeachingDays)
            .HasForeignKey(td => td.OfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Offering>()
            .Property(o => o.Price)
            .HasPrecision(10, 2); // up to 99,999,999.99 — plenty of headroom for a monthly tutoring rate


        // ───────────────── Sessions (redesigned) ─────────────────

        modelBuilder.Entity<Session>()
            .HasOne(s => s.Offering)
            .WithMany()
            .HasForeignKey(s => s.OfferingId)
            .OnDelete(DeleteBehavior.Restrict); // preserve session history

        modelBuilder.Entity<Session>()
            .HasOne(s => s.OverrideTutor)
            .WithMany()
            .HasForeignKey(s => s.OverrideTutorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Session>()
            .HasOne(s => s.CancelledBy)
            .WithMany()
            .HasForeignKey(s => s.CancelledByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attendance
        modelBuilder.Entity<SessionAttendance>()
            .HasKey(a => new { a.SessionId, a.LearnerUserId });

        modelBuilder.Entity<SessionAttendance>()
            .HasOne(a => a.Session)
            .WithMany(s => s.Attendance)
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionAttendance>()
            .HasOne(a => a.Learner)
            .WithMany()
            .HasForeignKey(a => a.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionAttendance>()
            .HasOne(a => a.MarkedBy)
            .WithMany()
            .HasForeignKey(a => a.MarkedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Notes / topics / learner remarks
        modelBuilder.Entity<SessionNote>()
            .HasOne(n => n.Session)
            .WithMany(s => s.Notes)
            .HasForeignKey(n => n.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionNote>()
            .HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionTopic>()
            .HasOne(t => t.Session)
            .WithMany(s => s.Topics)
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionTopic>()
            .HasIndex(t => t.NormalizedName);

        modelBuilder.Entity<SessionLearnerRemark>()
            .HasOne(r => r.Session)
            .WithMany(s => s.LearnerRemarks)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionLearnerRemark>()
            .HasOne(r => r.Learner)
            .WithMany()
            .HasForeignKey(r => r.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionLearnerRemark>()
            .HasOne(r => r.Author)
            .WithMany()
            .HasForeignKey(r => r.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Activities + materials
        modelBuilder.Entity<SessionActivity>()
            .HasOne(a => a.Session)
            .WithMany(s => s.Activities)
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionActivity>()
            .HasOne(a => a.Quiz)
            .WithMany()
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionActivity>()
            .HasOne(a => a.Assessment)
            .WithMany()
            .HasForeignKey(a => a.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionMaterial>()
            .HasKey(m => new { m.SessionId, m.StudyMaterialId });

        modelBuilder.Entity<SessionMaterial>()
            .HasOne(m => m.Session)
            .WithMany(s => s.Materials)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionMaterial>()
            .HasOne(m => m.StudyMaterial)
            .WithMany()
            .HasForeignKey(m => m.StudyMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionActivityMaterial>()
            .HasKey(m => new { m.SessionActivityId, m.StudyMaterialId });

        modelBuilder.Entity<SessionActivityMaterial>()
            .HasOne(m => m.SessionActivity)
            .WithMany(a => a.Materials)
            .HasForeignKey(m => m.SessionActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionActivityMaterial>()
            .HasOne(m => m.StudyMaterial)
            .WithMany()
            .HasForeignKey(m => m.StudyMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        // Results + feedback
        modelBuilder.Entity<SessionActivityResult>()
            .HasKey(r => new { r.SessionActivityId, r.LearnerUserId });

        modelBuilder.Entity<SessionActivityResult>()
            .HasOne(r => r.SessionActivity)
            .WithMany(a => a.Results)
            .HasForeignKey(r => r.SessionActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionActivityResult>()
            .HasOne(r => r.Learner)
            .WithMany()
            .HasForeignKey(r => r.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionActivityResult>()
            .HasOne(r => r.RecordedBy)
            .WithMany()
            .HasForeignKey(r => r.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SessionActivityResult>()
            .Property(r => r.Score)
            .HasPrecision(6, 2);

        modelBuilder.Entity<SessionActivityResult>()
            .Property(r => r.MaxScore)
            .HasPrecision(6, 2);

        modelBuilder.Entity<SessionFeedback>()
            .HasKey(f => new { f.SessionId, f.LearnerUserId });

        modelBuilder.Entity<SessionFeedback>()
            .HasOne(f => f.Session)
            .WithMany(s => s.Feedback)
            .HasForeignKey(f => f.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SessionFeedback>()
            .HasOne(f => f.Learner)
            .WithMany()
            .HasForeignKey(f => f.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<AiRecommendation>()
            .HasOne(r => r.Tutor)
            .WithMany()
            .HasForeignKey(r => r.TutorUserId)
            .OnDelete(DeleteBehavior.Cascade);


        // ───────────────── Quiz related configurations ─────────────────

        modelBuilder.Entity<LearnerQuizz>()
            .HasKey(lq => new { lq.QuizId, lq.LearnerUserId });

        modelBuilder.Entity<LearnerQuizz>()
            .HasOne(lq => lq.Quiz)
            .WithMany(q => q.AudienceLearners)
            .HasForeignKey(lq => lq.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LearnerQuizz>()
            .HasOne(lq => lq.Learner)
            .WithMany()
            .HasForeignKey(lq => lq.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Tutor)
            .WithMany()
            .HasForeignKey(q => q.TutorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Offering)
            .WithMany()
            .HasForeignKey(q => q.OfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.StudyMaterial)
            .WithMany()
            .HasForeignKey(q => q.StudyMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizQuestion>()
            .HasOne(q => q.Quiz)
            .WithMany(quiz => quiz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizQuestionOption>()
            .HasOne(o => o.QuizQuestion)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizAttempt>()
            .HasOne(a => a.Quiz)
            .WithMany()
            .HasForeignKey(a => a.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizAttempt>()
            .HasOne(a => a.Learner)
            .WithMany()
            .HasForeignKey(a => a.LearnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasOne(a => a.QuizAttempt)
            .WithMany(qa => qa.Answers)
            .HasForeignKey(a => a.QuizAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasOne(a => a.QuizQuestion)
            .WithMany()
            .HasForeignKey(a => a.QuizQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizAttemptSelectedOption>()
            .HasOne(o => o.QuizAttemptAnswer)
            .WithMany(a => a.SelectedOptions)
            .HasForeignKey(o => o.QuizAttemptAnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizAttemptSelectedOption>()
            .HasOne(o => o.QuizQuestionOption)
            .WithMany()
            .HasForeignKey(o => o.QuizQuestionOptionId)
            .OnDelete(DeleteBehavior.Restrict);


        // ───────────────── Seed data ─────────────────

        // Single TscSettings seed — 7:00–21:00 outer bound, 60–150 min sessions,
        // per the Offering/Session redesign lock-in.
        modelBuilder.Entity<TscSettings>().HasData(
            new TscSettings
            {
                TscSettingsId = 1,
                EarliestTeachingTime = new TimeSpan(7, 0, 0),
                LatestTeachingTime = new TimeSpan(21, 0, 0),
                MinSessionDurationMinutes = 60,
                MaxSessionDurationMinutes = 150,
                StandardSessionDurationMinutes = 120,
                MaxSessionsPerTutorPerDay = 4,
                MaxTutoringHoursPerTutorPerDay = 8,
                MaxSubjectsPerTutorPerDay = 2,
                MaxSubjectsPerTutorPerWeek = 2,
                MinBreakBetweenSessionsMinutes = 30,
                MaxConsecutiveSessionsBeforeLongerBreak = 2,
                LongerBreakMinutes = 60,
                MaxWeeklyTutoringHours = 40
            }
        );

        modelBuilder.Entity<OfferingTimeWindow>().HasData(
            new OfferingTimeWindow { OfferingTimeWindowId = 1, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 2, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(20, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 3, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(18, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 4, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Physical, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 5, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 6, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Remote, OfferingType = OfferingType.OneOnOne, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 7, DayType = DayType.Weekday, DeliveryMethod = DeliveryMethod.Hybrid, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(15, 30, 0), WindowEnd = new TimeSpan(17, 0, 0) },
            new OfferingTimeWindow { OfferingTimeWindowId = 8, DayType = DayType.Weekend, DeliveryMethod = DeliveryMethod.Hybrid, OfferingType = OfferingType.Group, WindowStart = new TimeSpan(7, 0, 0), WindowEnd = new TimeSpan(17, 0, 0) }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Learner" },
            new Role { RoleId = 2, RoleName = "Tutor" },
            new Role { RoleId = 3, RoleName = "Parent" },
            new Role { RoleId = 4, RoleName = "Admin" }
        );

        modelBuilder.Entity<Subject>().HasData(
            new Subject { SubjectId = 1, SubjectName = "Mathematics" },
            new Subject { SubjectId = 2, SubjectName = "Physical Sciences" },
            new Subject { SubjectId = 3, SubjectName = "Life Sciences" },
            new Subject { SubjectId = 4, SubjectName = "Mathematical Literacy" },
            new Subject { SubjectId = 5, SubjectName = "Accounting" }
        );

        modelBuilder.Entity<SubjectGrade>().HasData(
            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade10 },
            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade11 },
            new SubjectGrade { SubjectId = 1, Grade = Grade.Grade12 }, // Mathematics: 10-12
            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade10 },
            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade11 },
            new SubjectGrade { SubjectId = 2, Grade = Grade.Grade12 }, // Physical Sciences: 10-12
            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade10 },
            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade11 },
            new SubjectGrade { SubjectId = 3, Grade = Grade.Grade12 }, // Life Sciences: 10-12
            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade10 },
            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade11 },
            new SubjectGrade { SubjectId = 4, Grade = Grade.Grade12 }, // Maths Lit: 10-12
            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade10 },
            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade11 },
            new SubjectGrade { SubjectId = 5, Grade = Grade.Grade12 }  // Accounting: 10-12
        );

        modelBuilder.Entity<SubjectGrade>()
            .HasKey(sg => new { sg.SubjectId, sg.Grade });

        modelBuilder.Entity<SubjectGrade>()
            .HasOne(sg => sg.Subject)
            .WithMany(s => s.Grades)
            .HasForeignKey(sg => sg.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>().HasData(new User
        {
            UserId = 1,
            FirstName = "Admin",
            LastName = "AdminVila",
            Email = "lwandinhlengethwa25@gmail.com",
            PhoneNumber = "0000000000", // placeholder - replace with Michael's real number
            PasswordHash = "AQAAAAEAAYagAAAAEIvwxMAJ/t2/rwo6a30o39WIwMzC2nLntC98owiYso24zSx1fLtYAYjOcSRddprXUw==",
            DateCreated = new DateTime(2026, 1, 1),
            IsActive = true
        });

        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            UserId = 1,
            RoleId = 4 // Admin, per your existing Role seed
        });

        modelBuilder.Entity<Administrator>().HasData(new Administrator
        {
            UserId = 1,
            Position = "Owner / Head Tutor"
        });

        modelBuilder.Entity<TutorApplication>()
            .HasIndex(a => a.ReferenceNumber)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}