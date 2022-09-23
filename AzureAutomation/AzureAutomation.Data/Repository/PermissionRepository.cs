using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AzureAutomation.Models;
using Dapper;

namespace AzureAutomation.Data.Repository
{
    public class PermissionRepository
    {
        private readonly string _conn;

        public PermissionRepository(string conn)
        {
            _conn = conn;
        }

        public async Task<IEnumerable<AdUserModel>> GetAllUsers()
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT * FROM [vw_AA_GetAdUsersWithEmail] ORDER BY [Name]";
            return await db.GetListAsync<AdUserModel>(query);
        }

        public async Task<IEnumerable<AdGroupModel>> GetAllGroups()
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT * FROM [vw_AA_GetAdGroupsWithSecurityEnabled] ORDER BY [Name]";
            return await db.GetListAsync<AdGroupModel>(query);
        }

        public async Task<IEnumerable<PermissionMappingModel>> GetAssignedPermissionsAsync()
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT * FROM [vw_AA_AssignedPermissions]";
            return await db.GetListAsync<PermissionMappingModel>(query);
        }

        public async Task<bool> AddUserPermissionAsync(int userId, int virtualMachineId, string currentUser)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query =
                "INSERT INTO [AA_PermissionMapping] (AdUserObjectId, VirtualMachineAzureId, CreatedBy, CreatedDtUtc) " +
                "VALUES ((SELECT ObjectId from vw_AdUserDetail WHERE Id = @UserId), " +
                "(SELECT AzureId from vw_VirtualMachine WHERE Id = @VirtualMachineId), " +
                "@CreatedBy, @CreatedDtUtc)";
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("VirtualMachineId", virtualMachineId);
            parameters.Add("CreatedBy", currentUser);
            parameters.Add("CreatedDtUtc", DateTime.UtcNow);
            return await db.ExecuteQueryAsync(query, parameters) > 0;
        }

        public async Task<bool> AddGroupPermissionAsync(int groupId, int virtualMachineId, string currentUser)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query =
                "INSERT INTO [AA_PermissionMapping] (AdGroupObjectId, VirtualMachineAzureId, CreatedBy, CreatedDtUtc) " +
                "VALUES ((SELECT ObjectId from vw_AdGroupDetail WHERE Id = @GroupId), " +
                "(SELECT AzureId from vw_VirtualMachine WHERE Id = @VirtualMachineId), " +
                "@CreatedBy, @CreatedDtUtc)";
            var parameters = new DynamicParameters();
            parameters.Add("GroupId", groupId);
            parameters.Add("VirtualMachineId", virtualMachineId);
            parameters.Add("CreatedBy", currentUser);
            parameters.Add("CreatedDtUtc", DateTime.UtcNow);
            return await db.ExecuteQueryAsync(query, parameters) > 0;
        }

        public async Task<bool> DeletePermissionAsync(int idToDelete)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "DELETE FROM [AA_PermissionMapping] WHERE Id = @idToDelete";
            return await db.ExecuteQueryAsync(query, new {idToDelete}) > 0;
        }

        public async Task<IEnumerable<SubscriptionModel>> GetSubscriptionsForUserAsync(string email)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT DISTINCT [SubscriptionId], [Subscription] FROM [fn_AA_GetVmsAssignedToUser](@email)";
            return await db.GetListAsync<SubscriptionModel>(query, new { Email = email });
        }

        public async Task<IEnumerable<SubscriptionModel>> GetSubscriptionsForAdminAsync()
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT DISTINCT SubscriptionId, Subscription FROM [vw_VirtualMachine]";
            return await db.GetListAsync<SubscriptionModel>(query);
        }

        public async Task<IEnumerable<VmModel>> GetVmsForUserAsync(string email)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT [Id], [SubscriptionId], [Subscription], [ResourceGroup], [Name], [Location], [Os], [Size], [Tags] " +
                                 "FROM [fn_AA_GetVmsAssignedToUser](@email)";
            return await db.GetListAsync<VmModel>(query, new {Email = email});
        }

        public async Task<IEnumerable<VmModel>> GetVmsForAdminAsync(string subscriptionId)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT [Id], [SubscriptionId], [Subscription], [ResourceGroup], [Name], [Location], [Os], [Size], [Tags] " +
                                 "FROM [vw_VirtualMachine] WHERE [SubscriptionId] = @subscriptionId";
            return await db.GetListAsync<VmModel>(query, new {SubscriptionId = subscriptionId});
        }

        public bool HasAccessToVm(string email, VmModel vm)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "SELECT [Id], [SubscriptionId], [Subscription], [ResourceGroup], [Name], [Location], [Os], [Size], [Tags] " +
                                 "FROM [fn_AA_GetVmsAssignedToUser](@email) " +
                                 "WHERE [SubscriptionId] = @SubscriptionId " +
                                 "AND [ResourceGroup] = @ResourceGroup " +
                                 "AND [Name] = @Name";

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);
            parameters.Add("@SubscriptionId", vm.SubscriptionId);
            parameters.Add("@ResourceGroup", vm.ResourceGroup);
            parameters.Add("@Name", vm.Name);

            return db.GetAsync<int>(query, parameters).Result > 0;
        }

        public async Task<bool> AddAuditLogAsync(string username, string action, VmModel vm)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "INSERT INTO [AA_AuditLog] ([Username], [SubscriptionId], [Subscription], [ResourceGroup], [Vm], [Action], [TimestampUtc]) " +
                                 "VALUES (@Username, @SubscriptionId, @Subscription, @ResourceGroup, @Name, @Action, GetUtcDate())";

            var parameters = new DynamicParameters();
            parameters.Add("@Username", username);
            parameters.Add("@SubscriptionId", vm.SubscriptionId);
            parameters.Add("@Subscription", vm.Subscription);
            parameters.Add("@ResourceGroup", vm.ResourceGroup);
            parameters.Add("@Name", vm.Name);
            parameters.Add("@Action", action);

            return (await db.ExecuteQueryAsync(query, parameters)).Equals(1);
        }

        public async Task<bool> UpdateVmTags(string subscriptionId, string resourceGroup, string vmName, string tags)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "UPDATE [VirtualMachine] SET [Tags] = @Tags WHERE [SubscriptionId] = @SubscriptionId AND [ResourceGroup] = @ResourceGroup AND [Name] = @Name";

            var parameters = new DynamicParameters();
            parameters.Add("@Tags", tags);
            parameters.Add("@SubscriptionId", subscriptionId);
            parameters.Add("@ResourceGroup", resourceGroup);
            parameters.Add("@Name", vmName);

            return (await db.ExecuteQueryAsync(query, parameters)).Equals(1);
        }
    }
}