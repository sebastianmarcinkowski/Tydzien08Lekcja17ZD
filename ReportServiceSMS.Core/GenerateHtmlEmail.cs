using ReportServiceSMS.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportServiceSMS.Core
{
    public class GenerateHtmlEmail
    {
        public string GenerateErrors(List<Error> errors, int interval)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            if (!errors.Any())
                return string.Empty;

            var html = $"Błędy z ostatnich {interval} minut.<br /><br />";

            html +=
                @"
                    <table border=1 cellpadding=5 cellspacing=1>
                        <tr>
                            <td align=center bgcolor=lightgrey>Wiadomość</td>
                            <td align=center bgcolor=lightgrey>Data</td>
                        </tr>
                ";

            foreach (var error in errors)
            {
                html +=
                    $@"
                        <tr>
                            <td align=center>{error.Message}</td>
                            <td align=center>{error.Date.ToString("dd-MM-yyyy HH:mm")}</td>
                        </tr>
                    ";
            }

            html +=
                @"
                    </table><br /><br />
                    <i>Automatyczna wiadomość wysłana z aplikacji ReportService.</i>
                ";

            return html;
        }
    }
}
