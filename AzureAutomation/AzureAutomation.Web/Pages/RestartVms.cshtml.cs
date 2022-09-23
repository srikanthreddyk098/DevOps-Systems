using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureAutomation.Azure;
using AzureAutomation.Data.Repository;
using AzureAutomation.Models;
using AzureAutomation.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureAutomation.Web.Pages
{
    public class RestartVmsModel : PageModel
    {
        private readonly PermissionRepository _permissionRepository;
        //private readonly AzureVmPriceService _azureVmPricing;
        private readonly AzureService _azureService;
        private readonly string _adminGroupId;
        private readonly ILogger _logger;

        public IEnumerable<SubscriptionModel> Subscriptions;

        public RestartVmsModel(IConfiguration config, AzureVmPriceService azureVmPricing, ILogger<RestartVmsModel> logger)
        {
            _azureService = new AzureService(config["ServicePrincipalId"], config["ServicePrincipalSecret"], config["ServicePrincipalTenantId"]);
            _permissionRepository = new PermissionRepository(config["CADConnectionString"]);
            //_azureVmPricing = azureVmPricing;
            _adminGroupId = config["AdminGroupId"];
            _logger = logger;
        }

        private string Username => User.Identity.Name;

        public async Task OnGetAsync()
        {
            Subscriptions = IsUserAdmin()
                ? await _permissionRepository.GetSubscriptionsForAdminAsync()
                : await _permissionRepository.GetSubscriptionsForUserAsync(Username);
        }

        public async Task<IActionResult> OnGetVmsAsync(string subscriptionId)
        {
            var vms = IsUserAdmin()
                ? await _permissionRepository.GetVmsForAdminAsync(subscriptionId)
                : (await _permissionRepository.GetVmsForUserAsync(Username)).Where(x => x.SubscriptionId.Equals(subscriptionId));

            var vmDetailTasks = new List<Task<VmViewModel>>();
            foreach (var vm in vms) {
                vmDetailTasks.Add(GetVmViewModelAsync(vm));
            }

            return new JsonResult(new {data = await Task.WhenAll(vmDetailTasks)});
        }
        
        private async Task<VmViewModel> GetVmViewModelAsync(VmModel vm)
        {
            var vmViewModel = new VmViewModel {
                SubscriptionId = vm.SubscriptionId,
                Subscription = vm.Subscription,
                ResourceGroup = vm.ResourceGroup,
                Name = vm.Name
            };

            //vmViewModel.Schedule = _azureService.GetScheduleTags(JsonConvert.DeserializeObject<Dictionary<string, string>>(vm.Tags));

            //try {
            //    vmViewModel.HourlyCost = await _azureVmPricing.GetVmPriceAsync(vm.Os, vm.Location, vm.Size);
            //}
            //catch (Exception ex) {
            //    _logger.LogError(ex, "An exception occurred getting hourly cost for VM.");
            //}

            //if (vmViewModel.HourlyCost != null) {
            //    vmViewModel.WeeklyCost = vmViewModel.HourlyCost * 168;
            //    vmViewModel.WeeklySavings = GetWeeklyCostSavings(vmViewModel.HourlyCost.Value, vmViewModel.Schedule);

            //    if (vmViewModel.WeeklyCost.Equals(0.0) || vmViewModel.WeeklySavings.Equals(0.0)) {
            //        vmViewModel.WeeklySavingsPercent = 0;
            //    }
            //    else {
            //        vmViewModel.WeeklySavingsPercent = vmViewModel.WeeklySavings / vmViewModel.WeeklyCost * 100;
            //    }
            //}

            return vmViewModel;
        }

        private double GetWeeklyCostSavings(double hourlyCost, ScheduleTagsModel schedule)
        {
            if (string.IsNullOrEmpty(schedule.Days)) return 0;
            if (string.IsNullOrEmpty(schedule.StartTime)) return hourlyCost * 168;
            if (string.IsNullOrEmpty(schedule.StopTime) && !string.IsNullOrEmpty(schedule.StartTime)) return 0;

            var startTime = DateTime.ParseExact(schedule.StartTime, "h tt", CultureInfo.InvariantCulture);
            var stopTime = DateTime.ParseExact(schedule.StopTime, "h tt", CultureInfo.InvariantCulture);
            var hoursTurnedOff = stopTime > startTime ? (TimeSpan.FromHours(24) - (stopTime - startTime)).Hours : (startTime - stopTime).Hours;

            var scheduledDaysCount = schedule.Days.Split(',').Length;
            double weeklyCostSavings = Math.Abs(hoursTurnedOff * scheduledDaysCount) * hourlyCost;

            //get days that the machine doesn't turn on if any
            if (stopTime > startTime) {
                weeklyCostSavings += (7 - scheduledDaysCount) * 24 * hourlyCost;
            }

            return weeklyCostSavings;
        }

        public async Task<IActionResult> OnGetStartVmAsync(VmModel vm)
        {
            if (!HasAccessToVm(vm)) {
                return new JsonResult(null);
            }

            await _permissionRepository.AddAuditLogAsync(Username, "Start VM", vm);

            var azureVm = await _azureService.GetVirtualMachineByResourceGroupAsync(vm.SubscriptionId, vm.ResourceGroup, vm.Name);
            var task = azureVm.StartAsync();

            return new JsonResult(true);
        }

        public async Task<IActionResult> OnGetStopVmAsync(VmModel vm)
        {
            if (!HasAccessToVm(vm)) {
                return new JsonResult(null);
            }

            await _permissionRepository.AddAuditLogAsync(Username, "Stop VM", vm);

            var azureVm = await _azureService.GetVirtualMachineByResourceGroupAsync(vm.SubscriptionId, vm.ResourceGroup, vm.Name);
            var task = azureVm.DeallocateAsync();

            return new JsonResult(true);
        }

        public async Task<IActionResult> OnGetStatusAsync(VmModel vm)
        {
            if (!HasAccessToVm(vm)) {
                return new JsonResult(null);
            }

            var azureVm = await _azureService.GetVirtualMachineByResourceGroupAsync(vm.SubscriptionId, vm.ResourceGroup, vm.Name);
            return new JsonResult(_azureService.GetVmStatus(azureVm).Value);
        }

        private bool HasAccessToVm(VmModel vm)
        {
            return IsUserAdmin() || _permissionRepository.HasAccessToVm(Username, vm);
        }

        private bool IsUserAdmin()
        {
            var hasRole = User.HasClaim(ClaimTypes.Role, "DevOpsAdmin");
            var hasRole2 = User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "DevOpsAdmin");
            return User.HasClaim(ClaimTypes.Role, "DevOpsAdmin");
            //return User.Claims.Any(x => x.Type.Equals("groups", StringComparison.OrdinalIgnoreCase) && x.Value.Equals(_adminGroupId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
