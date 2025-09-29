using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml;
using System.Text;

namespace Applet.Nat.Api.Static
{
    public static class Format
    {
        public static DateTime DateFromTimestamp(long? vivtms)
        {
            if (vivtms == null)
                return DateTime.MinValue;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(vivtms ?? 0);
        }
        public static DateTime? DateFromTimestampNull(long? vivtms)
        {
            if (vivtms == null || vivtms == 0)
                return null;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(vivtms ?? 0);
        }
        public static long TimestampFromDate(DateTime vivdtm)
        {
            long epoch = (vivdtm.Ticks - 621355968000000000) / 10000000;
            return epoch;
        }
        public static long TimestampFromNow()
        {
            return TimestampFromDate(DateTime.Now);
        }
        public static DateTime? DateFromUX(string vivstrDate, string vivstrFormat)
        {
            DateTime livdtm;
            if (!DateTime.TryParseExact(vivstrDate, vivstrFormat, null, System.Globalization.DateTimeStyles.None, out livdtm))
                return null;
            return livdtm;
        }
        public static DataSet JsonToDataSet(string jsonstring)
        {
            DataSet dataSet = new DataSet();

            // Deserialize JSON to dynamic object
            dynamic jsonObject = JsonConvert.DeserializeObject(jsonstring);

            // Iterate through properties of the dynamic object
            foreach (JProperty property in jsonObject.Children())
            {
                DataTable dataTable = new DataTable(property.Name);

                // Add columns to the DataTable
                foreach (JToken token in property.Values())
                {
                    foreach (JProperty subProperty in token.Children<JProperty>())
                    {
                        if (!dataTable.Columns.Contains(subProperty.Name))
                        {
                            dataTable.Columns.Add(subProperty.Name);
                        }
                    }
                }

                // Add rows to the DataTable
                foreach (JToken token in property.Values())
                {
                    DataRow row = dataTable.NewRow();
                    foreach (JProperty subProperty in token.Children<JProperty>())
                    {
                        row[subProperty.Name] = subProperty.Value.ToString();
                    }
                    dataTable.Rows.Add(row);
                }

                dataSet.Tables.Add(dataTable);
            }

            return dataSet;
        }
        public static string NumToLetters(string vivstrCurr, string vivstrNum, bool vivblnUpper)
        {
            string livstrLiteral = $"son {vivstrCurr} ";
            string livstrParteDecimal;
            //si el numero utiliza (.) en lugar de (,) -> se reemplaza
            vivstrNum = vivstrNum.Replace(".", ",");

            //si el numero no tiene parte decimal, se le agrega ,00
            if (vivstrNum.IndexOf(",") == -1)
            {
                vivstrNum = vivstrNum + ",00";
            }
            //se valida formato de entrada -> 0,00 y 999 999 999,00
            mioRegex = new Regex(@"\d{1,9},\d{1,2}");
            MatchCollection mc = mioRegex.Matches(vivstrNum);
            if (mc.Count > 0)
            {
                //se divide el numero 0000000,00 -> entero y decimal
                string[] Num = vivstrNum.Split(',');
                if (Num[1].Length == 1)
                    Num[1] = Num[1].PadRight(2, '0');
                //de da formato al numero decimal
                livstrParteDecimal = "con " + Num[1] + "/100";
                //se convierte el numero a literal
                if (int.Parse(Num[0]) == 0)
                {//si el valor es cero                
                    livstrLiteral += "cero ";
                }
                else if (int.Parse(Num[0]) > 999999)
                {//si es millon
                    livstrLiteral += getMillones(Num[0]);
                }
                else if (int.Parse(Num[0]) > 999)
                {//si es miles
                    livstrLiteral += getMiles(Num[0]);
                }
                else if (int.Parse(Num[0]) > 99)
                {//si es centena
                    livstrLiteral += getCentenas(Num[0]);
                }
                else if (int.Parse(Num[0]) > 9)
                {//si es decena
                    livstrLiteral += getDecenas(Num[0]);
                }
                else
                {//sino unidades -> 9
                    livstrLiteral += getUnidades(Num[0]);
                }
                //devuelve el resultado en mayusculas o minusculas
                if (vivblnUpper)
                {
                    return (livstrLiteral + livstrParteDecimal).ToUpper();
                }
                else
                {
                    return (livstrLiteral + livstrParteDecimal);
                }
            }
            else
            {//error, no se puede convertir
                return livstrLiteral = null;
            }
        }
        private static string[] mcoUnidades = { "", "un ", "dos ", "tres ", "cuatro ", "cinco ", "seis ", "siete ", "ocho ", "nueve " };
        private static string[] mcoDecenas = {"diez ", "once ", "doce ", "trece ", "catorce ", "quince ", "dieciseis ",
        "diecisiete ", "dieciocho ", "diecinueve", "veinte ", "treinta ", "cuarenta ",
        "cincuenta ", "sesenta ", "setenta ", "ochenta ", "noventa "};
        private static string[] mcCententas = {"", "ciento ", "doscientos ", "trecientos ", "cuatrocientos ", "quinientos ", "seiscientos ",
        "setecientos ", "ochocientos ", "novecientos "};
        private static Regex mioRegex;
        private static string getUnidades(string vivstrNum)
        {   // 1 - 9            
            //si tuviera algun 0 antes se lo quita -> 09 = 9 o 009=9
            string livstrNum = vivstrNum.Substring(vivstrNum.Length - 1);
            return mcoUnidades[int.Parse(livstrNum)];
        }
        public static string GenerateRandomHex(int length)
        {
            var random = new Random();
            var buffer = new byte[length / 2];
            random.NextBytes(buffer);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }
        private static string getDecenas(string vivstrNum)
        {// 99                        
            int n = int.Parse(vivstrNum);
            if (n < 10)
            {//para casos como -> 01 - 09
                return getUnidades(vivstrNum);
            }
            else if (n > 19)
            {//para 20...99
                string u = getUnidades(vivstrNum);
                if (u.Equals(""))
                { //para 20,30,40,50,60,70,80,90
                    return mcoDecenas[int.Parse(vivstrNum.Substring(0, 1)) + 8];
                }
                else
                {
                    return mcoDecenas[int.Parse(vivstrNum.Substring(0, 1)) + 8] + "y " + u;
                }
            }
            else
            {//numeros entre 11 y 19
                return mcoDecenas[n - 10];
            }
        }

