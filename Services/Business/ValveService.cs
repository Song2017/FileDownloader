using Common;
using Common.Utils;
using Microsoft.Extensions.Logging;
using Models;
using Services.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Services.Business
{
    public class ValveService : IValveService
    {
        private readonly IDBDataService _dataService;
        private readonly ILogger _logger;


        public ValveService(ILoggerFactory logger, IDBDataService dataService)
        {
            _dataService = dataService;

            _logger = logger.CreateLogger<ValveService>();
        }

        public List<Data> ValidateValve(string tokenName, List<string> querys, ref string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            #region validate query parameter: owner, plant, valvetype, filetype, tagnumber, serialnumber
            message = "ERROR: Please check input parameters.";
            for (var i = 0; i < querys.Count - 2; i++)
                if (querys[i].IsNullOrEmptyOrSpace())
                    return null;

            var valveTable = querys[2].ToStringEx().Trim().ToUpper();
            if (!AppConstants.ValveTypes.Split(",").Contains(valveTable))
                return null;
            valveTable = valveTable.GetValveTableName();

            message = querys[3].ToStringEx().Trim().ToUpper();
            if (!AppConstants.DownloadFileTypes.Split(",").Contains(message))
                return null;

            // tag, serial can not be null at the same time
            if (querys[4].IsNullOrEmptyOrSpace() && querys[5].IsNullOrEmptyOrSpace())
                return null;

            #endregion

            var queryMode = "ALL";
            if (querys[4].IsNullOrEmptyOrSpace())
                queryMode = "SER";
            else if (querys[5].IsNullOrEmptyOrSpace())
                queryMode = "TAG";

            var paras = tokenName.DecryptDes().Split("|");
            var dtRepairs = _dataService.ValidateRepairData(paras[0].ToStringEx(),
                valveTable, querys[0].ToStringEx(), querys[1].ToStringEx(), querys[4].ToStringEx(),
                querys[5].ToStringEx(), queryMode, out var results);

            if (results[0].StartsWith("ERROR"))
            {
                message = results[0];
                return null;
            }

            if (dtRepairs == null)
            {
                message = "ERROR: REPAIR NOT EXISTS!";
                return null;
            }

            var valves = new List<Data>();
            var createDate = DateTime.UtcNow.ToString(Constants.DateFormat);
            foreach (DataRow dr in dtRepairs.Rows)
            {
                valves.Add(new Data
                {
                    TagNumber = dr["TAGNUMBER"].ToStringEx(),
                    SerialNumber = dr["SERIAL"].ToStringEx(),
                    OwnerName = querys[0].ToStringEx().Trim().ToUpper(),
                    PlantLocation = querys[1].ToStringEx().Trim().ToUpper(),
                    CreateDate = createDate,
                    TenantKey = dr["TENANTKEY"].ToStringEx(),
                    EquipmentKey = dr["EQUIPMENTKEY"].ToStringEx(),
                    ValveTable = valveTable,
                    Token = tokenName,
                    FileType = message
                });
            }

            return valves;
        }

        public List<Data> GetValves(List<Data> valveMains)
        {
            var equipkeys = valveMains.AsEnumerable().Select(s => s.EquipmentKey.ToStringEx())
                .Distinct().ToArray();

            if (!GetValvesDataTable(valveMains[0], equipkeys, out var dtRepairs))
                return null;

            for (var i = 0; i < valveMains.Count; i++)
            {
                var rows = dtRepairs.Select($"EQUIPMENTKEY = '{valveMains[i].EquipmentKey}'");
                valveMains[i] = BindValveRepairData(valveMains[i], rows);
            }

            return valveMains;
        }

        public string CreateValvesFile(List<Data> valves)
        {
            string fileName;

            if (valves.Count <= 0)
                return null;

            if (valves.Count == 1)
            {
                // generate xml file
                var valve = valves[0];
                fileName = string.Format(AppConstants.NameTemplateXml, valve.TagNumber, valve.SerialNumber,
                    DateTime.UtcNow.ToString(Constants.FileNameDateFormat));

                if (valve.CreateXmlFile(string.Empty, fileName))
                    return fileName;
            }
            else
            {

                var tmpDirectory = Guid.NewGuid().ToStringEx();
                if (!Utils.CreateDirectory(tmpDirectory))
                    return null;

                // generate xml file in temp directory
                foreach (var valve in valves)
                {
                    var fname = string.Format(AppConstants.NameTemplateXml, valve.TagNumber, valve.SerialNumber,
                        DateTime.UtcNow.ToString(Constants.FileNameDateFormat));
                    if (!valve.CreateXmlFile(tmpDirectory, fname))
                        return null;
                }

                fileName = string.Format(AppConstants.NameTemplateZip, valves[0].TagNumber, valves[0].SerialNumber,
                    DateTime.UtcNow.ToString(Constants.FileNameDateFormat));

                return Utils.CreateZipFile(tmpDirectory, ref fileName) ? fileName : null;
            }

            return fileName;
        }


        private bool GetValvesDataTable(Data valveMain, string[] equipKeys, out DataTable dtRepairs)
        {
            dtRepairs = null;

            try
            {
                dtRepairs = _dataService.GetRepairHistoryData(valveMain.TenantKey.ToStringEx(),
                    valveMain.ValveTable, string.Join((char)8, equipKeys).ToStringEx());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get Valve Data Error: {ex}");
            }

            return dtRepairs != null && dtRepairs.Rows.Count != 0;
        }

        private Data BindValveRepairData(Data valveMain, DataRow[] dataRows)
        {

            var repairKey = string.Empty;
            var repair = new DataSub1();
            var repairList = new List<DataSub1>();
            var partList = new List<DataSub2>();
            try
            {
                //bind repair history data and part to valve
                foreach (var dr in dataRows)
                {
                    if (!repairKey.EqualsEx(dr["VALVEKEY"].ToStringEx()))
                    {
                        repairKey = dr["VALVEKEY"].ToStringEx();
                        if (partList.Count > 0)
                            repair.Parts = partList;
                        repairList.Add(repair);
                        partList = new List<DataSub2>();

                        repair = new DataSub1()
                        {
                            EffectiveDate = dr["EFFECTIVEDATE"].ToStringEx(),
                            MaintenanceFor = dr["MAINTFOR"].ToStringEx()
                        };
                    }

                    if ((dr["PARTNAME"].ToStringEx() + dr["PARTNUMBER"].ToStringEx() + dr["QUANTITY"].ToStringEx() +
                         dr["WORKPERFORMED"].ToStringEx()).IsNullOrEmptyOrSpace())
                        continue;

                    partList.Add(new DataSub2()
                    {
                        PartQuantity = dr["QUANTITY"].ToStringEx(),
                        PartName = dr["PARTNAME"].ToStringEx(),
                        PartNumber = dr["PARTNUMBER"].ToStringEx(),
                        WorkPerformed = dr["WORKPERFORMED"].ToStringEx()
                    });
                }

                if (!repair.EffectiveDate.IsNullOrEmptyOrSpace())
                {
                    repair.Parts = partList;
                    repairList.Add(repair);
                }

                repairList.RemoveAt(0);
                valveMain.Repairs = repairList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get Valve Data Error: {ex}");
            }

            return valveMain;
        }


    }
}
