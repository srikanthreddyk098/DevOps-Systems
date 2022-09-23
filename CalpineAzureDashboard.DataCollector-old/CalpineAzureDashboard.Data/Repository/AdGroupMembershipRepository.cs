using System.Threading.Tasks;

namespace CalpineAzureDashboard.Data.Repository
{
    public class AdGroupMembershipRepository<T> : BaseRepository<T>
    {
        public AdGroupMembershipRepository(string conn) : base(conn)
        {
        }
    }
}
