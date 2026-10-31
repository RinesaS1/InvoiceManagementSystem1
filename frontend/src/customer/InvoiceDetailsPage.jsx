import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import invoiceService from '../services/invoiceService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import { InvoiceStatusBadge, PaymentStatusBadge } from '../components/Badge';
import './InvoiceDetails.css';

function InvoiceDetailsPage() {
  const { id } = useParams();
  const { t } = useTranslation();
  const [invoice, setInvoice] = useState(null);
  const [loading, setLoading] = useState(true);
  const [downloading, setDownloading] = useState(false);
  const [paying, setPaying] = useState(false);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  const fetchInvoice = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await invoiceService.getById(id);
      if (response && response.success && response.data) {
        setInvoice(response.data);
      } else {
        setError(response?.message || 'Invoice not found.');
      }
    } catch (err) {
      if (err.response?.status === 404) {
        setError('Invoice not found or you do not have permission to view it.');
      } else {
        setError(err.response?.data?.message || 'Could not load invoice details.');
      }
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchInvoice();
  }, [fetchInvoice]);

  const handleDownloadPdf = async () => {
    if (!invoice) return;
    setDownloading(true);
    setError('');
    try {
      await invoiceService.savePdfFile(invoice.id, invoice.invoiceNumber);
    } catch (err) {
      setError('Failed to download invoice PDF.');
    } finally {
      setDownloading(false);
    }
  };

  const handlePay = async () => {
    if (!invoice) return;
    setPaying(true);
    setError('');
    setSuccessMsg('');
    try {
      const res = await invoiceService.submitPaymentRequest(invoice.id);
      if (res && res.success) {
        setSuccessMsg('Your payment notification has been submitted! An administrator will review and verify your payment.');
        await fetchInvoice();
      } else {
        setError(res?.message || 'Failed to submit payment request.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Could not submit payment request.');
    } finally {
      setPaying(false);
    }
  };

  if (loading) {
    return (
      <div className="page-center">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error && !invoice) {
    return (
      <div className="invoice-details-page">
        <Alert type="error" message={error} />
        <Link to="/customer/invoices">
          <Button variant="outline">{t('invoices.details.backToInvoices')}</Button>
        </Link>
      </div>
    );
  }

  if (!invoice) return null;

  const isPaid = invoice.status === 1;
  const payments = invoice.payments || [];
  const pendingPayment = payments.find((p) => p.status === 0);
  const latestRejectedPayment = [...payments].reverse().find((p) => p.status === 2);

  return (
    <div className="invoice-details-page">
      {/* Top Header */}
      <div className="page-header">
        <div>
          <div className="breadcrumb">
            <Link to="/customer/invoices">{t('invoices.details.breadcrumb')}</Link> / <span>{invoice.invoiceNumber}</span>
          </div>
          <h1 className="page-title">{t('invoices.details.title', { number: invoice.invoiceNumber })}</h1>
        </div>
        <div className="header-actions">
          <Link to="/customer/invoices">
            <Button variant="secondary">{t('invoices.details.back')}</Button>
          </Link>
          <Button
            variant="outline"
            loading={downloading}
            onClick={handleDownloadPdf}
          >
            {t('invoices.details.downloadPdf')}
          </Button>
        </div>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />
      <Alert type="success" message={successMsg} onClose={() => setSuccessMsg('')} />

      {/* Payment Status Notification Banner */}
      {isPaid && (
        <div className="banner banner-success">
          <strong>{t('invoices.details.banners.paid')}</strong>
        </div>
      )}

      {!isPaid && pendingPayment && (
        <div className="banner banner-info">
          <strong>
            {t('invoices.details.banners.pending', {
              date: new Date(pendingPayment.submittedAt).toLocaleString(),
            })}
          </strong>
        </div>
      )}

      {!isPaid && latestRejectedPayment && !pendingPayment && (
        <div className="banner banner-warning">
          <strong>
            {t('invoices.details.banners.rejected', {
              reason: latestRejectedPayment.rejectionReason || '—',
            })}
          </strong>
        </div>
      )}

      {/* Main Grid: Details & Payment Action */}
      <div className="details-grid">
        {/* Left Column: Invoice Details */}
        <div className="card invoice-main-card">
          <div className="card-section">
            <h3 className="section-heading">{t('invoices.details.information')}</h3>
            <div className="info-grid">
              <div className="info-item">
                <span className="info-label">{t('invoices.table.invoiceNumber')}</span>
                <span className="info-value font-bold">{invoice.invoiceNumber}</span>
              </div>
              <div className="info-item">
                <span className="info-label">{t('invoices.table.status')}</span>
                <span className="info-value">
                  <InvoiceStatusBadge status={invoice.status} />
                </span>
              </div>
              <div className="info-item">
                <span className="info-label">{t('invoices.table.issueDate')}</span>
                <span className="info-value">{new Date(invoice.issueDate).toLocaleDateString()}</span>
              </div>
              <div className="info-item">
                <span className="info-label">{t('invoices.table.dueDate')}</span>
                <span className="info-value font-semibold">{new Date(invoice.dueDate).toLocaleDateString()}</span>
              </div>
              <div className="info-item">
                <span className="info-label">{t('invoices.details.billedTo')}</span>
                <span className="info-value">{invoice.customerName} ({invoice.customerEmail})</span>
              </div>
              <div className="info-item">
                <span className="info-label">{t('invoices.details.createdAt')}</span>
                <span className="info-value">{new Date(invoice.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
          </div>

          <div className="card-section">
            <h3 className="section-heading">{t('invoices.table.description')}</h3>
            <p className="desc-text">{invoice.description || t('invoices.details.noDescription')}</p>
          </div>

          {invoice.notes && (
            <div className="card-section">
              <h3 className="section-heading">{t('invoices.details.notes')}</h3>
              <p className="notes-text">{invoice.notes}</p>
            </div>
          )}
        </div>

        {/* Right Column: Amount Summary & Payment Action */}
        <div className="details-sidebar">
          <div className="card summary-card">
            <h3 className="section-heading">{t('invoices.details.amountDue')}</h3>
            <div className="amount-display">${Number(invoice.amount).toFixed(2)}</div>
            <div className="amount-status-row">
              <span>{t('invoices.details.statusLabel')}</span>
              <InvoiceStatusBadge status={invoice.status} />
            </div>

            <div className="payment-action-box">
              {isPaid ? (
                <div className="paid-box">{t('invoices.details.fullyPaid')}</div>
              ) : pendingPayment ? (
                <Button variant="secondary" size="lg" disabled className="w-full">
                  {t('invoices.details.paymentUnderReview')}
                </Button>
              ) : (
                <Button
                  variant="primary"
                  size="lg"
                  loading={paying}
                  onClick={handlePay}
                  className="w-full"
                >
                  {t('invoices.actions.iHavePaid')}
                </Button>
              )}

              <p className="payment-help-text">
                {isPaid
                  ? t('invoices.details.actionRequired')
                  : pendingPayment
                  ? t('invoices.details.inReviewQueue')
                  : t('invoices.details.clickPaidHelp')}
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Payment History Table */}
      <div className="card payment-history-card">
        <h3 className="section-heading">{t('invoices.details.paymentHistory')}</h3>

        {payments.length === 0 ? (
          <p className="no-payments-text">{t('invoices.details.noPayments')}</p>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('invoices.details.subId')}</th>
                  <th>{t('invoices.details.subDate')}</th>
                  <th>{t('invoices.table.status')}</th>
                  <th>{t('invoices.details.verifiedBy')}</th>
                  <th>{t('invoices.details.verifiedAt')}</th>
                  <th>{t('invoices.details.rejectionReason')}</th>
                </tr>
              </thead>
              <tbody>
                {payments.map((p) => (
                  <tr key={p.id}>
                    <td>#{p.id}</td>
                    <td>{new Date(p.submittedAt).toLocaleString()}</td>
                    <td>
                      <PaymentStatusBadge status={p.status} />
                    </td>
                    <td>{p.verifiedByName || '—'}</td>
                    <td>{p.verifiedAt ? new Date(p.verifiedAt).toLocaleString() : '—'}</td>
                    <td>
                      {p.rejectionReason ? (
                        <span className="text-danger font-semibold">{p.rejectionReason}</span>
                      ) : (
                        '—'
                      )}
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

export default InvoiceDetailsPage;
