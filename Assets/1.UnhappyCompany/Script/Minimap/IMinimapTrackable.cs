using UnityEngine;

public interface IMinimapTrackable
{
    GameObject CCTVIconPrefab { get; }
    void OnMinimapAdd();
    void OnMinimapRemove();
} 