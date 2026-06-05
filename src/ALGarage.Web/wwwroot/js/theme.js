// Tema do ÄLGarage. Dark é o padrão (ADR-0014); preferência do usuário fica em localStorage.
window.algTheme = {
  // Aplica o tema salvo (ou dark) o quanto antes para evitar "flash" de tela clara.
  init() {
    const saved = localStorage.getItem('alg-theme');
    document.documentElement.setAttribute('data-theme', saved || 'dark');
  },
  get() {
    return document.documentElement.getAttribute('data-theme') || 'dark';
  },
  toggle() {
    const next = this.get() === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('alg-theme', next);
    return next;
  }
};
