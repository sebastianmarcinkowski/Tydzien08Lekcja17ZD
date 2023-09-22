using ReportServiceSMS.Core.Domains;
using System;
using System.Collections.Generic;

namespace ReportServiceSMS.Core.Repositories
{
    public class ErrorRepository
    {
        public List<Error> GetLasErrors(int intervalInMinutes)
        {
            // Pobieranie z DB.

            return new List<Error>
            {
                new Error { Message = "Błąd testowy 1", Date = DateTime.Now },
                new Error { Message = "Błąd testowy 2", Date = DateTime.Now }
            };
        }
    }
}
