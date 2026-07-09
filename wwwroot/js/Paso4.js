// wwwroot/js/paso4.js
console.log('⚡ paso4.js cargado');

(function () {
    // 1) Drag & drop
    window.allowDrop = ev => ev.preventDefault();
    window.dragTask = ev => ev.dataTransfer.setData("text/plain", ev.target.dataset.id);

    window.dropTask = async (ev, nuevoEstado) => {
        ev.preventDefault();
        const id = ev.dataTransfer.getData("text/plain");
        const card = document.querySelector(`.task-card[data-id="${id}"]`);
        if (card) ev.currentTarget.appendChild(card);
        console.log(`Tarea ${id} movida a ${nuevoEstado}`);

        // Fetch antiforgery token from the main form
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        // Enviar FormData al servidor
        const form = new FormData();
        form.append('__RequestVerificationToken', token);
        form.append('idTarea', id);
        form.append('nuevoEstado', nuevoEstado);

        try {
            const response = await fetch(
                `${window.location.pathname}/ActualizarEstadoTarea`,
                { method: 'POST', body: form }
            );
            if (!response.ok) {
                console.error('Error actualizando estado:', await response.text());
            } else {
                console.log('Estado actualizado en BD');
            }
        } catch (err) {
            console.error('Fetch falló:', err);
        }
    };
})();

