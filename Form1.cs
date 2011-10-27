using System;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace GoogleAppsProvisioningTest
{    

    public partial class Form1 : Form
    {
//
        const string HEAD = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><atom:entry xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:apps=\"http://schemas.google.com/apps/2006\"><atom:category scheme=\"http://schemas.google.com/g/2005#kind\" term=\"http://schemas.google.com/apps/2006#user\"/>";
        const string USR_NAME = "<apps:login userName=\"$$loginuser$$\"/>";
        const string USR_CREATE = "<apps:login userName=\"$$loginuser$$\" password=\"$$password$$\" hashFunctionName=\"SHA-1\"/>";
        const string PWD_CHANGE = "<apps:login password=\"$$password$$\" hashFunctionName=\"SHA-1\"/>";
        const string PWD_FORCE_CHANGE = "<apps:login changePasswordAtNextLogin=\"$$forcechange$$\"/>";
        const string STA_CHANGE = "<apps:login suspended=\"$$suspended$$\"/>";
        const string NAME_FULL = "<apps:name familyName=\"$$familyName$$\" givenName=\"$$givenName$$\"/>";
        const string NAME_FAM = "<apps:name familyName=\"$$familyName$$\"/>";
        const string NAME_GIVEN = "<apps:name givenName=\"$$givenName$$\"/>";
        const string BOTTOM = "</atom:entry>";

        string AuthNToken;
        string DOMAIN;
        string ADMIN_USER;
        string ADMIN_PWD;
        
        public Form1()
        {
            InitializeComponent();
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Authentication
            if ((textBox2.Text == "") || (textBox7.Text == "") || (textBox8.Text == ""))
            {
                MessageBox.Show("Check Parameters");
                return;
            }
            DOMAIN = textBox2.Text;
            ADMIN_USER = textBox7.Text;
            ADMIN_PWD = textBox8.Text;

            System.Collections.Specialized.NameValueCollection ps = new System.Collections.Specialized.NameValueCollection();
            ps.Add("accountType", "HOSTED");
            ps.Add("Email", ADMIN_USER + "@" + DOMAIN);
            ps.Add("Passwd", ADMIN_PWD);
            ps.Add("service", "apps");
            WebClient wc = new WebClient();
            try
            {
                byte[] resData = wc.UploadValues(@"https://www.google.com/accounts/ClientLogin", ps);
                wc.Dispose();
                string[] a = System.Text.Encoding.UTF8.GetString(resData).Split('\n');
                foreach (string str in a)
                {
                    if (str.StartsWith("Auth="))
                    {
                        label1.Text = str;
                        AuthNToken = str.Substring(5);
                    }
                }
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
                button7.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                textBox1.Text = "operation completed";
                button1.Enabled = false;
            }
            catch (System.Net.WebException ex)
            {
                //HttpWebResponseを取得
                System.Net.HttpWebResponse errres =
                    (System.Net.HttpWebResponse)ex.Response;
                textBox1.Text =
                    errres.StatusCode.ToString() + ":" + errres.StatusDescription;

            }
            finally
            {
                wc.Dispose();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Update Name
            WebClient wc = new WebClient();
            wc.Headers.Add("Content-type", "application/atom+xml");
            wc.Headers.Add("Authorization", "GoogleLogin auth=" + AuthNToken);

            string str_user = HEAD +
                              NAME_FULL.Replace("$$familyName$$", textBox5.Text).Replace("$$givenName$$", textBox6.Text) + 
                              BOTTOM;

            textBox1.Text = wc.UploadString("https://www.google.com/a/feeds/" + DOMAIN + "/user/2.0/" + textBox3.Text, "PUT", str_user);
            wc.Dispose();

        }

        private string sha1_create(string original)
        {
            byte[] byteValue = Encoding.UTF8.GetBytes(original);

            // SHA1のハッシュ値を取得する
            SHA1 crypto = new SHA1CryptoServiceProvider();
            byte[] hashValue = crypto.ComputeHash(byteValue);

            // バイト配列をUTF8エンコードで文字列化
            StringBuilder hashedText = new StringBuilder();
            for (int i = 0; i < hashValue.Length; i++)
            {
                hashedText.AppendFormat("{0:X2}", hashValue[i]);
            }
            return hashedText.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // Change Password
            WebClient wc = new WebClient();
            wc.Headers.Add("Content-type", "application/atom+xml");
            wc.Headers.Add("Authorization", "GoogleLogin auth=" + AuthNToken);

            string str_user = HEAD +
                              PWD_CHANGE.Replace("$$password$$", sha1_create(textBox4.Text)) +
                              BOTTOM;

            textBox1.Text = wc.UploadString("https://www.google.com/a/feeds/" + DOMAIN + "/user/2.0/" + textBox3.Text, "PUT", str_user);
            wc.Dispose();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Change Status
            WebClient wc = new WebClient();
            wc.Headers.Add("Content-type", "application/atom+xml");
            wc.Headers.Add("Authorization", "GoogleLogin auth=" + AuthNToken);

            string status;
            if (checkBox1.Checked == false)
            {
                status = "true";
            }
            else
            {
                status = "false";
            }

            string str_user = HEAD +
                              STA_CHANGE.Replace("$$suspended$$", status) +
                              BOTTOM;

            textBox1.Text = wc.UploadString("https://www.google.com/a/feeds/" + DOMAIN + "/user/2.0/" + textBox3.Text, "PUT", str_user);
            wc.Dispose();

        }

        private void button9_Click(object sender, EventArgs e)
        {
            // Force user to change password on next login
            WebClient wc = new WebClient();
            wc.Headers.Add("Content-type", "application/atom+xml");
            wc.Headers.Add("Authorization", "GoogleLogin auth=" + AuthNToken);

            string force;
            if (checkBox2.Checked == true)
            {
                force = "true";
            }
            else
            {
                force = "false";
            }

            string str_user = HEAD +
                              PWD_FORCE_CHANGE.Replace("$$forcechange$$", force) +
                              BOTTOM;

            textBox1.Text = wc.UploadString("https://www.google.com/a/feeds/" + DOMAIN + "/user/2.0/" + textBox3.Text, "PUT", str_user);
            wc.Dispose();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Delete user
            WebClient wc = new WebClient();
            try
            {
                wc.Headers.Add("Content-type", "application/atom+xml");
                wc.Headers.Add("Authorization", "GoogleLogin auth=" + AuthNToken);

                wc.UploadString("https://www.google.com/a/feeds/" + DOMAIN + "/user/2.0/" + textBox3.Text, "DELETE", "");
                textBox1.Text = "operation completed";
            }
            catch (System.Net.WebException ex)
            {
                //HttpWebResponseを取得
                System.Net.HttpWebResponse errres =
                    (System.Net.HttpWebResponse)ex.Response;
                textBox1.Text =
                    errres.StatusCode.ToString() + ":" + errres.StatusDescription;
 
            }
            finally
            {
                wc.Dispose();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Create User
            WebClient wc = new WebClient();
            try
            {
                wc.Headers.Add("Content-type", "application/atom+xml");
                wc.Headers.Add("Authorization", "GoogleLogin auth=" + AuthNToken);

                string str_user = HEAD +
                                  USR_CREATE.Replace("$$loginuser$$", textBox3.Text).Replace("$$password$$", sha1_create(textBox4.Text)) +
                                  NAME_FULL.Replace("$$familyName$$", textBox5.Text).Replace("$$givenName$$", textBox6.Text) +
                                  BOTTOM;
                wc.Encoding = System.Text.Encoding.UTF8;
                textBox1.Text = wc.UploadString("https://www.google.com/a/feeds/" + DOMAIN + "/user/2.0", str_user);
            }
            catch (System.Net.WebException ex)
            {
                //HttpWebResponseを取得
                System.Net.HttpWebResponse errres =
                    (System.Net.HttpWebResponse)ex.Response;
                textBox1.Text =
                    errres.StatusCode.ToString() + ":" + errres.StatusDescription;

            }
            finally
            {
                wc.Dispose();
            }
        }

    }
}
