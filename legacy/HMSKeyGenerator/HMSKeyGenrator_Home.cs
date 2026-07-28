using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMSKeyGenerator
{
    public partial class HMSKeyGenrator_Home : Form
    {
        private KeyController keycontroller = null;
        public HMSKeyGenrator_Home()
        {
            InitializeComponent();
            keycontroller = new KeyController();
        }

        private void button_Encrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_ComputerName.Text))
            {
                errorProvider_Home.SetError(textBox_ComputerName, "Computer Name Required");

                return;
            }
            // LEGACY-SECRET-SCRUBBED: original literal key was the same string used as the
            // SQL Server SA password (single-secret-multiple-uses anti-pattern). Removed for
            // fork hygiene. See /SECURITY.md. v2 cloud uses Azure Key Vault for all keys.
            string password = Environment.GetEnvironmentVariable("HMS_KEYGEN_PASSWORD") ?? "__LEGACY_SECRET_REMOVED__";
            string notEncryptedText = "RITHMS"+textBox_ComputerName.Text;
            string encryptedText = keycontroller.Encrypt(password, notEncryptedText);
            textBox_ComputerName.Text = encryptedText;
            textBox_ComputerName.Enabled = false;
        }

        private void button_Decrypt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_ComputerName.Text))
            {
                errorProvider_Home.SetError(textBox_ComputerName, "Encrypted Computer Name Required");
                return;
            }
            // LEGACY-SECRET-SCRUBBED: see /SECURITY.md
            string password = Environment.GetEnvironmentVariable("HMS_KEYGEN_PASSWORD") ?? "__LEGACY_SECRET_REMOVED__";
            string encryptedText = textBox_ComputerName.Text;
            string notEncryptedText = keycontroller.Decrypt(password, encryptedText);
            textBox_ComputerName.Text = notEncryptedText;
            textBox_ComputerName.Enabled = false;
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            textBox_ComputerName.Clear();
            errorProvider_Home.Clear();
            textBox_ComputerName.Enabled = true;
            //  textBox_Password.Clear();

            //int[] arr = {1,2,3,4,5,6,7,8,5 };

            //for (int i = 0; i < arr.Length / 2; i++)
            //{
            //    int tmp = arr[i];
            //    arr[i] = arr[arr.Length - i - 1];
            //    arr[arr.Length - i - 1] = tmp;
            //}

            //int temp = 0;

            //for (int i = 0; i <= arr.Length - 1; i++)
            //{
            //    for (int j = i + 1; j < arr.Length; j++)
            //    {
            //        if (arr[i] > arr[j])
            //        {
            //            temp = arr[i];
            //            arr[i] = arr[j];
            //            arr[j] = temp;
            //        }
            //    }
            //}

            //var ss = 0;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox_ComputerName.Text))
            {
                Clipboard.SetText(textBox_ComputerName.Text);
            }
        }

        private void HMSKeyGenrator_Home_Load(object sender, EventArgs e)
        {
            label_Version.Text = "";

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                Version ver = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                label_Version.Text = string.Format("version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
            }
            else
            {
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                label_Version.Text = string.Format("version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
            }
        }
    }
}
