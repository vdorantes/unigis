using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace unigis
{
    class utils
    {
        public string guardarLog(string msg, string pathLog)
        {
            try
            {                
                using (TextWriter writer = new StreamWriter(@pathLog, true))  // true is for append mode
                {
                    string fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    writer.WriteLine(msg.ToString() + ',' + fecha.ToString());
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }

    
}
