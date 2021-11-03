﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Timegraphik.Helpers;
using Timegraphik.Data;

namespace Timegraphik.GUI {
    public class DatabaseGui : GuiManager {
        public DatabaseGui(Control.ControlCollection _control)
            : base(_control) { }
        public override void Initialize() {
            var testGrid = Access<DataGridView>("testGrid");
            bool test = testGrid != null;
            DataGrid = new DataGridView() {
                Location = test ? testGrid.Location : new Point(102, 10),                
                AllowUserToDeleteRows = true,
                AllowUserToAddRows = true,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
            };
            if (test) {
                DataGrid.Size = testGrid.Size;
            }           
            testGrid.Dispose();

            DataGrid.Columns.Add("Header", " ");
            Header.Width = 184;
            Controls.Add(DataGrid);

            SelectList = Access<ListBox>("AddDataSelect");
            SelectList.SelectedIndex = 0;

            Selected = new State<string>(() => SelectList.SelectedItem.ToString());

            LocalData = new ScheduleData();
            LoadFromStorage();
            LoadNewCategory();

            SelectList.SelectedIndexChanged += OnIndexChange;

            DataGrid.CellEndEdit += DataGrid_CellEndEdit;
        }

        private void DataGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e) {
            try {
                var cell = DataGrid[0, e.RowIndex];
                cell.Value = CensorField(cell.Value.ToString());
            } catch {
                MessageBox.Show($"Info: {e.RowIndex} {DataGrid[0, e.RowIndex]} {DataGrid[0, e.RowIndex].Value}");
            }
        }

        private void OnIndexChange(object sender, EventArgs e) {
            Selected.Update();                      
            UnloadCategory(Selected.Previous);
            LoadNewCategory();
        }

        public DataGridView DataGrid { get; private set; }
        public ListBox SelectList { get; private set; }
        private DataGridViewColumn Header => DataGrid.Columns[0];

        public ScheduleData LocalData { get; private set; }
        public State<string> Selected { get; private set; }

        #region Loading Data

        // Loads new 'schedule' data into the local db 
        public void LoadFromStorage() {
            //SQLSystem.TablesToStorage();

            for (int listIndex = 0; listIndex < 4; listIndex++) {
                LocalData[listIndex].Clear();
                foreach (var i in Storage.Data[listIndex]) {
                    LocalData[listIndex].Add(i);
                }
            }
        }
        // Passes / unloads data from local db into the main one
        public void UnloadToStorage() {
            for (int listIndex = 0; listIndex < 4; listIndex++) {
                Storage.Data[listIndex].Clear();

                foreach (var i in LocalData[listIndex]) {
                    Storage.Data[listIndex].Add(i);                    
                }
            }

            //SQLSystem.StorageToTables();
        }
        
        // Load data from storage into the DataGrid table
        public void LoadNewCategory() {
            // Update header when loading
            Header.HeaderText = Selected.Latest;
            
            var unloadFrom = LocalData[Selected.Latest];
            // Load to the datagrid view
            DataGrid.Rows.Clear();
            foreach (var i in unloadFrom) {
                DataGrid.Rows.Add(i);
            }
        }
        // Transfer (unload) data from DataGrid table to local db
        public void UnloadCategory(string category) {
            var loadTo = LocalData[category];
            // Load to local storage
            loadTo.Clear();
            for (int i = 0; i < DataGrid.RowCount - 1; i++) {
                var value = DataGrid[0, i].Value;
                if (value != null) loadTo.Add(value.ToString());
            }
        }

        #endregion
    }
}