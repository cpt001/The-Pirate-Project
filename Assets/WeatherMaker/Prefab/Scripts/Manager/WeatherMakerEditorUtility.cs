//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

#endif

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// A range of integers
    /// </summary>
    [System.Serializable]
    public struct RangeOfIntegers
    {
        /// <summary>Minimum value (inclusive)</summary>
        [Tooltip("Minimum value (inclusive)")]
        public int Minimum;

        /// <summary>Maximum value (inclusive)</summary>
        [Tooltip("Maximum value (inclusive)")]
        public int Maximum;

        /// <summary>
        /// Generate a random number
        /// </summary>
        /// <returns>Random value</returns>
        public int Random() { return Random(DigitalRuby.WeatherMaker.WeatherMakerRandomizer.Unity); }

        /// <summary>
        /// Generate a random number
        /// </summary>
        /// <param name="randomizer">Randomizer</param>
        /// <returns>Random value</returns>
        public int Random(IRandomizer randomizer) { return randomizer.Random(Minimum, Maximum); }

        /// <summary>
        /// Generate a random number using a specific random instance
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random value</returns>
        public int Random(System.Random r) { return r.Next(Minimum, Maximum + 1); }

        /// <summary>
        /// Convert min and max to Vector2
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 ToVector2() { return new Vector2(Minimum, Maximum); }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return "Min: " + Minimum + ", Max: " + Maximum;
        }
    }

    /// <summary>
    /// Represents a range of floats
    /// </summary>
    [System.Serializable]
    public struct RangeOfFloats
    {
        private float? lastValue;
        private float? lastMinimum;
        private float? lastMaximum;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        public RangeOfFloats(float min, float max)
        {
            Minimum = min;
            Maximum = max;
            lastValue = lastMinimum = lastMaximum = null;
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return "Min: " + lastMinimum + ", Max: " + lastMaximum + ", Last: " + lastValue;
        }

        /// <summary>
        /// Set the last value
        /// </summary>
        /// <param name="value">Last value</param>
        /// <returns>Last value</returns>
        public float SetLastValue(float value)
        {
            lastMinimum = Minimum;
            lastMaximum = Maximum;
            lastValue = value;
            return value;
        }

        /// <summary>
        /// Set the last value
        /// </summary>
        public float LastValue
        {
            set { SetLastValue(value); }
        }

        /// <summary>
        /// The last value
        /// </summary>
        public float GetLastValue()
        {
            return GetLastValue(DigitalRuby.WeatherMaker.WeatherMakerRandomizer.Unity);
        }

        /// <summary>
        /// The last value
        /// </summary>
        /// <param name="randomizer">The randomizer to use in case last value needs a value</param>
        /// <returns>Last value</returns>
        public float GetLastValue(DigitalRuby.WeatherMaker.IRandomizer randomizer)
        {
            if (lastValue == null || lastMinimum == null || lastMinimum.Value != Minimum || lastMaximum == null || lastMaximum.Value != Maximum)
            {
                lastMinimum = Minimum;
                lastMaximum = Maximum;
                lastValue = Random(randomizer);
            }
            return lastValue.Value;
        }

        /// <summary>Minimum value (inclusive)</summary>
        [Tooltip("Minimum value (inclusive)")]
        public float Minimum;

        /// <summary>Maximum value (inclusive)</summary>
        [Tooltip("Maximum value (inclusive)")]
        public float Maximum;

        /// <summary>
        /// Generate a random value between min and max
        /// </summary>
        /// <returns>Random value</returns>
        public float Random()
        {
            return Random(DigitalRuby.WeatherMaker.WeatherMakerRandomizer.Unity);
        }

        /// <summary>
        /// Generate a random value between min and max
        /// </summary>
        /// <param name="randomizer">Randomizer</param>
        /// <returns>Random value</returns>
        public float Random(DigitalRuby.WeatherMaker.IRandomizer randomizer) { return SetLastValue(randomizer.Random(Minimum, Maximum)); }

        /// <summary>
        /// Generate a random value between min and max using a specific random instance
        /// </summary>
        /// <param name="r">Random</param>
        /// <returns>Random value</returns>
        public float Random(System.Random r) { return SetLastValue(Minimum + ((float)r.NextDouble() * (Maximum - Minimum))); }

        /// <summary>
        /// Convert the min and max to Vector2
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 ToVector2() { return new Vector2(Minimum, Maximum); }
    }

    /// <summary>
    /// Single line attribute
    /// </summary>
    public class SingleLineAttribute : PropertyAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tooltip">Tooltip</param>
        public SingleLineAttribute(string tooltip) { Tooltip = tooltip; }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; private set; }
    }

    /// <summary>
    /// Single line attribute with clamping
    /// </summary>
    public class SingleLineClampAttribute : SingleLineAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tooltip">Tooltip</param>
        /// <param name="minValue">Min value</param>
        /// <param name="maxValue">Max value</param>
        public SingleLineClampAttribute(string tooltip, float minValue, float maxValue) : base(tooltip)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        /// <summary>
        /// Min value
        /// </summary>
        public float MinValue { get; private set; }

        /// <summary>
        /// Max value
        /// </summary>
        public float MaxValue { get; private set; }
    }

    /// <summary>
    /// Helper methods when serializing / deserializing fields with dynamic editor scripts
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Header byte for floats
        /// </summary>
        public const byte HeaderFloat = 0;

        /// <summary>
        /// Header byte for ints
        /// </summary>
        public const byte HeaderInt = 1;

        /// <summary>
        /// Header byte for shorts
        /// </summary>
        public const byte HeaderShort = 2;

        /// <summary>
        /// Header byte for bytes
        /// </summary>
        public const byte HeaderByte = 3;

        /// <summary>
        /// Header byte for colors
        /// </summary>
        public const byte HeaderColor = 4;

        /// <summary>
        /// Header byte for Vector2s
        /// </summary>
        public const byte HeaderVector2 = 5;

        /// <summary>
        /// Header byte for Vector3s
        /// </summary>
        public const byte HeaderVector3 = 6;

        /// <summary>
        /// Header byte for Vector4s
        /// </summary>
        public const byte HeaderVector4 = 7;

        /// <summary>
        /// Header byte for Quaternions
        /// </summary>
        public const byte HeaderQuaternion = 8;

        /// <summary>
        /// Header byte for Enums
        /// </summary>
        public const byte HeaderEnum = 9;

        /// <summary>
        /// Header byte for Bools
        /// </summary>
        public const byte HeaderBool = 10;

        /// <summary>
        /// Header byte for RangeofFloats
        /// </summary>
        public const byte HeaderFloatRange = 11;

        /// <summary>
        /// Header byte for RangeOfInts
        /// </summary>
        public const byte HeaderIntRange = 12;

        /// <summary>
        /// Convert a type to header byte
        /// </summary>
        public static readonly System.Collections.Generic.Dictionary<System.Type, byte> TypesToHeader = new System.Collections.Generic.Dictionary<System.Type, byte>
        {
            { typeof(float), HeaderFloat },
            { typeof(int), HeaderInt },
            { typeof(short), HeaderShort },
            { typeof(byte), HeaderByte },
            { typeof(Color), HeaderColor },
            { typeof(Vector2), HeaderVector2 },
            { typeof(Vector3), HeaderVector3 },
            { typeof(Vector4), HeaderVector4 },
            { typeof(Quaternion), HeaderQuaternion },
            { typeof(System.Enum), HeaderEnum },
            { typeof(bool), HeaderBool },
            { typeof(RangeOfFloats), HeaderFloatRange },
            { typeof(RangeOfIntegers), HeaderIntRange }
        };


        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Serialized bytes</returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms, System.Text.Encoding.UTF8);
            System.Type t = obj.GetType();
            byte header;
            if (!TypesToHeader.TryGetValue(t, out header))
            {

#if NETFX_CORE

                if (!System.Reflection.IntrospectionExtensions.GetTypeInfo(t).IsEnum)

#else

                if (!t.IsEnum)

#endif

                {
                    return null;
                }
                header = HeaderEnum;
            }
            writer.Write(header);
            switch (header)
            {
                case HeaderFloat: { writer.Write((float)obj); break; }
                case HeaderInt: { writer.Write((int)obj); break; }
                case HeaderShort: { writer.Write((short)obj); break; }
                case HeaderByte: { writer.Write((byte)obj); break; }
                case HeaderColor: { Color c = (Color)obj; writer.Write(c.r); writer.Write(c.g); writer.Write(c.b); writer.Write(c.a); break; }
                case HeaderVector2: { Vector2 v = (Vector2)obj; writer.Write(v.x); writer.Write(v.y); break; }
                case HeaderVector3: { Vector3 v = (Vector3)obj; writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); break; }
                case HeaderVector4: { Vector4 v = (Vector4)obj; writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); writer.Write(v.w); break; }
                case HeaderQuaternion: { Quaternion q = (Quaternion)obj; writer.Write(q.x); writer.Write(q.y); writer.Write(q.z); writer.Write(q.w); break; }
                case HeaderBool: { writer.Write((bool)obj); break; }
                case HeaderFloatRange: { RangeOfFloats v = (RangeOfFloats)obj; writer.Write(v.Minimum); writer.Write(v.Maximum); break; }
                case HeaderIntRange: { RangeOfIntegers v = (RangeOfIntegers)obj; writer.Write(v.Minimum); writer.Write(v.Maximum); break; }
                case HeaderEnum: { writer.Write((int)obj); break; }
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="bytes">Bytes to deserialize</param>
        /// <param name="type">Type of object if known</param>
        /// <returns>Deserialized object</returns>
        public static object Deserialize(byte[] bytes, System.Type type = null)
        {
            if (bytes == null || bytes.Length < 2)
            {
                return null;
            }
            MemoryStream ms = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(ms, System.Text.Encoding.UTF8);
            switch (reader.ReadByte())
            {
                case HeaderFloat: return reader.ReadSingle();
                case HeaderInt: return reader.ReadInt32();
                case HeaderShort: return reader.ReadInt16();
                case HeaderByte: return reader.ReadByte();
                case HeaderColor: return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderVector2: return new Vector2(reader.ReadSingle(), reader.ReadSingle());
                case HeaderVector3: return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderVector4: return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderQuaternion: return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                case HeaderBool: return reader.ReadBoolean();
                case HeaderFloatRange: return new RangeOfFloats { Minimum = reader.ReadSingle(), Maximum = reader.ReadSingle() };
                case HeaderIntRange: return new RangeOfIntegers { Minimum = reader.ReadInt32(), Maximum = reader.ReadInt32() };
                case HeaderEnum: return (type == null ? reader.ReadInt32() : System.Enum.ToObject(type, reader.ReadInt32()));
                default: return null;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Extract a 3D texture to a folder of images
        /// </summary>
        /// <param name="tex3D">Texture3D</param>
        /// <param name="folder">Folder to put images in</param>
        public static void Extract3DTexture(Texture3D tex3D, string folder)
        {
            int size = tex3D.width * tex3D.height;
            Color32[] allPixels = tex3D.GetPixels32();
            Color32[] subPixels = new Color32[size];
            System.IO.Directory.CreateDirectory(folder);
            for (int i = 0; i < tex3D.depth; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    System.Array.Copy(allPixels, i * size, subPixels, 0, size);
                    string outputPath;
                    switch (j)
                    {
                        case 0:
                            outputPath = System.IO.Path.Combine(folder, "1");
                            for (int k = 0; k < subPixels.Length; k++)
                            {
                                subPixels[k].r = subPixels[k].g = subPixels[k].b = subPixels[k].r;
                                subPixels[k].a = 255;
                            }
                            break;

                        case 1:
                            outputPath = System.IO.Path.Combine(folder, "2");
                            for (int k = 0; k < subPixels.Length; k++)
                            {
                                subPixels[k].r = subPixels[k].g = subPixels[k].b = subPixels[k].g;
                                subPixels[k].a = 255;
                            }
                            break;

                        case 2:
                            outputPath = System.IO.Path.Combine(folder, "3");
                            for (int k = 0; k < subPixels.Length; k++)
                            {
                                subPixels[k].r = subPixels[k].g = subPixels[k].b = subPixels[k].b;
                                subPixels[k].a = 255;
                            }
                            break;

                        default:
                            outputPath = System.IO.Path.Combine(folder, "4");
                            for (int k = 0; k < subPixels.Length; k++)
                            {
                                subPixels[k].r = subPixels[k].g = subPixels[k].b = subPixels[k].a;
                                subPixels[k].a = 255;
                            }
                            break;
                    }
                    System.IO.Directory.CreateDirectory(outputPath);
                    Texture2D tex = new Texture2D(tex3D.width, tex3D.height, TextureFormat.ARGB32, false, false);
                    tex.SetPixels32(subPixels);
                    byte[] png = tex.EncodeToPNG();
                    GameObject.DestroyImmediate(tex);
                    File.WriteAllBytes(System.IO.Path.Combine(outputPath, "ExtractedImage_" + i.ToString("D4") + ".png"), png);
                }
            }
        }

        /// <summary>
        /// Mark an object as dirty - only works in editor mode
        /// </summary>
        /// <param name="obj">Object to set dirty</param>
        public static void SetDirty(UnityEngine.Object obj)
        {
            if (Application.isPlaying || obj == null)
            {
                return;
            }
            Undo.RecordObject(obj, "WeatherMaker Change");
            EditorUtility.SetDirty(obj);
            if (!EditorUtility.IsPersistent(obj))
            {
                MonoBehaviour mb = obj as MonoBehaviour;
                if (mb != null)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mb.gameObject.scene);
                    return;
                }
                GameObject go = obj as GameObject;
                if (go != null)
                {
                    EditorSceneManager.MarkSceneDirty(go.scene);
                    return;
                }
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
        }

