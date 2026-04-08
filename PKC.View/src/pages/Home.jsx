import { useNavigate } from 'react-router-dom';
import { useTheme } from '../ThemeContext';
import { getToken } from '../api';

const FEATURES = [
  {
    tag: 'Search',
    title: 'Natural language retrieval',
    desc: 'Query your entire knowledge base in plain English. Vector similarity finds what you mean, not just what you typed.',
  },
  {
    tag: 'Embeddings',
    title: 'Fully offline AI',
    desc: 'Embeddings run on-device via Ollama. No external APIs, no per-call cost, no data leaving your server.',
  },
  {
    tag: 'Connections',
    title: 'Automatic link discovery',
    desc: 'Every resource saved is silently compared against your entire library. Relationships form without any manual tagging.',
  },
  {
    tag: 'Resurface',
    title: 'Proactive memory',
    desc: 'Forgotten content is surfaced the moment something new relates to it - like spaced repetition on autopilot.',
  },
  {
    tag: 'RAG',
    title: 'Synthesised answers',
    desc: 'Ask a question, get a cited answer drawn exclusively from your saved content. No hallucinations from generic web data.',
  },
  {
    tag: 'Privacy',
    title: 'Self-hosted by design',
    desc: 'Your documents never leave your machine. The entire AI stack runs locally via Ollama.',
  },
];

const PIPELINE = [
  { n: '01', name: 'Ingest', detail: '202 Accepted - instant response' },
  { n: '02', name: 'Extract', detail: 'HTML stripped, PDF parsed to text' },
  { n: '03', name: 'Chunk', detail: '500-word segments, sentence-aware' },
  { n: '04', name: 'Embed', detail: 'Ollama generates 768-dim vectors' },
  { n: '05', name: 'Connect', detail: 'pgvector cosine similarity scan' },
  { n: '06', name: 'Ready', detail: 'Fully indexed and queryable' },
];

const TECH = [
  ['Backend', 'ASP.NET Core 10 · C# · Entity Framework Core 10'],
  ['Database', 'PostgreSQL 16 · pgvector'],
  ['AI / ML', 'Ollama'],
  ['Parsing', 'HtmlAgilityPack · PdfPig'],
  ['Frontend', 'React · Vite · D3.js'],
];

