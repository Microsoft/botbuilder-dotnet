﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// The DialogStateManager manages memory scopes and pathresolvers
    /// MemoryScopes are named root level objects, which can exist either in the dialogcontext or off of turn state
    /// PathResolvers allow for shortcut behavior for mapping things like $foo -> dialog.foo.
    /// </summary>
    public class DialogStateManager : IDictionary<string, object>
    {
        private readonly DialogContext dialogContext;

        public DialogStateManager(DialogContext dc)
        {
            dialogContext = dc ?? throw new ArgumentNullException(nameof(dc));
        }

        /// <summary>
        /// Gets the path resolvers used to evaluate memory paths.
        /// </summary>
        /// <remarks>
        /// The built in path resolvers are $,#,@,@@,%.  Additional ones can be added here to handle path resolvers around additional scopes.
        /// </remarks>
        /// <value>
        /// The path resolvers used to evaluate memory paths.
        /// </value>
        public static List<IPathResolver> PathResolvers { get; } = new List<IPathResolver>
        {
            new DollarPathResolver(),
            new HashPathResolver(),
            new AtAtPathResolver(),
            new AtPathResolver(),
            new PercentPathResolver()
        };

        /// <summary>
        /// Gets the supported memory scopes for the dialog state manager.  
        /// </summary>
        /// <remarks>
        /// components can extend valid scopes by adding to this list, for example to add top level scopes such as Company, Team, etc.
        /// </remarks>
        /// <value>
        /// The supported memory scopes for the dialog state manager.  
        /// </value>
        public static List<MemoryScope> MemoryScopes { get; } = new List<MemoryScope>
        {
            new MemoryScope(ScopePath.USER),
            new MemoryScope(ScopePath.CONVERSATION),
            new MemoryScope(ScopePath.TURN),
            new SettingsMemoryScope(),
            new DialogMemoryScope(),
            new ClassMemoryScope(),
            new ThisMemoryScope()
        };

        public ICollection<string> Keys => MemoryScopes.Select(ms => ms.Name).ToList();

        public ICollection<object> Values => MemoryScopes.Select(ms => ms.GetMemory(dialogContext)).ToList();

        public int Count => MemoryScopes.Count;

        public bool IsReadOnly => true;

        public object this[string key] { get => GetValue<object>(key, () => null); set => SetValue(key, value); }

        /// <summary>
        /// Get MemoryScope by name.
        /// </summary>
        /// <param name="name">Name of scope.</param>
        /// <returns>A memory scope.</returns>
        public static MemoryScope GetMemoryScope(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return MemoryScopes.FirstOrDefault(ms => string.Compare(ms.Name, name, ignoreCase: true) == 0);
        }

        /// <summary>
        /// ResolveMemoryScope will find the MemoryScope for and return the remaining path.
        /// </summary>
        /// <param name="path">Incoming path to resolve to scope and remaining path.</param>
        /// <param name="remainingPath">Remaining subpath in scope.</param>
        /// <returns>The memory scope.</returns>
        public virtual MemoryScope ResolveMemoryScope(string path, out string remainingPath)
        {
            path = path.Trim();
            if (path.StartsWith("{") && path.EndsWith("}"))
            {
                // TODO: Eventually this should use the expression machinery
                // This allows doing something like {$foo} where dialog.foo contains $blah to compute a path.
                path = GetValue<string>(path.Substring(1, path.Length - 2));
            }

            var scope = path;
            var dot = path.IndexOf(".");
            if (dot > 0)
            {
                scope = path.Substring(0, dot);
                var memoryScope = GetMemoryScope(scope);
                if (memoryScope != null)
                {
                    remainingPath = path.Substring(dot + 1);
                    return memoryScope;
                }
            }

            remainingPath = string.Empty;
            return GetMemoryScope(scope) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(path));
        }

        /// <summary>
        /// Transform the path using the registered PathTransformers.
        /// </summary>
        /// <param name="path">Path to transform.</param>
        /// <returns>The transformed path.</returns>
        public virtual string TransformPath(string path)
        {
            foreach (var pathResolver in PathResolvers)
            {
                path = pathResolver.TransformPath(path);
            }

            return path;
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value).
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory.</remarks>
        /// <typeparam name="T">the value type to return.</typeparam>
        /// <param name="path">path expression to use.</param>
        /// <param name="value">Value out parameter.</param>
        /// <returns>True if found, false if not.</returns>
        public bool TryGetValue<T>(string path, out T value)
        {
            value = default;
            path = TransformPath(path ?? throw new ArgumentNullException(nameof(path)));

            var memoryScope = ResolveMemoryScope(path, out var remainingPath);
            var memory = memoryScope.GetMemory(dialogContext);

            // HACK to support .First() retrieval on turn.recognized.entities.foo, replace with Expressions once expression ship
            var iFirst = remainingPath.ToLower().LastIndexOf(".first()");
            if (iFirst >= 0)
            {
                return TryGetFirstNestedValue(ref value, ref remainingPath, memory, iFirst);
            }

            return ObjectPath.TryGetPathValue(memory, remainingPath, out value);
        }

        /// <summary>
        /// Get the value from memory using path expression (NOTE: This always returns clone of value).
        /// </summary>
        /// <remarks>This always returns a CLONE of the memory, any modifications to the result of this will not be affect memory.</remarks>
        /// <typeparam name="T">The value type to return.</typeparam>
        /// <param name="pathExpression">Path expression to use.</param>
        /// <param name="defaultValue">Function to give default value if there is none (OPTIONAL).</param>
        /// <returns>Result or null if the path is not valid.</returns>
        public T GetValue<T>(string pathExpression, Func<T> defaultValue = null)
        {
            if (TryGetValue<T>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out var value))
            {
                return value;
            }

            return defaultValue != null ? defaultValue() : default;
        }

        /// <summary>
        /// Get a int value from memory using a path expression.
        /// </summary>
        /// <param name="pathExpression">Path expression.</param>
        /// <param name="defaultValue">Default value if the value doesn't exist.</param>
        /// <returns>Value or null if path is not valid.</returns>
        public int GetIntValue(string pathExpression, int defaultValue = 0)
        {
            if (TryGetValue<int>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get a bool value from memory using a path expression.
        /// </summary>
        /// <param name="pathExpression">The path expression.</param>
        /// <param name="defaultValue">Default value if the value doesn't exist.</param>
        /// <returns>Bool or null if path is not valid.</returns>
        public bool GetBoolValue(string pathExpression, bool defaultValue = false)
        {
            if (TryGetValue<bool>(pathExpression ?? throw new ArgumentNullException(nameof(pathExpression)), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Set memory to value.
        /// </summary>
        /// <param name="path">Path to memory.</param>
        /// <param name="value">Object to set.</param>
        public void SetValue(string path, object value)
        {
            if (value is Task)
            {
                throw new Exception($"{path} = You can't pass an unresolved Task to SetValue");
            }

            path = TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            var memoryScope = ResolveMemoryScope(path, out var remainingPath);
            if (remainingPath == string.Empty)
            {
                memoryScope.SetMemory(dialogContext, value);
            }
            else
            {
                var memory = memoryScope.GetMemory(dialogContext);
                ObjectPath.SetPathValue(memory, remainingPath, value);
            }
        }

        /// <summary>
        /// Remove property from memory.
        /// </summary>
        /// <param name="path">Path to remove the leaf property.</param>
        public void RemoveValue(string path)
        {
            path = TransformPath(path ?? throw new ArgumentNullException(nameof(path)));
            var memoryScope = ResolveMemoryScope(path, out var remainingPath);
            var memory = memoryScope.GetMemory(dialogContext);
            ObjectPath.RemovePathValue(memory, remainingPath);
        }

        /// <summary>
        /// Gets all memoryscopes suitable for logging.
        /// </summary>
        /// <returns>object which represents all memory scopes.</returns>
        public JObject GetMemorySnapshot()
        {
            var result = new JObject();

            foreach (var scope in MemoryScopes.Where(ms => ms.IsReadOnly == false))
            {
                var memory = scope.GetMemory(dialogContext);
                if (memory != null)
                {
                    result[scope.Name] = JToken.FromObject(memory);
                }
            }

            return result;
        }

        public void Add(string key, object value)
        {
            SetValue(key, value);
        }

        public bool ContainsKey(string key)
        {
            return MemoryScopes.Any(ms => ms.Name.ToLower() == key.ToLower());
        }

        public bool Remove(string key)
        {
            RemoveValue(key);
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            return TryGetValue<object>(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            SetValue(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var ms in MemoryScopes)
            {
                array[arrayIndex++] = new KeyValuePair<string, object>(ms.Name, ms.GetMemory(dialogContext));
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach (var ms in MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms.GetMemory(dialogContext));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var ms in MemoryScopes)
            {
                yield return new KeyValuePair<string, object>(ms.Name, ms.GetMemory(dialogContext));
            }
        }

        private static string GetBadScopeMessage(string path)
        {
            return $"'{path}' does not match memory scopes:{string.Join(",", MemoryScopes.Select(ms => ms.Name))}";
        }

        private bool TryGetFirstNestedValue<T>(ref T value, ref string remainingPath, object memory, int iFirst)
        {
            remainingPath = remainingPath.Substring(0, iFirst);
            if (ObjectPath.TryGetPathValue<JArray>(memory, $"{remainingPath}", out var array))
            {
                if (array != null && array.Count > 0)
                {
                    var first = array[0] as JArray;
                    if (first != null)
                    {
                        if (first.Count > 0)
                        {
                            var second = first[0];
                            value = ObjectPath.MapValueTo<T>(second);
                            return true;
                        }

                        return false;
                    }

                    value = ObjectPath.MapValueTo<T>(array[0]);
                    return true;
                }
            }

            return false;
        }
    }
}
