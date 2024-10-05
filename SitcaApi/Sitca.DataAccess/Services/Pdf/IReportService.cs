using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.DataAccess.Services.Pdf
{
    public interface IReportService
    {
        public byte[] GeneratePdfReport(string view, string orientation = "default");
    }
}
