function mostrarToast(mensaje) {
    var toast = document.getElementById("toast");
    if (!toast) return;

    toast.innerText = mensaje;
    toast.classList.add("show");

    setTimeout(function () {
        toast.classList.remove("show");
    }, 2600);
}

function abrirModal(id) {
    var modal = document.getElementById(id);
    if (modal) {
        modal.style.display = "flex";
    }
}

function cerrarModal(id) {
    var modal = document.getElementById(id);
    if (modal) {
        modal.style.display = "none";
    }
}

function accionDemo(texto) {
    mostrarToast(texto);
}

document.addEventListener("DOMContentLoaded", function () {
    var headers = document.querySelectorAll(".accordion-header");

    headers.forEach(function (header) {
        header.addEventListener("click", function () {
            var item = this.parentElement;
            item.classList.toggle("active");
        });
    });
});