﻿using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using RestSharp;
using System.Dynamic;

namespace WPPConnect
{
    public class WPPConnection
    {
        #region Properties

        public Models.Config Config { get; internal set; }

        private static List<Models.Client> _Clients = new List<Models.Client>();

        #endregion

        #region Events

        //Auth - Authenticated
        public delegate void OnAuthAuthenticatedEventHandler(Models.Client client, Models.Token token);

        public event OnAuthAuthenticatedEventHandler OnAuthAuthenticated;

        private async Task BrowserPage_OnAuthAuthenticated(string sessionName)
        {
            Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

            Models.Token token = await client.Token();

            Console.WriteLine($"[{client.SessionName}:client] Authenticated");

            if (this.OnAuthAuthenticated != null)
                OnAuthAuthenticated(client, token);
        }

        //Auth - CodeChange
        public delegate void OnAuthCodeChangeEventHandler(Models.Client client, string token);

        public event OnAuthCodeChangeEventHandler OnAuthCodeChange;

        private async Task BrowserPage_OnAuthCodeChange(string sessionName, dynamic token)
        {
            if (this.OnAuthCodeChange != null && token != null)
            {
                Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

                string fullCode = token.fullCode;

                OnAuthCodeChange(client, fullCode);
            }
        }

        //Auth - Logout
        public delegate void OnAuthLogoutEventHandler(Models.Client client);

        public event OnAuthLogoutEventHandler OnAuthLogout;

        private async Task BrowserPage_OnAuthLogout(string sessionName)
        {
            Models.Client client = await Client(sessionName);

            BrowserClose(client);

            SessionRemove(client);

            if (this.OnAuthLogout != null)
            {
                OnAuthLogout(client);
            }
        }

        //Auth - Require
        public delegate void OnAuthRequireEventHandler(Models.Client client, string token);

        public event OnAuthRequireEventHandler OnAuthRequire;

        private async Task BrowserPage_OnAuthRequire(string sessionName, dynamic token)
        {
            if (this.OnAuthRequire != null && token != null)
            {
                Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

                string fullCode = token.fullCode;

                OnAuthRequire(client, fullCode);
            }
        }

        //Chat - OnMessageReceived
        public delegate void OnMessageReceivedEventHandler(Models.Client client, Models.Message message);

        public event OnMessageReceivedEventHandler OnMessageReceived;

        private async Task BrowserPage_OnMessageReceived(string sessionName, ExpandoObject message)
        {
            if (this.OnMessageReceived != null)
            {
                Models.Client client = _Clients.Single(c => c.SessionName == sessionName);

                dynamic response = message;

                Models.Message messageObj = new Models.Message()
                {
                    Id = response.id.id,
                    Content = response.body,
                    Number = response.from.Substring(0, response.from.IndexOf('@'))
                };

                OnMessageReceived(client, messageObj);
            }
        }

        #endregion

        #region Constructors

        public WPPConnection()
        {
            new WPPConnection(new Models.Config());
        }

        public WPPConnection(Models.Config config)
        {
            Config = config;

            Start();

            CheckVersion();

            SessionStart();
        }

        #endregion

        #region Methods - Private

        private void Start()
        {
            Console.WriteLine(@" _       ______  ____  ______                            __ ");
            Console.WriteLine(@"| |     / / __ \/ __ \/ ____/___  ____  ____  ___  _____/ /_");
            Console.WriteLine(@"| | /| / / /_/ / /_/ / /   / __ \/ __ \/ __ \/ _ \/ ___/ __/");
            Console.WriteLine(@"| |/ |/ / ____/ ____/ /___/ /_/ / / / / / / /  __/ /__/ /_  ");
            Console.WriteLine(@"|__/|__/_/   /_/    \____/\____/_/ /_/_/ /_/\___/\___/\__/  ");
            Console.WriteLine();
        }

