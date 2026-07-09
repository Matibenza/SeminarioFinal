document.addEventListener('DOMContentLoaded', function () {
    const avisoFT = document.getElementById('mensajeAsignacionFT');
    if (avisoFT) mostrarNotificacion(avisoFT.value, 'success');

    // ================================
    // PARTE 0: Detectar rol actual
    // ================================
    const modalContent = document.querySelector('#modalDetalleEquipo .modal-content');
    const esSupervisor = modalContent && modalContent.dataset.rol === 'supervisor';

    if (!esSupervisor) {
        // Ocultar todos los controles de edición si no es supervisor
        document.querySelectorAll(
            '#btnModificarNombreLink, #btnModificarDescripcionLink, ' +
            '#btnAsignarFichaTecnica, #btnDesasignarFichaTecnica, ' +
            '#btnGuardarCambios, #btnCancelar, #btnEliminarEquipo'
        ).forEach(el => el.classList.add('d-none'));

        // Deshabilitar selects en el modal
        document.querySelectorAll('.select-rol, #selectAgregarUsuario')
            .forEach(sel => {
                if (sel.tagName === 'SELECT') sel.setAttribute('disabled', '');
            });
    }

    // ====================================================
    // DECLARACIÓN compartida de Choices.js para el limpiar
    // ====================================================
    let choicesPiloto, choicesAuditores;

    // ================================================
    // PARTE 1+2: Inicializar "Crear equipo" (si existe)
    // ================================================
    const pilotoSelect = document.getElementById('Piloto');
    const auditoresSelect = document.getElementById('Auditores');
    const empleadosSelect = document.getElementById('Empleados');
    const formCrear = document.getElementById('formCrearEquipo');

    if (formCrear && pilotoSelect && auditoresSelect) {
        // Recopilamos empleados para controlar el enable/disable
        const empleadosData = Array.from(pilotoSelect.options)
            .filter(o => o.value)
            .map(o => ({ value: o.value, label: o.text }));

        // Instanciamos Choices y las guardamos en variables externas
        choicesPiloto = new Choices(pilotoSelect, {
            searchEnabled: true,
            shouldSort: false,
            placeholder: true,
            placeholderValue: 'Seleccionar piloto'
        });
        choicesAuditores = new Choices(auditoresSelect, {
            searchEnabled: true,
            removeItemButton: true,
            shouldSort: false,
            placeholder: true,
            placeholderValue: 'Seleccionar auditores'
        });

        if (empleadosSelect) {
            new Choices(empleadosSelect, {
                searchEnabled: true,
                removeItemButton: true,
                shouldSort: false,
                placeholder: true,
                placeholderValue: 'Seleccionar empleados'
            });
        }

        // Sincronizamos disabling entre Piloto y Auditores
        function refreshPiloto() {
            const selAud = choicesAuditores.getValue(true);
            const curPil = choicesPiloto.getValue(true)[0] || '';
            const opts = empleadosData.map(e => ({
                value: e.value,
                label: e.label,
                disabled: selAud.includes(e.value)
            }));
            choicesPiloto.clearChoices();
            choicesPiloto.setChoices(opts, 'value', 'label', true);
            if (curPil && !selAud.includes(curPil)) {
                choicesPiloto.setChoiceByValue(curPil);
            }
        }
        function refreshAuditores() {
            const selPil = choicesPiloto.getValue(true)[0] || '';
            const curAud = choicesAuditores.getValue(true);
            const opts = empleadosData.map(e => ({
                value: e.value,
                label: e.label,
                disabled: e.value === selPil
            }));
            choicesAuditores.clearChoices();
            choicesAuditores.setChoices(opts, 'value', 'label', true);
            curAud.forEach(val => {
                if (val !== selPil) choicesAuditores.setChoiceByValue(val);
            });
        }

        pilotoSelect.addEventListener('change', () => {
            refreshAuditores();
            refreshPiloto();
        });
        auditoresSelect.addEventListener('change', () => {
            refreshPiloto();
            refreshAuditores();
        });

        // Validación del formulario de creación
        formCrear.addEventListener('submit', async function (e) {
            e.preventDefault(); // SIEMPRE

            const nombre = document.getElementById('Nombre').value.trim();
            const descripcion = document.getElementById('Descripcion').value.trim();
            const piloto = document.getElementById('Piloto').value;
            
            const auditores = Array.from(document.querySelectorAll('#Auditores option:checked')).map(opt => +opt.value);
            const empleados = []; // si querés enviar esto también

            const faltan = [];
            if (!nombre) faltan.push('Nombre');
            if (!descripcion) faltan.push('Descripción');
            if (!piloto) faltan.push('Piloto');
            if (auditores.length === 0) faltan.push('Auditores');

            if (faltan.length) {
                const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCamposIncompletos'));
                modal._element.querySelector('.modal-body').textContent = 'Por favor, complete: ' + faltan.join(', ');
                modal.show();
                return;
            }

            // Validar nombre duplicado
            const idCreador = document.getElementById('idUsuarioActual')?.value;
            try {
                const resp = await fetch(`/Equipos/ValidarNombre?nombre=${encodeURIComponent(nombre)}&idCreador=${idCreador}`);
                const { disponible } = await resp.json();

                if (!disponible) {
                    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('modalCamposIncompletos'));
                    modal._element.querySelector('.modal-body').textContent = 'Ya existe un equipo con ese nombre.';
                    modal.show();
                    return;
                }

                // ✅ Todo bien, enviamos el formulario a mano
                const payload = new URLSearchParams();
                payload.append('nombre', nombre);
                payload.append('descripcion', descripcion);
                payload.append('piloto', piloto);
                
                auditores.forEach(a => payload.append('auditores', a));
                empleados.forEach(e => payload.append('empleados', e));

                const result = await fetch('/Equipos/Crear', {
                    method: 'POST',
                    body: payload,
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded'
                    }
                });

                if (result.ok) {
                    window.location.reload();
                } else {
                    mostrarNotificacion('Error al guardar equipo', 'error');
                }
            } catch (error) {
                console.error(error);
                mostrarNotificacion('Error de red al guardar equipo', 'error');
            }
        });

    }



    // ================================
    // Util: mostrar notificación
    // ================================
    function mostrarNotificacion(mensaje, tipo = 'success') {
        const noti = document.createElement('div');
        noti.innerHTML = tipo === 'success'
            ? `<i class="fas fa-check-circle me-2"></i>${mensaje}`
            : `<i class="fas fa-exclamation-circle me-2"></i>${mensaje}`;
        Object.assign(noti.style, {
            position: 'fixed',
            bottom: '30px',
            right: '30px',
            padding: '15px 25px',
            borderRadius: '6px',
            zIndex: 9999,
            color: '#fff',
            display: 'flex',
            alignItems: 'center',
            background: tipo === 'success' ? '#28a745' : '#dc3545',
            boxShadow: '0 0 10px rgba(0,0,0,0.2)'
        });
        document.body.appendChild(noti);
        setTimeout(() => noti.remove(), 3000);
    }
    const exito = document.getElementById('equipoCreadoExito');
    if (exito) mostrarNotificacion(exito.value, 'success');

    // ================================================
    // PARTE 3: Modal Detalle de equipo (sup y emp)
    // ================================================
    let usuariosEnEdicion = [];

    function actualizarSelectAgregarUsuario() {
        if (!esSupervisor) return;
        const sel = document.getElementById('selectAgregarUsuario');
        sel.innerHTML = '<option value="">-- Seleccione un empleado --</option>';
        const usados = usuariosEnEdicion.map(u => String(u.id));
        document.querySelectorAll('#Piloto option').forEach(o => {
            if (o.value && !usados.includes(o.value)) {
                const opt = document.createElement('option');
                opt.value = o.value;
                opt.textContent = o.text;
                sel.appendChild(opt);
            }
        });
    }
    function actualizarDisponibilidadPiloto() {
        if (!esSupervisor) return;
        const selects = Array.from(document.querySelectorAll('.select-rol'));
        const primero = selects.find(s => s.value === 'Piloto');
        selects.forEach(s => {
            const op = s.querySelector('option[value="Piloto"]');
            if (op) op.disabled = !!primero && s !== primero;
        });
    }
    function renderUsuariosAsignados(usuarios) {
        const cont = document.getElementById('listaUsuariosAsignados');
        cont.innerHTML = '';
        usuarios.forEach(usuario => {
            const row = document.createElement('div');
            row.classList.add('usuario-grid', 'mb-2');

            // Nombre
            const spanName = document.createElement('span');
            spanName.classList.add('nombre-usuario');
            spanName.textContent = usuario.nombre;
            row.append(spanName);

            if (esSupervisor) {
                // Select de rol
                const sel = document.createElement('select');
                sel.className = 'form-select select-rol ms-2';
                ['Piloto', 'Auditor'].forEach(r => {
                    const op = document.createElement('option');
                    op.value = r; op.textContent = r;
                    if (r === usuario.rol) op.selected = true;
                    sel.appendChild(op);
                });
                sel.addEventListener('change', function () {
                    usuariosEnEdicion = usuariosEnEdicion.map(u => {
                        if (u.id === usuario.id) u.rol = this.value;
                        return u;
                    });
                    actualizarDisponibilidadPiloto();
                });
                row.append(sel);

                // Botón quitar
                const btn = document.createElement('button');
                btn.className = 'btn btn-sm btn-danger ms-2';
                btn.innerHTML = '&times;';
                btn.addEventListener('click', () => {
                    usuariosEnEdicion = usuariosEnEdicion.filter(u => u.id !== usuario.id);
                    renderUsuariosAsignados(usuariosEnEdicion);
                    actualizarSelectAgregarUsuario();
                    actualizarDisponibilidadPiloto();
                });
                row.append(btn);
            } else {
                // Empleado ve su rol en texto
                const spanRol = document.createElement('span');
                spanRol.classList.add('nombre-usuario', 'ms-2');
                spanRol.textContent = usuario.rol;
                row.append(spanRol);
            }

            cont.append(row);
        });
        actualizarDisponibilidadPiloto();
    }

    document.addEventListener('click', function (e) {
        const fila = e.target.closest('.fila-equipo');
        if (!fila) return;

        // 1) Renderizamos tu lista de usuarios como ya tenías
        const datos = JSON.parse(fila.dataset.usuarios || '[]');
        usuariosEnEdicion = datos.map(u => ({ id: u.id, nombre: u.nombre, rol: u.rol }));
        renderUsuariosAsignados(usuariosEnEdicion);
        actualizarSelectAgregarUsuario();

        // 2) Rellenamos los campos básicos
        document.getElementById('detalleEquipo-nombre-titulo').textContent = fila.dataset.nombre;
        document.getElementById('detalleEquipo-nombre').textContent = fila.dataset.nombre;
        document.getElementById('detalleEquipo-descripcion').textContent = fila.dataset.descripcion;
        document.getElementById('detalleEquipo-fecha').textContent = fila.dataset.fechacreacion;
        document.getElementById('detalleEquipo-id-text').textContent = fila.dataset.id;
        document.getElementById('detalleEquipo-id').value = fila.dataset.id;

        // 3) Y llamamos a tu función que ya hace el fetch de FT y abre el modal
        const equipoId = fila.dataset.id;
        abrirDetalleEquipo(parseInt(fila.dataset.id, 10));
    });



    // ================================================
    // PARTE 4: Solo supervisor (agregar, guardar…)
    // ================================================
    if (esSupervisor) {
        // — Agregar usuario
        const selectAgregar = document.getElementById('selectAgregarUsuario');
        if (selectAgregar) {
            selectAgregar.addEventListener('change', function () {
                const id = this.value; if (!id) return;
                const nombre = this.options[this.selectedIndex].text;
                usuariosEnEdicion.push({ id: +id, nombre, rol: 'Auditor' });
                renderUsuariosAsignados(usuariosEnEdicion);
                actualizarSelectAgregarUsuario();
                actualizarDisponibilidadPiloto();
                this.value = '';
            });
        }

        // — Guardar cambios
        const btnSave = document.getElementById('btnGuardarCambios');
        if (btnSave) {
            btnSave.addEventListener('click', async () => {
                const payload = {
                    idEquipo: document.getElementById('detalleEquipo-id').value,
                    nombre: document.getElementById('detalleEquipo-nombre').textContent,
                    descripcion: document.getElementById('detalleEquipo-descripcion').textContent,
                    usuarios: usuariosEnEdicion
                };
                try {
                    const resp = await fetch('/Equipos/ActualizarEquipo', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    if (!resp.ok) throw '';
                    const json = await resp.json();
                    mostrarNotificacion(json.message, 'success');
                    bootstrap.Modal.getOrCreateInstance(
                        document.getElementById('modalDetalleEquipo')
                    ).hide();
                    window.location.reload();
                } catch {
                    mostrarNotificacion('Error actualizando equipo', 'error');
                }
            });
        }

        // — Modificar nombre
        const btnModName = document.getElementById('btnModificarNombreLink');
        if (btnModName) {
            btnModName.addEventListener('click', e => {
                e.preventDefault();
                const actual = document.getElementById('detalleEquipo-nombre').textContent.trim();
                document.getElementById('txtNuevoNombreEquipo').value = actual;
                document.getElementById('errorNombreEquipo').classList.add('d-none');
                bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('modalModificarNombre')
                ).show();
            });
        }
        const btnConfirmarNuevoNombre = document.getElementById('btnConfirmarNuevoNombre');
        if (btnConfirmarNuevoNombre) {
            btnConfirmarNuevoNombre.addEventListener('click', async () => {
                const input = document.getElementById('txtNuevoNombreEquipo');
                const error = document.getElementById('errorNombreEquipo');
                const nuevo = input.value.trim();
                if (!nuevo) return;
                try {
                    const idCreador = document.getElementById('idUsuarioActual')?.value; // o como lo tengas
                    const resp = await fetch(`/Equipos/ValidarNombre?nombre=${encodeURIComponent(nuevo)}&idCreador=${idCreador}`);
                    const { disponible } = await resp.json();
                    if (!disponible) {
                        error.classList.remove('d-none');
                        return;
                    }
                    document.getElementById('detalleEquipo-nombre').textContent = nuevo;
                    document.getElementById('detalleEquipo-nombre-titulo').textContent = nuevo;
                    bootstrap.Modal.getOrCreateInstance(
                        document.getElementById('modalModificarNombre')
                    ).hide();
                } catch (err) {
                    console.error('Error validando nombre:', err);
                }
            });
        }

        // — Modificar descripción
        const btnModDesc = document.getElementById('btnModificarDescripcionLink');
        if (btnModDesc) {
            btnModDesc.addEventListener('click', e => {
                e.preventDefault();
                const actual = document.getElementById('detalleEquipo-descripcion').textContent.trim();
                document.getElementById('txtNuevaDescripcionEquipo').value = actual;
                bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('modalModificarDescripcion')
                ).show();
            });
        }
        const btnConfirmarNuevaDescripcion = document.getElementById('btnConfirmarNuevaDescripcion');
        if (btnConfirmarNuevaDescripcion) {
            btnConfirmarNuevaDescripcion.addEventListener('click', () => {
                const nuevaDesc = document.getElementById('txtNuevaDescripcionEquipo').value.trim();
                document.getElementById('detalleEquipo-descripcion').textContent = nuevaDesc;
                bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('modalModificarDescripcion')
                ).hide();
            });
        }

        // — Eliminar equipo
        const btnEliminar = document.getElementById('btnEliminarEquipo');
        if (btnEliminar) {
            btnEliminar.addEventListener('click', () => {
                const nombre = document.getElementById('detalleEquipo-nombre').textContent.trim();
                document.querySelector('#textoConfirmarEliminar strong').textContent = nombre;
                const modalEl = document.getElementById('modalConfirmarEliminar');
                const modal = new bootstrap.Modal(modalEl);
                modal.show();
                modalEl.addEventListener('shown.bs.modal', () => {
                    document.querySelectorAll('.modal-backdrop').forEach(b => b.classList.add('blur-backdrop'));
                });
                modalEl.addEventListener('hidden.bs.modal', () => {
                    document.querySelectorAll('.modal-backdrop.blur-backdrop').forEach(b => b.classList.remove('blur-backdrop'));
                });
            });
        }
        const btnConfirmarEliminar = document.getElementById('btnConfirmarEliminar');
        if (btnConfirmarEliminar) {
            btnConfirmarEliminar.addEventListener('click', async () => {
                const id = document.getElementById('detalleEquipo-id').value;
                try {
                    const resp = await fetch(`/Equipos/Eliminar?id=${encodeURIComponent(id)}`, { method: 'POST' });
                    if (!resp.ok) throw '';
                    window.location.reload();
                } catch {
                    mostrarNotificacion('Error al eliminar el equipo', 'error');
                    bootstrap.Modal.getOrCreateInstance(
                        document.getElementById('modalConfirmarEliminar')
                    ).hide();
                }
            });
        }
    } // 👈 cierre del if (esSupervisor)

    // Al hacer click en una fila de equipo
    function abrirDetalleEquipo(equipoId) {
        const modalEl = document.getElementById('modalDetalleEquipo');
        const tbody = modalEl.querySelector('#ft-table-body');

        // Mostrar estado de carga y abrir modal
        tbody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Cargando...</td></tr>';
        bootstrap.Modal.getOrCreateInstance(modalEl).show();

        fetch(`/Equipos/GetFichasDeEquipo?idEquipo=${equipoId}`)
            .then(res => res.ok ? res.json() : Promise.reject(res.statusText))
            .then(lista => {
                if (!lista.length) {
                    tbody.innerHTML = '<tr><td colspan="3" class="text-center text-muted">Sin fichas técnicas</td></tr>';
                    return;
                }
                tbody.innerHTML = lista.map(f => `
        <tr>
          <td>${f.idFichaTecnica ?? f.IdFichaTecnica}</td>
          <td>${f.idNoConformidad ?? f.IdNoConformidad}</td>
          <td>${f.fecha ?? f.Fecha}</td>
        </tr>
      `).join('');
            })
            .catch(err => {
                console.error('Error al cargar FT:', err);
                tbody.innerHTML = '<tr><td colspan="3" class="text-center text-danger">Error al cargar fichas técnicas</td></tr>';
            });
    }


    // — Abrir modal de asignar FT
    document.getElementById('btnAsignarFichaTecnica').addEventListener('click', async () => {
        const equipoId = +document.getElementById('detalleEquipo-id').value;
        const nombre = document.getElementById('detalleEquipo-nombre').textContent;
        const modalEl = document.getElementById('modalAsignarFT');
        const select = modalEl.querySelector('#selectFTsinAsignar');
        const modal = new bootstrap.Modal(modalEl);

        // Actualizamos título y estado inicial
        document.getElementById('asignarFT-equipo-nombre').textContent = nombre;
        select.innerHTML = '<option>— Cargando… —</option>';

        try {
            const resp = await fetch(`/Equipos/GetFTSinAsignar?equipoId=${equipoId}`);
            if (!resp.ok) throw new Error(resp.statusText);
            const fts = await resp.json();
            console.log('FT sin asignar:', fts);

            if (!fts.length) {
                select.innerHTML = '<option disabled>No hay fichas técnicas sin asignar</option>';
            } else {
                select.innerHTML = fts
                    .map(ft => {
                        const id = ft.idFichaTecnica ?? ft.IdFichaTecnica;
                        return `<option value="${id}">FC-${id}</option>`;
                    })
                    .join('');
            }
        } catch (err) {
            console.error('Error al cargar FT sin asignar:', err);
            select.innerHTML = '<option disabled>Error al cargar fichas</option>';
        }

        modal.show();
    });

    document
        .getElementById('btnConfirmarAsignarFT')
        .addEventListener('click', async () => {
            const idEquipo = +document.getElementById('detalleEquipo-id').value;
            const idFT = +document.getElementById('selectFTsinAsignar').value;
            if (!idFT) return;

            try {
                const resp = await fetch('/Equipos/AsignarFT', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ IdEquipo: idEquipo, IdFichaTecnica: idFT })
                });
                const json = await resp.json();
                if (!resp.ok) throw new Error(json.message || resp.statusText);

                // redirigimos al índice con el mensaje en la query
                window.location.href = `/Equipos?mensaje=${encodeURIComponent(json.message)}`;
            } catch (err) {
                console.error('Error al asignar FT:', err);
                mostrarNotificacion(err.message || 'Error al asignar ficha técnica', 'error');
            }
        });


    // — Confirmar asignación
    document.getElementById('btnConfirmarAsignarFT').addEventListener('click', async () => {
        const equipoId = +document.getElementById('detalleEquipo-id').value;
        const idFT = +document.getElementById('selectFTsinAsignar').value;
        if (!idFT) return;

        try {
            const resp = await fetch('/Equipos/AsignarFT', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ IdEquipo: equipoId, IdFichaTecnica: idFT })
            });
            const json = await resp.json();
            if (resp.ok) {
                // Redirige al índice mostrando mensaje
                window.location.href = `/Equipos?mensaje=${encodeURIComponent(json.message)}`;
            } else {
                alert('Error: ' + (json.message || resp.statusText));
            }
        } catch {
            alert('Error al asignar ficha técnica');
        }
    });

    // — Abrir modal de desasignar FT
    document.getElementById('btnDesasignarFichaTecnica')?.addEventListener('click', async () => {
        const equipoId = +document.getElementById('detalleEquipo-id').value;
        const nombre = document.getElementById('detalleEquipo-nombre').textContent;
        const modalEl = document.getElementById('modalDesasignarFT');
        const select = modalEl.querySelector('#selectFTasignadas');
        const modal = new bootstrap.Modal(modalEl);

        // Pongo el título igual que en asignar
        document.getElementById('desasignarFT-equipo-nombre').textContent = nombre;
        select.innerHTML = '<option>— Cargando… —</option>';

        try {
            const resp = await fetch(`/Equipos/GetFichasDeEquipo?idEquipo=${equipoId}`);
            if (!resp.ok) throw new Error(resp.statusText);
            const fts = await resp.json();

            if (!fts.length) {
                select.innerHTML = '<option disabled>No hay fichas técnicas asignadas</option>';
            } else {
                select.innerHTML = fts
                    .map(ft => `<option value="${ft.IdFichaTecnica}">FC-${ft.IdFichaTecnica}</option>`)
                    .join('');
            }
        } catch (err) {
            console.error('Error al cargar FT asignadas:', err);
            select.innerHTML = '<option disabled>Error al cargar fichas</option>';
        }

        modal.show();
    });

    // — Abrir modal de desasignar FT
    document.getElementById('btnDesasignarFichaTecnica')?.addEventListener('click', async () => {
        const equipoId = +document.getElementById('detalleEquipo-id').value;
        const nombre = document.getElementById('detalleEquipo-nombre').textContent;
        const modalEl = document.getElementById('modalDesasignarFT');
        const select = modalEl.querySelector('#selectFTasignadas');
        const modal = new bootstrap.Modal(modalEl);

        document.getElementById('desasignarFT-equipo-nombre').textContent = nombre;
        select.innerHTML = '<option>— Cargando… —</option>';

        try {
            const resp = await fetch(`/Equipos/GetFichasDeEquipo?idEquipo=${equipoId}`);
            if (!resp.ok) throw new Error(resp.statusText);
            const fts = await resp.json();

            if (!fts.length) {
                select.innerHTML = '<option disabled>No hay fichas técnicas asignadas</option>';
            } else {
                select.innerHTML = fts
                    .map(ft => {
                        // aquí el fallback a camelCase o PascalCase
                        const id = ft.idFichaTecnica ?? ft.IdFichaTecnica;
                        return `<option value="${id}">FC-${id}</option>`;
                    })
                    .join('');
            }
        } catch (err) {
            console.error('Error al cargar FT asignadas:', err);
            select.innerHTML = '<option disabled>Error al cargar fichas</option>';
        }

        modal.show();
    });

    // — Confirmar desasignación
    document.getElementById('btnConfirmarDesasignarFT')?.addEventListener('click', async () => {
        const equipoId = +document.getElementById('detalleEquipo-id').value;
        const idFT = +document.getElementById('selectFTasignadas').value;
        if (!idFT) return;

        try {
            const resp = await fetch('/Equipos/DesasignarFT', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ IdEquipo: equipoId, IdFichaTecnica: idFT })
            });
            const json = await resp.json();
            if (!resp.ok) throw new Error(json.message || resp.statusText);
            window.location.href = `/Equipos?mensaje=${encodeURIComponent(json.message)}`;
        } catch (err) {
            console.error('Error al desasignar ficha técnica:', err);
            mostrarNotificacion('Error al desasignar ficha técnica', 'error');
        }
    });





}); // 👈 
