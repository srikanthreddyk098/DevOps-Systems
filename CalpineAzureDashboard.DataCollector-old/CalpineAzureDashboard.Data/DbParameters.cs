using System.Data;
using Dapper;

namespace CalpineAzureDashboard.Data
{
    public class DbParameters
    {
        public DynamicParameters DynamicParameters { get; }

        public DbParameters()
        {
            DynamicParameters = new DynamicParameters();
        }

        /// <summary>
        /// Add a parameter to this dynamic parameter list.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="precision">The precision of the parameter.</param>
        /// <param name="scale">The scale of the parameter.</param>
        public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null,
            int? size = null, byte? precision = null, byte? scale = null)
        {
            DynamicParameters.Add(name, value, dbType, direction, size, precision, scale);
        }
    }
}