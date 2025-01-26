using UnityEngine;

[CreateAssetMenu(fileName = "Agent", menuName = "Scriptable Objects/New Agent")]
public class AgentData : ScriptableObject
{
    public string characterName;

    public Texture2D characterSpriteSheet;
}
