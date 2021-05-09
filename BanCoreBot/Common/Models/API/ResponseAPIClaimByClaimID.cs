using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanCoreBot.Common.Models.API
{

    public class ResponseAPIClaimByClaimID
    {
        public Envelope Envelope { get; set; }
    }

    public class Envelope
    {
        public Body Body { get; set; }
    }

    public class Body
    {
        public Consultarreclamosariaresponse consultarReclamosAriaResponse { get; set; }
    }

    public class Consultarreclamosariaresponse
    {
        public Out _out { get; set; }
    }

    public class Out
    {
        public Arrayreclamo arrayReclamo { get; set; }
        public int codigoError { get; set; }
        public string descripcionError { get; set; }
        public Listareclamos listaReclamos { get; set; }
        public int secuencial { get; set; }
    }

    public class Arrayreclamo
    {
        public string nil { get; set; }
    }

    public class Listareclamos
    {
        public Reclamoariadto ReclamoAriaDTO { get; set; }
    }

    public class Reclamoariadto
    {
        public string areaTramitadora { get; set; }
        public string categoria { get; set; }
        public string cedula { get; set; }
        public int codArea { get; set; }
        public int codCategoria { get; set; }
        public string codEstado { get; set; }
        public int codEstatus { get; set; }
        public int codTipoReclamo { get; set; }
        public string decTipoReclamo { get; set; }
        public DateTime fechaReclamo { get; set; }
        public string montoReclamo { get; set; }
        public object nombre { get; set; }
        public int nroReclamo { get; set; }
    }

}
