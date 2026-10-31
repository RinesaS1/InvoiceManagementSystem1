import './FormInput.css';

/**
 * Reusable form input with label and error message.
 *
 * Props: label, id, error, type, value, onChange, placeholder, required, ...rest
 */
function FormInput({
  label,
  id,
  error,
  type = 'text',
  value,
  onChange,
  placeholder,
  required = false,
  className = '',
  ...rest
}) {
  return (
    <div className={`form-group ${error ? 'has-error' : ''} ${className}`}>
      {label && (
        <label htmlFor={id} className="form-label">
          {label}
          {required && <span className="form-required" aria-hidden="true"> *</span>}
        </label>
      )}
      <input
        id={id}
        name={id}
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        required={required}
        className="form-control"
        aria-describedby={error ? `${id}-error` : undefined}
        {...rest}
      />
      {error && (
        <p id={`${id}-error`} className="form-error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}

export default FormInput;

