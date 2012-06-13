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
        private FortMapper mapper = new FortMapper();
        private Boolean watching = false;
        private FileInfo lastFile;

        public ROGv()
        {
            InitializeComponent();
        }

        private void rogv_Load(object sender, EventArgs e)
        {
            targetPathBox.Text = Properties.Settings.Default.TargetPath;
            chatBox.Text = Properties.Settings.Default.FileHeader;
            serverUrlBox.Text = Properties.Settings.Default.ServerURL;
            serverKeyBox.Text = Properties.Settings.Default.ServerKey;
            authIdBox.Text = Properties.Settings.Default.AuthId;
            authPassBox.Text = Properties.Settings.Default.AuthPass;
            Properties.Settings.Default.Save();
        }

        private void targetButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                String path = fbd.SelectedPath;
                targetPathBox.Text = path;
                Properties.Settings.Default.TargetPath = path;
                Properties.Settings.Default.Save();
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

            Properties.Settings.Default.FileHeader = chatHeader;
            Properties.Settings.Default.Save();

            MessageBox.Show("設定されました。",
                "ROGv",
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk);
        }

        private void serverButton_Click(object sender, EventArgs e)
        {
            String url = serverUrlBox.Text;
            String key = serverKeyBox.Text;
            String aId = authIdBox.Text;
            String aPass = authPassBox.Text;

            try
            {
                if (PostServer("/status", "", "post", url, key, aId, aPass) == "OK")
                {
                    Properties.Settings.Default.ServerURL = url;
                    Properties.Settings.Default.ServerKey = key;
                    Properties.Settings.Default.AuthId = aId;
                    Properties.Settings.Default.AuthPass = aPass;
                    Properties.Settings.Default.Save();

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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "テキストファイル|*.txt";
            ofd.InitialDirectory = Properties.Settings.Default.TargetPath;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                loadBox.Text = ofd.FileName;
            }
        }

        private void loadSubmitButton_Click(object sender, EventArgs e)
        {
            String path = loadBox.Text;
            mapper.Reset();
            if (UpdateFortsInfo(path))
            {
                lastFile = new FileInfo(path);
                ViewFortsInfo();
            }
        }

        private void serverPostButton_Click(object sender, EventArgs e)
        {
            if (UpdateServerInfo())
            {
                MessageBox.Show("サーバに送信しました。",
                    "ROGv",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void serverCutinButton_Click(object sender, EventArgs e)
        {
            if (CutInServerInfo())
            {
                MessageBox.Show("サーバに送信しました。",
                    "ROGv",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void watchButton_Click(object sender, EventArgs e)
        {
            if (watching)
            {
                chatLogTimer.Enabled = false;
                loadPanel.Enabled = true;
                watching = false;
                watchButton.Text = "監視開始";
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
                watching = true;
                watchButton.Text = "監視停止";

                MessageBox.Show("フォルダの監視を開始しました。",
                       "ROGv",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Asterisk);
            }
        }

        private void chatLogTimer_Tick(object sender, EventArgs e)
        {
            var di = new DirectoryInfo(targetPathBox.Text);
            var pattern = "Chat_" + chatBox.Text + "*.txt";
            var target = di.EnumerateFiles(pattern).OrderBy(f => f.LastWriteTime).Last();
            if (target == null)
            {
                return;
            }
            if (lastFile == null || lastFile.LastWriteTime < target.LastWriteTime)
            {
                ApplyLatestServerInfo();
                if (mapper.UpdateTime < target.LastWriteTime)
                {
                    UpdateFortsInfo(target.FullName);
                    UpdateServerInfo();
                }
                lastFile = target;
                ViewFortsInfo();
            }
        }

        private void latestButton_Click(object sender, EventArgs e)
        {
            mapper.Reset();
            ApplyLatestServerInfo();
            ViewFortsInfo();
            lastFile = null;
        }

        private String PostServer(String path, String data, String method,
            String url = null, String key = null,
            String authId = null, String authPass = null)
        {

            if (url == null)
            {
                url = Properties.Settings.Default.ServerURL;
            }
            if (key == null)
            {
                key = Properties.Settings.Default.ServerKey;
            }
            if (authId == null)
            {
                authId = Properties.Settings.Default.AuthId;
            }
            if (authPass == null)
            {
                authPass = Properties.Settings.Default.AuthPass;
            }

            Uri uri = new Uri(url + path);
            client.Credentials = new NetworkCredential(authId, authPass);

            NameValueCollection vc = new NameValueCollection();
            vc.Add("k", key);
            vc.Add("d", data);
            if (method == "put" || method == "delete")
            {
                vc.Add("_method", method);
            }

            return Encoding.UTF8.GetString(client.UploadValues(uri, vc));
        }

        private Boolean UpdateFortsInfo(String path)
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
                mapper.ReadLog(
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

        private void SetFortInfo(String id, Label label)
        {
            Fort fort = mapper.FortInfo(id);
            if (fort == null)
            {
                label.Text = "-";
            }
            else
            {
                label.Text = fort.GuildName;
            }
        }

        private void ViewFortsInfo()
        {
            if (lastFile != null)
            {
                logUpdateTimeLabel.Text = lastFile.LastWriteTime.ToString();
            }
            dataUpdateTimeLabel.Text = mapper.UpdateTime.ToString();

            SetFortInfo("V1", resultV1);
            SetFortInfo("V2", resultV2);
            SetFortInfo("V3", resultV3);
            SetFortInfo("V4", resultV4);
            SetFortInfo("V5", resultV5);
            SetFortInfo("C1", resultC1);
            SetFortInfo("C2", resultC2);
            SetFortInfo("C3", resultC3);
            SetFortInfo("C4", resultC4);
            SetFortInfo("C5", resultC5);
            SetFortInfo("B1", resultB1);
            SetFortInfo("B2", resultB2);
            SetFortInfo("B3", resultB3);
            SetFortInfo("B4", resultB4);
            SetFortInfo("B5", resultB5);
            SetFortInfo("L1", resultL1);
            SetFortInfo("L2", resultL2);
            SetFortInfo("L3", resultL3);
            SetFortInfo("L4", resultL4);
            SetFortInfo("L5", resultL5);
            SetFortInfo("N1", resultN1);
            SetFortInfo("N2", resultN2);
            SetFortInfo("N3", resultN3);
            SetFortInfo("N4", resultN4);
            SetFortInfo("N5", resultN5);
            SetFortInfo("F1", resultF1);
            SetFortInfo("F2", resultF2);
            SetFortInfo("F3", resultF3);
            SetFortInfo("F4", resultF4);
            SetFortInfo("F5", resultF5);
        }

        private Boolean PostMapperInfo(String path)
        {
            try
            {
                if (PostServer(path, mapper.Serialize(), "put") == "OK")
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

        private Boolean UpdateServerInfo()
        {
            return PostMapperInfo("/update");
        }

        private Boolean CutInServerInfo()
        {
            return PostMapperInfo("/cutin");
        }

        private Boolean ApplyLatestServerInfo()
        {
            try
            {
                var ret = PostServer("/latest", "", "post");
                if (ret != null)
                {
                    mapper.ApplyLatestInfo(ret);
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
