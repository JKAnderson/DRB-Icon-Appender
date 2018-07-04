using DSFormats;
using Octokit;
using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Windows.Forms;

namespace DRB_Icon_Appender
{
    public partial class FormMain : Form
    {
        private const string UPDATE_URL = "https://www.nexusmods.com/darksouls/mods/1457?tab=files";
        private const string TPF_PATH = @"\menu\menu.tpf";
        private const string DRB_PATH = @"\menu\menu.drb";
        private static Properties.Settings settings = Properties.Settings.Default;

        private bool remastered;
        private DRBRaw drb;
        private List<string> textures;
        private List<SpriteShape> shapes;

        public FormMain()
        {
            InitializeComponent();
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            Text = "DRB Icon Appender " + System.Windows.Forms.Application.ProductVersion;
            Location = settings.WindowLocation;
            Size = settings.WindowSize;
            if (settings.WindowMaximized)
                WindowState = FormWindowState.Maximized;

            txtGameDir.Text = settings.GameDir;
            loadFiles(txtGameDir.Text, true);

            GitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("DRB-Icon-Appender"));
            try
            {
                Release release = await gitHubClient.Repository.Release.GetLatest("JKAnderson", "DRB-Icon-Appender");
                if (SemVersion.Parse(release.TagName) > System.Windows.Forms.Application.ProductVersion)
                {
                    lblUpdate.Visible = false;
                    LinkLabel.Link link = new LinkLabel.Link();
                    link.LinkData = UPDATE_URL;
                    llbUpdate.Links.Add(link);
                    llbUpdate.Visible = true;
                }
                else
                {
                    lblUpdate.Text = "App up to date";
                }
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is ApiException || ex is ArgumentException)
            {
                lblUpdate.Text = "Update status unknown";
            }
        }

