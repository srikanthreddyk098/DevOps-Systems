namespace CalpineAzureDashboard.Data.Repository
{
    public class VaultBackupRepository<T> : BaseRepository <T>
    {
        public VaultBackupRepository(string conn) : base(conn)
        {
        }
    }
}