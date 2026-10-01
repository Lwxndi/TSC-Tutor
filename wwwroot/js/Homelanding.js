:root{
  --board:#2a211a; --board-2:#362b20; --paper:#efe9da; --paper-2:#e2d7bc;
  --ink:#2a211a; --ink-soft:#6b5c48; --chalk:#efe9da;
  --hi:#a9835a; --hi-deep:#6b4f28;
  --line:rgba(42,33,26,.15); --line-chalk:rgba(239,233,218,.18); --board-soft:#cbbfa8;
  --grot:'Space Grotesk',sans-serif; --serif:'Source Serif 4',Georgia,serif; --mono:'IBM Plex Mono',monospace;
}
.landing *{box-sizing:border-box}
.landing{background:var(--paper);color:var(--ink);font-family:var(--grot);line-height:1.5;-webkit-font-smoothing:antialiased;margin:0 -12px}
.landing img{max-width:100%;display:block}
.landing a{color:inherit}
.rail-section{display:grid;grid-template-columns:44px 1fr}
.rail{writing-mode:vertical-rl;transform:rotate(180deg);font-family:var(--mono);font-size:.72rem;letter-spacing:.08em;color:var(--ink-soft);border-right:1px solid var(--line);padding:1.6rem 0;text-align:right}
.rail.chalk{color:var(--line-chalk);border-color:var(--line-chalk)}
.inner{padding:3.2rem 2rem 3.6rem 1.8rem;max-width:1200px}
@media (max-width:700px){.rail-section{grid-template-columns:26px 1fr}.rail{font-size:.62rem}.inner{padding:2.2rem 1rem 2.6rem 1rem}}

/* NAV */
.landing .nav{display:flex;justify-content:space-between;align-items:center;padding:1.3rem 1.8rem}
.landing .mark{font-weight:700;font-size:1.05rem;letter-spacing:-.01em}
.landing .mark .sym{display:inline-block;border:1.5px solid var(--ink);width:1.9rem;height:1.9rem;text-align:center;line-height:1.75rem;font-family:var(--mono);font-size:.8rem;margin-right:.5rem;border-radius:3px}
.landing .nav-links{display:flex;gap:1.8rem;font-family:var(--mono);font-size:.78rem;color:var(--ink-soft)}
.landing .nav-links a{text-decoration:none}
@media (max-width:700px){.landing .nav-links{display:none}}
.landing .nav-cta{font-family:var(--mono);font-size:.78rem;border:1px solid var(--ink);padding:.5rem 1rem;text-decoration:none;border-radius:2px}

/* HERO */
.hero .inner{padding-top:1.5rem}
.hero-top{font-family:var(--mono);font-size:.75rem;color:var(--hi-deep);margin-bottom:1rem}
.hero h1{font-family:var(--grot);font-weight:700;font-size:clamp(2.6rem,7vw,5.2rem);line-height:.98;letter-spacing:-.02em;max-width:14ch;margin:0}
.hero-body{display:grid;grid-template-columns:1.15fr .85fr;gap:2.5rem;margin-top:2.2rem;align-items:end}
.hero-body p{font-family:var(--serif);font-size:1.15rem;color:var(--ink-soft);max-width:38ch}
.hero-actions{margin-top:1.6rem;display:flex;gap:1rem;flex-wrap:wrap}
.btn-fill{background:var(--ink);color:var(--paper);font-family:var(--mono);font-size:.82rem;padding:.85rem 1.5rem;text-decoration:none;border-radius:2px;display:inline-block}
.btn-line{font-family:var(--mono);font-size:.82rem;padding:.85rem 0;border-bottom:1.5px solid var(--hi-deep);text-decoration:none}
.hero-photo{position:relative;clip-path:polygon(0 6%,100% 0,100% 94%,0 100%)}
.hero-photo img{aspect-ratio:3/4;object-fit:cover}
@media (max-width:820px){.hero-body{grid-template-columns:1fr}.hero-photo{order:-1;max-width:280px}}

