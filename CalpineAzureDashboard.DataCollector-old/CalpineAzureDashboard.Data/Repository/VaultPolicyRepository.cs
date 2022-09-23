namespace CalpineAzureDashboard.Data.Repository
{
    public class VaultPolicyRepository<T> : BaseRepository <T>
    {
        public VaultPolicyRepository(string conn) : base(conn)
        {
        }
    }
}