// wwwroot/js/Tablero.js

console.log('⚡ Tablero.js cargado');

(function () {
    // ─── 1) Drag & drop ────────────────────────────────────
    window.allowDrop = ev => ev.preventDefault();
    window.dragTask = ev => ev.dataTransfer.setData("text/plain", ev.target.dataset.id);

    window.dropTask = async (ev, nuevoEstado) => {
        ev.preventDefault();
        const id = ev.dataTransfer.getData("text/plain");
        const card = document.querySelector(`.task-card[data-id="${id}"]`);
        if (card) ev.currentTarget.appendChild(card);
        console.log(`Tarea ${id} movida a ${nuevoEstado}`);

        // 1.a) Busca el token antiforgery
        const tokenEl = document.querySelector('#modalNuevaTarea input[name="__RequestVerificationToken"]')
            || document.querySelector('#formComentarios input[name="__RequestVerificationToken"]');
        const formData = new FormData();
        if (tokenEl) formData.append('__RequestVerificationToken', tokenEl.value);
        formData.append('idTarea', id);
        formData.append('nuevoEstado', nuevoEstado);

        try {
            const resp = await fetch('/Tablero/ActualizarEstadoTarea', {
                method: 'POST',
                credentials: 'same-origin',
                body: formData
            });
            if (!resp.ok) {
                console.error('Error actualizando estado:', await resp.text());
            } else {
                console.log('Estado actualizado en BD');
            }
        } catch (err) {
            console.error('Fetch falló:', err);
        }
    };
})();

