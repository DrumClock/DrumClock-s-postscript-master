using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;


namespace DrumClock_s_postscript
{
    public partial class MainWindow : Window
    {
        string FileText;

        Regex coor = new Regex(@"G\d X\d+.?\d* Y\d+.?\d* E-?\d+.?\d*");
        Regex coor2 = new Regex(@"G\d E-?\d+.?\d* F\d+.?\d*");
        Regex coor3 = new Regex(@"G\d E-?\d+.?\d*");

        // Unusable
        Regex t0 = new Regex(@"^T0");
        Regex t1 = new Regex(@"^T1");
        Regex t2 = new Regex(@"^T2");
        Regex t3 = new Regex(@"^T3");

        string[] OriginalRows;
        string NewFileText;

        public MainWindow()
        {
            InitializeComponent();

            //NegT0.IsChecked = false;
            //NegT1.IsChecked = false;
            //NegT2.IsChecked = false;
            //NegT3.IsChecked = false;

            //RepT0.IsChecked = false;
            //RepT1.IsChecked = false;
            //RepT2.IsChecked = false;
            //RepT3.IsChecked = false;
        }

        private void lblSelectFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openfiledialog = new OpenFileDialog();
            openfiledialog.Filter = "Gcode files (*.gcode)|*.gcode";

            try
            {
                if (openfiledialog.ShowDialog() == true)
                {
                    FileText = File.ReadAllText(openfiledialog.FileName);
                    txtFileSource.Text = openfiledialog.FileName;
                    OriginalRows = FileText.Split('\n');
                }

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lblSaveFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try 
            {
                StringBuilder NewRows = new StringBuilder();
                bool Negate = false;

                foreach (string row in OriginalRows)
                {
                    string act = row.Trim();

                    if (!act.StartsWith(";"))
                    {
                        // Set negate in Tx line or Negate? on other line
                        if ((act == "T0") || (act == "T1") || (act == "T2") || (act == "T3"))
                        {
                            // Set negate for other rows
                            if ((NegT0.IsEnabled && NegT0.IsChecked == true && (act == "T0")) ||
                                (NegT1.IsEnabled && NegT1.IsChecked == true && (act == "T1")) ||
                                (NegT2.IsEnabled && NegT2.IsChecked == true && (act == "T2")) ||
                                (NegT3.IsEnabled && NegT3.IsChecked == true && (act == "T3")))
                                Negate = true;
                            else
                                Negate = false;

                            if (RepT0.IsChecked == true && !string.IsNullOrEmpty(Rep0.Text) && (act == "T0"))
                                act = Rep0.Text;

                            if (RepT1.IsChecked == true && !string.IsNullOrEmpty(Rep1.Text) && (act == "T1"))
                                act = Rep1.Text;

                            if (RepT2.IsChecked == true && !string.IsNullOrEmpty(Rep2.Text) && (act == "T2"))
                                act = Rep2.Text;

                            if (RepT3.IsChecked == true && !string.IsNullOrEmpty(Rep3.Text) && (act == "T3"))
                                act = Rep3.Text;


                        }
                        else
                        {
                            //if (Negate && (coor.IsMatch(act) || coor2.IsMatch(act)) || coor3.IsMatch(act))
                              if (Negate && act.StartsWith("G1"))
                                act = NegateProc(act);
                        }

                    }
                    NewRows.AppendLine(act);
                }


                //if (RepT0.IsEnabled && RepT0.IsChecked == true && !string.IsNullOrEmpty(Rep0.Text))
                //{
                //    T0sub = T0sub.Remove(0, 2);
                //    T0sub = T0sub.Insert(0, Rep0.Text.Replace(@"\r", ""));
                //}

                //if (RepT1.IsEnabled && RepT1.IsChecked == true && !string.IsNullOrEmpty(Rep1.Text))
                //{
                //    T1sub = T1sub.Remove(0, 2);
                //    T1sub = T1sub.Insert(0, Rep1.Text.Replace(@"\r", ""));
                //}

                //if (RepT2.IsEnabled && RepT2.IsChecked == true && !string.IsNullOrEmpty(Rep2.Text))
                //{
                //    T2sub = T2sub.Remove(0, 2);
                //    T2sub = T2sub.Insert(0, Rep2.Text.Replace(@"\r", ""));
                //}

                //if (RepT3.IsEnabled && RepT3.IsChecked == true && !string.IsNullOrEmpty(Rep3.Text))
                //{
                //    T3sub = T3sub.Remove(0, 2);
                //    T3sub = T3sub.Insert(0, Rep3.Text.Replace(@"\r", ""));
                //}

                string filename = Path.GetFileName(txtFileSource.Text).Split('.')[0];

                NewFileText = NewRows.ToString();

                SaveFileDialog savefiledialog = new SaveFileDialog();
                savefiledialog.Filter = "Gcode files (*.gcode)|*.gcode";
                savefiledialog.FileName = filename + "_new.gcode";

                if (savefiledialog.ShowDialog() == true)
                {
                    File.WriteAllText(savefiledialog.FileName, NewFileText);
                    MessageBox.Show("Uloženo", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    //txtFileSource.Text = "";
                }

            } 
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private string NegateE(string sub)
        //{

        //    string[] lines = sub.Split('\n');
        //    StringBuilder builder = new StringBuilder();

        //    foreach (string line in lines)
        //    {
        //        string outline = line;

        //        if (coor.IsMatch(line) || coor2.IsMatch(line))
        //            outline = NegateProc(line);

        //        builder.AppendLine(outline);
        //    }

        //    return builder.ToString(); 
        //}

        private string NegateProc(string input)
        {
            string sout=input;

            if (input.Contains("E"))
            {
                int ei = input.IndexOf('E');

                if (input[ei + 1] =='-')
                    sout = input.Replace("-", "");
                else
                    sout = input.Insert(ei+1, "-");

            }
            return sout;
        }
    }
}
