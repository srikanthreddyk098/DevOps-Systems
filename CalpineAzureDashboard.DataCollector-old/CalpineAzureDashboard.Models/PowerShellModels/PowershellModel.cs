using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.Models.PowerShellModels
{
    public class PowerShellModel<T>
    {
        public IEnumerable<T> Items { get; }
        public bool HadErrors { get; }
        public IList<ErrorRecord> ErrorRecords { get; }

        public PowerShellModel(IEnumerable<T> items, bool hadErrors, PSDataCollection<ErrorRecord> errorRecords)
        {
            Items = items;
            HadErrors = hadErrors;
            ErrorRecords = JsonConvert.DeserializeObject<PSDataCollection<ErrorRecord>>(JsonConvert.SerializeObject(errorRecords));
        }

        public string GetErrors()
        {
            if (!ErrorRecords.Any()) {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var error in ErrorRecords) {
                if (error.Exception != null) {
                    sb.Append(error.Exception.Message);
                    sb.Append(Environment.NewLine);
                }

                if (error.ErrorDetails != null) {
                    sb.Append(error.ErrorDetails.Message);
                    sb.Append(Environment.NewLine);
                }

            }

            return sb.ToString();
        }
    }
}