document.addEventListener("DOMContentLoaded", () => {
    // ─── 2) Inicializar Choices.js ──────────────────────────
    function initModalChoices() {
        document.querySelectorAll('#modalNuevaTarea .js-choice').forEach(sel => {
            // 1) Si ya había un choicesInstance, la destruyo
            if (sel.choicesInstance) {
                sel.choicesInstance.destroy();
            }
            // 2) Creo una nueva con autoRemoveItems
            sel.choicesInstance = new Choices(sel, {
                searchEnabled: false,
                placeholder: true,
                placeholderValue: sel.dataset.placeholder || '-- Seleccione --',
                removeItemButton: true,   // muestra la “X”
                maxItemCount: 2,          // un solo tag
                autoRemoveItems: true,    // al elegir uno nuevo borra el anterior
                position: 'bottom',
                shouldSort: false,
                itemSelectText: ''
            });
        });
    }

    function initChoices() {
        document.querySelectorAll('.js-choice').forEach(el => {
            if (!el.choicesInstance) {
                const inst = new Choices(el, {
                    searchEnabled: false,
                    removeItemButton: false,    // seguimos sin “X”
                    removeItems: true,          // al elegir una opción nueva borra la anterior
                    maxItemCount: 2,
                    placeholder: true,
                    placeholderValue: el.dataset.placeholder || '-- Seleccione --',
                    position: 'bottom',
                    shouldSort: false,
                    itemSelectText: ''
                });
                el.choicesInstance = inst;
            }
        });
    }

    initChoices();

    // ─── 2) Aquí, justo a continuación, agrega este código ──
    const selFichas = document.getElementById('Select_IdFichaTecnica');
    const txtCliente = document.getElementById('txtCliente');
    const txtIdPieza = document.getElementById('modalIdPieza') || document.getElementById('txtIdPieza');
    const selCausa = document.getElementById('Select_IdCausa');

    selFichas?.addEventListener('change', async () => {
        const opt = selFichas.selectedOptions[0];
        const idFicha = selFichas.value;
        const cliente = opt.dataset.cliente || '';
        const idpieza = opt.dataset.idpieza || '';

        // 1) Actualizo cliente y pieza
        if (txtCliente) txtCliente.value = cliente;
        if (txtIdPieza) txtIdPieza.value = idpieza;

        // 2) Cargo causas para esa ficha
        if (!idFicha) {
            selCausa.choicesInstance.clearChoices();
            return;
        }
        const resp = await fetch(`/Tablero/Get5PorquesPorFicha?idFicha=${idFicha}`);
        if (!resp.ok) {
            console.error('Error cargando 5-Porqués:', resp.status);
            return;
        }
        const lista = await resp.json();
        const ch = selCausa.choicesInstance;
        ch.clearChoices();
        ch.setChoices(lista, 'value', 'text', true);

    });

    // Re-init al abrir modal Nueva Tarea
    const modalNueva = document.getElementById('modalNuevaTarea');
    modalNueva?.addEventListener('shown.bs.modal', initModalChoices);

    // ─── 3) Comentarios: abrir modal con historial ─────────
    const modalComent = document.getElementById("modalComentariosTarea");
    const listaComentarios = modalComent?.querySelector("#listaComentarios");
    const inputTareaId = modalComent?.querySelector("#comentarioTareaId");
    const textareaNew = modalComent?.querySelector("#nuevoComentario");
    const formComentarios = document.getElementById("formComentarios");

    document.querySelectorAll(".task-card").forEach(card => {
        card.addEventListener("click", async () => {
            const id = card.dataset.id;
            if (!inputTareaId || !listaComentarios) return;

            inputTareaId.value = id;
            listaComentarios.innerHTML = '<li class="list-group-item text-center">Cargando…</li>';

            const resp = await fetch(`/Tablero/GetComentariosTarea?idTarea=${id}`);
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

    // ─── 4) Enviar nuevo comentario y recargar ───────────────
    formComentarios?.addEventListener("submit", async e => {
        e.preventDefault();
        const fd = new FormData(formComentarios);
        const res = await fetch("/Tablero/AgregarComentario", { method: "POST", body: fd });
        if (!res.ok) {
            console.error("Error guardando comentario:", await res.text());
            return;
        }
        // recarga comentarios
        const id = inputTareaId?.value;
        const resp2 = await fetch(`/Tablero/GetComentariosTarea?idTarea=${id}`);
        const comments2 = await resp2.json();
        if (listaComentarios) {
            listaComentarios.innerHTML = comments2.map(c => `
        <li class="list-group-item">
          <strong>${c.autor}</strong>
          <span class="text-muted small ms-2">${new Date(c.fecha).toLocaleString()}</span>
          <div>${c.texto}</div>
        </li>
      `).join("");
        }
        if (textareaNew) textareaNew.value = "";
    });

    const btnCrear = document.getElementById("btnCrearTarea");


    btnCrear?.addEventListener("click", () => {
        const form = modalNueva.querySelector("form");

        // 🔄 0) Asegurar que el action es CrearTarea
        form.action = form.action.replace("EditarTarea", "CrearTarea");
        // (o directamente: form.action = '/Tablero/CrearTarea';)

        // 1) reset de todos los inputs / textareas
        form.reset();

        // 2) quitar hidden de edición si existiera
        const hid = form.querySelector('input[name="IdTarea"]');
        if (hid) hid.remove();

        // 2bis) quitar hidden de ficha técnica si existiera
        const hidFicha = form.querySelector('input[name="IdFichaTecnica"]');
        if (hidFicha) hidFicha.remove();

        // 2ter) re-habilitar el select de ficha y su instancia Choices
        const selFicha = form.querySelector('#Select_IdFichaTecnica');
        if (selFicha) {
            selFicha.disabled = false;
            selFicha.choicesInstance.enable();
        }

        // 3) limpiar todos los selects de Choices.js
        form.querySelectorAll('.js-choice').forEach(el => {
            if (el.choicesInstance) {
                el.choicesInstance.removeActiveItems();
            }
        });

        // 4) finalmente muestro
        new bootstrap.Modal(modalNueva).show();
    });


    // ─── 6) Modificar Tarea ─────────────────────────────────
    document.getElementById("btnModificarTarea")
        ?.addEventListener("click", async () => {
            const id = inputTareaId?.value;
            if (!id) return;

            // 1) Traer datos de la tarea
            const resp = await fetch(`/Tablero/GetTarea?idTarea=${id}`);
            if (!resp.ok) { console.error(resp.statusText); return; }
            const tarea = await resp.json();

            // 2) Preparar formulario
            const form = modalNueva.querySelector("form");
            form.action = form.action.replace("CrearTarea", "EditarTarea");

            // hidden IdTarea
            let hidT = form.querySelector('input[name="IdTarea"]');
            if (!hidT) {
                hidT = document.createElement("input");
                hidT.type = "hidden";
                hidT.name = "IdTarea";
                form.appendChild(hidT);
            }
            hidT.value = tarea.idTarea;

            // ── A) FICHA TÉCNICA & CLIENTE & PIEZA ─────────────
            const selFicha = form.querySelector("#Select_IdFichaTecnica");
            // set native select
            selFicha.value = tarea.idFichaTecnica;
            // Choices.js
            const chFicha = selFicha.choicesInstance;
            chFicha.removeActiveItems();
            chFicha.setChoiceByValue(String(tarea.idFichaTecnica));
           // chFicha.hideDropdown();
           // chFicha.disable();
           // selFicha.disabled = true;
            // hidden para que viaje al servidor
            let hidF = form.querySelector('input[name="IdFichaTecnica"]');
            if (!hidF) {
                hidF = document.createElement("input");
                hidF.type = "hidden";
                hidF.name = "IdFichaTecnica";
                form.appendChild(hidF);
            }
            hidF.value = tarea.idFichaTecnica;
            // cliente e idPieza (asegurate de que los IDs coincidan con tu HTML)
            form.querySelector("#txtCliente").value = tarea.cliente;
            form.querySelector("#modalIdPieza").value = tarea.idPieza;

            // ── B) CAUSAS (5-Porqués) ────────────────────────
            //  B.1) Traer lista de causas para esta ficha
            const respC = await fetch(`/Tablero/Get5PorquesPorFicha?idFicha=${tarea.idFichaTecnica}`);
            const listaC = await respC.json();
            const selCausa = form.querySelector("#Select_IdCausa");
            const chCausa = selCausa.choicesInstance;
            chCausa.clearChoices();
            chCausa.setChoices(listaC, "value", "text", true);
            //  B.2) setear el valor y bloquear si querés
            chCausa.setChoiceByValue(String(tarea.idCausaPasos));
            // chCausa.disable(); selCausa.disabled = true; // opcional

            // ── C) CAMPOS DE TEXTO Y FECHA ────────────────────
            form.querySelector('textarea[name="AccionDeMejora"]').value = tarea.accionDeMejora;
            form.querySelector('textarea[name="DescripcionDeMejora"]').value = tarea.descripcionMejora;
            form.querySelector('input[name="CanalImplementacion"]').value = tarea.canal;
            form.querySelector('input[name="LugarAplicacion"]').value = tarea.lugar;
            form.querySelector('input[name="FechaObjetivo"]').value = tarea.fechaObjetivo?.split("T")[0] || "";
            form.querySelector('input[name="FechaFinal"]').value = tarea.fechaFinal?.split("T")[0] || "";

            // ── D) OTROS SELECTS (Responsable, Estado, Prioridad, TipoAcción) ──
            [
                { name: "IdResponsable", nameValue: tarea.idResponsable },
                { name: "IdEstado", nameValue: tarea.idEstado },
                { name: "IdPrioridad", nameValue: tarea.idPrioridad },
                { name: "IdTipoAccion", nameValue: tarea.idTipoAccion }
            ].forEach(({ name, nameValue }) => {
                const sel = form.querySelector(`select[name="${name}"]`);
                if (!sel?.choicesInstance) return;
                sel.choicesInstance.removeActiveItems();
                // sincronizar valor nativo
                sel.value = nameValue;
                sel.choicesInstance.setChoiceByValue(String(nameValue));
                // opcional: sel.choicesInstance.disable(); sel.disabled = true;
            });

            // ── E) Mostrar modal ───────────────────────────────
            new bootstrap.Modal(modalNueva).show();
        });

    modalNueva.addEventListener('shown.bs.modal', () => {
        // 1) (Re)inicializo todos los selects del modal
        initModalChoices();

        // 2) Pintar colores de Estado, Prioridad y Tipo de Acción
        const mappings = [
            {
                id: 'EstadoTarea',
                map: {
                    '2': 'estado-pendiente',
                    '3': 'estado-en-progreso',
                    '4': 'estado-completado',
                    '5': 'estado-terminado'
                }
            },
            {
                id: 'PrioridadTarea',
                map: {
                    'Alta': 'priority-alta',
                    'Media': 'priority-media',
                    'Baja': 'priority-baja'
                }
            },
            {
                id: 'TipoAccionTarea',
                map: {
                    'Correctiva': 'action-correctiva',
                    'Preventiva': 'action-preventiva'
                }
            }
        ];

        mappings.forEach(({ id, map }) => {
            const el = document.getElementById(id);
            if (!el) return;
            const cont = el.closest('.choices__inner');
            const paint = () => {
                Object.values(map).forEach(c => cont.classList.remove(c));
                const key = id === 'EstadoTarea' ? el.value : el.selectedOptions[0]?.text.trim();
                const cls = map[key];
                if (cls) cont.classList.add(cls);
            };
            el.addEventListener('change', paint);
            paint();
        });


        // 2) Precargo los valores que ya cargaste en tu click de Modificar
        //    y sincronizo Choices con setChoiceByValue
        modalNueva.querySelectorAll('select.js-choice').forEach(sel => {
            const val = sel.value;
            if (val && sel.choicesInstance) {
                sel.choicesInstance.removeActiveItems();
                sel.choicesInstance.setChoiceByValue(String(val));
            }
        });
    });



     // ─── 6.5) Preparar id para el modal de confirmación ─────────────────

         document.getElementById("btnShowDeleteTarea")
           ?.addEventListener("click", () => {
                 // cuando abra el modal de “Confirmar eliminación” volcamos el id
                     const tareaId = inputTareaId.value;
                 document.getElementById("deleteTareaId").value = tareaId;
               });
    // ─── 7) Confirmar eliminación ────────────────────────────
    document.getElementById("btnConfirmDelete")
        ?.addEventListener("click", async () => {
            const id = document.getElementById("deleteTareaId")?.value;
            const token = document.querySelector('#formComentarios input[name="__RequestVerificationToken"]')?.value || '';
            if (!id) return;
            const fd = new FormData();
            if (token) fd.append("__RequestVerificationToken", token);
            fd.append("idTarea", id);

            const res = await fetch("/Tablero/EliminarTarea", { method: "POST", body: fd });
            if (res.ok) location.reload();
            else console.error("Error eliminando tarea:", await res.text());
        });

    // ─── 9) Validar modal Nueva Tarea ───────────────────────
    const formTarea = document.querySelector("#modalNuevaTarea form");
    const modalFalt = document.getElementById("modalFaltanCamposTarea");
    formTarea?.addEventListener("submit", e => {
        e.preventDefault();
        const faltan = [];

        // validar campos de texto/fecha
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

        // selects obligatorios
        [
            { label: "Causa", sel: formTarea.querySelector('select[name="IdCausaPasos"]') },
            { label: "Responsable", sel: formTarea.querySelector('select[name="IdResponsable"]') },
            { label: "Estado", sel: formTarea.querySelector('select[name="IdEstado"]') },
            { label: "Prioridad", sel: formTarea.querySelector('select[name="IdPrioridad"]') },
            { label: "Tipo de acción", sel: formTarea.querySelector('select[name="IdTipoAccion"]') }
        ].forEach(({ label, sel }) => {
            if (sel && !sel.value) faltan.push(label);
        });

        if (faltan.length) {
            modalFalt.querySelector(".modal-body").innerHTML =
                `<p>Por favor completá los siguientes campos:</p>
         <ul>${faltan.map(f => `<li>${f}</li>`).join("")}</ul>`;
            new bootstrap.Modal(modalFalt).show();
            return;
        }

        // todo OK → submit
        formTarea.submit();
    });
});

