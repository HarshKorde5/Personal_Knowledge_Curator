import { useState, useRef } from 'react';
import { apiSearch, apiAsk } from '../api';

const EXAMPLE_QUERIES = [
  'How does attention work in transformers?',
  'What is pgvector used for?',
  'How do I implement semantic search?',
  'What are embeddings?',
];

function ScoreBar({ score }) {
  const pct = Math.max(0, Math.min(100, (1 - score) * 100));
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
      <div style={{
        background: 'var(--bg-3)',
        borderRadius: 2,
        height: 3,
        width: 56,
        overflow: 'hidden',
      }}>
        <div style={{
          height: '100%',
          width: `${pct}%`,
          background: 'var(--accent)',
          borderRadius: 2,
        }} />
      </div>
      <span style={{ fontFamily: 'var(--mono)', fontSize: 11, color: 'var(--text-3)' }}>
        {pct.toFixed(0)}% match
      </span>
    </div>
  );
}

export default function Search() {
  const [query, setQuery]     = useState('');
  const [loading, setLoading] = useState(false);
  const [answer, setAnswer]   = useState(null);
  const [chunks, setChunks]   = useState([]);
  const [related, setRelated] = useState([]);
  const [error, setError]     = useState('');
  const [mode, setMode]       = useState('ai');
  const inputRef = useRef();

  async function handleSearch(q) {
    const queryText = q || query;
    if (!queryText.trim()) return;
    setError('');
    setAnswer(null);
    setChunks([]);
    setRelated([]);
    setLoading(true);
    try {
      if (mode === 'ai') {
        const res = await apiAsk(queryText.trim());
        setAnswer(res.answer);
        setRelated(res.related || []);
      } else {
        const res = await apiSearch(queryText.trim());
        setChunks(res);
      }
    } catch (err) {
      setError(err.message || 'Search failed. Please try again.');
    } finally {
      setLoading(false);
    }
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSearch();
    }
  }

  function useExample(q) {
    setQuery(q);
    handleSearch(q);
  }

  const hasResults = answer || chunks.length > 0;

  return (
    <div className="fade-in">
      <div className="page-header">
        <h2>Search</h2>
        <p>Ask questions in natural language - answers grounded in your saved content.</p>
      </div>

      {/* Mode toggle */}
      <div className="tabs" style={{ marginBottom: 20 }}>
        <button
          className={`tab-btn${mode === 'ai' ? ' active' : ''}`}
          onClick={() => setMode('ai')}
        >
          AI Answer
        </button>
        <button
          className={`tab-btn${mode === 'chunks' ? ' active' : ''}`}
          onClick={() => setMode('chunks')}
        >
          Chunk Search
        </button>
      </div>

      {/* Search bar */}
      <div style={{ position: 'relative', marginBottom: 10 }}>
        <span className="search-icon-inside" style={{ top: '50%' }}>
          <svg width="14" height="14" viewBox="0 0 15 15" fill="none">
            <circle cx="6.5" cy="6.5" r="4" stroke="currentColor" strokeWidth="1.3"/>
            <path d="M9.5 9.5l3.5 3.5" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/>
          </svg>
        </span>
        <input
          ref={inputRef}
          type="text"
          className="search-input-big"
          placeholder={
            mode === 'ai'
              ? 'Ask anything about your saved content…'
              : 'Search for relevant content…'
          }
          value={query}
          onChange={e => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          autoFocus
        />
        <button
          className="btn btn-primary btn-sm"
          style={{ position: 'absolute', right: 8, top: '50%', transform: 'translateY(-50%)' }}
          onClick={() => handleSearch()}
          disabled={loading || !query.trim()}
        >
          {loading
            ? <div className="spinner" style={{ width: 12, height: 12 }} />
            : mode === 'ai' ? 'Ask' : 'Search'
          }
        </button>
      </div>

      <div style={{ fontSize: 12, color: 'var(--text-3)', marginBottom: 24, letterSpacing: '-0.01em' }}>
        {mode === 'ai'
          ? 'Synthesises an answer from your top matching chunks via the RAG pipeline.'
          : 'Returns the most semantically similar text chunks from your knowledge base.'
        }
        {' '}Press Enter to search.
      </div>

      {/* Example queries */}
      {!hasResults && !loading && (
        <div style={{ marginBottom: 28 }}>
          <div style={{
            fontSize: 11,
            fontWeight: 600,
            letterSpacing: '0.06em',
            textTransform: 'uppercase',
            color: 'var(--text-4)',
            marginBottom: 10,
          }}>
            Example queries
          </div>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
            {EXAMPLE_QUERIES.map(q => (
              <button key={q} className="filter-chip" onClick={() => useExample(q)}>
                {q}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Error */}
      {error && <div className="auth-error">{error}</div>}

      {/* Loading */}
      {loading && (
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          padding: '56px 0',
          gap: 14,
        }}>
          <div className="loading-dots"><span /><span /><span /></div>
          <div style={{ fontSize: 12.5, color: 'var(--text-3)', letterSpacing: '-0.01em' }}>
            {mode === 'ai'
              ? 'Embedding query → vector search → generating answer…'
              : 'Embedding query → cosine similarity search…'
            }
          </div>
        </div>
      )}

      {/* AI Answer */}
      {!loading && answer && (
        <div className="fade-in">
          <div className="answer-box">
            <div className="answer-label">AI Answer</div>
            <div className="answer-text">{answer}</div>
            <div style={{ marginTop: 12, fontSize: 11.5, color: 'var(--text-3)' }}>
              Generated by local Ollama (gemma3:1b) · grounded in your saved content only
            </div>
          </div>

          {related.length > 0 && (
            <>
              <div style={{
                fontSize: 11,
                fontWeight: 600,
                letterSpacing: '0.06em',
                textTransform: 'uppercase',
                color: 'var(--text-4)',
                marginBottom: 10,
              }}>
                Source chunks
              </div>
              {related.map((r, i) => (
                <div key={i} className="result-card fade-in">
                  <ScoreBar score={r.score} />
                  <div className="result-content" style={{ marginTop: 10 }}>{r.content}</div>
                </div>
              ))}
            </>
          )}
        </div>
      )}

      {/* Chunk results */}
      {!loading && chunks.length > 0 && (
        <div className="fade-in">
          <div style={{
            fontSize: 11,
            fontWeight: 600,
            letterSpacing: '0.06em',
            textTransform: 'uppercase',
            color: 'var(--text-4)',
            marginBottom: 10,
          }}>
            {chunks.length} matching chunk{chunks.length !== 1 ? 's' : ''}
          </div>
          {chunks.map((chunk, i) => (
            <div key={chunk.id || i} className="result-card fade-in">
              <ScoreBar score={chunk.score} />
              <div className="result-content" style={{ marginTop: 10 }}>{chunk.content}</div>
            </div>
          ))}
        </div>
      )}

      {/* Empty result */}
      {!loading && !error && query && !hasResults && chunks.length === 0 && (
        <div className="empty-state">
          <div className="empty-title">No results found</div>
          <div className="empty-sub">
            Try a different query, or add more content to your knowledge base first.
          </div>
        </div>
      )}
    </div>
  );
}
