using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using S7.Net; // استفاده از S7NetPlus
using System.IO;
using Newtonsoft.Json;

namespace RSA.PlcTester
{

    public partial class MainForm : Form
    {
        private Plc plc;
        private PlcSettings plcSettings;

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
        }
        private void LoadSettings()
        {
            try
            {
                // مسیر فایل JSON
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.json");

                // خواندن و تبدیل JSON به شیء
                string jsonContent = File.ReadAllText(jsonPath);
                plcSettings = JsonConvert.DeserializeObject<PlcSettings>(jsonContent);

                // نمایش تنظیمات در فرم (اختیاری)
                txtIpAddress.Text = plcSettings.Plc.IpAddress;
                txtRack.Text = plcSettings.Plc.Rack.ToString();
                txtSlot.Text = plcSettings.Plc.Slot.ToString();
                txtReadAddress.Text = plcSettings.Plc.ReadAddress;
                txtWriteAddress.Text = plcSettings.Plc.WriteAddress;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در بارگذاری تنظیمات: {ex.Message}");
            }
        }




        private void ConnectButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                // اتصال به PLC با تنظیمات خوانده‌شده
                plc = new Plc(CpuType.S71200, plcSettings.Plc.IpAddress, (short)plcSettings.Plc.Rack, (short)plcSettings.Plc.Slot);
                plc.Open();

                lblStatus.Text = plc.IsConnected ? "متصل شد!" : "اتصال برقرار نشد.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در اتصال: {ex.Message}");
            }
        }

        private void writeButton_clicked(object sender, EventArgs e)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    int newValue = Convert.ToInt32(txtWriteValue.Text);
                    plc.Write(plcSettings.Plc.WriteAddress, newValue);
                }
                else
                {
                    MessageBox.Show("ابتدا به PLC متصل شوید.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در نوشتن داده‌ها: {ex.Message}");
            }
        }

        private void readButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    object value = plc.Read(plcSettings.Plc.ReadAddress);
                    txtReadValue.Text = value.ToString();
                }
                else
                {
                    MessageBox.Show("ابتدا به PLC متصل شوید.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در خواندن داده‌ها: {ex.Message}");
            }
        }

        private void btnGet_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (plc != null && plc.IsConnected)
                {
                    // آدرس شروع آرایه و تعداد مقادیر از TextBox ها
                    var arrayStartAddress = Convert.ToInt32(txtArrayStartAddress.Text);
                    int arrayLength = Convert.ToInt32(txtArrayLength.Text);

                    // خواندن مقادیر از PLC
                    var values = plc.ReadBytes(DataType.DataBlock, 1, arrayStartAddress, arrayLength);

                    // نمایش مقادیر در TextBox یا ListBox
                    var result = "";
                    foreach (var value in values)
                    {
                        result+=value.ToString() + Environment.NewLine;
                    }

                    txtLog.Text = result;
                }
                else
                {
                    MessageBox.Show("ابتدا به PLC متصل شوید.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطا در خواندن آرایه: {ex.Message}");
            }
        }
    }


    // کلاس تنظیمات
    public class PlcSettings
    {
        public PlcInfo Plc { get; set; }
    }

    public class PlcInfo
    {
        public string IpAddress { get; set; }
        public int Rack { get; set; }
        public int Slot { get; set; }
        public string ReadAddress { get; set; }
        public string WriteAddress { get; set; }
    }
}