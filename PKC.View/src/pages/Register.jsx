import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { apiRegister } from '../api';
import { useTheme } from '../ThemeContext';

export default function Register() {
  const navigate = useNavigate();
  const { theme, toggle } = useTheme();
  const [email, setEmail]       = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm]   = useState('');
  const [error, setError]       = useState('');
  const [loading, setLoading]   = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError('');
    if (!email || !password) { setError('Email and password are required.'); return; }
    if (password.length < 6)  { setError('Password must be at least 6 characters.'); return; }
    if (password !== confirm)  { setError('Passwords do not match.'); return; }
    setLoading(true);
    try {
      await apiRegister(email, password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-page">
      <button
        className="theme-toggle"
        onClick={toggle}
        style={{ position: 'fixed', top: 20, right: 20 }}
        title="Toggle theme"
      >
        {theme === 'dark' ? '☀' : '☾'}
      </button>

      <div className="auth-card fade-in">
        <div className="auth-logo">
          <h1>PKC</h1>
          <p>Personal Knowledge Curator</p>
        </div>

        <h2 className="auth-title">Create account</h2>
        <p className="auth-sub">Start building your knowledge base</p>

        {error && <div className="auth-error">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Email address</label>
            <input
              type="email"
              className="form-input"
              placeholder="you@example.com"
              value={email}
              onChange={e => setEmail(e.target.value)}
              autoFocus
            />
          </div>
          <div className="form-group">
            <label className="form-label">Password</label>
            <input
              type="password"
              className="form-input"
              placeholder="min. 6 characters"
              value={password}
              onChange={e => setPassword(e.target.value)}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Confirm password</label>
            <input
              type="password"
              className="form-input"
              placeholder="repeat password"
              value={confirm}
              onChange={e => setConfirm(e.target.value)}
            />
          </div>
          <button
            type="submit"
            className="btn btn-primary btn-lg"
            style={{ width: '100%', marginTop: 8, justifyContent: 'center' }}
            disabled={loading}
          >
            {loading
              ? <><div className="spinner" style={{ width: 15, height: 15 }} /> Creating account…</>
              : 'Create account'
            }
          </button>
        </form>

        <div className="auth-footer">
          Already have an account? <Link to="/login">Sign in</Link>
        </div>
      </div>
    </div>
  );
}
