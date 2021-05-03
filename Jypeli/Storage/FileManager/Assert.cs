using System;

namespace Jypeli
{
    public partial class FileManager
    {
        public event Action<Exception> ReadAccessDenied;
        public event Action<Exception> WriteAccessDenied;

        private void OnAccessDenied( Exception e, bool write )
        {
            if ( !write && ReadAccessDenied != null )
                ReadAccessDenied( e );
            if ( write && WriteAccessDenied != null )
                WriteAccessDenied( e );
        }

        protected void FMAssert( Action func, bool write )
        {
#if DEBUG
            func();
#else
            try
            {
                func();
            }
            catch ( Exception e )
            {
                OnAccessDenied( e, write );
            }
#endif
        }

        protected void FMAssert<TP1>( Action<TP1> func, bool write, TP1 p1 )
        {
#if DEBUG
            func( p1 );
#else
            try
            {
                func( p1 );
            }
            catch ( Exception e )
            {
                OnAccessDenied( e, write );
            }
#endif
        }

        protected TR FMAssert<TR>(Func<TR> func, bool write, TR defaultVal)
        {
#if DEBUG
            return func();
#else
            try
            {
                return func();
            }
            catch ( Exception e )
            {
                OnAccessDenied( e, write );
            }

            return defaultVal;
#endif
        }

        protected TR FMAssert<TP1, TR>( Func<TP1, TR> func, bool write, TR defaultVal, TP1 p1 )
        {
#if DEBUG
            return func( p1 );
#else
            try
            {
                return func( p1 );
            }
            catch ( Exception e )
            {
                OnAccessDenied( e, write );
            }

            return defaultVal;
#endif
        }

        protected TR FMAssert<TP1, TP2, TR>( Func<TP1, TP2, TR> func, bool write, TR defaultVal, TP1 p1, TP2 p2 )
        {
#if DEBUG
            return func( p1, p2 );
#else
            try
            {
                return func( p1, p2 );
            }
            catch ( Exception e )
            {
                OnAccessDenied( e, write );
            }

            return defaultVal;
#endif
        }
    }
}
