using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServerTool.Avalonia.Core
{
    class ACFReader
    {

        public string FileLocation { get; private set; }

        public ACFReader(string FileLocation)
        {
            if (File.Exists(FileLocation))
                this.FileLocation = FileLocation;
            else
                throw new FileNotFoundException("Error", FileLocation);
        }

        public bool CheckIntegrity()
        {
            string Content = File.ReadAllText(FileLocation);
            int quote = Content.Count(x => x == '"');
            int braceleft = Content.Count(x => x == '{');
            int braceright = Content.Count(x => x == '}');

            return ((braceleft == braceright) && (quote % 2 == 0));
        }

        public ACF_Struct ACFFileToStruct()
        {
            return ACFFileToStruct(File.ReadAllText(FileLocation));
        }

        private ACF_Struct ACFFileToStruct(string RegionToReadIn)
        {
            ACF_Struct ACF = new ACF_Struct();

            var items = RegionToReadIn.Split('\t');
            string leftItem = "";
            foreach (string item in items)
            {
                if (item == "")
                {
                    continue;
                }

                if (item.Contains("{"))
                {
                    try
                    {
                        string[] subItemsArray = new ArraySegment<string>(items, 1, items.Length - 3).ToArray();
                        string subItemsString = string.Join('\t', subItemsArray);
                        ACF_Struct SubACFStruct = ACFFileToStruct(subItemsString);
                        string itemCleaned = item.Replace("\"", string.Empty).Replace("\n", string.Empty).Replace("{", string.Empty);
                        ACF.SubACF.Add(itemCleaned, SubACFStruct);

                        break;
                    }
                    catch (Exception e) { break; }
                }
                else
                {
                    if (item != "" && !item.Contains("}"))
                    {
                        if (leftItem == "")
                        {
                            leftItem = item.Replace("\"", string.Empty);
                        }
                        else
                        {
                            // add this item to left Item
                            string rightItem = item.Replace("\"", string.Empty).Replace("\n", string.Empty);
                            ACF.SubItems.Add(leftItem, rightItem);
                            leftItem = "";
                        }
                    }
                }
            }
            return ACF;
        }

    }


        class ACF_Struct
        {
            public Dictionary<string, ACF_Struct> SubACF { get; private set; }
            public Dictionary<string, string> SubItems { get; private set; }

            public ACF_Struct()
            {
                SubACF = new Dictionary<string, ACF_Struct>();
                SubItems = new Dictionary<string, string>();
            }

            public void WriteToFile(string File)
            {

            }

            public override string ToString()
            {
                return ToString(0);
            }

        private string ToString(int Depth)
        {
            StringBuilder SB = new StringBuilder();
            foreach (KeyValuePair<string, string> item in SubItems)
            {
                SB.Append('\t', Depth);
                SB.AppendFormat("\"{0}\"\t\t\"{1}\"\r\n", item.Key, item.Value);
            }
            foreach (KeyValuePair<string, ACF_Struct> item in SubACF)
            {
                SB.Append('\t', Depth);
                SB.AppendFormat("\"{0}\"\n", item.Key);
                SB.Append('\t', Depth);
                SB.AppendLine("{");
                SB.Append(item.Value.ToString(Depth + 1));
                SB.Append('\t', Depth);
                SB.AppendLine("}");
            }
            return SB.ToString();
        }
    }


    static class Extension
    {
        public static int NextEndOf(this string str, char Open, char Close, int startIndex)
        {
            if (Open == Close)
                throw new Exception("\"Open\" and \"Close\" char are equivalent!");

            int OpenItem = 0;
            int CloseItem = 0;
            for (int i = startIndex; i < str.Length; i++)
            {
                if (str[i] == Open)
                {
                    OpenItem++;
                }
                if (str[i] == Close)
                {
                    CloseItem++;
                    if (CloseItem > OpenItem)
                        return i;
                }
            }
            throw new Exception("Not enough closing characters!");
        }
    }
}
