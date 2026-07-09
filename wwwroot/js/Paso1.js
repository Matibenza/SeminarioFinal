// wwwroot/js/Paso1.js
console.log('⚡ Paso1.js cargado');

document.addEventListener('DOMContentLoaded', () => {


    // Referencias a DOM
    const respEl = document.getElementById('Responsable');
    const estadoEl = document.getElementById('IdEstado');
    const operadorEl = document.getElementById('Operador');

    // Inicializar Choices.js en RESPONSABLE
    if (respEl) {
        new Choices(respEl, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    // Inicializar Choices.js en ESTADO
    if (estadoEl) {
        new Choices(estadoEl, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    if (operadorEl) {
        new Choices(operadorEl, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    // Pintar degradado según valor de Estado
    if (estadoEl) {
        const cont = estadoEl.parentElement; // .choices__inner
        const mapaClases = {
            '2': 'estado-pendiente',
            '3': 'estado-en-progreso',
            '4': 'estado-completado',
            '5': 'estado-terminado'
        };
        function pintarEstado() {
            Object.values(mapaClases).forEach(c => cont.classList.remove(c));
            const cls = mapaClases[estadoEl.value];
            if (cls) cont.classList.add(cls);
        }
        estadoEl.addEventListener('change', pintarEstado);
        pintarEstado();
    }

    // Validación manual al enviar el form
    const form = document.getElementById('formVerificacionInicial');
    if (form) {
        form.addEventListener('submit', e => {
            console.log('🔍 Validando Paso1…');
            const iniEl = document.getElementById('FechaInicio');
            const finEl = document.getElementById('FechaFin');
            const fIni = iniEl?.value;
            const fFin = finEl?.value;

            // Orden de fechas
            if (fIni && fFin) {
                const dIni = new Date(fIni);
                const dFin = new Date(fFin);
                if (dIni > dFin) {
                    e.preventDefault();
                    bootstrap.Modal.getOrCreateInstance(
                        document.getElementById('modalErrorFechas')
                    ).show();
                    iniEl.focus();
                    return;
                }
            }

            // Campos obligatorios
            const faltan = [];
            if (!fIni) faltan.push('Fecha de inicio');
            if (!estadoEl?.value) faltan.push('Estado');
            if (!respEl?.value) faltan.push('Responsable');

            if (faltan.length) {
                e.preventDefault();
                const modal = document.getElementById('modalFaltanCampos');
                const body = modal.querySelector('.modal-body');
                const html = '<p>Por favor completa los siguientes campos:</p><ul>' +
                    faltan.map(f => `<li>${f}</li>`).join('') +
                    '</ul>';
                body.innerHTML = html;
                bootstrap.Modal.getOrCreateInstance(modal).show();

                switch (faltan[0]) {
                    case 'Fecha de inicio': iniEl.focus(); break;
                    case 'Estado': estadoEl.focus(); break;
                    case 'Responsable': respEl.focus(); break;
                }
                return;
            }

            console.log('✔ Validación OK, enviando formulario…');
        });
    }

    // Modal “permiso denegado” para auditores
    const btnGuardar = document.getElementById('btnGuardarPaso1');
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