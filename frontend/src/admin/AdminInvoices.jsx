import { useState, useEffect, useCallback } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import invoiceService from '../services/invoiceService';
import Spinner from '../components/Spinner';
import Alert from '../components/Alert';
import Button from '../components/Button';
import FormInput from '../components/FormInput';
import { InvoiceStatusBadge, PaymentStatusBadge } from '../components/Badge';
import './AdminInvoices.css';

function AdminInvoices() {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const initialSearch = searchParams.get('search') || '';
  const customerIdParam = searchParams.get('customerId') || '';

  const [invoices, setInvoices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState(initialSearch);
  const [statusFilter, setStatusFilter] = useState('');
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [downloadingId, setDownloadingId] = useState(null);

  // Modal states
  const [selectedInvoice, setSelectedInvoice] = useState(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editForm, setEditForm] = useState(null);
  const [editLoading, setEditLoading] = useState(false);
  const [editError, setEditError] = useState('');

  const fetchInvoices = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const response = await invoiceService.getAll({
        search: search.trim(),
        status: statusFilter,
      });

      if (response && response.success) {
        let list = response.data || [];
        if (customerIdParam) {
          list = list.filter((i) => i.customerId === customerIdParam);
        }
        setInvoices(list);
      } else {
        setError(response?.message || 'Failed to fetch invoices.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Error communicating with the invoices API.');
    } finally {
      setLoading(false);
    }
  }, [search, statusFilter, customerIdParam]);

  useEffect(() => {
    fetchInvoices();
  }, [fetchInvoices]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setSearchParams(search ? { search } : {});
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

  const openDetailsModal = (invoice) => {
    setSelectedInvoice(invoice);
    setIsDetailOpen(true);
  };

  const openEditModal = (invoice) => {
    setSelectedInvoice(invoice);
    setEditForm({
      invoiceNumber: invoice.invoiceNumber,
      description: invoice.description || '',
      amount: invoice.amount,
      issueDate: invoice.issueDate.split('T')[0],
      dueDate: invoice.dueDate.split('T')[0],
      notes: invoice.notes || '',
      status: invoice.status,
    });
    setEditError('');
    setIsEditOpen(true);
  };

  const handleEditChange = (e) => {
    const { name, value } = e.target;
    setEditForm((prev) => ({
      ...prev,
      [name]: name === 'status' ? parseInt(value, 10) : value,
    }));
  };

  const handleEditSubmit = async (e) => {
    e.preventDefault();
    setEditLoading(true);
    setEditError('');

    try {
      const payload = {
        invoiceNumber: editForm.invoiceNumber,
        description: editForm.description,
        amount: parseFloat(editForm.amount),
        issueDate: editForm.issueDate,
        dueDate: editForm.dueDate,
        notes: editForm.notes,
        status: editForm.status,
      };

      const res = await invoiceService.update(selectedInvoice.id, payload);
      if (res && res.success) {
        setSuccessMsg(`Invoice ${editForm.invoiceNumber} updated successfully!`);
        setIsEditOpen(false);
        await fetchInvoices();
      } else {
        setEditError(res?.message || 'Failed to update invoice.');
      }
    } catch (err) {
      setEditError(err.response?.data?.message || 'Failed to save invoice changes.');
    } finally {
      setEditLoading(false);
    }
  };

  const handleDelete = async (id, invoiceNumber) => {
    if (!window.confirm(t('invoices.actions.confirmDelete', { number: invoiceNumber }))) {
      return;
    }

    try {
      const res = await invoiceService.delete(id);
      if (res && res.success) {
        setSuccessMsg(`Invoice ${invoiceNumber} deleted successfully.`);
        await fetchInvoices();
      } else {
        setError(res?.message || 'Could not delete invoice.');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Cannot delete invoice (only Unpaid invoices without confirmed payments can be removed).');
    }
  };

  return (
    <div className="admin-invoices-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{t('invoices.title')}</h1>
          <p className="page-subtitle">{t('invoices.subtitle')}</p>
        </div>
        <Link to="/admin/invoices/create">
          <Button variant="primary">{t('invoices.createBtn')}</Button>
        </Link>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />
      <Alert type="success" message={successMsg} onClose={() => setSuccessMsg('')} />

      {customerIdParam && (
        <div className="banner banner-info" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span>{t('invoices.filteringCustomer')}</span>
          <Button variant="secondary" size="sm" onClick={() => setSearchParams({})}>
            {t('invoices.showAllInvoices')}
          </Button>
        </div>
      )}

      {/* Filter and Search Bar */}
      <div className="card filter-bar">
        <form onSubmit={handleSearchSubmit} className="search-form">
          <input
            type="text"
            className="search-input"
            placeholder={t('invoices.adminSearchPlaceholder')}
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
                setSearchParams({});
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
          <label htmlFor="admin-status-select" className="filter-label">
            {t('invoices.filterStatus')}
          </label>
          <select
            id="admin-status-select"
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

      {/* Invoices Table */}
      {loading ? (
        <div className="page-center">
          <Spinner size="lg" />
        </div>
      ) : invoices.length === 0 ? (
        <div className="card empty-state-card">
          <p className="empty-title">{t('invoices.noInvoicesFound')}</p>
          <p className="empty-description">{t('invoices.noInvoicesAdmin')}</p>
        </div>
      ) : (
        <div className="card table-card">
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>{t('invoices.table.invoiceNumber')}</th>
                  <th>{t('invoices.table.customer')}</th>
                  <th>{t('invoices.table.description')}</th>
                  <th>{t('invoices.table.amount')}</th>
                  <th>{t('invoices.table.dueDate')}</th>
                  <th>{t('invoices.table.status')}</th>
                  <th>{t('invoices.table.payments')}</th>
                  <th style={{ textAlign: 'right' }}>{t('invoices.table.actions')}</th>
                </tr>
              </thead>
              <tbody>
                {invoices.map((inv) => {
                  const latestPayment =
                    inv.payments && inv.payments.length > 0
                      ? inv.payments[inv.payments.length - 1]
                      : null;

                  return (
                    <tr key={inv.id}>
                      <td className="font-semibold">
                        <button
                          type="button"
                          className="link-btn"
                          onClick={() => openDetailsModal(inv)}
                        >
                          {inv.invoiceNumber}
                        </button>
                      </td>
                      <td>
                        <div className="font-semibold">{inv.customerName || '—'}</div>
                        <div className="customer-email-sub">{inv.customerEmail}</div>
                      </td>
                      <td className="desc-cell">{inv.description || '—'}</td>
                      <td className="font-semibold">${Number(inv.amount).toFixed(2)}</td>
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
                        <div className="action-buttons">
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => openDetailsModal(inv)}
                          >
                            {t('invoices.actions.details')}
                          </Button>

                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => openEditModal(inv)}
                          >
                            {t('invoices.actions.edit')}
                          </Button>

                          <Button
                            variant="outline"
                            size="sm"
                            loading={downloadingId === inv.id}
                            onClick={() => handleDownloadPdf(inv.id, inv.invoiceNumber)}
                          >
                            {t('invoices.actions.pdf')}
                          </Button>

                          <Button
                            variant="danger"
                            size="sm"
                            onClick={() => handleDelete(inv.id, inv.invoiceNumber)}
                          >
                            ✕
                          </Button>
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

      {/* VIEW DETAILS MODAL */}
      {isDetailOpen && selectedInvoice && (
        <div className="modal-backdrop" onClick={() => setIsDetailOpen(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                {t('invoices.details.title', { number: selectedInvoice.invoiceNumber })}
              </h3>
              <button className="modal-close-btn" onClick={() => setIsDetailOpen(false)}>×</button>
            </div>

            <div className="modal-body">
              <div className="info-grid">
                <div className="info-item">
                  <span className="info-label">{t('invoices.table.customer')}</span>
                  <span className="info-value font-semibold">
                    {selectedInvoice.customerName} ({selectedInvoice.customerEmail})
                  </span>
                </div>
                <div className="info-item">
                  <span className="info-label">{t('invoices.table.amount')}</span>
                  <span className="info-value font-bold text-primary" style={{ fontSize: '1.2rem' }}>
                    ${Number(selectedInvoice.amount).toFixed(2)}
                  </span>
                </div>
                <div className="info-item">
                  <span className="info-label">{t('invoices.table.status')}</span>
                  <span className="info-value">
                    <InvoiceStatusBadge status={selectedInvoice.status} />
                  </span>
                </div>
                <div className="info-item">
                  <span className="info-label">{t('invoices.table.issueDate')}</span>
                  <span className="info-value">{new Date(selectedInvoice.issueDate).toLocaleDateString()}</span>
                </div>
                <div className="info-item">
                  <span className="info-label">{t('invoices.table.dueDate')}</span>
                  <span className="info-value">{new Date(selectedInvoice.dueDate).toLocaleDateString()}</span>
                </div>
                <div className="info-item">
                  <span className="info-label">{t('invoices.details.createdAt')}</span>
                  <span className="info-value">{new Date(selectedInvoice.createdAt).toLocaleDateString()}</span>
                </div>
              </div>

              <div style={{ marginTop: '1rem' }}>
                <span className="info-label">{t('invoices.table.description')}</span>
                <p className="desc-text">{selectedInvoice.description || t('invoices.details.noDescription')}</p>
              </div>

              {selectedInvoice.notes && (
                <div style={{ marginTop: '0.75rem' }}>
                  <span className="info-label">{t('invoices.details.notes')}</span>
                  <p className="notes-text">{selectedInvoice.notes}</p>
                </div>
              )}

              {/* Payments History */}
              <div style={{ marginTop: '1.25rem' }}>
                <h4 style={{ fontSize: '0.95rem', fontWeight: 700, marginBottom: '0.5rem' }}>
                  {t('invoices.details.paymentHistory')} ({selectedInvoice.payments?.length || 0})
                </h4>
                {selectedInvoice.payments?.length === 0 ? (
                  <p style={{ fontSize: '0.85rem', color: 'var(--color-gray-500)' }}>
                    {t('invoices.details.noPayments')}
                  </p>
                ) : (
                  <table className="data-table" style={{ fontSize: '0.85rem' }}>
                    <thead>
                      <tr>
                        <th>{t('invoices.details.subDate')}</th>
                        <th>{t('invoices.table.status')}</th>
                        <th>{t('invoices.details.verifiedBy')}</th>
                        <th>{t('invoices.details.rejectionReason')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {selectedInvoice.payments.map((p) => (
                        <tr key={p.id}>
                          <td>{new Date(p.submittedAt).toLocaleDateString()}</td>
                          <td><PaymentStatusBadge status={p.status} /></td>
                          <td>{p.verifiedByName || '—'}</td>
                          <td>{p.rejectionReason || '—'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </div>

            <div className="modal-footer">
              <Button
                variant="outline"
                loading={downloadingId === selectedInvoice.id}
                onClick={() => handleDownloadPdf(selectedInvoice.id, selectedInvoice.invoiceNumber)}
              >
                {t('invoices.details.downloadPdf')}
              </Button>
              <Button variant="secondary" onClick={() => setIsDetailOpen(false)}>
                {t('common.close')}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* EDIT INVOICE MODAL */}
      {isEditOpen && editForm && (
        <div className="modal-backdrop" onClick={() => setIsEditOpen(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                {t('invoices.edit.title', { number: selectedInvoice?.invoiceNumber })}
              </h3>
              <button className="modal-close-btn" onClick={() => setIsEditOpen(false)}>×</button>
            </div>

            <form onSubmit={handleEditSubmit}>
              <div className="modal-body">
                <Alert type="error" message={editError} onClose={() => setEditError('')} />

                <FormInput
                  id="invoiceNumber"
                  name="invoiceNumber"
                  label={t('invoices.edit.invoiceNumber')}
                  value={editForm.invoiceNumber}
                  onChange={handleEditChange}
                  required
                />

                <FormInput
                  id="amount"
                  name="amount"
                  label={t('invoices.edit.amount')}
                  type="number"
                  step="0.01"
                  min="0.01"
                  value={editForm.amount}
                  onChange={handleEditChange}
                  required
                />

                <div className="grid-2-col">
                  <FormInput
                    id="issueDate"
                    name="issueDate"
                    label={t('invoices.edit.issueDate')}
                    type="date"
                    value={editForm.issueDate}
                    onChange={handleEditChange}
                    required
                  />

                  <FormInput
                    id="dueDate"
                    name="dueDate"
                    label={t('invoices.edit.dueDate')}
                    type="date"
                    value={editForm.dueDate}
                    onChange={handleEditChange}
                    required
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="status" className="form-label">{t('invoices.edit.status')}</label>
                  <select
                    id="status"
                    name="status"
                    className="form-control"
                    value={editForm.status}
                    onChange={handleEditChange}
                  >
                    <option value={0}>{t('invoices.status.unpaid')}</option>
                    <option value={1}>{t('invoices.status.paid')}</option>
                  </select>
                </div>

                <FormInput
                  id="description"
                  name="description"
                  label={t('invoices.edit.description')}
                  value={editForm.description}
                  onChange={handleEditChange}
                />

                <FormInput
                  id="notes"
                  name="notes"
                  label={t('invoices.edit.notes')}
                  value={editForm.notes}
                  onChange={handleEditChange}
                />
              </div>

              <div className="modal-footer">
                <Button type="button" variant="secondary" onClick={() => setIsEditOpen(false)}>
                  {t('invoices.edit.cancel')}
                </Button>
                <Button type="submit" variant="primary" loading={editLoading}>
                  {t('invoices.edit.saveChanges')}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default AdminInvoices;
