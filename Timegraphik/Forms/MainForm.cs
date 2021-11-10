using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using Timegraphik.Data;
using Timegraphik.GUI;
using System.Media;
using System.IO;

namespace Timegraphik.Forms {
    public partial class MainForm : Form {
        public static Storage Storage { get; } = new Storage();
        public static MainGui mainGui;
        public static DatabaseGui dbGui;
        public MainForm() {
            InitializeComponent();

            //Size = MaximumSize;

            ViewScheduleMenuItem.Click += ViewSchedule;
            editToolStripMenuItem.Click += OpenDatabase;
            clearToolStripMenuItem.Click += ClearDatabase;
            clearToolStripMenuItem.Click += GroupsCheck;
            AcceptChangesButton.Click += AcceptChanges;
            AcceptChangesButton.Click += GroupsCheck;
            CancelChangesButton.Click += LoadFromStorage;
        }

        private static ToolTip tooltip = new ToolTip();

        private void ClearDatabase(object sender, EventArgs e) {
            var res = MessageBox.Show("Вы действительно хотите очистить всю базу данных?",
                "Предупреждение",
                MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes) {
                mainGui.ClearSchedule();
                LoadFromStorage(sender, e);
            }
        }

        
        private void Form_OnLoad(object sender, EventArgs e) {           
            Storage.Load();
            mainGui = new MainGui(Controls);
            dbGui = new DatabaseGui(Controls);

            if (Storage.Data.groups.Count == 0) {
                var add = new AddGroupForm();
                add.ShowDialog();
            }
            SelectFirstGroup(sender, e);
            LoadFromStorage(sender, e);

            if (Storage.Settings.ServerConnection == null) {
                Storage.Settings.ServerConnection = "Data Source=desktop-t7m86ng\\sqlexpress;Initial Catalog=timegraphik;Integrated Security=True;";
            }
            SQLSystem.ConnectionString = Storage.Settings.ServerConnection;

            SQLSystem.Start();

            SetTooltips();
        }

        private void SetTooltips() {
            tooltip.SetToolTip(pictureBox11, "Просмотр расписания");
            tooltip.SetToolTip(pictureBox9, "Случайное расписание");
            tooltip.SetToolTip(pictureBox10, "Очистить базу данных");
            tooltip.SetToolTip(iconPush, "Выгрузить на сервер");
            tooltip.SetToolTip(iconPull, "Загрузить с сервера");
            tooltip.SetToolTip(iconSave, "Сохранить в файл (Экспорт)");
            tooltip.SetToolTip(iconOpen, "Открыть файл (Импорт)");
        }

        private void GroupsCheck(object sender, EventArgs e) {
            var groupsEmpty = Storage.Data.groups.Count == 0;
            mainGui.FieldsEnable(!groupsEmpty);
            if (!groupsEmpty && mainGui.GroupField.Text == "") SelectFirstGroup(sender, e);
        }

        private void SelectFirstGroup(object sender, EventArgs e) {
            mainGui.SelectFirstGroup();
        }

        private void Form_OnClosed(object sender, FormClosedEventArgs e) {
            if (mainGui.Groups.Latest != "") mainGui.UnloadSchedule(mainGui.Dates.Latest, mainGui.Groups.Latest);
            Storage.Unload();
        }

        private void ViewSchedule(object sender, EventArgs e) {
            var openForm = new OpenScheduleDialog(Storage, mainGui);
            openForm.ShowDialog();
            
        }

        private void OpenDatabase(object sender, EventArgs e) {
            var dbManager = new DatabaseEditor(Storage);
            dbManager.Show();
        }

        private void HelpMenuItem_Click(object sender, EventArgs e) {
            new AboutBox().ShowDialog();
        }

        private void AcceptChanges(object sender, EventArgs e) {
            dbGui.UnloadCategory(dbGui.Selected.Latest);
            dbGui.UnloadToStorage();
        }

        public static void LoadFromStorage(object sender, EventArgs e) {
            dbGui.LoadFromStorage();
            dbGui.LoadNewCategory();
        }

        private void MenuExport(object sender, EventArgs e) {
            var dialog = new SaveFileDialog() {
                Filter = "Timegraphik Data (*.tgd)|*.tgd",
                DefaultExt = "tgd",
                AddExtension = true
            };

            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            if (dialog.FilterIndex == 1) Storage.SaveFile(dialog.FileName);
        }

        private void MenuImport(object sender, EventArgs e) {
            var dialog = new OpenFileDialog() {
                Filter = "Timegraphik Data (*.tgd)|*.tgd",
                DefaultExt = "tgd",                
                AddExtension = true
            };

            if (dialog.ShowDialog() == DialogResult.Cancel) return;

            if (dialog.FilterIndex == 1) Storage.LoadFile(dialog.FileName);
        }

        private void FiilRandomMenuItem_Click(object sender, EventArgs e) {
            mainGui.FieldsAllRandom();
        }

        private void iconPushRemote(object sender, EventArgs e) {
            AcceptChanges(sender, e);
            SQLSystem.StorageToTables();

            mainGui.UnloadSchedule(mainGui.Dates.Latest, mainGui.Groups.Latest);
            SQLSystem.StorageToSchedules();
        }

        private void iconPullRemote(object sender, EventArgs e) {
            SQLSystem.TablesToStorage();
            LoadFromStorage(sender, e);

            SQLSystem.SchedulesToStorage();
            mainGui.LoadSchedule();
        }

        private void pictureBox9_Click(object sender, EventArgs e) {
            FiilRandomMenuItem_Click(sender, e);
        }

        private void pictureBox10_Click(object sender, EventArgs e) {
            ClearDatabase(sender, e);
        }

        private void pictureBox11_Click(object sender, EventArgs e) {
            ViewSchedule(sender, e);
        }

        private void pictureBox11_MouseHover(object sender, EventArgs e) {
            
        }

        private void pictureBox9_MouseHover(object sender, EventArgs e) {
            //tooltip.SetToolTip(pictureBox9, "Случайное расписание");
        }

        private void pictureBox10_MouseHover(object sender, EventArgs e) {
            //tooltip.SetToolTip(pictureBox10, "Очистить базу данных");
        }

        private void pictureBox3_MouseHover(object sender, EventArgs e) {
            //tooltip.SetToolTip(iconPush, "Выгрузить на сервер");
        }

        private void pictureBox4_MouseHover(object sender, EventArgs e) {
            //tooltip.SetToolTip(iconPull, "Загрузить с сервера");
        }

        private void iconSave_Click(object sender, EventArgs e) {
            MenuExport(sender, e);
        }

        private void iconOpen_Click(object sender, EventArgs e) {
            MenuImport(sender, e);
            LoadFromStorage(sender, e);
            mainGui.LoadSchedule();
        }

        private void iconSave_Hover(object sender, EventArgs e) {
            //tooltip.SetToolTip(iconSave, "Экспорт");
        }

        private void iconOpen_Hover(object sender, EventArgs e) {
            //tooltip.SetToolTip(iconOpen, "Импорт");
        }
    }
}
