using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EditPingMappingAlertsModel : PageModel
    {
        private readonly string _dbType;
        private readonly string _connectionString;
        private readonly PingMappingRepository _pingMappingRepository;

        public EditPingMappingAlertsModel(IConfiguration config)
        {
            _dbType = "sqlserver";
            _connectionString = config["CAMConnectionString"];
            _pingMappingRepository = new PingMappingRepository(_connectionString);
        }

        public async Task<IActionResult> OnGetListAsync()
        {
            var pingMappings = await _pingMappingRepository.GetAllAsync();
            return new JsonResult(new {data = pingMappings});
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            using (var db = new Database(_dbType, _connectionString)) {
                var editor = new Editor(db, "PingMapping", "Id").Model<Models.PingMappingModel>();
                editor.Field(new Field("Subscription").Get(false).Set(false));
                editor.Field(new Field("ResourceGroup").Get(false).Set(false));
                editor.Field(new Field("VmName").Get(false).Set(false));
                editor.Field(new Field("Region").Get(false).Set(false));
                editor.Field(new Field("Hostname").SetFormatter(Format.NullEmpty()));
                editor.Field(new Field("PrivateIp").SetFormatter(Format.NullEmpty()));
                editor.Field(new Field("Frequency").SetFormatter(Format.NullEmpty()).Validator(Validation.Numeric()));
                editor.Field(new Field("Email").SetFormatter(Format.NullEmpty()));
                editor.Field(new Field("IsDisabled").Validator(Validation.NotEmpty()));
                editor.Field(new Field("DisabledUntilDt")
                    .Validator(Validation.DateFormat("MM-dd-yyyy h:mm tt"))
                    .Validator(
                        (value, data, host) =>
                        {
                            if ((data["IsDisabled"]).Equals("True")) {
                                if (string.IsNullOrEmpty(value?.ToString())) {
                                    return "An end date is required when disabling a ping alert.";
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

                var changedValues = new List<Tuple<string, string, string, string>>();
                editor.ValidatedEdit += (sender, e) =>
                {
                    int id = int.Parse(e.Id.ToString());
                    Dictionary<string, object> newValues = e.Values;
                    Models.PingMappingModel oldValues = _pingMappingRepository.GetAsync(id).GetAwaiter().GetResult();
                    changedValues = GetChangedValues(oldValues, newValues);
                };

                editor.PostEdit += async (sender, e) =>
                {
                    var task = _pingMappingRepository.LogChangesAsync(changedValues, User.Identity.Name);
                };

                var response = editor.Process(Request.Form).Data();

                foreach (var data in response.data) {
                    var id = int.Parse(data.First(x => x.Key.Equals("DT_RowId")).Value.ToString().Split("_")[1]);
                    var pingMapping = await _pingMappingRepository.GetAsync(id);

                    //if (data.ContainsKey("Id")) { data["Id"] = pingMapping.Id; }
                    //else { data.Add("Id", pingMapping.Id); }

                    if (data.ContainsKey("Subscription")) { data["Subscription"] = pingMapping.Subscription; }
                    else {
                        data.Add("Subscription", pingMapping.Subscription);
                    }

                    if (data.ContainsKey("ResourceGroup")) {
                        data["ResourceGroup"] = pingMapping.ResourceGroup;
                    }
                    else {
                        data.Add("ResourceGroup", pingMapping.ResourceGroup);
                    }

                    if (data.ContainsKey("VmName")) {
                        data["VmName"] = pingMapping.VmName;
                    }
                    else {
                        data.Add("VmName", pingMapping.VmName);
                    }

                    if (data.ContainsKey("Region")) {
                        data["Region"] = pingMapping.Region;
                    }
                    else {
                        data.Add("Region", pingMapping.Region);
                    }

                    if (data.ContainsKey("Hostname")) {
                        data["Hostname"] = pingMapping.Hostname;
                    }
                    else {
                        data.Add("Hostname", pingMapping.Hostname);
                    }

                    if (data.ContainsKey("PrivateIp")) {
                        data["PrivateIp"] = pingMapping.PrivateIp;
                    }
                    else {
                        data.Add("PrivateIp", pingMapping.PrivateIp);
                    }

                    if (data.ContainsKey("Frequency")) {
                        data["Frequency"] = pingMapping.Frequency;
                    }
                    else {
                        data.Add("Frequency", pingMapping.Frequency);
                    }

                    if (data.ContainsKey("Email")) {
                        data["Email"] = pingMapping.Email;
                    }
                    else {
                        data.Add("Email", pingMapping.Email);
                    }

                    if (data.ContainsKey("IsDisabled")) {
                        data["IsDisabled"] = pingMapping.IsDisabled;
                    }
                    else {
                        data.Add("IsDisabled", pingMapping.IsDisabled);
                    }

                    if (data.ContainsKey("DisabledUntilDt")) {
                        data["DisabledUntilDt"] = pingMapping.DisabledUntilDt;
                    }
                    else {
                        data.Add("DisabledUntilDt", pingMapping.DisabledUntilDt);
                    }

                    if (data.ContainsKey("Comment")) {
                        data["Comment"] = pingMapping.Comment;
                    }
                    else {
                        data.Add("Comment", pingMapping.Comment);
                    }
                }

                return new JsonResult(response);
            }
        }

        private List<Tuple<string, string, string, string>> GetChangedValues(Models.PingMappingModel oldValues, Dictionary<string, object> newValues)
        {
            var changedValues = new List<Tuple<string, string, string, string>>();

            string oldHostName = oldValues.Hostname == null ? null : oldValues.Hostname.Contains("(default)") ? null : oldValues.Hostname;
            string newHostname = string.IsNullOrEmpty(newValues["Hostname"].ToString()) ? null : newValues["Hostname"].ToString();
            if (oldHostName != newHostname) {
                changedValues.Add(new Tuple<string, string, string, string>(oldValues.ResourceId, "Hostname", oldHostName, newHostname));
            }

            string oldPrivateIp = oldValues.PrivateIp == null ? null : oldValues.PrivateIp.Contains("(default)") ? null : oldValues.PrivateIp;
            string newPrivateIp = string.IsNullOrEmpty(newValues["PrivateIp"].ToString()) ? null : newValues["PrivateIp"].ToString();
            if (oldPrivateIp != newPrivateIp) {
                changedValues.Add(new Tuple<string, string, string, string>(oldValues.ResourceId, "PrivateIp", oldPrivateIp, newPrivateIp));
            }

            string oldFrequency = oldValues.Frequency == null ? null : oldValues.Frequency.Contains("(default)") ? null : oldValues.Frequency;
            string newFrequency = string.IsNullOrEmpty(newValues["Frequency"].ToString()) ? null : newValues["Frequency"].ToString();
            if (oldFrequency != newFrequency) {
                changedValues.Add(new Tuple<string, string, string, string>(oldValues.ResourceId, "Frequency", oldFrequency, newFrequency));
            }

            string oldEmail = oldValues.Email == null ? null : oldValues.Email.Contains("(default)") ? null : oldValues.Email;
            string newEmail = string.IsNullOrEmpty(newValues["Email"].ToString()) ? null : newValues["Email"].ToString();
            if (oldEmail != newEmail) {
                changedValues.Add(new Tuple<string, string, string, string>(oldValues.ResourceId, "Email", oldEmail, newEmail));
            }

            string oldIsDisabled = oldValues.IsDisabled == null ? null : oldValues.IsDisabled.Contains("(default)") ? null : oldValues.IsDisabled;
            string newIsDisabled = string.IsNullOrEmpty(newValues["IsDisabled"].ToString()) ? null : newValues["IsDisabled"].ToString();
            if (oldIsDisabled != newIsDisabled) {
                changedValues.Add(new Tuple<string, string, string, string>(oldValues.ResourceId, "IsDisabled", oldIsDisabled, newIsDisabled));
            }

            string oldDisabledUntilDt = oldValues.DisabledUntilDt == null ? null :
                oldValues.DisabledUntilDt.Contains("(default)") ? null : oldValues.DisabledUntilDt;
            string newDisabledUntilDt = string.IsNullOrEmpty(newValues["DisabledUntilDt"].ToString()) ? null : newValues["DisabledUntilDt"].ToString();
            if (oldDisabledUntilDt != newDisabledUntilDt) {
                if (DateTime.TryParse(oldDisabledUntilDt, out DateTime olDateTime) && DateTime.TryParse(newDisabledUntilDt, out DateTime newDateTime)) {
                    if (DateTime.Compare(olDateTime, newDateTime) != 0) {
                        changedValues.Add(
                            new Tuple<string, string, string, string>(oldValues.ResourceId, "DisabledUntilDt", oldDisabledUntilDt, newDisabledUntilDt));
                    }
                }
                else {
                    changedValues.Add(
                        new Tuple<string, string, string, string>(oldValues.ResourceId, "DisabledUntilDt", oldDisabledUntilDt, newDisabledUntilDt));
                }
            }

            string oldComment = oldValues.Comment == null ? null : oldValues.Comment.Contains("(default)") ? null : oldValues.Comment;
            string newComment = string.IsNullOrEmpty(newValues["Comment"].ToString()) ? null : newValues["Comment"].ToString();
            if (oldComment != newComment) {
                changedValues.Add(new Tuple<string, string, string, string>(oldValues.ResourceId, "Comment", oldComment, newComment));
            }

            return changedValues;
        }
    }
}
