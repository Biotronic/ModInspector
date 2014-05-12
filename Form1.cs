using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModInspector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DirectoryInfo tmpPath = null;

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (tmpPath == null)
                {
                    tmpPath = new DirectoryInfo(Tools.GetRandomTempFolder());
                }
                if (tmpPath.Exists)
                {
                    tmpPath.Delete(true);
                }
                Tools.ExtractArchive(new FileInfo(openFileDialog1.FileName), tmpPath);
                var node = CreateTree(tmpPath);
                node.Text = Path.GetFileName(openFileDialog1.FileName);
                treeView1.Nodes.Add(node);
            }
        }

        TreeNode CreateTree(DirectoryInfo dir)
        {
            dir.Refresh();
            var result = new TreeNode(dir.Name);
            result.Checked = true;
            if (dir.Exists)
            {
                foreach (var subDir in dir.GetDirectories())
                {
                    result.Nodes.Add(CreateTree(subDir));
                }
                foreach (var file in dir.GetFiles())
                {
                    result.Nodes.Add(CreateTree(file));
                }
            }
            return result;
        }

        TreeNode CreateTree(FileInfo file)
        {
            var result = new TreeNode(file.Name);
            result.Checked = true;
            var node = ConfigNode.LoadFromFile(file);
            if (file.Extension == ".cfg")
            {
                var nodeTree = CreateTree(node);
                result.Nodes.AddRange(nodeTree.Nodes.OfType<TreeNode>().ToArray());
            }
            return result;
        }

        TreeNode CreateTree(ConfigNode node)
        {
            var result = new TreeNode(node.Type + "(" + node["name"] + ")");
            
            foreach (var child in node.Children)
            {
                result.Nodes.Add(CreateTree(child));
            }
            foreach (var value in node.Values)
            {
                result.Nodes.Add(new TreeNode(value.Key + " = " + value.Value));
            }
            return result;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (tmpPath.Exists)
            {
                tmpPath.Delete(true);
            }
        }
    }
}
