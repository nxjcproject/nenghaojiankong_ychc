using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitor_ychc.Service.ProcessEnergyMonitor
{
    public class OrganizationHelper
    {
        public static string GetFactoryLevel(string sourceOrganization)
        {
            string result = "";

            string[] items = sourceOrganization.Split('_');
            if (items.Count() == 5)
            {
                int lastIndex = sourceOrganization.LastIndexOf('_');
                result = sourceOrganization.Substring(0, lastIndex);
                //result = items[0] + "_" + items[1] + "_" + items[2] + "_" + items[3];
            }

            return result;
        }
    }
}
