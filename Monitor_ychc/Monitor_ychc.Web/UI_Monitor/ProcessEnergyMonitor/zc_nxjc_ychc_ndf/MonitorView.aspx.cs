using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Monitor_ychc.Infrastructure.Configuration;
using Monitor_ychc.Service.FormulaEnergy;
using Monitor_ychc.Service.ProcessEnergyMonitor;
using System.Web.Services;

namespace Monitor_ychc.Web.UI_Monitor.ProcessEnergyMonitor.zc_nxjc_ychc_ndf
{
    public partial class MonitorView : System.Web.UI.Page
    {
        private static readonly string connString = ConnectionStringFactory.NXJCConnectionString;          //DCS连接字符串

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [WebMethod]
        public static SceneMonitor GetRealTimeData(string organizationId, string sceneName)
        {
            IList<DataItem> dataItems = new List<DataItem>();
            string factoryLevel = OrganizationHelper.GetFactoryLevel(organizationId);

            string dcsConn = ConnectionStringFactory.GetDCSConnectionString(organizationId);
            string ammeterConn = ConnectionStringFactory.GetAmmeterConnectionString(factoryLevel);

            //#region 获得表中实时数据
            //ProcessPowerMonitor precessPower = new ProcessPowerMonitor(connString);
            //DataTable sourceDt = precessPower.GetMonitorDatas(factoryLevel);
            //DataRow[] rows = sourceDt.Select(String.Format("OrganizationID='{0}'", organizationId));

            //string[] fields = { "本日合计", "本月累计", "本年累计" };
            //dataItems = ProcessEnergyMonitorService.GetPowerMonitor(rows, fields).ToList();
            //#endregion

            #region 获得dcs实时数据
            ProcessEnergyMonitorService monitorService = new ProcessEnergyMonitorService(dcsConn);
            IEnumerable<DataItem> monitorItems = monitorService.GetRealtimeDatas(organizationId, sceneName);
            foreach (var item in monitorItems)
            {
                dataItems.Add(item);
            }
            #endregion

            #region 获得电表功率数据
            ProcessEnergyMonitorService ammeterService = new ProcessEnergyMonitorService(ammeterConn);
            IEnumerable<DataItem> ammeterItems = ammeterService.GetRealtimeDatas(organizationId, sceneName);
            foreach (var item in ammeterItems)
            {
                dataItems.Add(item);
            }
            #endregion

            //#region 获得实时电能消耗数据
            //RealtimeFormulaValueService formulaValue = new RealtimeFormulaValueService(ammeterConn, "");
            //IEnumerable<DataItem> formulaValueItems = formulaValue.GetFormulaPowerConsumption(factoryLevel);
            //foreach (var item in formulaValueItems)
            //{
            //    dataItems.Add(item);
            //}
            //#endregion

            #region  获得实时公式电耗
            FormulaEnergyService formulaEnergyServer = new FormulaEnergyService(ammeterConn);
            IEnumerable<DataItem> formulaEnergyItems = formulaEnergyServer.GetFormulaPowerConsumption(factoryLevel);
            foreach (var item in formulaEnergyItems)
            {
                dataItems.Add(item);
            }
            #endregion

            #region 获取公式电耗月平均值
            IEnumerable<DataItem> formulaEnergyConsumptionMonthlyAverageItems = formulaEnergyServer.GetFormulaPowerConsumptionMonthlyAverage();
            foreach (var item in formulaEnergyConsumptionMonthlyAverageItems)
            {
                dataItems.Add(item);
            }
            #endregion

            #region 获得实时公式功率
            FormulaPowerService formulaPowerServer = new FormulaPowerService(connString);
            IEnumerable<DataItem> formulaPowerItems = formulaPowerServer.GetFormulaPower(factoryLevel);
            foreach (var item in formulaPowerItems)
            {
                dataItems.Add(item);
            }
            #endregion

            SceneMonitor result = new SceneMonitor();
            result.Name = sceneName;
            result.time = DateTime.Now;
            result.DataSet = dataItems;

            return result;
        }
    }
}