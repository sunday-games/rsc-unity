using System;
// ReSharper disable InconsistentNaming

namespace SG
{
    public record Environment
    {
        private static readonly Environment Local = new() { ApiUrl = "http://localhost:8080", WsUrl = "ws://localhost:8080" };
        private static readonly Environment LocalDocker = new() { ApiUrl = "http://host.docker.internal:8080", WsUrl = "ws://host.docker.internal:8080" };
        private static readonly Environment Dev = new() { ApiUrl = "https://supernova-server-dev.sunday.games", WsUrl = "wss://supernova-server-dev.sunday.games" };
        private static readonly Environment QA = new() { ApiUrl = "https://supernova-server-qa.sunday.games", WsUrl = "wss://supernova-server-qa.sunday.games" };
        private static readonly Environment Prod = new() { ApiUrl = "https://supernova-server.sunday.games", WsUrl = "wss://supernova-server.sunday.games" };

        public string ApiUrl;
        public string WsUrl;

        public enum Name
        {
            Prod,
            Dev,
            QA,
            Local,
            LocalDocker
        }

        public static Environment Get(Name env) => env switch
        {
            Name.Local => Local,
            Name.LocalDocker => LocalDocker,
            Name.Dev => Dev,
            Name.QA => QA,
            Name.Prod => Prod,
            _ => throw new ArgumentOutOfRangeException(nameof(env), env, "Unsupported environment")
        };
    }
}