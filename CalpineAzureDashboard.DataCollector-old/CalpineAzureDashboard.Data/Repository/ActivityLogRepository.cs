namespace CalpineAzureDashboard.Data.Repository
{
    public class ActivityLogRepository<T> : BaseRepository<T>
    {
        public ActivityLogRepository(string conn) : base(conn)
        {
        }
    }
}