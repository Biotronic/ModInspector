using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ModInspector
{
    class ConfigNode
    {
        public static ConfigNode LoadFromString(string content, string filename)
        {
            var lines = content.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; ++i)
            {
                var idx = lines[i].IndexOf("//");
                if (idx != -1)
                {
                    lines[i] = lines[i].Substring(0, idx);
                }
                lines[i] = lines[i].Replace("{", "\r\n{\r\n").Replace("}", "\r\n}\r\n").Trim();
            }

            lines = lines.SelectMany(a => a.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            Stack<ConfigNode> stack = new Stack<ConfigNode>(new[] { new ConfigNode("ROOT", "ROOT") });
            string nodeName = "";
            foreach (var line in lines)
            {
                switch (line)
                {
                    case "{":
                        var node = new ConfigNode(nodeName, filename);
                        stack.Peek().AddChild(node);
                        stack.Push(node);
                        continue;
                    case "}":
                        //System.Diagnostics.Debug.Assert(stack.Count > 1, "Mismatched curly bracket in file " + filename);
                        if (stack.Count > 1)
                        {
                            stack.Pop();
                        }
                        continue;
                }
                var stuffs = line.Split(new[] { '=' }, 2);
                if (stuffs.Length == 1)
                {
                    nodeName = line.Trim();
                }
                else
                {
                    stack.Peek()[stuffs[0]] = stuffs[1];
                }
            }

            while (stack.Count > 1)
            {
                stack.Pop();
            }
            return stack.Peek();
        }

        public static ConfigNode LoadFromFile(string filename)
        {
            return LoadFromString(File.ReadAllText(filename), filename);
        }

        public static ConfigNode LoadFromFile(FileInfo file)
        {
            return LoadFromString(File.ReadAllText(file.FullName), file.FullName);
        }

        public static ConfigNode LoadFromDirectory(string dirPath)
        {
            var nodes = Directory.GetFiles(dirPath, "*.cfg", SearchOption.AllDirectories).Select(ConfigNode.LoadFromFile);
            return Merge(nodes);
        }

        private string _filename;
        private string _type;
        private Dictionary<string, string> _values = new Dictionary<string, string>();
        private List<ConfigNode> _children = new List<ConfigNode>();

        public class NodeValueChangedEventArgs : EventArgs
        {
            private string _valueName;
            private string _oldValue;
            private string _newValue;

            public string ValueName
            {
                get
                {
                    return _valueName;
                }
            }
            public string OldValue
            {
                get
                {
                    return _oldValue;
                }
            }
            public string NewValue
            {
                get
                {
                    return _newValue;
                }
                set
                {
                    _newValue = value;
                }
            }

            public NodeValueChangedEventArgs(string valueName, string value)
            {
                _valueName = valueName;
                _oldValue = value;
                _newValue = value;
            }
        }

        public event EventHandler<NodeValueChangedEventArgs> valueChanged;

        public static ConfigNode Merge(IEnumerable<ConfigNode> nodes)
        {
            var result = new ConfigNode("ROOT", "ROOT");

            foreach (var node in nodes)
            {
                foreach (var child in node._children)
                {
                    result.AddChild(child);
                }
                foreach (var value in node._values)
                {
                    result[value.Key] = value.Value;
                }
            }

            return result;
        }

        public ConfigNode(string type, string filename)
        {
            _filename = filename;
            _type = type;
        }

        public void AddChild(ConfigNode child)
        {
            _children.Add(child);
        }

        public bool HasValue(string name)
        {
            return _values.ContainsKey(name);
        }

        public string this[string index]
        {
            get
            {
                if (_values.ContainsKey(index.Trim()))
                {
                    return _values[index.Trim()];
                }
                return "";
            }
            set
            {
                var args = new NodeValueChangedEventArgs(index.Trim(), value.Trim());
                OnValueChanged(args);
                _values[args.ValueName] = args.NewValue;
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
        }

        public IEnumerable<ConfigNode> Children
        {
            get
            {
                return _children;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Values
        {
            get
            {
                return _values.Values.OfType<KeyValuePair<string, string>>();
            }
        }

        private string ToStringImpl(string prefix)
        {
            StringBuilder result = new StringBuilder();
            result.Append(prefix);
            result.AppendLine(this.Type);
            result.Append(prefix);
            result.AppendLine("{");

            foreach (var value in _values)
            {
                result.Append(prefix);
                result.Append(value.Key);
                result.Append(" = ");
                result.AppendLine(value.Value);
            }

            foreach (var child in Children)
            {
                result.Append(child.ToStringImpl(prefix + "    "));
            }

            result.Append(prefix);
            result.AppendLine("}");
            return result.ToString();
        }

        public override string ToString()
        {
            return ToStringImpl("");
        }

        private void OnValueChanged(NodeValueChangedEventArgs args)
        {
            if (valueChanged != null)
            {
                valueChanged(this, args);
            }
        }
    }
}
