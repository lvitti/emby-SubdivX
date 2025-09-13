using System;
using System.IO;
using System.Text;
using Moq;

using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.IO;

namespace SubdivX.Test
{
    public static class TestHostFactory
    {
        public static (IApplicationHost AppHost,
                       ILogManager LogManager,
                       IJsonSerializer Json,
                       IFileSystem FileSystem,
                       string TempRoot) BuildAppHost()
        {
            var root  = Path.Combine(Path.GetTempPath(), "emby-tests", Guid.NewGuid().ToString("N"));
            var cfg   = Path.Combine(root, "config");
            var plug  = Path.Combine(root, "plugins");
            var cache = Path.Combine(root, "cache");
            var logs  = Path.Combine(root, "logs");

            Directory.CreateDirectory(cfg);
            Directory.CreateDirectory(plug);
            Directory.CreateDirectory(cache);
            Directory.CreateDirectory(logs);

            var paths = new Mock<IApplicationPaths>(MockBehavior.Loose);
            paths.SetupGet(p => p.ProgramDataPath).Returns(root);
            paths.SetupGet(p => p.PluginConfigurationsPath).Returns(cfg);
            paths.SetupGet(p => p.PluginsPath).Returns(plug);
            paths.SetupGet(p => p.CachePath).Returns(cache);
            paths.SetupGet(p => p.LogDirectoryPath).Returns(logs);

            // ---------------------- ILogManager / ILogger ----------------------
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<object[]>()));
            logger.Setup(l => l.Warn(It.IsAny<string>(), It.IsAny<object[]>()));
            logger.Setup(l => l.Error(It.IsAny<string>(), It.IsAny<object[]>()));
            logger.Setup(l => l.Debug(It.IsAny<string>(), It.IsAny<object[]>()));

            var logMgr = new Mock<ILogManager>(MockBehavior.Loose);
            logMgr.Setup(m => m.GetLogger(It.IsAny<string>())).Returns(logger.Object);

            // ---------------------- IJsonSerializer ----------------------
            var json = new Mock<IJsonSerializer>(MockBehavior.Loose);
            
            var fakeJsonSerializer = new JsonSerializer();
            json
                .Setup(x => x.SerializeToString(It.IsAny<It.IsAnyType>()))
                .Returns((object o) => fakeJsonSerializer.SerializeToString(o));
            json
                .Setup(j => j.DeserializeFromString(It.IsAny<string>(), It.IsAny<Type>()))
                .Returns((string s, Type t) => fakeJsonSerializer.DeserializeFromString(s, t));

            // ---------------------- IFileSystem ----------------------
            var fs = new Mock<IFileSystem>(MockBehavior.Loose);
            fs.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            fs.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
            fs.Setup(f => f.OpenRead(It.IsAny<string>()))
              .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("test")));

            // ---------------------- IApplicationHost (STRICT) ----------------------
            var appHost = new Mock<IApplicationHost>(MockBehavior.Strict)
            {
                DefaultValue = DefaultValue.Mock
            };

            appHost.SetupGet(h => h.ApplicationVersion).Returns(new Version(1, 0, 0));

            appHost.Setup(h => h.Resolve<IApplicationPaths>()).Returns(paths.Object);
            appHost.Setup(h => h.Resolve<ILogManager>()).Returns(logMgr.Object);
            appHost.Setup(h => h.Resolve<IJsonSerializer>()).Returns(json.Object);
            appHost.Setup(h => h.Resolve<IFileSystem>()).Returns(fs.Object);

            return (appHost.Object, logMgr.Object, fakeJsonSerializer, fs.Object, root);
        }

        public static void Cleanup(string tempRoot)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tempRoot) && Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, recursive: true);
            }
            catch { /* Ignore on tests */ }
        }
    }
}
