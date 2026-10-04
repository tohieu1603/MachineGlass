using System;
using AF.Station;


namespace Autoframe
{
    /// <summary>
    /// 产品数据，可添加定制化属性
    /// </summary>
    public class ProductData : ProductBase
    {
        ///// <summary>
        ///// SN
        ///// </summary>
        //public string  SN { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        /// <summary>
        /// 开始时间
        /// </summary>
        public string  DtStartTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff");

        /// <summary>
        /// 结束时间
        /// </summary>
        public string  DtEndTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff");


        /// <summary>
        /// 上钢环时间
        /// </summary>
        public string UpRingTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        ///// <summary>
        ///// 结果
        ///// </summary>
        //public bool Result { get; set; } = false;

        /// <summary>
        /// 钢环内料位全局序号（扩膜站取料时分配，AR最终判定后回写监控窗体）
        /// </summary>
        public int PianPos { get; set; } = 0;

        /// <summary>
        /// NG覆膜位全局序号
        /// </summary>
        public int NGPos { get; set; } = 0;

        /// <summary>
        /// OK覆膜位全局序号
        /// </summary>
        public int OKPos { get; set; } = 0;
        /// <summary>
        /// NG原因
        /// </summary>
        public string NGReason { get; set; } = "";

        /// <summary>
        /// 节拍,相应单独吸嘴放料
        /// </summary>
        public string CT { get; set; } = "";
        /// <summary>
        /// CCD3穴位
        /// </summary>
        public string CCD3Sit { get; set; } = "";

        /// <summary>
        /// CCD6穴位
        /// </summary>
        public string CCD6Sit { get; set; } = "";
        /// <summary>
        /// 上料钢环号
        /// </summary>
        public int Ringsite { get; set; } = 0;
        /// <summary>
        /// 宕机时长
        /// </summary>
        public double BreakDownTime { get; set; } = 0;
        public string[] strings { get; set; } = { "", "" };
    }

    public class Carrier : ProductContainer
    {
        public Carrier(string strSN = null):base(strSN)
        {
            this.Length = 48;
            if(strSN == null)
                this.SN = $"Carrier_{DateTime.Now}";
        }

        public ProductData this[int n]
        {
            get { return this.Container[n] as ProductData; }
            set { this.Container[n] = value; }
        }

        public ProductData Current
        {
            get { return this.Container[Index] as ProductData; }
            set { this.Container[Index] = value; }
        }
    }

    public class Tray : Carrier
    {
        public Tray(string strSN = null) : base(strSN)
        {
            this.Length = 48;
            if (strSN == null)
                this.SN = $"Tray_{DateTime.Now}";
        }
    }

    public class Fixture : Carrier
    {
        public Fixture(string strSN = null) : base(strSN)
        {
            this.Length = 48;
            if (strSN == null)
                this.SN = $"Fixture_{DateTime.Now}";
        }
    }

    public class Magazine : ProductContainer
    {
        public Magazine(string strSN = null) : base(strSN)
        {
            this.Length = 10;
            if (strSN == null)
                this.SN = $"Magazine_{DateTime.Now}";
        }

        public Carrier this[int n]
        {
            get { return this.Container[n] as Carrier; }
            set { this.Container[n] = value; }
        }

        public Carrier Current
        {
            get { return this.Container[Index] as Carrier; }
            set { this.Container[Index] = value; }
        }
    }
}