document.addEventListener("DOMContentLoaded", () => {
    // 2) Choices.js para selects principales
    const resp4 = document.getElementById("Responsable4");
    const est4 = document.getElementById("IdEstado4");
    if (resp4) new Choices(resp4, { searchEnabled: false, placeholder: true, placeholderValue: '-- Seleccione --', itemSelectText: '' });
    if (est4) new Choices(est4, { searchEnabled: false, placeholder: true, placeholderValue: '-- Seleccione --', itemSelectText: '' });

    // 3) Gradient según estado en el dropdown principal
    const mapaMain = {
        '2': 'estado-pendiente',
        '3': 'estado-en-progreso',
        '4': 'estado-completado',
        '5': 'estado-terminado'
    };
    if (est4) {
        const contMain = est4.closest('.choices__inner');
        const paintMain = () => {
            Object.values(mapaMain).forEach(c => contMain.classList.remove(c));
            const cls = mapaMain[est4.value];
            if (cls) contMain.classList.add(cls);
        };
        est4.addEventListener('change', paintMain);
        paintMain();
    }

    // 4) Inicializar Choices en el modal de nueva tarea
    const initModalChoices = () => {
        document.querySelectorAll('#modalNuevaTarea .js-choice').forEach(sel => {
            if (!sel.choicesInstance) {
                sel.choicesInstance = new Choices(sel, {
                    searchEnabled: false,
                    placeholder: true,
                    placeholderValue: '-- Seleccione --',
                    itemSelectText: ''
                });
            }
        });
        // Pintar selects de Estado, Prioridad y TipoAcción en el modal
        const mappings = [
            { id: 'EstadoTarea', map: mapaMain },
            { id: 'PrioridadTarea', map: { 'Alta': 'priority-alta', 'Media': 'priority-media', 'Baja': 'priority-baja' } },
            { id: 'TipoAccionTarea', map: { 'Correctiva': 'action-correctiva', 'Preventiva': 'action-preventiva' } }
        ];
        mappings.forEach(({ id, map }) => {
            const el = document.getElementById(id);
            if (!el) return;
            const cont = el.closest('.choices__inner');
            const paint = () => {
                Object.values(map).forEach(c => cont.classList.remove(c));
                const key = id === 'EstadoTarea'
                    ? el.value
                    : el.selectedOptions[0]?.text.trim();
                const cls = map[key];
                if (cls) cont.classList.add(cls);
            };
            el.addEventListener('change', paint);
            paint();
        });
    };
    initModalChoices();
    document.getElementById('modalNuevaTarea')?.addEventListener('shown.bs.modal', initModalChoices);

    // 5) Comentarios: click en tarjeta → abrir modal, set hidden, GET historial
    const modalComent = document.getElementById("modalComentariosTarea");
    const listaComentarios = modalComent.querySelector("#listaComentarios");
    const inputTareaId = modalComent.querySelector("#comentarioTareaId");
    const textareaNew = modalComent.querySelector("#nuevoComentario");
    const formComentarios = document.getElementById("formComentarios");

    document.querySelectorAll(".task-card").forEach(card => {
        card.addEventListener("click", async () => {
            const id = card.dataset.id;
            inputTareaId.value = id;
            listaComentarios.innerHTML = '<li class="list-group-item text-center">Cargando…</li>';

            const resp = await fetch(`${window.location.pathname}/GetComentariosTarea?idTarea=${id}`);
            const comments = await resp.json();

            listaComentarios.innerHTML = comments.map(c => `
        <li class="list-group-item">
          <strong>${c.autor}</strong>
          <span class="text-muted small ms-2">${new Date(c.fecha).toLocaleString()}</span>
          <div>${c.texto}</div>
        </li>
      `).join("");

            new bootstrap.Modal(modalComent).show();
        });
    });

    // 6) Submit formComentarios → POST + recarga lista
    formComentarios.addEventListener("submit", async e => {
        e.preventDefault();
        const fd = new FormData(formComentarios);
        const res = await fetch(
                `${window.location.pathname}/AgregarComentario`,
                { method: "POST", body: fd }
              );
        if (!res.ok) {
            console.error("Error guardando comentario:", await res.text());
            return;
        }
        // recarga del listado
        const id = inputTareaId.value;
        const resp2 = await fetch(`${window.location.pathname}/GetComentariosTarea?idTarea=${id}`);
        const comments2 = await resp2.json();
        listaComentarios.innerHTML = comments2.map(c => `
      <li class="list-group-item">
        <strong>${c.autor}</strong>
        <span class="text-muted small ms-2">${new Date(c.fecha).toLocaleString()}</span>
        <div>${c.texto}</div>
      </li>
    `).join("");
        textareaNew.value = "";
    });

    // 7) Validaciones del form principal (bottom wrapper)
    const form4 = document.getElementById('formPaso4');
    form4?.addEventListener("submit", e => {
        const inicioEl = document.getElementById("FechaInicio4");
        const finEl = document.getElementById("FechaFin4");
        const fIni = inicioEl?.value;
        const fFin = finEl?.value;
        const faltan = [];

        // fechas
        if (fIni && fFin && new Date(fIni) > new Date(fFin)) {
            e.preventDefault();
            bootstrap.Modal.getOrCreateInstance(document.getElementById("modalErrorFechas")).show();
            return;
        }
        // campos obligatorios
        if (!fIni) faltan.push("Fecha de inicio");
        if (!resp4.value) faltan.push("Responsable");
        if (!est4.value) faltan.push("Estado");

        if (faltan.length) {
            e.preventDefault();
            const modal = document.getElementById("modalFaltanCampos");
            modal.querySelector(".modal-body").innerHTML =
                `<p>Por favor completa:</p><ul>${faltan.map(f => `<li>${f}</li>`).join("")}</ul>`;
            bootstrap.Modal.getOrCreateInstance(modal).show();
        }
    });


    // 8) Botón “Crear Tarea”
    document.getElementById("btnCrearTarea")?.addEventListener("click", () => {
        new bootstrap.Modal(document.getElementById("modalNuevaTarea")).show();
    });

    // ———————————————————————————————————————————
    // 9) Modificar / Eliminar tareas desde el modal comentarios
    // ———————————————————————————————————————————

    // 9.0) Variables reusadas
    const btnModificar = document.getElementById("btnModificarTarea");
    const modalNuevaTarea = document.getElementById("modalNuevaTarea");
    const btnConfirmDelete = document.getElementById("btnConfirmDelete");

    // 9.1) Modificar Tarea: abre el modal de creación y carga datos
    btnModificar.addEventListener("click", async () => {
        const id = inputTareaId.value;
        const resp = await fetch(`${window.location.pathname}/GetTarea?idTarea=${id}`);
        const tarea = await resp.json();

        const formNew = modalNuevaTarea.querySelector("form");
        formNew.action = formNew.action.replace("CrearTarea", "EditarTarea");

        // hidden IdTarea
        let hid = formNew.querySelector('input[name="IdTarea"]');
        if (!hid) {
            hid = document.createElement("input");
            hid.type = "hidden";
            hid.name = "IdTarea";
            formNew.appendChild(hid);
        }
        hid.value = id;

        //  Pre-cargar todos los campos
        formNew.querySelector('textarea[name="AccionDeMejora"]').value = tarea.accionDeMejora;
        formNew.querySelector('textarea[name="DescripcionDeMejora"]').value = tarea.descripcionMejora;
        formNew.querySelector('input[name="CanalImplementacion"]').value = tarea.canal;
        formNew.querySelector('input[name="LugarAplicacion"]').value = tarea.lugar;
        formNew.querySelector('input[name="FechaObjetivo"]').value = tarea.fechaObjetivo?.split("T")[0] || "";
        formNew.querySelector('input[name="FechaFinal"]').value = tarea.fechaFinal?.split("T")[0] || "";

        // Para todos los selects que usan Choices
        const selects = [
            { name: 'IdCausaPasos', nameValue: tarea.idCausaPasos },
            { name: 'IdResponsable', nameValue: tarea.idResponsable },
            { name: 'IdEstado', nameValue: tarea.idEstado },
            { name: 'IdPrioridad', nameValue: tarea.idPrioridad },
            { name: 'IdTipoAccion', nameValue: tarea.idTipoAccion }

        ];

        selects.forEach(({ name, nameValue }) => {
            const sel = formNew.querySelector(`select[name="${name}"]`);
            if (!sel) return;

            // 1) Actualizo el <select> nativo
            sel.value = nameValue;

            // 2) Si Choices.js está inicializado, sincronizo la UI
            if (sel.choicesInstance) {
                sel.choicesInstance.removeActiveItems();
                sel.choicesInstance.setChoiceByValue(String(nameValue));
            }
        });

        // —————————————————————————————————
        // Traer y renderizar comentarios
        // —————————————————————————————————
        {
            const respCom = await fetch(`${window.location.pathname}/GetComentariosTarea?idTarea=${id}`);
            const comentarios = await respCom.json();

            // 1) Actualizar contador
            const badge = formNew.querySelector('.section-header .badge');
            if (badge) badge.textContent = comentarios.length;

            // 2) Renderizar un pequeño resumen en el carousel
            const carousel = formNew.querySelector('.comments-carousel');
            if (carousel) {
                if (!comentarios.length) {
                    carousel.innerHTML = '<p class="text-muted">Sin comentarios aún.</p>';
                } else {
                    carousel.innerHTML = comentarios
                        .slice(0, 3)
                        .map(c => `
          <div class="comment-summary mb-2">
            <strong>${c.autor}</strong>
            <small class="text-muted">${new Date(c.fecha).toLocaleDateString()}</small>
            <div>${c.texto.length > 100 ? c.texto.slice(0, 100) + '…' : c.texto}</div>
          </div>
        `)
                        .join('') +
                        (comentarios.length > 3
                            ? `<div class="text-center">
               <a href="#" class="small">Ver todos (${comentarios.length})</a>
             </div>`
                            : '');
                }
            }
        }
        // 5) Abrir el modal ya con todo precargado
        new bootstrap.Modal(modalNuevaTarea).show();

    });

    // 9.2) Confirmar eliminación: borra vía fetch y recarga la página
    btnConfirmDelete.addEventListener("click", async () => {
        const id = inputTareaId.value;
        const token = formComentarios.querySelector('input[name="__RequestVerificationToken"]').value;
        const fd = new FormData();
        fd.append("__RequestVerificationToken", token);
        fd.append("idTarea", id);

        const res = await fetch(`${window.location.pathname}/EliminarTarea`, {
            method: "POST",
            body: fd
        });

        if (res.ok) location.reload();
        else console.error("Error eliminando tarea:", await res.text());
    });

    // ———————————————————————————
    // 10) Validar modal Nueva Tarea
    // ———————————————————————————
    const formTarea = document.querySelector("#modalNuevaTarea form");
    const modalFalt = document.getElementById("modalFaltanCamposTarea");

    formTarea.addEventListener("submit", e => {
        e.preventDefault();

        const faltan = [];
        // Chequeo de campos de texto/fecha
        if (!formTarea.querySelector('textarea[name="AccionDeMejora"]').value.trim())
            faltan.push("Acción de Mejora");
        if (!formTarea.querySelector('textarea[name="DescripcionDeMejora"]').value.trim())
            faltan.push("Descripción de la Mejora");
        if (!formTarea.querySelector('input[name="CanalImplementacion"]').value.trim())
            faltan.push("Canal de Implementación");
        if (!formTarea.querySelector('input[name="LugarAplicacion"]').value.trim())
            faltan.push("Lugar de Aplicación");
        if (!formTarea.querySelector('input[name="FechaObjetivo"]').value)
            faltan.push("Fecha objetivo");
        if (!formTarea.querySelector('input[name="FechaFinal"]').value)
            faltan.push("Fecha final");

        // Chequeo de selects
        const selects = [
            { label: "Causa", sel: formTarea.querySelector('select[name="IdCausaPasos"]') },
            { label: "Responsable", sel: formTarea.querySelector('select[name="IdResponsable"]') },
            { label: "Estado", sel: formTarea.querySelector('select[name="IdEstado"]') },
            { label: "Prioridad", sel: formTarea.querySelector('select[name="IdPrioridad"]') },
            { label: "Tipo de acción", sel: formTarea.querySelector('select[name="IdTipoAccion"]') }
        ];
        selects.forEach(({ label, sel }) => {
            if (sel && !sel.value) faltan.push(label);
        });

        if (faltan.length) {
            // Inyecto contenido y muestro modal de faltantes
            modalFalt.querySelector(".modal-body").innerHTML =
                `<p>Por favor completá los siguientes campos:</p>
       <ul>${faltan.map(f => `<li>${f}</li>`).join("")}</ul>`;
            new bootstrap.Modal(modalFalt).show();
            return;
        }

        // Si todo ok, envío el form
        formTarea.submit();
    });

    // ———————————————————————————————
    // 11) Filtrar tarjetas por prioridad/persona
    // ———————————————————————————————
    const filtroPri = document.getElementById('filtroPrioridad');
    const filtroPers = document.getElementById('filtroPersonas');

    function aplicarFiltros() {
        const valPri = filtroPri.value;
        const valPers = filtroPers.value;
        document.querySelectorAll('.task-card').forEach(card => {
            const okPri = !valPri || card.dataset.priority === valPri;
            const okPers = !valPers || card.dataset.responsible === valPers;
            card.style.display = (okPri && okPers) ? '' : 'none';
        });
    }

    filtroPri.addEventListener('change', aplicarFiltros);
    filtroPers.addEventListener('change', aplicarFiltros);


});