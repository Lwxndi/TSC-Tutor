# TSC-Tutor
A web-based tutoring platform for The Science Community, connecting tutors, learners, and parents across multiple locations (Cape Town &amp; Mbombela). Built with ASP.NET Core MVC, SQL Server, and Entity Framework Core to handle session scheduling, missed lesson tracking, and progress visibility as the business scales.



# TSC-Tutor

Web-based tutoring platform for **The Science Community**, a tutoring business run by Michael Vilankulu. Built for Grade 10–12 learners following the CAPS curriculum in South Africa.

## Why This Exists

Running a tutoring business day to day comes with a few real, recurring problems.

**Sessions happen across different locations.** Michael's learners aren't all in one place — some are based in Cape Town, others in Mbombela. That means sessions can't just be treated as one-size-fits-all in-person meetings; some have to happen remotely, while others are in person. Without a system that accounts for this properly, there's no clean way to keep track of which sessions are online, which are in person, and how each one actually needs to run.

**Learners sometimes miss sessions.** Life happens — a learner might not be available for a scheduled session and misses it. Right now, there's no simple way to flag that a session was missed, so it can be followed up on, rescheduled, or at least recorded so nothing falls through the cracks.

**Progress needs to be visible to everyone involved, not just Michael.** Parents want to know how their child is doing. Tutors need to see how their own learners are progressing across sessions. Michael needs a bigger-picture view across all tutors and learners. Right now, none of that is tracked anywhere consistent, so most of these questions get answered from memory, if they get answered at all.

**The business is growing.** As Michael takes on more tutors and more learners — spread across more locations — all of the above becomes harder to manage manually. More sessions to track, more learners who might miss a class, more people asking about progress. A system that handles this properly now means the business can keep growing without everything depending on Michael personally holding it all together.

This platform exists to solve those four problems directly: giving sessions proper structure (including remote vs. in-person), giving missed lessons a place to be tracked and followed up on, giving every user type visibility into progress, and building a foundation that can scale as the business grows.

## About the Project

This platform connects tutors, learners, and parents/guardians in one place. Michael acts as both Admin and Tutor, so the system supports users holding more than one role at the same time.

### User Types
- **Admin** – manages the platform (also Michael, who tutors)
- **Tutor** – teaches subjects, manages sessions
- **Learner** – attends sessions, tracks subjects
- **Parent/Guardian** – linked to one or more learners

## Tech Stack
- ASP.NET Core MVC
- SQL Server
- Entity Framework Core

## Status
This project is under active development as part of an academic integrated project. Current focus: controllers and ViewModels for registration and login.

## Getting Started
1. Clone the repo
2. Open the solution in Visual Studio
3. Update the connection string in `appsettings.json`
4. Run EF Core migrations via Package Manager Console
5. Build and run the project
