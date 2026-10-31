import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import customerService from '../services/customerService';
import invoiceService from '../services/invoiceService';
import FormInput from '../components/FormInput';
import Button from '../components/Button';
import Alert from '../components/Alert';
import Spinner from '../components/Spinner';
import './CreateInvoice.css';

function CreateInvoicePage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const preselectedCustomerId = searchParams.get('customerId') || '';

  const navigate = useNavigate();

  const [customers, setCustomers] = useState([]);
  const [loadingCustomers, setLoadingCustomers] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // Form fields
  const today = new Date().toISOString().split('T')[0];
  const defaultDue = new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
  const defaultInvoiceNumber = `INV-${new Date().getFullYear()}-${Math.floor(10000 + Math.random() * 90000)}`;

  const [formData, setFormData] = useState({
    customerId: preselectedCustomerId,
    invoiceNumber: defaultInvoiceNumber,
    description: '',
    amount: '',
    issueDate: today,
    dueDate: defaultDue,
    notes: '',
  });

  useEffect(() => {
    async function loadCustomers() {
      try {
        const res = await customerService.getAll();
        if (res && res.success) {
          setCustomers(res.data || []);
          if (preselectedCustomerId) {
            setFormData((prev) => ({ ...prev, customerId: preselectedCustomerId }));
          }
        }
      } catch (err) {
        setError('Could not load registered customers. Make sure you are logged in as Admin.');
      } finally {
        setLoadingCustomers(false);
      }
    }

    loadCustomers();
  }, [preselectedCustomerId]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleGenerateInvoiceNumber = () => {
    const generated = `INV-${new Date().getFullYear()}-${Math.floor(10000 + Math.random() * 90000)}`;
    setFormData((prev) => ({ ...prev, invoiceNumber: generated }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');

    // Front-end validation
    if (!formData.customerId) {
      setError(t('createInvoice.validation.selectCustomer'));
      return;
    }
    if (!formData.invoiceNumber.trim()) {
      setError(t('createInvoice.validation.numberRequired'));
      return;
    }
    const numAmount = parseFloat(formData.amount);
    if (isNaN(numAmount) || numAmount <= 0) {
      setError(t('createInvoice.validation.positiveAmount'));
      return;
    }
    if (new Date(formData.dueDate) < new Date(formData.issueDate)) {
      setError(t('createInvoice.validation.dateOrder'));
      return;
    }

    setSubmitting(true);

    try {
      const payload = {
        customerId: formData.customerId,
        invoiceNumber: formData.invoiceNumber.trim(),
        description: formData.description.trim() || null,
        amount: numAmount,
        issueDate: formData.issueDate,
        dueDate: formData.dueDate,
        notes: formData.notes.trim() || null,
      };

      const response = await invoiceService.create(payload);
      if (response && response.success) {
        setSuccessMsg(`Invoice ${payload.invoiceNumber} created successfully!`);
        setTimeout(() => {
          navigate('/admin/invoices');
        }, 1500);
      } else {
        setError(response?.message || 'Failed to create invoice.');
      }
    } catch (err) {
      let message = 'Failed to create invoice.';
      if (err.response?.data?.message) {
        message = err.response.data.message;
      } else if (err.response?.data?.errors) {
        const errors = err.response.data.errors;
        if (Array.isArray(errors)) message = errors.join(' ');
        else if (typeof errors === 'object') message = Object.values(errors).flat().join(' ');
      }
      setError(message);
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingCustomers) {
    return (
      <div className="page-center">
        <Spinner size="lg" />
      </div>
    );
  }

  return (
    <div className="create-invoice-page">
      <div className="page-header">
        <div>
          <h1 className="page-title">{t('createInvoice.title')}</h1>
          <p className="page-subtitle">{t('createInvoice.subtitle')}</p>
        </div>
        <Link to="/admin/invoices">
          <Button variant="secondary">{t('createInvoice.cancel')}</Button>
        </Link>
      </div>

      <Alert type="error" message={error} onClose={() => setError('')} />
      <Alert type="success" message={successMsg} />

      <div className="card create-invoice-card">
        <form onSubmit={handleSubmit}>
          {/* Customer Selection */}
          <div className="form-group">
            <label htmlFor="customerId" className="form-label">
              {t('createInvoice.selectCustomer')} <span className="form-required">*</span>
            </label>
            {customers.length === 0 ? (
              <div className="no-customers-alert">
                {t('createInvoice.noCustomers')}
              </div>
            ) : (
              <select
                id="customerId"
                name="customerId"
                className="form-control"
                value={formData.customerId}
                onChange={handleChange}
                required
              >
                <option value="">{t('createInvoice.chooseCustomer')}</option>
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.fullName} ({c.email})
                  </option>
                ))}
              </select>
            )}
          </div>

          {/* Invoice Number & Generate Helper */}
          <div className="invoice-number-row">
            <div style={{ flex: 1 }}>
              <FormInput
                id="invoiceNumber"
                name="invoiceNumber"
                label={t('createInvoice.invoiceNumber')}
                value={formData.invoiceNumber}
                onChange={handleChange}
                placeholder="e.g. INV-2026-001"
                required
              />
            </div>
            <div style={{ marginBottom: '1rem', alignSelf: 'flex-end' }}>
              <Button
                type="button"
                variant="outline"
                size="md"
                onClick={handleGenerateInvoiceNumber}
              >
                {t('createInvoice.generate')}
              </Button>
            </div>
          </div>

          {/* Amount */}
          <FormInput
            id="amount"
            name="amount"
            label={t('createInvoice.amount')}
            type="number"
            step="0.01"
            min="0.01"
            placeholder="0.00"
            value={formData.amount}
            onChange={handleChange}
            required
          />

          {/* Dates */}
          <div className="grid-2-col">
            <FormInput
              id="issueDate"
              name="issueDate"
              label={t('createInvoice.issueDate')}
              type="date"
              value={formData.issueDate}
              onChange={handleChange}
              required
            />

            <FormInput
              id="dueDate"
              name="dueDate"
              label={t('createInvoice.dueDate')}
              type="date"
              value={formData.dueDate}
              onChange={handleChange}
              required
            />
          </div>

          {/* Description */}
          <div className="form-group">
            <label htmlFor="description" className="form-label">{t('createInvoice.description')}</label>
            <textarea
              id="description"
              name="description"
              rows="3"
              className="form-control"
              placeholder={t('createInvoice.descPlaceholder')}
              value={formData.description}
              onChange={handleChange}
            />
          </div>

          {/* Optional Notes */}
          <div className="form-group">
            <label htmlFor="notes" className="form-label">{t('createInvoice.notes')}</label>
            <textarea
              id="notes"
              name="notes"
              rows="2"
              className="form-control"
              placeholder={t('createInvoice.notesPlaceholder')}
              value={formData.notes}
              onChange={handleChange}
            />
          </div>

          <div className="form-actions-row">
            <Link to="/admin/invoices">
              <Button type="button" variant="secondary" size="lg">
                {t('createInvoice.cancel')}
              </Button>
            </Link>
            <Button
              type="submit"
              variant="primary"
              size="lg"
              loading={submitting}
              disabled={customers.length === 0}
            >
              {t('createInvoice.submitBtn')}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default CreateInvoicePage;
