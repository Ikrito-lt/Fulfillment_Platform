using System;
using System.Text;

namespace Ikrito_Fulfillment_Platform.Utils {
    public class HTMLTable : IDisposable {
        private readonly StringBuilder _sb;

        public HTMLTable(StringBuilder sb, string id = "default", string classValue = "") {
            _sb = sb;
            _sb.Append($"<table id=\"{id}\" class=\"{classValue}\">\n");
        }

        public void Dispose() {
            _sb.Append("</table>");
        }

        public HTMLRow AddRow() {
            return new HTMLRow(_sb);
        }

        public HTMLRow AddHeaderRow() {
            return new HTMLRow(_sb, true);
        }

        public void StartTableBody() {
            _sb.Append("<tbody>");

        }

        public void EndTableBody() {
            _sb.Append("</tbody>");

        }
    }

    public class HTMLRow : IDisposable {
        private readonly StringBuilder _sb;
        private readonly bool _isHeader;
        public HTMLRow(StringBuilder sb, bool isHeader = false) {
            _sb = sb;
            _isHeader = isHeader;
            if (_isHeader) {
                _sb.Append("<thead>\n");
            }
            _sb.Append("\t<tr>\n");
        }

        public void Dispose() {
            _sb.Append("\t</tr>\n");
            if (_isHeader) {
                _sb.Append("</thead>\n");
            }
        }

        public void AddCell(string innerText) {
            _sb.Append("\t\t<td>\n");
            _sb.Append("\t\t\t" + innerText);
            _sb.Append("\t\t</td>\n");
        }
    }
}
