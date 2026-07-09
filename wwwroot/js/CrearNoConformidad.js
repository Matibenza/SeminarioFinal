document.addEventListener('DOMContentLoaded', function () {
    window.currentNoConfId = document.getElementById('currentNoConfId')?.value;

      // Si no estamos en el formulario de NoConformidad, no hacemos nada
          if (!document.getElementById('formCrearNoConformidad')) return;
    // Inicialización de los dropdowns que no requieren estilos condicionales
    new Choices('#selectCliente', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    const piezaChoices = new Choices('#selectPieza', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    new Choices('#selectProceso', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    new Choices('#selectDetectabilidad', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    new Choices('#selectDefecto', {
        shouldSort: false,
        searchEnabled: false,
        removeItemButton: true,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    const selectCausa = document.getElementById('selectCausa');
    if (selectCausa && selectCausa.classList.contains('choices__input')) {
        const parent = selectCausa.closest('.choices');
        parent && parent.remove(); // eliminar el contenedor viejo si está
    }
    if (selectCausa && selectCausa.choices) {
        selectCausa.choices.destroy(); // destruir instancia previa si existe
    }
    new Choices('#selectCausa', {
        shouldSort: false,
        searchEnabled: false,
        removeItemButton: true,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });



    const prioridadChoices = new Choices('#selectPrioridad', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    prioridadChoices.containerOuter.element.classList.add('prioridad-choices');
    prioridadChoices.passedElement.element.addEventListener('change', function () {
        const selectedText = this.options[this.selectedIndex]?.text.toLowerCase().trim();
        prioridadChoices.containerOuter.element.classList.remove('prioridad-alta', 'prioridad-media', 'prioridad-baja');
        if (selectedText === "alta") {
            prioridadChoices.containerOuter.element.classList.add('prioridad-alta');
        } else if (selectedText === "media") {
            prioridadChoices.containerOuter.element.classList.add('prioridad-media');
        } else if (selectedText === "baja") {
            prioridadChoices.containerOuter.element.classList.add('prioridad-baja');
        }
    });

    const estadoChoices = new Choices('#selectEstado', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    estadoChoices.containerOuter.element.classList.add('estado-choices');
    estadoChoices.passedElement.element.addEventListener('change', function () {
        if (this.value && this.value.trim() !== "") {
            estadoChoices.containerOuter.element.classList.add('estado-has-value');
        } else {
            estadoChoices.containerOuter.element.classList.remove('estado-has-value');
        }
    });

    const clienteSelect = document.getElementById('selectCliente');
    clienteSelect.addEventListener('change', function () {
        const idCliente = this.value;
        fetch(`/NoConformidades/GetPiezasByCliente?idCliente=${idCliente}`)
            .then(response => response.json())
            .then(data => {
                piezaChoices.clearStore();
                const newChoices = data.map(pieza => ({
                    value: pieza.idPieza,
                    label: pieza.descripcion
                }));
                piezaChoices.setChoices(newChoices, 'value', 'label', true);
            })
            .catch(error => console.log('Error al obtener piezas:', error));
    });

    // Validación y envío del formulario
    const form = document.getElementById('formCrearNoConformidad');
    form.addEventListener('submit', function (event) {
        event.preventDefault();
        let errorMsg = "";

        function isEmpty(value) {
            return !value || value.trim() === "";
        }

        if (isEmpty(document.getElementById('selectCliente').value)) {
            errorMsg += "Seleccione un cliente.\n";
        }
        if (isEmpty(document.getElementById('selectPieza').value)) {
            errorMsg += "Seleccione una pieza.\n";
        }
        if (isEmpty(document.getElementById('selectProceso').value)) {
            errorMsg += "Seleccione un proceso.\n";
        }

        const recurrenteRadios = document.getElementsByName('Recurrente');
        let recurrenteSeleccionado = Array.from(recurrenteRadios).some(r => r.checked);
        if (!recurrenteSeleccionado) {
            errorMsg += "Seleccione una opción para recurrente.\n";
        }

        if (isEmpty(document.querySelector('input[name="Frecuencia"]').value)) {
            errorMsg += "Complete el campo Frecuencia.\n";
        }
        if (isEmpty(document.getElementById('selectDetectabilidad').value)) {
            errorMsg += "Seleccione una detectabilidad.\n";
        }

        const defectoSelect = document.getElementById('selectDefecto');
        if (!defectoSelect || defectoSelect.selectedOptions.length === 0) {
            errorMsg += "Seleccione al menos un defecto.\n";
        }

        const causaSelect = document.getElementById('selectCausa');
        if (!causaSelect || causaSelect.selectedOptions.length === 0) {
            errorMsg += "Seleccione al menos una causa.\n";
        }

        if (isEmpty(document.querySelector('input[name="FechaIncidente"]').value)) {
            errorMsg += "Complete la Fecha de Incidente.\n";
        }
        if (isEmpty(document.querySelector('input[name="FechaProduccion"]').value)) {
            errorMsg += "Complete la Fecha de Producción.\n";
        }
        if (isEmpty(document.getElementById('selectPrioridad').value)) {
            errorMsg += "Seleccione una prioridad.\n";
        }
        if (isEmpty(document.getElementById('selectEstado').value)) {
            errorMsg += "Seleccione un estado.\n";
        }
        if (isEmpty(document.querySelector('input[name="Gravedad"]').value)) {
            errorMsg += "Complete el campo Gravedad.\n";
        }
        if (isEmpty(document.querySelector('input[name="Consecuencia"]').value)) {
            errorMsg += "Complete el campo Consecuencia.\n";
        }
        if (isEmpty(document.querySelector('textarea[name="Descripcion"]').value)) {
            errorMsg += "Complete el campo Descripción.\n";
        }

        if (errorMsg !== "") {
            document.getElementById('mensajeValidacion').textContent = "Complete todos los datos obligatorios:\n" + errorMsg;
            const modalValidacion = new bootstrap.Modal(document.getElementById('modalValidacion'));
            modalValidacion.show();
            return;
        }

        const url = createAjaxUrl;
        const formData = new FormData(form);
        fetch(url, {
            method: "POST",
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error("Error en la comunicación con el servidor.");
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // 1) Mostrar el modal
                    const modalEl = document.getElementById('modalExito');
                    const exitoModal = new bootstrap.Modal(modalEl);
                    // Actualizar el texto dinámicamente:
                    const esEdicion = data.idNoConformidad && parseInt(data.idNoConformidad) > 0;
                    document.getElementById('mensajeExitoModal').textContent =
                        esEdicion
                            ? "No conformidad modificada correctamente."
                            : "No conformidad creada correctamente.";
                    exitoModal.show();

                    window.currentNoConfId = data.idNoConformidad;

 

                   
                }
                    else {
                    document.getElementById('mensajeErrorModal').textContent = data.message || "No conformidad no creada, intente nuevamente.";
                    new bootstrap.Modal(document.getElementById('modalError')).show();
                }
            })
            .catch(error => {
                document.getElementById('mensajeErrorModal').textContent = error.message;
                new bootstrap.Modal(document.getElementById('modalError')).show();
            });
    });

    // Si volvés desde edición
    const id = localStorage.getItem("volverDetalleNoConf");
    if (id) {
        localStorage.removeItem("volverDetalleNoConf");
        abrirDetalleNoConf(id);
    }

    // 1) Inicialización única de Choices para el dropdown de equipos (ponlo justo después de window.equiposChoices = ...)
    window.equiposChoices = new Choices('#selectEquipos', {
        shouldSort: false,
        searchEnabled: false,
        placeholder: true,
        placeholderValue: '-- Seleccione un equipo --'
    });

    // 2) Y justo después de eso (todavía dentro de DOMContentLoaded), añade esto:
    const btnAsignar = document.getElementById('btnAsignarEquipo');
    const bsModalExito = new bootstrap.Modal(document.getElementById('modalExito'));
    const bsModalAsig = new bootstrap.Modal(document.getElementById('modalAsignarEquipo'));

    btnAsignar.addEventListener('click', async () => {
        bsModalExito.hide();

        // 2.1) Trae la lista desde el servidor
        const equipos = await (await fetch('/Equipos/GetEquiposByCreador')).json();

        // 2.2) Limpia y recarga el Choices existente
        window.equiposChoices.clearChoices();
        window.equiposChoices.setChoices(
            equipos.map(e => ({ value: e.idEquipo, label: e.nombre })),
            'value',
            'label',
            true
        );

        // 2.3) Abre el modal
        bsModalAsig.show();

        // 3) Al cambiar de equipo, recargar lista de miembros:
        const selectEqui = document.getElementById('selectEquipos');
        const listaMiembros = document.getElementById('listaMiembros');

        selectEqui.addEventListener('change', async () => {
            listaMiembros.innerHTML = '<li class="list-group-item">Cargando…</li>';
            if (!selectEqui.value) {
                listaMiembros.innerHTML = '';
                return;
            }
            try {
                const miembros = await (await fetch(`/Equipos/GetMiembrosDeEquipo?idEquipo=${selectEqui.value}`)).json();
                listaMiembros.innerHTML = '';
                miembros.forEach(m => {
                    const li = document.createElement('li');
                    li.className = 'list-group-item';
                    li.textContent = m.nombreUsuario;
                    listaMiembros.append(li);
                });
            } catch {
                listaMiembros.innerHTML = '<li class="list-group-item text-danger">Error al cargar miembros</li>';
            }
        });

        // 4) Al presionar “Aceptar”, enviar la asignación:
        document.getElementById('btnAceptarAsignar').addEventListener('click', async () => {
            const equipoId = selectEqui.value;
            if (!equipoId) {
                alert('Seleccione un equipo antes de continuar.');
                return;
            }
            bsModalAsig.hide();
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            try {
                const res = await fetch('/NoConformidades/AsignarEquipoATecnica', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({
                        IdNoConformidad: window.currentNoConfId,
                        IdEquipo: parseInt(equipoId, 10)
                    })
                });
                if (!res.ok) throw new Error();
                window.location.href = '/NoConformidades/Index';
            } catch {
                alert('Error al asignar equipo, inténtelo de nuevo.');
                bsModalAsig.show();
            }
        });

    });
});

function volverADetalle() {
    const id = document.querySelector('[name="IdNoConformidad"]').value;
    window.location.href = `/NoConformidades/Index?abrirDetalle=${id}`;
}
