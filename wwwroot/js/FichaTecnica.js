document.addEventListener("DOMContentLoaded", () => {
    //
    // 1) Fecha mínima y bloqueo de escritura manual
    //
    const inputFecha = document.getElementById("fechaFinEstimada");
    if (inputFecha) {
        const hoy = new Date();
        const yyyy = hoy.getFullYear();
        const mm = String(hoy.getMonth() + 1).padStart(2, "0");
        const dd = String(hoy.getDate()).padStart(2, "0");
        const hoyStr = `${yyyy}-${mm}-${dd}`;

        inputFecha.setAttribute("min", hoyStr);
        inputFecha.onkeydown = () => false; // bloquea teclear
        console.log("⏳ Fecha mínima establecida:", hoyStr);
    }

    //
    // 2) Función para actualizar fecha límite (POST JSON)
    //
    window.actualizarFechaLimite = function (idFicha) {
        const nuevaFecha = document.getElementById("fechaFinEstimada").value;
        fetch(`/FichasTecnicas/ActualizarFecha`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ idFichaTecnica: idFicha, nuevaFecha }),
        })
            .then((response) => {
                if (!response.ok) throw new Error("Error al guardar la fecha");
                return response.text();
            })
            .then((msg) => console.log("✅ Fecha guardada:", msg))
            .catch((err) => {
                console.error("❌ Error:", err);
                alert("No se pudo guardar la fecha límite.");
            });
    };

    //
    // 3) Función para mostrar detalles de No Conformidad en modal
    //
    window.abrirInfoNoConformidad = function (idNoConf) {
        fetch(`/FichasTecnicas/ObtenerDetalleNoConformidad?id=${idNoConf}`)
            .then((res) => res.json())
            .then((data) => {
                document.getElementById("tituloNC").innerText = `NC-${data.id}`;

                const html = `
          <div class="row">
            <div class="col-md-6">
              <p><strong>Pieza:</strong> ${data.descripcionPieza}</p>
              <p><strong>Descripción:</strong> ${data.descripcion}</p>
              <p><strong>Cantidad:</strong> ${data.cantidad}</p>
              <p><strong>Consecuencia:</strong> ${data.consecuencia}</p>
              <p><strong>Estado:</strong> ${data.estado}</p>
              <p><strong>Prioridad:</strong> ${data.prioridad}</p>
              <p><strong>Frecuencia:</strong> ${data.frecuencia}</p>
              <p><strong>Gravedad:</strong> ${data.gravedad}</p>
            </div>
            <div class="col-md-6">
              <p><strong>Proceso:</strong> ${data.proceso}</p>
              <p><strong>Detectabilidad:</strong> ${data.detectabilidad}</p>
              <p><strong>Fecha creación:</strong> ${data.fechaCreacion}</p>
              <p><strong>Fecha incidente:</strong> ${data.fechaIncidente}</p>
              <p><strong>Fecha producción:</strong> ${data.fechaProduccion}</p>
              <p><strong>Fecha finalización:</strong> ${data.fechaFinalizacion ?? "—"}</p>
              <p><strong>Causas:</strong> ${data.causas?.join(", ") || "—"}</p>
              <p><strong>Defectos:</strong> ${data.defectos?.join(", ") || "—"}</p>
            </div>
          </div>
        `;
                document.getElementById("contenidoModalNC").innerHTML = html;
                new bootstrap.Modal(
                    document.getElementById("modalInfoNC")
                ).show();
            })
            .catch((err) => {
                console.error(err);
                alert("No se pudo cargar la información de la no conformidad.");
            });
    };

    //
    // 4) Modal “Asignar Equipo”
    //
    const modalAsignar = new bootstrap.Modal(
        document.getElementById("modalAsignarEquipo")
    );

    // Abrir modal desde el botón
    window.abrirModalAsignarEquipo = function (idFicha) {
        document
            .getElementById("btnConfirmarAsignar")
            .dataset.idFicha = idFicha;
        modalAsignar.show();
    };

    // Confirmar asignación
    const btnConfirmar = document.getElementById("btnConfirmarAsignar");
    if (btnConfirmar) {
        btnConfirmar.addEventListener("click", async function () {
            const idFicha = this.dataset.idFicha;
            const idEquipo = document.getElementById("selectEquipos").value;
            if (!idEquipo) return alert("Por favor seleccioná un equipo.");

            const token = document.querySelector(
                'input[name="__RequestVerificationToken"]'
            ).value;

            const form = new FormData();
            form.append("idFicha", idFicha);
            form.append("idEquipo", idEquipo);
            form.append("__RequestVerificationToken", token);

            try {
                const resp = await fetch("/FichasTecnicas/AsignarEquipo", {
                    method: "POST",
                    body: form,
                });
                if (!resp.ok) {
                    const text = await resp.text();
                    console.error("Error servidor:", resp.status, text);
                    return alert("Ocurrió un error al asignar el equipo.");
                }
                await resp.json();
                modalAsignar.hide();
                window.location.reload();
            } catch (e) {
                console.error(e);
                alert("Ocurrió un error al asignar el equipo.");
            }
        });
    }
});
