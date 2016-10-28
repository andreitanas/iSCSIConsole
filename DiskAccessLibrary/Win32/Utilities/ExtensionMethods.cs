using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DiskAccessLibrary
{
    public static class ExtensionMethods
    {
        public static void Close(this SafeHandle handle)
        {
            handle.Dispose();
        }

        public static void Close(this Stream stream)
        {
            stream.Dispose();
        }
    }
}
