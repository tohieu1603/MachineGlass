using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AF.Common;
using System.Threading;
using System.Diagnostics;
using AF.Card;
using AF.CardEx;
using AF.Data;
using Autoframe;
using AF.Station;
using AF.File;

namespace Autoframe.UI
{
    public partial class Form_Test: Form
    {
        public Form_Test()
        {
            InitializeComponent();
        }

        private void frm_test_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ProductData pd = new ProductData();
            pd.ShowData();

            //SdoData sdo = new SdoData(1,1, new byte[] { 0x40,0x07,0x40,0x27 });
            //sdo.Reverse();
            //string str = sdo.Get<string>();


            //ServoAlarm_Servotronix servo = new ServoAlarm_Servotronix();
            //var sss= servo.AlarmCode;

            //StationMgr.GetStation("下料站").LoadPoint(@"D:\exe\Point2.xml");
            //return;
            ////EventMgr.Warn("SSSSSS");

            //short n = 0;
            //CardMgr.Card[0].ReadAI(0, ref n);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //SdoData sdo = new SdoData(1, 1, 4);
            //CardMgr.Card[0].GetSDO(1, ref sdo);
            //int? nnn = sdo.Get<int>();

            //PdoData pdo = new PdoData(1, 1, 4);
            //CardMgr.Card[0].GetPDO(1, ref pdo);
            //float? fff = sdo.Get<float>();



            //CardMgr.Card[0].SetSDO(0, SdoData.New<int>(1, 1, 789));

            //CardMgr.Card[0].SetPDO(0, PdoData.New<Single>(1, 1, 0.1f));

            //CardMgr.Card[0].SetPDO(0, PdoData.New<string>(1, 1, "1234"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Statistics.UpdateMaterial(Material.Chassis);
            Statistics.UpdateMaterial(Material.Chassis,ThrowReason.Chassis_相机定位NG);
        }

        private void 清料_Click(object sender, EventArgs e)
        {
            RegBit.摆臂停止上料.Set(true);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //ProductInfo productInfo = new ProductInfo();

            ////ProductBase productBase = productInfo as ProductBase;
            //productInfo.Result = true;


            //productInfo.ShowData();
            //productInfo.Result = false;
            //productInfo.ShowData();

            //ProductData productData1 = new ProductData();
            //productData1.Result1 = false;
            //this[(NEST)0] = productData1;
            //this[(NEST)0].ShowData();
        }
    }
}
