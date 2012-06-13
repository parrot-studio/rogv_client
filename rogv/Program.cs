using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Collections;

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

        public DateTime UpdateTime
        {
            get { return updateTime; }
        }

        public void ReadLog(String logs, DateTime time)
        {
            buffer = "";
            foreach (String log in logs.Split('\n'))
            {
                UpdateFortFromLog(log, time);
                updateTime = time;
            }
        }

        public void ApplyLatestInfo(String stat)
        {
            var data = (new JavaScriptSerializer()).Deserialize<Dictionary<String, Object>>(stat);
            var utime = DateTime.Parse((String)data["update_time"]);
            if (this.updateTime != null && this.updateTime >= utime)
            {
                return;
            }

            foreach (Object d in (ArrayList)data["forts"])
            {
                var fd = (Dictionary<String, Object>)d;
                UpdateGuildStatus((String)fd["fort_id"],
                    (String)fd["fort_name"], (String)fd["formal_name"],
                    (String)fd["guild_name"], DateTime.Parse((String)fd["update_time"]));
            }
            this.updateTime = utime;
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

                UpdateGuildStatus(id, name, formal, guild, time);
                buffer = "";
            }
        }

        public void Reset()
        {
            fortMap = new Dictionary<String, Fort>();
            updateTime = new DateTime(2001,1,1);
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

        private void UpdateGuildStatus(String fid, String fortName, String formalName,
            String guildName, DateTime utime)
        {
            Fort fort = null;
            if (fortMap.ContainsKey(fid))
            {
                fort = fortMap[fid];
            }
            else
            {
                fort = new Fort(fid, fortName, formalName);
                fortMap[fid] = fort;
            }
            fort.UpdateGuildStatus(guildName, utime);
        }
    }
}
