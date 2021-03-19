#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion





namespace System
{
    //Attributes for the CompactFramework that dont exist int the CompactFramework so
    //this can compile under the CompactFramework without rewritting.
    //or having compiler directives surrounding all of these Attributes.
#if CompactFramework || WindowsCE || PocketPC || SILVERLIGHT
    [ComVisible(true)]
    [AttributeUsage(
        AttributeTargets.Delegate |
        AttributeTargets.Enum |
        AttributeTargets.Struct |
        AttributeTargets.Class,
        Inherited = false)]
    public sealed class SerializableAttribute : Attribute
    { }
#endif
#if CompactFramework || WindowsCE || PocketPC 
    namespace Runtime.Serialization
    {
        [Serializable]
        [ComVisible(true)]
        public class SerializationException : SystemException
        {
            public SerializationException() { }
            public SerializationException(string message) : base(message) { }
            public SerializationException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
#endif
#if CompactFramework || WindowsCE || PocketPC || XBOX360 || SILVERLIGHT
    [ComVisible(true)]
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class NonSerializedAttribute : Attribute
    { }
    namespace Xml.Serialization
    {
        [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
        public sealed class XmlIgnoreAttribute : Attribute
        { }
        [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
        public class XmlAttributeAttribute : Attribute
        {
            public XmlAttributeAttribute() { }
            public XmlAttributeAttribute(string attributeName) { }
            public XmlAttributeAttribute(Type type) { }
            public XmlAttributeAttribute(string attributeName, Type type) { }
        }
    }
    namespace ComponentModel
    {
        [AttributeUsage(AttributeTargets.All)]
        public sealed class DescriptionAttribute : Attribute
        {
            public DescriptionAttribute() { }
            public DescriptionAttribute(string description) { }
        }
    }
    namespace Runtime.Serialization
    {
        [ComVisible(true)]
        public interface IDeserializationCallback
        {
            void OnDeserialization(object sender);
        }
    }
#endif
#if SILVERLIGHT

    [ComVisible(true)]
    public interface ICloneable
    {
        object Clone();
    }

    public static class ExtensionMethods
    {
        public static int RemoveAll<T>(this System.Collections.Generic.List<T> self, Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            int index = 0;
            while ((index < self.Count) && !match(self[index]))
            {
                index++;
            }
            if (index >= self.Count)
            {
                return 0;
            }
            int index2 = index + 1;
            while (index2 < self.Count)
            {
                while ((index2 < self.Count) && match(self[index2]))
                {
                    index2++;
                }
                if (index2 < self.Count)
                {
                    self[index++] = self[index2++];
                }
            }
            int result = self.Count - index;
            self.RemoveRange(index, result);
            return result;
        }
    }
#endif
}
