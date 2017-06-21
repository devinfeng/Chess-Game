using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerCards
{
    /// <summary>
    /// 单例模型的实现类。
    /// </summary>
    /// <typeparam name="T">实际要创建的单例对象类型。</typeparam>
    public class ManagerBase<T> where T : new()
    {
        /// <summary>
        /// 获得单例对象的实例。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }

        /// <summary>
        /// 单例实例对象。
        /// </summary>
        private static T _instance;
    }
}
