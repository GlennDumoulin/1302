using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class BaseDataPersistanceManager : MonoBehaviour
{
    public abstract void NewFile();
    public abstract void LoadFile();
    public abstract void LoadResource(string resourcePath);
    public abstract void SaveFile();
}

public class DataPersistanceManager<TData> : BaseDataPersistanceManager where TData : new()
{
    [Header("File Storage Config")]
    [SerializeField] private string _fileName = string.Empty;

    protected TData _data = default;
    protected bool _usePresetData = false;

    private List<IDataPersistance<TData>> _dataPersistenceObjects = new List<IDataPersistance<TData>>();
    private FileDataHandler<TData> _fileDataHandler = null;

    private void Awake()
    {
        _fileDataHandler = new FileDataHandler<TData>(Application.persistentDataPath);
    }

    private void Start()
    {
        _dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance<TData>>().ToList();
    }

    public override void NewFile()
    {
        _data = new TData();
    }

    public override void LoadFile()
    {
        if (_fileDataHandler != null)
        {
            // Load data from saved file
            _data = _fileDataHandler.Load(_fileName);
        }

        // If there is no data to load, create a new game
        if (_data == null)
        {
            NewFile();
        }

        // Get all objects that implement the interface
        _dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance<TData>>().ToList();

        // Push loaded data to all scripts that need it
        foreach (IDataPersistance<TData> dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(_data);
        }
    }

    public override void LoadResource(string resourcePath)
    {
        if (_fileDataHandler != null)
        {
            _data = _fileDataHandler.LoadResource(resourcePath);
        }

        if (_data == null)
        {
            NewFile();
        }

        // Get all objects that implement the interface
        _dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance<TData>>().ToList();

        // Push loaded data to all scripts that need it
        foreach (IDataPersistance<TData> dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(_data);
        }
    }

    public override void SaveFile()
    {
        if (_data == null)
        {
            NewFile();
        }

        if (!_usePresetData)
        {
            // Get all objects that implement the interface
            _dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance<TData>>().ToList();

            // Pass data to other scripts
            foreach (IDataPersistance<TData> dataPersistenceObj in _dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(ref _data);
            }
        }

        if (_fileDataHandler != null)
        {
            // Save data to file
            _fileDataHandler.Save(_data, _fileName);
        }
    }
}
