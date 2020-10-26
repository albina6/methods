﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace _1labMethod
{
    public partial class Form1 : Form
    {
        private double [,] matrix;
        private string[] columnName;
        private int rowCount, columnCount;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    StreamWriter myWriter = new StreamWriter(myStream);
                    try
                    {
                        for ( int i = 0; i < columnCount; i++)
                        {
                            myWriter.Write(columnName[i] + ';');
                        }
                        myWriter.WriteLine();
                        for (int row = 0; row < rowCount; row++)
                        {
                            //str = str1[row + 1].Split(';');
                            for (int column = 0; column < columnCount; column++)
                            {
                                myWriter.Write(matrix[row, column].ToString() + ';');
                            }
                            myWriter.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        myWriter.Close();
                    }
                }
            }
        }

        private void correl_Click(object sender, EventArgs e)
        {
            //поиск вектора средних
            double[] xVect =new double [columnCount - 1];
            double auxiliary;
            for (int column = 1; column < columnCount; column++)
            {
                auxiliary = 0;
                for (int row = 0; row < rowCount; row++)
                {
                    auxiliary += matrix[row, column];
                }
                xVect[column - 1] = auxiliary / rowCount;//нужно ли -1 вроде да 
            }

            //среднеквадратичные отклонения 
            double[] sVect = new double[columnCount - 1];
            for (int column=1;column < columnCount; column++)
            {
                auxiliary = 0;
                for (int row = 0; row < rowCount; row++)
                {
                    auxiliary += Math.Pow((matrix[row, column] - xVect[column-1]), 2);
                }
                sVect[column - 1] = Math.Sqrt(auxiliary / rowCount);
            }

            //матрица парных коэффициентов корреляции
            double[,] rMatrix = new double[columnCount - 1,columnCount - 1];
            for (int colomn1 = 1; colomn1 < columnCount; colomn1++)
            {
                rMatrix[colomn1-1, colomn1-1] = 1;
                for (int colomn2 = colomn1 + 1; colomn2 < columnCount; colomn2++)
                {
                    auxiliary = 0;
                    for (int row = 0; row < rowCount; row++)
                    {
                        auxiliary += (matrix[row, colomn1] - xVect[colomn1 - 1]) * (matrix[row, colomn2] - xVect[colomn2 - 1]);
                    }
                    rMatrix[colomn1 - 1, colomn2 - 1] = auxiliary / (rowCount * sVect[colomn1 - 1] * sVect[colomn2 - 1]);
                    rMatrix[colomn2 - 1, colomn1 - 1] = rMatrix[colomn1 - 1, colomn2 - 1];
                }
            }

            ////проверка что алгоритм работает правильно 
            //for (int colomn1 = 1; colomn1 < columnCount; colomn1++)
            //{

            //    for (int colomn2 = 1; colomn2 < columnCount; colomn2++)
            //    {
            //        auxiliary = 0;
            //        for (int row = 0; row < rowCount; row++)
            //        {
            //            auxiliary += (matrix[row, colomn1] - xVect[colomn1-1])* (matrix[row, colomn2] - xVect[colomn2-1]);
            //        }
            //        rMatrix[colomn1-1, colomn2-1] = auxiliary / (rowCount * sVect[colomn1-1] * sVect[colomn2-1]);   
            //    }
            //}

            

        }

        // матрица частных коэффициентов корреляции
        private double [,] partiaCorrMatrix(double [,] matrix)
        {
            double[,] partialcorrMatrix = new double[columnCount, columnCount];
            for (int x = 0; x < rowCount; x++)
            {
                for (int y = 0; y < columnCount; y++)
                {

                }
            }
        }
        


    private double minorXY(double [,] matrix, int x, int y)
        {
            int size = matrix.GetLength(1);
            double[,] minor = new double[size - 1, size - 1];
            for (int row = 0; row < size; row++)
            {
                if (row == x)
                {
                    continue;
                }
                else
                {
                    for (int column = 0; column < size; column++)
                    {
                        if (column == y)
                        {
                            continue;
                        }
                        else
                        {
                            minor[row, column] = matrix[row, column];
                        }
                    }
                }
            }
            return determinant(minor);
        }

        //алгебраическое дополнение
        private double algCompl(double[,] matrix, int x, int y)
        {
            return Math.Pow((-1), x + y) * minorXY(matrix, x, y);
        }


        private double determinant(double [,] matrix)
        {
            int countCnt = matrix.GetLength(1);
            double determ = 0;
            if (matrix.GetLength(0) !=countCnt)
            {
                MessageBox.Show("Матрица не является квадратной");
                return -1;
            }
            else if (countCnt > 2)
            {
                for(int colomn = 0; colomn < countCnt; colomn++)
                {
                    determ += matrix[0, colomn] * Math.Pow((-1), colomn) * minorXY(matrix, 0, colomn); 
                }
                return determ;
            }
            else
            {
                determ = (matrix[0, 0] * matrix[1, 1]) - (matrix[0, 1] * matrix[1, 0]);
                return determ;
            }
        }
        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    StreamReader myReader = new StreamReader(myStream);
                    string[] str;

                    try
                    {
                        string[] str1 = myReader.ReadToEnd().Split('\n');
                        str = str1[0].Split(';');
                        columnCount = str.Length;
                        rowCount = str1.Length-2;
                        //названия столбцов нужно ли ?
                        columnName=new string [columnCount];
                        Array.Copy(str, columnName, columnCount);
                        matrix = new double[rowCount, columnCount];

                        for( int row = 0; row < rowCount; row++)
                        {
                            str = str1[row + 1].Split(';');
                            for (int column=0; column < columnCount; column++)
                            {
                                matrix[row, column] = double.Parse(str[column]);                                
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        myReader.Close();
                    }
                }
            }                      
        }
    }
}
