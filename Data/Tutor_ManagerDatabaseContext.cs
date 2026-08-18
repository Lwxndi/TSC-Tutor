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

        modelBuilder.Entity<LearnerSubject>()
            .HasKey(ls => new { ls.LearnerUserId, ls.SubjectId });

        base.OnModelCreating(modelBuilder);

        

    }
}