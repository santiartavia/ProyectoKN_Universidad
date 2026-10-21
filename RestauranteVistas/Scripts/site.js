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
    var toggle = document.querySelector('.navigation-toggle');
    var sidebar = document.querySelector('.sidebar');
    if (toggle && sidebar) toggle.addEventListener('click', function () {
        var open = sidebar.classList.toggle('navigation-open');
        toggle.setAttribute('aria-expanded', String(open));
    });
    document.querySelectorAll('.menu a').forEach(function (link) {
        if (new URL(link.href).pathname.toLowerCase() === location.pathname.toLowerCase())
            link.setAttribute('aria-current', 'page');
    });
    document.querySelectorAll('form').forEach(function (form) {
        if (form.method.toLowerCase() !== 'post') return;
        form.addEventListener('submit', function (event) {
            if (form.dataset.submitting === 'true') { event.preventDefault(); return; }
            if (event.defaultPrevented || !form.checkValidity()) return;
            var action = new URL(form.action).pathname;
            if (/Cancelar|Eliminar|DeleteConfirmed|CerrarCaja|FinalizarOrden/i.test(action) && !window.confirm('¿Confirma esta operación?')) {
                event.preventDefault(); return;
            }
            form.dataset.submitting = 'true';
            form.setAttribute('aria-busy', 'true');
            setTimeout(function () {
                form.querySelectorAll('button[type="submit"], input[type="submit"]').forEach(function (button) { button.disabled = true; });
            }, 0);
        });
    });
    document.querySelectorAll('table').forEach(function (table, index) {
        var wrapper = document.createElement('div');
        wrapper.className = 'table-scroll';
        table.parentNode.insertBefore(wrapper, table);
        wrapper.appendChild(table);
        if (!table.tBodies.length || table.tBodies[0].rows.length < 11) return;
        var rows = Array.from(table.tBodies[0].rows);
        var page = 0, size = 15;
        var tools = document.createElement('div'); tools.className = 'table-tools';
        var input = document.createElement('input'); input.type = 'search'; input.placeholder = 'Buscar en esta tabla…'; input.setAttribute('aria-label', 'Buscar en tabla ' + (index + 1));
        var count = document.createElement('small'); count.setAttribute('aria-live', 'polite');
        tools.append(input, count); wrapper.before(tools);
        var pagination = document.createElement('div'); pagination.className = 'table-pagination';
        var previous = document.createElement('button'), next = document.createElement('button'), label = document.createElement('span');
        previous.type = next.type = 'button'; previous.className = next.className = 'btn btn-soft'; previous.textContent = 'Anterior'; next.textContent = 'Siguiente';
        pagination.append(previous, label, next); wrapper.after(pagination);
        function render() {
            var term = input.value.trim().toLocaleLowerCase('es-CR');
            var filtered = rows.filter(function (row) { return row.textContent.toLocaleLowerCase('es-CR').includes(term); });
            rows.forEach(function (row) { row.hidden = true; });
            filtered.slice(page * size, (page + 1) * size).forEach(function (row) { row.hidden = false; });
            count.textContent = filtered.length + ' registros';
            label.textContent = 'Página ' + (page + 1) + ' de ' + Math.max(1, Math.ceil(filtered.length / size));
            previous.disabled = page === 0; next.disabled = (page + 1) * size >= filtered.length;
        }
        input.addEventListener('input', function () { page = 0; render(); });
        previous.addEventListener('click', function () { page--; render(); });
        next.addEventListener('click', function () { page++; render(); });
        render();
    });
    var headers = document.querySelectorAll(".accordion-header");

    headers.forEach(function (header) {
        header.addEventListener("click", function () {
            var item = this.parentElement;
            item.classList.toggle("active");
        });
    });
});
