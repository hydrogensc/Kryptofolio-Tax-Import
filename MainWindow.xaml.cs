using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kryptofo_Tax_Import
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int batchSize = 10;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void buttonElipsis_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".csv";
            dlg.Filter = "csv files (.csv)|*.csv";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                textBoxFileToImport.Text = filename;
            }
        }
        private IList<string> _batches = null;
        private int _currentBatch = 0;
        private static string _prefix;
        private void buttonGenerateQR_Click(object sender, RoutedEventArgs e)
        {
            generateBatches();
            if (_batches.Count > 0)
            {
                _currentBatch = 0;
                applyCurrentBatch();
            }
        }
        private void generateBatches()
        {
            try
            {
                var spacePos = coinType.Text.IndexOf(' ');
                _prefix = string.Format("kftx://kf?{0}", spacePos != -1 ? coinType.Text.Substring(0, spacePos) : coinType.Text);

                _batches = new List<string>();
                using (StreamReader myFile = new StreamReader(textBoxFileToImport.Text))
                {
                    var qrString = new StringBuilder();
                    var fileLine = myFile.ReadLine();
                    if (fileLine != null) fileLine = myFile.ReadLine();
                    var rowCount = 0;
                    while (fileLine != null)
                    {
                        var fields = fileLine.Split(',');
                        var dateTimeStr = fields[1];
                        dateTimeStr = dateTimeStr.Replace("\"", "");
                        dateTimeStr = dateTimeStr.Replace("-", "");
                        dateTimeStr = dateTimeStr.Replace("T", "");
                        dateTimeStr = dateTimeStr.Replace(":", "");
                        var amountStr = fields[5];
                        amountStr = amountStr.Replace("\"", "");
                        qrString.Append("&");
                        qrString.Append(dateTimeStr);
                        qrString.Append(",");
                        qrString.Append(amountStr);
                        fileLine = myFile.ReadLine();
                        rowCount++;
                        if (rowCount == batchSize)
                        {
                            _batches.Add(qrString.ToString());
                            qrString = new StringBuilder();
                            rowCount = 0;
                        }
                    }
                    string lastBatch = qrString.ToString();
                    if (lastBatch.Length > 0)
                        _batches.Add(lastBatch);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBatch > 0)
            {
                _currentBatch--;
                applyCurrentBatch();
            }
        }
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if(_currentBatch < _batches.Count - 1)
            {
                _currentBatch++;
                applyCurrentBatch();
            }
        }
        private void applyCurrentBatch()
        {
            labelBatch.Content = string.Format("{0} of {1}", _currentBatch + 1, _batches.Count);
            qrCodeImgControl1.Text = _prefix + _batches[_currentBatch];
            buttonPrev.IsEnabled = _currentBatch > 0;
            buttonNext.IsEnabled = _currentBatch < _batches.Count - 1;
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
