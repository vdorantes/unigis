using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace unigis
{
    class EstadoParada
    {
        public void procesar()
        {
            unigisEstadosParadas.wsGetEstadosParadas data = new unigisEstadosParadas.wsGetEstadosParadas();
            var json = data.getEstadosParadas();
            var JSSerializer = new JavaScriptSerializer();
            JSSerializer.MaxJsonLength = Int32.MaxValue;            
            EstadosParadas datos = JSSerializer.Deserialize<EstadosParadas>(json);

            if (datos.success)
            {
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (unigisws.pEstadoParada estado in datos.collection)
                    {                                             
                        int resultado = unigis.ModificarEstadoParada("1234",
                                                        estado);

                        Console.WriteLine("Guia: " + estado.RefDocumento+ " ViajeId: " + estado.IdViaje.ToString() + " Resultado Update: " + resultado);

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
        private class EstadosParadas
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

            private List<unigisws.pEstadoParada> _collection = null;
            [DataMember]
            public List<unigisws.pEstadoParada> collection
            {
                get { return _collection; }
                set { _collection = value; }
            }
        }

        [DataContract]
        [Serializable]
        private class pEstadoParada
        {

            private string refDocumentoField;

            private string estadoField;

            private string motivoField;

            private System.DateTime estadoFechaField;

            private string observacionesField;

            private double latitudField;

            private double longitudField;

            private System.Nullable<int> idViajeField;

            private System.Nullable<int> idParadaTraceEstadoField;

            private string referenciaViajeField;

            private string loginField;

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
            public string Estado
            {
                get
                {
                    return this.estadoField;
                }
                set
                {
                    this.estadoField = value;
                }
            }

            [DataMember]
            public string Motivo
            {
                get
                {
                    return this.motivoField;
                }
                set
                {
                    this.motivoField = value;
                }
            }

            [DataMember]
            public System.DateTime EstadoFecha
            {
                get
                {
                    return this.estadoFechaField;
                }
                set
                {
                    this.estadoFechaField = value;
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
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<int> IdViaje
            {
                get
                {
                    return this.idViajeField;
                }
                set
                {
                    this.idViajeField = value;
                }
            }

            [DataMember]
            [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            public System.Nullable<int> IdParadaTraceEstado
            {
                get
                {
                    return this.idParadaTraceEstadoField;
                }
                set
                {
                    this.idParadaTraceEstadoField = value;
                }
            }

            [DataMember]
            public string ReferenciaViaje
            {
                get
                {
                    return this.referenciaViajeField;
                }
                set
                {
                    this.referenciaViajeField = value;
                }
            }

            [DataMember]
            public string Login
            {
                get
                {
                    return this.loginField;
                }
                set
                {
                    this.loginField = value;
                }
            }


        }

    }
}
