import api from './api';

const paymentService = {
  /**
   * Get all pending payment submissions (Admin only)
   */
  getPending: async () => {
    const response = await api.get('/api/payments/pending');
    return response.data;
  },

  /**
   * Confirm a payment submission (Admin only)
   */
  confirm: async (paymentId) => {
    const response = await api.patch(`/api/payments/${paymentId}/verify`);
    return response.data;
  },

  /**
   * Reject a payment submission with a reason (Admin only)
   */
  reject: async (paymentId, rejectionReason) => {
    const response = await api.patch(`/api/payments/${paymentId}/reject`, {
      rejectionReason,
    });
    return response.data;
  },
};

export default paymentService;