        private static string getCentenas(string vivstrNum)
        {// 999 o 099
            if (int.Parse(vivstrNum) > 99)
            {//es centena
                if (int.Parse(vivstrNum) == 100)
                {//caso especial
                    return " cien ";
                }
                else
                {
                    return mcCententas[int.Parse(vivstrNum.Substring(0, 1))] + getDecenas(vivstrNum.Substring(1));
                }
            }
            else
            {//por Ej. 099 
                //se quita el 0 antes de convertir a decenas
                return getDecenas(int.Parse(vivstrNum) + "");
            }
        }

        private static string getMiles(string vivstrNum)
        {// 999 999
            //obtiene las centenas
            string c = vivstrNum.Substring(vivstrNum.Length - 3);
            //obtiene los miles
            string m = vivstrNum.Substring(0, vivstrNum.Length - 3);
            string n = "";
            //se comprueba que miles tenga valor entero
            if (int.Parse(m) > 0)
            {
                n = getCentenas(m);
                return n + "mil " + getCentenas(c);
            }
            else
            {
                return "" + getCentenas(c);
            }

        }

        private static string getMillones(string vivstrNum)
        { //000 000 000        
            //se obtiene los miles
            string miles = vivstrNum.Substring(vivstrNum.Length - 6);
            //se obtiene los millones
            string millon = vivstrNum.Substring(0, vivstrNum.Length - 6);
            string n = "";
            if (millon.Length > 1)
            {
                n = getCentenas(millon) + "millones ";
            }
            else
            {
                n = getUnidades(millon) + "millon ";
            }
            return n + getMiles(miles);
        }

