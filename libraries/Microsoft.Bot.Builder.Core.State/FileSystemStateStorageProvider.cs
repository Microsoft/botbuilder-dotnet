﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Core.State
{
    public class FileSystemStateStorageProvider : IStateStorageProvider
    {
        private static readonly JsonSerializer StateStorageEntrySerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            // TODO: dial in all settings
        });

        private readonly string _basePath;

        public FileSystemStateStorageProvider(string basePath)
        {
            _basePath = basePath;
        }

        public IStateStorageEntry CreateNewEntry(string stateNamespace, string key) => new FileSystemStateStorageEntry(stateNamespace, key, GetStateStorageEntryFilePath(_basePath, stateNamespace, key));

        public Task Delete(string stateNamespace)
        {
            Directory.Delete(ConvertStateNamespaceToDirectoryPath(_basePath, stateNamespace));

            return Task.CompletedTask;
        }

        public Task Delete(string stateNamespace, IEnumerable<string> keys)
        {
            var stateNamespaceDirectoryInfo = GetStateNamespaceDirectoryInfo(stateNamespace);

            if (stateNamespaceDirectoryInfo.Exists)
            {
                var distinctKeys = new HashSet<string>();

                foreach (var key in keys)
                {
                    distinctKeys.Add(ConvertKeyToFileName(key));
                }

                foreach (var stateEntryFile in stateNamespaceDirectoryInfo.EnumerateFiles())
                {
                    if (distinctKeys.Contains(stateEntryFile.Name))
                    {
                        stateEntryFile.Delete();
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace)
        {
            var stateNamespaceDirectoryInfo = GetStateNamespaceDirectoryInfo(stateNamespace);

            if (!stateNamespaceDirectoryInfo.Exists)
            {
                return Enumerable.Empty<IStateStorageEntry>();
            }

            var loadTasks = new List<Task<IStateStorageEntry>>();

            foreach (var stateEntryFile in stateNamespaceDirectoryInfo.EnumerateFiles())
            {
                var stateEntryKey = Path.GetFileNameWithoutExtension(stateEntryFile.Name);

                loadTasks.Add(Task.Run(() => LoadFileSystemStateStorageEntry(stateNamespace, stateEntryKey, stateEntryFile)));
            }

            await Task.WhenAll(loadTasks);

            var results = new List<IStateStorageEntry>(loadTasks.Count);

            foreach (var loadTask in loadTasks)
            {
                results.Add(loadTask.Result);
            }

            return results;
        }

        public Task<IStateStorageEntry> Load(string stateNamespace, string key)
        {
            var storageEntryFileInfo = GetStateStorageEntryFileInfo(_basePath, stateNamespace, key);

            return LoadFileSystemStateStorageEntry(stateNamespace, key, storageEntryFileInfo);
        }

        public async Task<IEnumerable<IStateStorageEntry>> Load(string stateNamespace, IEnumerable<string> keys)
        {
            var loadTasks = new List<Task<IStateStorageEntry>>();

            foreach (var key in keys)
            {
                loadTasks.Add(Load(stateNamespace, key));
            }

            await Task.WhenAll(loadTasks);

            var results = new List<IStateStorageEntry>(loadTasks.Count);

            foreach (var loadTask in loadTasks)
            {
                results.Add(loadTask.Result);
            }

            return results;
        }

        public Task Save(IEnumerable<IStateStorageEntry> stateStorageEntries)
        {
            var saveTasks = new List<Task>();

            foreach (var stateStorageEntry in stateStorageEntries)
            {
                if (!(stateStorageEntry is FileSystemStateStorageEntry fileSystemStateStorageEntry))
                {
                    throw new InvalidOperationException($"Only instances of {nameof(FileSystemStateStorageEntry)} are supported by {nameof(FileSystemStateStorageProvider)}.");
                }

                saveTasks.Add(fileSystemStateStorageEntry.WriteToFile());
            }

            return Task.WhenAll(saveTasks);
        }

        private DirectoryInfo GetStateNamespaceDirectoryInfo(string stateNamespace) => new DirectoryInfo(ConvertStateNamespaceToDirectoryPath(_basePath, stateNamespace));

        private static FileInfo GetStateStorageEntryFileInfo(string basePath, string stateNamespace, string key) => new FileInfo(GetStateStorageEntryFilePath(basePath, stateNamespace, key));

        private static string GetStateStorageEntryFilePath(string basePath, string stateNamespace, string key) => Path.Combine(ConvertStateNamespaceToDirectoryPath(basePath, stateNamespace), ConvertKeyToFileName(key));

        public static string ConvertStateNamespaceToDirectoryPath(string basePath, string stateNamespace)
        {
            if (string.IsNullOrEmpty(stateNamespace))
            {
                throw new ArgumentException("Expected a non-null/empty value.", nameof(stateNamespace));
            }
            // Strip any leading path separator so that it always becomes relative to basePath
            if (stateNamespace[0] == Path.DirectorySeparatorChar || stateNamespace[0] == Path.AltDirectorySeparatorChar)
            {
                stateNamespace = stateNamespace.Substring(1);
            }

            foreach (var invalidPathChar in Path.GetInvalidPathChars())
            {
                stateNamespace = stateNamespace.Replace(invalidPathChar, '`');
            }

            return Path.Combine(basePath, stateNamespace);
        }

        public static string ConvertKeyToFileName(string key)
        {
            foreach (var invalidPathChar in Path.GetInvalidFileNameChars())
            {
                key = key.Replace(invalidPathChar, '@');
            }

            key += ".json";

            return key;
        }

        private async Task<IStateStorageEntry> LoadFileSystemStateStorageEntry(string stateNamespace, string key, FileInfo storageEntryFileInfo)
        {
            try
            {
                using (var fileStream = new FileStream(storageEntryFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192, options: FileOptions.Asynchronous | FileOptions.SequentialScan))
                using (var textReader = new StreamReader(fileStream, Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    var data = (JObject)(await JObject.ReadFromAsync(jsonReader));

                    return new FileSystemStateStorageEntry(stateNamespace, key, storageEntryFileInfo.FullName, data);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // TODO: log
                return default(IStateStorageEntry);
            }
            catch (FileNotFoundException)
            {
                // TODO: log
                return default(IStateStorageEntry);
            }
        }

        private sealed class FileSystemStateStorageEntry : DeferredValueStateStorageEntry
        {
            private JObject _data;

            public FileSystemStateStorageEntry(string stateNamespace, string key, string filePath) : base(stateNamespace, key)
            {
                FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            }

            public FileSystemStateStorageEntry(string stateNamespace, string key, string filePath, JObject data) : this(stateNamespace, key, filePath)
            {
                _data = data ?? throw new ArgumentNullException(nameof(data));

                ETag = data.Value<JObject>("@@botFramework").Value<string>("eTag");
            }

            public string FilePath { get; }

            public async Task WriteToFile()
            {
                tryAgain:
                try
                {
                    using (var fileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 8192, options: FileOptions.Asynchronous | FileOptions.RandomAccess))
                    {
                        if (fileStream.Length > 0)
                        {
                            await EnusureETagsMatch(fileStream);
                        }

                        fileStream.Position = 0;

                        _data = RawValue != null ? JObject.FromObject(RawValue) : new JObject();

                        _data["@@botFramework"] = new JObject()
                        {
                            ["rawNamespace"] = Namespace,
                            ["rawKey"] = Key,
                            ["eTag"] = Guid.NewGuid().ToString("N")
                        };

                        using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8))
                        using (var jsonWriter = new JsonTextWriter(textWriter))
                        {
                            await _data.WriteToAsync(jsonWriter);
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

                    goto tryAgain;
                }
            }

            private async Task EnusureETagsMatch(FileStream fileStream)
            {
                /* {
                 *  "@@botFramework"
                 *  {
                 *     "eTag": "value"
                 *  }
                 * }
                 */

                using (var textReader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    // Read state StartObject
                    if (!(await jsonReader.ReadAsync())
                            ||
                        jsonReader.TokenType != JsonToken.StartObject)
                    {
                        return;
                    }

                    while (await jsonReader.ReadAsync())
                    {
                        // Look for the bot framework metadata property
                        if (jsonReader.TokenType == JsonToken.PropertyName
                                &&
                            (string)jsonReader.Value == "@@botFramework")
                        {
                            // Read bot framework framework metadata StartObject
                            if (!(await jsonReader.ReadAsync())
                                    ||
                                 jsonReader.TokenType != JsonToken.StartObject)
                            {
                                return;
                            }

                            while (await jsonReader.ReadAsync())
                            {
                                // Look for the eTag property
                                if (jsonReader.TokenType == JsonToken.PropertyName
                                        &&
                                    (string)jsonReader.Value == "eTag")
                                {
                                    var fileETag = await jsonReader.ReadAsStringAsync();

                                    // If the ETags don't match, then we've run into an optimistic concurrency violation
                                    if (ETag != fileETag)
                                    {
                                        throw new StateOptimisticConcurrencyViolationException($"An optimistic concurrency violation occurred when trying to save new state for: PartitionKey={Namespace};Key={Key}. The original ETag value was {ETag}, but the current ETag value is {fileETag}.");;
                                    }

                                    return;
                                }

                                await jsonReader.SkipAsync();
                            }
                        }

                        await jsonReader.SkipAsync();
                    }
                }
            }

            protected override T MaterializeValue<T>()
            {
                if (_data == null)
                {
                    return default(T);
                }

                return _data.ToObject<T>();
            }
        }
    }
}
