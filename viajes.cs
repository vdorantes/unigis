using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace unigis
{
    class viajes
    {
        public void procesar()
        {
            unigisViajes.wsGetViajes data = new unigisViajes.wsGetViajes();
            data.Timeout = 10 * 60 * 1000;
            var json = data.getViajes();
            
            var JSSerializer = new JavaScriptSerializer();
            JSSerializer.MaxJsonLength = Int32.MaxValue;
            Viajes datos = JSSerializer.Deserialize<Viajes>(json);            

            if (datos.success)
            {
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (Viaje viaje in datos.collection_Viajes)
                    {
                        unigisws.CrearViajeResultado resultado = new unigisws.CrearViajeResultado();

                        resultado = unigis.CrearViaje4("1234",
                                                        viaje.Empresa,
                                                        viaje.Sucursal,
                                                        viaje.Operacion,
                                                        viaje.Dominio,
                                                        viaje.FechaViaje.Value,
                                                        //DateTime.ParseExact("07/06/2017 15:07:25", "dd/MM/yyyy HH:mm:ss", null),                                                        
                                                        viaje.Referencia,
                                                        "", viaje.transporte
                                                        , null,
                                                        viaje.depositoSalida,
                                                        viaje.depositoLlegada,
                                                        viaje.conductor,
                                                        "", "", "", "", "",
                                                        viaje.Paradas.ToArray());
                        if (resultado.IdViaje < 0)
                        {
                            Console.WriteLine("RutaEAD: " + viaje.Referencia + " ERROR: " + resultado.IdViaje.ToString());
                        }
                        else
                        {
                            string error = data.updateRutaId(viaje.Referencia, resultado.IdViaje.ToString());
                            Console.WriteLine("RutaEAD: " + viaje.Referencia + " ViajeId: " + resultado.IdViaje.ToString() + " Resultado Update: " + error);
                            if (error == "")
                            {
                                if (unigis.ActivarViaje("1234", resultado.IdViaje))
                                {
                                    Console.WriteLine("**VIAJE ACTIVADO**");
                                }
                                else
                                {
                                    Console.WriteLine("**ERROR ocurrió algún problema en la ejecución del servicio, posiblemente el Viaje requerido ya está iniciado, finalizado o no existe**");
                                }
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine(datos.msg);
            }
        }

        public void procesarViajesCerrados()
        {
            unigisViajes.wsGetViajes data = new unigisViajes.wsGetViajes();
            data.Timeout = 10 * 60 * 1000;
            var json = data.getViajesCerrados();
            ViajesCerrados datos = new JavaScriptSerializer().Deserialize<ViajesCerrados>(json);

            if (datos.success)
            {
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (ViajeCerrado viaje in datos.collection_Viajes)
                    {

                        Console.WriteLine("ViajeId: " + viaje.viajeId.ToString());
                        if (unigis.FinalizarViaje("1234", Convert.ToInt32(viaje.viajeId)))
                        {
                            Console.WriteLine("**VIAJE FINALIZADO**");
                        }
                        else
                        {
                            Console.WriteLine("**ERROR ocurrió algún problema en la ejecución del servicio, posiblemente el Viaje requerido ya está iniciado, finalizado o no existe**");
                        }                       
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine(datos.msg);
            }
        }

        [DataContract]
        [Serializable]
        private class ViajesCerrados
        {
            private bool _success = false;
            [DataMember]
            public bool success
            {
                get { return _success; }
                set { _success = value; }
            }

            private string _msg = "";
            [DataMember]
            public string msg
            {
                get { return _msg; }
                set { _msg = value; }
            }

            private List<ViajeCerrado> _collection_Viajes = null;
            [DataMember]
            public List<ViajeCerrado> collection_Viajes
            {
                get { return _collection_Viajes; }
                set { _collection_Viajes = value; }
            }
        }

        [DataContract]
        [Serializable]
        private class ViajeCerrado
        {
            private string _viajeId = "";
            [DataMember]
            public string viajeId
            {
                get { return _viajeId; }
                set { _viajeId = value; }
            }
        }


        [DataContract]
        [Serializable]
        private class Viajes
        {
            private bool _success = false;
            [DataMember]
            public bool success
            {
                get { return _success; }
                set { _success = value; }
            }

            private string _msg = "";
            [DataMember]
            public string msg
            {
                get { return _msg; }
                set { _msg = value; }
            }

            private List<Viaje> _collection_Viajes = null;
            [DataMember]
            public List<Viaje> collection_Viajes
            {
                get { return _collection_Viajes; }
                set { _collection_Viajes = value; }
            }
        }

        [DataContract]
        [Serializable]
        private class Viaje
        {
            private string _Empresa = "";
            [DataMember]
            public string Empresa
            {
                get { return _Empresa; }
                set { _Empresa = value; }
            }
            private string _Sucursal = "";
            [DataMember]
            public string Sucursal
            {
                get { return _Sucursal; }
                set { _Sucursal = value; }
            }
            private string _Operacion = "";
            [DataMember]
            public string Operacion
            {
                get { return _Operacion; }
                set { _Operacion = value; }
            }
            private string _Dominio = "";
            [DataMember]
            public string Dominio
            {
                get { return _Dominio; }
                set { _Dominio = value; }
            }
            private Nullable<DateTime> _FechaViaje = null;
            [DataMember]
            public Nullable<DateTime> FechaViaje
            {
                get { return _FechaViaje; }
                set { _FechaViaje = value; }
            }
            private string _Referencia = "";
            [DataMember]
            public string Referencia
            {
                get { return _Referencia; }
                set { _Referencia = value; }
            }
            private unigisws.Transporte _transporte = null;
            [DataMember]
            public unigisws.Transporte transporte
            {
                get { return _transporte; }
                set { _transporte = value; }
            }
            private unigisws.pDeposito _depositoSalida = null;
            [DataMember]
            public unigisws.pDeposito depositoSalida
            {
                get { return _depositoSalida; }
                set { _depositoSalida = value; }
            }

            private unigisws.pDeposito _depositoLlegada = null;
            [DataMember]
            public unigisws.pDeposito depositoLlegada
            {
                get { return _depositoLlegada; }
                set { _depositoLlegada = value; }
            }

            private unigisws.pConductor _conductor = null;
            [DataMember]
            public unigisws.pConductor conductor
            {
                get { return _conductor; }
                set { _conductor = value; }
            }

            private List<unigisws.pParada> _Paradas = null;
            [DataMember]
            public List<unigisws.pParada> Paradas
            {
                get { return _Paradas; }
                set { _Paradas = value; }
            }
        }
        [DataContract]
        [Serializable]
        private class pParada
        {

            private int secuenciaField;

            private pCliente clienteField;

            private string refDocumentoField;

            private string refDocumentoAdicionalField;

            private string refDocumentoPedidoField;

            private string tipoParadaField;

            private string descripcionField;

            private string direccionField;

            private string nroPuertaField;

            private string entreCallesField;

            private string barrioField;

            private string localidadField;

            private string partidoField;

            private string provinciaField;

            private string paisField;

            private double latitudField;

            private double longitudField;

            private double volumenField;

            private double pesoField;

            private double bultoField;

            private int valorField;

            private double palletsField;

            private string telefonoField;

            private string telefono2Field;

            private string telefono3Field;

            private string emailField;

            private int tiempoEstadiaField;

            private System.Nullable<int> inicioHorario1Field;

            private System.Nullable<int> finHorario1Field;

            private System.Nullable<int> inicioHorario2Field;

            private System.Nullable<int> finHorario2Field;

            private System.Nullable<double> distanciaField;

            private string observacionesField;

            private string varchar1Field;

            private string varchar2Field;

            private System.Nullable<bool> requiereControlField;

            private string valorControlField;

            private System.Nullable<System.DateTime> inicioHorarioPlanificadoField;

            private System.Nullable<System.DateTime> finHorarioPlanificadoField;

            [DataMember]
            public int Secuencia
            {
                get
                {
                    return this.secuenciaField;
                }
                set
                {
                    this.secuenciaField = value;
                }
            }

            [DataMember]
            public pCliente cliente
            {
                get
                {
                    return this.clienteField;
                }
                set
                {
                    this.clienteField = value;
                }
            }

            [DataMember]
            public string RefDocumento
            {
                get
                {
                    return this.refDocumentoField;
                }
                set
                {
                    this.refDocumentoField = value;
                }
            }

            [DataMember]
            public string RefDocumentoAdicional
            {
                get
                {
                    return this.refDocumentoAdicionalField;
                }
                set
                {
                    this.refDocumentoAdicionalField = value;
                }
            }

            [DataMember]
            public string RefDocumentoPedido
            {
                get
                {
                    return this.refDocumentoPedidoField;
                }
                set
                {
                    this.refDocumentoPedidoField = value;
                }
            }

            [DataMember]
            public string TipoParada
            {
                get
                {
                    return this.tipoParadaField;
                }
                set
                {
                    this.tipoParadaField = value;
                }
            }

            [DataMember]
            public string Descripcion
            {
                get
                {
                    return this.descripcionField;
                }
                set
                {
                    this.descripcionField = value;
                }
            }

            [DataMember]
            public string Direccion
            {
                get
                {
                    return this.direccionField;
                }
                set
                {
                    this.direccionField = value;
                }
            }

            [DataMember]
            public string NroPuerta
            {
                get
                {
                    return this.nroPuertaField;
                }
                set
                {
                    this.nroPuertaField = value;
                }
            }

            [DataMember]
            public string EntreCalles
            {
                get
                {
                    return this.entreCallesField;
                }
                set
                {
                    this.entreCallesField = value;
                }
            }

            [DataMember]
            public string Barrio
            {
                get
                {
                    return this.barrioField;
                }
                set
                {
                    this.barrioField = value;
                }
            }

            [DataMember]
            public string Localidad
            {
                get
                {
                    return this.localidadField;
                }
                set
                {
                    this.localidadField = value;
                }
            }

            [DataMember]
            public string Partido
            {
                get
                {
                    return this.partidoField;
                }
                set
                {
                    this.partidoField = value;
                }
            }

            [DataMember]
            public string Provincia
            {
                get
                {
                    return this.provinciaField;
                }
                set
                {
                    this.provinciaField = value;
                }
            }

            [DataMember]
            public string Pais
            {
                get
                {
                    return this.paisField;
                }
                set
                {
                    this.paisField = value;
                }
            }

            [DataMember]
            public double Latitud
            {
                get
                {
                    return this.latitudField;
                }
                set
                {
                    this.latitudField = value;
                }
            }

            [DataMember]
            public double Longitud
            {
                get
                {
                    return this.longitudField;
                }
                set
                {
                    this.longitudField = value;
                }
            }

            [DataMember]
            public double Volumen
            {
                get
                {
                    return this.volumenField;
                }
                set
                {
                    this.volumenField = value;
                }
            }

            [DataMember]
            public double Peso
            {
                get
                {
                    return this.pesoField;
                }
                set
                {
                    this.pesoField = value;
                }
            }

            [DataMember]
            public double Bulto
            {
                get
                {
                    return this.bultoField;
                }
                set
                {
                    this.bultoField = value;
                }
            }

            [DataMember]
            public int Valor
            {
                get
                {
                    return this.valorField;
                }
                set
                {
                    this.valorField = value;
                }
            }

            [DataMember]
            public double Pallets
            {
                get
                {
                    return this.palletsField;
                }
                set
                {
                    this.palletsField = value;
                }
            }

            [DataMember]
            public string Telefono
            {
                get
                {
                    return this.telefonoField;
                }
                set
                {
                    this.telefonoField = value;
                }
            }

            [DataMember]
            public string Telefono2
            {
                get
                {
                    return this.telefono2Field;
                }
                set
                {
                    this.telefono2Field = value;
                }
            }

            [DataMember]
            public string Telefono3
            {
                get
                {
                    return this.telefono3Field;
                }
                set
                {
                    this.telefono3Field = value;
                }
            }

            [DataMember]
            public string Email
            {
                get
                {
                    return this.emailField;
                }
                set
                {
                    this.emailField = value;
                }
            }

            [DataMember]
            public int TiempoEstadia
            {
                get
                {
                    return this.tiempoEstadiaField;
                }
                set
                {
                    this.tiempoEstadiaField = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<int> InicioHorario1
            {
                get
                {
                    return this.inicioHorario1Field;
                }
                set
                {
                    this.inicioHorario1Field = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<int> FinHorario1
            {
                get
                {
                    return this.finHorario1Field;
                }
                set
                {
                    this.finHorario1Field = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<int> InicioHorario2
            {
                get
                {
                    return this.inicioHorario2Field;
                }
                set
                {
                    this.inicioHorario2Field = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<int> FinHorario2
            {
                get
                {
                    return this.finHorario2Field;
                }
                set
                {
                    this.finHorario2Field = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<double> Distancia
            {
                get
                {
                    return this.distanciaField;
                }
                set
                {
                    this.distanciaField = value;
                }
            }

            [DataMember]
            public string Observaciones
            {
                get
                {
                    return this.observacionesField;
                }
                set
                {
                    this.observacionesField = value;
                }
            }

            [DataMember]
            public string Varchar1
            {
                get
                {
                    return this.varchar1Field;
                }
                set
                {
                    this.varchar1Field = value;
                }
            }

            [DataMember]
            public string Varchar2
            {
                get
                {
                    return this.varchar2Field;
                }
                set
                {
                    this.varchar2Field = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<bool> RequiereControl
            {
                get
                {
                    return this.requiereControlField;
                }
                set
                {
                    this.requiereControlField = value;
                }
            }

            [DataMember]
            public string ValorControl
            {
                get
                {
                    return this.valorControlField;
                }
                set
                {
                    this.valorControlField = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<System.DateTime> InicioHorarioPlanificado
            {
                get
                {
                    return this.inicioHorarioPlanificadoField;
                }
                set
                {
                    this.inicioHorarioPlanificadoField = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<System.DateTime> FinHorarioPlanificado
            {
                get
                {
                    return this.finHorarioPlanificadoField;
                }
                set
                {
                    this.finHorarioPlanificadoField = value;
                }
            }
        }

        [DataContract]
        [Serializable]
        private class pCliente
        {

            private string refClienteField;

            private string razonSocialField;

            [DataMember]
            public string RefCliente
            {
                get
                {
                    return this.refClienteField;
                }
                set
                {
                    this.refClienteField = value;
                }
            }

            [DataMember]
            public string RazonSocial
            {
                get
                {
                    return this.razonSocialField;
                }
                set
                {
                    this.razonSocialField = value;
                }
            }

        }

    }
}
