using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    [SerializeField] private Text Text;
    [SerializeField] private int ConsoleLogCount;

    private List<string> ConsoleLogGroup = new List<string>();

    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLog;
    }

    private void OnLog(string Log, string Stack, LogType Type)
    {
        if (ConsoleLogGroup.Count + 1 > ConsoleLogCount)
            ConsoleLogGroup.Remove(ConsoleLogGroup[0]);
        
        ConsoleLogGroup.Add(Log);

        Text.text = "";

        foreach (string LogMessage in ConsoleLogGroup)
        {
            Text.text = Text.text + LogMessage + "\n";
        }
    }

    private void OnDisable()
    {
        Application.logMessageReceivedThreaded -= OnLog;
    }
}
