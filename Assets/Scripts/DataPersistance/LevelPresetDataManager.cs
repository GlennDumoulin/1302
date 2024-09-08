using UnityEngine;

public class LevelPresetDataManager : DataPersistanceManager<LevelPresetData>
{
    [Header("Preset Data")]
    [SerializeField] private bool _shouldUsePresetData = false;
    [SerializeField] private LevelPresetData _presetData = null;

    public override void LoadFile()
    {
        base.LoadFile();

        _presetData = _data;
    }

    public override void LoadResource(string resourcePath)
    {
        base.LoadResource(resourcePath);

        _presetData = _data;
    }

    public override void SaveFile()
    {
        if (_shouldUsePresetData)
        {
            _data = _presetData;
            _usePresetData = _shouldUsePresetData;
        }
        
        base.SaveFile();
    }
}
