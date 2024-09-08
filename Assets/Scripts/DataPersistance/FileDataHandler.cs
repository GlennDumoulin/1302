using UnityEngine;
using System;
using System.IO;

public class FileDataHandler<TData>
{
    private string _dataDirPath = string.Empty;
    private const string _dataFileExtension = ".json";

    public FileDataHandler(string dataDirPath)
    {
        _dataDirPath = dataDirPath;
    }

    public TData Load(string fileName)
    {
        if (fileName == string.Empty) fileName = "saveFile";
        fileName += _dataFileExtension;

        string fullPath = Path.Combine(_dataDirPath, fileName);

        TData loadedData = default;

        // Make sure the file we try to load exists
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = string.Empty;

                // Read serialized data from file
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize JSON to specified type
                loadedData = JsonUtility.FromJson<TData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred when trying to load data from file: {fullPath}\n{e}");
                throw;
            }
        }

        return loadedData;
    }

    public TData LoadResource(string resourcePath)
    {
        TData loadedData = default;

        TextAsset resource = Resources.Load<TextAsset>(resourcePath);
        if (resource != null)
        {
            string dataToLoad = resource.text;

            loadedData = JsonUtility.FromJson<TData>(dataToLoad);
        }
        else
        {
            Debug.LogError($"Error occurred when trying to load data from resource: {resourcePath}");
        }

        return loadedData;
    }

    public void Save(TData data, string fileName)
    {
        if (fileName == string.Empty) fileName = "saveFile";
        fileName += _dataFileExtension;

        string fullPath = Path.Combine(_dataDirPath, fileName);

        try
        {
            // Make sure the directory for the save files exists
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize data to JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            // Write serialized data to file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error occurred when trying to save data to file: {fullPath}\n{e}");
            throw;
        }
    }
}
