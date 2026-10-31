import { useState, useEffect } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context/AuthContext';
import authService from '../services/authService';
import FormInput from '../components/FormInput';
import Button from '../components/Button';
import Alert from '../components/Alert';
import './AuthForm.css';

function LoginPage() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const { login, isAuthenticated, role } = useAuth();
  const navigate = useNavigate();

  // If already authenticated, redirect to appropriate role portal
  useEffect(() => {
    if (isAuthenticated) {
      if (role === 'Admin') {
        navigate('/admin/dashboard', { replace: true });
      } else if (role === 'Customer') {
        navigate('/customer/dashboard', { replace: true });
      }
    }
  }, [isAuthenticated, role, navigate]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.login(email, password);

      // Backend returns ApiResponse<AuthResponseDto>: { success, message, data: { token, userId, email, fullName, role } }
      if (response && response.success && response.data?.token) {
        const { token, userId, email: userEmail, fullName, role: userRole } = response.data;
        login(token, { id: userId, email: userEmail, fullName, role: userRole });

        // Redirect based on role
        if (userRole === 'Admin') {
          navigate('/admin/dashboard', { replace: true });
        } else {
          navigate('/customer/dashboard', { replace: true });
        }
      } else {
        setError(response?.message || t('auth.invalidCredentials'));
      }
    } catch (err) {
      const message =
        err.response?.data?.message ||
        (Array.isArray(err.response?.data?.errors)
          ? err.response.data.errors.join(' ')
          : null) ||
        t('auth.invalidCredentials');
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card card">
        <h2 className="auth-title">{t('auth.signInTitle')}</h2>
        <p className="auth-subtitle">{t('auth.signInSubtitle')}</p>

        <Alert type="error" message={error} onClose={() => setError('')} />

        <form onSubmit={handleSubmit} className="auth-form">
          <FormInput
            id="email"
            label={t('auth.email')}
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder={t('auth.emailPlaceholder')}
            required
            autoComplete="email"
          />

          <FormInput
            id="password"
            label={t('auth.password')}
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder={t('auth.passwordPlaceholder')}
            required
            autoComplete="current-password"
          />

          <Button type="submit" loading={loading} className="auth-submit-btn">
            {t('auth.signInBtn')}
          </Button>
        </form>

        <div className="auth-footer">
          {t('auth.noAccount')} <Link to="/register">{t('auth.regAsCustomer')}</Link>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
