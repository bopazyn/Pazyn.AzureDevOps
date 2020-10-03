using System;
using System.IO;

namespace Pazyn.AzureDevOps
{
    public class File
    {
        public String Name { get; private set; }
        public Stream Data { get; private set; }

        public File(String name, Stream data)
        {
            Name = name;
            Data = data;
        }
    }
}