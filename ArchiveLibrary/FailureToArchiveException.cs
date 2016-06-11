using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveLibrary
{
    /// <summary>
    /// Custom exception for when we fail to archive so that I can catch this and say "Oh!, this Failed!" in one central location
    /// </summary>
    public class FailureToArchiveException : Exception
    {
        public FailureToArchiveException() : base() { }
        public FailureToArchiveException(string message) : base(message) { }
        public FailureToArchiveException(string message, Exception inner) : base(message,inner) { }
    }
}
