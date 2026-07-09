// wwwroot/js/Paso6.js
console.log('⚡ Paso6.js cargado');

document.addEventListener('DOMContentLoaded', () => {

    // ───────────────────────────────
    // 1) Inicializar Choices.js (main y modal)
    // ───────────────────────────────
    const respEl = document.getElementById('Responsable');
    const estadoEl = document.getElementById('IdEstado');
    const selAcc = document.getElementById('Select_IdTarea');

    let choicesResp = null;
    let choicesEstado = null;
    let choicesAcc = null;

    if (respEl) {
        choicesResp = new Choices(respEl, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    if (estadoEl) {
        choicesEstado = new Choices(estadoEl, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    if (selAcc) {
        choicesAcc = new Choices(selAcc, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccionar --'
        });
    }

    // ───────────────────────────────
    // Función para recargar sólo las acciones PENDIENTES
    // ───────────────────────────────
    async function recargarAcciones() {
        const idFicha = document.getElementById('formVerificacionEfectividad').dataset.idFicha;
        const url = `/FichasTecnicas/${idFicha}/Paso6/ObtenerAccionesDeMejoraPorFicha?idFicha=${idFicha}`;
        const resp = await fetch(url);
        const data = await resp.json();
        // Limpiar y setear nuevas opciones
        choicesAcc.clearChoices();
        choicesAcc.setChoices(
            data.map(x => ({ value: x.Id, label: x.Texto })), // ajustar 'Texto' si el JSON trae otro campo
            'value', 'label', false
        );
    }

    // ───────────────────────────────
    // 2) Colorear degradado según estado
    // ───────────────────────────────
    if (estadoEl) {
        const cont = estadoEl.closest('.combo-field')?.querySelector('.choices__inner');
        const mapaClases = {
            '2': 'estado-pendiente',
            '3': 'estado-en-progreso',
            '4': 'estado-completado',
            '5': 'estado-terminado'
        };

        function pintarEstado() {
            Object.values(mapaClases).forEach(c => cont?.classList.remove(c));
            const clase = mapaClases[estadoEl.value];
            if (clase) cont?.classList.add(clase);
        }

        estadoEl.addEventListener('change', pintarEstado);
        pintarEstado();
    }

    // ───────────────────────────────
    // 3) Validación formulario Paso6 (robusta para Razor)
    // ───────────────────────────────
    const formPaso6 = document.getElementById('formPaso6');
    if (formPaso6) {
        formPaso6.addEventListener('submit', e => {
            // Detecta IDs simples y los generados por Razor (Paso6_*), o bien por name="Paso6.*"
            const iniEl =
                document.getElementById('FechaInicio') ||
                document.getElementById('Paso6_FechaInicio') ||
                document.querySelector('input[name="Paso6.FechaInicio"]');

            const finEl =
                document.getElementById('FechaFin') ||
                document.getElementById('Paso6_FechaFin') ||
                document.querySelector('input[name="Paso6.FechaFin"]');

            const fIni = iniEl?.value;
            const fFin = finEl?.value;

            // Validación fechas (permite igualdad)
            if (fIni && fFin) {
                const dateIni = new Date(`${fIni}T00:00:00`);
                const dateFin = new Date(`${fFin}T00:00:00`);
                if (dateIni > dateFin) {
                    e.preventDefault();
                    const modal = document.getElementById('modalErrorFechas');
                    if (modal) {
                        bootstrap.Modal.getOrCreateInstance(modal).show();
                    } else {
                        alert('La fecha de fin no puede ser menor que la fecha de inicio.');
                    }
                    iniEl?.focus();
                    return;
                }
            }

            // (Opcional) campos obligatorios que ya tenías
            const faltan = [];
            if (!fIni) faltan.push('Fecha de inicio');
            const respEl = document.getElementById('Responsable') || document.getElementById('Paso6_Responsable') || document.querySelector('[name="Paso6.Responsable"]');
            const estadoEl = document.getElementById('IdEstado') || document.getElementById('Paso6_IdEstado') || document.querySelector('[name="Paso6.IdEstado"]');
            const estadoVal = (window.choicesEstado?.getValue?.(true)) ?? estadoEl?.value;
            const respVal = (window.choicesResp?.getValue?.(true)) ?? respEl?.value;
            if (!estadoVal) faltan.push('Estado');
            if (!respVal) faltan.push('Responsable');

            if (faltan.length) {
                e.preventDefault();
                const modal = document.getElementById('modalFaltanCampos');
                if (modal) {
                    modal.querySelector('.modal-body').innerHTML =
                        '<p>Por favor completa los siguientes campos:</p><ul>' +
                        faltan.map(f => `<li>${f}</li>`).join('') + '</ul>';
                    bootstrap.Modal.getOrCreateInstance(modal).show();
                } else {
                    alert('Faltan: ' + faltan.join(', '));
                }
                if (faltan[0] === 'Fecha de inicio') iniEl?.focus();
                else if (faltan[0] === 'Estado') estadoEl?.focus();
                else if (faltan[0] === 'Responsable') respEl?.focus();
            }
        });

        // (UX) Que el datepicker de Fin no permita elegir menor a Inicio
        const iniEl =
            document.getElementById('FechaInicio') ||
            document.getElementById('Paso6_FechaInicio') ||
            document.querySelector('input[name="Paso6.FechaInicio"]');

        const finEl =
            document.getElementById('FechaFin') ||
            document.getElementById('Paso6_FechaFin') ||
            document.querySelector('input[name="Paso6.FechaFin"]');

        function syncMinFin() {
            if (!iniEl || !finEl) return;
            finEl.min = iniEl.value || '';
            if (iniEl.value && finEl.value && finEl.value < iniEl.value) {
                finEl.value = '';
            }
        }
        iniEl?.addEventListener('change', syncMinFin);
        syncMinFin();
    }


    // ───────────────────────────────
    // 4) Modal "sin permiso"
    // ───────────────────────────────
    const btnGuardar = document.getElementById('btnGuardarPaso6');
    if (btnGuardar) {
        btnGuardar.addEventListener('click', e => {
            if (btnGuardar.classList.contains('no-permitido')) {
                e.preventDefault();
                bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('modalPermisoGuardar')
                ).show();
            }
        });
    }

    // ───────────────────────────────
    // 5) Modal Verificación de efectividad (Create vs Edit)
    // ───────────────────────────────
    const formVerif = document.getElementById('formVerificacionEfectividad');
    const hidVer = document.getElementById('inputIdVerificacion');
    const btnNuevo = document.getElementById('btnAgregarVerif');
    const itemsEd = document.querySelectorAll('.editable-verif');
    let wasEditing = false;
    const modalEl = document.getElementById('modalVerificacionEfectividad');

    modalEl.addEventListener('show.bs.modal', async (e) => {
        const trigger = e.relatedTarget;
        wasEditing = trigger.id !== 'btnAgregarVerif';
        formVerif.classList.remove('was-validated');
        const idFicha = formVerif.dataset.idFicha;

        if (!wasEditing) {
            // MODO CREAR
            formVerif.action = `/FichasTecnicas/${idFicha}/Paso6/GuardarVerificacion`;
            hidVer.value = '0';
            selAcc.disabled = false;
            choicesAcc.enable();
            await recargarAcciones();
            choicesAcc.removeActiveItems();
            document.getElementById('efectivaNo').checked = true;
            document.getElementById('inputMetodoConfirmacion').value = '';
            document.getElementById('inputFechaVerificacion').value = '';
        } else {
            // MODO EDITAR
            formVerif.action = `/FichasTecnicas/${idFicha}/Paso6/EditarVerificacion`;
            const item = trigger.closest('.editable-verif');
            hidVer.value = item.dataset.idVerificacion;
            const idTarea = item.dataset.idTarea;
            const label = item.querySelector('.fw-semibold').textContent;
            choicesAcc.setChoices([
                { value: idTarea, label }
            ], 'value', 'label', true);
            choicesAcc.setChoiceByValue(idTarea);
            selAcc.disabled = true;
            choicesAcc.disable();
            const eficaz = item.dataset.efectiva === 'true';
            document.getElementById('efectivaSi').checked = eficaz;
            document.getElementById('efectivaNo').checked = !eficaz;
            document.getElementById('inputMetodoConfirmacion').value = item.dataset.metodo;
            document.getElementById('inputFechaVerificacion').value = item.dataset.fecha;
        }
    });

    modalEl.addEventListener('hidden.bs.modal', () => {
        if (wasEditing) {
            window.location.reload();
        }
    });

    // ───────────────────────────────
    // 6) Validación y submit en modal Verificación
    // ───────────────────────────────
    const btnGrabarVerif = document.getElementById('btnGrabarVerificacion');
    if (btnGrabarVerif && formVerif) {
        btnGrabarVerif.addEventListener('click', e => {
            const faltan = [];
            const valAcc = selAcc.choicesInstance ? selAcc.choicesInstance.getValue(true) : selAcc.value;
            if (!valAcc) faltan.push('Acción de mejora');
            if (!document.querySelector('input[name="EsEfectiva"]:checked')) faltan.push('Efectividad (Sí/No)');
            const metodo = document.getElementById('inputMetodoConfirmacion').value.trim();
            if (!metodo) faltan.push('Método de confirmación');
            const fecha = document.getElementById('inputFechaVerificacion').value;
            if (!fecha) faltan.push('Fecha de verificación');

            if (faltan.length) {
                e.preventDefault();
                const modal = document.getElementById('modalFaltanCampos');
                const body = modal.querySelector('.modal-body');
                body.innerHTML = `<p>Por favor, completá los siguientes campos:</p><ul>${faltan.map(f => `<li>${f}</li>`).join('')}</ul>`;
                bootstrap.Modal.getOrCreateInstance(modal).show();
                const primero = faltan[0];
                if (primero === 'Acción de mejora') selAcc.focus();
                else if (primero === 'Efectividad (Sí/No)') document.querySelector('input[name="EsEfectiva"]').focus();
                else if (primero === 'Método de confirmación') document.getElementById('inputMetodoConfirmacion').focus();
                else document.getElementById('inputFechaVerificacion').focus();
                return;
            }
            formVerif.submit();
        });
    }

    // ───────────────────────────────
    // 7) Corrida de producción (auto-calculaciones)
    // ───────────────────────────────
    const prodEl = document.getElementById('cantidadProducida');
    const okEl = document.getElementById('cantidadOk');
    const noOkEl = document.getElementById('cantidadNoOk');
    const nivelEl = document.getElementById('nivelConformidad');
    const formCorr = document.getElementById('formCorrida');
    const btnSave = document.getElementById('btnGuardarCorrida');
    if (prodEl && okEl && noOkEl && nivelEl) {
        noOkEl.readOnly = true;
        function actualizarCorrida() {
            const prod = parseInt(prodEl.value, 10) || 0;
            const ok = parseInt(okEl.value, 10) || 0;
            const noOk = Math.max(prod - ok, 0);
            noOkEl.value = noOk;
            const pct = prod > 0 ? Math.round((ok / prod) * 100) : 0;
            nivelEl.textContent = pct + '%';
        }
        function actualizarNivel() {
            const pct = prodEl.value ? Math.round((parseInt(okEl.value, 10) / parseInt(prodEl.value, 10)) * 100) : 0;
            nivelEl.textContent = pct + '%';
            nivelEl.classList.toggle('text-success', pct >= 90);
            nivelEl.classList.toggle('text-warning', pct < 90 && pct >= 70);
            nivelEl.classList.toggle('text-danger', pct < 70);
        }
        [prodEl, okEl].forEach(el => el.addEventListener('input', () => { actualizarCorrida(); actualizarNivel(); }));
        actualizarCorrida(); actualizarNivel();
        btnSave.addEventListener('click', () => formCorr.submit());
    }

    // ───────────────────────────────
    // 8) Blur background on modals
    // ───────────────────────────────
    const fondo = document.getElementById('contenidoFondo');
    document.querySelectorAll('.modal').forEach(m => {
        m.addEventListener('show.bs.modal', () => fondo?.classList.add('modal-blur'));
        m.addEventListener('hidden.bs.modal', () => fondo?.classList.remove('modal-blur'));
    });

    // --- 8.b) Confirmación por modal al eliminar verificación ---
    const modalDelVerif = document.getElementById('modalConfirmarEliminarVerif');
    const inputDeleteVerif = document.getElementById('inputDeleteVerif');
    const btnConfirmarEliminar = document.getElementById('btnConfirmarEliminarVerif');

    if (modalDelVerif) {
        // Cuando se abre el modal, guardamos el ID en el input hidden
        modalDelVerif.addEventListener('show.bs.modal', e => {
            const trigger = e.relatedTarget;                     // El tacho que clickeaste
            const idVerif = trigger.getAttribute('data-verif');  // data-verif="123"
            inputDeleteVerif.value = idVerif;
        });
    }

    if (btnConfirmarEliminar) {
        // Al apretar “Eliminar”, enviamos el form
        btnConfirmarEliminar.addEventListener('click', () => {
            document.getElementById('formEliminarVerificacion').submit();
        });
    }

    // --- 8.c) Confirmación por modal al eliminar CORRIDA ---
    const modalDelCorrida = document.getElementById('modalConfirmarEliminarCorrida');
    const inputDeleteCorrida = document.getElementById('inputDeleteCorrida');
    const btnConfirmarEliminarCorrida = document.getElementById('btnConfirmarEliminarCorrida');

    if (modalDelCorrida) {
        modalDelCorrida.addEventListener('show.bs.modal', e => {
            const trigger = e.relatedTarget; // botón tachito que abrío el modal
            const id = trigger?.getAttribute('data-corrida') ?? '';
            console.log('🗑 idCorrida a eliminar:', id);   // ← esto tiene que verse en consola
            if (inputDeleteCorrida) inputDeleteCorrida.value = id;
        });
    }

    if (btnConfirmarEliminarCorrida) {
        btnConfirmarEliminarCorrida.addEventListener('click', () => {
            document.getElementById('formEliminarCorrida')?.submit();
        });
    }


    // ───────────────────────────────
    // 9) Toggle efectividad botón SÍ / NO
    // ───────────────────────────────
    document.querySelectorAll('.toggle-btn')?.forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.toggle-btn').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            document.getElementById('inputEsEfectiva').value = btn.dataset.value;
        });
    });

    // 10) Cargar corrida de producción (pinta la tarjeta y bloquea/desbloquea el botón Agregar)
    async function cargarCorrida() {
        const formVerif = document.getElementById('formVerificacionEfectividad');
        const listaCorrida = document.getElementById('lista-corrida');
        const btnAgregar = document.getElementById('btnAgregarCorrida');
        if (!formVerif || !listaCorrida) return;

        const idFicha = formVerif.dataset.idFicha;

        // Traigo última corrida (sin cache)
        let c = null;
        try {
            const resp = await fetch(`/FichasTecnicas/${idFicha}/Paso6/ObtenerCorrida`, { cache: 'no-store' });
            if (!resp.ok) throw new Error('No hay corrida');
            c = await resp.json();
        } catch { /* c queda null */ }

        // Si no hay corrida, muestro placeholder y habilito Agregar
        if (!c || !c.idProduccion) {
            listaCorrida.innerHTML = `
      <div class="list-group-item text-muted rounded-3 mb-2 px-3 py-2">
        Aún no hay corrida de producción.
      </div>`;
            if (btnAgregar) btnAgregar.disabled = false;
            return;
        }

        // Hay corrida → deshabilito Agregar
        if (btnAgregar) btnAgregar.disabled = true;

        // claves esperadas: idProduccion, fecha (yyyy-MM-dd), producida, ok, noOK
        const idCorrida = c.idProduccion ?? c.IdProduccion ?? '';
        const fecha = c.fecha;
        const producida = c.producida;
        const ok = c.ok;
        const noOK = c.noOK;

        // Tarjeta clickeable (abre modal en modo EDITAR) + tachito (abre modal eliminar)
        listaCorrida.innerHTML = `
    <div class="list-group-item text-white rounded-3 mb-2 px-3 py-2 corrida-item"
         data-idcorrida="${idCorrida}"
         data-fecha="${fecha ?? ''}"
         data-producida="${producida ?? 0}"
         data-ok="${ok ?? 0}"
         data-nook="${noOK ?? 0}"
         style="cursor:pointer">
      <div class="d-flex justify-content-between align-items-center">
        <div class="flex-grow-1">
          <div class="fw-semibold">Fecha: ${fecha ? new Date(fecha).toLocaleDateString() : '-'}</div>
          <div class="d-flex align-items-center gap-3 mt-1">
            <small class="text-muted">Producida: ${producida}</small>
            <small class="text-muted">OK: ${ok}</small>
            <small class="text-muted">No OK: ${noOK}</small>
          </div>
        </div>

        <!-- Tachito: eliminar (abre modal de confirmación) -->
        <button type="button"
                class="btn btn-outline-primary btn-sm ms-3"
                title="Eliminar corrida"
                data-bs-toggle="modal"
                data-bs-target="#modalConfirmarEliminarCorrida"
                data-corrida="${idCorrida}"
                onclick="event.stopPropagation();">
          🗑️
        </button>
      </div>
    </div>
  `;
    }



    cargarCorrida();

    // Click en la tarjeta de corrida => abrir modal con datos (modo EDITAR)
    const listaCorrida = document.getElementById('lista-corrida');
    if (listaCorrida) {
        listaCorrida.addEventListener('click', (e) => {
            const item = e.target.closest('.corrida-item');
            if (!item) return;

            // Completar campos del modal con los data-*
            document.getElementById('idProduccion').value = item.dataset.idcorrida || '';
            document.getElementById('cantidadProducida').value = item.dataset.producida || 0;
            document.getElementById('cantidadOk').value = item.dataset.ok || 0;
            document.getElementById('cantidadNoOk').value = item.dataset.nook || 0;
            document.getElementById('fechaProduccion').value = item.dataset.fecha || '';

            // Actualizar % visual
            const prod = parseInt(document.getElementById('cantidadProducida').value, 10) || 0;
            const ok = parseInt(document.getElementById('cantidadOk').value, 10) || 0;
            const nivelEl = document.getElementById('nivelConformidad');
            const pct = prod > 0 ? Math.round((ok / prod) * 100) : 0;
            nivelEl.textContent = pct + '%';

            // Cambiar action a EDITAR
            const formCorr = document.getElementById('formCorrida');
            const idFicha = document.getElementById('formVerificacionEfectividad').dataset.idFicha;
            formCorr.action = `/FichasTecnicas/${idFicha}/Paso6/EditarCorrida`;

            // Abrir modal
            bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCorrida')).show();
        });
    }

    // Botón "+ Agregar corrida" => limpiar y setear acción GuardarCorrida
    document.querySelector('[data-bs-target="#modalCorrida"]')?.addEventListener('click', () => {
        document.getElementById('idProduccion').value = '';
        document.getElementById('cantidadProducida').value = 0;
        document.getElementById('cantidadOk').value = 0;
        document.getElementById('cantidadNoOk').value = 0;
        document.getElementById('fechaProduccion').value = new Date().toISOString().slice(0, 10);

        const idFicha = document.getElementById('formVerificacionEfectividad').dataset.idFicha;
        document.getElementById('formCorrida').action = `/FichasTecnicas/${idFicha}/Paso6/GuardarCorrida`;
    });

});
