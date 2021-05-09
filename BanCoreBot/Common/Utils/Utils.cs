using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace BanCoreBot.Common.Utils
{
    public class Utils
    {
        public bool ValidateNumberPhone(string number) 
        {

            if (Regex.IsMatch(number, "^[0-9]+$"))
            {
                if (number.Length.Equals(11)) { return true; } else { return false; }
            }
            else
            {
                return false;
            }
        }

        public string ValidateNumberCI(string number, string typeDoc)
        {
            if (!typeDoc.ToLower().Equals("pasaporte") && !typeDoc.ToLower().Equals("p") && !typeDoc.ToLower().Equals("j") && !typeDoc.ToLower().Equals("g"))
            {
                if (Regex.IsMatch(number, @"\W\d*[0-9]{2}") || Regex.IsMatch(number, @"^[0-9]+$"))
                {
                    number = number.Replace(".", "").Replace("-", "").Replace("/", "").Replace(",", "").Replace("_", "");

                    if (int.TryParse(number, out int n))
                    {
                        var numberInt = Int64.Parse(number);

                        if (numberInt > 30000 && numberInt < 100000000)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "La cédula ingresada no está en el rango correcto, por favor ingresa nuevamente tu número de documento de identidad:";
                        }
                    }
                    else
                    {
                        return "El valor de la cédula sólo debe ser númerico, por favor ingresa nuevamente tu número de documento de identidad:";
                    }

                }
                else
                {
                    return "La cédula ingresada no está en el rango correcto, por favor ingresa nuevamente tu número de documento de identidad:";
                }
            }
            else
            {
                return "OK";
            }
        }
        

        public bool ValidateName(string name)
        {

            if (Regex.IsMatch(name, @"[\p{L}\s]+$"))
            {
                 return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsValidEmail(string emailaddress)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                     @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                     @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(emailaddress))
                return (true);
            else
                return (false);
        }

        public string ExtractEmails(string input)
        {      
            Regex emailRegex = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*",
                RegexOptions.IgnoreCase);
            //find items that matches with our pattern
            MatchCollection emailMatches = emailRegex.Matches(input);

            StringBuilder sb = new StringBuilder();

            foreach (Match emailMatch in emailMatches)
            {
                sb.AppendLine(emailMatch.Value);
            }
            //store to file
            return sb.ToString().Replace("\r\n", string.Empty);
        }

        public string Extractphone(string input)
        {
            input = input.Replace(".", "").Replace("-", "").Replace("/", "").Replace(",", "").Replace("_", "");
            Regex PhoneRegex = new Regex(@"\b0(?:248|281|282|283|235|247|278|243|244|245|246|273|278|235|285|286|288|241|242|243|245|249|258|287|212|259|268|269|237|235|238|246|247|251|252|253|271|273|274|275|212|234|239|287|291|292|295|255|256|257|293|294|276|277|271|272|212|251|253|254|261|262|263|264|265|266|267|271|275|260|270|412|414|424|416|426)[0-9]{7}",
                RegexOptions.IgnoreCase);
            //find items that matches with our pattern
            MatchCollection phoneMatches;
            phoneMatches = PhoneRegex.Matches(input);
            StringBuilder sb = new StringBuilder();

            foreach (Match phoneMatch in phoneMatches)
            {
                sb.AppendLine(phoneMatch.Value);
            }
            //store to file
            return sb.ToString().Replace("\r\n", string.Empty);
        }

        public bool ValidateNumber(string number)
        {

            if (Regex.IsMatch(number, "^[0-9]+$"))
            {
                return true; 
            }
            else
            {
                return false;
            }
        }

        public bool ValidateNumber4Dig(string number)
        {
            if (Regex.IsMatch(number, "^[0-9]+$"))
            {
                if (number.Length == 4) { return true; } else { return false; }
            }
            else
            {
                return false;
            }
        }

        public bool ValidateNumber20Dig(string number)
        {
            if (Regex.IsMatch(number, "^[0-9]+$"))
            {
                if (number.Length == 20) { return true; } else { return false; }
            }
            else
            {
                return false;
            }
        }


        public int ValidateDate(string number)
        {

            if (Regex.IsMatch(number, "^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$"))
            {
                DateTime date;
                if (DateTime.TryParseExact(number, "dd'/'MM'/'yyyy",
                                           CultureInfo.GetCultureInfo("es-ES"),
                                           DateTimeStyles.None,
                                           out date))
                {
                    if (date < DateTime.Today) { return 0; } else { return 1; }
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 2;
            }
        }

        public int ValidateDate72h(string number)
        {

            if (Regex.IsMatch(number, "^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$"))
            {
                DateTime date;
                if (DateTime.TryParseExact(number, "dd'/'MM'/'yyyy",
                                           CultureInfo.GetCultureInfo("es-ES"),
                                           DateTimeStyles.None,
                                           out date))
                {
                    if (date <= (DateTime.Today).AddDays(-3)) { return 0; } //Mayor a 72h
                    else if (date > DateTime.Today) { return 3; } // Fecha futura, aún no ocurre ese día
                    else { return 1; } // Menor a 72h
                }
                else
                {
                    return 2; //Formato de fecha incorrecto
                }
            }
            else
            {
                return 2; //Formato de fecha incorrecto
            }
        }

        public int ValidateDate72a120(string number)
        {
            if (Regex.IsMatch(number, "^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$"))
            {
                DateTime date;
                if (DateTime.TryParseExact(number, "dd'/'MM'/'yyyy",
                                           CultureInfo.GetCultureInfo("es-ES"),
                                           DateTimeStyles.None,
                                           out date))
                {
                    if ((date >= (DateTime.Today).AddDays(-120)) && (date <= (DateTime.Today).AddDays(-3))) { return 0; } else { return 1; }
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 2;
            }
        }

        public int ValidateDate0a120(string number)
        {

            if (Regex.IsMatch(number, "^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$"))
            {
                DateTime date;
                if (DateTime.TryParseExact(number, "dd'/'MM'/'yyyy",
                                           CultureInfo.GetCultureInfo("es-ES"),
                                           DateTimeStyles.None,
                                           out date))
                {
                    if ((date >= (DateTime.Today).AddDays(-120)) && (date <= DateTime.Today)) { return 0; } else { return 1; }
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 2;
            }
        }

        public int ValidateDate5a120(string number)
        {
            if (Regex.IsMatch(number, "^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[1-9]|[1][0-2])[./-]([0-9]{4}|[0-9]{2})$"))
            {
                DateTime date;
                if (DateTime.TryParseExact(number, "dd'/'MM'/'yyyy",
                                           CultureInfo.GetCultureInfo("es-ES"),
                                           DateTimeStyles.None,
                                           out date))
                {
                    if ((date >= (DateTime.Today).AddDays(-120)) && (date <= (DateTime.Today).AddDays(-5))) { return 0; } else { return 1; }
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 2;
            }
        }

        public bool ValidateOfficeHours()
        {
            DateTime ahora = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time"));
            TimeSpan ts = new TimeSpan(8, 00, 0);
            TimeSpan ts2 = new TimeSpan(16, 30, 0);
            DateTime inicioJornada = ahora.Date + ts;
            DateTime finJornada = ahora.Date + ts2;
            //CompareHours
            if (ahora.DayOfWeek != DayOfWeek.Sunday)
            {
                if (ahora >= inicioJornada && ahora < finJornada)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public string ValidateTime()
        {
            DateTime ahora = System.TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Venezuela Standard Time"));
         /* TimeSpan ts = new TimeSpan(6, 00, 0);
            TimeSpan ts2 = new TimeSpan(12, 00, 0);
            TimeSpan ts3 = new TimeSpan(19, 00, 0);
            DateTime inicioManana = ahora.Date + ts;
            DateTime inicioTarde = ahora.Date + ts2;
            DateTime inicioNoche = ahora.Date + ts3; */
            //CompareHours
            if (ahora.Hour >=6 && ahora.Hour < 12)
            {
                return " buen día";
            }
            else if (ahora.Hour >= 12 && ahora.Hour < 19)
            {
                return " buenas tardes";
            }
            else if (ahora.Hour >= 19 || ahora.Hour < 6)
            {
                return " buenas noches";
            }
            else
            {
                return " buen día";
            }
        }

    }
}