// wwwroot/js/Paso2.js
console.log('⚡ Paso2.js cargado');

document.addEventListener('DOMContentLoaded', () => {
    console.log('✅ DOM listo en Paso2.js');

    // ----- 1) INITIALIZE CHOICES.JS -----
    const elResp = document.getElementById('Responsable');
    const elEstado = document.getElementById('IdEstado');
    const elCat5M = document.getElementById('categoria5M');

    const choicesResp = elResp && new Choices(elResp, {
        searchEnabled: false,
        itemSelectText: '',
        shouldSort: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });
    const choicesEstado = elEstado && new Choices(elEstado, {
        searchEnabled: false,
        itemSelectText: '',
        shouldSort: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });
    const choicesCat5M = elCat5M && new Choices(elCat5M, {
        searchEnabled: false,
        itemSelectText: '',
        shouldSort: false,
        placeholder: true,
        placeholderValue: '-- Seleccione --'
    });

    // ----- 1.b) GRADIENT BACKGROUND FOR ESTADO -----
    if (elEstado) {
        let estadoChoices;
        setTimeout(() => {
            estadoChoices = elEstado.closest('.combo-field')?.querySelector('.choices__inner');
            pintarEstado(); // pintar con la clase correcta al cargar
        }, 0);
        const mapa = {
            '2': 'estado-pendiente',
            '3': 'estado-en-progreso',
            '4': 'estado-completado',
            '5': 'estado-terminado'
        };

        function pintarEstado() {
            if (!estadoChoices) return;
            Object.values(mapa).forEach(c => estadoChoices.classList.remove(c));
            const cls = mapa[elEstado.value];
            if (cls) estadoChoices.classList.add(cls);
        }

        elEstado.addEventListener('change', pintarEstado);
        pintarEstado();
    }


    // ----- 2) FORM VALIDATION (FECHAS + CAMPOS) -----
    const form = document.getElementById('formPaso2');
    const fechaIni = document.getElementById('FechaInicio');
    const fechaFin = document.getElementById('FechaFin');


    form?.addEventListener('submit', e => {
        // 2.a) FechaInicio ≤ FechaFin
        if (fechaIni.value && fechaFin.value &&
            new Date(fechaIni.value) > new Date(fechaFin.value)) {
            e.preventDefault();
            bootstrap.Modal
                .getOrCreateInstance(document.getElementById('modalErrorFechas'))
                .show();
            fechaIni.focus();
            return;
        }

        // 2.b) Campos obligatorios
        const faltan = [];
        if (!fechaIni.value) faltan.push('Fecha de inicio');
        if (!elEstado.value) faltan.push('Estado');
        if (!elResp.value) faltan.push('Responsable');

        if (faltan.length) {
            e.preventDefault();
            const modal = document.getElementById('modalFaltanCampos');
            modal.querySelector('.modal-body').innerHTML =
                '<p>Por favor completa los siguientes campos:</p><ul>' +
                faltan.map(f => `<li>${f}</li>`).join('') +
                '</ul>';
            bootstrap.Modal.getOrCreateInstance(modal).show();
            ({ 'Fecha de inicio': fechaIni, 'Estado': elEstado, 'Responsable': elResp }[faltan[0]]).focus();
        }
    });

    // ----- 3) MODAL “PERMISO DENEGADO” PARA GUARDAR FINAL -----
    document.getElementById('btnGuardarPaso2')?.addEventListener('click', e => {
        if (e.currentTarget.classList.contains('no-permitido')) {
            e.preventDefault();
            bootstrap.Modal
                .getOrCreateInstance(document.getElementById('modalPermisoGuardar'))
                .show();
        }
    });

    // ----- 4) LÓGICA DE CAUSAS EN MEMORIA -----
    const listaO = document.getElementById('lista-ocurrencia');
    const listaND = document.getElementById('lista-no-deteccion');
    const hiddenCausas = document.getElementById('CausasJson');
    const modalCausa = new bootstrap.Modal(document.getElementById('modalCausa'));
    const btnNueva = document.getElementById('btnNuevaCausa');
    const inputIdCausa = document.getElementById('IdCausa');
    const txtDesc = document.getElementById('descripcionCausa');


    // … tras crear modalCausa, btnNueva, inputIdCausa y txtDesc …
    if (btnNueva) {
        btnNueva.addEventListener('click', e => {
            // 1) ¿Paso2 ya existe?
            const esNuevo = document
                .getElementById('esNuevoPaso2')
                ?.value === 'True';
            if (esNuevo) {
                e.preventDefault();
                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById('modalPermisoGuardar')
                    )
                    .show();
                return;
            }

            // 2) ¿Faltan Responsable o Estado?
            const resp = document.getElementById('Responsable').value;
            const est = document.getElementById('IdEstado').value;
            if (!resp || !est) {
                e.preventDefault();
                const cuerpo = document.querySelector(
                    '#modalFaltanCampos .custom-modal-body'
                );
                cuerpo.textContent =
                    'Primero debes seleccionar Responsable y Estado antes de agregar causas.';
                bootstrap.Modal
                    .getOrCreateInstance(
                        document.getElementById('modalFaltanCampos')
                    )
                    .show();
                return;
            }

            // 3) Todo OK → reseteo el formulario interno y abro el modal
            inputIdCausa.value = '0';               
            document.getElementById('impactoOcurrencia').checked = true;
            document.getElementById('resOk').checked = true;
            modalCausa.show();
        });
    }






    let causasPendientes = [];
    let editarId = 0;

    // 4.a) Cargar JSON inicial desde Razor
    try {
        if (hiddenCausas?.value) {
            causasPendientes = JSON.parse(hiddenCausas.value);
        }
    } catch (err) {
        console.error('JSON inválido en #CausasJson', err);
    }

    // 4.b) Función de render
    function renderCausas() {
        listaO.innerHTML = '';
        listaND.innerHTML = '';

        causasPendientes.forEach(c => {
            const cont = c.ClasificacionImpacto === 0 ? listaO : listaND;
            const div = document.createElement('div');
            div.className = 'list-group-item causa-item';
            div.dataset.id = c.IdCausa;
            div.innerHTML = `
              <div class="d-flex justify-content-between align-items-start w-100">
                <div>
                  <div class="causa-text fw-bold">${c.DescripcionCausa}</div>
                  <div class="causa-category  small">${c.CategoriaDescripcion}</div>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger btn-eliminar-causa ms-2" data-id="${c.IdCausa}" title="Eliminar">
                  🗑️
                </button>
              </div>
            `;
            cont.appendChild(div);
        });

        // actualizar hidden para el postback si lo necesitas
        if (hiddenCausas) hiddenCausas.value = JSON.stringify(causasPendientes);
    }
    renderCausas();



    // 4.d) “Editar causa” al clickar sobre cualquier .causa-item
    document.body.addEventListener('click', e => {
        // Si se hizo clic en el botón de eliminar, no seguimos con editar
        if (e.target.closest('.btn-eliminar-causa')) return;

        const item = e.target.closest('.causa-item');
        if (!item) return;

        const id = +item.dataset.id;
        const dto = causasPendientes.find(c => c.IdCausa === id);
        if (!dto) return;

        editarId = dto.IdCausa;
        inputIdCausa.value = dto.IdCausa;

        // Reaplico la categoría en Choices.js:
        choicesCat5M && choicesCat5M.setChoiceByValue(dto.IdCategoria5M.toString());

        txtDesc.value = dto.DescripcionCausa;
        document.querySelector(`input[name="NuevaCausa.ClasificacionImpacto"][value="${dto.ClasificacionImpacto}"]`).checked = true;
        document.querySelector(`input[name="NuevaCausa.ResultadoVerificacion"][value="${dto.ResultadoVerificacion}"]`).checked = true;

        modalCausa.show();
    });

    // 4.e) Validación del modal de causa al hacer clic en "Guardar"
    document.getElementById('btnGuardarCausa')?.addEventListener('click', (e) => {
        const cat = document.getElementById('categoria5M');
        const desc = document.getElementById('descripcionCausa');

        const errores = [];

        if (!cat.value || cat.value === "") errores.push("Tipo de problema (5M)");
        if (!desc.value.trim()) errores.push("Descripción de la causa");

        if (errores.length > 0) {
            e.preventDefault(); // evita que el formulario se envíe

            const modal = document.getElementById('modalFaltanCampos');
            modal.querySelector('.modal-body').innerHTML =
                '<p>Por favor completá los siguientes campos:</p><ul>' +
                errores.map(f => `<li>${f}</li>`).join('') +
                '</ul>';

            bootstrap.Modal.getOrCreateInstance(modal).show();

            // Enfocar en el primer campo con error
            if (!cat.value || cat.value === "") cat.focus();
            else desc.focus();

            return false;
        }
    });

    // —––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––—
    //   5) ELIMINAR CAUSA CON CONFIRMACIÓN (con modal)

    let causaAEliminarId = null;

    // 5.a) Al clickar en el tachito abrimos el modal y guardamos el ID
    document.body.addEventListener('click', e => {
        const btn = e.target.closest('.btn-eliminar-causa');
        if (!btn) return;

        causaAEliminarId = +btn.dataset.id;

        // Abrimos el modal de confirmación
        const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalConfirmarEliminarCausa'));
        modal.show();
    });


    // 5.b) Al confirmar en el modal, enviamos el fetch
    document.getElementById('btnConfirmEliminar')?.addEventListener('click', () => {
        if (!causaAEliminarId) return;

        const idFicha = parseInt(document.querySelector('input[name="IdFichaTecnica"]').value);
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch(`/FichasTecnicas/${idFicha}/Paso2/EliminarCausaJson`, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "RequestVerificationToken": token
            },
            body: `idFicha=${idFicha}&idCausa=${causaAEliminarId}`
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    // Eliminamos del array
                    causasPendientes = causasPendientes.filter(c => c.IdCausa !== causaAEliminarId);
                    renderCausas();

                    // Cerramos el modal
                    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalConfirmarEliminarCausa')).hide();
                    causaAEliminarId = null;
                } else {
                    alert("⚠️ No se pudo eliminar la causa: " + data.message);
                }
            })
            .catch(err => {
                console.error("❌ Error al eliminar la causa", err);
                alert("Ocurrió un error al intentar eliminar la causa.");
            });
    });



});  

