﻿using Coreflow.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Coreflow.Activities.Filesystem.Files
{
    public class FileContentFilterLines : ICodeActivity
    {
        public List<string> Execute(string FilePath, string Needle)
        {
            List<string> ret = new List<string>();

            using (StreamReader reader = new StreamReader(FilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(Needle))
                    {
                        ret.Add(line);
                    }
                }
            }

            return ret;
        }
    }
}
