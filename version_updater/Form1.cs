using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using version_updater.Properties;
using System.Configuration;
using System.Reflection;

namespace version_updater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region Обновлялка
            // проверяем обновилась ли версия
            try
            {
                // смотрим инфу о текущей версии
                long oldVersion = Settings.Default.Version;
                // обновляем .config взятый с сервера
                string fConfigName = Assembly.GetExecutingAssembly().ManifestModule.Name + ".config";
                FileInfo fConfigServer = new FileInfo(Settings.Default.FConfigServer + fConfigName);
                if (!fConfigServer.Exists) throw new FileNotFoundException(Resources.ErrFileConfigNotFoundServer, fConfigServer.FullName);
                fConfigServer.CopyTo(fConfigName, true);
                Settings.Default.Reload();
                long newVersion = Settings.Default.Version;
                if (newVersion == 0) throw new Exception(Resources.ErrFileEmptyServer);

                foreach (string fileName in Settings.Default.CopyFileList)
                {
                    FileInfo fInfoServer = new FileInfo(Settings.Default.FConfigServer + fileName);
                    FileInfo fInfoLocal = new FileInfo(fileName);
                    // Если на серваке нет такого файла - ошибка
                    if (!fInfoServer.Exists) throw new System.IO.FileNotFoundException(Resources.ErrFileNotFoundServer, fInfoServer.FullName);
                    // Если локально такого файла нет - копируем
                    if (!fInfoLocal.Exists)
                    {
                        string path = Path.GetDirectoryName(fileName);
                        if (path != "")
                        {
                            DirectoryInfo dir = new DirectoryInfo(path);
                            if (!dir.Exists) dir.Create();
                        }
                        fInfoServer.CopyTo(fileName, true);
                        continue;
                    }
                    // Если дата изменения локального файла меньше - копируем
                    if (fInfoLocal.LastWriteTime < fInfoServer.LastWriteTime) fInfoServer.CopyTo(fileName, true);
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.FileName, Resources.ErrText, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message, Resources.ErrText); }

            // запускаем приложение
            try
            { System.Diagnostics.Process.Start(Settings.Default.AfterStartApplication); }
            catch (Exception ex)
            { MessageBox.Show(ex.Message + "\r\napp:" + Settings.Default.AfterStartApplication, Resources.ErrStartApplication); }

            #endregion
            this.Close();
        }
    }
}