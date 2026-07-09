// wwwroot/js/Paso5.js
console.log('⚡ Paso5.js cargado');

document.addEventListener('DOMContentLoaded', () => {

    // ───────────────────────────────
    // 1) Inicializar Choices.js
    // ───────────────────────────────
    const respEl = document.getElementById('Responsable');
    const estadoEl = document.getElementById('IdEstado');

    let choicesResp = null;
    let choicesEstado = null;

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
    // 3) Validación al enviar formulario
    // ───────────────────────────────
    const formPaso5 = document.getElementById('formPaso5');
    if (formPaso5) {
        formPaso5.addEventListener('submit', e => {
            const iniEl = document.getElementById('FechaInicio');
            const finEl = document.getElementById('FechaFin');

            const fIni = iniEl?.value;
            const fFin = finEl?.value;

            if (fIni && fFin) {
                const dateIni = new Date(`${fIni}T00:00:00`);
                const dateFin = new Date(`${fFin}T00:00:00`);

                if (dateIni > dateFin) {
                    e.preventDefault();
                    bootstrap.Modal.getOrCreateInstance(
                        document.getElementById('modalErrorFechas')
                    ).show();
                    iniEl.focus();
                    return;
                }
            }


            const faltan = [];
            if (!fIni) faltan.push('Fecha de inicio');
            const estadoVal = choicesEstado ? choicesEstado.getValue(true) : estadoEl.value;
            const respVal = choicesResp ? choicesResp.getValue(true) : respEl.value;

            if (!estadoVal) faltan.push('Estado');
            if (!respVal) faltan.push('Responsable');


            if (faltan.length) {
                e.preventDefault();
                const modal = document.getElementById('modalFaltanCampos');
                const body = modal.querySelector('.modal-body');
                body.innerHTML = '<p>Por favor completa los siguientes campos:</p><ul>' +
                    faltan.map(f => `<li>${f}</li>`).join('') +
                    '</ul>';
                bootstrap.Modal.getOrCreateInstance(modal).show();

                // Enfocar primer faltante
                if (faltan[0] === 'Fecha de inicio') iniEl.focus();
                if (faltan[0] === 'Estado') estadoEl.focus();
                if (faltan[0] === 'Responsable') respEl.focus();
            }
        });
    }

    // ───────────────────────────────
    // 4) Modal de “sin permiso”
    // ───────────────────────────────
    const btnGuardar = document.getElementById('btnGuardarPaso5');
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

const fondo = document.getElementById('contenidoFondo');
document.querySelectorAll('.modal').forEach(modalEl => {
    modalEl.addEventListener('show.bs.modal', () => {
        fondo?.classList.add('modal-blur');
    });
    modalEl.addEventListener('hidden.bs.modal', () => {
        fondo?.classList.remove('modal-blur');
    });
});