import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Button from '../components/Button';

function UnauthorizedPage() {
  const { t } = useTranslation();

  return (
    <div className="page-center">
      <div className="card" style={{ maxWidth: '480px', textAlign: 'center' }}>
        <h2 style={{ fontSize: '1.75rem', color: '#dc2626', marginBottom: '0.5rem' }}>
          {t('common.accessDenied')}
        </h2>
        <p style={{ color: 'var(--color-gray-600)', marginBottom: '1.5rem' }}>
          {t('common.accessDeniedMsg')}
        </p>
        <Link to="/">
          <Button variant="primary">{t('common.returnHome')}</Button>
        </Link>
      </div>
    </div>
  );
}

export default UnauthorizedPage;
