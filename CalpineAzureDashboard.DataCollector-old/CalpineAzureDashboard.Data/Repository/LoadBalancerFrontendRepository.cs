namespace CalpineAzureDashboard.Data.Repository
{
    public class LoadBalancerFrontendRepository<T> : BaseRepository<T>
    {
        public LoadBalancerFrontendRepository(string conn) : base(conn)
        {
        }
    }
}