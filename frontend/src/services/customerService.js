import api from './api';

const customerService = {
  /**
   * Get all registered customers (Admin only)
   */
  getAll: async () => {
    const response = await api.get('/api/customers');
    return response.data;
  },

  /**
   * Get customer details by ID (Admin only)
   */
  getById: async (id) => {
    const response = await api.get(`/api/customers/${id}`);
    return response.data;
  },
};

export default customerService;

