document.addEventListener("DOMContentLoaded", function () {
    const quoteText = document.getElementById("testimonialQuote");
    const authorName = document.getElementById("testimonialAuthor");
    const authorRole = document.getElementById("testimonialRole");
    const avatar = document.getElementById("testimonialAvatar");
    const tabs = document.querySelectorAll(".testimonial-tab");

    const testimonials = {
        "lerato": {
            quote: "Before The Science Community, I was failing Maths. After just one term with Michael, I got 78%. The group sessions made everything click — you learn from your peers too, not just the tutor.",
            name: "Lerato Dlamini",
            role: "Grade 11 Student, Mbombela",
            initials: "LD"
        },
        "grace": {
            quote: "The dashboard gives me peace of mind as a parent. I can easily monitor my child's attendance and assessment results without bothering the teachers.",
            name: "Grace Mokoena",
            role: "Parent, Cape Town",
            initials: "GM"
        },
        "sipho": {
            quote: "Physical Sciences was intimidating until I joined the online hybrid classes. The notes and recorded sessions helped me pass with distinction.",
            name: "Sipho Khumalo",
            role: "Grade 12 Student, Mbombela",
            initials: "SK"
        },
        "ayanda": {
            quote: "The group-based teaching model keeps costs accessible while delivering university-level academic guidance.",
            name: "Ayanda Nkosi",
            role: "Grade 10 Student, Cape Town",
            initials: "AN"
        }
    };

    tabs.forEach(tab => {
        tab.addEventListener("click", function () {
            tabs.forEach(t => t.classList.remove("active"));
            this.classList.add("active");

            const key = this.getAttribute("data-key");
            const data = testimonials[key];

            if (data) {
                quoteText.textContent = `"${data.quote}"`;
                authorName.textContent = data.name;
                authorRole.textContent = data.role;
                avatar.textContent = data.initials;
            }
        });
    });
});