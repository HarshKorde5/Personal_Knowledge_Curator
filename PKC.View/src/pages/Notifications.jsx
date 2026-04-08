import { useEffect, useState } from 'react';
import { apiGetNotifications, apiMarkNotificationRead } from '../api';

const TYPE_COLORS = {
  resurface: 'var(--accent)',
  ready:     'var(--ready)',
  failed:    'var(--failed)',
};

const TYPE_LABELS = {
  resurface: 'Resurface',
  ready:     'Ready',
  failed:    'Failed',
};

export default function Notifications() {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading]             = useState(true);
  const [filter, setFilter]               = useState('all');

  useEffect(() => {
    apiGetNotifications()
      .then(setNotifications)
      .finally(() => setLoading(false));
  }, []);

  async function markRead(id) {
    await apiMarkNotificationRead(id);
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, unread: false } : n));
  }

  async function markAllRead() {
    const unread = notifications.filter(n => n.unread);
    await Promise.all(unread.map(n => apiMarkNotificationRead(n.id)));
    setNotifications(prev => prev.map(n => ({ ...n, unread: false })));
  }

  const filtered = notifications.filter(n => {
    if (filter === 'unread')   return n.unread;
    if (filter === 'resurface') return n.type === 'resurface';
    return true;
  });

  const unreadCount = notifications.filter(n => n.unread).length;

  if (loading) return (
    <div className="flex-center" style={{ height: '60vh' }}>
      <div className="spinner" style={{ width: 24, height: 24 }} />
    </div>
  );

  return (
    <div className="fade-in" style={{ maxWidth: 700 }}>
      <div className="page-header">
        <div className="flex-between">
          <div>
            <h2>Notifications</h2>
            <p>
              {unreadCount > 0
                ? `${unreadCount} unread - resurfaced knowledge and processing updates`
                : 'All caught up'}
            </p>
          </div>
          {unreadCount > 0 && (
            <button className="btn btn-secondary btn-sm" onClick={markAllRead}>
              Mark all read
            </button>
          )}
        </div>
      </div>

      {/* Filter tabs */}
      <div className="tabs">
        <button className={`tab-btn${filter === 'all' ? ' active' : ''}`}       onClick={() => setFilter('all')}>
          All ({notifications.length})
        </button>
        <button className={`tab-btn${filter === 'unread' ? ' active' : ''}`}    onClick={() => setFilter('unread')}>
          Unread ({unreadCount})
        </button>
        <button className={`tab-btn${filter === 'resurface' ? ' active' : ''}`} onClick={() => setFilter('resurface')}>
          Resurfaces
        </button>
      </div>

      {filtered.length === 0 ? (
        <div className="empty-state">
          <div className="empty-title">No notifications here</div>
          <div className="empty-sub">
            {filter === 'unread'
              ? "You're all caught up."
              : 'Resurface notifications appear when new content connects to older saved resources.'
            }
          </div>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 0 }}>
          {filtered.map(n => (
            <div
              key={n.id}
              className={`notif-card${n.unread ? ' unread' : ''}`}
              style={{ marginBottom: 0, borderRadius: 0, borderBottom: 'none', borderLeft: 'none', borderRight: 'none', borderTop: '1px solid var(--border)' }}
            >
              {/* Type indicator */}
              <div style={{
                width: 3,
                height: 36,
                borderRadius: 2,
                background: TYPE_COLORS[n.type] || 'var(--border-mid)',
                flexShrink: 0,
                marginTop: 2,
              }} />

              <div className="notif-body">
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
                  <div className="notif-title" style={{ margin: 0 }}>{n.title}</div>
                  <span style={{
                    fontSize: 10,
                    fontWeight: 600,
                    letterSpacing: '0.06em',
                    textTransform: 'uppercase',
                    color: TYPE_COLORS[n.type] || 'var(--text-3)',
                    background: n.type === 'resurface' ? 'var(--accent-dim)' : n.type === 'ready' ? 'var(--ready-dim)' : 'var(--failed-dim)',
                    padding: '2px 6px',
                    borderRadius: 'var(--radius-xs)',
                  }}>
                    {TYPE_LABELS[n.type] || n.type}
                  </span>
                  {n.unread && (
                    <span style={{
                      width: 6, height: 6,
                      borderRadius: '50%',
                      background: 'var(--accent)',
                      display: 'inline-block',
                      flexShrink: 0,
                    }} />
                  )}
                </div>

                <div className="notif-desc">{n.description}</div>

                {n.relatedResource && (
                  <div style={{
                    display: 'inline-flex',
                    alignItems: 'center',
                    gap: 6,
                    background: 'var(--bg-3)',
                    border: '1px solid var(--border)',
                    borderRadius: 'var(--radius-xs)',
                    padding: '3px 9px',
                    fontSize: 11.5,
                    color: 'var(--text-2)',
                    marginBottom: 10,
                    fontFamily: 'var(--mono)',
                  }}>
                    {n.relatedResource}
                  </div>
                )}

                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 8 }}>
                  <span className="notif-time">{n.time}</span>

                  {n.unread && (
                    <div className="notif-actions">
                      {n.type === 'resurface' ? (
                        <>
                          <button
                            className="btn btn-sm"
                            style={{ background: 'var(--ready-dim)', color: 'var(--ready)', border: '1px solid #34d39930', fontSize: 12 }}
                            onClick={() => markRead(n.id)}
                          >
                            Useful
                          </button>
                          <button
                            className="btn btn-secondary btn-sm"
                            style={{ fontSize: 12 }}
                            onClick={() => markRead(n.id)}
                          >
                            Not relevant
                          </button>
                        </>
                      ) : (
                        <button className="btn btn-ghost btn-sm" onClick={() => markRead(n.id)}>
                          Mark read
                        </button>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}

          {/* Bottom border closure */}
          <div style={{ border: '1px solid var(--border)', borderTop: 'none', borderRadius: '0 0 var(--radius-md) var(--radius-md)' }} />
        </div>
      )}

      {/* Info box */}
      <div style={{
        marginTop: 24,
        padding: '13px 16px',
        background: 'var(--bg-2)',
        border: '1px solid var(--border)',
        borderRadius: 'var(--radius-sm)',
        fontSize: 12.5,
        color: 'var(--text-3)',
        lineHeight: 1.65,
        letterSpacing: '-0.01em',
      }}>
        <span style={{ color: 'var(--accent)', fontWeight: 500 }}>How resurfacing works - </span>
        When you save new content, PKC scans your entire library for related resources.
        Resources saved 30+ days ago with low engagement score highest.
        Your feedback trains future resurfacing precision.
      </div>
    </div>
  );
}
