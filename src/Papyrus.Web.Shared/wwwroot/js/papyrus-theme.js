(() => {
    const root = document.documentElement;
    root.classList.add('js');
    try {
        const storedTheme = window.localStorage.getItem('papyrus-theme');
        const preferredTheme = storedTheme ?? (window.matchMedia?.('(prefers-color-scheme: light)').matches ? 'light' : 'dark');
        root.setAttribute('data-theme', preferredTheme === 'light' ? 'light' : 'dark');
    } catch {
        root.setAttribute('data-theme', 'dark');
    }
})();
