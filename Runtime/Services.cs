using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZellyDevGames.Monoservice
{
    public static class Registry
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static bool Register(Type type, object instance)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!type.IsInstanceOfType(instance))
                throw new InvalidOperationException(
                    $"Cannot register instance of {instance.GetType().FullName} as {type.FullName}"
                );

            if (!Services.TryAdd(type, instance))
            {
                Debug.LogWarning($"Service already registered: {type.FullName}");
                return false;
            }

            return true;
        }

        public static T Get<T>()
        {
            var type = typeof(T);

            if (!Services.TryGetValue(type, out var service))
                throw new InvalidOperationException($"Service not registered: {type.FullName}");

            return (T)service;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Services.Clear();
            RegisterAttributeServices();

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void RegisterAttributeServices()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                    continue;

                var attr = type.GetCustomAttribute<ServiceAttribute>();

                if (attr == null) continue;

                try
                {
                    var instance = Activator.CreateInstance(type);
                    Register(type, instance);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            var rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                var monobehaviors = root.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var mb in monobehaviors)
                {
                    if (!mb) continue;
                    var type = mb.GetType();
                    if (Services.ContainsKey(type)) continue;

                    var serviceAttribute = Attribute.GetCustomAttribute(
                        type,
                        typeof(ServiceAttribute), false) as ServiceAttribute;
                    if (serviceAttribute == null) continue;
                    Register(type, mb);
                }
            }
        }
    }
}