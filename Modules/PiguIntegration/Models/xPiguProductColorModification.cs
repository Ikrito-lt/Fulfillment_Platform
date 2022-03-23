namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    internal class xPiguProductColorModification
    {
        public string modificationTitle { get; set; }
        public string weight { get; set; }  //in kg
        public string lenght { get; set; }  //in m
        public string height { get; set; }  //in m
        public string width { get; set; }   //in m
        public string barcode { get; set; }
        public string supplierCode { get; set; }
        public string manufCode { get; set; }

        public string GetXml()
        {
            string s =
                $@"
                        <modification>
                            <modification-title><![CDATA[{modificationTitle}]]></modification-title>
                            <weight>{weight}</weight>
                            <length>{lenght}</length>
                            <height>{height}</height>
                            <width>{width}</width>
                            <attributes>
                                <barcodes>
                                    <barcode><![CDATA[{barcode}]]></barcode>
                                </barcodes>
                                <supplier-code><![CDATA[{supplierCode}]]></supplier-code>
                                <manufacturer-code><![CDATA[{manufCode}]]></manufacturer-code>
                            </attributes>
                        </modification>
                        ";
            return s;
        }

    }
}
