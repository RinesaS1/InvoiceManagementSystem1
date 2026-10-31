import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';

// Pages
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import UnauthorizedPage from './pages/UnauthorizedPage';
import NotFoundPage from './pages/NotFoundPage';

// Admin
import AdminDashboard from './admin/AdminDashboard';
import AdminInvoices from './admin/AdminInvoices';
import CreateInvoicePage from './admin/CreateInvoicePage';
import ManageCustomersPage from './admin/ManageCustomersPage';
import PaymentVerificationPage from './admin/PaymentVerificationPage';

// Customer
import CustomerDashboard from './customer/CustomerDashboard';
import MyInvoicesPage from './customer/MyInvoicesPage';
import InvoiceDetailsPage from './customer/InvoiceDetailsPage';

function HomeRedirect() {
  const { isAuthenticated, role, isLoading } = useAuth();

  if (isLoading) return null;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (role === 'Admin') {
    return <Navigate to="/admin/dashboard" replace />;
  }

  return <Navigate to="/customer/dashboard" replace />;
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<HomeRedirect />} />
            <Route path="login" element={<LoginPage />} />
            <Route path="register" element={<RegisterPage />} />
            <Route path="unauthorized" element={<UnauthorizedPage />} />

            {/* Admin protected routes */}
            <Route
              path="admin/dashboard"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <AdminDashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="admin/invoices"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <AdminInvoices />
                </ProtectedRoute>
              }
            />
            <Route
              path="admin/invoices/create"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <CreateInvoicePage />
                </ProtectedRoute>
              }
            />
            <Route
              path="admin/customers"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <ManageCustomersPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="admin/payments"
              element={
                <ProtectedRoute allowedRoles={['Admin']}>
                  <PaymentVerificationPage />
                </ProtectedRoute>
              }
            />

            {/* Customer protected routes */}
            <Route
              path="customer/dashboard"
              element={
                <ProtectedRoute allowedRoles={['Customer']}>
                  <CustomerDashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="customer/invoices"
              element={
                <ProtectedRoute allowedRoles={['Customer']}>
                  <MyInvoicesPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="customer/invoices/:id"
              element={
                <ProtectedRoute allowedRoles={['Customer']}>
                  <InvoiceDetailsPage />
                </ProtectedRoute>
              }
            />

            {/* Backward compatibility aliases */}
            <Route path="my-invoices" element={<Navigate to="/customer/invoices" replace />} />
            <Route path="my-invoices/:id" element={<Navigate to="/customer/invoices" replace />} />

            {/* Catch-all */}
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
