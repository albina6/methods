using System;
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
            double[] xVect =new double [columnCount];
            double auxiliary;
            for (int column = 0; column < columnCount; column++)
            {
                auxiliary = 0;
                for (int row = 0; row < rowCount; row++)
                {
                    auxiliary += matrix[row, column];
                }
                xVect[column] = auxiliary / rowCount;
            }

            //среднеквадратичные отклонения 
            double[] sVect = new double[columnCount];
            for (int column=0;column < columnCount; column++)
            {
                auxiliary = 0;
                for (int row = 0; row < rowCount; row++)
                {
                    auxiliary += Math.Pow((matrix[row, column] - xVect[column]), 2);
                }
                sVect[column ] = Math.Sqrt(auxiliary / rowCount);
            }

            //матрица парных коэффициентов корреляции
            double[,] rMatrix = new double[columnCount,columnCount];
            for (int colomn1 = 0; colomn1 < columnCount; colomn1++)
            {
                rMatrix[colomn1, colomn1] = 1;
                for (int colomn2 = colomn1 + 1; colomn2 < columnCount; colomn2++)
                {
                    auxiliary = 0;
                    for (int row = 0; row < rowCount; row++)
                    {
                        auxiliary += (matrix[row, colomn1] - xVect[colomn1 ])
                            * (matrix[row, colomn2] - xVect[colomn2 ]);
                    }
                    rMatrix[colomn1, colomn2 ] = auxiliary / (rowCount * sVect[colomn1] * sVect[colomn2]);
                    rMatrix[colomn2, colomn1 ] = rMatrix[colomn1 , colomn2 ];
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

            //расчет матрицы алгебраических дополнений по матрице R
            double[,] alfComplementMatrix =  algComplementMatrix(rMatrix);

            //частные коэффициенты корреляции
            int sizeM = alfComplementMatrix.GetLength(0);
            double[,] partialCorrMatrix = new double[sizeM, sizeM];
            for (int x=0 ; x < sizeM; x++)
            {
                for( int y = x; y < sizeM; y++)
                {
                    partialCorrMatrix[x, y] = (-1) * alfComplementMatrix[x, y] 
                        / Math.Sqrt(alfComplementMatrix[x, x] * alfComplementMatrix[y, y]);
                    if (y != x)
                    {
                        partialCorrMatrix[y, x] = partialCorrMatrix[x, y];
                    }
                }
            }

            //проверка значимости частных коэффициентов корреляции
            //a=0,05; v=n-l-2; l=1
            // for n=8; 2.571
            //for n=53; 2.0085591
            //поверка значимости 
            //оценки 
            int sizeT = (sizeM * (sizeM - 1) / 2);
            double[] tObservedValue = new double[sizeT];
            for(int x=0,k=0; x < sizeM; x++)
            {
                for(int y = x + 1; y < sizeM; k++,y++)
                {
                    tObservedValue[k] = (partialCorrMatrix[x, y] * Math.Sqrt(rowCount - 3) )
                        / Math.Sqrt(1 - Math.Pow(partialCorrMatrix[x, y], 2));
                }
            }

            //поверка значимости , Фишера -Иейтса
            //for n=8: 0.754
            //for n=53; 0.270 

            //  интервальная оценка y=0.95
            //int sizeT = (sizeM * (sizeM - 1) / 2);
            double[,] rInterval = new double[sizeT,2];
            double sigma;
            double tZ;
            for (int x = 0, k = 0; x < sizeM; x++)
            {
                for (int y = x + 1; y < sizeM; k++, y++)
                {
                    sigma = 0.5 * (Math.Log(1 + partialCorrMatrix[x, y])
                        - Math.Log(1 - partialCorrMatrix[x, y]));
                    tZ = 1.96 * (Math.Sqrt(1.0 / (rowCount - 4)));
                    rInterval[k, 0] = (Math.Exp(2.0 * (sigma - tZ)) - 1)
                        / (Math.Exp(2.0 * (sigma - tZ)) + 1);
                    rInterval[k, 1] = (Math.Exp(2.0 * (sigma + tZ)) - 1)
                        / (Math.Exp(2.0 * (sigma + tZ)) + 1);
                }
            }

            //множественный коэффициент корреляции
            double[] rMulti = new double[columnCount];
            double determin=0;
            for (int x = 0; x < columnCount; x++)
            {
                determin += rMatrix[0, x] * alfComplementMatrix[0, x];
            }
            
            for (int x = 0; x < columnCount; x++)
            {
                rMulti[x] = Math.Sqrt(1.0 - determin / alfComplementMatrix[x, x]);
            }

            //Fкрит(0.05;2;5)=5.79
            //Fкрит(0.05;5;47)=3.20
            double[] fRMulti = new double[columnCount];
            double f =( double)(rowCount - columnCount) / (columnCount - 1);
            for (int x = 0; x < columnCount; x++)
            {
                fRMulti[x] = f * Math.Pow(rMulti[x], 2) / (1.0 - Math.Pow(rMulti[x], 2));
            }
        }

        //матрица алгебраических дополнений  
        // матрица частных коэффициентов корреляции
        private double [,] algComplementMatrix(double [,] matrix)
        {
            int size = matrix.GetLength(1);
            double[,] algComplMatrix = new double[size, size];
            for (int x = 0; x < size; x++)
            {
                for (int y = x; y < size; y++)
                {
                    algComplMatrix[x, y] = Math.Pow((-1), x + y) * determinant(minorXYmatrix(matrix, x, y));
                    if (y != x)
                    {
                        algComplMatrix[y, x] = algComplMatrix[x, y];
                    }
                }
            }
            return algComplMatrix;
        }
        
    private double [,] minorXYmatrix(double [,] matrix, int x, int y)
        {
            int size = matrix.GetLength(1) - 1;
            double[,] minor = new double[size, size];
            for (int row = 0, rowMatrix = 0; row < size; rowMatrix++, row++)
            {
                if (row == x)
                {
                    rowMatrix++;
                }

                for (int column = 0, columnMatrix = 0; column < size; columnMatrix++, column++)
                { 
                    if (column == y)
                    {
                        columnMatrix++;
                    }
                minor[row, column] = matrix[rowMatrix, columnMatrix];
                }
            }
            return minor;
        }

        ////алгебраическое дополнение
        //private double algCompl(double[,] matrix, int x, int y)
        //{
        //    return Math.Pow((-1), x + y) * minorXYmatrix(matrix, x, y);
        //}
        private double determinant(double[,] matrixForDet, double [,] algebrComplMatrix)
        {
            double det = 0;
            if (matrixForDet.GetLength(0) == matrixForDet.GetLength(1))
            {
                if (algebrComplMatrix.GetLength(0) == algebrComplMatrix.GetLength(1))
                {
                    if (algebrComplMatrix.GetLength(0) == matrixForDet.GetLength(0))
                    {
                        int size = algebrComplMatrix.GetLength(0);
                        for (int y = 0; y < size; y++)
                        {
                            det += matrixForDet[0, y] * algebrComplMatrix[0, y];
                        }
                        return det;
                    }
                    else
                    {
                        MessageBox.Show("Матрицы не одинаковой размерности {determinant} ");
                        return -1;
                    }
                }
                else
                {
                    MessageBox.Show("Матрица algebrComplMatrix не является квадратной {determinant}");
                    return -1;
                }
            }
            else
            {
                MessageBox.Show("Матрица X не является квадратной {determinant}");
                return -1;
            }
            
        }

        private double determinant(double [,] matrix)
        {
            int countCnt = matrix.GetLength(1);
            double determ = 0;
            if (matrix.GetLength(0) !=countCnt)
            {
                MessageBox.Show("Матрица не является квадратной {determinant}");
                return -1;
            }
            else if (countCnt > 2)
            {
                for(int colomn = 0; colomn < countCnt; colomn++)
                {
                    determ += matrix[0, colomn] * Math.Pow((-1), colomn) *determinant( minorXYmatrix(matrix, 0, colomn)); 
                }
                return determ;
            }
            else
            {
                determ = (matrix[0, 0] * matrix[1, 1]) - (matrix[0, 1] * matrix[1, 0]);
                return determ;
            }
        }

        private void regression_Click(object sender, EventArgs e)
        {
            double[,] yMatrix, xMatrix;
            yMatrix = new double[rowCount, 1];
            //xMatrix = new double[rowCount,columnCount];
            xMatrix=(double[,]) matrix.Clone();
            for (int i = 0; i < rowCount; i++)
            {
                yMatrix[i, 0] = matrix[i, 0];
                xMatrix[i, 0] = 1;
            }

            double[,] xTransposed = transposedMatrix(xMatrix);
            double[,] xTx = multiplicationMatrix(xTransposed, xMatrix);
            double[,] algCompl = algComplementMatrix(xTx);
            double[,] inverseMatrix_xTx = multiplicationMatrix
                            ((1.0 / determinant(xTx, algCompl)), algCompl);
            double[,] bMatrix = multiplicationMatrix(inverseMatrix_xTx,
                            multiplicationMatrix(xTransposed, yMatrix));

            //линейное уравнение регрессии
            double [,] yCalculatedLin = calculateY (bMatrix, xMatrix);
            check(yMatrix, yCalculatedLin, xMatrix, bMatrix, inverseMatrix_xTx);
            //про остаток 
            //double[,] qError = sumMatrix(yMatrix, (multiplicationMatrix(-1, yCalculatedLin)));
            


            //double[,] xTransposed = transposedMatrix(xMatrix);

            //double[,] xTransposed = transposedMatrix(xMatrix);
            //double[,] xTx = multiplicationMatrix(xTransposed, xMatrix);

            // double [,]a  = new double[,] { { 3}, { 2},{ 0},{ -1} };
            // //b = new double[,] { { -1, 1, 0, 2 } };
            // //a = new double[,] { { 2, 4, 0 }, { -2, 1, 3 }, { -1, 0, 1 } };
            // //b = new double[,] { { 1 }, { 2 }, { -1 } };

            //double [,] b = transposedMatrix(a);
        }

        private double[,] calculateY(double [,] bMatrix,double [,] xMatrix)
        {
            if ((bMatrix.GetLength(0) == xMatrix.GetLength(1))&&(bMatrix.GetLength(1))==1)
            {
                int xSize = xMatrix.GetLength(0);
                int ySize = xMatrix.GetLength(1);
                double[,] yCalculated = new double[xSize, 1];

                for (int x = 0; x < xSize; x++)
                {
                    double auxiliaryV = 0.0;
                    for(int y = 0; y < ySize; y++)
                    {
                        auxiliaryV += bMatrix[y, 0] * xMatrix[x, y];
                    }
                    yCalculated[x, 0] = auxiliaryV;
                }
                return yCalculated;
            }
            else
            {
                MessageBox.Show("Матрицы не удовлетворяют условиям  [x,1]");
                double[,] errorMatrix = new double[,] { { -1 } };
                return errorMatrix;
            }
        }
        private double [,] transposedMatrix(double [,] aMatrix)
        {
            int aX, aY;
            aX = aMatrix.GetLength(0);
            aY = aMatrix.GetLength(1);
            double[,] transposedMatrix = new double[aY, aX];
            for (int tX = 0; tX < aY; tX++)
            {
                for ( int tY = 0; tY < aX; tY++)
                {
                    transposedMatrix[tX, tY] = aMatrix[tY, tX];
                }
            }
            return transposedMatrix;
        }
        private void check(double [,] yMatrix, double [,] yCalculated ,
                        double [,] xMatrix, double[,] bMatrix, double[,]  inverseMatrix)
        {
            double qDiff =qDifference(yMatrix, yCalculated);
            double s2 = ((1.0 / (rowCount - columnCount - 1)) * qDiff);
            double qR = sumSquar(yCalculated);
            //F-критерий
            double fObserved = ((rowCount - columnCount - 1) * qR) 
                                    / ((columnCount + 1) * qDiff);
            double[,] vB = multiplicationMatrix(s2, inverseMatrix);
        }

        private double sumSquar(double [,] matrix)
        {
            if (matrix.GetLength(1) == 1)
            {
                int size = matrix.GetLength(0);
                double sum = 0;
                for (int i=0;i<size; i++)
                {
                    sum += Math.Pow(matrix[i, 0], 2);
                }
                return sum;
            }
            else
            {
                MessageBox.Show("Матрица не явдяется вектором [x,1]");
                Console.ReadKey(true);
                return -1;
            }
        }
        private double qDifference(double [,] y, double[,] yCalculated)
        {
            if ((y.GetLength(0)== yCalculated.GetLength(0))
                    &&( y.GetLength(1)== yCalculated.GetLength(1)) && (y.GetLength(1) == 1))
            {
                double q = 0;
                int size = y.GetLength(0);
                for (int i = 0; i < size; i++)
                {
                    q += Math.Pow((y[i, 0] - yCalculated[i, 0]), 2);
                }
                return q;
            }
            else
            {
                MessageBox.Show("Матрицы не равных размеров");
                return -1;
            }
            
        }

        //если второй параметр сделать отрицательным получим вычитание
        private double[,] sumMatrix(double[,]firstMatrix, double[,] secondMatrix)
        {
            if ((firstMatrix.GetLength(0)==secondMatrix.GetLength(0))
                    && (firstMatrix.GetLength(1) == secondMatrix.GetLength(1)))
            {
                int xSize = firstMatrix.GetLength(0);
                int ySize = firstMatrix.GetLength(1);
                double[,] sumMatrix = new double[xSize, ySize];
                for(int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        sumMatrix[x, y] = firstMatrix[x, y] + secondMatrix[x, y];
                    }
                }
                return sumMatrix;
            }
            else
            {
                MessageBox.Show("Матрицы не равных размеров");
                double[,] errorMatrix = new double[,] { { -1 } };
                return errorMatrix;
            }
        }
        private double[,] multiplicationMatrix(double factor, double[,] aMatrix)
        {
            int xSize = aMatrix.GetLength(0);
            int ySize = aMatrix.GetLength(1);
            double [,] bMatrix = new double [xSize, ySize] ;
            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    bMatrix[x, y] = aMatrix[x, y] * factor;
                }
            }
            return bMatrix;
        }
        private double [,] multiplicationMatrix(double [,] aMatrix, double [,] bMatrix)
        {
            int m, n, p;
            if (aMatrix.GetLength(1) == bMatrix.GetLength(0))
            {
                m = aMatrix.GetLength(0);
                n = bMatrix.GetLength(1);
                p = bMatrix.GetLength(0);
                double[,] cMatrix = new double[m, n];
                double helper;
                for (int x = 0; x < m; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        helper = 0;
                        for (int i = 0; i < p; i++)
                        {
                            helper += aMatrix[x, i] * bMatrix[i, y];
                        }
                        cMatrix[x, y] = helper;
                    }
                }
                return cMatrix;
            }
            else
            {
                MessageBox.Show("Матрицы не могут быть перемножены {multiplicationMatrix}");
                //Console.ReadKey(true);
                double[,] errorMatrix = new double[,] { { -1 } };
                return errorMatrix;
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
