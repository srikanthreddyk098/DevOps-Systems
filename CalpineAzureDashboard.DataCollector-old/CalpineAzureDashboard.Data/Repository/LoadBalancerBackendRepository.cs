namespace CalpineAzureDashboard.Data.Repository
{
    public class LoadBalancerBackendRepository<T> : BaseRepository<T>
    {
        public LoadBalancerBackendRepository(string conn) : base(conn)
        {
        }
    }
}