/* TICKER STATS */
.ticker-wrap{border-top:1px solid var(--line);border-bottom:1px solid var(--line);overflow:hidden;background:var(--paper-2)}
.ticker{display:flex;width:max-content;animation:tick 28s linear infinite}
@media (prefers-reduced-motion: reduce){.ticker{animation:none}}
@keyframes tick{from{transform:translateX(0)}to{transform:translateX(-50%)}}
.tick-item{font-family:var(--mono);font-size:.95rem;white-space:nowrap;padding:1rem 2.2rem;border-right:1px solid var(--line);display:flex;gap:.6rem;align-items:baseline}
.tick-item b{font-family:var(--grot);font-weight:700;font-size:1.3rem;color:var(--hi-deep)}

/* ABOUT */
.about .inner{display:grid;grid-template-columns:.7fr 1fr .8fr;gap:2.2rem}
.about h2{font-family:var(--grot);font-size:1.9rem;font-weight:600;line-height:1.15;margin:0}
.about p{font-family:var(--serif);color:var(--ink-soft);font-size:1.02rem;margin-bottom:.9rem}
.about-tags{display:flex;flex-direction:column;gap:.6rem;font-family:var(--mono);font-size:.78rem}
.about-tags span{border-bottom:1px solid var(--line);padding-bottom:.5rem}
@media (max-width:820px){.about .inner{grid-template-columns:1fr}}

/* TIMELINE STEPS */
.steps .inner{padding-top:2.6rem}
.steps h2{font-family:var(--grot);font-size:1.9rem;font-weight:600;margin-bottom:.6rem;margin-top:0}
.steps > .inner > p{font-family:var(--serif);color:var(--ink-soft);max-width:44ch;margin-bottom:2.6rem}
.tline{position:relative;padding-left:2.2rem;border-left:1.5px dashed var(--line)}
.trow{position:relative;padding:1.6rem 0;display:grid;grid-template-columns:auto 1fr;gap:1.6rem;align-items:start}
.trow:nth-child(even){grid-template-columns:1fr auto}
.trow:nth-child(even) .tbody{order:1;text-align:right}
.tdot{width:.6rem;height:.6rem;border-radius:50%;background:var(--hi-deep);position:absolute;left:-2.45rem;top:2rem}
.tno{font-family:var(--mono);font-size:.8rem;color:var(--hi-deep)}
.tbody h3{font-family:var(--grot);font-size:1.15rem;margin:.3rem 0 .35rem}
.tbody p{font-family:var(--serif);color:var(--ink-soft);font-size:.95rem;max-width:34ch;margin:0}
@media (max-width:700px){.trow,.trow:nth-child(even){grid-template-columns:1fr}.trow:nth-child(even) .tbody{text-align:left;order:0}}

/* PERIODIC SUBJECTS */
.periodic .inner{background:var(--board);color:var(--chalk);border-radius:4px}
.periodic h2{font-family:var(--grot);font-size:1.6rem;margin-bottom:1.6rem;margin-top:0}
.pgrid{display:grid;grid-template-columns:repeat(6,1fr);gap:.7rem}
.ptile{border:1px solid var(--line-chalk);border-radius:3px;padding:.8rem .7rem;min-height:100px;display:flex;flex-direction:column;justify-content:space-between}
.ptile .pi{font-family:var(--mono);font-size:.65rem;color:var(--hi)}
.ptile .ps{font-family:var(--grot);font-weight:700;font-size:1.5rem}
.ptile .pn{font-family:var(--mono);font-size:.62rem;color:var(--board-soft)}
@media (max-width:820px){.pgrid{grid-template-columns:repeat(3,1fr)}}

/* TUTORS FILMSTRIP */
.tutors .inner{padding-bottom:1rem}
.tutors h2{font-family:var(--grot);font-size:1.9rem;font-weight:600;margin-bottom:.4rem;margin-top:0}
.tutors > .inner > p{font-family:var(--mono);font-size:.8rem;color:var(--ink-soft);margin-bottom:1.8rem}
.filmstrip{display:flex;gap:1.1rem;overflow-x:auto;scroll-snap-type:x mandatory;padding-bottom:1rem;margin:0 -1.8rem;padding-left:1.8rem}
.film-card{scroll-snap-align:start;flex:0 0 220px;border:1px solid var(--line);border-radius:3px;overflow:hidden;background:var(--paper-2)}
.film-card img{aspect-ratio:3/4;object-fit:cover}
.film-info{padding:.9rem}
.film-info h4{font-family:var(--grot);font-size:.95rem;margin:0}
.film-info .role{font-family:var(--mono);font-size:.68rem;color:var(--hi-deep);margin:.25rem 0}
.film-info .subj{font-family:var(--serif);font-size:.85rem;color:var(--ink-soft)}
.film-card.hire{display:flex;flex-direction:column;justify-content:center;align-items:flex-start;padding:1.2rem;gap:.4rem}
.hbadge{font-family:var(--mono);font-size:.65rem;background:var(--hi);color:var(--ink);padding:.2rem .5rem;border-radius:2px}

