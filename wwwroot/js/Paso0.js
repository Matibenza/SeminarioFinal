// wwwroot/js/Paso0.js
console.log('⚡ Paso0.js cargado');

document.addEventListener('DOMContentLoaded', () => {
    console.log('✅ DOM listo en Paso0.js');

    // Referencias a DOM
    const respSelect = document.getElementById('Responsable');
    const estadoSelect = document.getElementById('IdEstado');
    const fechaIni = document.getElementById('FechaInicio');
    const fechaFin = document.getElementById('FechaFin');
    const form = document.getElementById('paso0Form');

    // — 1) Inicializar Choices.js en RESPONSABLE y ESTADO —
    if (respSelect) {
        new Choices(respSelect, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });
    }

    if (estadoSelect) {
        new Choices(estadoSelect, {
            searchEnabled: false,
            itemSelectText: '',
            shouldSort: false,
            placeholder: true,
            placeholderValue: '-- Seleccione --'
        });

        // — 1.b) Pintar degradado del select de Estado —
        const estadoCont = estadoSelect.parentElement;
        const mapaClases = {
            '2': 'estado-pendiente',
            '3': 'estado-en-progreso',
            '4': 'estado-completado',
            '5': 'estado-terminado'
        };
        function pintarEstado() {
            Object.values(mapaClases).forEach(c => estadoCont.classList.remove(c));
            const clase = mapaClases[estadoSelect.value];
            if (clase) estadoCont.classList.add(clase);
        }
        estadoSelect.addEventListener('change', pintarEstado);
        pintarEstado();
    }

    // — 2) Validación manual al enviar el form —
    if (form) {
        form.addEventListener('submit', e => {
            console.log('🔍 Validando campos obligatorios Paso0…');

            // 2.a) Orden de fechas
            if (fechaIni?.value && fechaFin?.value) {
                const dIni = new Date(fechaIni.value);
                const dFin = new Date(fechaFin.value);
                if (dIni > dFin) {
                    e.preventDefault();
                    bootstrap.Modal.getOrCreateInstance(
                        document.getElementById('modalErrorFechas')
                    ).show();
                    fechaIni.focus();
                    return;
                }
            }

            // 2.b) Campos obligatorios
            const faltan = [];
            if (!fechaIni?.value) faltan.push('Fecha de inicio');
            if (estadoSelect && !estadoSelect.value) faltan.push('Estado');
            if (respSelect && !respSelect.value) faltan.push('Responsable');

            if (faltan.length) {
                e.preventDefault();
                // Inyectar lista en el modal de campos faltantes
                const modal = document.getElementById('modalFaltanCampos');
                const body = modal.querySelector('.modal-body');
                body.innerHTML = '<p>Por favor completa los siguientes campos:</p><ul>' +
                    faltan.map(f => `<li>${f}</li>`).join('') +
                    '</ul>';
                bootstrap.Modal.getOrCreateInstance(modal).show();

                // Enfocar el primer faltante
                switch (faltan[0]) {
                    case 'Fecha de inicio': fechaIni.focus(); break;
                    case 'Estado': estadoSelect.focus(); break;
                    case 'Responsable': respSelect.focus(); break;
                }
                return;
            }

            console.log('✔ Validación Paso0 OK, enviando…');
        });
    }

    // — 3) Modal de advertencia en “Guardar” si no tienes permiso —
    const btnGuardar = document.getElementById('btnGuardarPaso0');
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