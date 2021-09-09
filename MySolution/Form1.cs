using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySolution
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Bitmap bitmapOriginal;
        private Bitmap bitmapModified;

        //load crypt
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = @"C:\Users\Main\Desktop\Информационная безопасность\Лабы\Лр5 Steganography";
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                try
                {
                    bitmapOriginal = (Bitmap)Bitmap.FromFile(fileName);
                    this.CenterToScreen();



                    //get Graphics object for painting original
                    Graphics gPanelOriginal = Graphics.FromHwnd(ImagePanel.Handle);

                    //draw original bitmap into panel
                    gPanelOriginal.DrawImage(bitmapOriginal, new Point(0, 0));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image. " + ex.Message);
                }
            }
        }
        //save crypt
        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.InitialDirectory = @"C:\Users\Main\Desktop\Информационная безопасность\Лабы\Лр5 Steganography";
            saveFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

            if(saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                bitmapModified.Save(fileName);
            }
        }
        //load decrypt
        private void buttonLoad2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = @"C:\Users\Main\Desktop\Информационная безопасность\Лабы\Лр5 Steganography";
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                try
                {
                    bitmapModified = (Bitmap)Bitmap.FromFile(fileName);
                    this.CenterToScreen();



                    //get Graphics object for painting original
                    Graphics gPanelOriginal = Graphics.FromHwnd(ImagePanel2.Handle);

                    //draw original bitmap into panel
                    gPanelOriginal.DrawImage(bitmapModified, new Point(0, 0));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image. " + ex.Message);
                }
            }
        }

        //crypt
        private void buttonCrypt_Click(object sender, EventArgs e)
        {
            try
            {
                //show wait cursor
                this.Cursor = Cursors.WaitCursor;

                //start off with copy of original image
                bitmapModified = new Bitmap(bitmapOriginal, bitmapOriginal.Width, bitmapOriginal.Height);
                string encryptedBytes = CipherClass.Encrypt(TextBox.Text, PasswordBox.Text);
                MessageBox.Show("Encrypted Bytes: "+encryptedBytes);

                //get original message to be hidden
                int numberbytes = (byte)encryptedBytes.Length * 2;
                byte[] bytesOriginal = new byte[numberbytes + 1];
                bytesOriginal[0] = (byte)numberbytes;

                //// RJD algoritm
                Encoding.UTF8.GetBytes(encryptedBytes, 0, encryptedBytes.Length, bytesOriginal, 1);

                //set bits 1, 2, 3 of byte into LSB red
                //set bits 4, 5, 6 of byte into LSB green
                //set bits 7 and 8 of byte into LSB blue
                int byteCount = 0;
                for (int i = 0; i < bitmapOriginal.Width; i++)
                {
                    for (int j = 0; j < bitmapOriginal.Height; j++)
                    {
                        if (bytesOriginal.Length == byteCount)
                            return;
                        Color clrPixelOriginal = bitmapOriginal.GetPixel(i, j);
                        byte r = (byte)((clrPixelOriginal.R & ~0x7) | (bytesOriginal[byteCount] >> 0) & 0x7);
                        byte g = (byte)((clrPixelOriginal.G & ~0x7) | (bytesOriginal[byteCount] >> 3) & 0x7);
                        byte b = (byte)((clrPixelOriginal.B & ~0x3) | (bytesOriginal[byteCount] >> 6) & 0x3);
                        byteCount++;
                        //set pixel to modified color
                        bitmapModified.SetPixel(i, j, Color.FromArgb(r, g, b));
                    }
                }


            }
            catch(Exception ex)
            {
                MessageBox.Show("Error hiding message." + ex.Message);
            }
            finally
            {
                //show normal cursor
                this.Cursor = Cursors.Arrow;
                //repaint
                Invalidate();
            }
        }
        //decrypt
        private void button1_Click(object sender, EventArgs e)
        {
            byte[] bytesExtracted = new byte[256 + 1];
            try
            {
                //show wait cursor, can be time-consuming
                this.Cursor = Cursors.WaitCursor;

                //get bits 1, 2, 3 of byte from LSB red
                //get bits 4, 5, 6 of byte from LSB green
                //get bits 7 and 8 of byte from LSB blue
                int byteCount = 0;
                for (int i = 0; i < this.bitmapModified.Width; i++)
                {
                    for (int j = 0; j < this.bitmapModified.Height; j++)
                    {
                        if (bytesExtracted.Length == byteCount)
                            return;

                        Color clrPixelModified = this.bitmapModified.GetPixel(i, j);
                        byte bits123 = (byte)((clrPixelModified.R & 0x7) << 0);
                        byte bits456 = (byte)((clrPixelModified.G & 0x7) << 3);
                        byte bits78 = (byte)((clrPixelModified.B & 0x3) << 6);
                        bytesExtracted[byteCount] = (byte)(bits78 | bits456 | bits123);
                        byteCount++;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error extracting message." + ex.Message);
            }
            finally
            {
                //show normal cursor
                this.Cursor = Cursors.Arrow;

                //get number of bytes from start of array
                int numberbytes = bytesExtracted[0];

                //get remaining bytes in array into string
                char[] cipheredBytes = Encoding.UTF8.GetString(bytesExtracted, 1, numberbytes).ToCharArray();
                string g = "";
                for (int i = 0; i < cipheredBytes.Length - cipheredBytes.Length / 2; g += cipheredBytes[i], i++) ;
                string variable = g;

                MessageBox.Show(variable, g + " " + cipheredBytes.Length);

                TextBox2.Text = CipherClass.Decrypt(g, PasswordBox2.Text);


            }
        }
    }
}