#endif

    }

    /// <summary>
    /// Enum flags attribute, allows drawing a flags property drawer
    /// </summary>
    public class EnumFlagAttribute : PropertyAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tooltip">Tooltip</param>
        public EnumFlagAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; private set; }
    }

    /// <summary>
    /// Read only label attribute
    /// </summary>
    public class ReadOnlyLabelAttribute : PropertyAttribute { }

    /// <summary>
    /// Help box message type
    /// </summary>
    public enum HelpBoxMessageType
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Info
        /// </summary>
        Info,

        /// <summary>
        /// Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Error
        /// </summary>
        Error
    }

    /// <summary>
    /// Help box attribute, use to turn field into a help box property drawer
    /// </summary>
    public class HelpBoxAttribute : PropertyAttribute
    {
        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Message type
        /// </summary>
        public HelpBoxMessageType MessageType { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="messageType">Message type</param>
        public HelpBoxAttribute(string text, HelpBoxMessageType messageType = HelpBoxMessageType.None)
        {
            Text = text;
            MessageType = messageType;
        }
    }

    /// <summary>
    /// Min max slider attribute, used to turn a field into a min/max slider
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        /// <summary>
        /// Max value
        /// </summary>
        public float Max { get; private set; }

        /// <summary>
        /// Max value
        /// </summary>
        public float Min { get; private set; }

        /// <summary>
        /// Tooltip
        /// </summary>
        public string Tooltip { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <param name="tooltip">Tooltip</param>
        public MinMaxSliderAttribute(float min, float max, string tooltip)
        {
            Min = min;
            Max = max;
            Tooltip = tooltip;
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// Weather Maker editor utility functions
    /// </summary>
    public static class WeatherMakerEditorUtility
    {
        /// <summary>
        /// Force all editors to repaint
        /// </summary>
        public static void RepaintInspector()
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                ed[i].Repaint();
            }
        }

        /// <summary>
        /// Check if a type exists
        /// </summary>
        /// <param name="type">Full type name (not including assembly)</param>
        /// <returns>True if type exists, false otherwise</returns>
        public static bool TypeExists(string type)
        {
            foreach (System.Reflection.Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetType(type) != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Update pre-processor
        /// </summary>
        /// <param name="add">True to add, false to remove</param>
        /// <param name="preProcessor">The pre-processor to add or remove</param>
        /// <param name="newPreprocessor">The pre-processor to change preProcessor to if it exists if add is false</param>
        public static void UpdatePreProcessor(bool add, string preProcessor, string newPreprocessor = null)
        {
            string currentDefinesString = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup);
            bool removed = false;
            List<string> currentDefinesList = new List<string>(currentDefinesString.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            if (add)
            {
                int index = currentDefinesList.IndexOf(preProcessor);
                if (index < 0)
                {
                    currentDefinesList.Add(preProcessor);
                }
            }
            else
            {
                removed = currentDefinesList.Remove(preProcessor);
            }
            if (removed && !string.IsNullOrEmpty(newPreprocessor))
            {
                currentDefinesList.Add(newPreprocessor);
            }
            string newDefinesString = string.Join(";", currentDefinesList.ToArray());
            if (newDefinesString.Length != currentDefinesString.Length)
            {
                Debug.LogWarning("Updating preprocessor to " + newDefinesString);
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup, newDefinesString);
            }
        }

        /// <summary>
        /// Enable / disable grab pass in all shaders
        /// </summary>
        /// <param name="enable">True to enable grab pass, false to disable it</param>
        public static void ReplaceGrabPassInAllShaders(bool enable)
        {
            foreach (string file in Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(file);
                if (!extension.Equals(".shader", System.StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".cginc", System.StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".hlsl", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string text = File.ReadAllText(file);
                string newText;
                if (enable)
                {
                    newText = Regex.Replace(text, @"\/\*\s\s?GrabPass.*?\{.*?\}\s\*\/", (match) =>
                    {
                        return match.Value.Replace("/* ", string.Empty).Replace(" */", string.Empty).Replace("/*", string.Empty).Replace("*/", string.Empty);
                    }, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                else
                {
                    newText = Regex.Replace(text, @"\sGrabPass\s?\{.*?\}", (match) =>
                    {
                        return "/* " + match.Value + " */";
                    }, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                }
                if (text != newText)
                {
                    Debug.LogWarningFormat("{0} grab pass in shader " + Path.GetFileName(file), (enable ? "Enabling" : "Disabling"));
                    File.WriteAllText(file, newText, new System.Text.UTF8Encoding(false));
                }
            }
        }
    }

    /// <summary>
    /// Single line property drawer, used on RangeOfIntegers, RangeOfFloats or Vector4.
    /// </summary>
    [CustomPropertyDrawer(typeof(SingleLineAttribute))]
    [CustomPropertyDrawer(typeof(SingleLineClampAttribute))]
    public class SingleLineDrawer : PropertyDrawer
    {
        private int rows = 1;

        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="property">Property</param>
        /// <param name="label">Label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string tooltip = (attribute as SingleLineAttribute).Tooltip;
            label.tooltip = tooltip;
            Rect origPosition = position;
            EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            switch (property.type)
            {
                case "RangeOfIntegers":
                    PropertyDrawerExtensions.DrawRangeField(this, position, property, label, false);
                    break;

                case "RangeOfFloats":
                    PropertyDrawerExtensions.DrawRangeField(this, position, property, label, true);
                    break;

                case "Vector4":
                    EditorGUI.BeginChangeCheck();
                    Vector4 v4 = EditorGUI.Vector4Field(origPosition, label, property.vector4Value);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector4Value = v4;
                    }
                    int newRows = EditorGUIUtility.currentViewWidth <= 332.5f ? 2 : 1;
                    if (rows != newRows)
                    {
                        rows = newRows;
                        if (EditorApplication.update != null)
                        {
                            EditorApplication.update.Invoke();
                        }
                    }
                    break;

                default:
                    EditorGUI.HelpBox(position, "[SingleLineDrawer] doesn't work with type '" + property.type + "'", MessageType.Error);
                    break;
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Get property height
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="label">Label</param>
        /// <returns>Height</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * rows;
        }
    }

    /// <summary>
    /// Help box property drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        /// <summary>
        /// Get height
        /// </summary>
        /// <returns>Height</returns>
        public override float GetHeight()
        {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return base.GetHeight();
            var helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
            if (helpBoxStyle == null) return base.GetHeight();
            return helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.Text), EditorGUIUtility.currentViewWidth) + 4.0f;
        }

        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="position">Position</param>
        public override void OnGUI(Rect position)
        {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return;
            EditorGUI.HelpBox(position, helpBoxAttribute.Text, GetMessageType(helpBoxAttribute.MessageType));
        }

        private MessageType GetMessageType(HelpBoxMessageType helpBoxMessageType)
        {
            switch (helpBoxMessageType)
            {
                default:
                case HelpBoxMessageType.None: return MessageType.None;
                case HelpBoxMessageType.Info: return MessageType.Info;
                case HelpBoxMessageType.Warning: return MessageType.Warning;
                case HelpBoxMessageType.Error: return MessageType.Error;
            }
        }
    }

    /// <summary>
    /// Property drawer extension methods
    /// </summary>
    public static class PropertyDrawerExtensions
    {
        /// <summary>
        /// Draw an int text field
        /// </summary>
        /// <param name="drawer">Property drawer</param>
        /// <param name="position">Position</param>
        /// <param name="text">Text</param>
        /// <param name="tooltip">Tooltip</param>
        /// <param name="prop">Property</param>
        public static void DrawIntTextField(this PropertyDrawer drawer, Rect position, string text, string tooltip, SerializedProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            int value = EditorGUI.IntField(position, new GUIContent(text, tooltip), prop.intValue);
            SingleLineClampAttribute clamp = drawer.attribute as SingleLineClampAttribute;
            if (clamp != null)
            {
                value = Mathf.Clamp(value, (int)clamp.MinValue, (int)clamp.MaxValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = value;
            }
        }

        /// <summary>
        /// Draw a float text field
        /// </summary>
        /// <param name="drawer">Property drawer</param>
        /// <param name="position">Position</param>
        /// <param name="text">Text</param>
        /// <param name="tooltip">Tooltip</param>
        /// <param name="prop">Property</param>
        public static void DrawFloatTextField(this PropertyDrawer drawer, Rect position, string text, string tooltip, SerializedProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            float value = EditorGUI.FloatField(position, new GUIContent(text, tooltip), prop.floatValue);
            SingleLineClampAttribute clamp = drawer.attribute as SingleLineClampAttribute;
            if (clamp != null)
            {
                value = Mathf.Clamp(value, (float)clamp.MinValue, (float)clamp.MaxValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value;
            }
        }

        /// <summary>
        /// Draw a range field
        /// </summary>
        /// <param name="drawer">Property drawer</param>
        /// <param name="position">Position</param>
        /// <param name="prop">Property</param>
        /// <param name="label">Label</param>
        /// <param name="floatingPoint">True for float, false for int</param>
        public static void DrawRangeField(this PropertyDrawer drawer, Rect position, SerializedProperty prop, GUIContent label, bool floatingPoint)
        {
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUIUtility.labelWidth = 30.0f;
            EditorGUIUtility.fieldWidth = 60.0f;
            float width = 100.0f;
            float spacing = 10.0f;
            position.x -= (EditorGUI.indentLevel * 15.0f);
            position.width = width;
            if (floatingPoint)
            {
                DrawFloatTextField(drawer, position, "Min", "Minimum value", prop.FindPropertyRelative("Minimum"));
            }
            else
            {
                DrawIntTextField(drawer, position, "Min", "Minimum value", prop.FindPropertyRelative("Minimum"));
            }
            position.x = position.xMax + spacing;
            position.width = width;
            if (floatingPoint)
            {
                DrawFloatTextField(drawer, position, "Max", "Maximum value", prop.FindPropertyRelative("Maximum"));
            }
            else
            {
                DrawIntTextField(drawer, position, "Max", "Maximum value", prop.FindPropertyRelative("Maximum"));
            }
        }
    }

    /// <summary>
    /// Popup list editor window content
    /// </summary>
    public class PopupList : PopupWindowContent
    {
        private Vector2 scrollViewOffset;
        private static Texture2D gray;

        /// <summary>
        /// Size
        /// </summary>
        public Vector2 Size = new Vector2(300.0f, 200.0f);

        /// <summary>
        /// Title
        /// </summary>
        public string Title = string.Empty;

        /// <summary>
        /// List style
        /// </summary>
        public GUIStyle ListStyle { get; set; }

        /// <summary>
        /// Items
        /// </summary>
        public GUIContent[] Items { get; set; }

        /// <summary>
        /// Selected index
        /// </summary>
        public int SelectedItemIndex { get; set; }

        /// <summary>
        /// True if showing, false otherwise
        /// </summary>
        public bool IsShowing { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PopupList()
        {
            ListStyle = "label";
            if (gray == null)
            {
                gray = new Texture2D(1, 1);
                gray.SetPixel(0, 0, Color.gray);
                gray.Apply();
            }
        }

        /// <summary>
        /// Get the window size
        /// </summary>
        /// <returns>Window size</returns>
        public override Vector2 GetWindowSize()
        {
            return Size;
        }

        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="rect">Rect</param>
        public override void OnGUI(Rect rect)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                GUILayout.Label(Title, EditorStyles.boldLabel);
            }
            scrollViewOffset = EditorGUILayout.BeginScrollView(scrollViewOffset);
            Texture2D origBg = ListStyle.normal.background;
            for (int i = 0; i < Items.Length; i++)
            {
                GUIStyle style = ListStyle;
                Rect r = GUILayoutUtility.GetRect(Items[i], style);
                if (i == SelectedItemIndex || r.Contains(Event.current.mousePosition))
                {
                    style = new GUIStyle(style);
                    style.normal.textColor = style.hover.textColor = Color.white;
                    style.normal.background = style.hover.background = gray;
                }
                if (GUI.Button(r, Items[i], style))
                {
                    SelectedItemIndex = i;
                    if (SelectedIndexChanged != null)
                    {
                        SelectedIndexChanged(this, System.EventArgs.Empty);
                    }
                    editorWindow.Close();
                }
            }
            ListStyle.normal.background = origBg;
            GUI.EndScrollView();
        }

        /// <summary>
        /// OnOpen
        /// </summary>
        public override void OnOpen()
        {
            IsShowing = true;
            scrollViewOffset.y = ListStyle.CalcHeight(Items[0], 1.0f) * SelectedItemIndex;
        }

        /// <summary>
        /// OnClose
        /// </summary>
        public override void OnClose()
        {
            IsShowing = false;
        }

        /// <summary>
        /// Fires when selected index changes
        /// </summary>
        public System.EventHandler SelectedIndexChanged;
    }

    /// <summary>
    /// Draw lines helper class
    /// </summary>
    public class EditorDrawLine
    {
        //****************************************************************************************************
        //  static function DrawLine(rect : Rect) : void
        //  static function DrawLine(rect : Rect, color : Color) : void
        //  static function DrawLine(rect : Rect, width : float) : void
        //  static function DrawLine(rect : Rect, color : Color, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, width : float) : void
        //  static function DrawLine(Vector2 pointA, Vector2 pointB, color : Color, width : float) : void
        //  
        //  Draws a GUI line on the screen.
        //  
        //  DrawLine makes up for the severe lack of 2D line rendering in the Unity runtime GUI system.
        //  This function works by drawing a 1x1 texture filled with a color, which is then scaled
        //   and rotated by altering the GUI matrix.  The matrix is restored afterwards.
        //****************************************************************************************************

        /// <summary>
        /// Line texture
        /// </summary>
        public static Texture2D lineTex;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        public static void DrawLine(Rect rect) { DrawLine(rect, GUI.contentColor, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public static void DrawLine(Rect rect, Color color) { DrawLine(rect, color, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="width"></param>
        public static void DrawLine(Rect rect, float width) { DrawLine(rect, GUI.contentColor, width); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        public static void DrawLine(Rect rect, Color color, float width) { DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="color"></param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) { DrawLine(pointA, pointB, color, 1.0f); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="width"></param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, float width) { DrawLine(pointA, pointB, GUI.contentColor, width); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="color"></param>
        /// <param name="width"></param>
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            // Save the current GUI matrix, since we're going to make changes to it.
            Matrix4x4 matrix = GUI.matrix;

            // Generate a single pixel texture if it doesn't exist
            if (!lineTex) { lineTex = new Texture2D(1, 1); }

            // Store current GUI color, so we can switch it back later,
            // and set the GUI color to the color parameter
            Color savedColor = GUI.color;
            GUI.color = color;

            // Determine the angle of the line.
            float angle = Vector3.Angle(pointB - pointA, Vector2.right);

            // Vector3.Angle always returns a positive number.
            // If pointB is above pointA, then angle needs to be negative.
            if (pointA.y > pointB.y) { angle = -angle; }

            // Use ScaleAroundPivot to adjust the size of the line.
            // We could do this when we draw the texture, but by scaling it here we can use
            //  non-integer values for the width and length (such as sub 1 pixel widths).
            // Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
            //  is centered on the origin at pointA.
            GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));

            // Set the rotation for the line.
            //  The angle was calculated with pointA as the origin.
            GUIUtility.RotateAroundPivot(angle, pointA);

            // Finally, draw the actual line.
            // We're really only drawing a 1x1 texture from pointA.
            // The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
            //  render with the proper width, length, and angle.
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);

            // We're done.  Restore the GUI matrix and GUI color to whatever they were before.
            GUI.matrix = matrix;
            GUI.color = savedColor;
        }
    }

    /// <summary>
    /// Enum flag property drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(EnumFlagAttribute))]
    public class EnumFlagDrawer : PropertyDrawer
    {
        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="property">Property</param>
        /// <param name="label">Label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.LabelField(position, "Multi-edit not supported");
                return;
            }

            EnumFlagAttribute flagSettings = (EnumFlagAttribute)attribute;
            System.Enum targetEnum = (System.Enum)System.Enum.ToObject(fieldInfo.FieldType, property.intValue);
            string tooltip = (attribute as WeatherMaker.EnumFlagAttribute).Tooltip;
            if (!string.IsNullOrEmpty(tooltip))
            {
                label.tooltip = tooltip;
            }
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            System.Enum enumNew = EditorGUI.EnumFlagsField(position, targetEnum);
            property.intValue = (int)System.Convert.ChangeType(enumNew, targetEnum.GetType());
            EditorGUI.EndProperty();
        }
    }

    /// <summary>
    /// Comment drawer, allows drawing a label instead of a field
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyLabelAttribute))]
    public class CommentDrawer : PropertyDrawer
    {
        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="property">Property</param>
        /// <param name="label">Label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, property.stringValue);
        }
    }

    /// <summary>
    /// Min max slider property drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        /// <summary>
        /// OnGUI
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="property">Property</param>
        /// <param name="label">Label</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float textFieldWidth = 44;
            MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
            if (attr != null)
            {
                label.tooltip = attr.Tooltip;
            }
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (attr != null && property.propertyType == SerializedPropertyType.Generic && (property.type == "RangeOfFloats" || property.type == "RangeOfIntegers"))
            {
                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUIUtility.fieldWidth = textFieldWidth;
                bool isFloat = property.type == "RangeOfFloats";
                float min = (isFloat ? property.FindPropertyRelative("Minimum").floatValue : property.FindPropertyRelative("Minimum").intValue);
                float max = (isFloat ? property.FindPropertyRelative("Maximum").floatValue : property.FindPropertyRelative("Maximum").intValue);
                Rect minPos = position;
                minPos.width = textFieldWidth;
                Rect maxPos = position;
                maxPos.x += textFieldWidth + 8.0f;
                maxPos.width = textFieldWidth;
                EditorGUI.BeginChangeCheck();
                if (isFloat)
                {
                    float minValue = EditorGUI.FloatField(minPos, min);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.FindPropertyRelative("Minimum").floatValue = minValue;
                    }
                }
                else
                {
                    int minValue = EditorGUI.IntField(minPos, (int)min);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.FindPropertyRelative("Minimum").intValue = minValue;
                    }
                }
                Rect lineRect = position;
                lineRect.x += textFieldWidth + 3.0f;
                lineRect.y += (lineRect.height * 0.45f);
                lineRect.height *= 0.1f;
                lineRect.width = 2.0f;
                EditorGUI.DrawRect(lineRect, Color.cyan);
                EditorGUI.BeginChangeCheck();
                if (isFloat)
                {
                    float maxValue = EditorGUI.FloatField(maxPos, max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.FindPropertyRelative("Maximum").floatValue = maxValue;
                    }
                }
                else
                {
                    int maxValue = EditorGUI.IntField(maxPos, (int)max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.FindPropertyRelative("Maximum").intValue = maxValue;
                    }
                }

                label.tooltip = attr.Tooltip;
                Rect sliderPos = position;
                float sliderX = textFieldWidth + textFieldWidth + 16.0f;
                sliderPos.x += sliderX;
                sliderPos.width -= sliderX;
                EditorGUI.BeginChangeCheck();
                EditorGUI.MinMaxSlider(sliderPos, ref min, ref max, attr.Min, attr.Max);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isFloat)
                    {
                        property.FindPropertyRelative("Minimum").floatValue = min;
                        property.FindPropertyRelative("Maximum").floatValue = max;
                    }
                    else
                    {
                        property.FindPropertyRelative("Minimum").intValue = (int)min;
                        property.FindPropertyRelative("Maximum").intValue = (int)max;
                    }
                }
                EditorGUI.indentLevel = indent;
            }
            else
            {
                EditorGUI.LabelField(position, label, "Use only with RangeOfFloats or RangeOfIntegers");
            }
            EditorGUI.EndProperty();
        }
    }

