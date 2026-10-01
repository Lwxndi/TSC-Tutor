
using Tutor_Manager.Models;

namespace Tutor_Manager.Services.Email
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public EmailMessage Build(EmailType type, string toEmail, Dictionary<string, string> data)
        {
            return type switch
            {
                EmailType.RegistrationConfirmation => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Welcome to The Science Community!",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"Welcome to The Science Community! Your registration is complete and your TSC number is {data["TscNumber"]}.\n\n" +
                           $"Please keep this number handy — you'll need it for any queries with our team, and it's also how a parent or guardian can link their account to yours if they haven't already.\n\n" +
                           $"You can now log in to your dashboard to view your subjects and, once available, your session schedule.\n\n" +
                           $"If you have any questions, feel free to reach out to us.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.GuardianLinked => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "You've been linked to your learner's account",
                    Body = $"Hi {data["GuardianName"]},\n\n" +
                           $"Good news — your account has been successfully linked to {data["LearnerName"]} (TSC number: {data["TscNumber"]}).\n\n" +
                           $"You can now log in to your dashboard to keep track of their progress and stay updated on their sessions.\n\n" +
                           $"If anything looks incorrect, please contact us and we'll sort it out.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.GuardianUnlinked => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Almost there — let's link your account",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"Thanks for registering! We weren't able to find a learner with the TSC number {data["TscNumber"]}, so your account hasn't been linked to a learner profile yet.\n\n" +
                           $"This is usually just a typo, or the learner may not be registered yet. Here's what to do next:\n\n" +
                           $"1. Log in to your dashboard.\n" +
                           $"2. Go to \"Link a Learner\" and double-check the TSC number with your learner.\n" +
                           $"3. If your learner hasn't registered yet, ask them to do so first — you'll then be able to link using their TSC number.\n\n" +
                           $"If you're still having trouble, just reply to this email and we'll help you sort it out.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.SessionBooked => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Your session is confirmed",
                    Body = $"Hi,\n\n" +
                           $"This confirms your upcoming session:\n\n" +
                           $"Subject: {data["Subject"]}\n" +
                           $"Date: {data["Date"]}\n" +
                           $"Location: {data["Location"]}\n\n" +
                           $"Please arrive a few minutes early, and bring any materials your tutor may have recommended.\n\n" +
                           $"If you need to reschedule or have any questions, please contact us as soon as possible.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.PaymentReceived => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Payment received — thank you!",
                    Body = $"Hi,\n\n" +
                           $"We've successfully received your payment of R{data["Amount"]}. Thank you for your prompt payment.\n\n" +
                           $"A record of this payment is available on your dashboard under Payment History.\n\n" +
                           $"If you believe there's a mistake with this amount, please contact us right away.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.TutorApplicationRejected => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Update on your TSC Tutor Application",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"Thank you for taking the time to apply to become a tutor with The Science Community (Reference: {data["ReferenceNumber"]}).\n\n" +
                           $"After careful review, we've decided not to move forward with your application at this time. This decision doesn't necessarily reflect your qualifications or abilities — we often receive more applications than positions available, or are looking for specific subject/grade combinations at this time.\n\n" +
                           $"We genuinely appreciate the effort you put into your application, and we'd encourage you to apply again in the future should a suitable opportunity arise.\n\n" +
                           $"Wishing you all the best going forward.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.TutorApplicationChangesRequired => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Action needed on your TSC Tutor Application",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"Thank you for your application to become a tutor with The Science Community (Reference: {data["ReferenceNumber"]}).\n\n" +
                           $"We're currently reviewing your application, but we need a bit more information before we can proceed:\n\n" +
                           $"{data["Notes"]}\n\n" +
                           $"Here's what to do next:\n\n" +
                           $"1. Log back in to your application using your reference number above.\n" +
                           $"2. Update the section(s) mentioned.\n" +
                           $"3. Resubmit your application once you're done.\n\n" +
                           $"If anything is unclear, feel free to reply to this email and we'll be happy to help.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.TutorApplicationApproved => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Congratulations — you're officially a TSC Tutor!",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"Congratulations! We're delighted to let you know that your application to become a tutor with The Science Community (Reference: {data["ReferenceNumber"]}) has been approved.\n\n" +
                           $"Your Tutor Number is: {data["TutorNumber"]}\n\n" +
                           $"To get started, you'll need to activate your account and set your own password:\n\n" +
                           $"1. Click the activation link below.\n" +
                           $"2. Choose a secure password for your account.\n" +
                           $"3. Log in and complete your tutor profile.\n\n" +
                           $"Activate your account here: {data["ActivationLink"]}\n\n" +
                           $"Please note this link is valid for 48 hours. If it expires before you activate your account, please contact us and we'll send you a new one.\n\n" +
                           $"We're excited to have you on board and can't wait to see the impact you'll make with our learners!\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.AdminAccountCreated => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "Your TSC Admin Account Has Been Created",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"An administrator account has been created for you at The Science Community. This gives you access to manage tutor applications, oversee accounts, and support the day-to-day running of the platform.\n\n" +
                           $"To get started, you'll need to activate your account and set your own password:\n\n" +
                           $"1. Click the activation link below.\n" +
                           $"2. Choose a secure password for your account.\n" +
                           $"3. Log in to access your admin dashboard.\n\n" +
                           $"Activate your account here: {data["ActivationLink"]}\n\n" +
                           $"Please note this link is valid for 48 hours. If it expires before you activate your account, please ask an existing admin to assist you.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },
                EmailType.OfferingAssigned => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "You've been assigned a new tutoring offering",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"You've been assigned to teach a new tutoring offering:\n\n" +
                           $"Subject: {data["Subject"]} (Grade {data["Grade"]})\n" +
                           $"Type: {data["Type"]}\n" +
                           $"Teaching Days: {data["TeachingDays"]}\n" +
                           $"Delivery: {data["DeliveryMethod"]}\n\n" +
                           $"Please log in to your dashboard to review the details.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },

                EmailType.OfferingDeactivated => new EmailMessage
                {
                    ToEmail = toEmail,
                    Subject = "A tutoring offering has been discontinued",
                    Body = $"Hi {data["FirstName"]},\n\n" +
                           $"The following tutoring offering has been discontinued and is no longer active:\n\n" +
                           $"Subject: {data["Subject"]} (Grade {data["Grade"]})\n\n" +
                           $"If you believe this is a mistake, please contact TSC administration.\n\n" +
                           $"Warm regards,\nThe Science Community Team"
                },
                _ => throw new NotImplementedException($"No template defined for {type}")
            };
        }
    }
}