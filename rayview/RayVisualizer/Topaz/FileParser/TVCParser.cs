using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Topaz
{
    public class TVCParser
    {
        public Dictionary<string, string> GeneralParse(Stream input)
        {
            StreamReader reader = new StreamReader(input);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                int splitIndex = line.IndexOf(": ");
                if (splitIndex == -1) throw new TopazException("Malformed TVC entry: {0}", line);
                string key = line.Substring(0, splitIndex);
                if (dict.ContainsKey(key)) throw new TopazException("TVC file contains duplicate key: {0}", key);
                dict[key] = line.Substring(splitIndex + 2);
            }
            return dict;
        }

        public class TVCInfo
        {
            public string OBJFile;
            public string RayFile;
            public string TBCFile;
        }
    }
}
