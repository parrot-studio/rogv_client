using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Linq;
using System.Collections.Specialized;

namespace rogv
{
    public partial class ROGv : Form
    {
        private WebClient client = new WebClient();
        private FortViewData feData = new FortViewData("FE");
        private FortViewData teData = new FortViewData("TE");
        private FortViewData dataForWatch = null;

        public ROGv()
        {
            InitializeComponent();
        }

        private void rogv_Load(object sender, EventArgs e)
        {
            Setting set = feData.Setting;
            targetPathBox.Text = set.TargetPath;
            chatBox.Text = set.FileHeader;
            serverUrlBox.Text = set.ServerUrl;
            serverKeyBox.Text = set.ServerKey;
            authIdBox.Text = set.AuthId;
            authPassBox.Text = set.AuthPass;

            Setting setTe = teData.Setting;
            targetPathTeBox.Text = setTe.TargetPath;
            chatTeBox.Text = setTe.FileHeader;
            serverUrlTeBox.Text = setTe.ServerUrl;
            serverKeyTeBox.Text = setTe.ServerKey;
            authIdTeBox.Text = setTe.AuthId;
            authPassTeBox.Text = setTe.AuthPass;
        }

        private void targetButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                String path = fbd.SelectedPath;
                targetPathBox.Text = path;
                Setting set = feData.Setting;
                set.TargetPath = path;
                set.Save();
            }
        }

        private void targetTeButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                String path = fbd.SelectedPath;
                targetPathTeBox.Text = path;
                Setting set = teData.Setting;
                set.TargetPath = path;
                set.Save();
            }
        }

        private void chatButton_Click(object sender, EventArgs e)
        {
            String chatHeader = chatBox.Text.Trim();
            if (chatHeader == "")
            {
                MessageBox.Show("チャットタブ名が指定されていません。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Setting set = feData.Setting;
            set.FileHeader = chatHeader;
            set.Save();

            MessageBox.Show("設定されました。",
                "ROGv",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk);
        }

        private void chatTeButton_Click(object sender, EventArgs e)
        {
            String chatHeader = chatTeBox.Text.Trim();
            if (chatHeader == "")
            {
                MessageBox.Show("チャットタブ名が指定されていません。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Setting set = teData.Setting;
            set.FileHeader = chatHeader;
            set.Save();

            MessageBox.Show("設定されました。",
                "ROGv",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk);
        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            Setting set = feData.Setting;
            set.ServerUrl = serverUrlBox.Text;
            set.ServerKey = serverKeyBox.Text;
            set.AuthId = authIdBox.Text;
            set.AuthPass = authPassBox.Text;

            try
            {
                if (PostServer("/status", "", "post", set) == "OK")
                {
                    set.Save();

                    MessageBox.Show("通信テストに成功しました。",
                        "ROGv",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Asterisk);
                }
                else
                {
                    throw new System.Net.WebException();
                }
            }
            catch
            {
                MessageBox.Show("通信に失敗しました。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void serverTeButton_Click(object sender, EventArgs e)
        {
            Setting set = teData.Setting;
            set.ServerUrl = serverUrlTeBox.Text;
            set.ServerKey = serverKeyTeBox.Text;
            set.AuthId = authIdTeBox.Text;
            set.AuthPass = authPassTeBox.Text;

            try
            {
                if (PostServer("/status", "", "post", set) == "OK")
                {
                    set.Save();

                    MessageBox.Show("通信テストに成功しました。",
                        "ROGv",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Asterisk);
                }
                else
                {
                    throw new System.Net.WebException();
                }
            }
            catch
            {
                MessageBox.Show("通信に失敗しました。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void loadSelectButton_Click(object sender, EventArgs e)
        {
            Setting set = feData.Setting;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "テキストファイル|*.txt";
            ofd.InitialDirectory = set.TargetPath;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadBox.Text = ofd.FileName;
            }
        }

        private void loadSelectTeButton_Click(object sender, EventArgs e)
        {
            Setting set = teData.Setting;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "テキストファイル|*.txt";
            ofd.InitialDirectory = set.TargetPath;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadTeBox.Text = ofd.FileName;
            }
        }

        private void loadSubmitButton_Click(object sender, EventArgs e)
        {
            String path = loadBox.Text;
            feData.Mapper.Reset();
            if (UpdateFortsInfo(path, feData.Mapper))
            {
                feData.LastFile = new FileInfo(path);
                ViewFortsInfo(feData);
            }
        }

        private void loadSubmitTeButton_Click(object sender, EventArgs e)
        {
            String path = loadTeBox.Text;
            teData.Mapper.Reset();
            if (UpdateFortsInfo(path, teData.Mapper))
            {
                teData.LastFile = new FileInfo(path);
                ViewFortsInfo(teData);
            }
        }

        private void serverPostButton_Click(object sender, EventArgs e)
        {
            if (UpdateServerInfo(feData))
            {
                MessageBox.Show("サーバに送信しました。",
                    "ROGv",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void serverPostTeButton_Click(object sender, EventArgs e)
        {
            if (UpdateServerInfo(teData))
            {
                MessageBox.Show("サーバに送信しました。",
                    "ROGv",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void serverCutinButton_Click(object sender, EventArgs e)
        {
            if (CutInServerInfo(feData))
            {
                MessageBox.Show("サーバに送信しました。",
                    "ROGv",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void serverCutinTeButton_Click(object sender, EventArgs e)
        {
            if (CutInServerInfo(teData))
            {
                MessageBox.Show("サーバに送信しました。",
                    "ROGv",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void watchButton_Click(object sender, EventArgs e)
        {
            if (dataForWatch != null)
            {
                if (dataForWatch.GvType == "FE")
                {
                    chatLogTimer.Enabled = false;
                    loadPanel.Enabled = true;
                    loadTePanel.Enabled = true;
                    startTePanel.Enabled = true;
                    dataForWatch = null;
                    watchButton.Text = "監視開始";
                    watchTeButton.Text = "監視開始";
                }
                else
                {
                    return;
                }
            }
            else
            {
                String path = targetPathBox.Text;
                if (!Directory.Exists(path))
                {
                    MessageBox.Show("指定されたフォルダは存在しません。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                chatLogTimer.Enabled = true;
                loadPanel.Enabled = false;
                loadTePanel.Enabled = false;
                startTePanel.Enabled = false;
                dataForWatch = feData;
                watchButton.Text = "監視停止";
                watchTeButton.Text = "FE監視中";

                MessageBox.Show("FEの監視を開始しました。",
                       "ROGv",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Asterisk);
            }
        }

        private void watchTeButton_Click(object sender, EventArgs e)
        {
            if (dataForWatch != null)
            {
                if (dataForWatch.GvType == "TE")
                {
                    chatLogTimer.Enabled = false;
                    loadPanel.Enabled = true;
                    loadTePanel.Enabled = true;
                    startPanel.Enabled = true;
                    dataForWatch = null;
                    watchButton.Text = "監視開始";
                    watchTeButton.Text = "監視開始";
                }
                else
                {
                    return;
                }
            }
            else
            {
                String path = targetPathTeBox.Text;
                if (!Directory.Exists(path))
                {
                    MessageBox.Show("指定されたフォルダは存在しません。",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                chatLogTimer.Enabled = true;
                loadPanel.Enabled = false;
                loadTePanel.Enabled = false;
                startPanel.Enabled = false;
                dataForWatch = teData;
                watchButton.Text = "TE監視中";
                watchTeButton.Text = "監視停止";

                MessageBox.Show("TEの監視を開始しました。",
                       "ROGv",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Asterisk);
            }
        }

        private void chatLogTimer_Tick(object sender, EventArgs e)
        {
            if (dataForWatch == null)
            {
                return;
            }

            var di = new DirectoryInfo(dataForWatch.Setting.TargetPath);
            var pattern = "Chat_" + dataForWatch.Setting.FileHeader + "*.txt";
            var targets = di.EnumerateFiles(pattern).OrderBy(f => f.LastWriteTime);

            if (targets.Count() <= 0)
            {
                return;
            }
            var target = targets.Last();
            if (dataForWatch.LastFile == null || dataForWatch.LastFile.LastWriteTime < target.LastWriteTime)
            {
                ApplyLatestServerInfo(dataForWatch);
                if (dataForWatch.Mapper.UpdateTime < target.LastWriteTime)
                {
                    UpdateFortsInfo(target.FullName, dataForWatch.Mapper);
                    UpdateServerInfo(dataForWatch);
                }
                dataForWatch.LastFile = target;
                ViewFortsInfo(dataForWatch);
            }
        }

        private void latestButton_Click(object sender, EventArgs e)
        {
            feData.Mapper.Reset();
            ApplyLatestServerInfo(feData);
            ViewFortsInfo(feData);
            feData.LastFile = null;
        }

        private void latestTeButton_Click(object sender, EventArgs e)
        {
            teData.Mapper.Reset();
            ApplyLatestServerInfo(teData);
            ViewFortsInfo(teData);
            teData.LastFile = null;
        }

        private String PostServer(String path, String data, String method, Setting set)
        {
            if (set == null)
            {
                throw new System.Net.WebException();
            }

            Uri uri = new Uri(set.ServerUrl + path);
            client.Credentials = new NetworkCredential(set.AuthId, set.AuthPass);

            NameValueCollection vc = new NameValueCollection();
            vc.Add("k", set.ServerKey);
            vc.Add("d", data);
            if (method == "put" || method == "delete")
            {
                vc.Add("_method", method);
            }

            return Encoding.UTF8.GetString(client.UploadValues(uri, vc));
        }

        private Boolean UpdateFortsInfo(String path, FortMapper map)
        {
            if (path == "" || !File.Exists(path))
            {
                MessageBox.Show("指定されたファイルは存在しません。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            try
            {
                DateTime uTime = File.GetLastWriteTime(path);
                map.ReadLog(
                    File.ReadAllText(path, Encoding.Default),
                    uTime);

                return true;
            }
            catch
            {
                MessageBox.Show("ログの読み込みに失敗しました。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void SetFortInfo(String id, Label label, FortMapper map)
        {
            Fort fort = map.FortInfo(id);
            if (fort == null)
            {
                label.Text = "-";
            }
            else
            {
                label.Text = fort.GuildName;
            }
        }

        private void ViewFortsInfo(FortViewData data)
        {
            if (data.GvType == "FE")
            {
                ViewFeFortsInfo(data);
            }
            else if (data.GvType == "TE")
            {
                ViewTeFortsInfo(data);
            }
        }

        private void ViewFeFortsInfo(FortViewData data)
        {
            if (data.LastFile != null)
            {
                logUpdateTimeLabel.Text = data.LastFile.LastWriteTime.ToString();
            }
            FortMapper map = data.Mapper;
            dataUpdateTimeLabel.Text = map.UpdateTime.ToString();

            SetFortInfo("V1", resultV1, map);
            SetFortInfo("V2", resultV2, map);
            SetFortInfo("V3", resultV3, map);
            SetFortInfo("V4", resultV4, map);
            SetFortInfo("V5", resultV5, map);
            SetFortInfo("C1", resultC1, map);
            SetFortInfo("C2", resultC2, map);
            SetFortInfo("C3", resultC3, map);
            SetFortInfo("C4", resultC4, map);
            SetFortInfo("C5", resultC5, map);
            SetFortInfo("B1", resultB1, map);
            SetFortInfo("B2", resultB2, map);
            SetFortInfo("B3", resultB3, map);
            SetFortInfo("B4", resultB4, map);
            SetFortInfo("B5", resultB5, map);
            SetFortInfo("L1", resultL1, map);
            SetFortInfo("L2", resultL2, map);
            SetFortInfo("L3", resultL3, map);
            SetFortInfo("L4", resultL4, map);
            SetFortInfo("L5", resultL5, map);
            SetFortInfo("N1", resultN1, map);
            SetFortInfo("N2", resultN2, map);
            SetFortInfo("N3", resultN3, map);
            SetFortInfo("N4", resultN4, map);
            SetFortInfo("N5", resultN5, map);
            SetFortInfo("F1", resultF1, map);
            SetFortInfo("F2", resultF2, map);
            SetFortInfo("F3", resultF3, map);
            SetFortInfo("F4", resultF4, map);
            SetFortInfo("F5", resultF5, map);
        }

        private void ViewTeFortsInfo(FortViewData data)
        {
            if (data.LastFile != null)
            {
                logUpdateTimeTeLabel.Text = data.LastFile.LastWriteTime.ToString();
            }
            FortMapper map = data.Mapper;
            dataUpdateTimeTeLabel.Text = map.UpdateTime.ToString();

            SetFortInfo("G1", resultG1, map);
            SetFortInfo("G2", resultG2, map);
            SetFortInfo("G3", resultG3, map);
            SetFortInfo("G4", resultG4, map);
            SetFortInfo("G5", resultG5, map);
            SetFortInfo("K1", resultK1, map);
            SetFortInfo("K2", resultK2, map);
            SetFortInfo("K3", resultK3, map);
            SetFortInfo("K4", resultK4, map);
            SetFortInfo("K5", resultK5, map);
        }

        private Boolean PostMapperInfo(String path, FortViewData data)
        {
            try
            {
                if (PostServer(path, data.Mapper.Serialize(), "put", data.Setting) == "OK")
                {
                    return true;
                }
                else
                {
                    throw new System.Net.WebException();
                }
            }
            catch
            {
                MessageBox.Show("通信に失敗しました。",
                   "エラー",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                return false;
            }
        }

        private Boolean UpdateServerInfo(FortViewData data)
        {
            return PostMapperInfo("/update", data);
        }

        private Boolean CutInServerInfo(FortViewData data)
        {
            return PostMapperInfo("/cutin", data);
        }

        private Boolean ApplyLatestServerInfo(FortViewData data)
        {
            try
            {
                var ret = PostServer("/latest", "", "post", data.Setting);
                if (ret != null)
                {
                    data.Mapper.ApplyLatestInfo(ret);
                    return true;
                }
                else
                {
                    throw new System.Net.WebException();
                }
            }
            catch
            {
                MessageBox.Show("通信に失敗しました。",
                   "エラー",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
