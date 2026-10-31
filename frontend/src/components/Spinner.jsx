import './Spinner.css';

/**
 * Loading spinner.
 * size: 'sm' | 'md' | 'lg'
 */
function Spinner({ size = 'md', className = '' }) {
  return (
    <span
      className={`spinner spinner-${size} ${className}`}
      role="status"
      aria-label="Loading"
    />
  );
}

export default Spinner;

