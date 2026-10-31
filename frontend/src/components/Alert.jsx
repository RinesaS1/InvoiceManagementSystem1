import './Alert.css';

/**
 * Alert component for success / error / warning / info messages.
 *
 * Props:
 *   type    — 'success' | 'error' | 'warning' | 'info'
 *   message — string or null
 *   onClose — optional close handler
 */
function Alert({ type = 'info', message, onClose }) {
  if (!message) return null;

  const icons = {
    success: '✓',
    error: '✕',
    warning: '⚠',
    info: 'ℹ',
  };

  return (
    <div className={`alert alert-${type}`} role="alert">
      <span className="alert-icon">{icons[type]}</span>
      <span className="alert-message">{message}</span>
      {onClose && (
        <button
          className="alert-close"
          onClick={onClose}
          aria-label="Close alert"
          type="button"
        >
          ×
        </button>
      )}
    </div>
  );
}

export default Alert;

