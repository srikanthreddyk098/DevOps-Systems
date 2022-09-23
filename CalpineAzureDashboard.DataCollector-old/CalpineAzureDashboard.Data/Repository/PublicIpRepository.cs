namespace CalpineAzureDashboard.Data.Repository
{
    public class PublicIpRepository<T> : BaseRepository<T>
    {
        public PublicIpRepository(string conn) : base(conn)
        {
        }
    }
}