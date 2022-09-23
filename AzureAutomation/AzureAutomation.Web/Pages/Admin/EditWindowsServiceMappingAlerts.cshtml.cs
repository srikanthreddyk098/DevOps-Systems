using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureAutomation.Data.Repository;
using DataTables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace AzureAutomation.Web.Pages.Admin
{
    [Authorize(Roles="DevOpsAdmin")]
    [ValidateAntiForgeryToken]
    public class EditWindowsServiceMappingAlertsModel : PageModel
    {
        private readonly string _dbType;
        private readonly string _connectionString;
        private readonly WindowsServiceMappingRepository _windowsServiceMappingRepository;

        public EditWindowsServiceMappingAlertsModel(IConfiguration config)
        {
            _dbType = "sqlserver";
            _connectionString = config["CAMConnectionString"];
            _windowsServiceMappingRepository = new WindowsServiceMappingRepository(_connectionString);
        }

        public async Task<IActionResult> OnGetListAsync()
        {
            var windowsServiceMappings = await _windowsServiceMappingRepository.GetAllAsync();
            return new JsonResult(new {data = windowsServiceMappings });
        }

        public async Task<IActionResult> OnPostEditAsync(string action)
        {
            using (var db = new Database(_dbType, _connectionString)) {
                var editor = new Editor(db, "WindowsServiceMapping", "Id").Model<Models.WindowsServiceMappingModel>();
                editor.Field(new Field("Vm").Validator(Validation.NotEmpty()));
                editor.Field(new Field("Service").Validator(Validation.NotEmpty()));
                editor.Field(new Field("Frequency").Validator(Validation.NotEmpty()).Validator(Validation.Numeric()).Validator(Validation.MinNum(1, null)));
                editor.Field(new Field("Email").Validator(Validation.NotEmpty()));
                editor.Field(new Field("IsDisabled").Validator(Validation.NotEmpty()));
                editor.Field(new Field("DisabledUntilDt")
                    .Validator(Validation.DateFormat("MM-dd-yyyy h:mm tt"))
                    .Validator(
                        (value, data, host) =>
                        {
                            if ((data["IsDisabled"]).Equals("True")) {
                                if (string.IsNullOrEmpty(value?.ToString())) {
                                    return "An end date is required when disabling a Windows service alert.";
                                }

                                if (DateTime.Parse(data["DisabledUntilDt"].ToString()) < TimeZoneInfo.ConvertTime(DateTime.UtcNow,
                                        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"))) {
                                    return "Disable until date and time cannot be before the current date and time.";
                                }
                            }

                            return null;
                        })
                    .SetFormatter(Format.NullEmpty()));
                editor.Field(new Field("Comment").Validator(Validation.NotEmpty(new ValidationOpts {
                    Message = "A reason for this change must be entered."
                })));

                var changedValues = new Dictionary<int, List<Tuple<string, string, string, string, string>>>();
                
                editor.ValidatedCreate += (sender, e) =>
                {
                    changedValues.Add(0, GetChangedValues(null, e.Values));
                };

                editor.PostCreate += (sender, e) =>
                {
                    var task = _windowsServiceMappingRepository.LogChangesAsync(action, changedValues[0], User.Identity.Name);
                };

                editor.ValidatedEdit += (sender, e) =>
                {
                    int id = int.Parse(e.Id.ToString());
                    Dictionary<string, object> newValues = e.Values;
                    Models.WindowsServiceMappingModel oldValues = _windowsServiceMappingRepository.GetAsync(id).GetAwaiter().GetResult();
                    changedValues.Add(id, GetChangedValues(oldValues, newValues));
                };
                
                editor.PostEdit += (sender, e) =>
                {
                    int id = int.Parse(e.Id.ToString());
                    var task = _windowsServiceMappingRepository.LogChangesAsync(action, changedValues[id], User.Identity.Name);
                };

                editor.PreRemove += (sender, e) =>
                {
                    int id = int.Parse(e.Id.ToString());
                    Models.WindowsServiceMappingModel oldValues = _windowsServiceMappingRepository.GetAsync(id).GetAwaiter().GetResult();
                    changedValues.Add(id, GetChangedValues(oldValues, null));
                };

                editor.PostRemove += (sender, e) =>
                {
                    int id = int.Parse(e.Id.ToString());
                    var task = _windowsServiceMappingRepository.LogChangesAsync(action, changedValues[id], User.Identity.Name);
                };

                var response = editor.Process(Request.Form).Data();

                return new JsonResult(response);
            }
        }

        private List<Tuple<string, string, string, string, string>> GetChangedValues(Models.WindowsServiceMappingModel oldValues, Dictionary<string, object> newValues)
        {
            var changedValues = new List<Tuple<string, string, string, string, string>>();
            string vmName = oldValues?.Vm ?? newValues?["Vm"].ToString();
            string serviceName = oldValues?.Service ?? newValues?["Service"].ToString();

            string oldFrequency = oldValues?.Frequency;
            string newFrequency = string.IsNullOrEmpty(newValues?["Frequency"].ToString()) ? null : newValues["Frequency"].ToString();
            if (oldFrequency != newFrequency) {
                changedValues.Add(new Tuple<string, string, string, string, string>(vmName, serviceName, "Frequency", oldFrequency, newFrequency));
            }

            string oldEmail = oldValues?.Email;
            string newEmail = string.IsNullOrEmpty(newValues?["Email"].ToString()) ? null : newValues["Email"].ToString();
            if (oldEmail != newEmail) {
                changedValues.Add(new Tuple<string, string, string, string, string>(vmName, serviceName, "Email", oldEmail, newEmail));
            }

            string oldIsDisabled = oldValues?.IsDisabled;
            string newIsDisabled = string.IsNullOrEmpty(newValues?["IsDisabled"].ToString()) ? null : newValues["IsDisabled"].ToString();
            if (oldIsDisabled != newIsDisabled) {
                changedValues.Add(new Tuple<string, string, string, string, string>(vmName, serviceName, "IsDisabled", oldIsDisabled, newIsDisabled));
            }

            string oldDisabledUntilDt = oldValues?.DisabledUntilDt;
            string newDisabledUntilDt = string.IsNullOrEmpty(newValues?["DisabledUntilDt"].ToString()) ? null : newValues["DisabledUntilDt"].ToString();
            if (oldDisabledUntilDt != newDisabledUntilDt) {
                if (DateTime.TryParse(oldDisabledUntilDt, out DateTime olDateTime) && DateTime.TryParse(newDisabledUntilDt, out DateTime newDateTime)) {
                    if (DateTime.Compare(olDateTime, newDateTime) != 0) {
                        changedValues.Add(
                            new Tuple<string, string, string, string, string>(vmName, serviceName, "DisabledUntilDt", oldDisabledUntilDt, newDisabledUntilDt));
                    }
                }
                else {
                    changedValues.Add(
                        new Tuple<string, string, string, string, string>(vmName, serviceName, "DisabledUntilDt", oldDisabledUntilDt, newDisabledUntilDt));
                }
            }

            string oldComment = oldValues?.Comment;
            string newComment = string.IsNullOrEmpty(newValues?["Comment"].ToString()) ? null : newValues["Comment"].ToString();
            if (oldComment != newComment) {
                changedValues.Add(new Tuple<string, string, string, string, string>(vmName, serviceName, "Comment", oldComment, newComment));
            }

            return changedValues;
        }
    }
}
