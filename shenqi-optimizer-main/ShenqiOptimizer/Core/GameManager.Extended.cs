using System;
using ShenqiOptimizer.Features;

namespace ShenqiOptimizer.Core
{
    public partial class GameManager
    {
        private GameMemory gameMemory;

        /// <summary>
        /// 获取 GameMemory 实例
        /// </summary>
        public GameMemory GetGameMemory()
        {
            return gameMemory;
        }
    }
}