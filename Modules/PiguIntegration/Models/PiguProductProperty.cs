using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    internal class PiguProductProperty
    {
        public string IDstr { get; set; }
        public List<string> values = new List<string>();

        private string GetValuesXml() {
            string s = "";
            foreach (string val in values) {
                s += $"<value><![CDATA[{val}]]></value>\n";
            }

            return s;
        }

        public string GetXml() {
            string s = 
                $@"
                    <property>
                        <id><![CDATA[{IDstr}]]></id>
                         <values>
                             <value><![CDATA[{GetValuesXml()}]]></value>
                         </values>
                     </property>
                ";
            return s;
        }
    }
}
