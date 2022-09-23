namespace CalpineAzureDashboard.Data.Repository
{
    public class SubnetServiceEndpointRepository<T> : BaseRepository<T>
    {
        public SubnetServiceEndpointRepository(string conn) : base(conn)
        {
        }
    }
}