using System;

namespace DiskAccessLibrary
{
    internal class DriveNotFoundException : Exception
    {
        public DriveNotFoundException()
        {
        }

        public DriveNotFoundException(string message) : base(message)
        {
        }

        public DriveNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}