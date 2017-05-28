using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SqliteHelper.DataModel;

namespace SqliteHelper.Provider
{
    internal class ContentProvider
    {
        internal List<UpdateMeta> Acquire(string markdownPath)
        {
            try
            {
                UpdateMeta updateMeta;

                Dictionary<string, object> limits;
                Dictionary<string, object> updates;

                List<UpdateMeta> contentUpdateMetaSet = new List<UpdateMeta>();

                Regex regKey = new Regex(@"\[//\]: # \(.*?\)", RegexOptions.IgnoreCase);

                Regex regeSplit = new Regex("-------------------------------------------------------------------");

                StreamReader reader = new StreamReader(markdownPath, Encoding.Default);

                string[] contentRes = regeSplit.Split(reader.ReadToEnd());

                reader.Close();

                foreach (string item in contentRes)
                {
                    updateMeta = new UpdateMeta();

                    limits = new Dictionary<string, object>();
                    updates = new Dictionary<string, object>();

                    limits.Add("CODE", regKey.Match(item.Replace("\n", "").Replace("\r", "")).Value.Replace("[//]: # (", "").Replace(")", ""));
                    updates.Add("CONTENT", item);

                    updateMeta.limits = limits;
                    updateMeta.updates = updates;

                    contentUpdateMetaSet.Add(updateMeta);
                }

                return contentUpdateMetaSet;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("error: {0}", ex));
                return null;
            }
        }
    }
}
