(() => {
    const storageKey = 'papyrus-theme';
    const root = document.documentElement;

    const toggle = document.querySelector('[data-theme-toggle]');
    const icon = document.querySelector('[data-theme-icon]');
    const label = document.querySelector('[data-theme-label]');
    const navToggle = document.querySelector('[data-nav-toggle]');
    const mobileNav = document.querySelector('[data-mobile-nav]');

    const icons = {
        light: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><path d="M12 1v2m7.73 3.27-1.42 1.42M20 12h-2m-.27 7.73-1.42-1.42M12 21v-2m-7.73-3.27 1.42-1.42M4 12H2m3.27-7.73 1.42 1.42"/></svg>',
        dark: '<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 2a7 7 0 0 0 0 20"/></svg>'
    };

    const applyTheme = (theme, animate = true) => {
        root.setAttribute('data-theme', theme);
        if (animate) root.setAttribute('data-theme-transitioning', 'true');
        if (toggle) {
            toggle.setAttribute('aria-pressed', theme === 'light' ? 'true' : 'false');
            toggle.setAttribute('aria-label', theme === 'light' ? 'Switch to dark theme' : 'Switch to light theme');
        }
        if (label) {
            label.textContent = theme === 'light' ? 'Switch to dark theme' : 'Switch to light theme';
        }
        if (icon) {
            icon.innerHTML = theme === 'light' ? icons.light : icons.dark;
        }
        if (animate) setTimeout(() => root.removeAttribute('data-theme-transitioning'), 300);
    };

    const storedTheme = window.localStorage.getItem(storageKey);
    const preferredTheme = storedTheme ?? (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark');
    applyTheme(preferredTheme === 'light' ? 'light' : 'dark', false);

    toggle?.addEventListener('click', () => {
        const nextTheme = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        window.localStorage.setItem(storageKey, nextTheme);
        applyTheme(nextTheme);
    });

    const closeMobileNav = () => {
        mobileNav?.classList.remove('is-open');
        navToggle?.setAttribute('aria-expanded', 'false');
        navToggle?.setAttribute('aria-label', 'Open navigation menu');
    };

    navToggle?.addEventListener('click', () => {
        const isOpen = mobileNav?.classList.toggle('is-open') ?? false;
        navToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        navToggle.setAttribute('aria-label', isOpen ? 'Close navigation menu' : 'Open navigation menu');
    });
    mobileNav?.querySelectorAll('a').forEach((link) => link.addEventListener('click', closeMobileNav));
    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') closeMobileNav();
    });
    document.addEventListener('click', (event) => {
        if (mobileNav?.classList.contains('is-open') && !event.target.closest('.site-nav')) closeMobileNav();
    });

    const revealTargets = document.querySelectorAll('.reveal-on-scroll');
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)').matches || !('IntersectionObserver' in window)) {
        revealTargets.forEach((target) => target.classList.add('is-visible'));
    } else if (revealTargets.length > 0) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });

        revealTargets.forEach((target) => observer.observe(target));
    }

    const form = document.querySelector('form.filter-bar');
    if (form) {
        const input = form.querySelector('input[name="q"]');
        let debounceTimer;
        input?.addEventListener('input', () => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => form.submit(), 500);
        });
    }

    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener('click', (e) => {
            e.preventDefault();
            const target = document.querySelector(anchor.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        });
    });

    const progress = document.querySelector('[data-reading-progress]');
    const progressBar = document.querySelector('[data-reading-progress-bar]');
    const article = document.querySelector('.article-content__prose');
    const updateReadingProgress = () => {
        if (!progress || !progressBar || !article) return;
        const range = article.offsetHeight - window.innerHeight;
        const percent = range <= 0 ? 100 : Math.min(100, Math.max(0, ((window.scrollY - article.offsetTop) / range) * 100));
        progressBar.style.width = `${percent}%`;
        progress.setAttribute('aria-valuenow', Math.round(percent).toString());
    };
    if (progress) {
        window.addEventListener('scroll', updateReadingProgress, { passive: true });
        window.addEventListener('resize', updateReadingProgress);
        updateReadingProgress();
    }
})();
