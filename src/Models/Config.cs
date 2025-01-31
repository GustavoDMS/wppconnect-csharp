﻿namespace WPPConnect.Models
{
    public class Config
    {
        public Enum.Browser Browser { get; set; } = Enum.Browser.Chromium;

        public string? BrowserWsUrl { get; set; }

        public string DeviceName { get; set; } = "WPPConnect";
        
        public bool Devtools { get; set; } = true;

        public bool Headless { get; set; } = true;

        public bool LogQrCode { get; set; } = true;

        public bool SessionsStart { get; set; } = true;

        public string SessionsFolderName { get; set; } = "sessions";

        public Enum.LibVersion Version { get; set; } = Enum.LibVersion.Latest;
    }
}