using System;

namespace CAD.Database.Bulk
{
    internal class IdentityException : Exception
    {
        public IdentityException(string message) : base(message + " This library requires the SetIdentityColumn method " +
                                                        "to be configured if an identity column is being used. Please reconfigure your setup and try again.")
        {
        }
    }
}
