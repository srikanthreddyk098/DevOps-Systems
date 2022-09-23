using System.Collections.Generic;

namespace AzureAutomation.Models
{
    public class AddPermissionModel
    {
        public IEnumerable<int> UserIdsToAdd { get; set; }
        public IEnumerable<int> GroupIdsToAdd { get; set; }
        public IEnumerable<int> VmIdsToAdd { get; set; }

    }
}