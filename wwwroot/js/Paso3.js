// wwwroot/js/Paso3.js
console.log('⚡ Paso3.js cargado');

document.addEventListener('DOMContentLoaded', () => {

    // ───────────────────────────────────────────────────────────────
    // 1) LÓGICA DE “5-PORQUÉS” (Paso 3 original)
    // ───────────────────────────────────────────────────────────────
    const selectFenomeno = document.getElementById('fenomeno');
    let choicesFenomeno;

    function initChoicesFen() {
        if (choicesFenomeno) choicesFenomeno.destroy();
        choicesFenomeno = new Choices(selectFenomeno, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione un fenómeno --'
        });
    }

    // Validación y submit del formCrearCausaPaso3
    const formCausa = document.getElementById('formCrearCausaPaso3');
    formCausa.addEventListener('submit', e => {
        const rawFen = choicesFenomeno
            ? choicesFenomeno.getValue(true)
            : selectFenomeno.value;
        if (!rawFen) {
            e.preventDefault();
            document.getElementById('modalErrorBody').textContent =
                "Por favor seleccione un fenómeno";
            new bootstrap.Modal(
                document.getElementById('modalError')
            ).show();
            return;
        }
        const isEditing = !!formCausa.querySelector('#IdAnalisis');
        const idCausa = parseInt(rawFen, 10);
        if (!isEditing && analizados.includes(idCausa)) {
            e.preventDefault();
            document.getElementById('modalErrorBody').textContent =
                "El fenómeno seleccionado ya ha sido analizado previamente";
            new bootstrap.Modal(
                document.getElementById('modalError')
            ).show();
            return;
        }
        const quinto = formCausa.querySelector('textarea[name="QuintoPorque"]').value.trim();
        if (!quinto) {
            e.preventDefault();
            document.getElementById('modalErrorBody').textContent =
                "Complete los datos correspondientes al quinto porque";
            new bootstrap.Modal(
                document.getElementById('modalError')
            ).show();
        }
    });

    if (selectFenomeno) initChoicesFen();

    // Botón “+ Agregar”
    document.getElementById('btnNuevaCausa').addEventListener('click', () => {
        formCausa.reset();
        formCausa.querySelector('#IdAnalisis')?.remove();
        initChoicesFen();
        choicesFenomeno.enable();
    });

    // Botón editar
    document.querySelectorAll('.btn-abrir-analisis').forEach(el => {
        el.addEventListener('click', () => {
            const modalEl = document.getElementById('modalCausaPaso3');
            const modal = new bootstrap.Modal(modalEl);

            // precarga Choices
            const idCausa = el.dataset.idcausa;
            const descripcion = el.dataset.descripcion;
            const exists = choicesFenomeno._store.choices.find(c => c.value === idCausa);
            if (!exists) {
                choicesFenomeno.setChoices([{
                    value: idCausa,
                    label: descripcion,
                    selected: true,
                    disabled: false
                }], 'value', 'label', false);
            } else {
                choicesFenomeno.setChoiceByValue(idCausa);
            }
            choicesFenomeno.disable();

            // precarga textos
            formCausa.querySelector('textarea[name="PrimerPorque"]').value = el.dataset.primerporque;
            formCausa.querySelector('textarea[name="SegundoPorque"]').value = el.dataset.segundoporque;
            formCausa.querySelector('textarea[name="TercerPorque"]').value = el.dataset.tercerporque;
            formCausa.querySelector('textarea[name="CuartoPorque"]').value = el.dataset.cuartoporque;
            formCausa.querySelector('textarea[name="QuintoPorque"]').value = el.dataset.quintoporque;

            // precarga radio
            formCausa.querySelector(el.dataset.escausaraiz === '0' ? '#causaSi' : '#causaNo').checked = true;

            // hidden IdAnalisis
            let inputAnalisis = formCausa.querySelector('#IdAnalisis');
            if (!inputAnalisis) {
                formCausa.insertAdjacentHTML('beforeend',
                    '<input type="hidden" id="IdAnalisis" name="IdAnalisis" />'
                );
                inputAnalisis = formCausa.querySelector('#IdAnalisis');
            }
            inputAnalisis.value = el.dataset.idanalisis;

            modal.show();
        });
    });

    // Blur detrás de modales (igual que Paso1)
    document.querySelectorAll('.modal').forEach(modalEl => {
        modalEl.addEventListener('show.bs.modal', () => document.body.classList.add('modal-blur'));
        modalEl.addEventListener('hidden.bs.modal', () => document.body.classList.remove('modal-blur'));
    });

    const modalCausaCont = document.querySelector('#modalCausaPaso3 .modal-content');
    document.getElementById('modalError').addEventListener('show.bs.modal', () => {
        modalCausaCont.classList.add('inner-blur');
    });
    document.getElementById('modalError').addEventListener('hidden.bs.modal', () => {
        modalCausaCont.classList.remove('inner-blur');
    });

    // ───────────────────────────────────────────────────────────────
    // 2) LÓGICA DEL BOTTOM-WRAPPER (copiada de Paso1.js)
    // ───────────────────────────────────────────────────────────────
    const respEl = document.getElementById('Responsable');
    const estadoEl2 = document.getElementById('IdEstado');
    const formPaso3 = document.getElementById('formPaso3');
    const btnGuardar = document.getElementById('btnGuardarPaso3');

    // Inicializar Choices.js en RESPONSABLE y ESTADO
    if (respEl) {
        new Choices(respEl, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }
    if (estadoEl2) {
        new Choices(estadoEl2, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    // Pintar degradado según valor de Estado
    if (estadoEl2) {
        const cont2 = estadoEl2.parentElement; // .choices__inner
        const mapaClases = {
            '2': 'estado-pendiente',
            '3': 'estado-en-progreso',
            '4': 'estado-completado',
            '5': 'estado-terminado'
        };
        function pintarEstado2() {
            Object.values(mapaClases).forEach(c => cont2.classList.remove(c));
            const cls = mapaClases[estadoEl2.value];
            if (cls) cont2.classList.add(cls);
        }
        estadoEl2.addEventListener('change', pintarEstado2);
        pintarEstado2();
    }

    // Validación manual al enviar el formPaso3
    if (formPaso3) {
        formPaso3.addEventListener('submit', e => {
            // Orden de fechas
            const iniEl = document.getElementById('FechaInicio');
            const finEl = document.getElementById('FechaFin');
            const fIni = iniEl?.value;
            const fFin = finEl?.value;
            if (fIni && fFin && new Date(fIni) > new Date(fFin)) {
                e.preventDefault();
                bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('modalErrorFechas')
                ).show();
                iniEl.focus();
                return;
            }

            // Campos obligatorios
            const faltan = [];
            if (!fIni) faltan.push('Fecha de inicio');
            if (!estadoEl2.value) faltan.push('Estado');
            if (!respEl.value) faltan.push('Responsable');

            if (faltan.length) {
                e.preventDefault();
                const modal = document.getElementById('modalFaltanCampos');
                const body = modal.querySelector('.modal-body');
                body.innerHTML = '<p>Por favor completa los siguientes campos:</p><ul>' +
                    faltan.map(f => `<li>${f}</li>`).join('') +
                    '</ul>';
                bootstrap.Modal.getOrCreateInstance(modal).show();

                // enfocar primer faltante
                if (faltan[0] === 'Fecha de inicio') iniEl.focus();
                if (faltan[0] === 'Estado') estadoEl2.focus();
                if (faltan[0] === 'Responsable') respEl.focus();
                return;
            }
        });
    }

    // Modal “permiso denegado” para usuarios sin permiso
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

});

