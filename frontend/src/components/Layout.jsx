import { Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Navbar from './Navbar';
import './Layout.css';

function Layout() {
  const { t } = useTranslation();

  return (
    <div className="app-layout">
      <Navbar />
      <main className="app-main">
        <div className="app-container">
          <Outlet />
        </div>
      </main>
      <footer className="app-footer">
        <div className="app-container">
          <p>{t('common.copyright', { year: new Date().getFullYear() })}</p>
        </div>
      </footer>
    </div>
  );
}

export default Layout;
