document.addEventListener("DOMContentLoaded", () => {
  // Theme handling
  const savedTheme = localStorage.getItem("theme") || "light";
  document.body.classList.remove("light-mode", "dark-mode");
  document.body.classList.add(`${savedTheme}-mode`);
 
  const themeSelect = document.getElementById("themeSelect");
  if (themeSelect) {
    themeSelect.value = savedTheme;
    themeSelect.addEventListener("change", function () {
      const selectedTheme = this.value;
      document.body.classList.remove("light-mode", "dark-mode");
      document.body.classList.add(`${selectedTheme}-mode`);
      localStorage.setItem("theme", selectedTheme);
    });
  }
 
  // Language handling
  const savedLang = localStorage.getItem("lang") || "English"; // Changed default to "English"
  const langSelect = document.getElementById("langSelect");
 
  // Your list of arrays: [English, Spanish]
  const translations = [
    ["Roku Remote", "Control Remoto de Roku"],
    ["Currently not connected", "No Estas Conectado"],
    ["Settings", "Configuración"],
    ["Select a Theme", "Seleccione un tema"],
    ["Light Mode", "Modo Claro"],
    ["Dark Mode", "Modo Oscuro"],
    ["Select a Language", "Seleccione un idioma"],
    ["English", "Inglés"],
    ["Spanish", "Español"],
    ["Enter Roku IP", "Ingrese la IP de Roku"],
    ["Enter", "Entrar"]
  ];
 
  // Helper: translate text from English to Spanish
  function toSpanish(text) {
    for (const [en, es] of translations) {
      if (text.trim() === en) return es;
    }
    return text; // no match
  }
 
  // Helper: translate text from Spanish back to English
  function toEnglish(text) {
    for (const [en, es] of translations) {
      if (text.trim() === es) return en;
    }
    return text; // no match
  }
 
  // Apply translations to all elements with an id
  function applyLanguage(lang) {
    const allElements = document.querySelectorAll('[id]');
    allElements.forEach(el => {
      if (el.tagName.toLowerCase() === 'img') return; // skip images
 
      // For text in elements
      if (el.tagName.toLowerCase() === 'input') {
        // Translate placeholder
        if (el.placeholder) {
          el.placeholder = (lang === 'Spanish')
            ? toSpanish(el.placeholder)
            : toEnglish(el.placeholder);
        }
      } else if (el.tagName.toLowerCase() === 'option' || el.tagName.toLowerCase() === 'button' || el.tagName.toLowerCase() === 'span' || el.tagName.toLowerCase().startsWith('h')) {
        if (el.textContent.trim().length > 0) {
          el.textContent = (lang === 'Spanish')
            ? toSpanish(el.textContent)
            : toEnglish(el.textContent);
        }
      }
    });
  }
 
  if (langSelect) {
    // Set the language selector to the saved language
    langSelect.value = savedLang;
   
    // Apply the saved language to the page on load
    applyLanguage(savedLang);
 
    langSelect.addEventListener("change", function () {
      const selectedLang = this.value;
      localStorage.setItem("lang", selectedLang);
      applyLanguage(selectedLang);
     
      // Publish event if CrComLib is available
      if (window.CrComLib) {
        window.CrComLib.publishEvent('s', 3, selectedLang);
      }
    });
  } else {
    // If no language selector exists, still apply the saved language
    applyLanguage(savedLang);
  }
});