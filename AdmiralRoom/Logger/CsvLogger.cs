﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CC0022

namespace Huoyaoyuan.AdmiralRoom.Logger
{
    public class CsvLogger<T> : Logger<T>
    {
        private readonly FileInfo file;
        private readonly Action CheckFile;
        private readonly string fileheader;
        public CsvLogger(string filename, bool useshown = false) : base(useshown)
        {
            file = new FileInfo(filename);
            string[] titles = useshown ? provider.Titles.Select(GetText).ToArray() : provider.Titles;
            fileheader = string.Join(",", titles);
            CheckFile = () =>
            {
                if (!file.Exists)
                    using (var writer = file.CreateText())
                    {
                        writer.WriteLine(fileheader);
                        writer.Flush();
                    }
            };
            CheckFile();
            using (var reader = file.OpenText())
                if (reader.ReadLine().Trim() == fileheader) return;
            try
            {
                file.MoveTo(filename + ".backup");
                file = new FileInfo(filename);
                CheckFile();
            }
            catch (IOException) { }
        }
        public override void Log(T item)
        {
            CheckFile();
            using (var fs = file.Open(FileMode.Append, FileAccess.Write))
            {
                var writer = new StreamWriter(fs);
                writer.WriteLine(string.Join(",", provider.GetValues(item)));
                writer.Flush();
            }
        }
        public override void Import(IEnumerable<T> items)
        {
            CheckFile();
            using (var fs = file.Open(FileMode.Append, FileAccess.Write))
            {
                var writer = new StreamWriter(fs);
                foreach (var item in items)
                    writer.WriteLine(string.Join(",", provider.GetValues(item)));
                writer.Flush();
            }
        }
        public override IEnumerable<T> Read()
        {
            using (var reader = file.OpenText())
            {
                reader.ReadLine();//标题
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var values = line.Split(',');
                    yield return provider.GetItem(values);
                }
            }
        }
    }
}