﻿/*
    Reflexil .NET assembly editor.
    Copyright (C) 2007 Sebastien LEBRETON

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#region " Imports "
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Mono.Cecil;
using Reflexil.Utils;
#endregion

namespace Reflexil.Forms
{
	public partial class StrongNameRemoverForm: Form
    {

        #region " Fields "
        private AssemblyDefinition m_snassembly = null;
        #endregion

        #region " Properties "
        public AssemblyDefinition AssemblyDefinition
        {
            get
            {
                return m_snassembly;
            }
            set
            {
                m_snassembly = value;
                Add.Enabled = AutoScan.Enabled = Process.Enabled = m_snassembly != null;
                if (m_snassembly != null)
                {
                    SNAssembly.Text = m_snassembly.ToString();
                    Tooltip.SetToolTip(SNAssembly, value.MainModule.Image.FileInformation.FullName);
                }
                else
                {
                    SNAssembly.Text = string.Empty;
                    Tooltip.SetToolTip(SNAssembly, null);
                }
            }
        }
        #endregion

        #region " Methods "
        public StrongNameRemoverForm()
        {
            InitializeComponent();
        }

        private AssemblyDefinition LoadAssembly(string filename)
        {
            try
            {
                return AssemblyFactory.GetAssembly(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Reflexil is unable to load this assembly: {0}", ex.Message));
            }
            return null;
        }
        #endregion

        #region " Events "
        private void Add_Click(object sender, EventArgs e)
        {
            OpenFileDialog.Multiselect = true;
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string filename in OpenFileDialog.FileNames)
                {
                    AssemblyDefinition asmdef = LoadAssembly(filename);
                    if (asmdef != null)
                    {
                        ReferencingAssemblies.Items.Add(asmdef);
                    }
                }
            }
        }

        private void ReferencingAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            Remove.Enabled = ReferencingAssemblies.SelectedItems.Count > 0;
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            IEnumerable assemblies = new ArrayList(ReferencingAssemblies.SelectedItems);
            foreach (AssemblyDefinition asmdef in assemblies)
            {
                ReferencingAssemblies.Items.Remove(asmdef);
            }
        }

        private void SelectSNAssembly_Click(object sender, EventArgs e)
        {
            OpenFileDialog.Multiselect = false;
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                AssemblyDefinition loader = LoadAssembly(OpenFileDialog.FileName);
                if (loader != null)
                {
                    AssemblyDefinition = loader;
                }
            }
        }

        private void AutoScan_Click(object sender, EventArgs e)
        {
            if (AssemblyDefinition != null)
            {
                ReferencingAssemblies.Items.Clear();
                using (DirectoryScanForm frm = new DirectoryScanForm())
                {
                    if (frm.ShowDialog(AssemblyDefinition) == DialogResult.OK)
                    {
                        ReferencingAssemblies.Items.AddRange(frm.ReferencingAssemblies);
                    }
                }
            }
        }

        private void Process_Click(object sender, EventArgs e)
        {
            try
            {
                using (ReferenceUpdaterForm frm = new ReferenceUpdaterForm())
                {
                    ArrayList assemblies = new ArrayList(ReferencingAssemblies.Items);
                    assemblies.Add(AssemblyDefinition);

                    frm.ShowDialog(assemblies.ToArray(typeof(AssemblyDefinition)) as AssemblyDefinition[]);
                }

                AssemblyDefinition = AssemblyDefinition;
                // Refresh hack
                ReferencingAssemblies.DisplayMember = this.GetType().Name;
                ReferencingAssemblies.DisplayMember = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Reflexil is unable to save this assembly: {0}", ex.Message));
            }
        }

        private void ReferencingAssemblies_MouseMove(object sender, MouseEventArgs e)
        {
            Point coords = new Point(e.X, e.Y);
            int index = ReferencingAssemblies.IndexFromPoint(coords);
            if (index > -1)
            {
                Tooltip.SetToolTip(ReferencingAssemblies, (ReferencingAssemblies.Items[index] as AssemblyDefinition).MainModule.Image.FileInformation.FullName);
            }
            else
            {
                Tooltip.SetToolTip(ReferencingAssemblies, string.Empty);
            }
        }
        #endregion

	}
}
