using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace rogv
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ROGv());
        }
    }

    public class Fort
    {
        private String id;
        private String name;
        private String formalName;
        private String guildName;
        private DateTime updateTime;

        public Fort(String id, String name, String formalName)
        {
            this.id = id;
            this.name = name;
            this.formalName = formalName;
        }

        public String ID
        {
            get { return id; }
        }

        public String Name
        {
            get { return name; }
        }

        public String FormalName
        {
            get { return formalName; }
        }

        public String GuildName
        {
            get { return guildName; }
        }

        public DateTime UpdateTime
        {
            get { return updateTime; }
        }

        public void UpdateGuildStatus(String name, DateTime time)
        {
            guildName = name;
            updateTime = time;
        }
    }

    public class FortMapper
    {
        private String parser = @"砦.*\[(.+) (.)\].*の(.+)を.*?\[(.+)\]";
        private Dictionary<String, Fort> fortMap = new Dictionary<String, Fort>();
        private DateTime updateTime;
        private String buffer = "";

        public void ReadLog(String logs, DateTime time)
        {
            buffer = "";
            foreach (String log in logs.Split('\n'))
            {
                UpdateFortFromLog(log, time);
                updateTime = time;
            }
        }

        private void UpdateFortFromLog(String log, DateTime time)
        {
            buffer += log.Trim();
            foreach (Match m in Regex.Matches(buffer, parser))
            {
                String place = m.Groups[1].Value.Trim();
                String num = m.Groups[2].Value.Trim();
                String formal = m.Groups[3].Value.Trim();
                String guild = m.Groups[4].Value.Trim();

                Char head = place[0];
                if (head == 'V' && place.StartsWith("Valfreya"))
                {
                    head = 'F';
                }

                String id = head + num;
                String name = place + " " + num;

                Fort fort = null;
                if (fortMap.ContainsKey(id))
                {
                    fort = fortMap[id];
                }
                else
                {
                    fort = new Fort(id, name, formal);
                    fortMap[id] = fort;
                }
                fort.UpdateGuildStatus(guild, time);
                buffer = "";
            }
        }

        public void Reset()
        {
            fortMap = new Dictionary<String, Fort>();
            updateTime = DateTime.Now;
        }

        public Fort FortInfo(String id)
        {
            if (!fortMap.ContainsKey(id))
            {
                return null;
            }
            return fortMap[id];
        }

        public String Serialize()
        {
            Dictionary<String, Object> dict = new Dictionary<String, Object>();
            dict.Add("update_time", updateTime.ToString());
            foreach (String id in fortMap.Keys)
            {
                Fort f = fortMap[id];
                dict.Add(id, new
                {
                    id = f.ID,
                    name = f.Name,
                    formal_name = f.FormalName,
                    guild_name = f.GuildName,
                    update_time = f.UpdateTime.ToString()
                });
            }
            return (new JavaScriptSerializer()).Serialize(dict);
        }
    }
}
