using Fiddler;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Fortnite_Cosmetics_Unlocker;

class Program
{
    enum CtrlType
    {
        CTRL_CLOSE_EVENT = 2, // × ボタン
        CTRL_LOGOFF_EVENT = 4, // ログオフ
        CTRL_SHUTDOWN_EVENT = 5, // シャットダウン
    }

    delegate bool PHandlerRoutine(CtrlType ctrlType);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleCtrlHandler(PHandlerRoutine handlerRoutine, bool add);

    static void Main(string[] args)
    {
        Console.WriteLine("[+] Fortnite Cosmetics Unlocker made by biru and landmark");

        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "profiles")))
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "profiles"));
        }

        string[] profile_template = new[]
        {
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/athena.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/campaign.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/collection_book_people0.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/collection_book_schematics0.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/collections.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/common_core.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/common_public.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/creative.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/metadata.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/outpost0.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/recycle_bin.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/theater0.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/theater1.json",
            "https://raw.githubusercontent.com/notbirufn/profile_template/refs/heads/main/theater2.json",
        };

        using (WebClient webClient = new WebClient())
        {
            foreach (string profile in profile_template)
            {
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "profiles", profile.Split('/').Last())))
                    continue;

                Console.WriteLine($"[+] Downloading {profile.Split('/').Last()}...");
                webClient.DownloadFile(profile, Path.Combine(Directory.GetCurrentDirectory(), "profiles", profile.Split('/').Last()));
            }
        }

        if (CertMaker.CreateRootCertificate() && CertMaker.TrustRootCertificate())
        {
            FiddlerApplication.BeforeRequest += OnBeforeRequest;
            FiddlerApplication.BeforeResponse += OnBeforeResponse;

            FiddlerCoreStartupSettings startupSettings = new FiddlerCoreStartupSettingsBuilder()
                .ListenOnPort(9999)
                .DecryptSSL()
                .RegisterAsSystemProxy()
                .Build();

            Console.WriteLine("[+] Starting fiddler application");
            FiddlerApplication.Startup(startupSettings);

            Backend.Listen();
            Console.WriteLine("[+] Listening to backend");

            Console.WriteLine("[+] Launch Fortnite from Epic Games Launcher");

            if (SetConsoleCtrlHandler(HandlerRoutine, true))
            {
                Thread.Sleep(Timeout.Infinite);
            }
        }
    }

    private static bool HandlerRoutine(CtrlType ctrlType)
    {
        if (ctrlType == CtrlType.CTRL_CLOSE_EVENT || ctrlType == CtrlType.CTRL_LOGOFF_EVENT || ctrlType == CtrlType.CTRL_SHUTDOWN_EVENT)
        {
            Console.WriteLine("[+] Shutting down fiddler application");
            FiddlerApplication.Shutdown();
            Thread.Sleep(1000);
        }

        return true;
    }

    private static void OnBeforeRequest(Session session)
    {
        if (session.RequestHeaders["User-Agent"].Split('/')[0] == "Fortnite")
        {
            if (session.PathAndQuery.StartsWith("/lightswitch/api/service/") || session.PathAndQuery.StartsWith("/fortnite/api/game/v2/profile/") || session.PathAndQuery.StartsWith("/api/locker/v4/"))
            {
                session.fullUrl = "http://localhost:1911" + session.PathAndQuery;
            }
        }
    }

    private static void OnBeforeResponse(Session session)
    {
        // リンゴはミカンです
    }
}
