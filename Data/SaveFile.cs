using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotUtilities.GameData;
using GodotUtilities.Logic;
using GodotUtilities.Serialization;
using HexGeneral.Game.Client;
using HexGeneral.Game.Generators;
using HexGeneral.Game.Logic;
using MessagePack;

namespace HexGeneral.Game;

public class SaveFile
{
    public List<Entity> Entities { get; private set; }
    public HostLogicData HostLogicData { get; private set; }
    public Guid PlayerGuid { get; private set; }
    public SaveFile(List<Entity> entities,
        HostLogicData hostLogicData, 
        Guid playerGuid)
    {
        Entities = entities;
        HostLogicData = hostLogicData;
        PlayerGuid = playerGuid;
    }
    public static SaveFile Save(string path, 
        string name,
        HexGeneralClient client)
    {
        var entities = client.Data.Entities.EntitiesById.Values.ToList();
        var logicData = client.Logic is HexGeneralHostLogic h
            ? h.HostLogicData
            : null;
        foreach (var entity in entities)
        {
            try
            {
                var b1 = client.Data.Serializer.Serialize(entity);

            }
            catch
            {
                throw new Exception($"couldnt serialize {entity.GetType().Name}");
            }

        }
        
        
        
        var b2 = client.Data.Serializer.Serialize(logicData);
        var b3 = client.Data.Serializer.Serialize(client.PlayerGuid);
        
        var saveFile = new SaveFile(entities,
            logicData,
            client.PlayerGuid);
        GodotFileExt.SaveFile(saveFile, path, name, ".sv", client.Data.Serializer);

        return saveFile;
    }
    
    public static (HexGeneralData, ILogic, Guid) Load(string dir, string file)
    {
        var path = dir.PathJoin(file);
        using var fileAccess = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var bytes = fileAccess.GetBuffer((long)fileAccess.GetLength());
        var gameSettings = new GameSettings();
        var data = new HexGeneralData(gameSettings);
        var saveFile = data.Serializer.Deserialize<SaveFile>(bytes);
        foreach (var e in saveFile.Entities)
        {
            data.Entities.EntitiesById.Add(e.Id, e);
            if (e is ISingletonEntity)
            {
                data.SetSingleton(e);
            }
        }

        ILogic logic = saveFile.HostLogicData is null
            ? new RemoteLogic()
            : new HexGeneralHostLogic(data, saveFile.PlayerGuid);
        GodotUtilities.GameData.Data.SetupForLoad(data, 
            
            new HexGeneralModelImporter(data.ModelPredefs));
        return (data, logic, saveFile.PlayerGuid);
    }
    
}