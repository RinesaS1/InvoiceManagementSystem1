import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context/AuthContext';
import invoiceService from '../services/invoiceService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import { InvoiceStatusBadge, PaymentStatusBadge } from '../components/Badge';
import './CustomerDashboard.css';

function CustomerDashboard() {
  const { user } = useAuth();
  const { t } = useTranslation();
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadDashboard() {
      setLoading(true);
      setError('');
      try {
        const response = await invoiceService.getAll();
        if (response && response.success) {
          setInvoices(response.data || []);
        } else {
          setError(response?.message || 'Failed to load customer metrics.');
        }
      } catch (err) {
        setError(err.response?.data?.message || 'Could not connect to the server to load invoices.');
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, []);

  // Compute live statistics directly from backend data (no hardcoded figures)
  const totalInvoices = invoices.length;
  const paidInvoices = invoices.filter((inv) => inv.status === 1);
  const unpaidInvoices = invoices.filter((inv) => inv.status === 0);

  const pendingSubmissionsCount = invoices.filter((inv) =>
    inv.payments && inv.payments.some((p) => p.status === 0)
  ).length;

  const totalAmount = invoices.reduce((sum, inv) => sum + Number(inv.amount || 0), 0);
  const paidAmount = paidInvoices.reduce((sum, inv) => sum + Number(inv.amount || 0), 0);
  const unpaidAmount = unpaidInvoices.reduce((sum, inv) => sum + Number(inv.amount || 0), 0);

  if (loading) {
    return (
      <div className="page-center">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="customer-dashboard">
      <div className="page-header">
        <div>
          <h1 className="page-title">
            {t('dashboard.customer.welcome', { name: user?.fullName || 'Customer' })}
          </h1>
          <p className="page-subtitle">{t('dashboard.customer.subtitle')}</p>
        </div>
        <Link to="/customer/invoices">
          <Button variant="outline">{t('dashboard.customer.viewAll')}</Button>
        </Link>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />

      {/* Metrics Grid */}
      <div className="stats-grid">
        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon icon-total">📄</span>
            <span className="stat-label">{t('dashboard.customer.totalInvoices')}</span>
          </div>
          <div className="stat-value">{totalInvoices}</div>
          <div className="stat-subtext">
            {t('dashboard.customer.total')}: ${totalAmount.toFixed(2)}
          </div>
        </div>

        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon icon-paid">✓</span>
            <span className="stat-label">{t('dashboard.customer.paidInvoices')}</span>
          </div>
          <div className="stat-value text-success">{paidInvoices.length}</div>
          <div className="stat-subtext">
            {t('dashboard.customer.paid')}: ${paidAmount.toFixed(2)}
          </div>
        </div>

        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon icon-unpaid">⏳</span>
            <span className="stat-label">{t('dashboard.customer.unpaidInvoices')}</span>
          </div>
          <div className="stat-value text-warning">{unpaidInvoices.length}</div>
          <div className="stat-subtext">
            {t('dashboard.customer.outstanding')}: ${unpaidAmount.toFixed(2)}
          </div>
        </div>

        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon icon-pending">🔍</span>
            <span className="stat-label">{t('dashboard.customer.pendingReviews')}</span>
          </div>
          <div className="stat-value text-info">{pendingSubmissionsCount}</div>
          <div className="stat-subtext">{t('dashboard.customer.awaitingAdmin')}</div>
        </div>
      </div>

      {/* Recent Invoices Section */}
      <div className="card recent-invoices-section">
        <div className="recent-header">
          <h2 className="section-title">{t('dashboard.customer.recentInvoices')}</h2>
          <Link to="/customer/invoices" className="link-subtle">
            {t('dashboard.customer.seeAll', { count: totalInvoices })}
          </Link>
        </div>

        {invoices.length === 0 ? (
          <div className="empty-state">
            <p>{t('dashboard.customer.noInvoices')}</p>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('invoices.table.invoiceNumber')}</th>
                  <th>{t('invoices.table.description')}</th>
                  <th>{t('invoices.table.amount')}</th>
                  <th>{t('invoices.table.dueDate')}</th>
                  <th>{t('invoices.table.status')}</th>
                  <th>{t('invoices.table.paymentStatus')}</th>
                  <th style={{ textAlign: 'right' }}>{t('invoices.table.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {invoices.slice(0, 5).map((inv) => {
                  const latestPayment =
                    inv.payments && inv.payments.length > 0
                      ? inv.payments[inv.payments.length - 1]
                      : null;

                  return (
                    <tr key={inv.id}>
                      <td className="font-semibold">
                        <Link to={`/customer/invoices/${inv.id}`} className="invoice-link">
                          {inv.invoiceNumber}
                        </Link>
                      </td>
                      <td>{inv.description || '—'}</td>
                      <td>${Number(inv.amount).toFixed(2)}</td>
                      <td>{new Date(inv.dueDate).toLocaleDateString()}</td>
                      <td>
                        <InvoiceStatusBadge status={inv.status} />
                      </td>
                      <td>
                        {latestPayment ? (
                          <PaymentStatusBadge status={latestPayment.status} />
                        ) : (
                          <span className="text-muted">—</span>
                        )}
                      </td>
                      <td style={{ textAlign: 'right' }}>
                        <Link to={`/customer/invoices/${inv.id}`}>
                          <Button variant="secondary" size="sm">
                            {t('dashboard.customer.viewDetails')}
                          </Button>
                        </Link>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default CustomerDashboard;
