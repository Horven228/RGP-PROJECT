using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Простой Service Locator для доступа к глобальным сервисам
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>
    /// Регистрирует сервис в контейнере
    /// </summary>
    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"[ServiceLocator] Сервис {type.Name} уже зарегистрирован. Перезаписываем.");
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
        }
    }

    /// <summary>
    /// Получает сервис из контейнера
    /// </summary>
    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            return service as T;
        }

        Debug.LogError($"[ServiceLocator] Сервис {type.Name} не найден!");
        return null;
    }

    /// <summary>
    /// Проверяет, зарегистрирован ли сервис
    /// </summary>
    public static bool Has<T>() where T : class
    {
        return _services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Очищает все сервисы (используется для тестирования)
    /// </summary>
    public static void Clear()
    {
        _services.Clear();
    }
}