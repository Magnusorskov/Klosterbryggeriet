window.printPriceList = () => {
    const prev = document.title;
    document.title = '';
    window.print();
    document.title = prev;
};
