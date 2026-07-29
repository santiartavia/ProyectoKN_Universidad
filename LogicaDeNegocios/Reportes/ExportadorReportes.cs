using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LogicaDeNegocios.Reportes
{
    public static class ExportadorReportes
    {
        public static string ContentType(string formato)
        {
            switch ((formato ?? "csv").ToLower())
            {
                case "xls":
                case "xlsx":
                    return "application/vnd.ms-excel";
                case "pdf":
                    return "application/pdf";
                default:
                    return "text/csv";
            }
        }

        public static string Extension(string formato)
        {
            switch ((formato ?? "csv").ToLower())
            {
                case "xls":
                case "xlsx":
                    return ".xls";
                case "pdf":
                    return ".pdf";
                default:
                    return ".csv";
            }
        }

        public static string Etiqueta(string formato)
        {
            switch ((formato ?? "csv").ToLower())
            {
                case "xls":
                case "xlsx":
                    return "Excel";
                case "pdf":
                    return "PDF";
                default:
                    return "CSV";
            }
        }

        public static byte[] Generar(string formato, ReporteResultado reporte)
        {
            switch ((formato ?? "csv").ToLower())
            {
                case "xls":
                case "xlsx":
                    return GenerarExcel(reporte);
                case "pdf":
                    return GenerarPdf(reporte);
                default:
                    return GenerarCsv(reporte);
            }
        }

        public static byte[] GenerarCsv(ReporteResultado r)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", ToCsv(r.Headers)));
            if (r.Rows != null)
            {
                foreach (var row in r.Rows)
                    sb.AppendLine(string.Join(",", ToCsv(row)));
            }
            var body = Encoding.UTF8.GetBytes(sb.ToString());
            var bom = Encoding.UTF8.GetPreamble();
            var final = new byte[bom.Length + body.Length];
            bom.CopyTo(final, 0);
            body.CopyTo(final, bom.Length);
            return final;
        }

        public static byte[] GenerarExcel(ReporteResultado r)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
            sb.AppendLine(" <Styles>");
            sb.AppendLine("  <Style ss:ID=\"Titulo\"><Font ss:Bold=\"1\" ss:Size=\"14\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Cabecera\"><Font ss:Bold=\"1\" ss:Color=\"#FFFFFF\"/><Interior ss:Color=\"#2E7D32\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Fila\"><Font ss:Color=\"#333333\"/></Style>");
            sb.AppendLine("  <Style ss:ID=\"Total\"><Font ss:Bold=\"1\"/></Style>");
            sb.AppendLine(" </Styles>");
            sb.AppendLine(" <Worksheet ss:Name=\"Reporte\">");
            sb.AppendLine("  <Table>");

            sb.Append("   <Row ss:Height=\"20\"><Cell ss:StyleID=\"Titulo\"><Data ss:Type=\"String\">");
            sb.Append(XmlEscape(r.Titulo ?? "Reporte"));
            sb.AppendLine("</Data></Cell></Row>");

            sb.Append("   <Row>");
            if (r.Headers != null)
            {
                foreach (var h in r.Headers)
                {
                    sb.Append("<Cell ss:StyleID=\"Cabecera\"><Data ss:Type=\"String\">");
                    sb.Append(XmlEscape(h ?? ""));
                    sb.Append("</Data></Cell>");
                }
            }
            sb.AppendLine("</Row>");

            if (r.Rows != null)
            {
                foreach (var row in r.Rows)
                {
                    sb.Append("   <Row ss:StyleID=\"Fila\">");
                    if (row != null)
                    {
                        foreach (var cell in row)
                        {
                            decimal numero;
                            sb.Append("<Cell>");
                            if (decimal.TryParse(cell, NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
                                sb.Append("<Data ss:Type=\"Number\">").Append(numero.ToString("0.00", CultureInfo.InvariantCulture)).Append("</Data>");
                            else
                                sb.Append("<Data ss:Type=\"String\">").Append(XmlEscape(cell ?? "")).Append("</Data>");
                            sb.Append("</Cell>");
                        }
                    }
                    sb.AppendLine("</Row>");
                }
            }

            sb.AppendLine("  </Table>");
            sb.AppendLine(" </Worksheet>");
            sb.AppendLine("</Workbook>");
            var preamble = Encoding.UTF8.GetPreamble();
            var cuerpo = Encoding.UTF8.GetBytes(sb.ToString());
            var resultado = new byte[preamble.Length + cuerpo.Length];
            preamble.CopyTo(resultado, 0);
            cuerpo.CopyTo(resultado, preamble.Length);
            return resultado;
        }

        public static byte[] GenerarPdf(ReporteResultado r)
        {
            var enc = Encoding.GetEncoding(1252);
            var cols = r.Headers != null ? r.Headers.Length : 0;
            var charW = 5.2;
            var pageW = 612.0;
            var left = 40.0;
            var rightLimit = pageW - 40.0;
            var ancho = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                int maxLen = r.Headers != null && r.Headers[j] != null ? r.Headers[j].Length : 0;
                if (r.Rows != null)
                {
                    foreach (var row in r.Rows)
                    {
                        if (row != null && j < row.Length && row[j] != null && row[j].Length > maxLen)
                            maxLen = row[j].Length;
                    }
                }
                ancho[j] = Math.Min(maxLen * charW + 8.0, 150.0);
            }
            var x = new double[cols];
            for (int j = 0; j < cols; j++)
                x[j] = j == 0 ? left : x[j - 1] + ancho[j - 1];

            var paginas = new List<string>();
            var actual = new StringBuilder();
            double y = 760.0;

            void NuevaLinea(double cantidad)
            {
                y -= cantidad;
                if (y < 45.0)
                {
                    actual.Append("ET\n");
                    paginas.Add(actual.ToString());
                    actual.Clear();
                    y = 760.0;
                    actual.Append("BT\n/F1 9 Tf\n");
                    for (int j = 0; j < cols; j++)
                    {
                        if (r.Headers != null && j < r.Headers.Length)
                            actual.Append(x[j].ToString("0.##", CultureInfo.InvariantCulture)).Append(" ").Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (")
                                .Append(EscPdf(r.Headers[j])).Append(") Tj\n");
                    }
                    y -= 16.0;
                    actual.Append("0 0 0 rg\n");
                }
            }

            actual.Append("BT\n");
            if (r.Headers != null && r.Headers.Length > 0)
            {
                actual.Append("/F2 14 Tf\n").Append(left.ToString("0.##", CultureInfo.InvariantCulture)).Append(" ").Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (").Append(EscPdf(r.Titulo ?? "Reporte")).Append(") Tj\n");
                y -= 20.0;
            }
            actual.Append("/F1 9 Tf\n");
            actual.Append("0 0 0 rg\n");
            if (r.Headers != null && r.Headers.Length > 0)
            {
                for (int j = 0; j < cols; j++)
                    actual.Append(x[j].ToString("0.##", CultureInfo.InvariantCulture)).Append(" ").Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (").Append(EscPdf(r.Headers[j])).Append(") Tj\n");
                y -= 14.0;
            }
            if (r.Rows != null)
            {
                foreach (var row in r.Rows)
                {
                    if (row == null) { continue; }
                    for (int j = 0; j < cols && j < row.Length; j++)
                        actual.Append(x[j].ToString("0.##", CultureInfo.InvariantCulture)).Append(" ").Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (").Append(EscPdf(row[j] ?? "")).Append(") Tj\n");
                    NuevaLinea(14.0);
                }
            }
            actual.Append("ET\n");
            paginas.Add(actual.ToString());

            return EnsamblarPdf(enc, paginas);
        }

        private static byte[] EnsamblarPdf(Encoding enc, List<string> paginas)
        {
            using (var ms = new MemoryStream())
            {
                var w = new StreamWriter(ms, enc);
                var offsets = new List<long>();
                int nPaginas = paginas.Count;
                int totalObjetos = 2 + nPaginas + 2 + nPaginas + 1;

                void Objeto(int num, string contenido)
                {
                    offsets.Add(ms.Position);
                    w.Write(num.ToString());
                    w.Write(" 0 obj\n");
                    w.Write(contenido);
                    w.Write("\nendobj\n");
                    w.Flush();
                }

                w.Write("%PDF-1.4\n");
                w.Write("%\u00E2\u00E3\u00CF\u00D3\n");
                w.Flush();

                offsets.Add(-1); // 0 no usado
                Objeto(1, "<< /Type /Catalog /Pages 2 0 R >>");
                var kids = new StringBuilder();
                for (int i = 0; i < nPaginas; i++)
                {
                    if (i > 0) kids.Append(" ");
                    kids.Append(3 + i).Append(" 0 R");
                }
                Objeto(2, "<< /Type /Pages /Kids [" + kids + "] /Count " + nPaginas + " >>");
                for (int i = 0; i < nPaginas; i++)
                {
                    int contenido = 3 + nPaginas + 2 + i;
                    Objeto(3 + i, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 5 0 R /F2 4 0 R >> >> /Contents " + contenido + " 0 R >>");
                }
                Objeto(3 + nPaginas, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");
                Objeto(3 + nPaginas + 1, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
                for (int i = 0; i < nPaginas; i++)
                {
                    int contenido = 3 + nPaginas + 2 + i;
                    var bytes = enc.GetBytes(paginas[i]);
                    Objeto(contenido, "<< /Length " + bytes.Length + " >>\nstream\n" + paginas[i] + "\nendstream");
                }

                long xref = ms.Position;
                w.Write("xref\n0 " + totalObjetos + "\n");
                w.Write("0000000000 65535 f \n");
                for (int i = 1; i < totalObjetos; i++)
                {
                    long off = i < offsets.Count ? offsets[i] : 0;
                    w.Write(off.ToString("D10") + " 00000 n \n");
                }
                w.Write("trailer\n<< /Size " + totalObjetos + " /Root 1 0 R >>\nstartxref\n");
                w.Write(xref.ToString());
                w.Write("\n%%EOF\n");
                w.Flush();
                return ms.ToArray();
            }
        }

        private static string[] ToCsv(string[] values)
        {
            if (values == null) return new string[0];
            var result = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                var v = values[i] ?? "";
                if (v.Contains(",") || v.Contains("\"") || v.Contains("\n") || v.Contains("\r"))
                    result[i] = "\"" + v.Replace("\"", "\"\"") + "\"";
                else
                    result[i] = v;
            }
            return result;
        }

        private static string XmlEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        private static string EscPdf(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c == '\\' || c == '(' || c == ')')
                {
                    sb.Append('\\').Append(c);
                }
                else if (c >= 32 && c <= 255)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('?');
                }
            }
            return sb.ToString();
        }
    }
}
