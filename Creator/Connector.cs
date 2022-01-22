using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Creator
{
    static class Connector
    {
        private static string[] ListOrder()
        {
            string readFile = File.ReadAllText(Creator.FilePath);
            string[] delimiter = new string[] { "###StartsHere###" };
            string[] cols = readFile.Split(delimiter, StringSplitOptions.None);

            if(cols.Length < 2)
            {
                throw new Exception();
            }

            return cols;
        }

        public static string[] MakeCommand()
        {
            string databaseLine = null;

            try
            {
                databaseLine = ListOrder()[1];
            }
            catch (Exception)
            {                
                return null;
            }

            string[] cols = databaseLine.Split(' ');

            bool equals = cols[1].Equals("DATABASE", StringComparison.CurrentCultureIgnoreCase);

            if (!equals)
            {
                string[] com = { "- - - Aborted - - -" };
                return com;
            }

            string[] commands = ListOrder();
            int numberOfCommands = commands.Length;
            string[] commandNames = new string[numberOfCommands];

            for (int i = 0; i < numberOfCommands; i++)
            {
                commandNames[i] = commands[i].CommandType();
            }

            return commandNames;
        }

        private static string CommandType(this string list)
        {
            string[] cols = list.Split(' ');

            if (cols[0] == "\r\nCREATE" || cols[0] == "CREATE")
            {
                if (cols[1] == "DATABASE")
                {
                    return $"- - - creating - database - {cols[2]} - - -";
                }
                else if (cols[1] == "TABLE")
                {
                    return $"- - - creating - table - {cols[2]} - - -";
                }
            }
            else if (cols[0] == "DELIMITER" || cols[0] == "\r\nDELIMITER")
            {
                if (cols[1] == "CREATE" || cols[1] == "$$CREATE" || cols[1] == "$$\r\nCREATE")
                {
                    if (cols[3] == "PROCEDURE")
                    {
                        return $"- - - creating - procedure - {cols[4]} - - -";
                    }
                }
            }

            return "Executing Instraction - - -";
        }

        private static string[] GetDataBaseInfo()
        {
            string databaseLine = ListOrder()[1];

            string[] cols = databaseLine.Split(' ');

            bool equals = cols[1].Equals("DATABASE", StringComparison.CurrentCultureIgnoreCase);

            if (!equals)
            {
                throw new Exception();
            }

            string untrimmedDatabaseName = cols[2];
            string databaseName = null;
            for (int i = 0; i < untrimmedDatabaseName.Length; i++)
            {
                if (i != 0 && i != untrimmedDatabaseName.Length - 1)
                {
                    databaseName += untrimmedDatabaseName[i];
                }
            }

            string databaseInfoLine = ListOrder()[0];
            string[] info = databaseInfoLine.Split(' ');
            string[] dbInfo = { info[0], info[1], info[2], databaseName };

            return dbInfo;
        }

        public static void ExecuteCommand()
        {
            string[] databaseInfo = new string[4];

            try
            {
                databaseInfo = GetDataBaseInfo();
            }
            catch (Exception)
            {
                MessageBox.Show("The selected file is not formatted accordingly, please refer to FileFormatGuide.txt.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string directory = $@"/k cd {Creator.FolderPath}";
            string serverName = databaseInfo[0];
            string userName = databaseInfo[1];
            string password = databaseInfo[2];
            string databaseName = databaseInfo[3];

            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = directory;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.CreateNoWindow = true;

                cmd.Start();

                string databaseCred = $"mysql -h {serverName} -u {userName} -p{password}";
                string createDatabase = ListOrder()[1];
                string changeDb = $@"\u {databaseName}";

                StreamWriter writer = cmd.StandardInput;

                writer.WriteLine(databaseCred);
                writer.WriteLine(createDatabase);
                writer.WriteLine(changeDb);

                for (int i = 2; i < ListOrder().Length; i++)
                {
                    writer.WriteLine(ListOrder()[i]);
                }

                writer.WriteLine(@"\q");
                writer.Close();
                cmd.WaitForExit();
            }
        }
    }
}
