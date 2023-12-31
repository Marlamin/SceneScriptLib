using MoonSharp.Interpreter;
using System.Numerics;

namespace SceneScriptLib
{
    public static class SceneScriptReader
    {
        public static bool DebugOutput = false;

        public static TimelineScene ParseTimelineScript(string sceneScript)
        {
            System.Globalization.CultureInfo greatestCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            greatestCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = greatestCulture;

            var timelineScene = new TimelineScene();

            var prepend = @"
                function wid(id)
                    return id
                end

                function cdiid(id)
                    return id
                end

                function fid(id)
                    return id
                end

                function cid(id)
                    return id
                end

                function gdi(id)
                    return id
                end

                function iid(id)
                    return id
                end

                function SceneTimelineAddFileData(name, table)
                    return table 
                end
            ";

            var script = new Script();

            try
            {
                var val = script.DoString(prepend + "\n return " + sceneScript);

                // Don't crash on doc scenes
                if (val.Type == DataType.Void)
                    return timelineScene;

                var table = val.Table;
                foreach (var key in table.Keys)
                {
                    if (key.String == "actors")
                    {
                        timelineScene.actors = ParseActors(table.Get(key).ToObject<Table>());
                    }
                    else
                    {
                        throw new Exception("Unhandled root key " + key.String);
                    }
                }
            }
            catch (ScriptRuntimeException ex)
            {
                Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
            }

            return timelineScene;
        }

        static Dictionary<string, Actor> ParseActors(Table table)
        {
            var actors = new Dictionary<string, Actor>();
            foreach (var actorName in table.Keys)
            {
                var actor = new Actor();

                //Console.WriteLine("New actor: " + actorName);

                var subTable = table.Get(actorName).ToObject<Table>();
                if (subTable.Keys.Count() != 1 || subTable.Keys.First().String != "properties")
                    throw new Exception("Actor table has unexpected number of keys (" + subTable.Keys.Count() + "), or the key is not 'properties' (got " + subTable.Keys.First().String + ")");

                actor.properties = new ActorProperties();

                var propertiesTable = subTable.Get("properties").ToObject<Table>();
                foreach (var propertyKey in propertiesTable.Keys)
                {
                    var propertyTable = propertiesTable.Get(propertyKey).ToObject<Table>();
                    if (propertyTable.Keys.Count() != 1 || propertyTable.Keys.First().String != "events")
                        throw new Exception("Property table has unexpected number of keys (" + propertyTable.Keys.Count() + "), or the key is not 'events' (got " + propertyTable.Keys.First().String + ")");

                    switch (propertyKey.String)
                    {
                        case "Appearance":
                            actor.properties.Appearance = ParseAppearanceProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "CustomScript":
                            actor.properties.CustomScript = ParseCustomScriptProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "EquipWeapon":
                            actor.properties.EquipWeapon = ParseEquipWeaponProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "Fade":
                            actor.properties.Fade = ParseFadeProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "FadeRegion":
                            actor.properties.FadeRegion = ParseFadeRegionProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "GroundSnap":
                            actor.properties.GroundSnap = ParseGroundSnapProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "MoveSpline":
                            actor.properties.MoveSpline = ParseMoveSplineProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "Music":
                            actor.properties.Music = ParseMusicProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "Scale":
                            actor.properties.Scale = ParseScaleProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "Sheathe":
                            actor.properties.Sheathe = ParseSheatheProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        case "Transform":
                            actor.properties.Transform = ParseTransformProperty(propertyTable.Get("events").ToObject<Table>());
                            break;
                        default:
                            if (!DebugOutput)
                                break;

                            Console.WriteLine("Unhandled property: " + propertyKey.String);
                            ParseEvents(propertyTable.Get("events").ToObject<Table>());
                            break;
                    }
                }

                actors.Add(actorName.ToString().Replace("\"", ""), actor);
            }

            return actors;
        }

        static MusicProperty ParseMusicProperty(Table table)
        {
            var musicProperty = new MusicProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    musicProperty.events.Add(eventTime, new MusicEvent { soundKitID = int.Parse(subTable["soundKitID"].ToString()) });
                }
            }

