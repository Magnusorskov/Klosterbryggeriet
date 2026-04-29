window.printPriceList = (suppressFooter = false) => {
    const prev = document.title;
    document.title = '';

    let styleEl = null;
    if (suppressFooter) {
        // Override the global @page @bottom-right footer for this print only.
        // We rewrite the entire @page rule (size + margin + empty bottom-right)
        // because @bottom-right declarations from a later @page cascade with the
        // earlier ones — and emptying `content` is the most reliable way to drop
        // the line across Chrome / Safari / Firefox print engines.
        styleEl = document.createElement('style');
        styleEl.setAttribute('data-print-override', 'fadoel');
        styleEl.textContent =
            '@page { size: A4; margin: 1.5cm; @bottom-right { content: ""; } }';
        document.head.appendChild(styleEl);
    }

    window.print();

    if (styleEl) {
        styleEl.remove();
    }
    document.title = prev;
};
