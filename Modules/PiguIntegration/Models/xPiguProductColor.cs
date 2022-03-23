using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    internal class xPiguProductColor
    {
        public string colorTitle { get; set; }
        public List<string> images = new List<string>();
        public List<xPiguProductColorModification> modifications = new List<xPiguProductColorModification>();

        public string GetImagesXml()
        {
            string s = "";
            foreach (string url in images)
            {
                s += $"<image><![CDATA[<url>{url}</url>]]></image>\n";
            }
            return s;
        }

        public string GetModificationsXml()
        {
            string s = "";
            foreach (xPiguProductColorModification mod in modifications)
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
