using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    /// <summary>
    /// 流程管理数据结构体
    /// </summary>
    public struct ST_ProcessManage
    {
        /// <summary>
        /// 当前步ID
        /// </summary>
        public int StepID;

        /// <summary>
        /// 动作名称
        /// </summary>
        public string StepName;

        /// <summary>
        /// 过程中标志
        /// </summary>
        public bool Processing;

        /// <summary>
        /// 过程完成标志
        /// </summary>
        public bool Done;

        /// <summary>
        /// 过程执行条件
        /// </summary>
        public bool Condition;

        /// <summary>
        /// 过程执行允许
        /// </summary>
        public bool Permit;

    }
}