        private void CheckVersion()
        {
            try
            {
                string versionUrl;

                if (Config.Version == Models.Enum.LibVersion.Latest)
                    versionUrl = "https://api.github.com/repos/wppconnect-team/wa-js/releases/latest";
                else
                    versionUrl = "https://api.github.com/repos/wppconnect-team/wa-js/releases/tags/nightly";

                RestClient client = new RestClient(versionUrl);

                RestRequest request = new RestRequest();
                request.Timeout = 5000;

                RestResponse response = client.GetAsync(request).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                {
                    JObject json = JObject.Parse(response.Content);

                    string version = (string)json["name"];

                    Console.WriteLine($"[wa-js : {version}]");
                }
                else
                    throw new Exception("[wa-js version:não foi possível obter a versão]");

                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void BrowserClose(Models.Client client)
        {
            client.Connection.BrowserContext.PagesAsync().Result.FirstOrDefault().CloseAsync();
            client.Connection.BrowserContext.CloseAsync();

            Console.WriteLine($"[{client.SessionName}:browser] Closed");

            _Clients.Remove(client);

            Console.WriteLine($"[{client.SessionName}:client] Closed");
        }

        private void SessionStart()
        {
            if (Config.SessionStart)
            {
                Console.WriteLine($"[wa-js : Sessions Starting]");
                Console.WriteLine();

                string directory = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionFolderName}";

                Directory.CreateDirectory(directory);

                List<string> listSessionFolders = Directory.GetDirectories(directory).ToList();

                foreach (string sessionFolderPath in listSessionFolders)
                {
                    DirectoryInfo folder = new DirectoryInfo(sessionFolderPath);

                    SessionCreate(folder.Name, true).Wait();
                }

                Console.WriteLine($"[wa-js : Sessions Started]");
                Console.WriteLine();
            }
        }

        private void SessionRemove(Models.Client client)
        {
            if (Config.TokenStore == Models.Enum.TokenStore.File)
                Directory.Delete($"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionFolderName}\\{client.SessionName}", true);
        }

        #endregion

        #region Methods - Public

        public async Task SessionCreate(string sessionName, bool token = false)
        {
            try
            {
                Models.Client client = _Clients.SingleOrDefault(i => i.SessionName == sessionName);

                if (client == null)
                {
                    Console.WriteLine($"[{sessionName}:browser] Initializing");

                    if (!string.IsNullOrEmpty(Config.BrowserWsUrl))
                    {
                        ConnectOptions connectOptions = new ConnectOptions
                        {
                            BrowserWSEndpoint = Config.BrowserWsUrl
                        };

                        Browser browser = await Puppeteer.ConnectAsync(connectOptions);

                        client = new Models.Client(sessionName, browser);
                    }
                    else
                    {
                        await new BrowserFetcher().DownloadAsync();

                        string[] args = new string[]
                                {
                                  "--enable-gpu",
                                  "--display-entrypoints",
                                  "--disable-http-cache",
                                  "--no-sandbox",
                                  "--disable-setuid-sandbox",
                                  "--disable-2d-canvas-clip-aa",
                                  "--disable-2d-canvas-image-chromium",
                                  "--disable-3d-apis",
                                  "--disable-accelerated-2d-canvas",
                                  "--disable-accelerated-jpeg-decoding",
                                  "--disable-accelerated-mjpeg-decode",
                                  "--disable-accelerated-video-decode",
                                  "--disable-app-list-dismiss-on-blur",
                                  "--disable-audio-output",
                                  "--disable-background-timer-throttling",
                                  "--disable-backgrounding-occluded-windows",
                                  "--disable-breakpad",
                                  "--disable-canvas-aa",
                                  "--disable-client-side-phishing-detection",
                                  "--disable-component-extensions-with-background-pages",
                                  "--disable-composited-antialiasing",
                                  "--disable-default-apps",
                                  "--disable-dev-shm-usage",
                                  "--disable-extensions",
                                  "--disable-features=TranslateUI,BlinkGenPropertyTrees",
                                  "--disable-field-trial-config",
                                  "--disable-fine-grained-time-zone-detection",
                                  "--disable-geolocation",
                                  "--disable-gl-extensions",
                                  "--disable-gpu",
                                  "--disable-gpu-early-init",
                                  "--disable-gpu-sandbox",
                                  "--disable-gpu-watchdog",
                                  "--disable-histogram-customizer",
                                  "--disable-in-process-stack-traces",
                                  "--disable-infobars",
                                  "--disable-ipc-flooding-protection",
                                  "--disable-notifications",
                                  "--disable-popup-blocking",
                                  "--disable-renderer-backgrounding",
                                  "--disable-session-crashed-bubble",
                                  "--disable-setuid-sandbox",
                                  "--disable-site-isolation-trials",
                                  "--disable-software-rasterizer",
                                  "--disable-sync",
                                  "--disable-threaded-animation",
                                  "--disable-threaded-scrolling",
                                  "--disable-translate",
                                  "--disable-webgl",
                                  "--disable-webgl2",
                                  "--enable-features=NetworkService",
                                  "--force-color-profile=srgb",
                                  "--hide-scrollbars",
                                  "--ignore-certifcate-errors",
                                  "--ignore-certifcate-errors-spki-list",
                                  "--ignore-certificate-errors",
                                  "--ignore-certificate-errors-spki-list",
                                  "--ignore-gpu-blacklist",
                                  "--ignore-ssl-errors",
                                  "--log-level=3",
                                  "--metrics-recording-only",
                                  "--mute-audio",
                                  "--no-crash-upload",
                                  "--no-default-browser-check",
                                  "--no-experiments",
                                  "--no-first-run",
                                  "--no-sandbox",
                                  "--no-zygote",
                                  "--renderer-process-limit=1",
                                  "--safebrowsing-disable-auto-update",
                                  "--silent-debugger-extension-api",
                                  "--single-process",
                                  "--unhandled-rejections=strict",
                                  "--window-position=0,0" };

                        LaunchOptions launchOptions = new LaunchOptions
                        {
                            Args = Config.Headless == true ? args : new string[0],
                            Headless = Config.Headless,
                            Devtools = Config.Devtools,
                            DefaultViewport = new ViewPortOptions
                            {
                                Width = 1920,
                                Height = 1080
                            },
                            UserDataDir = $"{AppDomain.CurrentDomain.BaseDirectory}\\{Config.SessionFolderName}\\{sessionName}"
                        };

                        Browser browser = await Puppeteer.LaunchAsync(launchOptions);

                        client = new Models.Client(sessionName, browser);
                    }

                    Console.WriteLine($"[{sessionName}:browser] Initialized");

                    Console.WriteLine($"[{client.SessionName}:client] Initializing");

                    await client.Connection.BrowserPage.SetUserAgentAsync("WhatsApp/2.2043.8 Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");

                    await client.Connection.BrowserPage.SetBypassCSPAsync(true);

                    client.Connection.BrowserPage.DOMContentLoaded += BrowserPage_DOMContentLoaded;

                    await client.Connection.BrowserPage.GoToAsync("https://web.whatsapp.com");

                    #region Inject


                    #endregion

                    bool mainLoaded = await client.Connection.BrowserPage.EvaluateFunctionAsync<bool>("async => WPP.conn.isMainLoaded()");

                    if (!mainLoaded && token)
                    {
                        Console.WriteLine($"[{client.SessionName}:client] Authentication Failed");

                        BrowserClose(client);

                        SessionRemove(client);

                        return;
                    }

                    #region Events

                    //Auth - Require
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, object, Task>("browserPage_OnAuthRequire", BrowserPage_OnAuthRequire);


                    //Auth - Authenticated
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, Task>("browserPage_OnAuthAuthenticated", BrowserPage_OnAuthAuthenticated);
                    await client.Connection.BrowserPage.EvaluateFunctionAsync("async => WPP.conn.on('authenticated', function(e) { browserPage_OnAuthAuthenticated('" + client.SessionName + "') })");

                    //Auth - CodeChange
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, object, Task>("browserPage_OnAuthCodeChange", BrowserPage_OnAuthCodeChange);
                    await client.Connection.BrowserPage.EvaluateFunctionAsync("async => WPP.conn.on('auth_code_change', function(e) { browserPage_OnAuthCodeChange('" + client.SessionName + "', e) })");

                    //Auth - Logout
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, Task>("browserPage_OnAuthLogout", BrowserPage_OnAuthLogout);
                    await client.Connection.BrowserPage.EvaluateFunctionAsync("async => WPP.conn.on('logout', function() { browserPage_OnAuthLogout('" + client.SessionName + "') })");

                    //Chat - OnMessageReceived
                    await client.Connection.BrowserPage.ExposeFunctionAsync<string, ExpandoObject, Task>("browserPage_OnMessageReceived", BrowserPage_OnMessageReceived);
                    await client.Connection.BrowserPage.EvaluateFunctionAsync("async => WPP.whatsapp.MsgStore.on('change', function(e) { browserPage_OnMessageReceived('" + client.SessionName + "', e) })");

                    #endregion

                    _Clients.Add(client);
                }
                else
                    throw new Exception($"Já existe uma session com o nome {sessionName}");

                Models.Session session = await client.QrCode();

                if (session.Status == Models.Enum.Status.QrCode)
                {
                    Console.WriteLine($"[{sessionName}:client] Authentication Required");

                    dynamic qrCodeJson = new JObject();
                    qrCodeJson.fullCode = session.Mensagem;

                    BrowserPage_OnAuthCodeChange(client.SessionName, qrCodeJson);
                }

                Console.WriteLine($"[{sessionName}:client] Initialized");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void BrowserPage_DOMContentLoaded(object? sender, EventArgs e)
        {
            Page page = (Page)sender;

            page.AddScriptTagAsync("https://github.com/wppconnect-team/wa-js/releases/download/nightly/wppconnect-wa.js");

            page.EvaluateFunctionAsync("async => WPP.conn.on('require_auth', function(e) { console.log('require_auth') })");
        }

        public async Task SessionRemove(string sessionName)
        {
            Models.Client client = await Client(sessionName);

            BrowserClose(client);
        }

        public async Task<Models.Client> Client(string sessionName)
        {
            Models.Client client = _Clients.SingleOrDefault(c => c.SessionName == sessionName);

            if (client == null)
                throw new Exception($"Não foi encontrado nenhuma sessão com o nome {sessionName}");
            else
                return client;
        }

        #endregion
    }
}