        private void llbUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.WindowMaximized = WindowState == FormWindowState.Maximized;
            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowLocation = Location;
                settings.WindowSize = Size;
            }
            else
            {
                settings.WindowLocation = RestoreBounds.Location;
                settings.WindowSize = RestoreBounds.Size;
            }

            settings.GameDir = txtGameDir.Text;
        }

        private void enableControls(bool enable)
        {
            btnBrowse.Enabled = !enable;
            btnOpen.Enabled = !enable;
            btnAddIcon.Enabled = enable;
            btnSave.Enabled = enable;
            btnClose.Enabled = enable;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ofdExecutable.InitialDirectory = txtGameDir.Text;
            if (ofdExecutable.ShowDialog() == DialogResult.OK)
            {
                txtGameDir.Text = Path.GetDirectoryName(ofdExecutable.FileName);
                loadFiles(txtGameDir.Text);
            }
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtGameDir.Text))
                Process.Start(txtGameDir.Text);
            else
                SystemSounds.Hand.Play();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            closeFiles();
            string drbPath = txtGameDir.Text + DRB_PATH + (remastered ? ".dcx" : "");
            if (File.Exists(drbPath + ".bak"))
            {
                File.Delete(drbPath);
                File.Move(drbPath + ".bak", drbPath);
            }
            loadFiles(txtGameDir.Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteBytes(drb.shpr.Bytes);
            foreach (SpriteShape shape in shapes)
            {
                bw.Position = shape.ShprOffset;
                shape.WriteSHPR(bw, textures);
            }
            drb.shpr.Bytes = bw.Finish();

            byte[] bytes = drb.Write();
            if (remastered)
                bytes = DCX.Compress(bytes);

            string drbPath = txtGameDir.Text + DRB_PATH + (remastered ? ".dcx" : "");
            if (!File.Exists(drbPath + ".bak"))
                File.Copy(drbPath, drbPath + ".bak");
            File.WriteAllBytes(drbPath, bytes);
            SystemSounds.Asterisk.Play();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            loadFiles(txtGameDir.Text);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            closeFiles();
        }

        private bool shapePresent(int id)
        {
            foreach (SpriteShape shape in shapes)
                if (shape.ID == id)
                    return true;
            return false;
        }

        private void btnAddIcon_Click(object sender, EventArgs e)
        {
            int id = (int)nudIconID.Value;

            if (shapePresent(id))
            {
                DialogResult choice = MessageBox.Show("That icon ID is already in use.\nWould you like to use the next available one?",
                    "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (choice == DialogResult.Yes)
                {
                    while (shapePresent(id))
                        id++;
                    if (id > 9999)
                    {
                        showError("ID may not exceed 9999.");
                        return;
                    }
                    nudIconID.Value = id;
                }
                else
                    return;
            }

            SpriteShape shape = new SpriteShape(id, drb, textures, remastered);
            spriteShapeBindingSource.Add(shape);
            shapes.Sort((s1, s2) => s1.ID.CompareTo(s2.ID));

            foreach (DataGridViewRow row in dgvIcons.Rows)
                if ((int)row.Cells[0].Value == id)
                    dgvIcons.CurrentCell = row.Cells[0];
        }

        private void showError(string message, bool silent = false)
        {
            if (!silent)
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private byte[] readFile(string path)
        {
            if (!File.Exists(path))
            {
                showError("File not found:\n" + path);
                return null;
            }

            byte[] result;

            try
            {
                result = File.ReadAllBytes(path);
            }
            catch
            {
                showError("Could not read file:\n" + path);
                return null;
            }

            if (Path.GetExtension(path) == ".dcx")
            {
                try
                {
                    result = DCX.Decompress(result);
                }
                catch
                {
                    showError("Could not decompress file:\n" + path);
                    return null;
                }
            }

            return result;
        }

        private void loadFiles(string gameDir, bool silent = false)
        {
            if (File.Exists(gameDir + "\\DARKSOULS.exe"))
                remastered = false;
            else if (File.Exists(gameDir + "\\DarkSoulsRemastered.exe"))
                remastered = true;
            else
            {
                showError("Dark Souls executable not found in this directory:\n" + gameDir, silent);
                return;
            }

            TPF menuTPF;
            string tpfPath = gameDir + TPF_PATH + (remastered ? ".dcx" : "");
            byte[] tpfBytes = readFile(tpfPath);

            if (tpfBytes == null)
                return;
            else
            {
                try
                {
                    menuTPF = TPF.Unpack(tpfBytes);
                }
                catch
                {
                    showError("Could not unpack TPF:\n" + tpfPath, silent);
                    return;
                }
            }

            DRBRaw menuDRB;
            string drbPath = gameDir + DRB_PATH + (remastered ? ".dcx" : "");
            byte[] drbBytes = readFile(drbPath);

            if (drbBytes == null)
                return;
            else
            {
                try
                {
                    menuDRB = DRBRaw.Read(drbBytes);
                }
                catch
                {
                    showError("Could not read DRB:\n" + drbPath, silent);
                    return;
                }
            }

            fillDataGridView(menuTPF, menuDRB);
            enableControls(true);
        }

        public void fillDataGridView(TPF menuTPF, DRBRaw menuDRB)
        {
            spriteShapeBindingSource.Clear();
            textures = new List<string>();
            foreach (TPFEntry entry in menuTPF.Files)
                textures.Add(entry.Name);

            List<string> sortedNames = new List<string>(textures);
            sortedNames.Sort();
            textureDataGridViewComboBoxColumn.DataSource = sortedNames;

            drb = menuDRB;
            shapes = new List<SpriteShape>();
            foreach (DRBRaw.DLGEntry dlg in menuDRB.dlg.Entries)
            {
                if (dlg.Name == "Icon")
                {
                    foreach (DRBRaw.DLGOEntry dlgo in dlg.DLGOEntries)
                        shapes.Add(new SpriteShape(dlgo, drb, textures, remastered));
                }
            }
            shapes.Sort((s1, s2) => s1.ID.CompareTo(s2.ID));
            spriteShapeBindingSource.DataSource = shapes;
        }

        private void closeFiles()
        {
            spriteShapeBindingSource.Clear();
            drb = null;
            textures = null;
            shapes = null;
            enableControls(false);
        }

        private void dgvIcons_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            SystemSounds.Hand.Play();
            e.Cancel = true;
        }
    }
}