/* TESTIMONIAL STAGE */
.stage{background:var(--board-2);color:var(--chalk);padding:5rem 1.8rem;position:relative;overflow:hidden}
.stage-inner{max-width:760px;margin:0 auto;position:relative}
.qmark{position:absolute;top:-4.5rem;left:-1.5rem;font-family:var(--serif);font-size:9rem;color:rgba(239,233,218,.08);line-height:1}
.stage p.quote{font-family:var(--serif);font-size:clamp(1.4rem,2.6vw,2rem);line-height:1.4;position:relative;z-index:1;margin:0}
.stage-author{display:flex;gap:.9rem;align-items:center;margin-top:2rem}
.savatar{width:2.6rem;height:2.6rem;border-radius:50%;background:var(--hi);color:var(--ink);display:flex;align-items:center;justify-content:center;font-family:var(--mono);font-weight:600;font-size:.8rem}
.stage-author strong{display:block;font-size:.92rem}
.stage-author span{font-family:var(--mono);font-size:.72rem;color:var(--board-soft)}
.stage-tabs{display:flex;gap:1.4rem;margin-top:2.2rem;flex-wrap:wrap}
.stage-tabs button{background:none;border:none;font-family:var(--mono);font-size:.72rem;color:var(--board-soft);cursor:pointer;padding-bottom:.25rem;border-bottom:1.5px solid transparent}
.stage-tabs button.active{color:var(--chalk);border-color:var(--hi)}

/* CONTACT */
.contact .inner{display:grid;grid-template-columns:.8fr 1fr;gap:3rem}
.contact h2{font-family:var(--grot);font-size:1.9rem;margin-bottom:.7rem;margin-top:0}
.contact-blurb{font-family:var(--serif);color:var(--ink-soft);max-width:38ch}
.citem{margin-top:1.5rem;font-family:var(--mono);font-size:.78rem}
.citem .l{color:var(--ink-soft);letter-spacing:.05em}
.citem strong{display:block;font-family:var(--grot);font-size:1rem;font-weight:600;margin-top:.15rem}
.lform label{font-family:var(--mono);font-size:.72rem;color:var(--ink-soft);letter-spacing:.03em;display:block;margin-bottom:.4rem}
.lform .fld{margin-bottom:1.4rem}
.lform input,.lform textarea{width:100%;background:transparent;border:none;border-bottom:1px solid var(--line);padding:.5rem 0;font-family:var(--grot);font-size:1rem;color:var(--ink)}
.lform input:focus,.lform textarea:focus{outline:none;border-color:var(--hi-deep);box-shadow:none}
.lform button{margin-top:.6rem;font-family:var(--mono);font-size:.82rem;background:var(--ink);color:var(--paper);border:none;padding:.85rem 1.6rem;border-radius:2px;cursor:pointer}
@media (max-width:820px){.contact .inner{grid-template-columns:1fr}}

/* FOOTER */
.landing footer{margin-top:0}
.landing footer .inner{display:flex;justify-content:space-between;flex-wrap:wrap;gap:2rem;padding-bottom:1.4rem}
.landing footer h5{font-family:var(--mono);font-size:.72rem;color:var(--ink-soft);margin-bottom:.7rem;letter-spacing:.05em}
.landing footer p{font-family:var(--serif);color:var(--ink-soft);max-width:28ch;font-size:.92rem}
.flinks{list-style:none;font-family:var(--mono);font-size:.78rem;padding:0}
.flinks li{margin-bottom:.5rem}
.flinks a{text-decoration:none;color:var(--ink-soft)}
.fbottom{border-top:1px solid var(--line);margin:0 1.8rem;padding:1.2rem 0;display:flex;justify-content:space-between;font-family:var(--mono);font-size:.72rem;color:var(--ink-soft);flex-wrap:wrap;gap:.5rem}