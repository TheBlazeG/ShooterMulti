using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UiManager : MonoBehaviour
{
    public const string lineBreak = "<br>";
    public const string bulletPoint = "\u2022";

    public static UiManager singleton;

    public TextMeshProUGUI killText;


    private void Awake() => singleton = this;

    public void ShowKills(string info )
    {
        
        killText.text = info ;
    }
}
