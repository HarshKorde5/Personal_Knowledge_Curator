import { useState } from 'react';
import { apiCreateUrl, apiCreateNote, apiUploadPdf } from '../api';

function SuccessToast({ itemId, onDismiss }) {
  return (
    <div style={{
      background: 'var(--ready-dim)',
      border: '1px solid #34d39928',
      borderRadius: 'var(--radius-sm)',
      padding: '12px 16px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      marginBottom: 16,
      animation: 'fadeIn 0.2s ease',
    }}>
      <div>
        <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--ready)', marginBottom: 2 }}>
          Content queued successfully
        </div>
        <div style={{ fontSize: 11.5, color: 'var(--text-3)', fontFamily: 'var(--mono)' }}>
          ID: {itemId} - processing in background
        </div>
      </div>
      <button className="btn btn-ghost btn-sm" onClick={onDismiss}>Dismiss</button>
    </div>
  );
}

const PIPELINE_STEPS = [
  { n: '01', label: 'Ingest', desc: 'Resource created, 202 returned instantly' },
  { n: '02', label: 'Extract', desc: 'HTML stripped or PDF parsed to text' },
  { n: '03', label: 'Chunk', desc: 'Text split into ~500-word segments' },
  { n: '04', label: 'Embed', desc: 'ONNX generates 768-dim vectors per chunk' },
  { n: '05', label: 'Tag & Connect', desc: 'AI tags assigned, cross-item connections built' },
  { n: '06', label: 'Ready', desc: 'Semantic search and AI queries enabled' },
];

