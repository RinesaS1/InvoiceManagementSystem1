import { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context/AuthContext';
import LanguageSwitcher from './LanguageSwitcher';
import './Navbar.css';

function Navbar() {
  const { user, role, logout, isAuthenticated } = useAuth();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const toggleMenu = () => setMenuOpen((prev) => !prev);
  const closeMenu = () => setMenuOpen(false);

  return (
    <nav className="navbar">
      <div className="navbar-container">
        {/* Brand */}
        <Link to="/" className="navbar-brand" onClick={closeMenu}>
          <span className="navbar-logo">📄</span>
          <span>InvoiceApp</span>
        </Link>

        {/* Hamburger (mobile) */}
        <button
          className={`navbar-hamburger ${menuOpen ? 'open' : ''}`}
          onClick={toggleMenu}
          aria-label="Toggle navigation"
          aria-expanded={menuOpen}
          type="button"
        >
          <span />
          <span />
          <span />
        </button>

        {/* Nav links */}
        <div className={`navbar-links ${menuOpen ? 'navbar-links-open' : ''}`}>
          {isAuthenticated && role === 'Admin' && (
            <>
              <NavLink to="/admin/dashboard" className="nav-link" onClick={closeMenu}>
                {t('navbar.dashboard')}
              </NavLink>
              <NavLink to="/admin/invoices" className="nav-link" onClick={closeMenu}>
                {t('navbar.invoices')}
              </NavLink>
              <NavLink to="/admin/customers" className="nav-link" onClick={closeMenu}>
                {t('navbar.customers')}
              </NavLink>
              <NavLink to="/admin/payments" className="nav-link" onClick={closeMenu}>
                {t('navbar.payments')}
              </NavLink>
            </>
          )}

          {isAuthenticated && role === 'Customer' && (
            <>
              <NavLink to="/customer/dashboard" className="nav-link" onClick={closeMenu}>
                {t('navbar.dashboard')}
              </NavLink>
              <NavLink to="/customer/invoices" className="nav-link" onClick={closeMenu}>
                {t('navbar.myInvoices')}
              </NavLink>
            </>
          )}

          {!isAuthenticated && (
            <>
              <NavLink to="/login" className="nav-link" onClick={closeMenu}>
                {t('navbar.login')}
              </NavLink>
              <NavLink to="/register" className="nav-link" onClick={closeMenu}>
                {t('navbar.register')}
              </NavLink>
            </>
          )}

          {/* Language Switcher */}
          <div className="navbar-lang-wrapper">
            <LanguageSwitcher />
          </div>

          {isAuthenticated && (
            <div className="navbar-user">
              <span className="navbar-user-info">
                <span className="navbar-user-name">{user?.fullName || user?.email}</span>
                <span className="navbar-user-role">{role}</span>
              </span>
              <button
                className="btn-logout"
                onClick={() => {
                  handleLogout();
                  closeMenu();
                }}
                type="button"
              >
                {t('navbar.logout')}
              </button>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
}

export default Navbar;
