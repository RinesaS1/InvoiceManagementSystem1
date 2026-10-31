import { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import paymentService from '../services/paymentService';
import invoiceService from '../services/invoiceService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import { PaymentStatusBadge } from '../components/Badge';
import './PaymentVerification.css';

function PaymentVerificationPage() {
  const { t } = useTranslation();
  const [pendingPayments, setPendingPayments] = useState([]);
  const [processedPayments, setProcessedPayments] = useState([]);
  const [activeTab, setActiveTab] = useState('pending'); // 'pending' | 'processed'
  const [loading, setLoading] = useState(true);
  const [actionLoadingId, setActionLoadingId] = useState(null);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // Rejection modal state
  const [rejectModalPayment, setRejectModalPayment] = useState(null);
  const [rejectionReason, setRejectionReason] = useState('');
  const [rejectModalError, setRejectModalError] = useState('');

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [pendingRes, invoicesRes] = await Promise.all([
        paymentService.getPending(),
        invoiceService.getAll(),
      ]);

      if (pendingRes && pendingRes.success) {
        setPendingPayments(pendingRes.data || []);
      }

      // Collect all verified / rejected payments from invoices history
      if (invoicesRes && invoicesRes.success) {
        const allInvoices = invoicesRes.data || [];
        const processed = [];
        allInvoices.forEach((inv) => {
          if (inv.payments && inv.payments.length > 0) {
            inv.payments.forEach((p) => {
              if (p.status === 1 || p.status === 2) {
                // Confirmed or Rejected
                processed.push({
                  ...p,
                  invoiceNumber: inv.invoiceNumber,
                  customerName: inv.customerName,
                  customerEmail: inv.customerEmail,
                  amount: inv.amount,
                });
              }
            });
          }
        });
        processed.sort((a, b) => new Date(b.verifiedAt || b.submittedAt) - new Date(a.verifiedAt || a.submittedAt));
        setProcessedPayments(processed);
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Could not load payment submissions from the server.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleConfirm = async (payment) => {
    if (!window.confirm(t('payments.confirmPrompt', { number: payment.invoiceNumber }))) {
      return;
    }

    setActionLoadingId(payment.id);
    setError('');
    setSuccessMsg('');
    try {
      const res = await paymentService.confirm(payment.id);
      if (res && res.success) {
        setSuccessMsg(`Payment for ${payment.invoiceNumber} successfully confirmed! Related invoice is now marked as Paid.`);
        await loadData();
      } else {
        setError(res?.message || 'Failed to confirm payment.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to confirm payment.');
    } finally {
      setActionLoadingId(null);
    }
  };

  const openRejectModal = (payment) => {
    setRejectModalPayment(payment);
    setRejectionReason('');
    setRejectModalError('');
  };

  const handleRejectSubmit = async (e) => {
    e.preventDefault();
    if (!rejectionReason.trim()) {
      setRejectModalError(t('payments.rejectModal.reasonRequired'));
      return;
    }

    setActionLoadingId(rejectModalPayment.id);
    setRejectModalError('');
    try {
      const res = await paymentService.reject(rejectModalPayment.id, rejectionReason.trim());
      if (res && res.success) {
        setSuccessMsg(`Payment submission #${rejectModalPayment.id} rejected. The invoice remains Unpaid.`);
        setRejectModalPayment(null);
        await loadData();
      } else {
        setRejectModalError(res?.message || 'Failed to reject payment.');
      }
    } catch (err) {
      setRejectModalError(err.response?.data?.message || 'Failed to reject payment.');
    } finally {
      setActionLoadingId(null);
    }
  };

  return (
    <div className="payment-verification-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{t('payments.title')}</h1>
          <p className="page-subtitle">{t('payments.subtitle')}</p>
        </div>
        <Button variant="outline" onClick={loadData}>
          {t('payments.refresh')}
        </Button>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />
      <Alert type="success" message={successMsg} onClose={() => setSuccessMsg('')} />

      {/* Tabs */}
      <div className="tabs-container">
        <button
          type="button"
          className={`tab-btn ${activeTab === 'pending' ? 'active' : ''}`}
          onClick={() => setActiveTab('pending')}
        >
          {t('payments.pendingTab')} <span className="tab-badge">{pendingPayments.length}</span>
        </button>
        <button
          type="button"
          className={`tab-btn ${activeTab === 'processed' ? 'active' : ''}`}
          onClick={() => setActiveTab('processed')}
        >
          {t('payments.processedTab')} <span className="tab-badge">{processedPayments.length}</span>
        </button>
      </div>

      {loading ? (
        <div className="page-center">
          <Spinner size="lg" />
        </div>
      ) : activeTab === 'pending' ? (
        /* PENDING TAB */
        pendingPayments.length === 0 ? (
          <div className="card empty-state-card">
            <p className="empty-title">{t('payments.allCaughtUp')}</p>
            <p className="empty-description">{t('payments.noPending')}</p>
          </div>
        ) : (
          <div className="card table-card">
            <div className="table-responsive">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>{t('payments.table.invoiceNumber')}</th>
                    <th>{t('payments.table.customer')}</th>
                    <th>{t('payments.table.invoiceAmount')}</th>
                    <th>{t('payments.table.submittedDate')}</th>
                    <th>{t('payments.table.status')}</th>
                    <th style={{ textAlign: 'right' }}>{t('payments.table.actions')}</th>
                  </tr>
                </thead>
                <tbody>
                  {pendingPayments.map((p) => (
                    <tr key={p.id}>
                      <td className="font-semibold">
                        <Link to={`/admin/invoices?search=${encodeURIComponent(p.invoiceNumber)}`} className="invoice-link">
                          {p.invoiceNumber}
                        </Link>
                      </td>
                      <td>
                        <div className="font-semibold">{p.customerName || 'Customer'}</div>
                        <div className="customer-email-sub">{p.customerId}</div>
                      </td>
                      <td className="font-semibold text-primary" style={{ fontSize: '1rem' }}>
                        ${Number(p.invoiceAmount).toFixed(2)}
                      </td>
                      <td>{new Date(p.submittedAt).toLocaleString()}</td>
                      <td>
                        <PaymentStatusBadge status={p.status} />
                      </td>
                      <td style={{ textAlign: 'right' }}>
                        <div className="action-buttons">
                          <Button
                            variant="primary"
                            size="sm"
                            loading={actionLoadingId === p.id}
                            onClick={() => handleConfirm(p)}
                          >
                            {t('payments.confirmBtn')}
                          </Button>
                          <Button
                            variant="danger"
                            size="sm"
                            disabled={actionLoadingId === p.id}
                            onClick={() => openRejectModal(p)}
                          >
                            {t('payments.rejectBtn')}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )
      ) : (
        /* PROCESSED TAB */
        processedPayments.length === 0 ? (
          <div className="card empty-state-card">
            <p className="empty-title">{t('payments.noProcessed')}</p>
            <p className="empty-description">{t('payments.noProcessedDesc')}</p>
          </div>
        ) : (
          <div className="card table-card">
            <div className="table-responsive">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>{t('payments.table.invoiceNumber')}</th>
                    <th>{t('payments.table.customer')}</th>
                    <th>{t('payments.table.amount')}</th>
                    <th>{t('payments.table.status')}</th>
                    <th>{t('payments.table.verifiedBy')}</th>
                    <th>{t('payments.table.verifiedDate')}</th>
                    <th>{t('payments.table.rejectionReason')}</th>
                  </tr>
                </thead>
                <tbody>
                  {processedPayments.map((p) => (
                    <tr key={p.id}>
                      <td className="font-semibold">
                        <Link to={`/admin/invoices?search=${encodeURIComponent(p.invoiceNumber)}`} className="invoice-link">
                          {p.invoiceNumber}
                        </Link>
                      </td>
                      <td>{p.customerName || p.customerEmail}</td>
                      <td>${Number(p.amount).toFixed(2)}</td>
                      <td>
                        <PaymentStatusBadge status={p.status} />
                      </td>
                      <td>{p.verifiedByName || 'Admin'}</td>
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
          </div>
        )
      )}

      {/* REJECT PAYMENT MODAL */}
      {rejectModalPayment && (
        <div className="modal-backdrop" onClick={() => setRejectModalPayment(null)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()} style={{ maxWidth: '500px' }}>
            <div className="modal-header">
              <h3 className="modal-title">{t('payments.rejectModal.title')}</h3>
              <button className="modal-close-btn" onClick={() => setRejectModalPayment(null)}>×</button>
            </div>

            <form onSubmit={handleRejectSubmit}>
              <div className="modal-body">
                <Alert type="error" message={rejectModalError} onClose={() => setRejectModalError('')} />

                <div className="reject-info-summary">
                  <p><strong>{t('payments.rejectModal.invoice')}</strong> {rejectModalPayment.invoiceNumber}</p>
                  <p><strong>{t('payments.rejectModal.customer')}</strong> {rejectModalPayment.customerName}</p>
                  <p><strong>{t('payments.rejectModal.amount')}</strong> ${Number(rejectModalPayment.invoiceAmount).toFixed(2)}</p>
                </div>

                <div className="form-group" style={{ marginTop: '1rem' }}>
                  <label htmlFor="rejectionReason" className="form-label">
                    {t('payments.rejectModal.reasonLabel')} <span className="form-required">*</span>
                  </label>
                  <textarea
                    id="rejectionReason"
                    className="form-control"
                    rows="3"
                    placeholder={t('payments.rejectModal.reasonPlaceholder')}
                    value={rejectionReason}
                    onChange={(e) => setRejectionReason(e.target.value)}
                    required
                  />
                  <p className="form-help-text">
                    {t('payments.rejectModal.reasonHelp')}
                  </p>
                </div>
              </div>

              <div className="modal-footer">
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => setRejectModalPayment(null)}
                >
                  {t('payments.rejectModal.cancel')}
                </Button>
                <Button
                  type="submit"
                  variant="danger"
                  loading={actionLoadingId === rejectModalPayment.id}
                >
                  {t('payments.rejectModal.confirmRejection')}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default PaymentVerificationPage;
