namespace CalpineAzureDashboard.Data.Repository
{
    public class SecurityRuleRepository<T> : BaseRepository <T>
    {
        public SecurityRuleRepository(string conn) : base(conn)
        {
        }
    }
}