﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Implements an Azure Table based storage provider for a bot.
    /// </summary>
    public class AzureTableStorage : IStorage
    {
        private static readonly char[] IllegalKeyCharacters = new char[] { '\\', '?', '/', '#', '\t', '\n', '\r' };
        private static Lazy<Dictionary<char, string>> IllegalKeyCharacterReplacementMap = new Lazy<Dictionary<char, string>>(() => IllegalKeyCharacters.ToDictionary(c => c, c => '%' + ((int)c).ToString("x2")));

        private readonly CloudStorageAccount _storageAccount;
        private readonly string _tableName;
        private CloudTable _table;

        /// <summary>
        /// Creates a new instance of the storage provider.
        /// </summary>
        /// <param name="dataConnectionString">The Azure Storage Connection string.</param>
        /// <param name="tableName">Name of the table to use for storage.
        /// See https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#table-names
        /// for rules about naming tables.</param>
        public AzureTableStorage(string dataConnectionString, string tableName)
            : this(CloudStorageAccount.Parse(dataConnectionString), tableName)
        {
        }

        /// <summary>
        /// Creates a new instance of the storage provider.
        /// </summary>
        /// <param name="storageAccount">CloudStorageAccount information.</param>
        /// <param name="tableName">Name of the table to use for storage.
        /// See https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model#table-names
        /// for rules about naming tables.</param>
        public AzureTableStorage(CloudStorageAccount storageAccount, string tableName)
        {
            _storageAccount = storageAccount ?? throw new ArgumentNullException(nameof(storageAccount));

            // Checks if table name is valid
            NameValidator.ValidateTableName(tableName);

            _tableName = tableName;
        }

        /// <summary>
        /// Removes store items from storage.
        /// </summary>
        /// <param name="keys">Array of item keys to remove from the store.</param>
        public async Task Delete(string[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var table = await GetTable().ConfigureAwait(false);

            try
            {
                await Task.WhenAll(
                    keys.Select(k => new EntityKey(k))
                        .Select(ek => table.ExecuteAsync(TableOperation.Delete(new TableEntity(ek.PartitionKey, ek.RowKey) { ETag = "*" }))))
                            .ConfigureAwait(false);
            }
            catch (StorageException ex)
                when ((HttpStatusCode)ex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
            }
            catch (AggregateException ex)
                when (ex.InnerException is StorageException iex
                && (HttpStatusCode)iex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
            }
        }

        /// <summary>
        /// Loads store items from storage.
        /// </summary>
        /// <param name="keys">Array of item keys to read from the store.</param>
        public async Task<IDictionary<string, object>> Read(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentException("Please provide at least one key to read from storage.", nameof(keys));
            }

            var table = await GetTable().ConfigureAwait(false);

            var readTasks = keys.Select(async key =>
            {
                var ek = new EntityKey(key);
                var tableEntity = await table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(ek.PartitionKey, ek.RowKey)).ConfigureAwait(false);

                if (tableEntity.HttpStatusCode == (int)HttpStatusCode.OK)
                {
                    // re-create expected object
                    var value = ReadObject(tableEntity);
                    return new KeyValuePair<string, object>(key, value);
                }

                return new KeyValuePair<string, object>();
            });

            // Wait for all the reads to complete
            var completedTasks = await Task.WhenAll(readTasks).ConfigureAwait(false);

            // Filter out the Nulls and convert to a dictionary
            var dict = completedTasks.Where(kv => kv.Key != null)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 

            return dict; 
        }

        /// <summary>
        /// Saves store items to storage.
        /// </summary>
        /// <param name="changes">Map of items to write to storage.</param>
        public async Task Write(IDictionary<string, object> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            // Re-create objects as table entities
            var tableEntities = changes.Select(kv => new KeyValuePair<string, DynamicTableEntity>(kv.Key, ToTableEntity(kv.Key, kv.Value)));
            var bogusEtagKeys = tableEntities.Where(item => item.Value.ETag != null && item.Value.ETag.Length == 0);
            if (bogusEtagKeys.Any())
            {
                throw new ArgumentException("Invalid ETag values detected for items with the following keys: " + string.Join(", ", bogusEtagKeys.Select(o => o.Key)));
            }

            var table = await GetTable().ConfigureAwait(false);

            var writeTasks = tableEntities.Select(o => o.Value)
                .Select(tableEntity =>
                {
                    if (tableEntity.ETag == null || tableEntity.ETag == "*")
                    {
                        // New item or etag=* then insert or replace unconditionaly
                        return table.ExecuteAsync(TableOperation.InsertOrReplace(tableEntity));
                    }


                    // Optimistic Update
                    return table.ExecuteAsync(TableOperation.Replace(tableEntity));
                });

            await Task.WhenAll(writeTasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Ensure table is created.
        /// </summary>
        private ValueTask<CloudTable> GetTable()
        {
            if (_table != null)
            {
                return new ValueTask<CloudTable>(_table);
            }

            return new ValueTask<CloudTable>(EnsureTableExists());

            async Task<CloudTable> EnsureTableExists()
            {
                var table = _storageAccount.CreateCloudTableClient().GetTableReference(_tableName);

                // This call may not be thread-safe and multiple calls may be made, but Azure will handle it correctly and there is no destructive side effect.
                await table.CreateIfNotExistsAsync();

                _table = table;

                return _table;
            }
        }

        private static object ReadObject(TableResult tableEntity)
        {
            // Create instance of proper type
            var dynamicTableEntity = (DynamicTableEntity)tableEntity.Result;
            var properties = dynamicTableEntity.Properties;
            var type = Type.GetType(properties["__type"].StringValue);

            var value = Activator.CreateInstance(type);
            TableEntity.ReadUserObject(value, properties, new OperationContext());

            // IStoreItem? apply Etag
            if (value is IStoreItem iStoreItem)
            {
                iStoreItem.eTag = tableEntity.Etag;
            }

            return value;
        }

        private static DynamicTableEntity ToTableEntity(string key, object value)
        {
            // Flatten properties
            var properties = EntityPropertyConverter.Flatten(value, new OperationContext());

            // Add Type information
            var type = value.GetType();
            var typeQualifiedName = type.AssemblyQualifiedName;
            properties.Add("__type", EntityProperty.GeneratePropertyForString(typeQualifiedName));

            // Etag and PK/RK
            var etag = (value as IStoreItem)?.eTag;
            var ek = new EntityKey(key);
            return new DynamicTableEntity(ek.PartitionKey, ek.RowKey)
            {
                ETag = etag,
                Properties = properties
            };
        }

        /// <summary>
        /// Entity that maps property to PartitionKey and RowKey
        /// </summary>
        private struct EntityKey
        {
            public string PartitionKey { get; private set; }
            public string RowKey { get; private set; }

            public EntityKey(string propertyKey)
            {
                PartitionKey = SanitizeKey(propertyKey);
                RowKey = string.Empty;
            }

            public EntityKey(string partitionKey, string rowKey)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
            }

            /// <summary>
            /// Escapes a property key into a PartitionKey that can be used with Azure Tables.
            /// More information at https://docs.microsoft.com/en-us/rest/api/storageservices/Understanding-the-Table-Service-Data-Model?redirectedfrom=MSDN#table-names
            /// </summary>
            /// <param name="key">The Property Key</param>
            /// <returns>Sanitized key that can be used as PartitionKey</returns>
            public static string SanitizeKey(string key)
            {
                var firstIllegalCharIndex = key.IndexOfAny(IllegalKeyCharacters);

                // If there are no illegal characters return immediately and avoid any further processing/allocations
                if (firstIllegalCharIndex == -1) return key;

                // Allocate a builder that assumes that all remaining characters might be replaced to avoid any extra allocations
                var sanitizedKeyBuilder = new StringBuilder(key.Length + (key.Length - firstIllegalCharIndex + 1) * 3);

                // Add all good characters up to the first bad character to the builder first
                for (int index = 0; index < firstIllegalCharIndex; index++)
                {
                    sanitizedKeyBuilder.Append(key[index]);
                }

                var illegalCharacterReplacementMap = IllegalKeyCharacterReplacementMap.Value;

                // Now walk the remaining characters, starting at the first known bad character, replacing any bad ones with their designated replacement value from the map
                for (int index = firstIllegalCharIndex; index < key.Length; index++)
                {
                    var ch = key[index];

                    // Check if this next character is considered illegal and, if so, append its replacement; otherwise just append the good character as is
                    if (illegalCharacterReplacementMap.TryGetValue(ch, out var replacement))
                    {
                        sanitizedKeyBuilder.Append(replacement);
                    }
                    else
                    {
                        sanitizedKeyBuilder.Append(ch);
                    }
                }

                return sanitizedKeyBuilder.ToString();
            }
        }
    }
}
