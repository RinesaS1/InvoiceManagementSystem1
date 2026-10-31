import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import invoiceService from '../services/invoiceService';
import paymentService from '../services/paymentService';
import customerService from '../services/customerService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import { InvoiceStatusBadge } from '../components/Badge';
import './AdminDashboard.css';

function AdminDashboard() {
  const { t } = useTranslation();
  const [invoices, setInvoices] = useState([]);
  const [pendingPayments, setPendingPayments] = useState([]);
  const [customerCount, setCustomerCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadDashboardData() {
      setLoading(true);
      setError('');
      try {
        const [invoicesRes, paymentsRes, customersRes] = await Promise.all([
          invoiceService.getAll(),
          paymentService.getPending(),
          customerService.getAll().catch(() => ({ data: [] })),
        ]);

        if (invoicesRes && invoicesRes.success) {
          setInvoices(invoicesRes.data || []);
        }
        if (paymentsRes && paymentsRes.success) {
          setPendingPayments(paymentsRes.data || []);
        }
        if (customersRes && customersRes.success) {
          setCustomerCount((customersRes.data || []).length);
        }
      } catch (err) {
        setError('Could not connect to the backend API to fetch administrative statistics.');
      } finally {
        setLoading(false);
      }
    }

    loadDashboardData();
  }, []);

  const totalInvoices = invoices.length;
  const paidInvoices = invoices.filter((i) => i.status === 1);
  const unpaidInvoices = invoices.filter((i) => i.status === 0);

  const totalAmount = invoices.reduce((sum, i) => sum + Number(i.amount || 0), 0);
  const paidAmount = paidInvoices.reduce((sum, i) => sum + Number(i.amount || 0), 0);
  const unpaidAmount = unpaidInvoices.reduce((sum, i) => sum + Number(i.amount || 0), 0);

  if (loading) {
    return (
      <div className="page-center">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="admin-dashboard">
      <div className="page-header">
        <div>
          <h1 className="page-title">{t('dashboard.admin.title')}</h1>
          <p className="page-subtitle">{t('dashboard.admin.subtitle')}</p>
        </div>
        <div className="dashboard-top-actions">
          <Link to="/admin/invoices/create">
            <Button variant="primary">{t('dashboard.admin.createInvoice')}</Button>
          </Link>
          <Link to="/admin/payments">
            <Button variant="outline">
              {t('dashboard.admin.reviewPayments')} ({pendingPayments.length})
            </Button>
          </Link>
        </div>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />

      {/* Primary KPI Grid */}
      <div className="stats-grid">
        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon">📄</span>
            <span className="stat-label">{t('dashboard.admin.totalInvoices')}</span>
          </div>
          <div className="stat-value">{totalInvoices}</div>
          <div className="stat-subtext">
            {t('dashboard.admin.cumulative')}: ${totalAmount.toFixed(2)}
          </div>
        </div>

        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon text-success">✓</span>
            <span className="stat-label">{t('dashboard.admin.paidInvoices')}</span>
          </div>
          <div className="stat-value text-success">{paidInvoices.length}</div>
          <div className="stat-subtext">
            {t('dashboard.admin.collected')}: ${paidAmount.toFixed(2)}
          </div>
        </div>

        <div className="card stat-card">
          <div className="stat-header">
            <span className="stat-icon text-warning">⏳</span>
            <span className="stat-label">{t('dashboard.admin.unpaidInvoices')}</span>
          </div>
          <div className="stat-value text-warning">{unpaidInvoices.length}</div>
          <div className="stat-subtext">
            {t('dashboard.admin.outstanding')}: ${unpaidAmount.toFixed(2)}
          </div>
        </div>

        <div className="card stat-card highlight-card">
          <div className="stat-header">
            <span className="stat-icon text-danger">⚠️</span>
            <span className="stat-label">{t('dashboard.admin.pendingVerification')}</span>
          </div>
          <div className="stat-value text-danger">{pendingPayments.length}</div>
          <div className="stat-subtext">
            {pendingPayments.length > 0 ? (
              <Link to="/admin/payments" className="alert-link">
                {t('dashboard.admin.immediateAction')}
              </Link>
            ) : (
              t('dashboard.admin.allVerified')
            )}
          </div>
        </div>
      </div>

      {/* Pending Reviews Notice */}
      {pendingPayments.length > 0 && (
        <div className="card pending-alert-card">
          <div className="pending-alert-content">
            <div>
              <h3 className="pending-alert-title">
                {t('dashboard.admin.pendingAlertTitle', { count: pendingPayments.length })}
              </h3>
              <p className="pending-alert-desc">
                {t('dashboard.admin.pendingAlertDesc')}
              </p>
            </div>
            <Link to="/admin/payments">
              <Button variant="primary" size="md">
                {t('dashboard.admin.openQueue')}
              </Button>
            </Link>
          </div>
        </div>
      )}

      {/* Quick Navigation Cards */}
      <div className="quick-nav-grid">
        <Link to="/admin/invoices" className="card quick-nav-card">
          <div className="quick-nav-icon">📊</div>
          <div>
            <h4 className="quick-nav-title">{t('dashboard.admin.manageInvoicesTitle')}</h4>
            <p className="quick-nav-desc">{t('dashboard.admin.manageInvoicesDesc')}</p>
          </div>
        </Link>

        <Link to="/admin/customers" className="card quick-nav-card">
          <div className="quick-nav-icon">👥</div>
          <div>
            <h4 className="quick-nav-title">
              {t('dashboard.admin.customersTitle', { count: customerCount })}
            </h4>
            <p className="quick-nav-desc">{t('dashboard.admin.customersDesc')}</p>
          </div>
        </Link>

        <Link to="/admin/invoices/create" className="card quick-nav-card">
          <div className="quick-nav-icon">✍️</div>
          <div>
            <h4 className="quick-nav-title">{t('dashboard.admin.createInvoiceTitle')}</h4>
            <p className="quick-nav-desc">{t('dashboard.admin.createInvoiceDesc')}</p>
          </div>
        </Link>
      </div>

      {/* Recent Invoices Table */}
      <div className="card recent-table-card">
        <div className="recent-header">
          <h2 className="section-title">{t('dashboard.admin.latestInvoices')}</h2>
          <Link to="/admin/invoices" className="link-subtle">
            {t('dashboard.admin.viewAll', { count: totalInvoices })}
          </Link>
        </div>

        {invoices.length === 0 ? (
          <p className="empty-state">{t('dashboard.admin.noInvoices')}</p>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('invoices.table.invoiceNumber')}</th>
                  <th>{t('invoices.table.customer')}</th>
                  <th>{t('invoices.table.amount')}</th>
                  <th>{t('invoices.table.dueDate')}</th>
                  <th>{t('invoices.table.status')}</th>
                  <th style={{ textAlign: 'right' }}>{t('invoices.table.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {invoices.slice(0, 5).map((inv) => (
                  <tr key={inv.id}>
                    <td className="font-semibold">
                      <Link to={`/admin/invoices?search=${encodeURIComponent(inv.invoiceNumber)}`} className="invoice-link">
                        {inv.invoiceNumber}
                      </Link>
                    </td>
                    <td>{inv.customerName || inv.customerEmail}</td>
                    <td className="font-semibold">${Number(inv.amount).toFixed(2)}</td>
                    <td>{new Date(inv.dueDate).toLocaleDateString()}</td>
                    <td>
                      <InvoiceStatusBadge status={inv.status} />
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <Link to={`/admin/invoices`}>
                        <Button variant="secondary" size="sm">
                          {t('dashboard.admin.inspect')}
                        </Button>
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

export default AdminDashboard;
