using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace unigis
{
    class Rutas
    {
        public void obtenerRutas()
        {
            unigisRutas.wsRutas data = new unigisRutas.wsRutas();
            var json = data.getProgramacion();
            var JSSerializer = new JavaScriptSerializer();
            JSSerializer.MaxJsonLength = Int32.MaxValue;            
            programaciones datos = JSSerializer.Deserialize<programaciones>(json);

            if (datos.success)
            {
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (programacion programacion in datos.collection)
                    {                        
                        unigisws.pRuta[] resultado = unigis.ObtenerRutas("1234", DateTime.ParseExact(programacion.fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture), programacion.sucursalId);
                        foreach (unigisws.pRuta ruta in resultado)
                        {
                            if (ruta.Vehiculo != null /*&& ruta.Estado.ToLower() == "planeada"*/)
                            {
                                unigisws.pVehiculo vehiculo = ruta.Vehiculo;

                                Console.WriteLine("RutaIdUNIGIS: " + ruta.IdRuta
                                    + " ViajeId: " + ruta.IdViaje.ToString()
                                    + " Estado: " + ruta.Estado.ToString()
                                    + " EstadoJornada: " + ruta.EstadoJornada.ToString()
                                    + " FechaHoraCarga: " + ruta.FechaHoraCarga.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                                    //+ " Operador: " + vehiculo.Conductor.ToString()
                                    + " Dominio: " + vehiculo.Dominio.ToString()
                                    + " Eco: " + vehiculo.ReferenciaExterna.ToString()
                                    );

                                //limpiar Ruta
                                unigisRutas.wsRutas operacionesRuta = new unigisRutas.wsRutas();
                                string error = operacionesRuta.deleteRuta(ruta.IdRuta.ToString());

                                if (error == "")
                                {
                                    //insertar ruta con detalle de guias
                                    Console.WriteLine("**ORDENES**");
                                    unigisws.pOrdenEntrega[] ordenes = ruta.Ordenes;
                                    foreach (unigisws.pOrdenEntrega orden in ordenes)
                                    {
                                        error = operacionesRuta.insertRuta(ruta.IdRuta.ToString()
                                            , ruta.FechaHoraCarga.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                                            , vehiculo.Dominio.ToString()
                                            , vehiculo.ReferenciaExterna.ToString()
                                            , orden.RefDocumento
                                            , orden.Tipo
                                            , ruta.Estado.ToString()
                                            , orden.Orden.ToString()
                                            , orden.Latitud.Value
                                            , orden.Longitud.Value);
                                        Console.WriteLine("Guia: " + orden.RefDocumento + " Servicio: " + orden.Tipo + " Error: " + error + " ORDEN: "+ orden.Orden.ToString()
                                            + " lat: "+ orden.Latitud.Value.ToString()
                                            + " lon: " + orden.Longitud.Value.ToString());
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error: " + error);
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
        }

        [DataContract]
        [Serializable]
        private class programaciones
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

            private List<programacion> _collection = null;
            [DataMember]
            public List<programacion> collection
            {
                get { return _collection; }
                set { _collection = value; }
            }
        }

        [DataContract]
        [Serializable]
        private class programacion
        {
            private string _sucursalId = "";
            [DataMember]
            public string sucursalId
            {
                get { return _sucursalId; }
                set { _sucursalId = value; }
            }

            private string _fecha = "";
            [DataMember]
            public string fecha
            {
                get { return _fecha; }
                set { _fecha = value; }
            }
        }
    }
}