            return musicProperty;
        }
        static GroundSnapProperty ParseGroundSnapProperty(Table table)
        {
            var groundSnapProperty = new GroundSnapProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    groundSnapProperty.events.Add(eventTime, new GroundSnapEvent { snap = bool.Parse(subTable["snap"].ToString()) });
                }
            }

            return groundSnapProperty;
        }
        static TransformProperty ParseTransformProperty(Table table)
        {
            var property = new TransformProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime, ParseTransform(((Table)subTable["transform"])));
                }
            }

            return property;
        }
        static AppearanceProperty ParseAppearanceProperty(Table table)
        {
            var property = new AppearanceProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var appearanceEvent = new AppearanceEvent();
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    foreach (var propertyKey in subTable.Keys)
                    {
                        var value = subTable.Get(propertyKey);
                        switch (propertyKey.String)
                        {
                            case "creatureID":
                                appearanceEvent.creatureID = new CreatureID { ID = int.Parse(value.ToString().Replace("cid(", "").Replace(")", "")) };
                                break;
                            case "creatureDisplaySetIndex":
                                appearanceEvent.creatureDisplaySetIndex = int.Parse(value.ToString());
                                break;
                            case "creatureDisplayInfoID":
                                appearanceEvent.creatureDisplayInfoID = int.Parse(value.ToString());
                                break;
                            case "fileDataID":
                                appearanceEvent.fileDataID = new FileDataID { ID = int.Parse(value.ToString().Replace("fid(", "").Replace(")", "")) };
                                break;
                            case "wmoGameObjectDisplayID":
                                appearanceEvent.wmoGameObjectDisplayID = new GameObjectDisplayInfoID { ID = int.Parse(value.ToString().Replace("gdi(", "").Replace(")", "")) };
                                break;
                            case "itemID":
                                appearanceEvent.itemID = new ItemID { ID = int.Parse(value.ToString().Replace("iid(", "").Replace(")", "")) };
                                break;
                            case "isPlayerClone":
                                appearanceEvent.isPlayerClone = bool.Parse(value.ToString());
                                break;
                            case "isPlayerCloneNative":
                                appearanceEvent.isPlayerCloneNative = bool.Parse(value.ToString());
                                break;
                            case "playerSummon":
                                appearanceEvent.playerSummon = bool.Parse(value.ToString());
                                break;
                            case "playerGroupIndex":
                                appearanceEvent.playerGroupIndex = int.Parse(value.ToString());
                                break;
                            case "smoothPhase":
                                appearanceEvent.smoothPhase = bool.Parse(value.ToString());
                                break;
                            default:
                                throw new Exception("!!! Unhandled property: " + propertyKey.String);
                        }
                    }

                    property.events.Add(eventTime, appearanceEvent);
                }
            }

            return property;
        }
        static MoveSplineProperty ParseMoveSplineProperty(Table table)
        {
            var property = new MoveSplineProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    if (eventKey.Type == DataType.Table)
                    {
                        var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                        foreach (var propertyKey in subTable.Keys)
                        {
                            var value = subTable.Get(propertyKey);
                            switch (propertyKey.String)
                            {
                                case "overrideSpeed":
                                    property.overrideSpeed = float.Parse(value.ToString());
                                    break;
                                case "useModelRunSpeed":
                                    property.useModelRunSpeed = bool.Parse(value.ToString());
                                    break;
                                case "useModelWalkSpeed":
                                    property.useModelWalkSpeed = bool.Parse(value.ToString());
                                    break;
                                case "yawUsesSplineTangent":
                                    property.yawUsesSplineTangent = bool.Parse(value.ToString());
                                    break;
                                case "yawUsesNodeTransform":
                                    property.yawUsesNodeTransform = bool.Parse(value.ToString());
                                    break;
                                case "yawBlendDisabled":
                                    property.yawBlendDisabled = bool.Parse(value.ToString());
                                    break;
                                case "pitchUsesSplineTangent":
                                    property.pitchUsesSplineTangent = bool.Parse(value.ToString());
                                    break;
                                case "pitchUsesNodeTransform":
                                    property.pitchUsesNodeTransform = bool.Parse(value.ToString());
                                    break;
                                case "rollUsesNodeTransform":
                                    property.rollUsesNodeTransform = bool.Parse(value.ToString());
                                    break;
                                default:
                                    throw new Exception("!!! Unhandled property: " + propertyKey.String);
                            }
                        }
                    }
                    else if (eventKey.Type == DataType.Number)
                    {
                        var eventTime = (float)eventKey.Number;
                        var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                        property.events.Add(eventTime, ParseTransform(((Table)subTable["position"])));
                    }
                }
            }

            return property;
        }
        static CustomScriptProperty ParseCustomScriptProperty(Table table)
        {
            var property = new CustomScriptProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime, new CustomScriptEvent { script = subTable["script"].ToString() });
                }
            }

            return property;
        }
        static ScaleProperty ParseScaleProperty(Table table)
        {
            var property = new ScaleProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime, new ScaleEvent { scale = float.Parse(subTable["scale"].ToString()), duration = float.Parse(subTable["duration"].ToString()) });
                }
            }

            return property;
        }
        static FadeProperty ParseFadeProperty(Table table)
        {
            var property = new FadeProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime, new FadeEvent { alpha = float.Parse(subTable["alpha"].ToString()), time = float.Parse(subTable["time"].ToString()) });
                }
            }

            return property;
        }
        static FadeRegionProperty ParseFadeRegionProperty(Table table)
        {
            var property = new FadeRegionProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime, 
                        new FadeRegionEvent { 
                            enabled = bool.Parse(subTable["enabled"].ToString()), 
                            radius = float.Parse(subTable["radius"].ToString()),
                            includePlayer = bool.Parse(subTable["includePlayer"].ToString()),
                            excludePlayers = bool.Parse(subTable["excludePlayers"].ToString()),
                            excludeNonPlayers = bool.Parse(subTable["excludeNonPlayers"].ToString()),
                            includeSounds = bool.Parse(subTable["includeSounds"].ToString()),
                            includeWMOs = bool.Parse(subTable["includeWMOs"].ToString())
                        }
                    );
                }
            }

            return property;
        }

        static SheatheProperty ParseSheatheProperty(Table table)
        {
            var property = new SheatheProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime,
                        new SheatheEvent
                        {
                            isSheathed = bool.Parse(subTable["isSheathed"].ToString()),
                            isRanged = bool.Parse(subTable["isRanged"].ToString()),
                            animated = bool.Parse(subTable["animated"].ToString()),
                        }
                    );
                }
            }

            return property;
        }
        static EquipWeaponProperty ParseEquipWeaponProperty(Table table)
        {
            var property = new EquipWeaponProperty { events = [] };

            foreach (var key in table.Keys)
            {
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var eventTime = (float)eventKey.Number;
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    property.events.Add(eventTime,
                        new EquipWeaponEvent
                        {
                            itemID = int.Parse(subTable["itemID"].ToString()),
                            MainHand = bool.Parse(subTable["MainHand"].ToString()),
                            OffHand = bool.Parse(subTable["OffHand"].ToString()),
                            Ranged = bool.Parse(subTable["Ranged"].ToString()),
                        }
                    );
                }
            }

            return property;
        }

        static void ParseEvents(Table table)
        {
            foreach (var key in table.Keys)
            {
                Console.WriteLine("\t \t Event: " + key);
                foreach (var eventKey in table.Get(key).ToObject<Table>().Keys)
                {
                    var subTable = table.Get(key).ToObject<Table>().Get(eventKey).ToObject<Table>();
                    Console.WriteLine("\t \t \t " + eventKey);
                    foreach (var propertyKey in subTable.Keys)
                    {
                        var value = subTable.Get(propertyKey);
                        object formattedValue = null;
                        switch (value.Type)
                        {
                            case DataType.String:
                                formattedValue = value.String;
                                break;
                            case DataType.Boolean:
                                formattedValue = value.Boolean;
                                break;
                            case DataType.Number:
                                formattedValue = value.Number;
                                break;
                            case DataType.Table:
                                switch (propertyKey.String)
                                {
                                    case "transform":
                                    case "position": // This is almost always the same as transform as it has another "position" inside
                                        var tf = ParseTransform(value.Table);
                                        formattedValue = "XYZ: " + tf.Position.ToString() + ", yaw: " + tf.Yaw + ", pitch: " + tf.Pitch + ", roll: " + tf.Roll;
                                        break;
                                    case "offset":
                                        var pos = ParsePosition(value.Table);
                                        formattedValue = pos.X + " " + pos.Y + " " + pos.Z;
                                        break;
                                    default:
                                        throw new Exception("!!! Unhandled table type: " + propertyKey.String);
                                }
                                break;
                            default:
                                throw new Exception("!!! Unhandled property type: " + value.Type);
                        }

                        Console.WriteLine("\t \t \t \t " + propertyKey.String + " = " + formattedValue.ToString());
                    }
                }
            }
        }

        static TransformEvent ParseTransform(Table table)
        {
            var transform = new TransformEvent();

            if (table.Keys.Count() != 4 || table.Keys.First().String != "position")
                throw new Exception("Property table has unexpected number of keys (" + table.Keys.Count() + "), or the 1st key is not 'position' (got " + table.Keys.First().String + ")");

            transform.Position = ParsePosition(table.Get("position").ToObject<Table>());
            transform.Yaw = (float)table.Get("yaw").Number;
            transform.Pitch = (float)table.Get("pitch").Number;
            transform.Roll = (float)table.Get("roll").Number;

            return transform;
        }

        static Vector3 ParsePosition(Table table)
        {
            var position = new Vector3();

            if (table.Keys.Count() != 3 || table.Keys.First().String != "x")
                throw new Exception("Property table has unexpected number of keys (" + table.Keys.Count() + "), or the 1st key is not 'x' (got " + table.Keys.First().String + ")");

            position.X = (float)table.Get("x").Number;
            position.Y = (float)table.Get("y").Number;
            position.Z = (float)table.Get("z").Number;
            return position;
        }
    }
}
