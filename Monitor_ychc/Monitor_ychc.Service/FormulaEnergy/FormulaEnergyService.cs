using Monitor_ychc.Service.ProcessEnergyMonitor;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor_ychc.Service.FormulaEnergy
{
    public class FormulaEnergyService
    {
        private SqlServerDataFactory _dataFactory;

        public FormulaEnergyService(string connString)
        {
            _dataFactory = new SqlServerDataFactory(connString);
        }

        private DataTable GetFormulaValues(string organizationId)
        {
            DataTable result;
            string queryString = "select * from RealtimeFormulaValue where OrganizationID like @organizationId";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId + "%") };

            result = _dataFactory.Query(queryString, parameters);
            return result;
        }

        private DataTable CalculatePowerConsumption(DataTable sourceTable)
        {
            DataColumn newCol = new DataColumn("PowerConsumption", typeof(decimal));
            sourceTable.Columns.Add(newCol);
            foreach (DataRow item in sourceTable.Rows)
            {
                if (Convert.IsDBNull(item["DenominatorValue"]) || (Convert.ToDecimal(item["DenominatorValue"]) == 0))
                {
                    item["PowerConsumption"] = 0;
                }
                else
                {
                    item["PowerConsumption"] = (decimal)item["FormulaValue"] / (decimal)item["DenominatorValue"];
                }
            }
            return sourceTable;
        }

        private IEnumerable<DataItem> ConvertToDataItems(DataTable sourceTable)
        {
            string[] columns = { "PowerConsumption" };
            IList<DataItem> result = new List<DataItem>();
            foreach (DataRow item in sourceTable.Rows)
            {
                foreach (string col in columns)
                {
                    DataItem value = new DataItem();
                    value.ID = item["OrganizationID"].ToString().Trim() + item["LevelCode"].ToString().Trim() + col;
                    value.Value = item["PowerConsumption"].ToString().Trim();
                    result.Add(value);
                }
            }
            return result;
        }
        /// <summary>
        /// 根据公式获得电耗的键值对，
        /// 键为OrganizationID值、LevelCode值和PowerConsumption字符的连接字符串
        /// 值为PowerConsumption字段对应的值
        /// </summary>
        /// <param name="organizaionId"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public IEnumerable<DataItem> GetFormulaPowerConsumption(string organizaionId)
        {
            DataTable sourceTable = GetFormulaValues(organizaionId);   //获得RealtimeFormulaValue表值
            sourceTable = CalculatePowerConsumption(sourceTable);            //根据表中电量消耗值和产量计算电耗值
            IEnumerable<DataItem> result = ConvertToDataItems(sourceTable);  //将表中电耗值转换为键值对
            return result;
        }

        public IEnumerable<DataItem> GetFormulaPowerConsumptionMonthlyAverage()
        {
            string queryString = @"  SELECT [OrganizationID], [LevelCode], (SUM([FormulaValue]) / SUM([DenominatorValue])) AS [PowerConsumptionAverage]
                                       FROM [HistoyFormulaValue]
                                      WHERE [vDate] >= CONVERT(char(7), GETDATE(),20) + '-01' AND 
                                            [vDate] <= GETDATE() AND [DenominatorValue] IS NOT NULL AND [DenominatorValue] <> 0
                                   GROUP BY [OrganizationID], [LevelCode]";

            DataTable dt = _dataFactory.Query(queryString);

            IList<DataItem> result = new List<DataItem>();
            foreach (DataRow item in dt.Rows)
            {
                DataItem value = new DataItem();
                value.ID = item["OrganizationID"].ToString().Trim() + item["LevelCode"].ToString().Trim() + "PowerConsumptionMonthlyAverage";
                value.Value = item["PowerConsumptionAverage"].ToString().Trim();
                result.Add(value);
            }
            return result;
        }
    }
}
