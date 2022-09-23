namespace CalpineAzureDashboard.Data.Repository
{
    public class SubnetRepository<T> : BaseRepository<T>
    {
        public SubnetRepository(string conn) : base(conn)
        {
        }
    }
}