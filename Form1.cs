using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace szyfrowanie_plikow
{
    public partial class Form1 : Form
    {
        public string[] passs;
        int leng_key = 128;
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {//random string generator
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnoprstuwyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static RijndaelManaged setupAES(byte[] passwordBytes, byte[] salt, RijndaelManaged AES)
        {
            AES = new RijndaelManaged();
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000, HashAlgorithmName.SHA512);
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);
            AES.Padding = PaddingMode.None;
            AES.Mode = CipherMode.CBC;
            return AES;
        }

        static string EncryptStringToBytes(string plainText, byte[] passwordBytes, byte[] salt)
        {
            string encrypted;
            RijndaelManaged AES = new RijndaelManaged();
            AES = setupAES(passwordBytes, salt, AES);
            //setup AES 

            ICryptoTransform encryptor = AES.CreateEncryptor(AES.Key, AES.IV); //create encryptor

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    byte[] temp = msEncrypt.ToArray();
                    encrypted = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            return encrypted;
        }

        static string DecryptStringFromBytes(byte[] toDec, byte[] passwordBytes, byte[] salt)
        {
            string plaintext = null;

            RijndaelManaged AES = new RijndaelManaged();
            AES=setupAES(passwordBytes,salt,AES);
            //setup AES 

            ICryptoTransform decryptor = AES.CreateDecryptor(AES.Key, AES.IV);

            using (MemoryStream msDecrypt = new MemoryStream(toDec))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
 
        public byte[] SaveHead(byte[] salt, string[] pass, string main)
        {
            string text = null;
            for (int i = 0; i < pass.Length; i++)
            {
                text = text + EncryptStringToBytes(main, Encoding.ASCII.GetBytes(pass[i]), salt) + "|";
            }
            return Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.ASCII.GetBytes(pass.Length.ToString() + "|" + Encoding.ASCII.GetString(salt) + "|" + text))+"\n");
        }
        public string[] ReadHead(string str, string passwd)
        {
            string[] a = str.Split('|');
            string[] b = new string[2];
            b[0] = a[1];


            for (int i = 0; i < int.Parse(a[0]); i++)
            {
                try
                {
                    string temp = DecryptStringFromBytes(Convert.FromBase64String(a[i + 2]), Encoding.ASCII.GetBytes(passwd), Encoding.ASCII.GetBytes(a[1]));
                    if (temp.Length == leng_key)
                        b[1] = temp;

                }
                catch { }
            }

            return b;
        }

        private void AES_Encrypt(string inputFile)
        {
            string passwd = RandomString(leng_key);
            byte[] salt = Encoding.ASCII.GetBytes(RandomString(32));

            FileStream fsCrypt = new FileStream(inputFile + ".aes", FileMode.Create);

            byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(passwd);

            RijndaelManaged AES = new RijndaelManaged();
            AES = setupAES(passwordBytes, salt, AES);
            AES.Padding = PaddingMode.PKCS7;

            byte[] to_save = SaveHead(salt, passs, passwd);
            fsCrypt.Write(to_save, 0, to_save.Length);

            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

            FileStream fsIn = new FileStream(inputFile, FileMode.Open);

            byte[] buffer = new byte[1048576];
            int read;

            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Application.DoEvents();
                    cs.Write(buffer, 0, read);
                }

                //close up
                fsIn.Close();

            }
            catch
            {
                //Debug.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                cs.Close();
                fsCrypt.Close();
                MessageBox.Show("File has encrypted!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AES_Decrypt(string inputFile, string password)
        {
            byte[] temp = new byte[1048576];
            int len = 0;
            byte[] salt = new byte[32];
            byte[] passwordBytes = new byte[leng_key];

            try
            {
                using (var stream1 = new StreamReader(new FileStream(inputFile, FileMode.Open)))
                {
                    string from_stream = stream1.ReadLine();
                    temp = Convert.FromBase64String(from_stream);
                    len = from_stream.Length;
                    stream1.Close();
                }

                string[] tempp = ReadHead(Encoding.ASCII.GetString(temp), password);
                salt = Encoding.ASCII.GetBytes(tempp[0]);
                if (tempp[1] != null)
                    passwordBytes = System.Text.Encoding.ASCII.GetBytes(tempp[1]);
                else
                {
                    MessageBox.Show("Incorrect password!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    passwordBytes = null;
                }

            }
            catch
            {
                MessageBox.Show("Selected corrupted or invaild file!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (passwordBytes != null && passwordBytes.Length!=0)
            {
                FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

                RijndaelManaged AES = new RijndaelManaged();
                AES = setupAES(passwordBytes, salt, AES);
                AES.Padding = PaddingMode.PKCS7;

                fsCrypt.Position = len + 1;
                fsCrypt.Seek(0, SeekOrigin.Current);

                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
                FileStream fsOut;
                if (inputFile.IndexOf(".aes") != -1)
                {
                    inputFile = inputFile.Substring(0, inputFile.IndexOf(".aes"));
                    fsOut = new FileStream(inputFile, FileMode.Create);
                }

                else
                {
                    fsOut = new FileStream(inputFile + ".decrypted", FileMode.Create);
                }

                int read;
                byte[] buffer = new byte[1048576];

                try
                {
                    while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Application.DoEvents();
                        fsOut.Write(buffer, 0, read);
                    }
                }
                catch //(System.Security.Cryptography.CryptographicException ex_CryptographicException)
                {
                    //Debug.WriteLine("CryptographicException error: " + ex_CryptographicException.Message);
                    fsOut.Close();
                    if(File.Exists(inputFile))
                        File.Delete(inputFile);
                }
                try
                {
                    cs.Close();
                }
                catch
                {
                    //Debug.WriteLine("Error by closing CryptoStream: " + ex.Message);
                }
                finally
                {
                    fsOut.Close();
                    fsCrypt.Close();
                    MessageBox.Show("File has decrypted!", "INFO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void encrypt_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog()!=DialogResult.Cancel)
            {
                using (var form = new Enc())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        int t = 0;
                        string[] buffer =new string[8];
                        for (int i = 0; i <= form.passwd.Length-1; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(form.passwd[i]))
                            {
                                buffer[i] = form.passwd[i];
                                t++;
                            }
                        }

                        passs = new string[t];

                        for (int i = 0; i < t; i++)
                        {
                                passs[i] = buffer[i];
                        }
                        AES_Encrypt(openFileDialog.FileName);
                    }
                }
            }
            passs = null;
        }

        private void decrypt_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "AES files (*.aes)|*.aes|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                using (var form = new Dec())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        AES_Decrypt(openFileDialog.FileName, form.passwd);
                    }
                }
            }
            passs = null;
        }
    }
}
