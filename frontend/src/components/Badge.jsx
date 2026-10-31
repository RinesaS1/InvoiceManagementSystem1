import { useTranslation } from 'react-i18next';
import './Badge.css';

/**
 * Badge for Invoice and Payment statuses.
 *
 * InvoiceStatus:  0 = Unpaid, 1 = Paid
 * PaymentStatus:  0 = Pending, 1 = Confirmed, 2 = Rejected
 */

const INVOICE_STATUS_CLASSES = {
  0: 'badge-unpaid',
  1: 'badge-paid',
};

const PAYMENT_STATUS_CLASSES = {
  0: 'badge-pending',
  1: 'badge-confirmed',
  2: 'badge-rejected',
};

export function InvoiceStatusBadge({ status }) {
  const { t } = useTranslation();

  const className = INVOICE_STATUS_CLASSES[status] || 'badge-unknown';
  let label = t('invoices.status.unknown');
  if (status === 0) label = t('invoices.status.unpaid');
  else if (status === 1) label = t('invoices.status.paid');

  return <span className={`badge ${className}`}>{label}</span>;
}

export function PaymentStatusBadge({ status }) {
  const { t } = useTranslation();

  const className = PAYMENT_STATUS_CLASSES[status] || 'badge-unknown';
  let label = t('invoices.paymentStatus.unknown');
  if (status === 0) label = t('invoices.paymentStatus.pending');
  else if (status === 1) label = t('invoices.paymentStatus.confirmed');
  else if (status === 2) label = t('invoices.paymentStatus.rejected');

  return <span className={`badge ${className}`}>{label}</span>;
}

export default { InvoiceStatusBadge, PaymentStatusBadge };
