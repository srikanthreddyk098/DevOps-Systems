using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureAutomation.Data.Repository;
using AzureAutomation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace AzureAutomation.Web.Pages.Admin
{
    [Authorize(Roles="DevOpsAdmin")]
    [ValidateAntiForgeryToken]
    public class EditVmRestartPermissionsModel : PageModel
    {
        private readonly PermissionRepository _userRepository;
        private readonly VmRepository _vmRepository;

        private IList<AdUserModel> _adUsers;
        private DateTime _adUsersLastUpdateTime = DateTime.MinValue;
        private readonly object _adUserLock = new object();
        
        IList<AdGroupModel> _adGroups;
        private DateTime _adGroupsLastUpdateTime = DateTime.MinValue;
        private readonly object _adGroupLock = new object();

        public EditVmRestartPermissionsModel(IConfiguration config)
        {
            _userRepository = new PermissionRepository(config["CADConnectionString"]);
            _vmRepository = new VmRepository(config["CADConnectionString"]);
        }

        public async Task<IActionResult> OnGetAdUsersAsync(int pageNumber, string searchTerm)
        {
            const int rowsPerPage = 50;
            if (_adUsers == null || _adUsersLastUpdateTime.AddHours(-1) < DateTime.Now) {
                var adUsers = await _userRepository.GetAllUsers();
                lock (_adUserLock) {
                    _adUsers = adUsers.ToList();
                    _adUsersLastUpdateTime = DateTime.Now;
                }
            }

            var results = searchTerm == null
                ? _adUsers.Skip(pageNumber * rowsPerPage).Take(rowsPerPage)
                : _adUsers.Where(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).Take(rowsPerPage);

            var hasMore = searchTerm == null
                ? _adUsers.Count > pageNumber * rowsPerPage + rowsPerPage
                : _adUsers.Count(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) > rowsPerPage;


            return new JsonResult(new {results = results.Select(x => new {id = x.Id, text = x.Name}), hasMore});
        }

        public async Task<IActionResult> OnGetAdGroupsAsync(int pageNumber, string searchTerm)
        {
            const int rowsPerPage = 50;
            if (_adGroups == null || _adGroupsLastUpdateTime.AddHours(-1) < DateTime.Now) {
                var adGroups = await _userRepository.GetAllGroups();
                lock (_adGroupLock) {
                    _adGroups = adGroups.ToList();
                    _adGroupsLastUpdateTime = DateTime.Now;
                }
            }

            var results = searchTerm == null
                ? _adGroups.Skip(pageNumber * rowsPerPage).Take(rowsPerPage)
                : _adGroups.Where(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).Take(rowsPerPage);

            var hasMore = searchTerm == null
                ? _adGroups.Count > pageNumber * rowsPerPage + rowsPerPage
                : _adGroups.Count(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) > rowsPerPage;


            return new JsonResult(new {results = results.Select(x => new {id = x.Id, text = x.Name}), hasMore});
        }

        public async Task<IActionResult> OnGetVmsAsync()
        {
            var vms = await _vmRepository.GetAllVmsAsync();
            return new JsonResult(new {data = vms.Select(x => new {x.Id, x.Subscription, x.ResourceGroup, VmName = x.Name})});
        }

        public async Task<IActionResult> OnPostAddAsync([FromBody] AddPermissionModel permissionsToAdd)
        {
            var numberOfPermissions = 0;
            var successCount = 0;
            var duplicateCount = 0;

            foreach (var vmId in permissionsToAdd.VmIdsToAdd) {
                foreach (var userId in permissionsToAdd.UserIdsToAdd) {
                    numberOfPermissions++;
                    try {
                        var completedSuccessfully = await _userRepository.AddUserPermissionAsync(userId, vmId, User.Identity.Name);
                        if (completedSuccessfully) successCount++;
                    }
                    catch (Exception ex) {
                        if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint")) {
                            duplicateCount++;
                        }
                        else {
                            throw;
                        }
                    }
                }

                foreach (var groupId in permissionsToAdd.GroupIdsToAdd) {
                    numberOfPermissions++;
                    try {
                        var completedSuccessfully = await _userRepository.AddGroupPermissionAsync(groupId, vmId, User.Identity.Name);
                        if (completedSuccessfully) successCount++;
                    }
                    catch (Exception ex) {
                        if (ex.Message.StartsWith("Violation of UNIQUE KEY constraint")) {
                            duplicateCount++;
                        }
                        else {
                            throw;
                        }
                    }
                }
            }

            return successCount.Equals(numberOfPermissions) ?
                new  JsonResult($"{successCount} permission(s) were added successfully.") : 
                new JsonResult($"{successCount} out of {numberOfPermissions} permission(s) were added successfully. {duplicateCount} permission(s) already existed.");
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] int? idToDelete)
        {
            if (idToDelete == null) throw new Exception("Id to delete cannot be null.");
            if (idToDelete == 0) throw new Exception("Cannot delete a default permission.");
            var completedSuccessfully = await _userRepository.DeletePermissionAsync(idToDelete.Value);
            return new JsonResult(completedSuccessfully);
        }

        public async Task<IActionResult> OnGetUserVmsAsync()
        {
            var permissions = await _userRepository.GetAssignedPermissionsAsync();
            return new JsonResult(new {data = permissions });
        }
    }
}
