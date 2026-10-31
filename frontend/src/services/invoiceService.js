import api from './api';

const invoiceService = {
  /**
   * Get all invoices (Admin: all, Customer: own only)
   * Supports ?search= and ?status= query params
   */
  getAll: async ({ search = '', status = '' } = {}) => {
    const params = {};
    if (search) params.search = search;
    if (status !== '' && status !== null && status !== undefined) {
      params.status = status;
    }
    const response = await api.get('/api/invoices', { params });
    return response.data;
  },

  /**
   * Get a single invoice by ID
   */
  getById: async (id) => {
    const response = await api.get(`/api/invoices/${id}`);
    return response.data;
  },

  /**
   * Create a new invoice (Admin only)
   */
  create: async (data) => {
    const response = await api.post('/api/invoices', data);
    return response.data;
  },

  /**
   * Update an invoice (Admin only)
   */
  update: async (id, data) => {
    const response = await api.put(`/api/invoices/${id}`, data);
    return response.data;
  },

  /**
   * Delete an invoice (Admin only)
   */
  delete: async (id) => {
    const response = await api.delete(`/api/invoices/${id}`);
    return response.data;
  },

  /**
   * Download invoice PDF — returns raw Axios response with blob data
   */
  downloadPdf: async (id) => {
    const response = await api.get(`/api/invoices/${id}/pdf`, {
      responseType: 'blob',
    });
    return response;
  },

  /**
   * Helper that downloads the invoice PDF to the user's computer
   */
  savePdfFile: async (id, invoiceNumber) => {
    const res = await invoiceService.downloadPdf(id);
    const blob = new Blob([res.data], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `Fatura-${invoiceNumber}.pdf`);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  /**
   * Customer submits "I Have Paid" for an invoice
   */
  submitPaymentRequest: async (invoiceId) => {
    const response = await api.post(`/api/invoices/${invoiceId}/payment-request`);
    return response.data;
  },
};

export default invoiceService;
