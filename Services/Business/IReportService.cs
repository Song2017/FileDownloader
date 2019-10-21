using System.Collections.Generic;
using Models;
using Microsoft.Extensions.Localization;

namespace Services.Business
{
    public interface IReportService
    {
        bool CreateWordFile(Data valve, string fileName, IStringLocalizer localizer, string directory = "");

        string CreateWordFiles(List<Data> valves, IStringLocalizer localizer);
    }
}
