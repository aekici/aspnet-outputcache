/* 
 One note: the connection is thread safe and intended to be massively shared; don't do a connection per operation ( from Marc Gravell, author of protobuf-net )
 * 
 * https://gist.github.com/jalaziz/6769315
 */

using System;

using BookSleeve;

namespace AspNet.Caching.Output.Connection
{
    public class RedisConnectionManager : IDisposable
    {
        private volatile RedisConnection connection;
        private readonly object connectionLock = new object();

        public string Host { get; set; }
        public int Port { get; set; }
        public int IOTimeout { get; set; }
        public string Password { get; set; }
        public int MaxUnsent { get; set; }
        public bool AllowAdmin { get; set; }
        public int SyncTimeout { get; set; }

        public RedisConnectionManager(string host, int port = 6379, int ioTimeout = -1, string password = null, int maxUnsent = 2147483647, bool allowAdmin = false, int syncTimeout = 10000)
        {
            Host = host;
            Port = port;
            IOTimeout = ioTimeout;
            Password = password;
            MaxUnsent = maxUnsent;
            AllowAdmin = allowAdmin;
            SyncTimeout = syncTimeout;
        }

        public RedisConnection GetConnection(bool waitOnOpen = false)
        {
            var redisConnection = this.connection;

            if (redisConnection == null)
            {
                lock (connectionLock)
                {
                    if (this.connection == null)
                    {
                        this.connection = new RedisConnection(Host, Port, IOTimeout, Password, MaxUnsent, AllowAdmin, SyncTimeout);
                        this.connection.Shutdown += ConnectionOnShutdown;
                        var openTask = this.connection.Open();

                        if (waitOnOpen) { this.connection.Wait(openTask); }
                    }

                    redisConnection = this.connection;
                }
            }

            return redisConnection;
        }

        private void ConnectionOnShutdown(object sender, ErrorEventArgs errorEventArgs)
        {
            lock (connectionLock)
            {
                connection.Shutdown -= ConnectionOnShutdown;
                connection = null;
            }
        }

        public void Reset(bool abort = false)
        {
            lock (connectionLock)
            {
                if (connection != null)
                {
                    connection.Close(abort);
                    connection = null;
                }
            }
        }

        public void Dispose()
        {
            lock (connectionLock)
            {
                if (connection == null)
                    return;
                
                connection.Dispose();
                connection = null;
            }
        }
    }
}
