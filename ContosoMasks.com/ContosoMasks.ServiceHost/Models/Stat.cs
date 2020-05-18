using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContosoMasks.ServiceHost.Models
{
    public class LoadTestRun
    {
        public LoadTestRun(string url, string id)
        {
            ID = id;
            Url = url;
            Regions = new Dictionary<string, IEnumerable<Stat>>();
            RegionCount = new Dictionary<string, int>();
        }

        public void AddFile(string region, string file)
        {
            var stats = Stat.ParseFile(file);
            IEnumerable<Stat> alreadyThere;

            if ( !Regions.TryGetValue(region, out alreadyThere))
            {
                Regions[region] = stats;
                RegionCount[region] = 1;
            }
            else
            {
                int regionCount = RegionCount[region];

                alreadyThere.SafeForEach(curStat =>
                {
                    var newStat = stats.SafeFirstOrDefault(s => s.Name.EqualsOI(curStat.Name));
                    if ( newStat != null )
                    {
                        curStat.Average = ((curStat.Average * regionCount) + newStat.Average) / (regionCount + 1);
                        curStat.Minimum = ((curStat.Minimum * regionCount) + newStat.Minimum) / (regionCount + 1);
                        curStat.Median = ((curStat.Median * regionCount) + newStat.Median) / (regionCount + 1);
                        curStat.Maximum = ((curStat.Maximum * regionCount) + newStat.Maximum) / (regionCount + 1);
                        curStat.Percentile90 = ((curStat.Percentile90 * regionCount) + newStat.Percentile90) / (regionCount + 1);
                        curStat.Percentile95 = ((curStat.Percentile95 * regionCount) + newStat.Percentile95) / (regionCount + 1);
                    }
                });

                RegionCount[region] = regionCount + 1;
            }
        }

        public Dictionary<string, IEnumerable<Stat>> Regions { get; set; }
        private Dictionary<string, int> RegionCount { get; set; }
        public string ID { get; set; }
        public string Url { get; set; }
    }

    public class Stat
    {
        static Regex LINE = new Regex(@"^\s+([^\s^\.]+)\.+:\s+avg=([\d\.]+\S+)\s+min=([\d\.]+\S+)\s+med=([\d\.]+\S+)\s+max=([\d\.]+\S+)\s+p\(90\)=([\d\.]+\S+)\s+p\(95\)=([\d\.]+\S+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static Regex NUMBER = new Regex(@"^([\d\.]+)([^\d^\.]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase); 
        static Regex MINNUMBER = new Regex(@"^([\d]+)m([\d]+)s$", RegexOptions.Compiled | RegexOptions.IgnoreCase); 

        public static IEnumerable<Stat>  ParseFile(string file)
        {
            var lines = file.Split('\n').Where(line => LINE.IsMatch(line)).ToList();
                
                
            return lines.Select(line =>
            {
                var matches = LINE.Match(line);
                return new Stat()
                {
                    Name = matches.Groups[1].Value,
                    Average = toMicroSeconds(matches.Groups[2].Value),
                    Minimum = toMicroSeconds(matches.Groups[3].Value),
                    Median = toMicroSeconds(matches.Groups[4].Value),
                    Maximum = toMicroSeconds(matches.Groups[5].Value),
                    Percentile90 = toMicroSeconds(matches.Groups[6].Value),
                    Percentile95 = toMicroSeconds(matches.Groups[7].Value),
                };
            }).ToList();
        }

        private static long toMicroSeconds(string val)
        {
            if ( string.IsNullOrEmpty(val))
            {
                return 0;
            }

            if ( MINNUMBER.IsMatch(val))
            {
                var minMatches = MINNUMBER.Match(val);
                return (long)((double.Parse(minMatches.Groups[1].Value) * 1000d * 1000d * 60d) + (double.Parse(minMatches.Groups[1].Value) * 1000d * 1000d));
            }

            var matches = NUMBER.Match(val);
            double number = double.Parse(matches.Groups[1].Value);
            switch (matches.Groups[2].Value)
            {
                case "ms":
                    return (long)(number * 1000d);
                case "s":
                    return (long)(number * 1000d * 1000d);
                case "m":
                    return (long)(number * 1000d * 1000d * 60d);
            }

            return (long)number;
        }

        public string Name { get; set; }
        public long Average { get; set; }
        public long Minimum { get; set; }
        public long Median { get; set; }
        public long Maximum { get; set; }
        public long Percentile90 { get; set; }
        public long Percentile95 { get; set; }
    }
}
