import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import invoiceService from '../services/invoiceService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import { InvoiceStatusBadge, PaymentStatusBadge } from '../components/Badge';
import './MyInvoices.css';

function MyInvoicesPage() {
  const { t } = useTranslation();
  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [downloadingId, setDownloadingId] = useState(null);
  const [payingId, setPayingId] = useState(null);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // Search & filter state
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const fetchInvoices = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await invoiceService.getAll({
        search: search.trim(),
        status: statusFilter,
      });

      if (response && response.success) {
        setInvoices(response.data || []);
      } else {
        setError(response?.message || 'Failed to fetch invoices.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Could not load your invoices from the server.');
    } finally {
      setLoading(false);
    }
  }, [search, statusFilter]);

  useEffect(() => {
    fetchInvoices();
  }, [statusFilter]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    fetchInvoices();
  };

  const handleDownloadPdf = async (id, invoiceNumber) => {
    setDownloadingId(id);
    setError('');
    try {
      await invoiceService.savePdfFile(id, invoiceNumber);
    } catch (err) {
      setError('Failed to download invoice PDF.');
    } finally {
      setDownloadingId(null);
    }
  };

  const handlePay = async (id, invoiceNumber) => {
    setPayingId(id);
    setError('');
    setSuccessMsg('');
    try {
      const res = await invoiceService.submitPaymentRequest(id);
      if (res && res.success) {
        setSuccessMsg(`Payment notification for ${invoiceNumber} submitted successfully! It is now pending admin review.`);
        await fetchInvoices();
      } else {
        setError(res?.message || 'Failed to submit payment notification.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Could not submit payment notification.');
    } finally {
      setPayingId(null);
    }
  };

  return (
    <div className="my-invoices-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{t('invoices.customerTitle')}</h1>
          <p className="page-subtitle">{t('invoices.customerSubtitle')}</p>
        </div>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />
      <Alert type="success" message={successMsg} onClose={() => setSuccessMsg('')} />

      {/* Filter and Search Bar */}
      <div className="card filter-bar">
        <form onSubmit={handleSearchSubmit} className="search-form">
          <input
            type="text"
            className="search-input"
            placeholder={t('invoices.searchPlaceholder')}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          <Button type="submit" variant="primary" size="md">
            {t('invoices.searchBtn')}
          </Button>
          {search && (
            <Button
              type="button"
              variant="secondary"
              size="md"
              onClick={() => {
                setSearch('');
                invoiceService.getAll({ search: '', status: statusFilter }).then((res) => {
                  if (res?.success) setInvoices(res.data || []);
                });
              }}
            >
              {t('invoices.clearBtn')}
            </Button>
          )}
        </form>

        <div className="filter-group">
          <label htmlFor="status-select" className="filter-label">
            {t('invoices.filterStatus')}
          </label>
          <select
            id="status-select"
            className="filter-select"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">{t('invoices.allStatuses')}</option>
            <option value="0">{t('invoices.status.unpaid')}</option>
            <option value="1">{t('invoices.status.paid')}</option>
          </select>
        </div>
      </div>

      {/* Invoices List */}
      {loading ? (
        <div className="page-center">
          <Spinner size="lg" />
        </div>
      ) : invoices.length === 0 ? (
        <div className="card empty-state-card">
          <p className="empty-title">{t('invoices.noInvoicesFound')}</p>
          <p className="empty-description">
            {search || statusFilter !== ''
              ? t('invoices.noInvoicesAdmin')
              : t('invoices.noInvoicesCustomer')}
          </p>
        </div>
      ) : (
        <div className="card table-card">
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('invoices.table.invoiceNumber')}</th>
                  <th>{t('invoices.table.description')}</th>
                  <th>{t('invoices.table.amount')}</th>
                  <th>{t('invoices.table.issueDate')}</th>
                  <th>{t('invoices.table.dueDate')}</th>
                  <th>{t('invoices.table.status')}</th>
                  <th>{t('invoices.table.paymentStatus')}</th>
                  <th style={{ textAlign: 'right' }}>{t('invoices.table.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {invoices.map((inv) => {
                  const hasPendingPayment =
                    inv.payments && inv.payments.some((p) => p.status === 0);
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
                      <td className="desc-cell">{inv.description || '—'}</td>
                      <td className="font-semibold">${Number(inv.amount).toFixed(2)}</td>
                      <td>{new Date(inv.issueDate).toLocaleDateString()}</td>
                      <td>{new Date(inv.dueDate).toLocaleDateString()}</td>
                      <td>
                        <InvoiceStatusBadge status={inv.status} />
                      </td>
                      <td>
                        {latestPayment ? (
                          <div>
                            <PaymentStatusBadge status={latestPayment.status} />
                            {latestPayment.status === 2 && latestPayment.rejectionReason && (
                              <div className="rejected-hint" title={latestPayment.rejectionReason}>
                                {t('invoices.details.reasonPrefix')}{latestPayment.rejectionReason}
                              </div>
                            )}
                          </div>
                        ) : (
                          <span className="text-muted">—</span>
                        )}
                      </td>
                      <td style={{ textAlign: 'right' }}>
                        <div className="action-buttons">
                          <Link to={`/customer/invoices/${inv.id}`}>
                            <Button variant="secondary" size="sm">
                              {t('invoices.actions.details')}
                            </Button>
                          </Link>

                          <Button
                            variant="outline"
                            size="sm"
                            loading={downloadingId === inv.id}
                            onClick={() => handleDownloadPdf(inv.id, inv.invoiceNumber)}
                          >
                            {t('invoices.actions.pdf')}
                          </Button>

                          {inv.status === 0 && (
                            <Button
                              variant="primary"
                              size="sm"
                              disabled={hasPendingPayment}
                              loading={payingId === inv.id}
                              onClick={() => handlePay(inv.id, inv.invoiceNumber)}
                            >
                              {hasPendingPayment
                                ? t('invoices.actions.pendingReview')
                                : t('invoices.actions.iHavePaid')}
                            </Button>
                          )}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

export default MyInvoicesPage;
