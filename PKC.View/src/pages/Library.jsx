import { useEffect, useState } from 'react';
import { apiGetResources } from '../api';

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

function TypeBadge({ type }) {
  return <span className={`badge badge-${(type || '').toLowerCase()}`}>{type}</span>;
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

const ALL_STATUSES = ['All', 'Ready', 'Pending', 'Extracting', 'Chunking', 'Embedding', 'Tagging', 'Failed'];
const ALL_TYPES    = ['All', 'Url', 'Note', 'Pdf'];

export default function Library() {
  const [resources, setResources]             = useState([]);
  const [loading, setLoading]         = useState(true);
  const [search, setSearch]           = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [typeFilter, setTypeFilter]   = useState('All');

  useEffect(() => {
    apiGetResources()
      .then(setResources)
      .finally(() => setLoading(false));
  }, []);

  const filtered = resources.filter(resource => {
    const matchSearch = !search
      || (resource.title || '').toLowerCase().includes(search.toLowerCase())
      || (resource.sourceUrl || '').toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === 'All' || resource.status === statusFilter;
    const matchType   = typeFilter   === 'All' || resource.type   === typeFilter;
    return matchSearch && matchStatus && matchType;
  });

  const sorted = [...filtered].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

  if (loading) return (
    <div className="flex-center" style={{ height: '60vh' }}>
      <div className="spinner" style={{ width: 24, height: 24 }} />
    </div>
  );

  const hasFilter = search || statusFilter !== 'All' || typeFilter !== 'All';

  return (
    <div className="fade-in">
      <div className="page-header">
        <div className="flex-between">
          <div>
            <h2>Library</h2>
            <p>{resources.length} resource{resources.length !== 1 ? 's' : ''} in your knowledge base</p>
          </div>
        </div>
      </div>

      {/* Search */}
      <div className="search-bar-wrapper" style={{ marginBottom: 14 }}>
        <span className="search-icon-inside">
          <svg width="14" height="14" viewBox="0 0 15 15" fill="none">
            <circle cx="6.5" cy="6.5" r="4" stroke="currentColor" strokeWidth="1.3"/>
            <path d="M9.5 9.5l3.5 3.5" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/>
          </svg>
        </span>
        <input
          type="text"
          className="search-input-big"
          placeholder="Filter by title or URL…"
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
      </div>

      {/* Filters */}
      <div className="filter-bar">
        <span style={{ fontSize: 11, fontWeight: 600, letterSpacing: '0.06em', textTransform: 'uppercase', color: 'var(--text-4)', marginRight: 4 }}>Status</span>
        {ALL_STATUSES.map(s => (
          <button key={s} className={`filter-chip${statusFilter === s ? ' active' : ''}`} onClick={() => setStatusFilter(s)}>{s}</button>
        ))}
      </div>
      <div className="filter-bar" style={{ marginBottom: 20 }}>
        <span style={{ fontSize: 11, fontWeight: 600, letterSpacing: '0.06em', textTransform: 'uppercase', color: 'var(--text-4)', marginRight: 4 }}>Type</span>
        {ALL_TYPES.map(t => (
          <button key={t} className={`filter-chip${typeFilter === t ? ' active' : ''}`} onClick={() => setTypeFilter(t)}>{t}</button>
        ))}
      </div>

      {/* Results meta */}
      {hasFilter && (
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, fontSize: 12, color: 'var(--text-3)', marginBottom: 12, fontFamily: 'var(--mono)' }}>
          <span>{sorted.length} result{sorted.length !== 1 ? 's' : ''}{search ? ` for "${search}"` : ''}</span>
          <button
            className="btn btn-ghost btn-sm"
            style={{ fontFamily: 'var(--font)', fontSize: 12 }}
            onClick={() => { setSearch(''); setStatusFilter('All'); setTypeFilter('All'); }}
          >
            Clear
          </button>
        </div>
      )}

      {/* Resources */}
      {sorted.length === 0 ? (
        <div className="empty-state">
          <div className="empty-title">
            {resources.length === 0 ? 'Your library is empty' : 'No resources match your filters'}
          </div>
          <div className="empty-sub">
            {resources.length === 0 ? 'Add your first URL, note, or PDF from the Add Content page.' : 'Try adjusting the search or filter criteria.'}
          </div>
        </div>
      ) : (
        <div className="items-grid">
          {sorted.map(resource => (
            <div key={resource.id} className="item-card">

              <TypeMark type={resource.type} />

              <div className="item-info">
                <div className="item-title">{resource.title || 'Untitled'}</div>
                <div className="item-meta">
                  <StatusBadge status={resource.status} />
                  <TypeBadge type={resource.type} />
                  {resource.wordCount && (
                    <span style={{ fontFamily: 'var(--mono)', fontSize: 11 }}>
                      {resource.wordCount.toLocaleString()} words
                    </span>
                  )}
                  <span style={{ fontFamily: 'var(--mono)', fontSize: 11 }}>{timeAgo(resource.createdAt)}</span>
                  {resource.sourceUrl && (
                    <a
                      href={resource.sourceUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      style={{ color: 'var(--accent)', fontSize: 11 }}
                      onClick={e => e.stopPropagation()}
                    >
                      source ↗
                    </a>
                  )}
                </div>
                {resource.failureReason && (
                  <div style={{ fontSize: 11.5, color: 'var(--failed)', marginTop: 5, fontFamily: 'var(--mono)' }}>
                    {resource.failureReason}
                  </div>
                )}
              </div>

              {/* Status indicator dot */}
              <div style={{
                width: 7, height: 7, borderRadius: '50%', flexShrink: 0,
                background: resource.status === 'Ready'
                  ? 'var(--ready)'
                  : resource.status === 'Failed'
                  ? 'var(--failed)'
                  : 'var(--processing)',
                boxShadow: resource.status === 'Ready' ? '0 0 6px var(--ready)' : 'none',
                animation: !['Ready', 'Failed'].includes(resource.status) ? 'pulse 1.6s ease-in-out infinite' : 'none',
              }} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
