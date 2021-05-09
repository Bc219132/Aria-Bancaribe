

namespace BanCoreBot.Dialogs.Reclamos
{
    public class EnumStatusClaim
    {
        public const string
           A = "Tu reclamo se encuentra en proceso de atención por el Banco",
           PD = "Tu reclamo se encuentra en validación por el Banco",
           D = "Tu reclamo se encuentra en validación por el Banco",
           PV = "Tu reclamo se encuentra en verificación y durante los próximos días te estaremos emitiendo respuesta",
           V = "Tu reclamo se encuentra en verificación y durante los próximos días te estaremos emitiendo respuesta",
           C20 = "Tu reclamo fue favorable y se realizó el abono en tu cuenta",
           C21 = "Tu reclamo fue No Favorable y fue enviado a tu email la notificación que sustenta el dictamen del banco",
           C50 = "Tu reclamo fue Favorable y se realizó el abono en tu cuenta antes de la fecha del reclamo; te invito a consultar tu estado de cuenta desde la fecha en que realizaste el reclamo",
           CA20 = "Tu reclamo ha sido favorable y ya tienes el abono en cuenta, te invito a consultar tu estado de cuenta desde la fecha en que realizaste el reclamo",
           CA22 = "Tu reclamo ha sido favorable y ya tienes el abono en cuenta, te invito a consultar tu estado de cuenta desde la fecha en que realizaste el reclamo",
           CN21 = "Tu reclamo fue No Favorable y fue enviado a tu email la notificación que sustenta el dictamen del banco",
           CNA20 = "Tu reclamo ha sido favorable y tu cuenta presenta una condición que no permitió realizar el abono en cuenta",
           CPN21 = "Tu reclamo fue No Favorable y fue enviada a tu email la notificación que sustenta el dictamen del banco";
    }
}
