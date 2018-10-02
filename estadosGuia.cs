using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace unigis
{
    class estadosGuia
    {
        public void procesar()
        {
            unigisEstadosGuia.wsGetEstatus data = new unigisEstadosGuia.wsGetEstatus();
            var json = data.getEstadosGuia();
            var JSSerializer = new JavaScriptSerializer();
            JSSerializer.MaxJsonLength = Int32.MaxValue;            
            Estados estadosData = JSSerializer.Deserialize<Estados>(json);

            if (estadosData.success)
            {                                
                unigisws.Service unigis = new unigisws.Service();
                unigis.Timeout = 10 * 60 * 1000;

                try
                {
                    foreach (var estadoData in estadosData.collection_Estados)
                    {

                        unigisws.pEstadoOrdenPedido estado = new unigisws.pEstadoOrdenPedido();                       

                        try
                        {                           
                            Console.WriteLine(estadoData.RefDocumento);                            
                           
                            estado.RefDocumento = estadoData.RefDocumento;                                
                            estado.Estado = estadoData.Estatus;                                                                           

                            //sincronizar registro
                            int resp = unigis.ModificarEstadoOrdenPedido("1234", estado);
                            Console.WriteLine(resp.ToString()+' '+estado.RefDocumento+' '+ estado.Estado);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(estadoData.RefDocumento + " " + e.Message); // order error
                        }                        
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message); // orders error
                }
            }
            else
            {
                Console.WriteLine(estadosData.msg);
            }
        }

        [DataContract]
        [Serializable]
        private class Estados
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

            private List<EstadoGuia> _collection_Estados = null;
            [DataMember]
            public List<EstadoGuia> collection_Estados
            {
                get { return _collection_Estados; }
                set { _collection_Estados = value; }
            }
        }


        [DataContract]
        [Serializable]
        private class EstadoGuia
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

            private string _Estatus = "";
            [DataMember]
            public string Estatus
            {
                get { return _Estatus; }
                set { _Estatus = value; }
            }
        }
    }
}
