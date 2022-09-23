namespace CalpineAzureDashboard.Data.Repository
{
    public class AdApplicationRepository<T> : BaseRepository <T>
    {
        public AdApplicationRepository(string conn) : base(conn)
        {
        }
    }
}