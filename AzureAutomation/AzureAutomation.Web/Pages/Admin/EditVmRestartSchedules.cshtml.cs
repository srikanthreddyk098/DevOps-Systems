using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureAutomation.Azure;
using AzureAutomation.Data.Repository;
using AzureAutomation.Models;
using AzureAutomation.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AzureAutomation.Web.Pages.Admin
{
    [Authorize(Roles="DevOpsAdmin")]
    [ValidateAntiForgeryToken]
    public class EditVmRestartSchedulesModel : PageModel
    {
        private readonly PermissionRepository _permissionRepository;
        private readonly AzureService _azureService;

        public IEnumerable<SubscriptionModel> Subscriptions;

        public EditVmRestartSchedulesModel(IConfiguration config)
        {
            _azureService = new AzureService(config["ServicePrincipalId"], config["ServicePrincipalSecret"], config["ServicePrincipalTenantId"]);
            _permissionRepository = new PermissionRepository(config["CADConnectionString"]);
        }

        public async Task OnGetAsync()
        {
            Subscriptions = await _permissionRepository.GetSubscriptionsForAdminAsync();
        }

        public async Task<IActionResult> OnGetVmsAsync(string subscriptionId)
        {
            var vms = await _permissionRepository.GetVmsForAdminAsync(subscriptionId);
            var vmTasks = vms.Select(vm => _azureService.GetVirtualMachineByIdAsync(vm.SubscriptionId, GetVmResourceId(vm))).ToList();

            var vmViewModels = new List<VmViewModel>();

            foreach (var vm in await Task.WhenAll(vmTasks)) {
                vmViewModels.Add(new VmViewModel
                {
                    SubscriptionId = subscriptionId,
                    ResourceGroup = vm.ResourceGroupName,
                    Name = vm.Name,
                    //Schedule = _azureService.GetScheduleTags(vm.Tags)
                });
            }

            return new JsonResult(new {data = vmViewModels});
        }

        public async Task<IActionResult> OnPostUpdateAsync([FromBody]IEnumerable<VmViewModel> vms)
        {
            var tasks = new List<Task<bool>>();

            var vmList = vms.ToList();
            foreach (var vm in vmList) {
                tasks.Add(Task.Run(async () =>
                {
                    vm.Schedule.StartTime = string.IsNullOrEmpty(vm.Schedule.StartTime) ? "" : Convert.ToDateTime(vm.Schedule.StartTime).ToString("HH");
                    vm.Schedule.StopTime = string.IsNullOrEmpty(vm.Schedule.StopTime) ? "" : Convert.ToDateTime(vm.Schedule.StopTime).ToString("HH");

                    var shutdownSchedule = new ScheduleJsonModel {
                        Days = vm.Schedule.Days,
                        On = vm.Schedule.StartTime,
                        Off = vm.Schedule.StopTime
                    };

                    var deleteShutdownSchedule = string.IsNullOrEmpty(shutdownSchedule.Days) && string.IsNullOrEmpty(shutdownSchedule.On) &&
                                                 string.IsNullOrEmpty(shutdownSchedule.Off);

                    var azureVm = await _azureService.GetVirtualMachineByIdAsync(vm.SubscriptionId, GetVmResourceId(vm));
                    
                    var tags = (Dictionary<string, string>) azureVm.Tags;
                    var tagsModified = false;

                    if (tags.ContainsKey("Shutdown_Schedule")) {
                        if (!deleteShutdownSchedule) {
                            tags["Shutdown_Schedule"] = JsonConvert.SerializeObject(shutdownSchedule);
                        }
                        else {
                            tags.Remove("Shutdown_Schedule");
                        }

                        tagsModified = true;
                    }
                    else {
                        if (!deleteShutdownSchedule) {
                            tags.Add("Shutdown_Schedule", JsonConvert.SerializeObject(shutdownSchedule));
                            tagsModified = true;
                        }
                    }

                    if (tagsModified) {
                        var updatedTags = await _azureService.UpdateVmTagAsync(GetVmResourceId(vm), tags, azureVm.Region.Name);

                        //var updatedVm = await azureVm.Update()
                        //    .WithTag("Shutdown_Schedule", JsonConvert.SerializeObject(json))
                        //    .ApplyAsync();

                        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(updatedTags);
                        var completedSuccessfully = await _permissionRepository.UpdateVmTags(vm.SubscriptionId, vm.ResourceGroup, vm.Name, JsonConvert.SerializeObject(dict));

                        var addAuditLog = _permissionRepository.AddAuditLogAsync(User.Identity.Name, "Update schedule",
                            new VmModel {Subscription = vm.Subscription, SubscriptionId = vm.SubscriptionId, ResourceGroup = vm.ResourceGroup, Name = vm.Name});

                        return completedSuccessfully;
                    }

                    return true;
                }));
            }

            var results = await Task.WhenAll(tasks);
            return new JsonResult(results.Length + " schedule(s) were updated successfully");
        }

        public async Task<IActionResult> OnPostDeleteAsync([FromBody] IEnumerable<VmViewModel> vms)
        {
            var tasks = new List<Task<bool>>();

            var vmList = vms.ToList();
            foreach (var vm in vmList) {
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var azureVm = await _azureService.GetVirtualMachineByIdAsync(vm.SubscriptionId, GetVmResourceId(vm));

                        var tags = (Dictionary<string, string>) azureVm.Tags;

                        if (tags.ContainsKey("Shutdown_Schedule")) {
                            tags.Remove("Shutdown_Schedule");

                            var updatedTags = await _azureService.UpdateVmTagAsync(GetVmResourceId(vm), tags, azureVm.Region.Name);

                            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(updatedTags);
                            var completedSuccessfully =
                                await _permissionRepository.UpdateVmTags(vm.SubscriptionId, vm.ResourceGroup, vm.Name, JsonConvert.SerializeObject(dict));

                            var addAuditLog = _permissionRepository.AddAuditLogAsync(User.Identity.Name, "Delete schedule",
                                new VmModel {
                                    Subscription = vm.Subscription, SubscriptionId = vm.SubscriptionId, ResourceGroup = vm.ResourceGroup, Name = vm.Name
                                });

                            return completedSuccessfully;
                        }

                        return true;
                    }));
                }
            }

            var results = await Task.WhenAll(tasks);
            return new JsonResult(results.Length + " schedule(s) were deleted successfully");
        }

        private string GetVmResourceId(VmModel vm)
        {
            return $"/subscriptions/{vm.SubscriptionId}/resourceGroups/{vm.ResourceGroup}/providers/Microsoft.Compute/virtualMachines/{vm.Name}";
        }

        private string GetVmResourceId(VmViewModel vm)
        {
            return $"/subscriptions/{vm.SubscriptionId}/resourceGroups/{vm.ResourceGroup}/providers/Microsoft.Compute/virtualMachines/{vm.Name}";
        }

        private ScheduleTagsModel GetScheduleTags(string tags)
        {
            var tagJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(tags);
            if (tagJson.ContainsKey("Shutdown_Schedule")) {
                var scheduleJson = tagJson["Shutdown_Schedule"];
                if (scheduleJson != null) {
                    var schedule = JsonConvert.DeserializeObject<Dictionary<string, string>>(scheduleJson);
                    var scheduleTagsModel = new ScheduleTagsModel();
                    scheduleTagsModel.Days = schedule["Days"];

                    if (int.TryParse(schedule["On"].Split(':')[0], out var scheduledStartTimeInt)) {
                        scheduleTagsModel.StartTime = new DateTime(TimeSpan.FromHours(scheduledStartTimeInt).Ticks).ToString("h tt");
                    }

                    if (int.TryParse(schedule["Off"].Split(':')[0], out var scheduledStopTimeInt)) {
                        scheduleTagsModel.StopTime =
                            new DateTime(TimeSpan.FromHours(scheduledStopTimeInt).Ticks).ToString("h tt");
                    }

                    return scheduleTagsModel;
                }
            }

            return new ScheduleTagsModel();
        }
    }
}
