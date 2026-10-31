import api from './api';

const authService = {
  /**
   * Register a new Customer account (public)
   */
  register: async (data) => {
    const response = await api.post('/api/auth/register', data);
    return response.data;
  },

  /**
   * Login — returns { token, user: { id, email, fullName, role } }
   */
  login: async (email, password) => {
    const response = await api.post('/api/auth/login', { email, password });
    return response.data;
  },

  /**
   * Get current authenticated user info
   */
  me: async () => {
    const response = await api.get('/api/auth/me');
    return response.data;
  },
};

export default authService;

