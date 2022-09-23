namespace CalpineAzureDashboard.Data.Repository
{
    public class VaultBackupPolicyRepository<T> : BaseRepository <T>
    {
        public VaultBackupPolicyRepository(string conn) : base(conn)
        {
        }
    }
}