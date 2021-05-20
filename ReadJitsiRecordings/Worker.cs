using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReadJitsiRecordings.service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadJitsiRecordings
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _folderWatcher;
        private readonly string _inputFolder;
        private readonly IStorageService _storageService;

        public Worker(ILogger<Worker> logger, IStorageService storageService)
        {
            _storageService = storageService;
            _logger = logger;
            _inputFolder = @"/config/recordings";
            //_inputFolder = @"c:/temp";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    string[] dirs = Directory.GetDirectories(_inputFolder, "*", SearchOption.TopDirectoryOnly);
                    Console.WriteLine("The number of directories starting with * is {0}.", dirs.Length);
                    foreach (string dir in dirs)
                    {
                        Console.WriteLine(dir);
                        await Upload(dir);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                } 

                await Task.Delay(1000, stoppingToken);
            }
            await Task.CompletedTask;
        }

        private async Task Upload(string dir)
        {
            _logger.LogInformation($"InBound Change Event Triggered by [{dir}]");
             

            string text = File.ReadAllText(dir + "/metadata.json", Encoding.UTF8);
            var segment = text.Split(',');
            var section = segment[0].Split(':');
            var first = section[2];
            var name = first.Split('/');
            var _name = name[3].Replace("\"", "");

            _logger.LogInformation($"metadata text by [{text}]"); 
            string[] filePaths = Directory.GetFiles(dir, "*.mp4");
            _logger.LogInformation($"filePaths[0] text by [{filePaths[0]}]");
            await _storageService.UploadFileAsync(dir, filePaths[0], _name);

            // do some work 
            _logger.LogInformation("Directory Delete Change Event");
            _logger.LogInformation("Done Directory Delete Change Event");

            _logger.LogInformation("Done with Inbound Change Event");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Starting");
            if (!Directory.Exists(_inputFolder))
            {
                _logger.LogWarning($"Please make sure the InputFolder [{_inputFolder}] exists, then restart the service.");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Binding Events from Input Folder: {_inputFolder}");
            _folderWatcher = new FileSystemWatcher(_inputFolder, "*.*")
            {
                NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size
        };

            _folderWatcher.IncludeSubdirectories = true;
            _folderWatcher.Created += Input_OnChanged; 
            _folderWatcher.EnableRaisingEvents = true;
            _folderWatcher.Filter = "*.json";
            _folderWatcher.IncludeSubdirectories = true;

            return base.StartAsync(cancellationToken);
        }

        protected void Input_OnChanged(object source, FileSystemEventArgs e)
        { 
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    _logger.LogInformation($"InBound Change Event Triggered by [{e.FullPath}]");
                    var directory = e.FullPath.Replace("metadata.json", "");
                    string[] filePaths = Directory.GetFiles(directory, "*.mp4");
 
                    string text = File.ReadAllText(e.FullPath, Encoding.UTF8);
                    var segment = text.Split(',');
                    var section = segment[0].Split(':');
                    var first = section[2];
                    var name = first.Split('/');
                    var _name = name[3].Replace("\"", "");

                    _logger.LogInformation($"metadata text by [{text}]");
                    //_storageService.UploadFileAsync(directory, filePaths[0], _name);

                    // do some work 
                    _logger.LogInformation("Directory Delete Change Event");
                    _logger.LogInformation("Done Directory Delete Change Event");

                    _logger.LogInformation("Done with Inbound Change Event");
                } 
        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Service");
            _folderWatcher.EnableRaisingEvents = false;
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _folderWatcher.Dispose();
            base.Dispose();
        }
    }
}
