using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Configuration;

namespace unigis
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string EsActualizarEstadosGuia = ConfigurationManager.AppSettings["EsActualizarEstadosGuia"].ToString();
                string EsEnviarGuias = ConfigurationManager.AppSettings["EsEnviarGuias"].ToString();
                string EsCrearViajes = ConfigurationManager.AppSettings["EsCrearViajes"].ToString();
                string EsModificarEstadosParada = ConfigurationManager.AppSettings["EsModificarEstadosParada"].ToString();
                string EsAgregarParadaViaje = ConfigurationManager.AppSettings["EsAgregarParadaViaje"].ToString();
                string EsObtenerRutasPlaneadas = ConfigurationManager.AppSettings["EsObtenerRutasPlaneadas"].ToString();


                if (EsActualizarEstadosGuia == "1")
                {
                    estadosGuia estadosGuia = new estadosGuia();
                    estadosGuia.procesar();
                }

                if (EsEnviarGuias == "1")
                {
                    enviarGuias();
                }

                if (EsCrearViajes == "1")
                {
                    viajes viajes = new viajes();
                    viajes.procesar();
                    viajes.procesarViajesCerrados();
                }

                if (EsModificarEstadosParada == "1")
                {
                    EstadoParada estadoParada = new EstadoParada();
                    estadoParada.procesar();
                }

                if (EsAgregarParadaViaje == "1")
                {
                    procesarAgregarParada();
                }

                if (EsObtenerRutasPlaneadas == "1")
                {
                    Rutas rutas = new Rutas();
                    rutas.obtenerRutas();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.InnerException.Message);
            }

            //Console.ReadLine();
        }

        static void enviarGuias()
        {
            string fechaCortediasAdicionales = ConfigurationManager.AppSettings["fechaCortediasAdicionales"];
            string fechaCorteHora = ConfigurationManager.AppSettings["fechaCorteHora"];
            string pathLog = ConfigurationManager.AppSettings["pathLog"].ToString();

            //Crear instancia para guardar log
            utils utils = new utils();

            int contador = 1;
            string date = "";
            DateTime dt = DateTime.Now;
            unigisData.wsGetGuias data = new unigisData.wsGetGuias();
            date = dt.AddDays(Convert.ToDouble(fechaCortediasAdicionales)).ToString("yyyyMMdd") + " " + fechaCorteHora;
            //date = dt.ToString("yyyyMMdd") + " 12:00:00";
            data.Timeout = 10 * 60 * 1000;
            var json = data.getGuias(date);
            var JSSerializer = new JavaScriptSerializer();
            JSSerializer.MaxJsonLength = Int32.MaxValue;
            Orders ordersData = JSSerializer.Deserialize<Orders>(json);

            if (ordersData.success)
            {
                string guid_registro = "";
                string prevReg = "";
                List<unigisws.pOrdenPedido> orders = new List<unigisws.pOrdenPedido>();
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (var orderData in ordersData.collection_Orders)
                    {

                        unigisws.pOrdenPedido order = new unigisws.pOrdenPedido();
                        unigisws.pOrdenPedidoItem orderItem = new unigisws.pOrdenPedidoItem();
                        unigisws.pCliente clientData = new unigisws.pCliente();
                        unigisws.pProducto productData = new unigisws.pProducto();
                        unigisws.CampoValor dynamicCamp = new unigisws.CampoValor();
                        List<unigisws.pOrdenPedidoItem> orderItems = new List<unigisws.pOrdenPedidoItem>();
                        List<unigisws.CampoValor> dynamicCamps = new List<unigisws.CampoValor>();

                        try
                        {
                            guid_registro = orderData.guid_registro;

                            if (orderData.RefDocumento == prevReg)
                            {
                                int index = orders.FindLastIndex(o => o.RefDocumento == orderData.RefDocumento);
                                orderItem.RefDocumento = orderData.Items_pOrdenPedidoItem_RefDocumento;
                                orderItem.Descripcion = orderData.Items_pOrdenPedidoItem_Producto_Descripcion;

                                productData.RefProducto = orderData.Items_pOrdenPedidoItem_Producto_RefProducto;
                                productData.Descripcion = orderData.Items_pOrdenPedidoItem_Producto_Descripcion;
                                productData.Alto = orderData.Items_pOrdenPedidoItem_Producto_Alto;
                                productData.Ancho = orderData.Items_pOrdenPedidoItem_Producto_Ancho;
                                productData.Profundidad = orderData.Items_pOrdenPedidoItem_Producto_Profundidad;
                                productData.Rotacion = orderData.Items_pOrdenPedidoItem_Producto_RotacionPermitida;
                                productData.RotacionesPermitidas = orderData.Items_pOrdenPedidoItem_Producto_RotacionPermitida;
                                productData.Apilable = orderData.Items_pOrdenPedidoItem_Producto_Apilable;
                                productData.Peso = orderData.Items_pOrdenPedidoItem_Peso;
                                productData.Volumen = orderData.Items_pOrdenPedidoItem_Volumen;
                                orderItem.Producto = productData;

                                if (orderData.Items_pOrdenPedidoItem_Cantidad.HasValue)
                                {
                                    orderItem.Cantidad = orderData.Items_pOrdenPedidoItem_Cantidad.Value;
                                }
                                orderItem.Volumen = orderData.Items_pOrdenPedidoItem_Volumen;
                                orderItem.Peso = orderData.Items_pOrdenPedidoItem_Peso;
                                orderItem.Bulto = orderData.Items_pOrdenPedidoItem_Bulto;
                                orderItem.Pallets = orderData.Items_pOrdenPedidoItem_Pallets;
                                orderItem.ImporteCosto = orderData.Items_pOrdenPedidoItem_ImporteCosto;
                                orderItem.FechaEntrega = orderData.Items_pOrdenPedidoItem_FechaEntrega;
                                //Getting Existing Items from Order
                                var existingItems = orders[index].Items.ToArray();
                                for (var i = 0; i < existingItems.Length; i++)
                                {
                                    orderItems.Add(existingItems[i]);
                                }
                                //Adding the new one
                                orderItems.Add(orderItem);
                                orders[index].Items = orderItems.ToArray();
                            }

                            if (contador > 1)
                            {
                                Console.WriteLine((contador - 1).ToString() + " " + prevReg);
                            }

                            if (((orderData.RefDocumento != prevReg) && (prevReg != "")) || ((contador == ordersData.collection_Orders.Count()) && (ordersData.collection_Orders.Count() > 1)))
                            {
                                //sincronizar registro
                                int resp = unigis.CrearOrdenesPedido("10:Feb:2017 \"This is an Example!\"", orders.ToArray());
                                Console.WriteLine(resp.ToString());
                                if (resp == 1)
                                {
                                    string error = data.updateGuiaCargada(orders[0].RefDocumento, guid_registro);
                                    Console.WriteLine(error);
                                }
                                //inicializar orden
                                orders = new List<unigisws.pOrdenPedido>();
                            }

                            if (orderData.RefDocumento != prevReg)
                            {
                                order.Tipo = orderData.TipoGuia;
                                order.RefDocumento = orderData.RefDocumento;
                                //Se comenta a peticion del proveedor
                                if (orderData.FechaEntrega.HasValue)
                                {
                                    //nuevaParada.Fecha = orderData.Fecha.Value;
                                    order.Fecha = orderData.FechaEntrega.Value;
                                }
                                order.FechaEntrega = orderData.FechaEntrega;
                                order.FechaEntregaOriginal = orderData.FechaEntregaOriginal;
                                clientData.RefCliente = orderData.Cliente_RefCliente;

                                clientData.RazonSocial = orderData.Cliente_RazonSocial;
                                clientData.Telefono = orderData.Cliente_Telefono;
                                clientData.EMail = orderData.Cliente_Email;
                                clientData.Contacto = orderData.Cliente_Contacto;
                                clientData.Direccion = orderData.Cliente_Direccion;
                                clientData.Calle = orderData.Cliente_Calle;
                                clientData.NumeroPuerta = orderData.Cliente_NumeroPuerta;
                                clientData.Barrio = orderData.Cliente_Barrio;
                                clientData.Localidad = orderData.Cliente_Localidad;
                                clientData.Partido = orderData.Cliente_Partido;
                                clientData.Provincia = orderData.Cliente_Provincia;
                                clientData.Pais = orderData.Cliente_Pais;

                                //clientData.Latitud = orderData.Cliente_Latitud;
                                //clientData.Longitud = orderData.Cliente_Longitud;
                                clientData.RefDomicilioExterno = orderData.Cliente_RefDomicilioExterno;


                                clientData.DomicilioDescripcion = orderData.Cliente_DomicilioDescripcion;
                                clientData.InicioHorario1 = orderData.Cliente_InicioHorario1;
                                clientData.FinHorario1 = orderData.Cliente_FinHorario1;
                                clientData.TiempoEspera = orderData.Cliente_TiempoEspera;
                                order.Cliente = clientData;
                                order.Telefono = orderData.Telefono;
                                order.Descripcion = orderData.Descripcion;
                                order.CodigoSucursal = orderData.CodigoSucursal;
                                order.CodigoOperacion = orderData.CodigoSucursal;
                                order.TipoPedido = orderData.TipoPedido;
                                order.Estado = orderData.Estado;
                                order.Direccion = orderData.Direccion;
                                order.Calle = orderData.Calle;
                                order.NroPuerta = orderData.NroPuerta;
                                order.Barrio = orderData.Barrio;
                                order.Localidad = orderData.Localidad;
                                order.Partido = orderData.Partido;
                                order.Provincia = orderData.Provincia;
                                order.Pais = orderData.Pais;
                                order.CodigoPostal = orderData.CodigoPostal;
                                order.InicioHorario1 = orderData.InicioHorario1;
                                order.FinHorario1 = orderData.FinHorario1;
                                order.TiempoEspera = orderData.TiempoEspera;
                                order.Volumen = orderData.Volumen;
                                order.Peso = orderData.Peso;
                                order.Bulto = orderData.Bulto;
                                order.Pallets = orderData.Pallets;
                                //nuevaParada.Latitud = orderData.Latitud;
                                //nuevaParada.Longitud = orderData.Longitud;
                                order.Observaciones = orderData.Observaciones;
                                order.Email = orderData.Email;
                                order.Categoria = orderData.Categoria;
                                order.Prioridad = orderData.Prioridad;
                                order.cargaExclusiva = orderData.cargaExclusiva == "NO" ? false : true;
                                orderItem.RefDocumento = orderData.Items_pOrdenPedidoItem_RefDocumento;

                                productData.RefProducto = orderData.Items_pOrdenPedidoItem_Producto_RefProducto;
                                productData.Descripcion = orderData.Items_pOrdenPedidoItem_Producto_Descripcion;
                                productData.Alto = orderData.Items_pOrdenPedidoItem_Producto_Alto;
                                productData.Ancho = orderData.Items_pOrdenPedidoItem_Producto_Ancho;
                                productData.Profundidad = orderData.Items_pOrdenPedidoItem_Producto_Profundidad;
                                productData.Rotacion = orderData.Items_pOrdenPedidoItem_Producto_RotacionPermitida;
                                productData.RotacionesPermitidas = orderData.Items_pOrdenPedidoItem_Producto_RotacionPermitida;
                                productData.Apilable = orderData.Items_pOrdenPedidoItem_Producto_Apilable;
                                productData.Peso = orderData.Items_pOrdenPedidoItem_Peso;
                                productData.Volumen = orderData.Items_pOrdenPedidoItem_Volumen;


                                orderItem.Producto = productData;
                                if (orderData.Items_pOrdenPedidoItem_Cantidad.HasValue)
                                {
                                    orderItem.Cantidad = orderData.Items_pOrdenPedidoItem_Cantidad.Value;
                                }
                                orderItem.Descripcion = orderData.Items_pOrdenPedidoItem_Producto_Descripcion;
                                orderItem.Volumen = orderData.Items_pOrdenPedidoItem_Volumen;
                                orderItem.Peso = orderData.Items_pOrdenPedidoItem_Peso;
                                orderItem.Bulto = orderData.Items_pOrdenPedidoItem_Bulto;
                                orderItem.Pallets = orderData.Items_pOrdenPedidoItem_Pallets;
                                orderItem.ImporteCosto = orderData.Items_pOrdenPedidoItem_ImporteCosto;
                                orderItem.FechaEntrega = orderData.Items_pOrdenPedidoItem_FechaEntrega;
                                orderItems.Add(orderItem);
                                order.Items = orderItems.ToArray();
                                dynamicCamp.Campo = orderData.CampoDinamico_Campo;
                                dynamicCamp.Valor = orderData.CampoDinamico_Valor;
                                dynamicCamps.Add(dynamicCamp);
                                order.CampoDinamico = dynamicCamps.ToArray();
                                orders.Add(order);
                            }

                            //sincronizar cuando el resultado sea un solo registro o el ultimo
                            if (((contador == 1) && (contador == ordersData.collection_Orders.Count()) && (ordersData.collection_Orders.Count() > 0))
                                || ((contador == ordersData.collection_Orders.Count()) && (ordersData.collection_Orders.Count() > 1)))
                            {
                                if ((contador == ordersData.collection_Orders.Count()) && (ordersData.collection_Orders.Count() > 1))
                                {
                                    Console.WriteLine(contador.ToString() + " " + prevReg);
                                }
                                //sincronizar registro
                                int resp = unigis.CrearOrdenesPedido("10:Feb:2017 \"This is an Example!\"", orders.ToArray());
                                Console.WriteLine(resp.ToString());
                                if (resp == 1)
                                {
                                    string error = data.updateGuiaCargada(orders[0].RefDocumento, guid_registro);
                                    Console.WriteLine(error);
                                }
                                //inicializar orden
                                orders = new List<unigisws.pOrdenPedido>();
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(orderData.RefDocumento + " " + e.Message + " " + utils.guardarLog(orderData.RefDocumento + ": " + e.Message, pathLog)); // nuevaParada error                            
                        }

                        contador++;
                        prevReg = orderData.RefDocumento; //Sticker anterior
                    }
                    //foreach (var k in orders)
                    //{
                    //    Console.Write(k.RefDocumento + Environment.NewLine);
                    //    for (int s = 0; s < k.Items.Length; s++)
                    //    {
                    //        Console.Write(k.Items[s].Volumen + Environment.NewLine);
                    //    }
                    //}

                    /*
                    int resp = unigis.CrearOrdenesPedido("10:Feb:2017 \"This is an Example!\"", orders.ToArray());
                    Console.WriteLine(resp);
                    */
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + utils.guardarLog(e.Message, pathLog)); // orders error
                }
            }
            else
            {
                Console.WriteLine(ordersData.msg);
            }
        }

        static void procesarAgregarParada()
        {
            unigisData.wsGetGuias data = new unigisData.wsGetGuias();
            var json = data.getNuevasParadas();
            NuevasParadas datos = new JavaScriptSerializer().Deserialize<NuevasParadas>(json);

            if (datos.success)
            {
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (NuevaParada nuevaParada in datos.collection_Paradas)
                    {
                        Console.WriteLine("Viaje: " + nuevaParada.viajeId.ToString());
                        unigisws.AgregarParadaViajeResponse[] paradaResponse = unigis.AgregarParadaViaje("1234", Convert.ToInt32(nuevaParada.viajeId), nuevaParada.Paradas.ToArray());

                        for (int i = 0; i < paradaResponse.Count(); i++)
                        {
                            if (paradaResponse[i].IdParada <= 0)
                            {
                                Console.WriteLine("ViajeId: " + nuevaParada.viajeId.ToString() + "Guía: " + paradaResponse[i].RefDocumento + " ERROR: " + paradaResponse[i].IdParada.ToString());
                            }
                            else
                            {
                                string error = data.updateParadaId(nuevaParada.viajeId, paradaResponse[i].IdParada.ToString(), paradaResponse[i].RefDocumento.ToString());
                                Console.WriteLine("ViajeId: " + nuevaParada.viajeId.ToString() + " ParadaId: " + paradaResponse[i].IdParada.ToString() + " Resultado Update: " + error);
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


        [DataContract]
        [Serializable]
        private class Orders
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

            private List<Order> _collection_Orders = null;
            [DataMember]
            public List<Order> collection_Orders
            {
                get { return _collection_Orders; }
                set { _collection_Orders = value; }
            }
        }

        [DataContract]
        [Serializable]
        private class Order
        {
            private string _TipoGuia = "";
            [DataMember]
            public string TipoGuia
            {
                get { return _TipoGuia; }
                set { _TipoGuia = value; }
            }

            private string _RefDocumento = "";
            [DataMember]
            public string RefDocumento
            {
                get { return _RefDocumento; }
                set { _RefDocumento = value; }
            }

            private Nullable<DateTime> _Fecha = null;
            [DataMember]
            public Nullable<DateTime> Fecha
            {
                get { return _Fecha; }
                set { _Fecha = value; }
            }

            private Nullable<DateTime> _FechaEntrega = null;
            [DataMember]
            public Nullable<DateTime> FechaEntrega
            {
                get { return _FechaEntrega; }
                set { _FechaEntrega = value; }
            }

            private Nullable<DateTime> _FechaEntregaOriginal = null;
            [DataMember]
            public Nullable<DateTime> FechaEntregaOriginal
            {
                get { return _FechaEntregaOriginal; }
                set { _FechaEntregaOriginal = value; }
            }

            private string _Cliente_RefCliente = null;
            [DataMember]
            public string Cliente_RefCliente
            {
                get { return _Cliente_RefCliente; }
                set { _Cliente_RefCliente = value; }
            }

            private string _Cliente_RazonSocial = "";
            [DataMember]
            public string Cliente_RazonSocial
            {
                get { return _Cliente_RazonSocial; }
                set { _Cliente_RazonSocial = value; }
            }

            private string _Cliente_Telefono = "";
            [DataMember]
            public string Cliente_Telefono
            {
                get { return _Cliente_Telefono; }
                set { _Cliente_Telefono = value; }
            }

            private string _Cliente_Email = "";
            [DataMember]
            public string Cliente_Email
            {
                get { return _Cliente_Email; }
                set { _Cliente_Email = value; }
            }

            private string _Cliente_Contacto = "";
            [DataMember]
            public string Cliente_Contacto
            {
                get { return _Cliente_Contacto; }
                set { _Cliente_Contacto = value; }
            }

            private string _Cliente_Direccion = "";
            [DataMember]
            public string Cliente_Direccion
            {
                get { return _Cliente_Direccion; }
                set { _Cliente_Direccion = value; }
            }

            private string _Cliente_Calle = "";
            [DataMember]
            public string Cliente_Calle
            {
                get { return _Cliente_Calle; }
                set { _Cliente_Calle = value; }
            }

            private string _Cliente_NumeroPuerta = "";
            [DataMember]
            public string Cliente_NumeroPuerta
            {
                get { return _Cliente_NumeroPuerta; }
                set { _Cliente_NumeroPuerta = value; }
            }

            private string _Cliente_Barrio = "";
            [DataMember]
            public string Cliente_Barrio
            {
                get { return _Cliente_Barrio; }
                set { _Cliente_Barrio = value; }
            }

            private string _Cliente_Localidad = "";
            [DataMember]
            public string Cliente_Localidad
            {
                get { return _Cliente_Localidad; }
                set { _Cliente_Localidad = value; }
            }

            private string _Cliente_Partido = "";
            [DataMember]
            public string Cliente_Partido
            {
                get { return _Cliente_Partido; }
                set { _Cliente_Partido = value; }
            }

            private string _Cliente_Provincia = "";
            [DataMember]
            public string Cliente_Provincia
            {
                get { return _Cliente_Provincia; }
                set { _Cliente_Provincia = value; }
            }

            private string _Cliente_Pais = "";
            [DataMember]
            public string Cliente_Pais
            {
                get { return _Cliente_Pais; }
                set { _Cliente_Pais = value; }
            }

            private Nullable<double> _Cliente_Latitud = null;
            [DataMember]
            public Nullable<double> Cliente_Latitud
            {
                get { return _Cliente_Latitud; }
                set { _Cliente_Latitud = value; }
            }

            private Nullable<double> _Cliente_Longitud = null;
            [DataMember]
            public Nullable<double> Cliente_Longitud
            {
                get { return _Cliente_Longitud; }
                set { _Cliente_Longitud = value; }
            }

            private string _Cliente_RefDomicilioExterno = "";
            [DataMember]
            public string Cliente_RefDomicilioExterno
            {
                get { return _Cliente_RefDomicilioExterno; }
                set { _Cliente_RefDomicilioExterno = value; }
            }

            private string _Cliente_DomicilioDescripcion = "";
            [DataMember]
            public string Cliente_DomicilioDescripcion
            {
                get { return _Cliente_DomicilioDescripcion; }
                set { _Cliente_DomicilioDescripcion = value; }
            }

            private Nullable<int> _Cliente_InicioHorario1 = null;
            [DataMember]
            public Nullable<int> Cliente_InicioHorario1
            {
                get { return _Cliente_InicioHorario1; }
                set { _Cliente_InicioHorario1 = value; }
            }

            private Nullable<int> _Cliente_FinHorario1 = null;
            [DataMember]
            public Nullable<int> Cliente_FinHorario1
            {
                get { return _Cliente_FinHorario1; }
                set { _Cliente_FinHorario1 = value; }
            }

            private Nullable<int> _Cliente_TiempoEspera = null;
            [DataMember]
            public Nullable<int> Cliente_TiempoEspera
            {
                get { return _Cliente_TiempoEspera; }
                set { _Cliente_TiempoEspera = value; }
            }

            private string _Telefono = "";
            [DataMember]
            public string Telefono
            {
                get { return _Telefono; }
                set { _Telefono = value; }
            }

            private string _Descripcion = "";
            [DataMember]
            public string Descripcion
            {
                get { return _Descripcion; }
                set { _Descripcion = value; }
            }

            private string _CodigoSucursal = "";
            [DataMember]
            public string CodigoSucursal
            {
                get { return _CodigoSucursal; }
                set { _CodigoSucursal = value; }
            }

            private string _TipoPedido = "";
            [DataMember]
            public string TipoPedido
            {
                get { return _TipoPedido; }
                set { _TipoPedido = value; }
            }

            private string _Estado = "";
            [DataMember]
            public string Estado
            {
                get { return _Estado; }
                set { _Estado = value; }
            }

            private string _Direccion = "";
            [DataMember]
            public string Direccion
            {
                get { return _Direccion; }
                set { _Direccion = value; }
            }

            private string _Calle = "";
            [DataMember]
            public string Calle
            {
                get { return _Calle; }
                set { _Calle = value; }
            }

            private string _NroPuerta = "";
            [DataMember]
            public string NroPuerta
            {
                get { return _NroPuerta; }
                set { _NroPuerta = value; }
            }

            private string _Barrio = "";
            [DataMember]
            public string Barrio
            {
                get { return _Barrio; }
                set { _Barrio = value; }
            }

            private string _Localidad = "";
            [DataMember]
            public string Localidad
            {
                get { return _Localidad; }
                set { _Localidad = value; }
            }

            private string _Partido = "";
            [DataMember]
            public string Partido
            {
                get { return _Partido; }
                set { _Partido = value; }
            }

            private string _Provincia = "";
            [DataMember]
            public string Provincia
            {
                get { return _Provincia; }
                set { _Provincia = value; }
            }

            private string _Pais = "";
            [DataMember]
            public string Pais
            {
                get { return _Pais; }
                set { _Pais = value; }
            }

            private string _CodigoPostal = "";
            [DataMember]
            public string CodigoPostal
            {
                get { return _CodigoPostal; }
                set { _CodigoPostal = value; }
            }

            private Nullable<int> _InicioHorario1 = null;
            [DataMember]
            public Nullable<int> InicioHorario1
            {
                get { return _InicioHorario1; }
                set { _InicioHorario1 = value; }
            }

            private Nullable<int> _FinHorario1 = null;
            [DataMember]
            public Nullable<int> FinHorario1
            {
                get { return _FinHorario1; }
                set { _FinHorario1 = value; }
            }

            private Nullable<int> _TiempoEspera = null;
            [DataMember]
            public Nullable<int> TiempoEspera
            {
                get { return _TiempoEspera; }
                set { _TiempoEspera = value; }
            }

            private Nullable<double> _Volumen = null;
            [DataMember]
            public Nullable<double> Volumen
            {
                get { return _Volumen; }
                set { _Volumen = value; }
            }

            private Nullable<double> _Peso = null;
            [DataMember]
            public Nullable<double> Peso
            {
                get { return _Peso; }
                set { _Peso = value; }
            }

            private Nullable<int> _Bulto = null;
            [DataMember]
            public Nullable<int> Bulto
            {
                get { return _Bulto; }
                set { _Bulto = value; }
            }

            private Nullable<int> _Pallets = null;
            [DataMember]
            public Nullable<int> Pallets
            {
                get { return _Pallets; }
                set { _Pallets = value; }
            }

            private Nullable<double> _Latitud = null;
            [DataMember]
            public Nullable<double> Latitud
            {
                get { return _Latitud; }
                set { _Latitud = value; }
            }

            private Nullable<double> _Longitud = null;
            [DataMember]
            public Nullable<double> Longitud
            {
                get { return _Longitud; }
                set { _Longitud = value; }
            }

            private string _Observaciones = "";
            [DataMember]
            public string Observaciones
            {
                get { return _Observaciones; }
                set { _Observaciones = value; }
            }

            private string _Email = "";
            [DataMember]
            public string Email
            {
                get { return _Email; }
                set { _Email = value; }
            }

            private string _Categoria = "";
            [DataMember]
            public string Categoria
            {
                get { return _Categoria; }
                set { _Categoria = value; }
            }

            private string _Prioridad = "";
            [DataMember]
            public string Prioridad
            {
                get { return _Prioridad; }
                set { _Prioridad = value; }
            }

            private string _cargaExclusiva = "";
            [DataMember]
            public string cargaExclusiva
            {
                get { return _cargaExclusiva; }
                set { _cargaExclusiva = value; }
            }

            private string _Items_pOrdenPedidoItem_RefDocumento = "";
            [DataMember]
            public string Items_pOrdenPedidoItem_RefDocumento
            {
                get { return _Items_pOrdenPedidoItem_RefDocumento; }
                set { _Items_pOrdenPedidoItem_RefDocumento = value; }
            }

            private string _Items_pOrdenPedidoItem_Producto_RefProducto = "";
            [DataMember]
            public string Items_pOrdenPedidoItem_Producto_RefProducto
            {
                get { return _Items_pOrdenPedidoItem_Producto_RefProducto; }
                set { _Items_pOrdenPedidoItem_Producto_RefProducto = value; }
            }

            private string _Items_pOrdenPedidoItem_Producto_Descripcion = "";
            [DataMember]
            public string Items_pOrdenPedidoItem_Producto_Descripcion
            {
                get { return _Items_pOrdenPedidoItem_Producto_Descripcion; }
                set { _Items_pOrdenPedidoItem_Producto_Descripcion = value; }
            }

            /*nuevo 18052017*/
            private double _Items_pOrdenPedidoItem_Producto_Profundidad = 0;
            [DataMember]
            public double Items_pOrdenPedidoItem_Producto_Profundidad
            {
                get { return _Items_pOrdenPedidoItem_Producto_Profundidad; }
                set { _Items_pOrdenPedidoItem_Producto_Profundidad = value; }
            }

            private double _Items_pOrdenPedidoItem_Producto_Ancho = 0;
            [DataMember]
            public double Items_pOrdenPedidoItem_Producto_Ancho
            {
                get { return _Items_pOrdenPedidoItem_Producto_Ancho; }
                set { _Items_pOrdenPedidoItem_Producto_Ancho = value; }
            }

            private double _Items_pOrdenPedidoItem_Producto_Alto = 0;
            [DataMember]
            public double Items_pOrdenPedidoItem_Producto_Alto
            {
                get { return _Items_pOrdenPedidoItem_Producto_Alto; }
                set { _Items_pOrdenPedidoItem_Producto_Alto = value; }
            }

            private int _Items_pOrdenPedidoItem_Producto_Apilable = 0;
            [DataMember]
            public int Items_pOrdenPedidoItem_Producto_Apilable
            {
                get { return _Items_pOrdenPedidoItem_Producto_Apilable; }
                set { _Items_pOrdenPedidoItem_Producto_Apilable = value; }
            }

            private string _Items_pOrdenPedidoItem_Producto_Rotacion = "";
            [DataMember]
            public string Items_pOrdenPedidoItem_Producto_Rotacion
            {
                get { return _Items_pOrdenPedidoItem_Producto_Rotacion; }
                set { _Items_pOrdenPedidoItem_Producto_Rotacion = value; }
            }

            private string _Items_pOrdenPedidoItem_Producto_RotacionPermitida = "";
            [DataMember]
            public string Items_pOrdenPedidoItem_Producto_RotacionPermitida
            {
                get { return _Items_pOrdenPedidoItem_Producto_RotacionPermitida; }
                set { _Items_pOrdenPedidoItem_Producto_RotacionPermitida = value; }
            }

            /*FIN nuevo 18052017*/

            private Nullable<int> _Items_pOrdenPedidoItem_Cantidad = null;
            [DataMember]
            public Nullable<int> Items_pOrdenPedidoItem_Cantidad
            {
                get { return _Items_pOrdenPedidoItem_Cantidad; }
                set { _Items_pOrdenPedidoItem_Cantidad = value; }
            }

            private double _Items_pOrdenPedidoItem_Volumen = 0;
            [DataMember]
            public double Items_pOrdenPedidoItem_Volumen
            {
                get { return _Items_pOrdenPedidoItem_Volumen; }
                set { _Items_pOrdenPedidoItem_Volumen = value; }
            }

            private double _Items_pOrdenPedidoItem_Peso = 0;
            [DataMember]
            public double Items_pOrdenPedidoItem_Peso
            {
                get { return _Items_pOrdenPedidoItem_Peso; }
                set { _Items_pOrdenPedidoItem_Peso = value; }
            }

            private Nullable<int> _Items_pOrdenPedidoItem_Bulto = null;
            [DataMember]
            public Nullable<int> Items_pOrdenPedidoItem_Bulto
            {
                get { return _Items_pOrdenPedidoItem_Bulto; }
                set { _Items_pOrdenPedidoItem_Bulto = value; }
            }

            private int _Items_pOrdenPedidoItem_Pallets = 0;
            [DataMember]
            public int Items_pOrdenPedidoItem_Pallets
            {
                get { return _Items_pOrdenPedidoItem_Pallets; }
                set { _Items_pOrdenPedidoItem_Pallets = value; }
            }

            private int _Items_pOrdenPedidoItem_ImporteCosto = 0;
            [DataMember]
            public int Items_pOrdenPedidoItem_ImporteCosto
            {
                get { return _Items_pOrdenPedidoItem_ImporteCosto; }
                set { _Items_pOrdenPedidoItem_ImporteCosto = value; }
            }

            private Nullable<DateTime> _Items_pOrdenPedidoItem_FechaEntrega = null;
            [DataMember]
            public Nullable<DateTime> Items_pOrdenPedidoItem_FechaEntrega
            {
                get { return _Items_pOrdenPedidoItem_FechaEntrega; }
                set { _Items_pOrdenPedidoItem_FechaEntrega = value; }
            }

            private string _CampoDinamico_Campo = "";
            [DataMember]
            public string CampoDinamico_Campo
            {
                get { return _CampoDinamico_Campo; }
                set { _CampoDinamico_Campo = value; }
            }

            private string _CampoDinamico_Valor = "";
            [DataMember]
            public string CampoDinamico_Valor
            {
                get { return _CampoDinamico_Valor; }
                set { _CampoDinamico_Valor = value; }
            }

            private string _guid_registro = "";
            [DataMember]
            public string guid_registro
            {
                get { return _guid_registro; }
                set { _guid_registro = value; }
            }
        }


        [DataContract]
        [Serializable]
        private class NuevasParadas
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

            private List<NuevaParada> _collection_Paradas = null;
            [DataMember]
            public List<NuevaParada> collection_Paradas
            {
                get { return _collection_Paradas; }
                set { _collection_Paradas = value; }
            }
        }

        [DataContract]
        [Serializable]
        private class NuevaParada
        {
            private string _viajeId = "";
            [DataMember]
            public string viajeId
            {
                get { return _viajeId; }
                set { _viajeId = value; }
            }
            private List<unigisws.pParada> _Paradas = null;
            [DataMember]
            public List<unigisws.pParada> Paradas
            {
                get { return _Paradas; }
                set { _Paradas = value; }
            }
        }

    }
}
