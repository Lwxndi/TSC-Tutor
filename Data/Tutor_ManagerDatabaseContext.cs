//using Microsoft.EntityFrameworkCore;
//using Tutor_Manager.Models;

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

//    //Tutor Application related tables
//    public DbSet<TutorApplication> TutorApplications { get; set; }
//    public DbSet<TutorApplicationSubject> TutorApplicationSubjects { get; set; }
//    public DbSet<TutorApplicationExperience> TutorApplicationExperiences { get; set; }
//    public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<UserRole>()
//            .HasKey(ur => new { ur.UserId, ur.RoleId });

//        modelBuilder.Entity<LearnerGuardian>()
//            .HasKey(lg => new { lg.LearnerUserId, lg.ParentUserId });

//        modelBuilder.Entity<LearnerGuardian>()
//            .HasOne(lg => lg.Parent)
//            .WithMany(p => p.Learners)
//            .HasForeignKey(lg => lg.ParentUserId)
//            .OnDelete(DeleteBehavior.NoAction);


//        modelBuilder.Entity<TutorSubject>()
//            .HasKey(ts => new { ts.TutorUserId, ts.SubjectId, ts.GradeLevel });

//        modelBuilder.Entity<LearnerSubject>()
//            .HasKey(ls => new { ls.LearnerUserId, ls.SubjectId });

//        modelBuilder.Entity<Role>().HasData(
//            new Role { RoleId = 1, RoleName = "Learner" },
//            new Role { RoleId = 2, RoleName = "Tutor" },
//            new Role { RoleId = 3, RoleName = "Parent" },
//            new Role { RoleId = 4, RoleName = "Admin" }
//        );

//        modelBuilder.Entity<Subject>().HasData(
//               new Subject { SubjectId = 1, SubjectName = "Mathematics" },
//               new Subject { SubjectId = 2, SubjectName = "Physical Sciences" },
//               new Subject { SubjectId = 3, SubjectName = "Life Sciences" },
//               new Subject { SubjectId = 4, SubjectName = "English Home Language" },
//               new Subject { SubjectId = 5, SubjectName = "Accounting" }
//           );

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<LearnerGuardian>()
            .HasKey(lg => new { lg.LearnerUserId, lg.ParentUserId });

        modelBuilder.Entity<LearnerGuardian>()
            .HasOne(lg => lg.Parent)
            .WithMany(p => p.Learners)
            .HasForeignKey(lg => lg.ParentUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TutorSubject>()
            .HasKey(ts => new { ts.TutorUserId, ts.SubjectId, ts.GradeLevel });

        // Prevent multiple cascade paths into TutorSubject (same issue as LearnerGuardian above)
        modelBuilder.Entity<TutorSubject>()
            .HasOne(ts => ts.Tutor)
            .WithMany(t => t.SubjectsTaught)
            .HasForeignKey(ts => ts.TutorUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TutorSubject>()
            .HasOne(ts => ts.Subject)
            .WithMany(s => s.Tutors)
            .HasForeignKey(ts => ts.SubjectId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<LearnerSubject>()
            .HasKey(ls => new { ls.LearnerUserId, ls.SubjectId });

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