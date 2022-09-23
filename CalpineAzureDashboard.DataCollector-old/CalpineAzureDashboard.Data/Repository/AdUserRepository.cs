namespace CalpineAzureDashboard.Data.Repository
{
    public class AdUserRepository<T> : BaseRepository<T>
    {
        public AdUserRepository(string conn) : base(conn)
        {
        }
    }
}