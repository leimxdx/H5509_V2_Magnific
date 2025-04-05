using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    /// <summary>
    /// 操作返回结果
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }


        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMsg { get; set; }

        /// <summary>
        /// 返回成功的操作结果
        /// </summary>
        /// <returns>操作结果</returns>
        public static OperationResult CreateSuccessResult()
        {
            return new OperationResult()
            {
                IsSuccess = true,
                ErrorMsg = "OK"
            };
        }

        /// <summary>
        /// 返回失败的操作结果   
        /// </summary>
        /// <returns>操作结果</returns>
        public static OperationResult CreateFailResult()
        {
            return new OperationResult()
            {
                IsSuccess = false,
                ErrorMsg = "NG"
            };
        }
    }
}
