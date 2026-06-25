window.imageInterop = {

    // Скачивание файла по байтам.
    // JS interop здесь обязателен: .NET/WASM не имеет доступа к DOM API,
    // а браузерное скачивание требует создания Blob, object URL и клика по <a download>.
    downloadFile: function (fileName, bytes) {
        const blob = new Blob([new Uint8Array(bytes)], { type: 'image/png' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }

};
