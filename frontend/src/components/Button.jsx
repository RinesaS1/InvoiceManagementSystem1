import './Button.css';

/**
 * Reusable Button component.
 *
 * Props:
 *   variant  — 'primary' | 'secondary' | 'danger' | 'outline' | 'ghost'
 *   size     — 'sm' | 'md' | 'lg'
 *   loading  — boolean
 *   disabled — boolean
 *   type     — 'button' | 'submit' | 'reset'
 *   onClick  — handler
 *   children — button label
 *   className — extra class names
 */
function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  disabled = false,
  type = 'button',
  onClick,
  children,
  className = '',
  ...rest
}) {
  return (
    <button
      type={type}
      className={`btn btn-${variant} btn-${size} ${loading ? 'btn-loading' : ''} ${className}`}
      disabled={disabled || loading}
      onClick={onClick}
      {...rest}
    >
      {loading ? (
        <span className="btn-spinner" aria-hidden="true" />
      ) : null}
      <span className={loading ? 'btn-text-hidden' : ''}>{children}</span>
    </button>
  );
}

export default Button;

