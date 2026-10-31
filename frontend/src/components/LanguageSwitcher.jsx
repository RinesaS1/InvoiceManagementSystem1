import { useTranslation } from 'react-i18next';
import './LanguageSwitcher.css';

function LanguageSwitcher() {
  const { i18n } = useTranslation();

  // Normalize language code to 'en' or 'sq' (e.g. 'en-US' -> 'en')
  const currentLang = (i18n.resolvedLanguage || i18n.language || 'en').startsWith('sq') ? 'sq' : 'en';

  const switchLanguage = (lang) => {
    i18n.changeLanguage(lang);
  };

  return (
    <div className="language-switcher" aria-label="Language selector">
      <button
        type="button"
        className={`lang-btn ${currentLang === 'sq' ? 'active' : ''}`}
        onClick={() => switchLanguage('sq')}
        title="Shqip"
      >
        SQ
      </button>
      <span className="lang-divider">|</span>
      <button
        type="button"
        className={`lang-btn ${currentLang === 'en' ? 'active' : ''}`}
        onClick={() => switchLanguage('en')}
        title="English"
      >
        EN
      </button>
    </div>
  );
}

export default LanguageSwitcher;

