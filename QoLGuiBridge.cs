using System;
using UnityEngine;

namespace QoLCaptainOfIndustry;

public sealed class QoLGuiBridge : IDisposable
{
    private readonly GameObject m_gameObject;

    private QoLGuiBridge(GameObject gameObject)
    {
        m_gameObject = gameObject;
    }

    public static QoLGuiBridge Install(QoLCaptainOfIndustryCommands commands)
    {
        var existing = UnityEngine.Object.FindObjectOfType<QoLGuiBehaviour>();
        if (existing != null)
        {
            existing.Initialize(commands);
            return new QoLGuiBridge(existing.gameObject);
        }

        var gameObject = new GameObject("QoLCaptainOfIndustry.Gui");
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        UnityEngine.Object.DontDestroyOnLoad(gameObject);

        var behaviour = gameObject.AddComponent<QoLGuiBehaviour>();
        behaviour.Initialize(commands);

        return new QoLGuiBridge(gameObject);
    }

    public void Dispose()
    {
        if (m_gameObject != null)
        {
            UnityEngine.Object.Destroy(m_gameObject);
        }
    }
}
