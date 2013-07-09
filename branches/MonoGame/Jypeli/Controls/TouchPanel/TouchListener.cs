﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Controls;

namespace Jypeli
{
    /// <summary>
    /// Kuuntelija kosketusnäytölle.
    /// </summary>
    public struct TouchListener : Listener
    {
        private Predicate<Touch> isTriggered;
        private Delegate handler;
        private object[] handlerParams;
        private bool isDestroyed;
        private string _helpText;

        private bool dynamicContext;
        private ListenContext context;
        private ControlContexted contextedObject;

        /// <summary>
        /// Ohjeteksti.
        /// </summary>
        public string HelpText
        {
            get { return _helpText; }
            set { _helpText = value; }
        }

        internal ListenContext Context
        {
            get { return ( dynamicContext ? contextedObject.ControlContext : context ); }
        }

        public TouchListener( Predicate<Touch> triggerRule, ListenContext context, string helpText, Delegate handler, params object[] args )
        {
            this.isDestroyed = false;
            this.Destroyed = null;
            this.isTriggered = triggerRule;
            this.handler = handler;
            this._helpText = helpText;
            this.dynamicContext = false;
            this.context = context;
            this.contextedObject = null;

            this.handlerParams = new object[args.Length + 1];
            
            if ( args.Length > 0 )
            {
                Array.ConstrainedCopy( args, 1, handlerParams, 0, handlerParams.Length );
            }
        }

        public TouchListener( Predicate<Touch> triggerRule, ControlContexted contexted, string helpText, Delegate handler, params object[] args )
        {
            this.isDestroyed = false;
            this.Destroyed = null;
            this.isTriggered = triggerRule;
            this.handler = handler;
            this._helpText = helpText;
            this.dynamicContext = true;
            this.context = null;
            this.contextedObject = contexted;

            this.handlerParams = new object[args.Length + 1];

            if ( args.Length > 0 )
            {
                Array.ConstrainedCopy( args, 1, handlerParams, 0, handlerParams.Length );
            }
        }

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// </summary>
        /// <param name="context"></param>
        public Listener InContext( ListenContext context )
        {
            this.dynamicContext = false;
            this.context = context;
            return this;
        }

        /// <summary>
        /// Kuuntelee tapahtumaa vain tietyssä kontekstissa.
        /// Esim. Keyboard.Listen(parametrit).InContext(omaIkkuna) kuuntelee
        /// haluttua näppäimistötapahtumaa ainoastaan kun ikkuna on näkyvissä ja päällimmäisenä.
        /// </summary>
        /// <param name="context"></param>
        public Listener InContext( ControlContexted obj )
        {
            this.dynamicContext = true;
            this.contextedObject = obj;
            return this;
        }

        public void Invoke( Touch touch )
        {
#if NETFX_CORE
            // Win8
            MethodInfo handlerMethod = handler.GetMethodInfo();
#else
            MethodInfo handlerMethod = handler.Method;
#endif
            handlerParams[0] = touch;
            handlerMethod.Invoke( handler.Target, handlerParams );
        }

        public void CheckAndInvoke( Touch touch )
        {
            if ( !IsDestroyed && Context != null && !Context.IsDestroyed && Context.Active && isTriggered( touch ) )
                Invoke( touch );
        }

        #region Destroyable Members

        /// <summary>
        /// Onko olio tuhottu.
        /// </summary>
        /// <returns></returns>
        public bool IsDestroyed
        {
            get { return isDestroyed; }
        }

        public void Destroy()
        {
            isDestroyed = true;
            OnDestroyed();
        }

        /// <summary> 
        /// Tapahtuu, kun olio tuhotaan. 
        /// </summary> 
        public event Action Destroyed;

        private void OnDestroyed()
        {
            if ( Destroyed != null )
                Destroyed();
        }

        #endregion
    }
}
