using System;
using System;
using System.Collections.Generic;
using TesisPractica.Models; // para poder usar Cliente, Pieza, etc.

namespace TesisPractica.ViewModels
{
    public class NoConformidadCreateViewModel
    {
        // Datos generales (ya existentes)
        public int? IdNoConformidad { get; set; }
        public int? IdCliente { get; set; }
        public List<Cliente> ListaClientes { get; set; } = new List<Cliente>();

        // Campos para el lado izquierdo
        public int? IdPieza { get; set; }
        public List<Pieza> ListaPiezas { get; set; } = new List<Pieza>();
        public int? Cantidad { get; set; }
        public List<int> IdCausa { get; set; } = new List<int>();

        public List<Causa> ListaCausas { get; set; } = new List<Causa>();
    
        public int? IdProceso { get; set; }
        public List<Proceso> ListaProcesos { get; set; } = new List<Proceso>();
        public bool Recurrente { get; set; }
        public int? Frecuencia { get; set; }
        public int? IdDetectabilidad { get; set; }
        public List<Detectabilidad> ListaDetectabilidades { get; set; } = new List<Detectabilidad>();
        public List<int> IdDefecto { get; set; } = new List<int>();

        public List<Defecto> ListaDefectos { get; set; } = new List<Defecto>();



        // Campos para el lado derecho
        public DateTime FechaIncidente { get; set; }
        public DateTime FechaProduccion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int? IdPrioridad { get; set; }
        public List<Prioridad> ListaPrioridades { get; set; } = new List<Prioridad>();
        public int? IdEstado { get; set; }
        public List<Estado> ListaEstados { get; set; } = new List<Estado>();
        public string? Gravedad { get; set; }

        public string Consecuencia { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
