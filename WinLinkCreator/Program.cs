using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace WinLinkCreator
{
    internal class Program
    {
        /// <param name="base">Base path, which is full should be relative to</param>
        /// <param name="target">Full path</param>
        public static string MakeRelativePath(string @base, string target)
        {
            var fromUri = new Uri("file:///" + @base);
            var toUri = new Uri("file:///" + target);
		
            return Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());
        }

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length <= 0)
                return;
            
            var p = args[0];
            if (p.EndsWith("\\") || p.EndsWith("/"))
                p = p.Substring(0, p.Length - 1);
            
            var isDirectory = Directory.Exists(p);
            var isFile = File.Exists(p);

            if (!isDirectory && !isFile)
            {
                MessageBox.Show($"{p} is not a file and not a directory", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            var fn = Path.GetFileName(p);
            var dir = Path.GetDirectoryName(p);
            var basePath = p;
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save symlink to";
            saveFileDialog1.FileName = fn;
            saveFileDialog1.InitialDirectory = dir;
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.AutoUpgradeEnabled = true;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.AddExtension = false;

            var result = saveFileDialog1.ShowDialog();
            if (result != DialogResult.OK)
                return;

            var newFn = saveFileDialog1.FileName;
            var relativePath = MakeRelativePath(newFn, basePath);

            var relative = MessageBox.Show($"Should create relative?\nYes = {relativePath}\nNo = {p}", "Relative",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

            string linkArgs = "";
            if (isDirectory)
                linkArgs += "/D ";

            var targetPath = relative ? relativePath : p;
            targetPath = targetPath.Replace("/", "\\");
            
            linkArgs += $"\"{newFn}\" ";
            linkArgs += $"\"{targetPath}\"";

            var si = new ProcessStartInfo("cmd");
            si.Arguments = $"/c mklink {linkArgs}";
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            si.RedirectStandardOutput = true;
            si.RedirectStandardError = true;
            
            var process = Process.Start(si);
            process.WaitForExit();

            var trimChars = new char[] { '\r', '\n' };
            var outputInfo = process.StandardOutput.ReadToEnd()?.Trim(trimChars).Trim()?.Trim(trimChars);
            var outputError = process.StandardError.ReadToEnd()?.Trim(trimChars).Trim()?.Trim(trimChars);

            if (!string.IsNullOrWhiteSpace(outputError))
            {
                MessageBox.Show(outputError, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

            if (!string.IsNullOrWhiteSpace(outputInfo))
            {
                MessageBox.Show(outputInfo, "Execution Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}