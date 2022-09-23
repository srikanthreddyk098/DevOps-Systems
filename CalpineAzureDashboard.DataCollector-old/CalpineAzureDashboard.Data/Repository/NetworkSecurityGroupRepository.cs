namespace CalpineAzureDashboard.Data.Repository
{
    public class NetworkSecurityGroupRepository<T> : BaseRepository <T>
    {
        public NetworkSecurityGroupRepository(string conn) : base(conn)
        {
        }
    }
}