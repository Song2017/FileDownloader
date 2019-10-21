using System.Collections.Generic;
using Models;

namespace Services.Business
{
    public interface IValveService
    {
        List<Data> ValidateValve(string tokenName, List<string> query, ref string message);

        List<Data> GetValves(List<Data> valveMains);

        string CreateValvesFile(List<Data> valves);
    }
}
