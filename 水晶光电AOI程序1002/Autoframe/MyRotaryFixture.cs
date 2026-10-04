using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoframe
{
    /// <summary>
    /// 转盘工位枚举
    /// </summary>
    public enum RotaryFixture
    {
        上料,
        撕膜,
        组装,
        复检,
        下料,
    }

    public class MyRotaryFixture : RotaryTable<RotaryFixture>.Fixture
    {
        //工位的DI、DO、气缸、数据，任何东西
        public DI diVacuum;
        public DO doVacuum;
        public ProductData productData = null;

        //子类构造函数
        public MyRotaryFixture(RotaryFixture enumFixture) : base(enumFixture)
        {
        }

        //转盘实例        
        public static RotaryTable<RotaryFixture> rotaryTable = new RotaryTable<RotaryFixture>();
        /// <summary>
        /// 获取转盘夹具
        /// </summary>
        /// <param name="rotaryFixture"></param>
        /// <returns></returns>
        public static MyRotaryFixture Get(RotaryFixture rotaryFixture)
        {
            return rotaryTable[rotaryFixture] as MyRotaryFixture;
        }

        /// <summary>
        /// 初始化，填充RotaryFixture
        /// </summary>
        public static void Init()
        {
            for (int i = 1; i <= 5; i++)
            {
                RotaryFixture rotaryFixture = (RotaryFixture)(i - 1);
                DI di = (DI)Enum.Parse(typeof(DI), $"转盘真空{i}");
                DO do1 = (DO)Enum.Parse(typeof(DO), $"转盘真空{1}_吸");
                rotaryTable.Set(new MyRotaryFixture(rotaryFixture)
                {
                    diVacuum = di,
                    doVacuum = do1,
                });

                rotaryTable.SpaceChanged?.Invoke("转盘", rotaryFixture.ToString(),null);
            }
        }        
    }
}
