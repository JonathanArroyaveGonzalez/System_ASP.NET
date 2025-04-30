// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Funcionalidad básica
$(document).ready(function () {
    // Inicializar tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Cerrar automáticamente alerts después de 5 segundos
    setTimeout(function () {
        $('.alert').alert('close');
    }, 5000);
});