export default function Ingest() {
  const [tab, setTab] = useState('url');
  const [url, setUrl] = useState('');
  const [urlTitle, setUrlTitle] = useState('');
  const [noteContent, setNoteContent] = useState('');
  const [noteTitle, setNoteTitle] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(null);
  const [pdfFile, setPdfFile] = useState(null);
  const [pdfTitle, setPdfTitle] = useState('');
  const [uploadProgress, setUploadProgress] = useState(0);

  function reset() {
    setUrl(''); setUrlTitle('');
    setNoteContent(''); setNoteTitle('');
    setError('');
  }

  async function handleUrlSubmit(e) {
    e.preventDefault();
    setError(''); setSuccess(null);
    if (!url.trim()) { setError('Please enter a URL.'); return; }
    try { new URL(url); } catch { setError('Enter a valid URL including https://.'); return; }
    setLoading(true);
    try {
      const res = await apiCreateUrl(url.trim(), urlTitle.trim() || undefined);
      setSuccess(res.resourceId);
      reset();
    } catch (err) {
      setError(err.message || 'Failed to save URL.');
    } finally {
      setLoading(false);
    }
  }

  async function handleNoteSubmit(e) {
    e.preventDefault();
    setError(''); setSuccess(null);
    if (!noteContent.trim()) { setError('Note content cannot be empty.'); return; }
    setLoading(true);
    try {
      const res = await apiCreateNote(noteContent.trim(), noteTitle.trim() || undefined);
      setSuccess(res.resourceId);
      reset();
    } catch (err) {
      setError(err.message || 'Failed to save note.');
    } finally {
      setLoading(false);
    }
  }

  const wordCount = noteContent.split(/\s+/).filter(Boolean).length;

  async function handlePdfSubmit(e) {
    e.preventDefault();
    setError('');
    setSuccess(null);

    if (!pdfFile) {
      setError('Please select a PDF file.');
      return;
    }

    setLoading(true);
    setUploadProgress(0);

    try {
      const fakeProgress = setInterval(() => {
        setUploadProgress((p) => (p < 90 ? p + 10 : p));
      }, 200);

      const res = await apiUploadPdf(pdfFile, pdfTitle.trim() || undefined);

      clearInterval(fakeProgress);
      setUploadProgress(100);

      setSuccess(res.resourceId);
      setPdfFile(null);
      setPdfTitle('');
    } catch (err) {
      setError(err.message || 'Failed to upload PDF.');
    } finally {
      setLoading(false);
      setTimeout(() => setUploadProgress(0), 1000);
    }
  }

  return (
    <div className="fade-in" style={{ maxWidth: 640 }}>
      <div className="page-header">
        <h2>Add Content</h2>
        <p>Save a URL, write a note, or upload a PDF to your knowledge base.</p>
      </div>

      {success && <SuccessToast itemId={success} onDismiss={() => setSuccess(null)} />}
      {error && <div className="auth-error" style={{ marginBottom: 16 }}>{error}</div>}

      {/* Tabs */}
      <div className="tabs">
        {[
          { key: 'url', label: 'URL' },
          { key: 'note', label: 'Note' },
          { key: 'pdf', label: 'PDF' },
        ].map(t => (
          <button
            key={t.key}
            className={`tab-btn${tab === t.key ? ' active' : ''}`}
            onClick={() => { setTab(t.key); setError(''); }}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* URL Tab */}
      {tab === 'url' && (
        <div className="card fade-in">
          <form onSubmit={handleUrlSubmit}>
            <div className="form-group">
              <label className="form-label">URL <span style={{ color: 'var(--failed)' }}>*</span></label>
              <input
                type="text"
                className="form-input"
                placeholder="https://example.com/article"
                value={url}
                onChange={e => setUrl(e.target.value)}
                autoFocus
              />
            </div>
            <div className="form-group">
              <label className="form-label">
                Title{' '}
                <span style={{ color: 'var(--text-4)', fontWeight: 400 }}>optional</span>
              </label>
              <input
                type="text"
                className="form-input"
                placeholder="Auto-detected from page if left blank"
                value={urlTitle}
                onChange={e => setUrlTitle(e.target.value)}
              />
            </div>

            <div style={{
              background: 'var(--bg-3)',
              border: '1px solid var(--border)',
              borderRadius: 'var(--radius-sm)',
              padding: '10px 13px',
              marginBottom: 16,
              fontSize: 12.5,
              color: 'var(--text-3)',
              lineHeight: 1.6,
            }}>
              The page will be fetched, cleaned, chunked, and embedded in the background.
              Processing typically takes under 30 seconds.
            </div>

            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading
                ? <><div className="spinner" style={{ width: 13, height: 13 }} /> Saving…</>
                : 'Save URL'
              }
            </button>
          </form>
        </div>
      )}

      {/* Note Tab */}
      {tab === 'note' && (
        <div className="card fade-in">
          <form onSubmit={handleNoteSubmit}>
            <div className="form-group">
              <label className="form-label">
                Title{' '}
                <span style={{ color: 'var(--text-4)', fontWeight: 400 }}>optional</span>
              </label>
              <input
                type="text"
                className="form-input"
                placeholder="Give your note a name"
                value={noteTitle}
                onChange={e => setNoteTitle(e.target.value)}
              />
            </div>
            <div className="form-group">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                <label className="form-label" style={{ margin: 0 }}>
                  Content <span style={{ color: 'var(--failed)' }}>*</span>
                </label>
                <span style={{ fontSize: 11, fontFamily: 'var(--mono)', color: 'var(--text-3)' }}>
                  {wordCount} words
                </span>
              </div>
              <textarea
                className="form-input"
                placeholder="Write or paste your notes here…"
                value={noteContent}
                onChange={e => setNoteContent(e.target.value)}
                rows={10}
                autoFocus
              />
            </div>

            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading
                ? <><div className="spinner" style={{ width: 13, height: 13 }} /> Saving…</>
                : 'Save note'
              }
            </button>
          </form>
        </div>
      )}

      {/* PDF Tab */}
      {tab === 'pdf' && (
        <div className="card fade-in">
          <form onSubmit={handlePdfSubmit}>
            <div className="form-group">
              <label className="form-label">PDF File *</label>
              <input
                type="file"
                accept="application/pdf"
                className="form-input"
                onChange={(e) => setPdfFile(e.target.files[0])}
              />
            </div>

            <div className="form-group">
              <label className="form-label">
                Title <span style={{ color: 'var(--text-4)' }}>optional</span>
              </label>
              <input
                type="text"
                className="form-input"
                placeholder="Optional title"
                value={pdfTitle}
                onChange={(e) => setPdfTitle(e.target.value)}
              />
            </div>

            {/* ✅ Progress Bar */}
            {uploadProgress > 0 && (
              <div style={{ marginBottom: 16 }}>
                <div style={{
                  height: 6,
                  background: 'var(--bg-3)',
                  borderRadius: 4,
                  overflow: 'hidden'
                }}>
                  <div style={{
                    width: `${uploadProgress}%`,
                    height: '100%',
                    background: 'var(--accent)',
                    transition: 'width 0.2s ease'
                  }} />
                </div>
                <div style={{
                  fontSize: 11,
                  color: 'var(--text-3)',
                  marginTop: 4
                }}>
                  Uploading... {uploadProgress}%
                </div>
              </div>
            )}

            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading
                ? <><div className="spinner" style={{ width: 13, height: 13 }} /> Uploading…</>
                : 'Upload PDF'
              }
            </button>
          </form>

        </div>
      )}

      {/* Pipeline steps */}
      <div style={{ marginTop: 20 }}>
        <div style={{ fontSize: 11, fontWeight: 600, letterSpacing: '0.06em', textTransform: 'uppercase', color: 'var(--text-4)', marginBottom: 10 }}>
          Processing pipeline
        </div>
        <div style={{ border: '1px solid var(--border)', borderRadius: 'var(--radius-md)', overflow: 'hidden', background: 'var(--bg-2)' }}>
          {PIPELINE_STEPS.map((s, i, arr) => (
            <div
              key={s.n}
              style={{
                display: 'flex',
                gap: 14,
                padding: '11px 16px',
                borderBottom: i < arr.length - 1 ? '1px solid var(--border)' : 'none',
              }}
            >
              <span style={{ fontFamily: 'var(--mono)', fontSize: 10, color: 'var(--accent)', minWidth: 22, paddingTop: 2, flexShrink: 0 }}>
                {s.n}
              </span>
              <div>
                <div style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-1)', letterSpacing: '-0.01em' }}>{s.label}</div>
                <div style={{ fontSize: 12, color: 'var(--text-3)', marginTop: 2 }}>{s.desc}</div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
