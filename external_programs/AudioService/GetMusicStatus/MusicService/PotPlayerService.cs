using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using CSCore.CoreAudioAPI;

public class PotPlayerService
{
    public static void PrintMusicStatus(AudioSessionManager2 sessionManager)
    {
        Console.OutputEncoding = Encoding.UTF8;

        double volume = 0;
        bool musicAppRunning = false;
        string windowTitle = "";
        string procName = "";

        try
        {
            AudioSessionEnumerator sessionEnumerator = sessionManager.GetSessionEnumerator();
            
            AudioSessionControl2 sessionControl;

            // 遍历所有会话，寻找匹配的进程
            foreach (AudioSessionControl session in sessionEnumerator)
            {
                if (session == null)
                {
                    continue;
                }

                sessionControl = session.QueryInterface<AudioSessionControl2>();
                if (sessionControl == null || sessionControl.Process == null)
                {
                    continue;
                }

                string processName = sessionControl.Process.ProcessName;

                if (processName.StartsWith("PotPlayer"))
                {
                    musicAppRunning = true;
                    volume = session.QueryInterface<AudioMeterInformation>().PeakValue;
                    windowTitle = sessionControl.Process.MainWindowTitle;
                    procName = processName;
                    break;
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("None");
            return;
        }

        // 未检测到音乐软件进程
        if (!musicAppRunning)
        {
            Console.WriteLine("None");
            return;
        }

        // 如果 PotPlayer 开启了其它窗口，那么主窗口标题就不是歌曲信息了
        // 此时，需要遍历该进程的所有窗口来获取有效窗口标题
        try
        {
            if (string.IsNullOrEmpty(windowTitle) || !windowTitle.Contains('.'))
            {
                windowTitle = "";

                List<string> allTitles = WindowDetector.GetWindowTitles(procName);
                foreach (string title in allTitles)
                {
                    if (title.Contains('.'))  // 出现了点就说明有文件扩展名，通过这个来判断出有效窗口标题
                    {
                        windowTitle = title;
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("None");
            return;
        }

        // 如果窗口标题为空（说明没成功获取到），则返回 None
        if (string.IsNullOrEmpty(windowTitle))
        {
            Console.WriteLine("None");
            return;
        }

        // 修正窗口标题
        windowTitle = FixTitlePotPlayer(windowTitle);

        // 输出结果
        string status = volume > 0.00001 ? "Playing" : "Paused";
        Console.WriteLine(status);
        Console.WriteLine(windowTitle);
    }

    /*
        修正 PotPlayer 标题
        "Christopher Cross - Sailing.flac - PotPlayer" → "Christopher Cross - Sailing"
    */
    static string FixTitlePotPlayer(string windowTitle)
    {
        int lastDotIndex = windowTitle.LastIndexOf('.');

        if (lastDotIndex != -1)
        {
            windowTitle = windowTitle.Substring(0, lastDotIndex);
        }

        return windowTitle.Trim();
    }
}