export default function Home() {
  const navigate = useNavigate();
  const { theme, toggle } = useTheme();
  const isLoggedIn = !!getToken();

  return (
    <div style={{ background: 'var(--bg-1)', color: 'var(--text-1)', minHeight: '100vh', fontFamily: "'Georgia', 'Times New Roman', serif" }}>

      <style>{`
        .home-nav-2 {
          position: fixed;
          top: 0; left: 0; right: 0;
          z-index: 100;
          display: flex;
          align-items: center;
          justify-content: space-between;
          padding: 0 48px;
          height: 60px;
          border-bottom: 1px solid var(--border);
          background: var(--bg-1);
          backdrop-filter: blur(12px);
        }
        .nav-logo-2 {
          font-family: 'Georgia', serif;
          font-size: 18px;
          font-weight: 700;
          letter-spacing: -0.02em;
          color: var(--text-1);
        }
        .nav-logo-2 span {
          color: var(--accent);
        }
        .nav-right {
          display: flex;
          align-items: center;
          gap: 8px;
        }
        .nav-link {
          font-family: system-ui, sans-serif;
          font-size: 13px;
          color: var(--text-2);
          background: none;
          border: none;
          cursor: pointer;
          padding: 6px 12px;
          border-radius: 6px;
          transition: color 0.15s, background 0.15s;
          letter-spacing: 0;
        }
        .nav-link:hover { color: var(--text-1); background: var(--bg-2); }
        .nav-btn-primary {
          font-family: system-ui, sans-serif;
          font-size: 13px;
          background: var(--text-1);
          color: var(--bg-1);
          border: none;
          cursor: pointer;
          padding: 7px 16px;
          border-radius: 6px;
          font-weight: 500;
          transition: opacity 0.15s;
        }
        .nav-btn-primary:hover { opacity: 0.85; }
        .theme-btn {
          background: none;
          border: 1px solid var(--border);
          color: var(--text-2);
          cursor: pointer;
          width: 32px; height: 32px;
          border-radius: 6px;
          font-size: 14px;
          display: flex; align-items: center; justify-content: center;
          transition: border-color 0.15s, color 0.15s;
        }
        .theme-btn:hover { border-color: var(--text-3); color: var(--text-1); }

        /*  HERO  */
        .hero-2 {
          padding-top: 140px;
          padding-bottom: 100px;
          padding-left: 48px;
          padding-right: 48px;
          max-width: 1200px;
          margin: 0 auto;
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 80px;
          align-items: center;
        }
        .hero-eyebrow {
          font-family: system-ui, sans-serif;
          font-size: 11px;
          font-weight: 600;
          letter-spacing: 0.1em;
          text-transform: uppercase;
          color: var(--accent);
          margin-bottom: 20px;
        }
        .hero-h1 {
          font-family: 'Georgia', 'Times New Roman', serif;
          font-size: clamp(36px, 4vw, 56px);
          font-weight: 700;
          line-height: 1.1;
          letter-spacing: -0.03em;
          color: var(--text-1);
          margin: 0 0 24px;
        }
        .hero-h1 em {
          font-style: italic;
          color: var(--accent);
        }
        .hero-body {
          font-family: system-ui, sans-serif;
          font-size: 15px;
          line-height: 1.7;
          color: var(--text-2);
          margin: 0 0 36px;
          max-width: 440px;
        }
        .hero-actions {
          display: flex;
          gap: 12px;
          flex-wrap: wrap;
        }
        .btn-hero-primary {
          font-family: system-ui, sans-serif;
          font-size: 14px;
          font-weight: 500;
          background: var(--text-1);
          color: var(--bg-1);
          border: none;
          cursor: pointer;
          padding: 12px 24px;
          border-radius: 6px;
          transition: opacity 0.15s;
          letter-spacing: -0.01em;
        }
        .btn-hero-primary:hover { opacity: 0.85; }
        .btn-hero-ghost {
          font-family: system-ui, sans-serif;
          font-size: 14px;
          font-weight: 400;
          background: transparent;
          color: var(--text-2);
          border: 1px solid var(--border);
          cursor: pointer;
          padding: 12px 24px;
          border-radius: 6px;
          transition: border-color 0.15s, color 0.15s;
        }
        .btn-hero-ghost:hover { color: var(--text-1); border-color: var(--text-3); }

        /*  APP PREVIEW  */
        .app-preview-wrap {
          position: relative;
        }
        .app-preview-frame {
          border: 1px solid var(--border);
          border-radius: 12px;
          overflow: hidden;
          background: var(--bg-2);
          box-shadow: 0 24px 80px rgba(0,0,0,0.35);
        }
        .preview-titlebar {
          display: flex;
          align-items: center;
          gap: 6px;
          padding: 10px 14px;
          border-bottom: 1px solid var(--border);
          background: var(--bg-1);
        }
        .preview-dot-2 {
          width: 10px; height: 10px;
          border-radius: 50%;
        }
        .preview-content {
          display: flex;
          height: 320px;
        }
        .preview-side {
          width: 130px;
          border-right: 1px solid var(--border);
          padding: 16px 12px;
          display: flex;
          flex-direction: column;
          gap: 4px;
          background: var(--bg-1);
        }
        .preview-logo-row {
          display: flex;
          align-items: center;
          gap: 6px;
          margin-bottom: 12px;
          padding: 0 4px;
        }
        .preview-logo-mark {
          width: 20px; height: 20px;
          border-radius: 4px;
          background: var(--accent);
        }
        .preview-logo-text {
          font-family: 'Georgia', serif;
          font-size: 11px;
          font-weight: 700;
          color: var(--text-1);
        }
        .prev-nav {
          height: 28px;
          border-radius: 5px;
          display: flex;
          align-items: center;
          padding: 0 8px;
          gap: 6px;
        }
        .prev-nav.active { background: var(--accent-dim); }
        .prev-nav-dot {
          width: 6px; height: 6px;
          border-radius: 50%;
          background: var(--text-3);
          flex-shrink: 0;
        }
        .prev-nav.active .prev-nav-dot { background: var(--accent); }
        .prev-nav-line {
          height: 7px;
          border-radius: 4px;
          background: var(--bg-3);
          flex: 1;
        }
        .prev-nav.active .prev-nav-line { background: var(--accent); opacity: 0.4; }
        .preview-body {
          flex: 1;
          padding: 16px;
          overflow: hidden;
        }
        .prev-stat-row {
          display: grid;
          grid-template-columns: repeat(3, 1fr);
          gap: 8px;
          margin-bottom: 16px;
        }
        .prev-stat-box {
          background: var(--bg-1);
          border: 1px solid var(--border);
          border-radius: 6px;
          padding: 8px;
        }
        .prev-stat-num {
          font-family: 'Georgia', serif;
          font-size: 18px;
          font-weight: 700;
          color: var(--text-1);
          line-height: 1;
        }
        .prev-stat-lbl {
          font-size: 9px;
          color: var(--text-3);
          margin-top: 2px;
          font-family: system-ui, sans-serif;
        }
        .prev-item-row {
          display: flex;
          align-items: center;
          gap: 8px;
          padding: 7px 8px;
          border-radius: 5px;
          margin-bottom: 4px;
          background: var(--bg-1);
          border: 1px solid var(--border);
        }
        .prev-status-dot {
          width: 6px; height: 6px;
          border-radius: 50%;
          flex-shrink: 0;
        }
        .prev-title-line {
          height: 7px;
          border-radius: 4px;
          background: var(--bg-3);
          flex: 1;
        }
        .prev-badge {
          height: 14px;
          width: 36px;
          border-radius: 99px;
          flex-shrink: 0;
        }

        /*  DIVIDER  */
        .section-divider {
          max-width: 1200px;
          margin: 0 auto;
          padding: 0 48px;
          border-top: 1px solid var(--border);
        }

        /*  FEATURES  */
        .features-2 {
          max-width: 1200px;
          margin: 0 auto;
          padding: 80px 48px;
        }
        .section-label {
          font-family: system-ui, sans-serif;
          font-size: 11px;
          font-weight: 600;
          letter-spacing: 0.1em;
          text-transform: uppercase;
          color: var(--text-3);
          margin-bottom: 16px;
        }
        .section-heading {
          font-family: 'Georgia', serif;
          font-size: clamp(24px, 3vw, 36px);
          font-weight: 700;
          letter-spacing: -0.03em;
          color: var(--text-1);
          margin: 0 0 48px;
          line-height: 1.2;
          max-width: 560px;
        }
        .features-grid-2 {
          display: grid;
          grid-template-columns: repeat(3, 1fr);
          gap: 0;
          border: 1px solid var(--border);
          border-radius: 10px;
          overflow: hidden;
        }
        .feature-cell {
          padding: 28px 28px;
          border-right: 1px solid var(--border);
          border-bottom: 1px solid var(--border);
          transition: background 0.15s;
        }
        .feature-cell:hover { background: var(--bg-2); }
        .feature-cell:nth-child(3n) { border-right: none; }
        .feature-cell:nth-child(4), .feature-cell:nth-child(5), .feature-cell:nth-child(6) { border-bottom: none; }
        .feature-tag {
          font-family: system-ui, sans-serif;
          font-size: 10px;
          font-weight: 600;
          letter-spacing: 0.08em;
          text-transform: uppercase;
          color: var(--accent);
          margin-bottom: 10px;
        }
        .feature-title-2 {
          font-family: 'Georgia', serif;
          font-size: 16px;
          font-weight: 700;
          color: var(--text-1);
          margin-bottom: 8px;
          letter-spacing: -0.01em;
          line-height: 1.3;
        }
        .feature-desc-2 {
          font-family: system-ui, sans-serif;
          font-size: 13px;
          line-height: 1.65;
          color: var(--text-3);
        }

        /*  PIPELINE  */
        .pipeline-2 {
          border-top: 1px solid var(--border);
          border-bottom: 1px solid var(--border);
          background: var(--bg-2);
          padding: 80px 48px;
        }
        .pipeline-inner {
          max-width: 1200px;
          margin: 0 auto;
        }
        .pipeline-steps-2 {
          display: grid;
          grid-template-columns: repeat(6, 1fr);
          gap: 0;
          margin-top: 48px;
          border: 1px solid var(--border);
          border-radius: 10px;
          overflow: hidden;
          background: var(--bg-1);
        }
        .pipeline-cell {
          padding: 24px 20px;
          border-right: 1px solid var(--border);
          position: relative;
        }
        .pipeline-cell:last-child { border-right: none; }
        .pipeline-num-2 {
          font-family: system-ui, sans-serif;
          font-size: 10px;
          font-weight: 700;
          letter-spacing: 0.08em;
          color: var(--accent);
          margin-bottom: 10px;
        }
        .pipeline-name-2 {
          font-family: 'Georgia', serif;
          font-size: 15px;
          font-weight: 700;
          color: var(--text-1);
          margin-bottom: 6px;
          letter-spacing: -0.01em;
        }
        .pipeline-detail {
          font-family: system-ui, sans-serif;
          font-size: 11.5px;
          color: var(--text-3);
          line-height: 1.5;
        }
        .pipeline-arrow {
          position: absolute;
          right: -7px;
          top: 50%;
          transform: translateY(-50%);
          width: 13px;
          height: 13px;
          background: var(--bg-2);
          border-right: 1px solid var(--border);
          border-top: 1px solid var(--border);
          transform: translateY(-50%) rotate(45deg);
          z-index: 2;
        }
        .pipeline-cell:last-child .pipeline-arrow { display: none; }

        /*  TECH STACK  */
        .tech-2 {
          max-width: 1200px;
          margin: 0 auto;
          padding: 80px 48px;
        }
        .tech-table {
          width: 100%;
          border-collapse: collapse;
          margin-top: 40px;
        }
        .tech-row {
          border-top: 1px solid var(--border);
        }
        .tech-row:last-child { border-bottom: 1px solid var(--border); }
        .tech-row td {
          padding: 16px 0;
          font-family: system-ui, sans-serif;
        }
        .tech-category {
          font-size: 11px;
          font-weight: 600;
          letter-spacing: 0.06em;
          text-transform: uppercase;
          color: var(--text-3);
          width: 140px;
          vertical-align: top;
          padding-top: 18px;
        }
        .tech-values {
          font-size: 14px;
          color: var(--text-1);
          line-height: 1.6;
        }

        /*  CTA  */
        .cta-2 {
          border-top: 1px solid var(--border);
          padding: 100px 48px;
          text-align: center;
        }
        .cta-h2 {
          font-family: 'Georgia', serif;
          font-size: clamp(28px, 4vw, 48px);
          font-weight: 700;
          letter-spacing: -0.03em;
          color: var(--text-1);
          margin: 0 0 16px;
          line-height: 1.1;
        }
        .cta-sub {
          font-family: system-ui, sans-serif;
          font-size: 15px;
          color: var(--text-2);
          margin: 0 0 36px;
          max-width: 480px;
          margin-left: auto;
          margin-right: auto;
          line-height: 1.6;
        }
        .cta-actions {
          display: flex;
          gap: 12px;
          justify-content: center;
          flex-wrap: wrap;
        }

        /*  FOOTER  */
        .footer-2 {
          border-top: 1px solid var(--border);
          padding: 28px 48px;
          display: flex;
          align-items: center;
          justify-content: space-between;
          max-width: 1200px;
          margin: 0 auto;
        }
        .footer-left {
          font-family: 'Georgia', serif;
          font-size: 13px;
          font-weight: 700;
          color: var(--text-2);
          letter-spacing: -0.01em;
        }
        .footer-right {
          font-family: system-ui, sans-serif;
          font-size: 11px;
          color: var(--text-3);
          letter-spacing: 0.02em;
        }

        @media (max-width: 900px) {
          .hero-2 { grid-template-columns: 1fr; padding-top: 100px; gap: 48px; }
          .features-grid-2 { grid-template-columns: 1fr 1fr; }
          .feature-cell:nth-child(3n) { border-right: 1px solid var(--border); }
          .feature-cell:nth-child(2n) { border-right: none; }
          .feature-cell:nth-child(4), .feature-cell:nth-child(5), .feature-cell:nth-child(6) { border-bottom: 1px solid var(--border); }
          .feature-cell:nth-child(5), .feature-cell:nth-child(6) { border-bottom: none; }
          .pipeline-steps-2 { grid-template-columns: repeat(3, 1fr); }
          .pipeline-cell { border-bottom: 1px solid var(--border); }
          .pipeline-cell:nth-child(3) { border-right: none; }
          .pipeline-cell:nth-child(4) { border-right: 1px solid var(--border); }
          .pipeline-cell:nth-child(5), .pipeline-cell:nth-child(6) { border-bottom: none; }
          .home-nav-2 { padding: 0 24px; }
          .hero-2, .features-2, .pipeline-inner, .tech-2 { padding-left: 24px; padding-right: 24px; }
          .footer-2 { padding: 24px; flex-direction: column; gap: 8px; text-align: center; }
        }
      `}</style>

      {/* NAV */}
      <nav className="home-nav-2">
        <div className="nav-logo-2">PK<span>C</span></div>
        <div className="nav-right">
          <button className="theme-btn" onClick={toggle} title="Toggle theme">
            {theme === 'dark' ? '☀' : '☾'}
          </button>
          {isLoggedIn ? (
            <button className="nav-btn-primary" onClick={() => navigate('/dashboard')}>
              Open dashboard
            </button>
          ) : (
            <>
              <button className="nav-link" onClick={() => navigate('/login')}>Sign in</button>
              <button className="nav-btn-primary" onClick={() => navigate('/register')}>
                Get started
              </button>
            </>
          )}
        </div>
      </nav>

      {/* HERO */}
      <div className="hero-2">

        {/* Left */}
        <div>
          <div className="hero-eyebrow">AI / Knowledge Management</div>
          <h1 className="hero-h1">
            The knowledge base<br />
            that <em>works</em> while<br />
            you sleep
          </h1>
          <p className="hero-body">
            Save any URL, PDF, or note. PKC automatically extracts, chunks,
            embeds, and connects your content - so when you need something
            you learned three months ago, it finds you first.
          </p>
          <div className="hero-actions">
            <button
              className="btn-hero-primary"
              onClick={() => navigate(isLoggedIn ? '/dashboard' : '/register')}
            >
              {isLoggedIn ? 'Go to dashboard' : 'Start for free'} →
            </button>
            {!isLoggedIn && (
              <button className="btn-hero-ghost" onClick={() => navigate('/login')}>
                Sign in
              </button>
            )}
          </div>
        </div>

        {/* Right - App Preview */}
        <div className="app-preview-wrap">
          <div className="app-preview-frame">
            <div className="preview-titlebar">
              <div className="preview-dot-2" style={{ background: '#ff5f57' }} />
              <div className="preview-dot-2" style={{ background: '#febc2e' }} />
              <div className="preview-dot-2" style={{ background: '#28c840' }} />
            </div>
            <div className="preview-content">

              {/* Sidebar */}
              <div className="preview-side">
                <div className="preview-logo-row">
                  <div className="preview-logo-mark" />
                  <span className="preview-logo-text">PKC</span>
                </div>
                {['Dashboard', 'Add Content', 'Library', 'Search', 'Resurface'].map((label, i) => (
                  <div key={label} className={`prev-nav${i === 0 ? ' active' : ''}`}>
                    <div className="prev-nav-dot" />
                    <div className="prev-nav-line" style={{ width: `${55 + i * 8}%` }} />
                  </div>
                ))}
              </div>

              {/* Main panel */}
              <div className="preview-body">
                <div className="prev-stat-row">
                  {[['24', 'Resources'], ['21', 'Ready'], ['3', 'Processing']].map(([n, l]) => (
                    <div key={l} className="prev-stat-box">
                      <div className="prev-stat-num">{n}</div>
                      <div className="prev-stat-lbl">{l}</div>
                    </div>
                  ))}
                </div>
                {[
                  { color: '#3ecf8e', badgeBg: '#3ecf8e18', w: '72%' },
                  { color: '#3ecf8e', badgeBg: '#3ecf8e18', w: '58%' },
                  { color: '#60aaff', badgeBg: '#60aaff18', w: '81%' },
                  { color: '#f5a623', badgeBg: '#f5a62318', w: '50%' },
                  { color: '#ff5555', badgeBg: '#ff555518', w: '65%' },
                ].map((row, i) => (
                  <div key={i} className="prev-item-row">
                    <div className="prev-status-dot" style={{ background: row.color }} />
                    <div className="prev-title-line" style={{ width: row.w }} />
                    <div className="prev-badge" style={{ background: row.badgeBg }} />
                  </div>
                ))}
              </div>

            </div>
          </div>
          {/* Subtle glow */}
          <div style={{
            position: 'absolute', bottom: -40, left: '50%', transform: 'translateX(-50%)',
            width: '60%', height: 80,
            background: 'var(--accent)',
            opacity: 0.06,
            filter: 'blur(40px)',
            borderRadius: '50%',
            pointerEvents: 'none',
          }} />
        </div>
      </div>

      {/* FEATURES */}
      <div className="section-divider" />
      <div className="features-2">
        <div className="section-label">Capabilities</div>
        <h2 className="section-heading">
          Everything your bookmarks folder was never going to be
        </h2>
        <div className="features-grid-2">
          {FEATURES.map((f) => (
            <div key={f.tag} className="feature-cell">
              <div className="feature-tag">{f.tag}</div>
              <div className="feature-title-2">{f.title}</div>
              <div className="feature-desc-2">{f.desc}</div>
            </div>
          ))}
        </div>
      </div>

      {/* PIPELINE */}
      <div className="pipeline-2">
        <div className="pipeline-inner">
          <div className="section-label">Processing pipeline</div>
          <h2 className="section-heading">
            From saved to searchable - automatically
          </h2>
          <p style={{ fontFamily: 'system-ui, sans-serif', fontSize: 14, color: 'var(--text-3)', maxWidth: 560, lineHeight: 1.6, margin: 0 }}>
            Every resource passes through a six-stage pipeline the moment you save it.
            No configuration. No manual steps. The API returns immediately - everything else happens in the background.
          </p>
          <div className="pipeline-steps-2">
            {PIPELINE.map((step, i) => (
              <div key={step.n} className="pipeline-cell">
                <div className="pipeline-num-2">{step.n}</div>
                <div className="pipeline-name-2">{step.name}</div>
                <div className="pipeline-detail">{step.detail}</div>
                {i < PIPELINE.length - 1 && <div className="pipeline-arrow" />}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* TECH STACK */}
      <div className="tech-2">
        <div className="section-label">Technology</div>
        <h2 className="section-heading">
          Production-grade stack, open source throughout
        </h2>
        <table className="tech-table">
          <tbody>
            {TECH.map(([cat, vals]) => (
              <tr key={cat} className="tech-row">
                <td className="tech-category">{cat}</td>
                <td className="tech-values">{vals}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* CTA */}
      <div className="cta-2">
        <h2 className="cta-h2">
          Stop losing what you learn.
        </h2>
        <p className="cta-sub">
          Build your personal knowledge base once. PKC compounds its value
          with every resource you add - connecting dots you would never have drawn yourself.
        </p>
        <div className="cta-actions">
          <button
            className="btn-hero-primary"
            onClick={() => navigate(isLoggedIn ? '/dashboard' : '/register')}
          >
            {isLoggedIn ? 'Open dashboard' : 'Create your account'} →
          </button>
          {!isLoggedIn && (
            <button className="btn-hero-ghost" onClick={() => navigate('/login')}>
              Sign in instead
            </button>
          )}
        </div>
      </div>

      {/* FOOTER */}
      <div style={{ borderTop: '1px solid var(--border)' }}>
        <div className="footer-2">
          <div className="footer-left">Personal Knowledge Curator</div>
          <div className="footer-right">
            ASP.NET Core 10 · PostgreSQL · pgvector · Ollama · React
          </div>
        </div>
      </div>

    </div>
  );
}