        public static double GetAmountFromXmlNode(XmlNode vioXmlNode, int vivnroLengthEntera, int vivnroLengthDecimal, out string rivException)
        {

            if (vioXmlNode == null)
            {
                rivException = "no encontrado";
                return -1;
            }
            if (vioXmlNode.InnerText.Length == 0)
            {
                rivException = string.Empty;
                return 0;
            }
            if (vioXmlNode.InnerText.Trim() == "0")
            {
                rivException = string.Empty;
                return 0;
            }
            string vivstrDoubleval = vioXmlNode.InnerText.Replace(".", string.Empty).Replace(",", string.Empty);

            if (vivstrDoubleval.Length < vivnroLengthDecimal)
            {
                rivException = "con formato erroneo";
                return -1;
            }
            if (vivstrDoubleval.Length > vivnroLengthDecimal + vivnroLengthEntera)
            {
                rivException = "con formato erroneo";
                return -1;
            }
            double? lioResult = GetDoubleFromString(vivstrDoubleval, (short)vivnroLengthDecimal);
            rivException = lioResult == null ? "con formato erroneo" : string.Empty;
            return lioResult??0;
            /*
                        vivnroLengthEntera = vioXmlNode.InnerText.Length - vivnroLengthDecimal;
                        long livlng = 0;
                        if (!long.TryParse(vioXmlNode.InnerXml.Substring(0, vioXmlNode.InnerText.Length - vivnroLengthDecimal), out livlng))
                        {
                            rivException = "con formato erroneo";
                            return -1;
                        }
                        double livdec = 0;
                        if (!double.TryParse(vioXmlNode.InnerXml.Substring(vivnroLengthEntera, vivnroLengthDecimal), out livdec))
                        {
                            rivException = "con formato erroneo";
                            return -1;
                        }
                        if (livdec != 0)
                            livdec = livdec / (Math.Pow(10, vivnroLengthDecimal));
                        rivException = String.Empty;
                        return livlng + livdec;*/
        }
        public static string Property(string vivstrPropName, string vivstrPropValue)
        {
            if (!vivstrPropName.Contains("dbl"))
                return vivstrPropValue;
            if (!vivstrPropValue.Contains(","))
                return vivstrPropValue;
            //tiene ,
            if (vivstrPropValue.Contains("."))
                //tiene , y . reemplaza primero los . y luego las , por .
                return vivstrPropValue.Replace(".", string.Empty).Replace(",", ".");
            //solo reemplaza las ,
            return vivstrPropValue.Replace(",", ".");
        }

        public static double? GetDoubleFromString(string vivstrDoubleval, short vivnroDecpos)
        {
            vivstrDoubleval = vivstrDoubleval.Replace(".", string.Empty).Replace(",", string.Empty);
            long livvalEntero;
            double livvalDecimal;
            vivstrDoubleval = vivstrDoubleval.Trim();
            if (vivstrDoubleval == string.Empty || vivstrDoubleval == "0")
                return 0;
            if (vivstrDoubleval.Length < vivnroDecpos)
                return null;
            livvalEntero = 0;
            if (vivstrDoubleval.Length > vivnroDecpos)
                if (!long.TryParse(vivstrDoubleval.Trim().Substring(0, vivstrDoubleval.Length - vivnroDecpos), out livvalEntero))
                    return null;
            if (!double.TryParse(vivstrDoubleval.Trim().Substring(vivstrDoubleval.Length - vivnroDecpos, vivnroDecpos), out livvalDecimal))
                return null;
            if (livvalDecimal != 0)
                livvalDecimal = livvalDecimal / (Math.Pow(10, vivnroDecpos));
            return livvalEntero + livvalDecimal;
        }
        public static string Compress(string value)
        {
            //Transform string into byte[]  
            byte[] byteArray = new byte[value.Length];
            int indexBA = 0;
            foreach (char item in value.ToCharArray())
            {
                byteArray[indexBA++] = (byte)item;
            }
            return Compress(byteArray);
        }
        public static string Compress(byte[] _byteArray)
        {
            //Prepare for compress
            MemoryStream ms = new MemoryStream();
            System.IO.Compression.GZipStream sw = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress);

            //Compress
            sw.Write(_byteArray, 0, _byteArray.Length);
            //Close, DO NOT FLUSH cause bytes will go missing...
            sw.Close();

            //Transform byte[] zip data to string
            _byteArray = ms.ToArray();
            System.Text.StringBuilder sB = new System.Text.StringBuilder(_byteArray.Length);
            foreach (byte item in _byteArray)
            {
                sB.Append((char)item);
            }
            ms.Close();
            sw.Dispose();
            ms.Dispose();
            return Convert.ToBase64String(_byteArray);
        }
        public static string UnCompress(string vivstr, Encoding vioEncoding)
        {
            return vioEncoding.GetString(UnCompress2(vivstr));
        }
        public static byte[] UnCompress2(string value)
        {
            byte[] byteArray = Convert.FromBase64String(value);
            //Prepare for decompress
            using (var mso = new MemoryStream())
            {
                MemoryStream ms = new System.IO.MemoryStream(byteArray);
                System.IO.Compression.GZipStream sr = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);
                sr.CopyTo(mso);
                sr.Close();
                ms.Close();
                sr.Dispose();
                ms.Dispose();
                return mso.ToArray();

            }

        }
        public static string RemoveSpacesFromJson(string vivstrJson)
        {
            if (string.IsNullOrEmpty(vivstrJson))
                return vivstrJson;

            var lioJson = JsonConvert.DeserializeObject(vivstrJson);
            return JsonConvert.SerializeObject(lioJson, Newtonsoft.Json.Formatting.None);
        }
    }
}
