﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace The4Dimension
{

    class CustomStringWriter : System.IO.StringWriter
    {
        private readonly Encoding encoding;

        public CustomStringWriter(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }

    public class CustomStack<T>
    {
        private List<T> items = new List<T>();
        public int MaxItems = 50;

        public int Count
        { get { return items.Count(); } }

        public void Remove(int index)
        {
            items.RemoveAt(index);
        }

        public void Push(T item)
        {
            items.Add(item);
            if (items.Count > MaxItems)
            {
                for (int i = MaxItems; i < items.Count; i++) Remove(0);
            }
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T tmp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return tmp;
            }
            else return default(T);
        }

        public T[] ToArray()
        {
            return items.ToArray();
        }
    }

    public class UndoAction
    {
        public string actionName;
        public Action<object[]> Action = null;
        object[] Args;

        public void Undo()
        {
            Action.Invoke(Args);
        }

        public override string ToString()
        {
            return actionName;
        }

        public UndoAction(string name, object[] args, Action<object[]> Act)
        {
            actionName = name;
            Args = args;
            Action = Act;
        }
       
    }

    public class ClipBoardItem
    {
        public enum ClipboardType
        {
            NotSet = 0,
            Position = 1,
            Rotation = 2,
            Scale = 3,
            IntArray = 4,
            FullObject = 5,
            Rail = 6,
            ObjectArray = 7
        }

        public Single X = 0;
        public Single Y = 0;
        public Single Z = 0;
        public int[] Args = null;
        public ClipboardType Type = 0;
        public Rail Rail = null;
        public LevelObj[] Objs = null;

        public override string ToString()
        {
            switch (Type)
            {
                case ClipboardType.Position:
                    return String.Format("Position - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.Rotation:
                    return String.Format("Rotation - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.Scale:
                    return String.Format("Scale - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.IntArray:
                    return "Args[]";
                case ClipboardType.Rail:
                    return "Rail - " + Rail.Name;
                case ClipboardType.FullObject:
                    return "Object - " + Objs[0].ToString();
                case ClipboardType.ObjectArray:
                    return "Object[" + Objs.Length.ToString() + "]";
                default:
                    return "Not set";
            }
        }

        public string ToString(int ObjectAsChildren)
        {
            switch (Type)
            {
                case ClipboardType.Position:
                    return String.Format("Position - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.Rotation:
                    return String.Format("Rotation - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.Scale:
                    return String.Format("Scale - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.IntArray:
                    return "Args[]";
                case ClipboardType.Rail:
                    return "Rail - " + Rail.Name;
                case ClipboardType.FullObject:
                    if (ObjectAsChildren < 0) return "Object - " + Objs[0].ToString();
                    else
                        return "Paste object as children - " + Objs[0].ToString();
                case ClipboardType.ObjectArray:
                    return "Object[" + Objs.Length.ToString() + "]";
                default:
                    return "Not set";
            }
        }
    }

    public class AllInfoSection : List<LevelObj>
    {
        public bool IsHidden = false;
    }

    class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        IDictionary _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }
    }

    class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        IDictionary _dictionary;
        object _key;

        internal DictionaryPropertyDescriptor(IDictionary d, object key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override Type PropertyType
        {
            get { return _dictionary[_key].GetType(); }
        }

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }

    public class ObjectDb
    {
        public Dictionary<int, string> Categories = new Dictionary<int, string>();
        public Dictionary<string, ObjectDbEntry> Entries = new Dictionary<string, ObjectDbEntry>();
        public Dictionary<string, string> IdToModel = new Dictionary<string, string>();
        public int timestamp;

        public static ObjectDb FromXml(string xml)
        {
            ObjectDb res = new ObjectDb();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode n = doc.SelectSingleNode("/database");
            res.timestamp = int.Parse(n.Attributes["timestamp"].InnerText);
            foreach (XmlNode node in n.ChildNodes)
            {
                if (node.Name == "categories")
                {
                    foreach (XmlNode subn in node.ChildNodes)
                    {
                        res.Categories.Add(int.Parse(subn.Attributes["id"].InnerText), subn.InnerText);
                    }
                }
                else if (node.Name == "object")
                {
                    ObjectDbEntry tmp = ObjectDbEntry.FromXml(node.ChildNodes);
                    res.Entries.Add(node.Attributes["id"].InnerText, tmp);
                    if (tmp.model.Trim() != "") res.IdToModel.Add(node.Attributes["id"].InnerText, tmp.model);
                }
            }
            return res;
        }

        public string GetXml(bool updateTimestamp = true)
        {
            return ASCIIEncoding.UTF8.GetString(GetXmlBytes(updateTimestamp));
        }

        public byte[] GetXmlBytes(bool updateTimestamp = true)
        {
            if (updateTimestamp)
            {
                TimeSpan ts = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                timestamp = (int)ts.TotalSeconds;
            }
            using (var stream = new MemoryStream())
            {
                using (var xr = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, Encoding = ASCIIEncoding.UTF8 }))
                {
                    xr.WriteStartDocument();
                    xr.WriteStartElement("database");
                    xr.WriteAttributeString("timestamp", timestamp.ToString());
                    xr.WriteStartElement("categories");
                    foreach (int i in Categories.Keys.ToArray())
                    {
                        xr.WriteStartElement("category");
                        xr.WriteAttributeString("id", i.ToString());
                        xr.WriteString(Categories[i]);
                        xr.WriteEndElement();
                    }
                    xr.WriteEndElement();
                    foreach (string id in Entries.Keys.ToArray())
                    {
                        xr.WriteStartElement("object");
                        xr.WriteAttributeString("id", id);
                        Entries[id].WriteXml(xr);
                        xr.WriteEndElement();
                        xr.WriteRaw("\r\n".ToCharArray(), 0, 1);
                    }
                    xr.WriteEndElement();
                    xr.Close();
                }
                return stream.ToArray();
            }
        }

        public class ObjectDbEntry
        {
            public string name, notes, files,model = "" , type = "";
            public int Known, Complete, Category;
            public List<ObjectDbField> Fields = new List<ObjectDbField>();

            public static ObjectDbEntry FromXml(XmlNodeList nodes)
            {
                ObjectDbEntry res = new ObjectDbEntry();
                foreach (XmlNode n in nodes)
                {
                    switch (n.Name)
                    {
                        case "name":
                            res.name = n.InnerText;
                            break;
                        case "type":
                            res.type = n.InnerText;
                            break;
                        case "model":
                            res.model = n.InnerText;
                            break;
                        case "flags":
                            res.Known = int.Parse(n.Attributes["known"].InnerText);
                            res.Complete = int.Parse(n.Attributes["complete"].InnerText);
                            break;
                        case "category":
                            res.Category = int.Parse(n.Attributes["id"].InnerText);
                            break;
                        case "notes":
                            res.notes = n.InnerText;
                            break;
                        case "files":
                            res.files = n.InnerText;
                            break;
                        case "field":
                            res.Fields.Add(ObjectDbField.FromXml(n));
                            break;
                    }
                }
                return res;
            }

            public void WriteXml(XmlWriter xr)
            {
                xr.WriteStartElement("name");
                xr.WriteString(name);
                xr.WriteEndElement();
                xr.WriteStartElement("type");
                xr.WriteString(type);
                xr.WriteEndElement();
                xr.WriteStartElement("model");
                xr.WriteString(model);
                xr.WriteEndElement();
                xr.WriteStartElement("flags");
                xr.WriteAttributeString("known", Known.ToString());
                xr.WriteAttributeString("complete", Complete.ToString());
                xr.WriteEndElement();
                xr.WriteStartElement("category");
                xr.WriteAttributeString("id", Category.ToString());
                xr.WriteEndElement();
                xr.WriteStartElement("notes");
                xr.WriteString(notes);
                xr.WriteEndElement();
                xr.WriteStartElement("files");
                xr.WriteString(files);
                xr.WriteEndElement();
                foreach (ObjectDbField f in Fields) f.WriteXml(xr);
            }

            public class ObjectDbField
            {
                public int id;
                public string type = "int";
                public string name, values, notes;

                public void WriteXml(XmlWriter xr)
                {
                    xr.WriteStartElement("field");
                    xr.WriteAttributeString("id", id.ToString());
                    xr.WriteAttributeString("type", type);
                    xr.WriteAttributeString("name", name);
                    xr.WriteAttributeString("values", values);
                    xr.WriteAttributeString("notes", notes);
                    xr.WriteEndElement();
                }

                public static ObjectDbField FromXml(XmlNode n)
                {
                    ObjectDbField res = new ObjectDbField();
                    res.id = int.Parse(n.Attributes["id"].InnerText);
                    res.type = n.Attributes["type"].InnerText;
                    res.name = n.Attributes["name"].InnerText;
                    res.values = n.Attributes["values"].InnerText;
                    res.notes = n.Attributes["notes"].InnerText;
                    return res;
                }
            }
        }
    }

    public class IndexedProperty<TIndex, TValue>
    {
        readonly Action<TIndex, TValue> SetAction;
        readonly Func<TIndex, TValue> GetFunc;

        public IndexedProperty(Func<TIndex, TValue> getFunc, Action<TIndex, TValue> setAction)
        {
            this.GetFunc = getFunc;
            this.SetAction = setAction;
        }

        public TValue this[TIndex i]
        {
            get
            {
                return GetFunc(i);
            }
            set
            {
                SetAction(i, value);
            }
        }
    }
}

namespace ExtensionMethods
{
    static class Extensions
    {
        public static Vector3D ToVect(this Point3D p)
        {
            return new Vector3D(p.X, p.Y, p.Z);
        }
    }
}
