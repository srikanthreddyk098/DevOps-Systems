namespace CalpineAzureDashboard.Data.Repository
{
    public class VaultBackupJobRepository<T> : BaseRepository <T>
    {
        public VaultBackupJobRepository(string conn) : base(conn)
        {
        }
    }
}