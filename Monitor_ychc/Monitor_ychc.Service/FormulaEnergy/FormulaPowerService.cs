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
    public class FormulaPowerService
    {
        private SqlServerDataFactory _dataFactory;

        public FormulaPowerService(string connString)
        {
            _dataFactory = new SqlServerDataFactory(connString);
        }

        private DataTable GetFormulaPowerValues(string organizationId)
        {
            DataTable result;
            string queryString = @"select * from formula_power where 
                                    vDate=(select top 1 vDate from formula_power order by vDate desc) 
                                    and OrganizationID=@organizationId";
            SqlParameter[] parameters = { new SqlParameter("@organizationId", organizationId + "%") };
            result = _dataFactory.Query(queryString, parameters);
            return result;
        }

        private IEnumerable<DataItem> ConvertToDataItems(DataTable sourceTable)
        {
            IList<DataItem> result = new List<DataItem>();
            foreach (DataRow item in sourceTable.Rows)
            {
                DataItem values = new DataItem();
                values.ID = item["OrganizationID"].ToString().Trim() + item["LevelCode"].ToString().Trim() + "PowerValue";
                values.Value = item["PowerValue"].ToString().Trim();
                result.Add(values);
            }
            return result;
        }
        /// <summary>
        /// 获得公式功率值，
        /// 键为OrganizationID值、LevelCode值和字符串PowerValue的拼接，
        /// 值为公式功率值
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public IEnumerable<DataItem> GetFormulaPower(string organizationId)
        {
            DataTable sourceTable = GetFormulaPowerValues(organizationId);    // 获得formula_power表值
            IEnumerable<DataItem> result = ConvertToDataItems(sourceTable);   // 将表中功率值转换为键值对
            return result;
        }
    }
}
