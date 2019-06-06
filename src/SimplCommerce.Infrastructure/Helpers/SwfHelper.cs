using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/****************************************************************
*   Author：L
*   Time：2019/5/28 14:35:31
*   FrameVersion：2.2
*   Description：
*
*****************************************************************/
namespace SimplCommerce.Infrastructure.Helpers
{
    public static class SwfHelper
    {

        #region =============属性============



        #endregion

        #region ===========构造函数==========



        #endregion

        #region ===========基本方法==========



        #endregion

        #region =============方法============

        public static void Pdf2Img(string pdfPath,string imgFloderPath)
        {
            string fileName = Environment.CurrentDirectory + "/wwwroot/tools/mudraw.exe";
            var psi = new ProcessStartInfo(fileName, " -r100 -o " + imgFloderPath + "pdf_%d.png " + pdfPath) { RedirectStandardOutput = true };
            var proc= Process.Start(psi);
            if (proc == null)
            {
                Console.WriteLine("Can not exec.");
            }
            else
            {
                Console.WriteLine("-------------Start read standard output--------------");
                //开始读取
                using (var sr = proc.StandardOutput)
                {
                    while (!sr.EndOfStream)
                    {
                        Console.WriteLine(sr.ReadLine());
                    }

                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }
                Console.WriteLine("---------------Read end------------------");
                Console.WriteLine($"Total execute time :{(proc.ExitTime - proc.StartTime).TotalMilliseconds} ms");
                Console.WriteLine($"Exited Code ： {proc.ExitCode}");
            }
        }

        #endregion
    }
}
