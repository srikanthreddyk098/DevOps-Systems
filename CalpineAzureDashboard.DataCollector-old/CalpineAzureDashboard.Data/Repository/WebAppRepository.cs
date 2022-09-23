namespace CalpineAzureDashboard.Data.Repository
{
    public class WebAppRepository<T> : BaseRepository<T>
    {
        public WebAppRepository(string conn) : base(conn)
        {
        }
    }
}