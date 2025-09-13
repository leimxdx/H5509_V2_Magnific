using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SicUIAssist
{
    public static class ControlDoubleBufferUtil
    {
        /// <summary>
        /// 为多个类型批量设置
        /// </summary>
        /// <param name="container"></param>
        /// <param name="controlTypes"></param>
        public static void EnableDoubleBuffering(Control container, params Type[] controlTypes)
        {
            foreach (var type in controlTypes)
            {
                EnableDoubleBuffering(container, type);
            }
        }

        /// <summary>
        /// 为指定容器内的所有指定类型控件启用双缓冲
        /// </summary>
        /// <param name="container"></param>
        /// <param name="controlType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void EnableDoubleBuffering(Control container, Type controlType)
        {
            // 参数校验
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (controlType == null) throw new ArgumentNullException(nameof(controlType));

            // 提前获取双缓冲属性的反射信息（优化性能）
            var prop = controlType.GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (prop == null) return;

            // 递归遍历控件树
            TraverseControls(container, controlType, prop);
        }


        private static void TraverseControls(Control parent, Type targetType, PropertyInfo doubleBufferedProp)
        {
            foreach (Control control in parent.Controls)
            {
                // 设置符合条件的控件
                if (control.GetType() == targetType)
                {
                    try
                    {
                        doubleBufferedProp.SetValue(control, true, null);
                    }
                    catch (Exception ex)
                    {
                        // 可记录日志或处理异常
                        Debug.WriteLine($"设置双缓冲失败: {ex.Message}");
                    }
                }

                // 递归遍历子控件
                if (control.HasChildren)
                {
                    TraverseControls(control, targetType, doubleBufferedProp);
                }
            }
        }
    }
}
