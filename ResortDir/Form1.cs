using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResortDir
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Рабочая папка пролграммы
        /// </summary>
        private string appDir = Directory.GetCurrentDirectory();

        /// <summary>
        /// Запуск перемещения 
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            var processedDirCount = 0;                                                  /// Количество обработанных папок
            var listRootDir = Directory.GetDirectories(appDir);                         /// Список папок в рабочей папке

            RefreshInfo();
            var totalDir = Directory.GetDirectories(appDir).Length;                     /// Общее количество папок в рабочей папке

            Task th = new Task(() =>
            {
                progressBar1.Invoke(new Action(() =>
                {
                    progressBar1.Value = 0;
                    progressBar1.Visible = panel1.Visible = true;
                }));
                
                var totalProcessedDirCount = 0;

                foreach (string childDir in listRootDir)
                {
                    totalProcessedDirCount++;
                    progressBar1.Invoke(new Action(() => progressBar1.Value = (int)((float)totalProcessedDirCount/totalDir*100)));
                    listBox1.Invoke(new Action(() => listBox1.Items.Add(childDir)));
                    var listChildDir = Directory.GetDirectories(childDir);              /// Получение подпапок для папки из корневой директории 
                    if (listChildDir.Length >= 1)
                    {
                        /// В папке из корневой директории есть подпапки. Обрабатываем
                        processedDirCount++;
                        foreach (string childSubDir in listChildDir)
                        {
                            var listFile = Directory.GetFiles(childSubDir);             /// Получение списка файлов в папке
                            foreach (string sourceFile in listFile)
                            {
                                try
                                {
                                    if (File.Exists(childDir + "\\" + Path.GetFileName(sourceFile)))                            /// Проверка на существование такого же файла в папке назначения
                                    {
                                        var i = 1;

                                        while (i != -1)
                                        {
                                            if (!File.Exists(childDir + "\\" + Path.GetFileNameWithoutExtension(sourceFile) + "_" + i.ToString() + Path.GetExtension(sourceFile)))
                                            {
                                                Directory.Move(sourceFile, childDir + "\\" + Path.GetFileNameWithoutExtension(sourceFile) + "_" + i.ToString() + Path.GetExtension(sourceFile));      /// Копируем с изменением имени при наличии такого же файла
                                                i = -1;
                                            }
                                            else i++;
                                        }
                                    }
                                    else Directory.Move(sourceFile, childDir + "\\" + Path.GetFileName(sourceFile));            /// Перемещение файла
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                            if (Directory.GetFiles(childSubDir).Length == 0)
                                Directory.Delete(childSubDir);                          /// Удаление пустых после перемещения папок
                        }
                        toolStripStatusLabel6.Text = processedDirCount.ToString();      /// Вывод количества обработанных папок
                    }
                    Thread.Sleep(100);
                }
                progressBar1.Invoke(new Action(() => progressBar1.Visible = panel1.Visible = false)); 
            });
            th.Start();             /// Запуск потока обработки папок 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshInfo();
        }

        /// <summary>
        /// Обновление отображаемых данных в окне
        /// </summary>
        private void RefreshInfo()
        {
            #region Вывод информации в строку статуса
            listBox1.Items.Clear();
            toolStripStatusLabel4.Text = appDir;                                                /// Вывод рабочей папки                                              
            toolStripStatusLabel2.Text = Directory.GetDirectories(appDir).Length.ToString();    /// Вывод общего количества папок в рабочей директории
            toolStripStatusLabel6.Text = "0";
            #endregion
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshInfo();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    btnRefresh.PerformClick();
                    break;
                default:
                    break;
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Draw the current item text based on the current Font 
            // and the custom brush settings.
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }
    }
}
