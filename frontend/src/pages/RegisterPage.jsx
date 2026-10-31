import { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../context/AuthContext';
import authService from '../services/authService';
import FormInput from '../components/FormInput';
import Button from '../components/Button';
import Alert from '../components/Alert';
import './AuthForm.css';

function RegisterPage() {
  const { t } = useTranslation();
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const { isAuthenticated, role } = useAuth();
  const navigate = useNavigate();

  // If already logged in, redirect
  useEffect(() => {
    if (isAuthenticated) {
      if (role === 'Admin') {
        navigate('/admin/dashboard', { replace: true });
      } else if (role === 'Customer') {
        navigate('/customer/dashboard', { replace: true });
      }
    }
  }, [isAuthenticated, role, navigate]);

  const handleChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (formData.password !== formData.confirmPassword) {
      setError(t('auth.passwordsDoNotMatch'));
      return;
    }

    if (formData.password.length < 8) {
      setError(t('auth.passwordRequirements'));
      return;
    }

    setLoading(true);

    try {
      const response = await authService.register({
        fullName: formData.fullName,
        email: formData.email,
        password: formData.password,
        confirmPassword: formData.confirmPassword,
      });

      if (response && response.success) {
        setSuccess(t('auth.regSuccess'));
        setTimeout(() => {
          navigate('/login');
        }, 1500);
      } else {
        setError(response?.message || 'Registration failed.');
      }
    } catch (err) {
      let message = 'Registration failed. Please check your data.';

      if (err.response?.data?.message) {
        message = err.response.data.message;
      } else if (err.response?.data?.errors) {
        const errors = err.response.data.errors;
        if (Array.isArray(errors)) {
          message = errors.join(' ');
        } else if (typeof errors === 'object') {
          // ModelState dictionary: { Email: [...], Password: [...] }
          message = Object.values(errors).flat().join(' ');
        }
      }

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card card">
        <h2 className="auth-title">{t('auth.registerTitle')}</h2>
        <p className="auth-subtitle">{t('auth.registerSubtitle')}</p>

        <Alert type="error" message={error} onClose={() => setError('')} />
        <Alert type="success" message={success} />

        <form onSubmit={handleSubmit} className="auth-form">
          <FormInput
            id="fullName"
            label={t('auth.fullName')}
            value={formData.fullName}
            onChange={handleChange}
            placeholder={t('auth.fullNamePlaceholder')}
            required
            autoComplete="name"
          />

          <FormInput
            id="email"
            label={t('auth.email')}
            type="email"
            value={formData.email}
            onChange={handleChange}
            placeholder={t('auth.emailPlaceholder')}
            required
            autoComplete="email"
          />

          <FormInput
            id="password"
            label={t('auth.password')}
            type="password"
            value={formData.password}
            onChange={handleChange}
            placeholder={t('auth.passwordPlaceholder')}
            required
            autoComplete="new-password"
          />

          <FormInput
            id="confirmPassword"
            label={t('auth.confirmPassword')}
            type="password"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder={t('auth.confirmPasswordPlaceholder')}
            required
            autoComplete="new-password"
          />

          <Button type="submit" loading={loading} className="auth-submit-btn">
            {t('auth.registerBtn')}
          </Button>
        </form>

        <div className="auth-footer">
          {t('auth.haveAccount')} <Link to="/login">{t('navbar.login')}</Link>
        </div>
      </div>
    </div>
  );
}

export default RegisterPage;
