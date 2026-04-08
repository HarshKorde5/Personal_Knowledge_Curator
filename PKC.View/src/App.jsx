import { BrowserRouter, Routes, Route, Navigate, NavLink, useNavigate } from 'react-router-dom';
import { ThemeProvider, useTheme } from './ThemeContext';
import { getToken, getEmail, clearToken } from './api';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Ingest from './pages/Ingest';
import Library from './pages/Library';
import Search from './pages/Search';
import Notifications from './pages/Notifications';
import './index.css';

function ProtectedRoute({ children }) {
  return getToken() ? children : <Navigate to="/login" replace />;
}

/*  SVG Icons  */
const IconDashboard = () => (
  <svg width="15" height="15" viewBox="0 0 15 15" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1" y="1" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.2"/>
    <rect x="8.5" y="1" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.2"/>
    <rect x="1" y="8.5" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.2"/>
    <rect x="8.5" y="8.5" width="5.5" height="5.5" rx="1" stroke="currentColor" strokeWidth="1.2"/>
  </svg>
);

const IconPlus = () => (
  <svg width="15" height="15" viewBox="0 0 15 15" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M7.5 2v11M2 7.5h11" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round"/>
  </svg>
);

const IconLibrary = () => (
  <svg width="15" height="15" viewBox="0 0 15 15" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="1.5" y="2" width="4" height="11" rx="1" stroke="currentColor" strokeWidth="1.2"/>
    <rect x="7" y="2" width="4" height="11" rx="1" stroke="currentColor" strokeWidth="1.2"/>
    <path d="M13 4l-1.5 9" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
  </svg>
);

const IconSearch = () => (
  <svg width="15" height="15" viewBox="0 0 15 15" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle cx="6.5" cy="6.5" r="4" stroke="currentColor" strokeWidth="1.2"/>
    <path d="M9.5 9.5l3.5 3.5" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
  </svg>
);

const IconBell = () => (
  <svg width="15" height="15" viewBox="0 0 15 15" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M7.5 2a4 4 0 0 1 4 4v2.5l1 2H2.5l1-2V6a4 4 0 0 1 4-4z" stroke="currentColor" strokeWidth="1.2" strokeLinejoin="round"/>
    <path d="M6 12.5a1.5 1.5 0 0 0 3 0" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
  </svg>
);

const IconSun = () => (
  <svg width="13" height="13" viewBox="0 0 13 13" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle cx="6.5" cy="6.5" r="2.5" stroke="currentColor" strokeWidth="1.2"/>
    <path d="M6.5 1v1.5M6.5 10.5V12M1 6.5h1.5M10.5 6.5H12M2.6 2.6l1.1 1.1M9.3 9.3l1.1 1.1M9.3 2.6l-1.1 1.1M3.7 9.3l-1.1 1.1" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
  </svg>
);

const IconMoon = () => (
  <svg width="13" height="13" viewBox="0 0 13 13" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M11 7.5A5 5 0 0 1 5.5 2a5 5 0 1 0 5.5 5.5z" stroke="currentColor" strokeWidth="1.2" strokeLinejoin="round"/>
  </svg>
);

/*  Sidebar  */
function Sidebar() {
  const navigate = useNavigate();
  const { theme, toggle } = useTheme();
  const email = getEmail() || 'user@pkc.dev';
  const initials = email.slice(0, 2).toUpperCase();

  function logout() {
    clearToken();
    navigate('/login');
  }

  const navItems = [
    { to: '/dashboard',      Icon: IconDashboard, label: 'Dashboard' },
    { to: '/ingest',         Icon: IconPlus,      label: 'Add Content' },
    { to: '/library',        Icon: IconLibrary,   label: 'Library' },
    { to: '/search',         Icon: IconSearch,    label: 'Search' },
    { to: '/notifications',  Icon: IconBell,      label: 'Notifications' },
  ];

  return (
    <aside className="sidebar">
      {/* Logo */}
      <div className="sidebar-logo">
        <h1>PKC</h1>
        <span>knowledge curator</span>
      </div>

      {/* Nav */}
      <nav className="sidebar-nav">
        {navItems.map(({ to, Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <span className="icon"><Icon /></span>
            {label}
          </NavLink>
        ))}
      </nav>

      {/* Footer */}
      <div className="sidebar-footer">
        <div className="sidebar-footer-top">
          <span className="sidebar-footer-label">Appearance</span>
          <button
            className="theme-toggle"
            onClick={toggle}
            title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
          >
            {theme === 'dark' ? <IconSun /> : <IconMoon />}
          </button>
        </div>
        <div className="sidebar-user" onClick={logout} title="Click to sign out">
          <div className="user-avatar">{initials}</div>
          <div style={{ minWidth: 0 }}>
            <div className="user-email">{email}</div>
            <div className="logout-btn">Sign out</div>
          </div>
        </div>
      </div>
    </aside>
  );
}

function AppLayout({ children }) {
  return (
    <div className="app-layout">
      <Sidebar />
      <main className="main-content">{children}</main>
    </div>
  );
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/"              element={<Home />} />
      <Route path="/login"         element={<Login />} />
      <Route path="/register"      element={<Register />} />
      <Route path="/dashboard"     element={<ProtectedRoute><AppLayout><Dashboard /></AppLayout></ProtectedRoute>} />
      <Route path="/ingest"        element={<ProtectedRoute><AppLayout><Ingest /></AppLayout></ProtectedRoute>} />
      <Route path="/library"       element={<ProtectedRoute><AppLayout><Library /></AppLayout></ProtectedRoute>} />
      <Route path="/search"        element={<ProtectedRoute><AppLayout><Search /></AppLayout></ProtectedRoute>} />
      <Route path="/notifications" element={<ProtectedRoute><AppLayout><Notifications /></AppLayout></ProtectedRoute>} />
      <Route path="*"              element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <ThemeProvider>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </ThemeProvider>
  );
}
