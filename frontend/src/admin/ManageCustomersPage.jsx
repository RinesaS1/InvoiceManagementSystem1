import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import customerService from '../services/customerService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import './ManageCustomers.css';

function ManageCustomersPage() {
  const { t } = useTranslation();
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');

  const fetchCustomers = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await customerService.getAll();
      if (response && response.success) {
        setCustomers(response.data || []);
      } else {
        setError(response?.message || 'Failed to load customers.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Could not connect to the backend to fetch customers.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, []);

  const filteredCustomers = customers.filter((c) => {
    const term = search.toLowerCase();
    return (
      (c.fullName && c.fullName.toLowerCase().includes(term)) ||
      (c.email && c.email.toLowerCase().includes(term))
    );
  });

  return (
    <div className="manage-customers-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{t('customers.title')}</h1>
          <p className="page-subtitle">{t('customers.subtitle')}</p>
        </div>
        <Link to="/admin/invoices/create">
          <Button variant="primary">{t('customers.newInvoice')}</Button>
        </Link>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />

      {/* Search Bar */}
      <div className="card filter-bar">
        <div className="search-form">
          <input
            type="text"
            className="search-input"
            placeholder={t('customers.searchPlaceholder')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <Button variant="secondary" size="md" onClick={() => setSearch('')}>
              {t('customers.clearBtn')}
            </Button>
          )}
        </div>
        <div className="customer-count-badge">
          {t('customers.totalRegistered')} <strong>{customers.length}</strong>
        </div>
      </div>

      {loading ? (
        <div className="page-center">
          <Spinner size="lg" />
        </div>
      ) : filteredCustomers.length === 0 ? (
        <div className="card empty-state-card">
          <p className="empty-title">{t('customers.noCustomers')}</p>
          <p className="empty-description">
            {search ? t('customers.noMatch') : t('customers.noneYet')}
          </p>
        </div>
      ) : (
        <div className="card table-card">
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('customers.table.name')}</th>
                  <th>{t('customers.table.email')}</th>
                  <th>{t('customers.table.since')}</th>
                  <th>{t('customers.table.totalInvoices')}</th>
                  <th style={{ textAlign: 'right' }}>{t('customers.table.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {filteredCustomers.map((c) => (
                  <tr key={c.id}>
                    <td className="font-semibold">{c.fullName || '—'}</td>
                    <td>{c.email}</td>
                    <td>{new Date(c.createdAt).toLocaleDateString()}</td>
                    <td>
                      <span className="invoice-count-pill">{c.invoiceCount}</span>
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <div className="action-buttons">
                        <Link to={`/admin/invoices?customerId=${c.id}`}>
                          <Button variant="outline" size="sm">
                            {t('customers.viewInvoices', { count: c.invoiceCount })}
                          </Button>
                        </Link>
                        <Link to={`/admin/invoices/create?customerId=${c.id}`}>
                          <Button variant="primary" size="sm">
                            {t('customers.plusInvoice')}
                          </Button>
                        </Link>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

export default ManageCustomersPage;
