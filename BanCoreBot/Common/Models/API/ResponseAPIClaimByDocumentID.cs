using System;

namespace BanCoreBot.Common.Models.API
{
    public class ResponseAPIClaimByDocumentID
    {
        public EnvelopeDocumentID Envelope { get; set; }
    }

    public class EnvelopeDocumentID
    {
        public BodyDocumentID Body { get; set; }
    }
    public class BodyDocumentID
    {
        public ConsultarreclamosariaresponseDocumentID consultarReclamosAriaResponse { get; set; }
    }


    public class ConsultarreclamosariaresponseDocumentID
    {
        public OutDocumentID _out { get; set; }
    }

    public class OutDocumentID
    {
        public ArrayreclamoDocumentID arrayReclamo { get; set; }
        public int codigoError { get; set; }
        public string descripcionError { get; set; }
        public ListareclamosDocumentID listaReclamos { get; set; }
        public int secuencial { get; set; }
    }

    public class ArrayreclamoDocumentID
    {
        public string nil { get; set; }
    }

    public class ListareclamosDocumentID
    {
        public ReclamoariadtoDocumentID[] ReclamoAriaDTO { get; set; }
    }

    public class ReclamoariadtoDocumentID
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
