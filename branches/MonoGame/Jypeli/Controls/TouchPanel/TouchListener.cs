using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli
{
    public struct TouchListener
    {
        private Predicate<Touch> isTriggered;
        private Delegate handler;
        private object[] handlerParams;
        private string helpText;

        public TouchListener( Predicate<Touch> triggerRule, string helpText, Delegate handler, params object[] args )
        {
            this.isTriggered = triggerRule;
            this.handler = handler;
            this.helpText = helpText;
            
            this.handlerParams = new object[args.Length + 1];
            
            if ( args.Length > 0 )
            {
                Array.ConstrainedCopy( args, 1, handlerParams, 0, handlerParams.Length );
            }
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
            if ( isTriggered( touch ) )
                Invoke( touch );
        }
    }
}
