using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    internal class PiguProductColor
    {
        public string colorTitle { get; set; }
        public List<string> images = new List<string>();
        public List<PiguProductColorModification> modifications = new List<PiguProductColorModification>();

        public string GetImagesXml()
        {
            string s = "";
            foreach (string url in images)
            {
                s += $"<image><url>{url}</url></image>\n";
            }
            return s;
        }

        public string GetModificationsXml()
        {
            string s = "";
            foreach (PiguProductColorModification mod in modifications)
            {
                s += $"{mod.GetXml()}\n";
            }
            return s;
        }

        public string GetXml()
        {
            string s =
                    $@"
                        <colour>
                            <colour-title><![CDATA[{colorTitle}]]></colour-title>
                            <images>
                                {GetImagesXml()}
                            </images>
                            <modifications>
                                {GetModificationsXml()}
                            </modifications>
                        </colour>
                        ";
            return s;
        }
    }
}
