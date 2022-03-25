using System;
using System.Collections.Generic;

namespace Jypeli
{
    public partial class Game
    {
        /// <summary>
        /// Luo tehdasmetodin tietylle tyypille ja tagille.
        /// </summary>
        /// <typeparam name="T">Oliotyyppi</typeparam>
        /// <param name="tag">Tagi</param>
        /// <param name="method">Metodi joka palauttaa olion</param>
        public void AddFactory<T>(string tag, Factory.FactoryMethod method)
        {
            Factory.AddFactory<T>(tag, method);
        }

        /// <summary>
        /// Poistaa tehdasmetodin.
        /// </summary>
        /// <typeparam name="T">Oliotyyppi</typeparam>
        /// <param name="tag">Tagi</param>
        /// <param name="method">Poistettava tehdasmetodi</param>
        public void RemoveFactory<T>(string tag, Factory.FactoryMethod method)
        {
            Factory.RemoveFactory<T>(tag, method);
        }

        /// <summary>
        /// Käyttää tehdasmetodia uuden olion luomiseen ja palauttaa olion.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi</typeparam>
        /// <param name="tag">Olion tagi</param>
        /// <returns>Uusi olio</returns>
        public T FactoryCreate<T>(string tag)
        {
            return Factory.FactoryCreate<T>(tag);
        }
    }

    public static class Factory
    {
        struct FactoryKey
        {
            public Type type;
            public string tag;

            public FactoryKey(Type type, string tag)
            {
                this.type = type;
                this.tag = tag;
            }
        }

        public delegate object FactoryMethod();
        static Dictionary<FactoryKey, FactoryMethod> constructors = new Dictionary<FactoryKey, FactoryMethod>();

        public static void AddFactory<T>(string tag, FactoryMethod method)
        {
            foreach (var key in constructors.Keys.FindAll<FactoryKey>(k => k.type == typeof(T) && k.tag == tag))
            {
                // Overwrite an existing method
                constructors[key] = method;
                return;
            }

            FactoryKey newKey = new FactoryKey(typeof(T), tag);
            constructors.Add(newKey, method);
        }

        public static void RemoveFactory<T>(string tag, FactoryMethod method)
        {
            foreach (var key in constructors.Keys.FindAll<FactoryKey>(k => k.type == typeof(T) && k.tag == tag))
                constructors.Remove(key);
        }

        public static T FactoryCreate<T>(string tag)
        {
            return (T)FactoryCreate(typeof(T), tag);
        }

        internal static object FactoryCreate(Type type, string tag)
        {
            foreach (FactoryKey key in constructors.Keys)
            {
                if (key.type == type && key.tag == tag)
                    return constructors[key].Invoke();
            }

            throw new KeyNotFoundException("Key " + tag + " for class " + type.Name + " was not found.");
        }
    }
}
