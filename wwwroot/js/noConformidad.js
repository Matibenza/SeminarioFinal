function abrirDetalleNoConf(idNoConf) {
   // console.log("▶ abrirDetalleNoConf se llamó con id =", idNoConf);
    fetch(detalleNoConfUrl + '?id=' + idNoConf)
        .then(response => {
            if (!response.ok) throw new Error("Error al obtener el detalle.");
            return response.json();
        })
        .then(data => {
          //  console.log("CAUSAS →", data.causas, "DEFECTOS →", data.defectos);
            const contenido = document.getElementById('contenidoDetalleNoConf');
            contenido.innerHTML = `
                <div class="row">
                  <div class="col-md-6 text-start">
                    <p><strong>ID:</strong> ${data.id}</p>
                    <p><strong>Descripción de la pieza:</strong> ${data.descripcionPieza}</p>
                    <p><strong>Descripción:</strong> ${data.descripcion}</p>
                    <p><strong>Cantidad:</strong> ${data.cantidad}</p>
                    <p><strong>Consecuencia:</strong> ${data.consecuencia}</p>
                    <p><strong>Nombre del proceso:</strong> ${data.nombreProceso}</p>
                    <p><strong>Estado:</strong> ${data.descripcionEstado}</p>
                    <p><strong>Prioridad:</strong> ${data.descripcionPrioridad}</p>
                    <p><strong>Detectabilidad:</strong> ${data.descripcionDetectabilidad}</p>
                  </div>

                  <div class="col-md-6 text-start">
                    <p><strong>Causas:</strong> ${(data.causas && data.causas.length)
                                ? data.causas.join(', ')
                                : '—'
                            }</p>
                    <p><strong>Defectos:</strong> ${(data.defectos && data.defectos.length)
                                ? data.defectos.join(', ')
                                : '—'
                            }</p>

                    <p><strong>Frecuencia:</strong> ${data.frecuencia}</p>
                    <p><strong>Gravedad:</strong> ${data.gravedad}</p>
                    <p><strong>Recurrencia:</strong> ${data.recurrencia}</p>
                    <p><strong>Fecha de creación:</strong> ${data.fechaCreacion}</p>
                    <p><strong>Fecha de incidente:</strong> ${data.fechaIncidente}</p>
                    <p><strong>Fecha de producción:</strong> ${data.fechaProduccion}</p>
                    <p><strong>Fecha de finalización:</strong> ${data.fechaFinalizacion ?? "Sin finalizar"
                            }</p>
                  </div>
                </div>
              `;
            contenido.dataset.idNoConf = data.id;
            new bootstrap.Modal(document.getElementById('modalDetalleNoConf')).show();
        })

        .catch(error => {
            console.error("Error en abrirDetalleNoConf:", error);
            alert("No se pudo cargar el detalle de la No Conformidad.");
        });
}

function confirmarEliminacion() {
    const idNoConf = document.getElementById("contenidoDetalleNoConf").dataset.idNoConf;
    document.getElementById("mensajeConfirmacionEliminar").innerHTML = `
        <i class="bi bi-exclamation-triangle-fill text-warning me-2"></i>
        ¿Está seguro que desea eliminar la No Conformidad <strong>#${idNoConf}</strong>?
    `;
    document.getElementById("detalleModalContent").classList.add("blur-modal");
    const modal = new bootstrap.Modal(document.getElementById('modalConfirmarEliminar'));
    modal.show();
    document.getElementById("btnConfirmarEliminar").onclick = function () {
        eliminarNoConformidad(idNoConf);
    };
}

document.getElementById('modalConfirmarEliminar').addEventListener('hidden.bs.modal', function () {
    document.getElementById("detalleModalContent").classList.remove("blur-modal");
});

function eliminarNoConformidad(id) {
    const btn = document.getElementById("btnConfirmarEliminar");
    btn.disabled = true;
    fetch(`/NoConformidades/Delete?id=${id}`, { method: 'POST' })
        .then(response => {
            btn.disabled = false;
            if (response.ok) location.reload();
            else alert("Error al eliminar la no conformidad.");
        })
        .catch(error => {
            btn.disabled = false;
            console.error("Error al eliminar:", error);
            alert("Error de conexión al eliminar.");
        });
}

function abrirReasignarModal() {
    const idNoConf = document.getElementById("contenidoDetalleNoConf").dataset.idNoConf;
    document.getElementById("detalleModalContent").classList.add("blur-modal");

    fetch('/NoConformidades/ObtenerSupervisores')
        .then(response => response.json())
        .then(data => {
            const select = document.getElementById("selectSupervisor");
            select.innerHTML = '<option value="">Seleccione un supervisor</option>';
            data.forEach(user => {
                select.innerHTML += `<option value="${user.id}">${user.nombreUsuario}</option>`;
            });
        });

    const modal = new bootstrap.Modal(document.getElementById('modalReasignar'));
    modal.show();

    document.getElementById("btnConfirmarReasignar").onclick = function () {
        const idUsuario = document.getElementById("selectSupervisor").value;
        if (!idUsuario) return alert("Seleccioná un supervisor para reasignar.");
        confirmarReasignacion(idNoConf, idUsuario);
    };
}

document.getElementById('modalReasignar').addEventListener('hidden.bs.modal', function () {
    document.getElementById("detalleModalContent").classList.remove("blur-modal");
});

function confirmarReasignacion(idNoConf, idUsuario) {
    // 1) Debug: muestro en consola los parámetros
    console.log("confirmarReasignacion → idNoConformidad:", idNoConf, "idUsuario:", idUsuario);

    // 2) Construyo y muestro la URL completa
    const url = `/NoConformidades/Reasignar`
        + `?idNoConformidad=${encodeURIComponent(idNoConf)}`
        + `&idUsuario=${encodeURIComponent(idUsuario)}`;
    console.log("Llamando a:", url);

    // 3) Hago el fetch
    fetch(url, { method: 'POST' })
        .then(async response => {
            if (response.ok) {
                console.log("Reasignación exitosa");
                location.reload();
            } else {
                // leo el mensaje de error que venga del servidor
                const msg = await response.text();
                console.error("Error al reasignar (status " + response.status + "):", msg);
                alert("No se pudo reasignar:\n" + msg);
            }
        })
        .catch(error => {
            console.error("Error en fetch:", error);
            alert("Error de conexión con el servidor.");
        });
}


function editarNoConformidad() {
    const contenido = document.getElementById('contenidoDetalleNoConf');
    if (!contenido) return alert("No se encontró la información de la No Conformidad.");

    const idNoConf = contenido.dataset.idNoConf;

    const modal = bootstrap.Modal.getInstance(document.getElementById('modalDetalleNoConf'));
    modal.hide();

    window.location.href = `/NoConformidades/Create?id=${idNoConf}&volverADetalle=true`;
}

document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    const abrirId = urlParams.get("abrirDetalle");

    if (abrirId) {
        // ✅ Limpiar la URL después de abrir el modal
        const newUrl = window.location.origin + window.location.pathname;
        window.history.replaceState({}, document.title, newUrl);

        abrirDetalleNoConf(abrirId);
    }
});

window.irAFichaTecnica = function (idFichaTecnica) {
    window.location.href = `/FichasTecnicas/Detalle/${idFichaTecnica}`;
};

