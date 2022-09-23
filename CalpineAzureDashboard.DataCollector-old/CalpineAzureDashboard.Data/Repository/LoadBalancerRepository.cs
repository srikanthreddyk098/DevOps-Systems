namespace CalpineAzureDashboard.Data.Repository
{
    public class LoadBalancerRepository<T> : BaseRepository<T>
    {
        public LoadBalancerRepository(string conn) : base(conn)
        {
        }
    }
}