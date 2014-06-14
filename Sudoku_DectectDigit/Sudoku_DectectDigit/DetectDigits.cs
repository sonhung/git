using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sudoku_DectectDigit
{
    public class DetectDigits
    {
        #region Contructor

        private int _defineWhite = -1;
        private int _defineBlack = -2;
        private int _defineHeight = -1;
        private WriteableBitmap _bitmapToFind, _bitmapToCut;
        private int[,] _digitalArray;
        private Dictionary<int, int> _numberElementOfColor;
        private int _numberColor;
        private int _Height;
        private int _Width;
        private int _top, _left, _bot, _right, _mainColor;
        private List<int> _ListColorToFind;

        #endregion

        /// <summary>
        /// ham goi tu ben ngoai de xu ly anh
        /// height trong ham chi de dung cho De Qui ( define height=-1)
        /// </summary>
        /// <param name="image"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public WriteableBitmap[,] Detect(WriteableBitmap image, int height)
        {
            _defineHeight = height;
            _bitmapToCut = ToAverageBinary(image);
            _bitmapToFind = FilterProcessImage(_bitmapToCut);
            _Height = _bitmapToFind.PixelHeight;
            _Width = _bitmapToFind.PixelWidth;
            _numberElementOfColor = new Dictionary<int, int>();
            _ListColorToFind = new List<int>();
            _numberColor = 0;
            _digitalArray = new int[_Height, _Width];

            try
            {
                SoHoaAnh(_bitmapToFind);
                PaintColor(ref _digitalArray);
                FindMaxColor(_digitalArray);
                return CutSubPuzzle(_bitmapToFind, _bitmapToCut);
            }
            catch 
            {
                return new WriteableBitmap[9, 9];
            }
        }

        #region processing

        /// <summary>
        /// chuyen image dang writeablebitmap sang dang array
        /// </summary>
        /// <param name="image"></param>
        private void SoHoaAnh(WriteableBitmap image)
        {
            int height = image.PixelHeight;
            int width = image.PixelWidth;
            _digitalArray = new int[height, width];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    byte[] color = BitConverter.GetBytes(image.Pixels[row * width + col]);
                    if (color[0] == 0 && color[1] == 0 && color[2] == 0 && color[3] == 255)// mau den
                        _digitalArray[row, col] = _defineBlack;
                    else //mau trang
                        _digitalArray[row, col] = _defineWhite;
                }
            }
        }
        /// <summary>
        /// to mau cho cac diem anh, cac diem anh ke nhau co cung 1 mau
        /// </summary>
        /// <param name="Array"></param>
        private void PaintColor(ref int[,] Array)
        {
            for (int row = 0; row < _Height; row++)
            {
                for (int col = 0; col < _Width; col++)
                {
                    if (Array[row, col] == _defineWhite)// neu la mau trang
                    {
                        if (col - 1 > 0 && Array[row, col - 1] >= 0) //left
                        {
                            int temp = Array[row, col - 1];
                            Array[row, col] = temp;
                            _numberElementOfColor[temp] += 1;
                        }
                        else if (row - 1 > 0 && col - 1 > 0 && Array[row - 1, col - 1] >= 0)//left+top
                        {
                            int temp = Array[row - 1, col - 1];
                            Array[row, col] = temp;
                            _numberElementOfColor[temp] += 1;
                        }
                        else if (row - 1 > 0 && Array[row - 1, col] >= 0)//top
                        {
                            int temp = Array[row - 1, col];
                            Array[row, col] = temp;
                            _numberElementOfColor[temp] += 1;
                        }
                        else if (row - 1 > 0 && col + 1 < _Width && Array[row - 1, col + 1] >= 0)//right+top
                        {
                            int temp = Array[row - 1, col + 1];
                            Array[row, col] = temp;
                            _numberElementOfColor[temp] += 1;
                        }
                        else
                        {
                            Array[row, col] = _numberColor;
                            _numberElementOfColor.Add(_numberColor, 1);
                            _numberColor++;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Tim list 9 vung 3x3 cua sudoku, hoac tra ve mau cua khung lon nhat cua Sudoku
        /// </summary>
        /// <param name="array"></param>
        private void FindMaxColor(int[,] array)
        {
            int col = _numberElementOfColor.Count;
            int index = _numberElementOfColor.OrderByDescending(i => i.Value).First().Key; ;
            int max = _numberElementOfColor[index];
            Dictionary<int, int> NColorDictionary = new Dictionary<int, int>();

            _top = FindTop(_digitalArray, index);
            _bot = FindBot(_digitalArray, index);
            _left = FindLeft(_digitalArray, index);
            _right = FindRight(_digitalArray, index);
            if ((_bot - _top) > 0.5 * _Height && (_right - _left) > 0.5 * _Width)
            {
                _mainColor = index;
                for (int i = _top; i < _bot; i++)
                {
                    for (int j = _left; j < _right; j++)//tim 9 mau trong vung co so phan tu lon nhat
                    {
                        int tempColor = _digitalArray[i, j];
                        if (tempColor >= 0 && tempColor != _mainColor)
                            if (!NColorDictionary.ContainsKey(tempColor))
                                NColorDictionary.Add(tempColor, _numberElementOfColor[tempColor]);
                    }
                }
                var query = NColorDictionary.OrderByDescending(i => i.Value).Take(9).ToList();
                // neu cac mau co so phan tu lon hon 0.7 lan so phan tu cua khung chinh
                // => tim dc 1 khung trong 9 khung cua soduku
                if (query.Count == 9 && query[8].Value > 0.7 * max)
                    _ListColorToFind = query.OrderBy(x => x.Key).Select(n => n.Key).ToList();
            }
            else
            {
                var query = _numberElementOfColor.OrderByDescending(i => i.Value).Take(9).ToList();

                //neu chenh lech giua cac mau khong qua 0.8 ket qua tra lai 9 mau tim dc
                // neu chenh lech qua 0.8 se ko tra ve mau nao cho  _ListColorToFind
                if (query.Count == 9 && query[8].Value > 0.8 * query[0].Value)
                    _ListColorToFind = query.OrderBy(x => x.Key).Select(n => n.Key).ToList();
                else
                {
                    max = -1;
                    index = -1;
                    int top, bot, left, right;
                    for (int i = 0; i < query.Count; i++)
                    {
                        top = FindTop(_digitalArray, query[i].Key);
                        bot = FindBot(_digitalArray, query[i].Key);
                        left = FindLeft(_digitalArray, query[i].Key);
                        right = FindRight(_digitalArray, query[i].Key);
                        if ((bot - top) * (right - left) > max)
                        {
                            max = (bot - top) * (right - left);
                            index = i;
                        }
                    }
                    _mainColor = query[index].Key;
                }
            }
        }
        /// <summary>
        /// tim dia chi diem anh tren cung trong array co mau la color
        /// </summary>
        /// <param name="array"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private int FindTop(int[,] array, int color)
        {

            for (int i = 0; i < _Height; i++)
            {
                for (int j = 0; j < _Width; j++)
                {
                    if (array[i, j] == color)
                        return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// tim dia chi diem anh Duoi cung trong array co mau la color
        /// </summary>
        /// <param name="array"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private int FindBot(int[,] array, int color)
        {

            for (int i = _Height - 1; i > 0; i--)
            {
                for (int j = 0; j < _Width; j++)
                {
                    if (array[i, j] == color)
                        return i;
                }
            }

            return -1;
        }
        /// <summary>
        /// tim dia chi diem anh nam gan ben trai nhat co mau la color
        /// </summary>
        /// <param name="array"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private int FindLeft(int[,] array, int color)
        {

            for (int j = 0; j < _Width; j++)
            {
                for (int i = 0; i < _Height; i++)
                {
                    if (array[i, j] == color)
                        return j;
                }
            }

            return -1;
        }
        /// <summary>
        /// tim dia chi diem anh nam gan ben phai nhat co mau la color
        /// </summary>
        /// <param name="array"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private int FindRight(int[,] array, int color)
        {

            for (int j = _Width - 1; j > 0; j--)
            {
                for (int i = 0; i < _Height; i++)
                {
                    if (array[i, j] == color)
                        return j;
                }
            }

            return -1;
        }
        /// <summary>
        /// cat lay tung vung nho chua ky tu so
        /// </summary>
        /// <param name="imageToFind"></param>
        /// <param name="imageToCut"></param>
        /// <returns></returns>
        private WriteableBitmap[,] CutSubPuzzle(WriteableBitmap imageToFind, WriteableBitmap imageToCut)
        {
            WriteableBitmap[,] result;
            int top, bot, left, right;
            int numberColor = _ListColorToFind.Count;
            int width, height, heightTb = 0;
            int color;
            if (numberColor == 9)
            {
                result = new WriteableBitmap[9, 9];
                WriteableBitmap[,] result1 = new WriteableBitmap[9, 9];
                for (int i = 0; i < 9; i++)
                {
                    _top = FindTop(_digitalArray, _ListColorToFind[i]);
                    _bot = FindBot(_digitalArray, _ListColorToFind[i]);
                    _left = FindLeft(_digitalArray, _ListColorToFind[i]);
                    _right = FindRight(_digitalArray, _ListColorToFind[i]);
                    width = (_right - _left) / 3;
                    height = (_bot - _top) / 3;
                    result[i / 3, i % 3] = imageToCut.Crop(_left, _top, _right - _left, _bot - _top);
                    for (int row = 0; row < 3; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            //result[i, row * 3 + col] = imageToCut.Crop(_left+width*col,_top+height*row,width,height);
                            color = FindTheBestColor(_digitalArray, new MPoint(_top + row * height, _left + col * width), new MPoint(_top + (row + 1) * height, _left + (col + 1) * width), _ListColorToFind[i]);
                            if (color == -1)
                                result[i, row * 3 + col] = null;
                            else
                            {
                                top = FindTop(_digitalArray, color);
                                bot = FindBot(_digitalArray, color);
                                left = FindLeft(_digitalArray, color);
                                right = FindRight(_digitalArray, color);
                                if (heightTb == 0) heightTb = bot - top;
                                if (heightTb < bot - top) heightTb = (heightTb + bot - top) / 2;
                                result1[i, row * 3 + col] = imageToCut.Crop(left, top, right - left, heightTb);
                            }
                        }
                    }
                }
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        result[j / 3 + i - i % 3, j % 3 + i % 3 * 3] = result1[i, j];
                    }
                }
            }
            else
            {
                if (_Width < 100 && _Height < 100)// su dung cho de qui class
                {
                    if (_numberElementOfColor.Count > 1)
                        color = FindTheBestColor(_digitalArray, new MPoint(_Height /4, _Width / 4), new MPoint(_Height - _Height / 4, _Width - _Width / 4), _mainColor);
                    else color = _mainColor;
                    _top = FindTop(_digitalArray, color);
                    _bot = FindBot(_digitalArray, color);
                    _left = FindLeft(_digitalArray, color);
                    _right = FindRight(_digitalArray, color);
                    if (_top > 0 && _bot > 0 && _right > 0 && _left > 0)
                    {
                        if (_defineHeight == -1 || _defineHeight < _bot - _top) _defineHeight = _bot - _top;
                        result = new WriteableBitmap[1, 1]; ;
                        result[0, 0] = imageToCut.Crop(_left, _top, _right - _left, _defineHeight);
                        return result;
                    }
                    else return new WriteableBitmap[1, 1];
                }
                else
                {
                    result = new WriteableBitmap[9, 9];
                    _top = FindTop(_digitalArray, _mainColor);
                    _bot = FindBot(_digitalArray, _mainColor);
                    _left = FindLeft(_digitalArray, _mainColor);
                    _right = FindRight(_digitalArray, _mainColor);
                    width = (_right - _left) / 9;
                    height = (_bot - _top) / 9;
                   // result[0, 0] = imageToFind.Crop(_left, _top, _right - _left, _bot - _top);
                    for (int row = 0; row < 9; row++)
                    {
                        for (int col = 0; col < 9; col++)
                        {
                            top = _top + height * row;
                            left = _left + width * col;
                            bot = _top + height * (row + 1);
                            right = _left + width * (col + 1);
                            if (top > 0 && left > 0 && right > 0 && left > 0)
                            {
                                WriteableBitmap temp = imageToCut.Crop(_left + col * width, _top + row * height, width, height);
                                WriteableBitmap[,] temp2 = new DetectDigits().Detect(temp, _defineHeight);
                                if (_defineHeight == -1 && temp2[0, 0] != null) _defineHeight = temp2[0, 0].PixelHeight;
                                result[row, col] = temp2[0, 0];
                               // result[row, col] = temp;
                            }                           
                            
                        }
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// tim mau co so phan tu lon nhat trong vung chi dinh
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="exceptColor"></param>
        /// <returns></returns>
        private int FindTheBestColor(int[,] matrix, MPoint topLeft, MPoint bottomRight, int exceptColor)
        {
            Dictionary<int, int> NColorDictionary = new Dictionary<int, int>();
            NColorDictionary.Add(-1, 10);
            for (int i = topLeft.Row; i < bottomRight.Row - 1; i++)
            {
                for (int j = topLeft.Col; j < bottomRight.Col - 1; j++)
                {
                    if (matrix[i, j] != exceptColor && matrix[i, j] >= 0)
                    {
                        if (!NColorDictionary.ContainsKey(matrix[i, j]))
                        {
                            NColorDictionary.Add(matrix[i, j], 1);
                        }
                        else
                            NColorDictionary[matrix[i, j]] += 1;
                    }
                }
            }
            return NColorDictionary.OrderByDescending(i => i.Value).First().Key;
        }
        /// <summary>
        /// chuyen anh RBG sang anh trang den (binary image)
        /// </summary>
        /// <param name="grayscale"></param>
        /// <returns></returns>
        private WriteableBitmap ToAverageBinary(WriteableBitmap grayscale)
        {
            WriteableBitmap binary =
                new WriteableBitmap(grayscale.PixelWidth,
                    grayscale.PixelHeight);

            int[] histogramData = new int[256];
            int maxCount = 0;
            //first we will determine the histogram
            //for the grayscale image
            for (int pixelIndex = 0;
                pixelIndex < grayscale.Pixels.Length;
                pixelIndex++)
            {
                byte intensity = (byte)grayscale.Pixels[pixelIndex];
                //simply add another to the count
                //for that intensity
                histogramData[intensity]++;
                if (histogramData[intensity] > maxCount)
                {
                    maxCount = histogramData[intensity];
                }
            }
            //now we need to figure out the average intensity
            long average = 0;
            for (int intensity = 0; intensity < 256; intensity++)
            {
                average += intensity * histogramData[intensity];
            }
            //this is our threshold
            average /= grayscale.Pixels.Length;
            for (int pixelIndex = 0;
                pixelIndex < grayscale.Pixels.Length;
                pixelIndex++)
            {
                byte intensity = (byte)grayscale.Pixels[pixelIndex];
                //now we’re going to set the pixels
                //greater than or equal to the average
                //to white and everything else to black
                if (intensity >= average)
                {
                    intensity = 255;
                    unchecked { binary.Pixels[pixelIndex] = (int)0xFFFFFFFF; }
                }
                else
                {
                    intensity = 0;
                    unchecked { binary.Pixels[pixelIndex] = (int)0xFF000000; }
                }
            }
            return binary;
        }
        /// <summary>
        /// tim canh trong anh sau khi anh da chuyen sang binary
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private WriteableBitmap FilterProcessImage(WriteableBitmap image)
        {
            WriteableBitmap ret = new WriteableBitmap(image.PixelWidth, image.PixelHeight);
            for (int i = 1; i < image.PixelWidth - 1; i++)
            {
                for (int j = 1; j < image.PixelHeight - 1; j++)
                {
                    Color cr = image.GetPixel(i + 1, j);
                    Color cl = image.GetPixel(i - 1, j);
                    Color cu = image.GetPixel(i, j - 1);
                    Color cd = image.GetPixel(i, j + 1);
                    Color cld = image.GetPixel(i - 1, j + 1);
                    Color clu = image.GetPixel(i - 1, j - 1);
                    Color crd = image.GetPixel(i + 1, j + 1);
                    Color cru = image.GetPixel(i + 1, j - 1);
                    int power = getMaxD(cr.R, cl.R, cu.R, cd.R, cld.R, clu.R, cru.R, crd.R);
                    if (power > 50)
                        ret.SetPixel(i, j, Colors.White);
                    else
                        ret.SetPixel(i, j, Colors.Black);
                }
            }
            return ret;
        }
        private int getD(int cr, int cl, int cu, int cd, int cld, int clu, int cru, int crd, int[,] matrix)
        {
            return Math.Abs(matrix[0, 0] * clu + matrix[0, 1] * cu + matrix[0, 2] * cru
               + matrix[1, 0] * cl + matrix[1, 2] * cr
                  + matrix[2, 0] * cld + matrix[2, 1] * cd + matrix[2, 2] * crd);
        }
        private int getMaxD(int cr, int cl, int cu, int cd, int cld, int clu, int cru, int crd)
        {
            int max = int.MinValue;
            for (int i = 0; i < templates.Count; i++)
            {
                int newVal = getD(cr, cl, cu, cd, cld, clu, cru, crd, templates[i]);
                if (newVal > max)
                    max = newVal;
            }
            return max;
        }
        private List<int[,]> templates = new List<int[,]> 
{
   new int[,] {{ -3, -3, 5 }, { -3, 0, 5 }, { -3, -3, 5 } },
   new int[,] {{ -3, 5, 5 }, { -3, 0, 5 }, { -3, -3, -3 } },
   new int[,] {{ 5, 5, 5 }, { -3, 0, -3 }, { -3, -3, -3 } },
   new int[,] {{ 5, 5, -3 }, { 5, 0, -3 }, { -3, -3, -3 } },
   new int[,] {{ 5, -3, -3 }, { 5, 0, -3 }, { 5, -3, -3 } },
   new int[,] {{ -3, -3, -3 }, { 5, 0, -3 }, { 5, 5, -3 } },
   new int[,] {{ -3, -3, -3 }, { -3, 0, -3 }, { 5, 5, 5 } },
   new int[,] {{ -3, -3, -3 }, { -3, 0, 5 }, { -3, 5, 5 } }
};
        #endregion
    }
    public class MPoint
    {
        public int Row { set; get; }
        public int Col { set; get; }
        public MPoint(int _row, int _col)
        {
            Row = _row;
            Col = _col;
        }
        public MPoint()
        {
            Row = 0;
            Col = 0;
        }
    }
}