#endif

    /// <summary>
    /// Extension methods for objects
    /// </summary>
    public static class WeatherMakerObjectExtensions
    {
        private readonly static Dictionary<Object, string> cachedNames = new Dictionary<Object, string>();

        /// <summary>
        /// Get cached name
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Cached name</returns>
        public static string CachedName(this Object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            string name;
            if (cachedNames.TryGetValue(obj, out name))
            {
                return name;
            }
            return SetCachedName(obj, obj.name);
        }

        /// <summary>
        /// Put name in the cache
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="name">Name</param>
        /// <returns>Name</returns>
        public static string SetCachedName(this Object obj, string name)
        {
            obj.name = name;
            cachedNames[obj] = name;
            return name;
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        public static void Clear()
        {
            cachedNames.Clear();
        }
    }

    /// <summary>
    /// Reflection helper methods
    /// </summary>
    public static class WeatherMakerMemberInfoExtensions
    {
        /// <summary>
        /// GetUnderlyingType
        /// </summary>
        /// <param name="member">Member</param>
        /// <returns>Type</returns>
        public static System.Type GetUnderlyingType(this System.Reflection.MemberInfo member)
        {
            switch (member.MemberType)
            {
                case System.Reflection.MemberTypes.Event:
                    return ((System.Reflection.EventInfo)member).EventHandlerType;
                case System.Reflection.MemberTypes.Field:
                    return ((System.Reflection.FieldInfo)member).FieldType;
                case System.Reflection.MemberTypes.Method:
                    return ((System.Reflection.MethodInfo)member).ReturnType;
                case System.Reflection.MemberTypes.Property:
                    return ((System.Reflection.PropertyInfo)member).PropertyType;
                default:
                    return null;
            }
        }

        /// <summary>
        /// GetUnderlyingValue
        /// </summary>
        /// <param name="member">Member</param>
        /// <param name="target">Instance</param>
        /// <returns>Value</returns>
        public static object GetUnderlyingValue(this System.Reflection.MemberInfo member, object target)
        {
            switch (member.MemberType)
            {
                case System.Reflection.MemberTypes.Field:
                    return ((System.Reflection.FieldInfo)member).GetValue(target);
                case System.Reflection.MemberTypes.Property:
                    return ((System.Reflection.PropertyInfo)member).GetValue(target, null);
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Output parameter
    /// </summary>
    /// <typeparam name="T">Type of value</typeparam>
    public abstract class WeatherMakerOutputParameter<T>
    {
        /// <summary>
        /// Value
        /// </summary>
        public T Value { get; set; }
    }

    /// <summary>
    /// Output parameter for float
    /// </summary>
    public class WeatherMakerOutputParameterFloat : WeatherMakerOutputParameter<float> { }

    /// <summary>
    /// Output change event for float
    /// </summary>
    [System.Serializable]
    public class WeatherMakerOutputParameterEventFloat : UnityEngine.Events.UnityEvent<WeatherMakerOutputParameterFloat> { }
}
