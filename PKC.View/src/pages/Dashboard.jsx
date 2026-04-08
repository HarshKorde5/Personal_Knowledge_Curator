import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiGetResources, apiGetNotifications, getEmail } from '../api';

function StatusBadge({ status }) {
  
  const s = String(status || '').toLowerCase();
  const isProcessing = !['ready', 'failed'].includes(s);
  return (
    <span className={`badge badge-${s}`}>
      <span className={`status-dot ${isProcessing ? 'dot-processing' : s === 'ready' ? 'dot-ready' : 'dot-failed'}`} />
      {status}
    </span>
  );
}

function TypeMark({ type }) {
  const map = { Url: 'URL', Note: 'TXT', Pdf: 'PDF' };
  const cls = { Url: 'item-icon-url', Note: 'item-icon-note', Pdf: 'item-icon-pdf' };
  return (
    <div className={`item-icon ${cls[type] || ''}`}>
      {map[type] || 'DOC'}
    </div>
  );
}

function timeAgo(dateStr) {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1)  return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24)  return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}

export default function Dashboard() {
  const navigate = useNavigate();
  const [resources, setResources] = useState([]);
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const email = getEmail() || '';

  useEffect(() => {
    Promise.all([apiGetResources(), apiGetNotifications()])
      .then(([its, notifs]) => { setResources(its); setNotifications(notifs); })
      .finally(() => setLoading(false));
  }, []);

  const ready      = resources.filter(i => i.status === 'Ready').length;
  const processing = resources.filter(i => !['Ready', 'Failed'].includes(i.status)).length;
  const failed     = resources.filter(i => i.status === 'Failed').length;
  const unread     = notifications.filter(n => n.unread).length;
  const recent     = [...resources].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).slice(0, 5);
  const indexedPct = resources.length > 0 ? Math.round((ready / resources.length) * 100) : 0;

  if (loading) return (
    <div className="flex-center" style={{ height: '60vh' }}>
      <div className="spinner" style={{ width: 24, height: 24 }} />
    </div>
  );

  return (
    <div className="fade-in">
      <div className="page-header">
        <h2>Dashboard</h2>
        <p>
          {email ? `Welcome back, ${email.split('@')[0]}.` : 'Welcome back.'} Your knowledge base at a glance.
        </p>
      </div>

      {/* Stats */}
      <div className="stats-grid">
        {[
          { label: 'Total resources',    value: resources.length, sub: 'saved',           color: null },
          { label: 'Indexed',        value: ready,        sub: 'ready to query',  color: 'var(--ready)' },
          { label: 'Processing',     value: processing,   sub: 'in pipeline',     color: 'var(--processing)' },
          { label: 'Resurfaces',     value: unread,       sub: 'unread',          color: unread > 0 ? 'var(--accent)' : null },
        ].map(row => (
          <div className="stat-card" key={row.label}>
            <div className="stat-label">{row.label}</div>
            <div className="stat-value" style={{ color: row.color || 'var(--text-1)' }}>{row.value}</div>
            <div className="stat-sub">{row.sub}</div>
          </div>
        ))}
      </div>

      {/* Quick actions */}
      <div className="card" style={{ marginBottom: 20 }}>
        <div className="section-header">
          <span className="section-title">Quick actions</span>
        </div>
        <div className="flex gap-8" style={{ flexWrap: 'wrap' }}>
          <button className="btn btn-primary" onClick={() => navigate('/ingest')}>
            Add content
          </button>
          <button className="btn btn-secondary" onClick={() => navigate('/search')}>
            Ask a question
          </button>
          <button className="btn btn-secondary" onClick={() => navigate('/library')}>
            Browse library
          </button>
          {unread > 0 && (
            <button
              className="btn btn-secondary"
              style={{ borderColor: 'var(--accent-border)', color: 'var(--accent)' }}
              onClick={() => navigate('/notifications')}
            >
              {unread} new resurface{unread > 1 ? 's' : ''}
            </button>
          )}
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>

        {/* Recent resources */}
        <div className="card">
          <div className="section-header">
            <span className="section-title">Recent resources</span>
            <button className="btn btn-ghost btn-sm" onClick={() => navigate('/library')}>View all</button>
          </div>
          <div className="activity-list">
            {recent.length === 0 ? (
              <div style={{ padding: '20px 0', textAlign: 'center', color: 'var(--text-3)', fontSize: 13 }}>
                No resources yet - add your first content above.
              </div>
            ) : recent.map(resource => (
              <div
                key={resource.id}
                className="activity-resource"
                style={{ cursor: 'pointer' }}
                onClick={() => navigate('/library')}
              >
                <TypeMark type={resource.type} />
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-1)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', letterSpacing: '-0.01em', marginBottom: 4 }}>
                    {resource.title || 'Untitled'}
                  </div>
                  <StatusBadge status={resource.status} />
                </div>
                <span className="activity-time">{timeAgo(resource.createdAt)}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Pipeline */}
        <div className="card">
          <div className="section-header">
            <span className="section-title">Pipeline</span>
          </div>

          {/* Progress */}
          <div style={{ marginBottom: 20 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 11.5, color: 'var(--text-3)', marginBottom: 7 }}>
              <span>Indexed</span>
              <span style={{ fontFamily: 'var(--mono)', color: 'var(--text-2)' }}>{indexedPct}%</span>
            </div>
            <div style={{ background: 'var(--bg-3)', borderRadius: 2, height: 3, overflow: 'hidden' }}>
              <div style={{
                height: '100%',
                background: 'var(--accent)',
                width: `${indexedPct}%`,
                borderRadius: 2,
                transition: 'width 0.8s var(--ease)',
              }} />
            </div>
          </div>

          {/* Breakdown */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
            {[
              { label: 'Ready to query',  count: ready,      color: 'var(--ready)' },
              { label: 'Processing',      count: processing, color: 'var(--processing)' },
              { label: 'Failed',          count: failed,     color: 'var(--failed)' },
            ].map(row => (
              <div key={row.label} style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13, color: 'var(--text-2)' }}>
                  <span style={{ width: 6, height: 6, borderRadius: '50%', background: row.color, display: 'inline-block', flexShrink: 0 }} />
                  {row.label}
                </div>
                <span style={{ fontFamily: 'var(--mono)', fontSize: 13, color: 'var(--text-1)' }}>{row.count}</span>
              </div>
            ))}
          </div>

          <hr className="divider" />

          <div style={{ fontSize: 12, color: 'var(--text-3)', lineHeight: 1.65 }}>
            Ingest → Extract → Chunk → Embed → Connect → Ready.
            All processing is async and runs in the background.
          </div>
        </div>
      </div>

      {/* Resurface preview */}
      {unread > 0 && (
        <div className="card" style={{ marginTop: 16 }}>
          <div className="section-header">
            <span className="section-title">Resurfaced knowledge</span>
            <button className="btn btn-ghost btn-sm" onClick={() => navigate('/notifications')}>See all</button>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            {notifications.filter(n => n.unread).slice(0, 2).map(n => (
              <div key={n.id} style={{
                padding: '12px 14px',
                background: 'var(--accent-dim)',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--accent-border)',
              }}>
                <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-1)', marginBottom: 3, letterSpacing: '-0.01em' }}>
                  {n.title}
                </div>
                <div style={{ fontSize: 12.5, color: 'var(--text-2)', lineHeight: 1.5 }}>
                  {n.description}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
