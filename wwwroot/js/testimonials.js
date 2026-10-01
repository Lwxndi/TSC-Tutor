const testimonials = {
  lerato: { q: "Before The Science Community, I was failing Maths. After just one term with Michael, I got 78%. The group sessions made everything click — you learn from your peers too, not just the tutor.", n: "Lerato Dlamini", r: "Grade 11 Student, Mbombela", a: "LD" },
  grace: { q: "As a parent I finally have visibility. I can see attendance and homework without asking — it changed how involved I could be.", n: "Grace Mokoena", r: "Parent, Cape Town", a: "GM" },
  sipho: { q: "The recorded lessons saved me before every test. I could rewatch the tricky parts of a Geography lesson as many times as I needed.", n: "Sipho Khumalo", r: "Grade 10 Student, Mbombela", a: "SK" },
  ayanda: { q: "Switching to hybrid sessions meant I didn't have to choose between tutoring and my part-time job.", n: "Ayanda Nkosi", r: "Grade 12 Student, Cape Town", a: "AN" }
};

document.addEventListener("DOMContentLoaded", function () {
  const tabs = document.querySelectorAll(".stage-tabs button");
  const quoteEl = document.getElementById("sQuote");
  const nameEl = document.getElementById("sName");
  const roleEl = document.getElementById("sRole");
  const avatarEl = document.getElementById("sAvatar");

  if (!tabs.length) return;

  tabs.forEach(function (btn) {
    btn.addEventListener("click", function () {
      tabs.forEach(function (x) { x.classList.remove("active"); });
      btn.classList.add("active");
      const t = testimonials[btn.dataset.key];
      if (!t) return;
      quoteEl.textContent = t.q;
      nameEl.textContent = t.n;
      roleEl.textContent = t.r;
      avatarEl.textContent = t.a;
    });
  });

  const contactForm = document.getElementById("homeContactForm");
  if (contactForm) {
    contactForm.addEventListener("submit", function (e) {
      e.preventDefault();
      const btn = contactForm.querySelector("button[type=submit]");
      if (btn) btn.textContent = "Sent ✓";
    });
  }
});